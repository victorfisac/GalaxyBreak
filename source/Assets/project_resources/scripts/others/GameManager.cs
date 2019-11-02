using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
	#region Static Members
	private static GameManager instance = null;
	#endregion

	#region Private Members
	private bool initialized;					// Is singleton instance initialized
	private int alreadyPlayed;					// Has player already played before
	private int coins;							// Player total coins amount
	private int bestScore;						// Best game score amount
	private int volume;							// Enabled volume current state
	private int[] ships;						// Purchased ships states
	private int currentShip;					// Current selected ship
	private int noAds;							// Purchased no advertising state
	private int totalTimes;						// Total times played
	#if !UNITY_EDITOR
	private bool isSplash;						// Current scene is splash screen state
	private bool goodAchievement;				// Good achievement unlocked state
	private bool increibleAchievement;			// Increible achievement unlocked state
	private bool awesomeAchievement;			// Awesome achievement unlocked state
	private bool bossAchievement;				// Boss achievement unlocked state
	private bool buyShipAchievement;			// Bought new ship achievement unlocked state
	private SplashUI splashUI;					// Splash interface reference
	private bool needLeaderboard;				// Open leaderboard after authentication
	private AndroidManager androidManager;		// Android manager reference
	#endif
	#endregion

	#region Initialization Methods
	#if !UNITY_EDITOR
	public void SetReferences(SplashUI splash)
	{
		// Update null references
		splashUI = splash;
	}
	#endif

	#region Initialization Internal Methods
	private void Initialize()
	{
		// Initialize values
		initialized = true;
		alreadyPlayed = 0;
		coins = 0;
		bestScore = 0;
		volume = 1;
		ships = new int[6];
		ships[0] = 1;
		noAds = 0;
		currentShip = 0;
		totalTimes = 0;

		#if !UNITY_EDITOR
		isSplash = false;
		goodAchievement = false;
		increibleAchievement = false;
		awesomeAchievement = false;
		bossAchievement = false;
		buyShipAchievement = false;
		#endif

		LoadData();

		// Initialize platform manager
		#if !UNITY_EDITOR
		androidManager = AndroidManager.Instance;
		#endif

		#if DEBUG_INFO
		Debug.Log("GameManager: game manager initialized successfully");
		#endif
	}
	#endregion
	#endregion

	#region Data Methods
	public void SaveData()
	{
		// Update in game values to PlayerPrefs
		PlayerPrefs.SetInt("Saved", 0);
		PlayerPrefs.SetInt("AlreadyPlayed", alreadyPlayed);
		PlayerPrefs.SetInt("coins", coins);
		PlayerPrefs.SetInt("bestScore", bestScore);
		PlayerPrefs.SetInt("volume", volume);
		PlayerPrefs.SetInt("CurrentShip", currentShip);
		PlayerPrefs.SetInt("NoAds", noAds);
		PlayerPrefs.SetInt("TotalTimes", totalTimes);
		for (int i = 0; i < ships.Length; i++) PlayerPrefs.SetInt("Ship" + i, ships[i]);
		#if !UNITY_EDITOR
		PlayerPrefs.SetInt("GoodAchievement", (goodAchievement ? 1 : 0));
		PlayerPrefs.SetInt("IncreibleAchievement", (increibleAchievement ? 1 : 0));
		PlayerPrefs.SetInt("AwesomeAchievement", (awesomeAchievement ? 1 : 0));
		PlayerPrefs.SetInt("BossAchievement", (bossAchievement ? 1 : 0));
		PlayerPrefs.SetInt("BuyShipAchievement", (buyShipAchievement ? 1 : 0));
		#endif

		// Save all data in PlayerPrefs to disk
		PlayerPrefs.Save();

		#if DEBUG_INFO
		Debug.Log("GameManager: local data saved successfully");
		#endif
	}

	#region Data Internal Methods
	private void LoadData()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to load local data");
		#endif

		// Check if player has data already saved
		if (!PlayerPrefs.HasKey("Saved")) SaveData();
		else
		{
			// Update in game values from PlayerPrefs
			alreadyPlayed = PlayerPrefs.GetInt("AlreadyPlayed");
			coins = PlayerPrefs.GetInt("coins");
			bestScore = PlayerPrefs.GetInt("bestScore");
			volume = PlayerPrefs.GetInt("volume");
			currentShip = PlayerPrefs.GetInt("CurrentShip");
			noAds = PlayerPrefs.GetInt("NoAds");
			totalTimes = PlayerPrefs.GetInt("TotalTimes");
			for (int i = 0; i < ships.Length; i++) ships[i] = PlayerPrefs.GetInt("Ship" + i);
			#if !UNITY_EDITOR
			goodAchievement = (PlayerPrefs.GetInt("GoodAchievement") == 1);
			increibleAchievement = (PlayerPrefs.GetInt("IncreibleAchievement") == 1);
			awesomeAchievement = (PlayerPrefs.GetInt("AwesomeAchievement") == 1);
			bossAchievement = (PlayerPrefs.GetInt("BossAchievement") == 1);
			buyShipAchievement = (PlayerPrefs.GetInt("BuyShipAchievement") == 1);
			#endif

			// Initialize external members
			AudioListener.volume = volume;

			#if DEBUG_INFO
			Debug.Log("GameManager: local data loaded successfully");
			#endif
		}
	}
	#endregion
	#endregion

	#if !UNITY_EDITOR
	#region Services Methods
	public void InitPlayServices()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to initialized Google Play Services");
		#endif

		#if UNITY_ANDROID
		androidManager.InitPlayServices();
		#endif
	}

	public void CheckAchievements()
	{
		if (bestScore >= ProjectManager.goodAchievementScore)
		{
			goodAchievement = true;
			UploadAchievement(ProjectManager.goodAchievement);
		}

		if (bestScore >= ProjectManager.increibleAchievementScore)
		{
			increibleAchievement = true;
			UploadAchievement(ProjectManager.increibleAchievement);
		}

		if (bestScore >= ProjectManager.awesomeAchievementScore)
		{
			awesomeAchievement = true;
			UploadAchievement(ProjectManager.awesomeAchievement);
		}

		if (bestScore >= ProjectManager.bossAchievementScore)
		{
			bossAchievement = true;
			UploadAchievement(ProjectManager.bossAchievement);
		}

		for (int i = 1; i < ships.Length; i++)
		{
			if (ships[i] == 1)
			{
				buyShipAchievement = true;
				UploadAchievement(ProjectManager.buyShipAchievement);
				break;
			}
		}

		SaveData();

		// Switch to game scene if current scene is splash screen
		if (isSplash)
		{
			isSplash = false;
			splashUI.ChangeScene();
		}
	}

	public void ShowLeaderboard(string leaderboard)
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to show leaderboard");
		#endif

		#if UNITY_ANDROID
		if (!androidManager.PlayServicesConnected)
		{
			androidManager.NeedLeaderboard = true;
			InitPlayServices();
		}
		else androidManager.ShowLeaderboard(leaderboard);
		#endif
	}

	public void UploadLeaderboard(string leaderboard, float score)
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to upload score: " + score.ToString("00") + " to leaderboard id: " + leaderboard);
		#endif

		#if UNITY_ANDROID
		androidManager.UploadLeaderboard(leaderboard, score);
		#endif
	}

	public void ShowAchievements()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to show achievements interface");
		#endif

		#if UNITY_ANDROID
		androidManager.ShowAchievements();
		#endif
	}

	public void ResetAchievements()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to reset achievements");
		#endif

		#if UNITY_ANDROID
		androidManager.ResetAchievements();
		#endif
	}

	public void UploadAchievement(string achievement)
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to submit achievement id: " + achievement);
		#endif

		#if UNITY_ANDROID
		androidManager.UploadAchievement(achievement);
		#endif
	}
	#endregion

	#region Billing Methods
	public void PurchaseProduct(string product)
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to purchase product id: " + product);
		#endif

		#if UNITY_ANDROID
		androidManager.PurchaseProduct(product);
		#endif
	}

	public void RestorePurchases()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to restore purchases");
		#endif

		#if UNITY_ANDROID
		androidManager.RestorePurchases();
		#endif
	}
	#endregion

	#region Share Methods
	public void ShareScreenshot(string text)
	{
		ShareScreenshot(text, null, null);
	}

	public void ShareScreenshot(string text, GameObject[] disable, GameObject[] enable)
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to share screenshot");
		#endif

		#if UNITY_ANDROID
		androidManager.ShareScreenshot(text, disable, enable);
		#endif
	}
	#endregion

	#region Event Methods
	public void ShowPreloader()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to show preloader");
		#endif

		#if UNITY_ANDROID
		androidManager.ShowPreloader();
		#endif
	}

	public void HidePreloader()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to hide preloader");
		#endif

		#if UNITY_ANDROID
		androidManager.HidePreloader();
		#endif
	}

	public void ShowPopUp(string title, string message)
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to show a pop up");
		#endif

		#if UNITY_ANDROID
		androidManager.ShowPopUp(title, message);
		#endif
	}

	public void ShowRatePopUp(string title, string message)
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to open rate pop-up dialog");
		#endif

		#if UNITY_ANDROID
		androidManager.ShowRatePopUp(title, message);
		#endif
	}
	#endregion

	#region Advertising Methods
	public void ShowBanner()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to show banner advertisement");
		#endif

		#if UNITY_ANDROID
		androidManager.ShowBanner();
		#endif
	}

	public void HideBanner()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to hide banner advertisement");
		#endif

		#if UNITY_ANDROID
		androidManager.HideBanner();
		#endif
	}

	public void LoadIntersticial()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to load intersticial advertisement");
		#endif

		#if UNITY_ANDROID
		androidManager.LoadIntersticial();
		#endif
	}

	public void ShowIntersticial()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to show intersticial advertisement");
		#endif

		#if UNITY_ANDROID
		androidManager.ShowIntersticial();
		#endif
	}

	public void LoadRewardedVideo()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to load rewarded video advertisement");
		#endif

		#if UNITY_ANDROID
		androidManager.LoadRewardedVideo();
		#endif
	}

	public void ShowRewardedVideo()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: attempting to show rewarded video advertisement");
		#endif

		#if UNITY_ANDROID
		androidManager.ShowRewardedVideo();
		#endif
	}

	public void OnVideoStarted()
	{
		#if DEBUG_INFO
		Debug.Log("GameManager: rewarded video advertisement started");
		#endif

		#if UNITY_ANDROID
		HidePreloader();
		GameObject gameplay = GameObject.FindWithTag("GameController");
		if (gameplay) gameplay.GetComponent<GameplayManager>().DisableWatchAndWin();
		#endif
	}
	#endregion
	#endif

	#region Properties
	public static GameManager Instance
	{
		get 
		{
			if(!instance) 
			{
				instance = (GameManager)FindObjectOfType(typeof(GameManager));
				
				if(!instance)
				{
					instance = (new GameObject("GameManager")).AddComponent<GameManager>();

					#if DEBUG_INFO
					instance.gameObject.AddComponent<DebugConsole>();
					#endif

					if (!instance.initialized) instance.Initialize();
					DontDestroyOnLoad (instance.gameObject);
				}
			}

			return instance;
		}
	}

	public bool Initialized
	{
		get { return initialized; }
	}

	public int AlreadyPlayed
	{
		get { return alreadyPlayed; }
		set { alreadyPlayed = value; }
	}

	public int Coins
	{
		get { return coins; }
		set { coins = value; }
	}

	public int BestScore
	{
		get { return bestScore; }
		set { bestScore = value; }
	}

	public int Volume
	{
		get { return volume; }
		set { volume = value; }
	}

	public int[] Ships
	{
		get { return ships; }
		set { ships = value; }
	}

	public int CurrentShip
	{
		get { return currentShip; }
		set { currentShip = value; }
	}

	public int NoAds
	{
		get { return noAds; }
		set { noAds = value; } 
	}

	public int TotalTimes
	{
		get { return totalTimes; }
		set { totalTimes = value; }
	}

	public bool ShowingAd
	{
		get 
		{
			#if !UNITY_EDITOR
			return androidManager.ShowingAd;
			#else
			return true;
			#endif
		}
	}

	#if !UNITY_EDITOR
	public bool IsSplash
	{
		get { return isSplash; }
		set { isSplash = value; }
	}

	public bool GoodAchievement
	{
		get { return goodAchievement; }
		set { goodAchievement = value; }
	}

	public bool IncreibleAchievement
	{
		get { return increibleAchievement; }
		set { increibleAchievement = value; }
	}

	public bool AwesomeAchievement
	{
		get { return awesomeAchievement; }
		set { awesomeAchievement = value; }
	}

	public bool BossAchievement
	{
		get { return bossAchievement; }
		set { bossAchievement = value; }
	}

	public bool BuyShipAchievement
	{
		get { return buyShipAchievement; }
		set { buyShipAchievement = value; }
	}

	public bool Connected
	{
		get { return androidManager.PlayServicesConnected; }
	}
	#endif
	#endregion
}
