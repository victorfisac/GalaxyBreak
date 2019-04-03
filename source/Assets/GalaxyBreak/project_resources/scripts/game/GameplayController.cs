using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameplayController : MonoBehaviour 
{
    #region Inspector Members
    [Header("Player")]
	[Tooltip("Player logic reference")]
	[SerializeField] private Player player;

	[Tooltip("Player spawn position transform")]
	[SerializeField] private Transform spawnTrans;

	[Tooltip("Player start animation final position transform")]
	[SerializeField] private Transform startTrans;

	[Tooltip("Player exit animation final position transform")]
	[SerializeField] private Transform exitTrans;

	[Header("Limits")]
	[Tooltip("World limits colliders game objects for no ads")]
	[SerializeField] private GameObject[] worldLimits;

	[Header("Coins")]
	[Tooltip("How ofter a new coin is instantiated")]
	[SerializeField] private float coinFrequency;

	[Tooltip("How ofter a new bonus coin is instantiated")]
	[SerializeField] private float bonusFrequency;

	[Tooltip("How ofter a new extra coin is instantiated")]
	[SerializeField] private float extraFrequency;

	[Tooltip("World space axes ranges to spawn coins")]
	[SerializeField] private Vector2 coinRanges;

	[Header("Enemies")]
	[Tooltip("How ofter a new enemy is spawned")]
	[SerializeField] private float enemyFrequency;

	[Tooltip("How decreases spawn duration for each spawned enemies")]
	[SerializeField] private float difficultyRatio;

	[Tooltip("How increases spawned enemies speed")]
	[SerializeField] private float speedRatio;

	[Tooltip("Possible positions to spawn enemies")]
	[SerializeField] private Transform enemyPositionsRoot;

	[Header("Habilities")]
	[Tooltip("Time scale to interpolate when freeze hability is used")]
	[SerializeField] private float freezeTimeScale;

	[Tooltip("Freeze hability state duration")]
	[SerializeField] private float freezeDuration;

	[Tooltip("How much enemies needs to destroy to unlock hability")]
	[SerializeField] private int habilityEnemies;

	[Header("References")]
	[Tooltip("Gameplay elements pool manager reference")]
	[SerializeField] private GameplayElementsPool gameplayElementsPool;
    #endregion
    
    #region Private Members
	private GameplayManager gameplayManager;	// Gameplay manager reference
	private float timeCounter;					// Gameplay duration time counter
	private float coinCounter;					// Score instantiate time counter
	private float bonusCounter;					// Bonus instantiate time counter
	private float extraCounter;					// Extra instantiate time counter
	private float enemyCounter;					// Enemy instantiate time counter
	private int spawnedEnemies;					// Total spawned enemies in current gameplay
	private float currentSpeed;					// Current spawned enemy speed
	private Transform[] enemyPositions;			// Possible positions to spawn new enemies
	private bool timeFreeze;					// Current time freeze state
	private float freezeCounter;				// Freeze state time counter
	private float timeScale;					// Current time scale
	private int habilityCount;					// Destroyed enemies for hability count
    #endregion
    
    #region Main Methods
	public void Initialize (GameplayManager manager)
    {
    	// Get references
		enemyPositions = new Transform[enemyPositionsRoot.childCount];
		for (int i = 0; i < enemyPositions.Length; i++) enemyPositions[i] = enemyPositionsRoot.GetChild(i);

    	// Initialize values
    	gameplayManager = manager;
    	timeCounter = 0f;
    	coinCounter = 0f;
    	bonusCounter = 0f;
    	extraCounter = 0f;
    	enemyCounter = 0f;
    	spawnedEnemies = 0;
    	currentSpeed = 0f;
		timeFreeze = false;
		freezeCounter = 0f;
		timeScale = 1f;
		habilityCount = 0;
		for (int i = 0; i < worldLimits.Length; i++) worldLimits[i].SetActive(false);

		// Initialize player
		if (!player) player = GameObject.FindWithTag("Player").GetComponent<Player>();
		player.Initialize(spawnTrans.position, startTrans.position, exitTrans.position, gameplayManager, gameplayManager.BannerDown);
	}

	private void Update()
	{
		// Update gameplay time counter
		timeCounter += Time.deltaTime;

		UpdateLimits();
		UpdateElements();
		UpdateTimeScale();
	}
    #endregion

    #region Controller Methods
    public void SetGame()
    {
		// Enable spawning behaviour
    	enabled = true;

    	// Update player state to game behaviour
    	player.SetGame();
    }

    public void FinishGame()
    {
		// Explode all enabled coins
		gameplayElementsPool.ExplodeCoins();

		// Explode all enabled enemies
		gameplayElementsPool.ExplodeEnemies();

		// Explode all enabled shields
		gameplayElementsPool.ExplodeShields();

    	// Reset controller values
    	timeCounter = 0f;
		coinCounter = 0f;
		bonusCounter = 0f;
		extraCounter = 0f;
		enemyCounter = 0f;
		spawnedEnemies = 0;
		currentSpeed = 0f;
		timeFreeze = false;
		freezeCounter = 0f;
		timeScale = 1f;
		habilityCount = 0;
		for (int i = 0; i < worldLimits.Length; i++) worldLimits[i].SetActive(false);

    	// Update player state to finish behaviour
    	player.FinishGame();

		// Disable spawning behaviour
    	enabled = false;
    }

    public void ExitGame()
    {
    	// Update player state to exit behaviour
    	player.ExitGame();
    }

    #region Controller Internal Methods
    private void UpdateLimits()
    {
		if (gameplayManager.BannerDown)
		{
			if (GameManager.Instance.ShowingAd)
			{
				if (!worldLimits[0].activeSelf || worldLimits[1].activeSelf)
				{
					worldLimits[0].SetActive(true);
					worldLimits[1].SetActive(false);
				}
			}
			else if (!worldLimits[1].activeSelf || worldLimits[0].activeSelf)
			{
				worldLimits[0].SetActive(false);
				worldLimits[1].SetActive(true);
			}
		}
		else if (!worldLimits[1].activeSelf || worldLimits[0].activeSelf)
		{
			worldLimits[0].SetActive(false);
			worldLimits[1].SetActive(true);
		}
    }

    private void UpdateElements()
    {
		// Coins instantiate logic based on spawn frequency and range random positions
		if (coinCounter < coinFrequency) coinCounter += Time.deltaTime;
		else
		{
			coinCounter = 0f;
			if (!gameplayElementsPool.IsDefaultCoin)
			{
				Vector3 coinPos = new Vector3(Random.Range(-coinRanges.x, coinRanges.x), Random.Range(-coinRanges.y, coinRanges.y), player.transform.position.z);
				gameplayElementsPool.AddCoin(coinPos);
			}
		}

		// Bonus coins instantiate logic based on spawn frequency and range random positions
		if (bonusCounter < bonusFrequency) bonusCounter += Time.deltaTime;
		else
		{
			bonusCounter = 0f;
			if (!gameplayElementsPool.IsBonusCoin)
			{
				Vector3 coinPos = new Vector3(Random.Range(-coinRanges.x, coinRanges.x), Random.Range(-coinRanges.y, coinRanges.y), player.transform.position.z);
				gameplayElementsPool.AddBonus(coinPos);
			}
		}

		// Extra coins instantiate logic based on spawn frequency and range random positions
		if (extraCounter < extraFrequency) extraCounter += Time.deltaTime;
		else
		{
			extraCounter = 0f;
			if (!gameplayElementsPool.IsExtraCoin)
			{
				Vector3 coinPos = new Vector3(Random.Range(-coinRanges.x, coinRanges.x), Random.Range(-coinRanges.y, coinRanges.y), player.transform.position.z);
				gameplayElementsPool.AddExtra(coinPos);
			}
		}

		// Enemies instantiate logic based on spawn frequency and specific random positions
		if (enemyCounter < Mathf.Max(1f, enemyFrequency - spawnedEnemies*difficultyRatio)) enemyCounter += Time.deltaTime;
		else
		{
			enemyCounter = 0f;
			spawnedEnemies++;
			InstantiateEnemy(Random.Range(0, enemyPositions.Length));
		}
    }

	private void UpdateTimeScale()
	{
		// Check if time freeze is enabled
		if (timeFreeze)
		{
			// Update current time scale based on linear interpolation
			timeScale = Mathf.Lerp(timeScale, freezeTimeScale, Time.deltaTime);

			// Update time counter to reset time scale
			freezeCounter += Time.deltaTime;

			if (freezeCounter >= freezeDuration)
			{
				// Reset freeze state and time counter
				timeFreeze = false;
				freezeCounter = 0f;
			}
		}
		else timeScale = Mathf.Lerp(timeScale, 1f, Time.deltaTime);
	}
    #endregion
    #endregion

    #region Elements Methods
    public void IncreaseHability()
    {
		// Check if current ship doesn't have any hability
    	if (GameManager.Instance.CurrentShip > 0)
    	{
			// Update current destroyed enemies
	    	habilityCount++;

	    	if (habilityCount >= habilityEnemies)
	    	{
	    		// Enable hability feature and reset counter
				gameplayManager.EnableHabilities();
	    		habilityCount = 0;
	    	}
	    }
    }

    public void RestartPlayer()
    {
		// Initialize player
		player.RestartPlayer(gameplayManager.BannerDown);
    }

    #region Elements Internal Methods
	private void InstantiateEnemy(int posIndex)
	{
		// Instantiate bonus game object
		Enemy newEnemy = gameplayElementsPool.AddEnemy();
		newEnemy.Initialize(enemyPositions[posIndex].position, player.transform, this, currentSpeed);
		currentSpeed += speedRatio;
	}
    #endregion
    #endregion

    #region Skills Methods
    public void UseHability(int hability)
    {
    	// Apply logic based on hability index
    	switch (hability)
    	{
    		case 0:
    		{
    			// Enable player turbo state and update used turbo count
    			player.SetTurbo();

    			#if DEBUG_INFO
    			Debug.Log("GameplayController: using turbo hability");
    			#endif
    		} break;
    		case 1:
    		{
    			// Enable player shield logic and update used shield count
    			player.SetShield();

				#if DEBUG_INFO
    			Debug.Log("GameplayController: using shield hability");
    			#endif
    		} break;
    		case 2:
    		{
				// Explode all spawned enemies and update used kaboom count
				gameplayElementsPool.Kaboom();

				#if DEBUG_INFO
    			Debug.Log("GameplayController: using kaboom hability");
    			#endif
			} break;
			case 3:
			{
				// Enable time freeze state, reset time counter and update used freeze count
		    	timeFreeze = true;
		    	freezeCounter = 0f;

				#if DEBUG_INFO
    			Debug.Log("GameplayController: using turbo hability");
    			#endif
			} break;
			default: break;
    	}

    	// Play player hability visual effects
    	player.UseHability();

		// Enable gameplay interface hability effect
		gameplayManager.UseHability(hability);
    }
    #endregion

    #region Properties
    public GameplayManager Gameplay
    {
    	get { return gameplayManager; }
    }

    public Player CurrentPlayer
    {
    	get { return player; }
    }

    public float TimeScale
    {
    	get { return timeScale; }
    }
    #endregion
}
