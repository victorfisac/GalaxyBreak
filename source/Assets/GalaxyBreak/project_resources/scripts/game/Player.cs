using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour 
{
	#region Enums
	public enum PlayerStates { START, GAME, DEAD, EXIT };
	#endregion

	#region Inspector Members
	[Header("Settings")]
	[Tooltip("Normalized height min position to detect touching")]
	[SerializeField] private float screenMinHeight;

	[Tooltip("Normalized height min position to detect touching when ad is displaying")]
	[SerializeField] private float bannerScreenMinHeight;

	[Tooltip("Normalized width min position to detect touching when ad is displaying")]
	[SerializeField] private float screenMinWidth;

	[Header("Start")]
	[Tooltip("Start player animation curve")]
	[SerializeField] private AnimationCurve startCurve;

	[Tooltip("Start player animation duration")]
	[SerializeField] private float startDuration;

	[Header("Linear")]
	[Tooltip("Player maximum forward velocity")]
	[SerializeField] private float speed;

	[Tooltip("Player speed increase for each destroyed enemy")]
	[SerializeField] private float speedRatio;

	[Header("Angular")]
	[Tooltip("Torque maximum amount")]
	[SerializeField] private float torque;

	[Tooltip("Torque acceleartion speed")]
	[SerializeField] private float torqueLerp;

	[Header("Turbo")]
	[Tooltip("Turbo speed animation curve")]
	[SerializeField] private AnimationCurve turboCurve;

	[Tooltip("Turbo speed animation duration")]
	[SerializeField] private float turboDuration;

	[Tooltip("Turbo speed scale")]
	[SerializeField] private float turboScale;

	[Header("Shield")]
	[Tooltip("Shields spawn position transforms")]
	[SerializeField] private Transform[] shieldPositions;

	[Header("Dead")]
	[Tooltip("Dead explosion game object")]
	[SerializeField] private GameObject explosionObject;

	[Tooltip("Delay after game finishes to destroy last player")]
	[SerializeField] private float destroyDelay;

	[Header("References")]
	[Tooltip("Player rigidbody reference")]
	[SerializeField] private Rigidbody rb;

	[Tooltip("Player visual elements reference")]
	[SerializeField] private GameObject visualObject;

	[Tooltip("Player ships root transform reference")]
	[SerializeField] private Transform shipsTrans;

	[Tooltip("Player ship trail renderer reference")]
	[SerializeField] private TrailRenderer trail;

	[Tooltip("Player hability particle system")]
	[SerializeField] private ParticleSystem habilityParticles;

	[Tooltip("Player bloom trail scale animation reference")]
	[SerializeField] private ScaleObject scaleObject;

	[Tooltip("Gameplay elements pool manager reference")]
	[SerializeField] private GameplayElementsPool gameplayElementsPool;
	#endregion

	#region Private Members
	private PlayerStates state;					// Current player state
	private float timeCounter;					// Player states time counter
	private bool touching;						// Current input touching state to rotate player
	private Vector3 initPosition;				// Position when game starts
	private float currentSpeed;					// Current player speed for start acceleration
	private float currentTorque;				// Current player angular velocity for acceleration
	private Color trailColor;					// Trail renderer color during explosion fade out
	private Color trailColorInit;				// Trail renderer color when game starts
	private Vector3 startPosition;				// Start player animation final position
	private Vector3 exitPosition;				// Exit player animation final position
	private GameObject[] ships;					// Player ships game objects array
	private GameplayManager gameplayManager;	// Gameplay manager reference for state based behaviours
	private bool firstTouch;					// Input first touch to start playing
	private Collider[] colls;					// Ships colliders references
	private bool turbo;							// Current turbo state
	private float turboCounter;					// Turbo speed state time counter
	private GameManager gameManager;			// Game manager reference
	private bool bannerDown;					// Current displaying banner state
	#endregion

	#region Main Methods
	private void Update()
	{
		switch (state)
		{
			case PlayerStates.GAME:
			{
				// Update current speed for player acceleration
				currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime*2f);

				// Update current touching state based on screen touch count
				if (!firstTouch) firstTouch = ((Input.touchCount > 0) && (Input.touches[0].phase == TouchPhase.Ended));
				else
				{
					// Check if screen is touched
					if (Input.touchCount > 0)
					{
						if (!gameplayManager.CanHability) touching = true;
						else
						{
							if (Input.touches[0].position.y/Screen.height > (bannerDown ? bannerScreenMinHeight : screenMinHeight)) touching = true;
							else if (gameManager.CurrentShip < (gameManager.Ships.Length - 1))	// Check if current spaceship is not ultimate
							{
								float horizontal = Input.touches[0].position.x/Screen.width;
								if ((horizontal <= screenMinWidth) || (horizontal >= (1f - screenMinWidth))) touching = true;
							}
						}
					}
					else touching = false;
				}

				#if UNITY_EDITOR
				if (Input.GetMouseButton(0))
				{
					if (!gameplayManager.CanHability) touching = true;
					else
					{
						if (Input.mousePosition.y/Screen.height > (bannerDown ? bannerScreenMinHeight : screenMinHeight)) touching = true;
						else if (gameManager.CurrentShip < (gameManager.Ships.Length - 1))	// Check if current spaceship is not ultimate
						{
							float horizontal = Input.mousePosition.x/Screen.width;
							if ((horizontal <= screenMinWidth) || (horizontal >= (1f - screenMinWidth))) touching = true;
						}
					}
				}
				else touching = false;
				#endif

				// Update current torque for player rotation acceleration
				if (touching) currentTorque = Mathf.Lerp(currentTorque, torque, Time.deltaTime*torqueLerp);
				else currentTorque = Mathf.Lerp(currentTorque, 0f, Time.deltaTime*torqueLerp);
			} break;
			case PlayerStates.DEAD:
			{
				// Get current trail material color
				trailColor = trail.sharedMaterial.GetColor("_TintColor");

				// Update alpha to fade out trail color
				if (trailColor.a > 0)
				{
					trailColor.a -= Time.deltaTime*2f;
					trail.sharedMaterial.SetColor("_TintColor", trailColor);
				}
				else enabled = false;
			} break;
		}

		if (Input.GetKeyDown(KeyCode.Space)) SetShield();
	}

	private void FixedUpdate () 
	{
		switch (state)
		{
			case PlayerStates.START:
			{
				if (timeCounter <= startDuration)
				{
					// Update player position based on start animation curve
					rb.MovePosition(Vector3.Lerp(initPosition, startPosition, startCurve.Evaluate(timeCounter/startDuration)));

					// Update time counter
					timeCounter += Time.fixedDeltaTime;
				}
				else if (transform.position != startPosition) rb.MovePosition(startPosition);
			} break;
			case PlayerStates.GAME:
			{
				if (turbo)
				{
					// Calculate final turbo speed
					float finalSpeed = currentSpeed + turboCurve.Evaluate(turboCounter/turboDuration)*turboScale;
					rb.velocity = transform.TransformDirection(Vector3.up)*finalSpeed;

					// Update turbo time counter
					turboCounter += Time.deltaTime;

					if (turboCounter >= turboDuration)
					{
						// Reset turbo state and time counter
						turbo = false;
						turboCounter = 0f;
					}
				}
				else rb.velocity = transform.TransformDirection(Vector3.up)*currentSpeed; 		// Apply forward velocity to rigidbody

				// Apply angular velocity to rigidbody
				rb.angularVelocity = Vector3.back*currentTorque;

				// Fix hability particle system rotation
				habilityParticles.transform.rotation = Quaternion.identity;
			} break;
			case PlayerStates.EXIT:
			{
				// Update player position based on exit animation
				rb.MovePosition(Vector3.Lerp(transform.position, exitPosition, Time.fixedDeltaTime*3f));
			} break;
			default: break;
		}
	}
	#endregion

	#region Player Methods
	public void Initialize (Vector3 position, Vector3 startPos, Vector3 exitPos, GameplayManager manager, bool banner)
	{
		// Get references
		if (!gameplayElementsPool) gameplayElementsPool = transform.parent.GetComponent<GameplayElementsPool>();
		colls = shipsTrans.GetComponentsInChildren<Collider>();
		gameManager = GameManager.Instance;

		// Initialize values
		state = PlayerStates.START;
		timeCounter = 0f;
		touching = false;
		transform.position = position;
		initPosition = position;
		currentSpeed = 0f;
		currentTorque = 0f;
		startPosition = startPos;
		exitPosition = exitPos;
		gameplayManager = manager;
		firstTouch = false;
		turbo = false;
		turboCounter = 0f;
		bannerDown = banner;
		trailColorInit = trail.sharedMaterial.GetColor("_TintColor");

		// Force screen min height to detect input if current ship doesn't have habilities
		if (gameManager.CurrentShip == 0)
		{
			screenMinHeight = 0f;
			bannerScreenMinHeight = 0f;
		}

		SetModel();
	}

	public void SetGame()
	{
		// Update player state
		state = PlayerStates.GAME;
		rb.isKinematic = false;
		for (int i = 0; i < colls.Length; i++) colls[i].enabled = true;
		trail.Clear();
		trail.enabled = true;

		// Play bloom trail scale animation
		scaleObject.Play();
	}

	public void FinishGame()
	{
		// Update player state
		state = PlayerStates.DEAD;
		rb.isKinematic = true;
		for (int i = 0; i < colls.Length; i++) colls[i].enabled = false;

		// Instantiate explosion game object
		visualObject.SetActive(false);
		explosionObject.SetActive(true);
		explosionObject.transform.LookAt(Vector3.forward);
	}

	public void RestartPlayer(bool banner)
	{
		// Reset values
		state = PlayerStates.START;
		transform.position = initPosition;
		transform.rotation = Quaternion.identity;

		timeCounter = 0f;
		touching = false;
		currentSpeed = 0f;
		currentTorque = 0f;
		firstTouch = false;
		turbo = false;
		turboCounter = 0f;
		bannerDown = banner;
		if (habilityParticles.isPlaying) habilityParticles.Stop();
		trail.Clear();
		trail.sharedMaterial.SetColor("_TintColor", trailColorInit);
		trail.enabled = false;
		visualObject.SetActive(true);
		explosionObject.SetActive(false);
		enabled = true;
		gameObject.SetActive(true);
	}

	public void ExitGame()
	{
		// Update player state
		state = PlayerStates.EXIT;

		// Enable player trail renderer
		trail.Clear();
		trail.enabled = true;

		// Play bloom trail scale animation
		scaleObject.Play();
	}

	public void SetModel()
	{
		// Initialize ship visual game objects
		ships = new GameObject[shipsTrans.childCount];
		for (int i = 0; i < ships.Length; i++)
		{
			ships[i] = shipsTrans.GetChild(i).gameObject;
			ships[i].SetActive(false);
		}

		// Set ship visual based on current selected ship in game manager
		ships[gameManager.CurrentShip].SetActive(true);
	}

	public void IncreaseSpeed()
	{
		// Increase player max speed
		speed += speedRatio;
		torqueLerp += speedRatio/2f;
		torque += speedRatio/4f;
	}

	public void SetTurbo()
	{
		// Enable turbo state and reset time counter
		turbo = true;
		turboCounter = 0f;
	}

	public void SetShield()
	{
		Shield shield = null;
		for (int i = 0; i < shieldPositions.Length; i++)
		{
			shield = gameplayElementsPool.AddShield();
			if (shield) shield.Initialize(shieldPositions[i].position, shieldPositions[i].rotation);
		}
	}

	public void UseHability()
	{
		// Play hability particle system
		habilityParticles.Play();
	}

	#region Player Internal Methods
	private void OnTriggerEnter(Collider other)
	{
		switch (other.tag)
		{
			case "Coin":
			{
				gameplayManager.AddCoin(other.transform.position);
				gameplayElementsPool.DefaultCoin.Explode();
			} break;
			case "Bonus":
			{
				gameplayManager.AddBonus(other.transform.position);
				gameplayElementsPool.BonusCoin.Explode();
			} break;
			case "Extra":
			{
				gameplayManager.AddExtra(other.transform.position);
				gameplayElementsPool.ExtraCoin.Explode();
			} break;
			case "Killer": gameplayManager.FinishGame(); break;
			default: break;
		}
	}
	#endregion
	#endregion
}
