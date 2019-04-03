using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicElementsPool : MonoBehaviour
{
	#region Inspector Members
	[Header("References")]
	[Tooltip("Add scores root transform")]
	[SerializeField] private Transform addScoresRoot;

	[Tooltip("Add coins root transform")]
	[SerializeField] private Transform addCoinsRoot;

	[Tooltip("Add bonus root transform")]
	[SerializeField] private Transform addBonusRoot;

	[Tooltip("Add extra root transform")]
	[SerializeField] private Transform addExtrasRoot;
	#endregion

	#region Private Members
	private GameObject[] addScores;		// Add scores cached game objects
	private GameObject[] addCoins;		// Add coins cached game objects
	private GameObject[] addBonus;		// Add bonus cached game objects
	private GameObject[] addExtras;		// Add extra cached game objects
	#endregion

	#region Main Methods
	private void Start()
	{
		// Initialize values
		addScores = new GameObject[addScoresRoot.childCount];
		for (int i = 0; i < addScores.Length; i++) addScores[i] = addScoresRoot.GetChild(i).gameObject;

		addCoins = new GameObject[addCoinsRoot.childCount];
		for (int i = 0; i < addCoins.Length; i++) addCoins[i] = addCoinsRoot.GetChild(i).gameObject;

		addBonus = new GameObject[addBonusRoot.childCount];
		for (int i = 0; i < addBonus.Length; i++) addBonus[i] = addBonusRoot.GetChild(i).gameObject;

		addExtras = new GameObject[addExtrasRoot.childCount];
		for (int i = 0; i < addExtras.Length; i++) addExtras[i] = addExtrasRoot.GetChild(i).gameObject;
	}
	#endregion

	#region Pool Methods
	public GameObject AddScore()
	{
		GameObject result = null;

		for (int i = 0; i < addScores.Length; i++)
		{
			if (!addScores[i].gameObject.activeSelf)
			{
				result = addScores[i].gameObject;
				break;
			}
		}

		#if DEBUG_INFO
		if (!result) Debug.LogWarning("DynamicElementsPool: no available score effects to initialize");
		#endif

		return result;
	}

	public GameObject AddCoin()
	{
		GameObject result = null;

		for (int i = 0; i < addCoins.Length; i++)
		{
			if (!addCoins[i].gameObject.activeSelf)
			{
				result = addCoins[i].gameObject;
				break;
			}
		}

		#if DEBUG_INFO
		if (!result) Debug.LogWarning("DynamicElementsPool: no available coin effects to initialize");
		#endif

		return result;
	}

	public GameObject AddBonus()
	{
		GameObject result = null;

		for (int i = 0; i < addBonus.Length; i++)
		{
			if (!addBonus[i].gameObject.activeSelf)
			{
				result = addBonus[i].gameObject;
				break;
			}
		}

		#if DEBUG_INFO
		if (!result) Debug.LogWarning("DynamicElementsPool: no available bonus coin effects to initialize");
		#endif

		return result;
	}

	public GameObject AddExtra()
	{
		GameObject result = null;

		for (int i = 0; i < addExtras.Length; i++)
		{
			if (!addExtras[i].gameObject.activeSelf)
			{
				result = addExtras[i].gameObject;
				break;
			}
		}

		#if DEBUG_INFO
		if (!result) Debug.LogWarning("DynamicElementsPool: no available extra coin effects to initialize");
		#endif

		return result;
	}
	#endregion
}
