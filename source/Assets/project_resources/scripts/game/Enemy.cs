using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour 
{
	#region Inspector Members
	[Header("Settings")]
	[Tooltip("Enemy maximum forward speed")]
	[SerializeField] private float speed;

	[Tooltip("Enemy direction interpolation speed")]
	[SerializeField] private float movementLerp;

	[Tooltip("Enemy game object destroy delay after explosion")]
	[SerializeField] private float destroyDelay;

	[Tooltip("Gameplay space range to enable enemy collider")]
	[SerializeField] private Vector2 gameplayRange;

	[Header("References")]
	[Tooltip("Enemy rigidbody reference")]
	[SerializeField] private Rigidbody rb;

	[Tooltip("Enemy collider reference")]
	[SerializeField] private Collider coll;

	[Tooltip("Enemy trail renderer reference")]
	[SerializeField] private TrailRenderer trail;

	[Tooltip("Enemy visual elements game object")]
	[SerializeField] private GameObject visualObject;

	[Tooltip("Enemy explosion game object")]
	[SerializeField] private GameObject explosionObject;

	[Tooltip("Enemy spawn audio source")]
	[SerializeField] private AudioSource enemySource;
	#endregion

	#region Private Members
	private Transform target;							// Enemy follow logic target transform
	private Transform playerTarget;						// Player target reference to reset value
	private Vector3 direction;							// Direction vector from enemy to target position
	private GameplayController gameplayController;		// Gameplay controller reference
	private float speedInit;							// Enemy speed by default
	private float movementLerpInit;						// Enemy movement lerp by default
	private Color trailColor;							// Current trail renderer color during explosion effect
	private Color trailColorInit;						// Trail renderer color at start
	private List<Shield> shields;						// Current detected shields
	private Shield currentShield;						// Current shield target
	private float defaultLerp;							// Default movement lerp when following player
	#endregion

	#region Main Methods
	private void Start()
	{
		// Initialize values
		speedInit = speed;
		movementLerpInit = movementLerp;
		trailColorInit = trail.material.GetColor("_TintColor");
		trail.Clear();
		shields = new List<Shield>();
		currentShield = null;

		// Disable game object by default
		gameObject.SetActive(false);
	}

	private void FixedUpdate()
	{
		// Check if enemy exploded to switch its behaviour
		if (!explosionObject.activeSelf)
		{
			// Check if target transform is not destroyed
			if (target)
			{
				if (currentShield && currentShield.IsDestroyed)
				{
					float distance = Mathf.Infinity;
					Shield newTarget = null;
					foreach (Shield shield in shields)
					{
						if (shield != currentShield)
						{
							Vector3 direction = shield.transform.position - transform.position;
							if (direction.magnitude < distance)
							{
								distance = direction.magnitude;
								newTarget = shield;
							}
						}
					}

					// Assign new shield target or player if any other shield is available or 
					if (!newTarget)
					{
						currentShield = null;
						target = playerTarget;
						movementLerp = defaultLerp;
					}
					else
					{
						currentShield = newTarget;
						target = newTarget.transform;
						movementLerp = defaultLerp*5f;
					}
				}

				if (!coll.enabled && !rb.isKinematic) 
					coll.enabled = (transform.position.x > -gameplayRange.x && transform.position.x < gameplayRange.x && transform.position.y > -gameplayRange.y && transform.position.y < gameplayRange.y);

				// Calcualte target direction
				Vector3 newDirection = target.position - transform.position;
				newDirection.Normalize();

				// Interpolate between new position direction and previous
				direction = Vector3.Lerp(direction, newDirection, Time.fixedDeltaTime*gameplayController.TimeScale*10);

				// Apply forward velocity to rigidbody
				rb.velocity = Vector3.Lerp(rb.velocity, direction*speed*gameplayController.TimeScale, Time.fixedDeltaTime*movementLerp);

				// Apply angular velocity to rigidbody
				rb.MoveRotation(Quaternion.LookRotation(rb.velocity.normalized));
			}
			else if (gameplayController.CurrentPlayer) target = gameplayController.CurrentPlayer.transform;
		}
		else
		{
			// Get current trail material color
			trailColor = trail.material.GetColor("_TintColor");

			// Update alpha to fade out trail color
			if (trailColor.a > 0)
			{
				trailColor.a -= Time.fixedDeltaTime*2f;
				trail.material.SetColor("_TintColor", trailColor);
			}
			else enabled = false;
		}
	}
	#endregion

	#region Enemy Methods
	public void Initialize(Vector3 position, Transform targetTrans, GameplayController controller, float extraSpeed)
	{
		// Initialize values
		transform.position = position;
		transform.LookAt(targetTrans);
		target = targetTrans;
		playerTarget = targetTrans;
		gameplayController = controller;
		speed = speedInit + extraSpeed;
		movementLerp = movementLerpInit + extraSpeed/4f;
		defaultLerp = movementLerp;

		// Reset to default values
		rb.isKinematic = false;
		coll.enabled = false;
		trail.material.SetColor("_TintColor", trailColorInit);
		trail.Clear();
		visualObject.SetActive(true);
		explosionObject.SetActive(false);
		gameObject.SetActive(true);
		enabled = true;
		enemySource.Play();
	}

	public void Explode()
	{
		// Update rigidbody kinematic and collider enabled states
		rb.isKinematic = true;
		coll.enabled = false;

		// Remove all shields references
		shields.Clear();
		currentShield = null;

		// Enable explosion game object and disable visual elements
		visualObject.SetActive(false);
		explosionObject.SetActive(true);
		explosionObject.transform.LookAt(Vector3.forward);

		Invoke("Unitialize", destroyDelay);
	}

	public void Kaboom()
	{
		if (!explosionObject.activeSelf)
		{
			// Add score due to kaboom explosion without increase destroyed missiles to unlock hability
			gameplayController.Gameplay.AddScore(1, transform.position, false);

			// Apply explosion visual logic
			Explode();
		}
	}

	#region Enemy Internal Methods
	private void Unitialize()
	{
		// Remove all shields references
		shields.Clear();

		// Disable game object
		gameObject.SetActive(false);
	}
	#endregion
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Killer")
		{
			// Check if missile crashed with gameplay limits to always play explosion sound
			bool playAlway = (other.gameObject.name == "GameplayLimits");
			gameplayController.Gameplay.AddScore(1, transform.position, playAlway);

			// Update current destroyed missiles to unlock hability
			gameplayController.IncreaseHability();

			// Apply explosion visual logic
			Explode();
		}
		else if (other.tag == "Shield")
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Detection"))
			{
				// Get shield reference from detected object
				Shield shield = other.transform.parent.GetComponent<Shield>();
				if (shield)
				{
					// Add detected shield to available shields to follow list
					shields.Add(shield);

					// Check if current shield target is not assigned
					if (!currentShield)
					{
						// Assign current shield and target
						currentShield = shield;
						target = currentShield.transform;
						movementLerp = defaultLerp*5f;
					}
				}
			}
			else
			{
				// Add score to gameplay manager
				gameplayController.Gameplay.AddScore(1, transform.position, true);

				// Apply explosion visual logic to shield
				Shield shield = other.GetComponent<Shield>();
				if (shield) shield.Explode();

				// Apply explosion visual logic
				Explode();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Shield")
		{
			// Get shield reference from detected object and remove from list
			Shield shield = other.transform.parent.GetComponent<Shield>();
			if (shield) shields.Remove(shield);
		}
	}
	#endregion
}
