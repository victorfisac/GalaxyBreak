using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayElementsPool : MonoBehaviour
{
	#region Inspector Members
	[Header("Coins")]
	[Tooltip("Default coin logic reference")]
	[SerializeField] private Coin defaultCoin; 

	[Tooltip("Bonus coin logic reference")]
	[SerializeField] private Coin bonusCoin;

	[Tooltip("Extra coin logic reference")]
	[SerializeField] private Coin extraCoin;

	[Header("Enemies")]
	[Tooltip("Enemies pool transform root reference")]
	[SerializeField] private Transform enemiesRoot;
	[SerializeField] private AudioSource enemiesSource;

	[Header("Shields")]
	[Tooltip("Shields pool transform root reference")]
	[SerializeField] private Transform shieldsRoot;
	#endregion

	#region Private Members
	private GameObject[] enemies;		// Enemies cached game objects
	private Enemy[] enemiesLogic;		// Enemies cached behaviours references
	private GameObject[] shields;		// Shields cached game objects
	private Shield[] shieldsLogic;		// Shields cached behaviours references
	#endregion

	#region Main Methods
	private void Start()
	{
		// Initialize values
		enemies = new GameObject[enemiesRoot.childCount];
		enemiesLogic = new Enemy[enemies.Length];
		for (int i = 0; i < enemies.Length; i++)
		{
			enemies[i] = enemiesRoot.GetChild(i).gameObject;
			enemiesLogic[i] = enemies[i].GetComponent<Enemy>();
		}

		shields = new GameObject[shieldsRoot.childCount];
		shieldsLogic = new Shield[shields.Length];
		for (int i = 0; i < shields.Length; i++)
		{
			shields[i] = shieldsRoot.GetChild(i).gameObject;
			shieldsLogic[i] = shields[i].GetComponent<Shield>();
		}
	}
	#endregion

	#region Pool Methods
	public void AddCoin(Vector3 position)
	{
		// Initialize default coin at desired position
		defaultCoin.Initialize(position);
	}

	public void AddBonus(Vector3 position)
	{
		// Initialize default coin at desired position
		bonusCoin.Initialize(position);
	}

	public void AddExtra(Vector3 position)
	{
		// Initialize default coin at desired position
		extraCoin.Initialize(position);
	}

	public void ExplodeCoins()
	{
		if (defaultCoin.gameObject.activeSelf) defaultCoin.Explode();
		if (bonusCoin.gameObject.activeSelf) bonusCoin.Explode();
		if (extraCoin.gameObject.activeSelf) extraCoin.Explode();
	}

	public Enemy AddEnemy()
	{
		Enemy result = null;

		for (int i = 0; i < enemies.Length; i++)
		{
			if (!enemies[i].activeSelf)
			{
				result = enemiesLogic[i];
				break;
			}
		}

		#if DEBUG_INFO
		if (!result) Debug.LogWarning("GameplayElementsPool: no available enemies to initialize");
		#endif

		return result;
	}

	public void Kaboom()
	{
		for (int i = 0; i < enemies.Length; i++)
		{
			if (enemies[i].activeSelf) enemiesLogic[i].Kaboom();
		}
	}

	public void ExplodeEnemies()
	{
		for (int i = 0; i < enemies.Length; i++)
		{
			if (enemies[i].activeSelf)
			{
				enemiesSource.Play();
				break;
			}
		}

		for (int i = 0; i < enemies.Length; i++)
		{
			if (enemies[i].activeSelf) enemiesLogic[i].Explode();
		}
	}

	public Shield AddShield()
	{
		Shield result = null;

		for (int i = 0; i < shields.Length; i++)
		{
			if (!shields[i].activeSelf)
			{
				result = shieldsLogic[i];
				break;
			}
		}

		#if DEBUG_INFO
		if (!result) Debug.LogWarning("GameplayElementsPool: no available shields to initialize");
		#endif

		return result;
	}

	public void ExplodeShields()
	{
		for (int i = 0; i < shields.Length; i++)
		{
			if (shields[i].activeSelf) shieldsLogic[i].Explode();
		}
	}
	#endregion

	#region Properties
	public Coin DefaultCoin
	{
		get { return defaultCoin; }
	}

	public Coin BonusCoin
	{
		get { return bonusCoin; }
	}

	public Coin ExtraCoin
	{
		get { return extraCoin; }
	}

	public bool IsDefaultCoin
	{
		get { return defaultCoin.gameObject.activeSelf; }
	}

	public bool IsBonusCoin
	{
		get { return bonusCoin.gameObject.activeSelf; }
	}

	public bool IsExtraCoin
	{
		get { return extraCoin.gameObject.activeSelf; }
	}
	#endregion
}
