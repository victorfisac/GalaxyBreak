#if !UNITY_EDITOR
using UnityEngine;
using System.Collections;

public class AndroidManager : MonoBehaviour 
{
	#region Static Members
	private static AndroidManager instance = null;
	#endregion

	#region Private Members
	private bool initialized;						// Is singleton instance initialized
	#if UNITY_ANDROID
	private bool needLeaderboard;					// Open leaderboard after authentication state
	GoogleMobileAdBanner adBanner;					// Advertising banner reference
	#endif
	#endregion

	#region Initialization Methods
	private void Initialize()
	{
		InitBilling();
		InitAdvertising();

		// Update instance initialized state
		initialized = true;
		
		#if DEBUG_INFO
		Debug.Log("AndroidManager: Android manager initialized successfully");
		#endif
	}

	public void InitPlayServices()
	{
		#if UNITY_ANDROID
		GooglePlayConnection.ActionConnectionResultReceived += ActionConnectionResultReceived;
		GooglePlayConnection.Instance.Connect();
		#endif

		#if DEBUG_INFO
		Debug.Log("AndroidManager: Google Play Services initialized successfully");
		#endif
	}

	private void InitBilling()
	{
		#if UNITY_ANDROID
		// Listen for store initialising finish
		AndroidInAppPurchaseManager.ActionBillingSetupFinished += OnBillingConnected;
		AndroidInAppPurchaseManager.ActionProductPurchased += OnProductPurchased;
		AndroidInAppPurchaseManager.ActionProductConsumed += OnProductConsumed;

		// Connect to Google Play Store Purchasing
		AndroidInAppPurchaseManager.Client.Connect();
		#endif

		#if DEBUG_INFO
		Debug.Log("AndroidManager: Google Play billing initialized successfully");
		#endif
	}

	private void InitAdvertising()
	{
		#if UNITY_ANDROID

		// Initialize Admob client and set advertisements ids
		AndroidAdMob.Client.Init(ProjectManager.bannerId);
		AndroidAdMob.Client.SetInterstisialsUnitID(ProjectManager.intersticialId);
		for (int i = 0; i < ProjectManager.adKeywords.Length; i++) AndroidAdMob.Client.AddKeyword(ProjectManager.adKeywords[i]);
		AndroidAdMob.Client.TagForChildDirectedTreatment(false);
		#if VIDEO_ADMOB
		AndroidAdMob.Client.SetRewardedVideoAdUnitID(ProjectManager.rewardedId);
		AndroidAdMob.Client.OnRewardedVideoAdClosed += OnRewardedVideoAdClosed;
		#elif VIDEO_ADCOLONY
		// Initialize AdColony client and advertisements ids
		AdColony.OnVideoStarted += OnVideoStarted;
		AdColony.OnV4VCResult = OnV4VCResult;
		AdColony.Configure(ProjectManager.adVersion, ProjectManager.adAppId, ProjectManager.adZoneId);
		#endif
		#endif

		#if DEBUG_INFO
		Debug.Log("AndroidManager: advertising initialized successfully");
		#endif
	}
	#endregion

	#region Play Services Methods
	public void ShowLeaderboard(string leaderboard)
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager attempting to show Google Play Games leaderboards interface");
		#endif

		#if UNITY_ANDROID
		GooglePlayManager.Instance.ShowLeaderBoardById(leaderboard);
		#endif
	}

	public void UploadLeaderboard(string leaderboard, float score)
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to upload score: " + score.ToString("00") + " to leaderboard id: " + leaderboard);
		#endif

		#if UNITY_ANDROID
		GooglePlayManager.ActionScoreSubmited += OnScoreSubmited;
		GooglePlayManager.Instance.SubmitScoreById(leaderboard, (long)score);
		#endif
	}

	public void ShowAchievements()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to show Google Play Games achievements interface");
		#endif

		#if UNITY_ANDROID
		GooglePlayManager.Instance.ShowAchievementsUI();
		#endif
	}

	public void ResetAchievements()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to reset Google Play Games achievements");
		#endif

		#if UNITY_ANDROID
		GooglePlayManager.Instance.ResetAllAchievements();
		#endif
	}

	public void UploadAchievement(string achievement)
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to upload achievement id: " + achievement);
		#endif

		#if UNITY_ANDROID
		GooglePlayManager.Instance.UnlockAchievementById(achievement);
		#endif
	}
	#region Play Services Internal Methods
	#if UNITY_ANDROID
	private void ActionConnectionResultReceived(GooglePlayConnectionResult result) 
	{
		#if UNITY_ANDROID
		GooglePlayConnection.ActionConnectionResultReceived -= ActionConnectionResultReceived;

		// Check for locked achievements with all progress completed
		GameManager.Instance.CheckAchievements();

		if (result.IsSuccess)
		{
			if (needLeaderboard)
			{
				needLeaderboard = false;
				GooglePlayManager.Instance.ShowLeaderBoardById(ProjectManager.leaderboardId);
			}
		}
		else AndroidMessage.Create(ProjectManager.playServicesTitle, ProjectManager.playServicesError);
		#endif

		#if DEBUG_INFO
		if(result.IsSuccess) Debug.Log("AndroidManager: connected to Google Play Services successfully");
		else Debug.Log("AndroidManager: failed to connect to Google Play Services with error: " + result.code.ToString());
		#endif
	}

	private void OnScoreSubmited(GP_LeaderboardResult result) 
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: score submited for leaderboard " + result.Leaderboard.Id + " with result:" + result.Message);
		#endif

		#if UNITY_ANDROID
		GooglePlayManager.ActionScoreSubmited -= OnScoreSubmited;
		#endif
	}
	#endif
	#endregion
	#endregion

	#region Billing Methods
	public void PurchaseProduct(string product)
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to buy product id: " + product);
		#endif

		switch (product)
		{
			case "no_ads":
			{
				#if UNITY_ANDROID
				if (AndroidInAppPurchaseManager.Client.Inventory.IsProductPurchased(product)) AndroidInAppPurchaseManager.Client.Consume(product);
				else AndroidInAppPurchaseManager.Client.Purchase(product);
				#endif
			} break;
			default: AndroidInAppPurchaseManager.Client.Purchase(product); break;
		}
	}

	public void RestorePurchases()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to restore purchases");
		#endif

		#if UNITY_ANDROID
		if (AndroidInAppPurchaseManager.Client.Inventory.IsProductPurchased("no_ads")) AndroidInAppPurchaseManager.Client.Consume("no_ads");
		#endif
	}

	#region Billing Internal Methods
	#if UNITY_ANDROID
	private void OnBillingConnected(BillingResult result) 
	{
		AndroidInAppPurchaseManager.ActionBillingSetupFinished -= OnBillingConnected;

		if(result.IsSuccess)
		{
			#if DEBUG_INFO
			Debug.Log("AndroidManager: billing module connected successfully");
			#endif

			AndroidInAppPurchaseManager.Client.RetrieveProducDetails();
		}
		#if DEBUG_INFO
		else Debug.Log("AndroidManager: billing module connection failed");
		#endif
	}

	private void OnProductPurchased(BillingResult result)
	{
		if (result.IsSuccess)
		{
			AndroidMessage.Create(ProjectManager.billingTitle, ProjectManager.billingComplete);
			OnProcessingPurchasedProduct(result.Purchase);
		}
		else AndroidMessage.Create(ProjectManager.billingTitle, ProjectManager.billingError);
	}

	private void OnProcessingPurchasedProduct(GooglePurchaseTemplate purchase)
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: in app purchase id: " + purchase.SKU + " processed successfully");
		#endif

		AndroidInAppPurchaseManager.Client.Consume(purchase.SKU);
	}

	private void OnProductConsumed(BillingResult result)
	{
		if (result.IsSuccess)
		{
			AndroidMessage.Create(ProjectManager.billingTitle, ProjectManager.billingComplete);
			OnProcessingConsumeProduct(result.Purchase);
		}
		else AndroidMessage.Create(ProjectManager.billingTitle, ProjectManager.billingError);
	}

	private void OnProcessingConsumeProduct(GooglePurchaseTemplate purchase)
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: in app consumition id: " + purchase.SKU + " processed successfully");
		#endif

		switch (purchase.SKU)
		{
			case "coins_pack_1": GameManager.Instance.Coins += ProjectManager.coinsPackCoins[0]; break;
			case "coins_pack_2": GameManager.Instance.Coins += ProjectManager.coinsPackCoins[1]; break;
			case "coins_pack_3": GameManager.Instance.Coins += ProjectManager.coinsPackCoins[2]; break;
			case "no_ads": GameManager.Instance.NoAds = 1; break;
			default: break;
		}

		GameManager.Instance.SaveData();
	}
	#endif
	#endregion
	#endregion

	#region Share
	public void ShareScreenshot(string text, GameObject[] disableObjects, GameObject[] enableObjects)
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to share a screenshot by native method");
		#endif

		#if UNITY_ANDROID
		StartCoroutine(CaptureScreenshot(text, disableObjects, enableObjects));
		#endif
	}

	#region Share Internal
	#if UNITY_ANDROID
	private IEnumerator CaptureScreenshot(string text, GameObject[] disableObjects, GameObject[] enableObjects) 
	{
		if (disableObjects != null)
		{
			for(int i = 0; i < disableObjects.Length; i++) disableObjects[i].SetActive(false);
		}

		if (enableObjects != null)
		{
			for(int i = 0; i < enableObjects.Length; i++) enableObjects[i].SetActive(true);
		}

		yield return new WaitForEndOfFrame();

		// Create a texture the size of the screen, RGB24 format
		int width = Screen.width;
		int height = Screen.height;
		Texture2D tex = new Texture2D( width, height, TextureFormat.RGB24, false );

		// Read screen contents into the texture
		tex.ReadPixels( new Rect(0, 0, width, height), 0, 0 );

		if (enableObjects != null)
		{
			for(int i = 0; i < enableObjects.Length; i++) enableObjects[i].SetActive(false);
		}

		if (disableObjects != null)
		{
			for(int i = 0; i < disableObjects.Length; i++) disableObjects[i].SetActive(true);
		}

		tex.Apply();

		AndroidSocialGate.StartShareIntent("Share", text, tex);
		
		Destroy(tex);
	}
	#endif
	#endregion
	#endregion

	#region Event Methods
	public void ShowPreloader()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to show preloader");
		#endif

		#if UNITY_ANDROID
		AndroidNativeUtility.ShowPreloader("Loading", "Please wait...");
		#endif
	}

	public void HidePreloader()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to hide preloader");
		#endif

		#if UNITY_ANDROID
		AndroidNativeUtility.HidePreloader();
		#endif
	}

	public void ShowPopUp(string title, string message)
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to show a pop up");
		#endif

		#if UNITY_ANDROID
		AndroidMessage.Create(title, message);
		#endif
	}

	public void ShowRatePopUp(string title, string message)
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to show rate pop up");
		#endif

		#if UNITY_ANDROID
		AndroidRateUsPopUp rate = AndroidRateUsPopUp.Create(title, message, "market://details?id=" + ProjectManager.bundleId);
		rate.ActionComplete += OnRatePopUpClosed;
		#endif
	}

	#region Event Internal Methods
	private void OnRatePopUpClosed(AndroidDialogResult result)
	{
		switch (result)
		{
			case AndroidDialogResult.REMIND:
			{
				GameManager.Instance.TotalTimes = 0;
				GameManager.Instance.SaveData();
			} break;
			default: break;
		}
	}
	#endregion
	#endregion

	#region Advertising Methods
	public void ShowBanner()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to show banner advertisement");
		#endif

		#if UNITY_ANDROID
		if (adBanner == null)
		{
			adBanner = AndroidAdMob.Client.CreateAdBanner(TextAnchor.LowerCenter, GADBannerSize.SMART_BANNER);
			adBanner.ShowOnLoad = false;
		}
		else if (adBanner.IsLoaded) adBanner.Show();
		#endif
	}

	public void HideBanner()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to hide banner advertisement");
		#endif

		#if UNITY_ANDROID
		if (adBanner != null) adBanner.Hide();
		#endif
	}

	public void LoadIntersticial()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to load intersticial advertisement");
		#endif

		#if UNITY_ANDROID
		AndroidAdMob.Client.LoadInterstitialAd();
		#endif
	}

	public void ShowIntersticial()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to show intersticial advertisement");
		#endif

		#if UNITY_ANDROID
		AndroidAdMob.Client.ShowInterstitialAd();
		#endif
	}

	public void LoadRewardedVideo()
	{
		#if VIDEO_ADMOB
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to load rewarded video advertisement");
		#endif

		#if UNITY_ANDROID
		AndroidAdMob.Client.LoadRewardedVideo();
		#endif
		#endif
	}

	public void ShowRewardedVideo()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: attempting to show rewarded video advertisement");
		#endif

		#if UNITY_ANDROID
		#if VIDEO_ADMOB
		AndroidAdMob.Client.ShowRewardedVideo();
		#elif VIDEO_ADCOLONY
		if (AdColony.IsV4VCAvailable(ProjectManager.adZoneId)) AdColony.ShowV4VC(false, ProjectManager.adZoneId);
		else ShowPopUp(ProjectManager.rewardTitle, ProjectManager.rewardMessageError);

		GameManager.Instance.OnVideoStarted();
		#endif
		#endif
	}

	#region Advertising Internal Methods
	#if UNITY_ANDROID
	#if VIDEO_ADMOB
	private void OnRewardedVideoAdClosed()
	{
		#if DEBUG_INFO
		Debug.Log("AndroidManager: video advertisement showed and closed");
		#endif

		GameManager.Instance.Coins += ProjectManager.rewardCoins;
		GameManager.Instance.SaveData();
	}
	#elif VIDEO_ADCOLONY
	private void OnVideoStarted()
	{
		GameManager.Instance.OnVideoStarted();
	}

	private void OnV4VCResult(bool success, string name, int amount)
	{
		if (success) 
		{
			#if DEBUG_INFO
			Debug.Log("AndroidManager: video advertisement showed and closed");
			#endif

			GameManager.Instance.HidePreloader();
			GameManager.Instance.Coins += ProjectManager.rewardCoins;
			GameManager.Instance.SaveData();

			ShowPopUp(ProjectManager.rewardTitle, ProjectManager.rewardMessageSuccess);
		}
	}
	#endif
	#endif
	#endregion
	#endregion

	#region Properties
	public static AndroidManager Instance
	{
		get 
		{
			if(!instance) 
			{
				instance = (AndroidManager)FindObjectOfType(typeof(AndroidManager));
				
				if(!instance)
				{
					instance = (new GameObject("AndroidManager")).AddComponent<AndroidManager>();
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

	public bool PlayServicesConnected
	{
		get { return GooglePlayConnection.Instance.IsConnected; }
	}

	public bool NeedLeaderboard
	{
		get { return needLeaderboard; }
		set { needLeaderboard = value; }
	}

	public bool ShowingAd
	{
		get
		{
			bool result = false;
			if (adBanner != null) result = (adBanner.IsOnScreen && adBanner.IsLoaded);
			return result;
		}
	}
	#endregion
}
#endif