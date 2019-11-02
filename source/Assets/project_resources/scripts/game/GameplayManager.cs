using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using System.Collections;

public class GameplayManager : MonoBehaviour 
{
	#region Enums
	public enum GameplayStates { MENU, GAME, EXIT };
	public enum DynamicElementType { SCORE, COIN, BONUS, EXTRA };
	#endregion

	#region Inspector Members
	[Header("Menu")]
	[Tooltip("Menu canvas group for gameplay transition")]
	[SerializeField] private CanvasGroup menuGroup;

	[Tooltip("Game coins text displayed in menu interface")]
	[SerializeField] private Text coinsText;

	[Tooltip("Best score text displayed in menu interface")]
	[SerializeField] private Text bestScoreText;

	[Tooltip("Menu buttons disabled when gameplay starts")]
	[SerializeField] private Button[] menuButtons;

	[Tooltip("Starting game invisible button disabled when gameplay starts")]
	[SerializeField] private Button startPlayingButton;

	[Tooltip("Watch and win reward amount text")]
	[SerializeField] private Text watchAndWinText;

	[Header("Gameplay")]
	[Tooltip("Gameplay canvas group for transition")]
	[SerializeField] private CanvasGroup gameplayGroup;

	[Tooltip("Game coins text displayed in gameplay interface")]
	[SerializeField] private Text gameplayCoinsText;

	[Tooltip("Best score text displayed in gameplay interface")]
	[SerializeField] private Text gameplayBestText;

	[Tooltip("Gameplay buttons disabled when gameplay finishes")]
	[SerializeField] private Button[] gameplayButtons;

	[Tooltip("Back to menu after game finish delay time")]
	[SerializeField] private float backDelay;

	[Tooltip("New record interface scale reference")]
	[SerializeField] private ScaleObject recordScale;

	[Tooltip("New record interface alpha reference")]
	[SerializeField] private AlphaObject recordAlpha;

	[Header("Habilities")]
	[Tooltip("Gameplay habilities buttons game objects root transform")]
	[SerializeField] private Transform skillButtonsRoot;

	[Tooltip("Gameplay habilities messages rectangle transform")]
	[SerializeField] private RectTransform gameplayMessages;

	[Tooltip("Habilities messages scale and alpha references")]
	[SerializeField] private Hability[] habilities;

	[Header("Tutorial")]
	[Tooltip("Tutorial interface game object")]
	[SerializeField] private GameObject tutorialObject;

	[Header("Times Logic")]
	[Tooltip("How much times to show rate app dialog")]
	[SerializeField] private int timesForRate;

	[Tooltip("How much times to show an intersticial ad")]
	[SerializeField] private int timesForIntersticial;

	[Tooltip("How much times the watch and button is displayed with random factor")]
	[SerializeField] private int timesForWatch;

	[Header("Effects")]
	[Tooltip("Background bubbles particle system disabled when gameplay starts")]
	[SerializeField] private ParticleSystem bubbleParticles;

	[Header("Bonus")]
	[Tooltip("Bonus animation curve for background material")]
	[SerializeField] private AnimationCurve bonusCurve;

	[Tooltip("Bonus animation duration for background material")]
	[SerializeField] private float bonusDuration;

	[Tooltip("Bonus animation final color for background material")]
	[SerializeField] private Color bonusColor;

	[Header("Extra")]
	[Tooltip("Extra animation duration for background material")]
	[SerializeField] private float extraDuration;

	[Tooltip("Extra animation final color for background material")]
	[SerializeField] private Color extraColor;

	[Header("Pause")]
	[Tooltip("Button used to enter in pause state")]
	[SerializeField] private Button pauseButton;

	[Tooltip("Pause interface game object")]
	[SerializeField] private GameObject pauseObject;

	[Header("Audio")]
	[Tooltip("Audio source played when pressing any interface button")]
	[SerializeField] private AudioSource tapSource;

	[Tooltip("Audio source played when game starts")]
	[SerializeField] private AudioSource startSource;

	[Tooltip("Audio source played when missile explodes")]
	[SerializeField] private AudioSource scoreSource;

	[Tooltip("Audio source played when new record or habilities are used")]
	[SerializeField] private AudioSource recordSource;

	[Tooltip("Audio source played when coin is picked")]
	[SerializeField] private AudioSource coinSource;

	[Tooltip("Audio source played when bonus coin is picked")]
	[SerializeField] private AudioSource bonusSource;

	[Tooltip("Audio source played when extra coin is picked")]
	[SerializeField] private AudioSource extraSource;

	[Tooltip("Audio source played when game finishes")]
	[SerializeField] private AudioSource finishSource;

	[Header("References")]
	[Tooltip("Menu switch animation controller reference")]
	[SerializeField] private MenuController menuController;

	[Tooltip("Gameplay controller reference")]
	[SerializeField] private GameplayController gameplayController;

	[Tooltip("Background material reference for exit effect")]
	[SerializeField] private Material backgroundMat;

	[Tooltip("Trail material reference for exit effect")]
	[SerializeField] private Material trailMat;

	[Tooltip("Dynamic elements pool manager reference")]
	[SerializeField] private DynamicElementsPool dynamicElementsPool;

	[Tooltip("Add coin interface effect prefab")]
	[SerializeField] private GameObject addCoinEffect;

	[Tooltip("Add bonus interface effect prefab")]
	[SerializeField] private GameObject addBonusEffect;

	[Tooltip("Add extra interface effect prefab")]
	[SerializeField] private GameObject addExtraEffect;

	[Tooltip("Add score interface effect prefab")]
	[SerializeField] private GameObject addScoreEffect;

	[Tooltip("Interface camera reference")]
	[SerializeField] private Camera interfaceCamera;

	[Tooltip("Interface canvas scaler reference")]
	[SerializeField] private CanvasScaler canvasScaler;

	[Tooltip("No Ads button canvas group for disabled effect")]
	[SerializeField] private CanvasGroup noAds;

	[Tooltip("Gameplay habilities buttons canvas group for enabled state effect")]
	[SerializeField] private CanvasGroup habilitiesGroup;

	[Tooltip("Volume sprite game object disabled when audio is muted")]
	[SerializeField] private GameObject volumeObject;

	[Tooltip("Watch and Win button game object disabled when pressed")]
	[SerializeField] private GameObject rewardButton;
	#endregion

	#region Private Members
	private GameplayStates state;			// Current game state (MENU, GAME)
	private GameManager gameManager;		// Game manager reference
	private int currentScore;				// Current gameplay score
	private int rateTimes;					// Current session rate time
	private int rewardedTimes;				// Current session played times to show rewarded video
	private int intersticialTimes;			// Current session played times to show intersticial video
	private Color initColorT;				// Background material top color on start
	private Color initColorB;				// Background material bottom color on start
	private float bonusCounter;				// Bonus background material animation time counter
	private float extraCounter;				// Extra background material animation time counter
	private bool isTop;						// Bonus background material animation current shader property
	private bool newRecord;					// Current gameplay new record displayed state
	private GameObject[] skillButtons;		// Gameplay skills interface buttons
	private bool bannerDown;				// Is banner displaying state
	private bool canHability;				// Can use habilities state
	#if UNITY_EDITOR
	private float ratioInit;				// Background material ratio on start
	#endif
	#endregion

	#region Main Methods
	private void Start()
	{
		#if DEBUG_INFO
		Debug.Log("GameplayManager: initializing gameplay manager");
		#endif

		// Get references
		skillButtons = new GameObject[skillButtonsRoot.childCount];
		for (int i = 0; i < skillButtons.Length; i++) skillButtons[i] = skillButtonsRoot.GetChild(i).gameObject;

		// Initialize values
		state = GameplayStates.MENU;
		gameManager = GameManager.Instance;
		currentScore = 0;
		rewardedTimes = 0;
		intersticialTimes = 0;
		initColorT = backgroundMat.GetColor("_TopColor");
		initColorB = backgroundMat.GetColor("_BottomColor");
		bonusCounter = 0f;
		extraCounter = 0f;
		isTop = false;
		newRecord = false;
		bannerDown = false;
		canHability = false;
		#if UNITY_EDITOR
		ratioInit = backgroundMat.GetFloat("_Ratio");
		#endif
		if (Application.targetFrameRate != 60) Application.targetFrameRate = 60;

		// Initialize gameplay manager
		InitMenuUI();
		InitGameplayUI();
		InitAds();

		// Initialize gameplay controller
		gameplayController.Initialize(this);
	}

	private void Update()
	{
		switch (state)
		{
			case GameplayStates.MENU:
			{
				// Fade out gameplay interface if gameplay finished recently
				if (gameplayGroup.alpha > 0f) gameplayGroup.alpha -= Time.deltaTime*2f;

				// Update menu interface information (coins and best score)
				coinsText.text = gameManager.Coins.ToString();
				bestScoreText.text = gameManager.BestScore.ToString();

				// Apply disabled effect to no advertisements button if purchased
				if ((noAds.alpha == 1f) && (gameManager.NoAds == 1))
				{
					noAds.alpha = 0.2f;
					noAds.GetComponent<Button>().interactable = false;
				}

				// Check Android physic buttons input
				if (Input.GetKeyDown(KeyCode.Escape))
				{
					#if !UNITY_EDITOR
					gameManager.HidePreloader();
					#endif

					switch (menuController.CurrentMenu)
					{
						case 0:
						{
							// Check if menu start animation is finished
							if (startPlayingButton.interactable)
							{
								startSource.Play();
								ExitGame();
							}
						} break;
						case 1:
						{
							// Go back from shop to default menu
							tapSource.Play();
							BackShop();
						} break;
						default: break;
					}
				}
			} break;
			case GameplayStates.GAME:
			{
				// Fade out menu interface if gameplay started recently
				if (menuGroup.alpha > 0f) menuGroup.alpha -= Time.deltaTime;

				// Fade in gameplay interface if gameplay started recently
				if (gameplayGroup.alpha < 1f) gameplayGroup.alpha += Time.deltaTime;

				// Check if banner should be loaded
				if (bannerDown)
				{
					// Check if banner is loaded and displaying in screen
					if (gameManager.ShowingAd)
					{
						if (gameplayMessages.anchoredPosition.y != 100f)
						{
							skillButtonsRoot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 100f);
							gameplayMessages.anchoredPosition = new Vector2(0f, 100f);
						}
					}
					else if (gameplayMessages.anchoredPosition.y != 0f)
					{
						skillButtonsRoot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
						gameplayMessages.anchoredPosition = Vector2.zero;
					}
				}
				else if (gameplayMessages.anchoredPosition.y != 0f)
				{
					skillButtonsRoot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
					gameplayMessages.anchoredPosition = Vector2.zero;
				}

				// Update gameplay interface information (coins and best score)
				gameplayCoinsText.text = gameManager.Coins.ToString();
				gameplayBestText.text = currentScore.ToString();

				if (bonusCounter > 0f)
				{
					if (isTop) backgroundMat.SetColor("_TopColor", Color.Lerp(initColorT, Color.white, bonusCurve.Evaluate((bonusDuration - bonusCounter)/bonusDuration)));
					else backgroundMat.SetColor("_BottomColor", Color.Lerp(initColorB, bonusColor, bonusCurve.Evaluate((bonusDuration - bonusCounter)/bonusDuration)));

					// Update bonus animation time counter
					bonusCounter -= Time.deltaTime;
				}

				if (extraCounter > 0f)
				{
					if (isTop) backgroundMat.SetColor("_TopColor", Color.Lerp(initColorT, Color.white, bonusCurve.Evaluate((extraDuration - extraCounter)/extraDuration)));
					else backgroundMat.SetColor("_BottomColor", Color.Lerp(initColorB, extraColor, bonusCurve.Evaluate((extraDuration - extraCounter)/extraDuration)));

					// Update extra animation time counter
					extraCounter -= Time.deltaTime;
				}

				if (!canHability && habilitiesGroup.alpha > 0.2f) habilitiesGroup.alpha -= Time.deltaTime*4f;
				else if (canHability && habilitiesGroup.alpha < 1f) habilitiesGroup.alpha += Time.deltaTime*4f;
			} break;
			case GameplayStates.EXIT:
			{
				// Fade out menu interface if gameplay started recently
				if (menuGroup.alpha > 0f) menuGroup.alpha -= Time.deltaTime*2f;

				if (AudioListener.volume > 0f) AudioListener.volume -= Time.deltaTime*2f;

				// Update background material gradient ratio
				float ratio = backgroundMat.GetFloat("_Ratio");
				if (ratio < 1f) backgroundMat.SetFloat("_Ratio", ratio + Time.deltaTime*2f);
			} break;
			default: break;
		}
	}
	#endregion

	#region Initialization Methods
	private void InitMenuUI()
	{
		// Disable volume sprite if game is muted
		if (gameManager.Volume == 0) volumeObject.SetActive(false);

		// Enable menu buttons and disable gameplay buttons
		for (int i = 0; i < menuButtons.Length; i++) menuButtons[i].interactable = true;

		// Update watch and win reward text
		#if !UNITY_EDITOR
		watchAndWinText.text = ProjectManager.rewardCoins.ToString();
		#else
		watchAndWinText.text = "0";
		#endif

		// Disable start playing button during menu animation
		startPlayingButton.interactable = false;

		// Enable start playing invisible button after menu start animation
		Invoke("EnableStartPlaying", 2f);
	}

	private void InitGameplayUI()
	{
		// Initialize gameplay interface buttons
		for (int i = 0; i < gameplayButtons.Length; i++) gameplayButtons[i].interactable = false;

		// Initialize gameplay interface alpha
		if (gameplayGroup.gameObject.activeSelf) gameplayGroup.gameObject.SetActive(false);
		gameplayGroup.alpha = 0f;

		// Initialize pause menu and button
		pauseButton.interactable = false;
		pauseButton.gameObject.SetActive(true);
		pauseObject.SetActive(false);
	}

	private void InitAds()
	{
		#if !UNITY_EDITOR
		// Call show banner first time to load ad but without displaying it
		gameManager.ShowBanner();
		#endif

		// Enable watch and win button for each five played times with a random factor
		int random = Random.Range(0, 2);
		if (random == 1)
		{
			rewardButton.SetActive(true);

			#if !UNITY_EDITOR
			// Load rewarded video to avoid waiting when pressing button
			gameManager.LoadRewardedVideo();
			#endif
		}
	}
	#endregion

	#region Menu Methods
	public void StartGame()
	{
		if (state == GameplayStates.MENU)
		{
			#if DEBUG_INFO
			Debug.Log("GameplayManager: starting new game");
			#endif

			// Update gameplay manager values
			state = GameplayStates.GAME;
			currentScore = 0;
			intersticialTimes++;
			newRecord = false;
			gameplayGroup.gameObject.SetActive(true);
			bubbleParticles.Stop();
			bonusCounter = 0f;
			extraCounter = 0f;
			canHability = false;

			// Disable all menu buttons to avoid game flow bugs
			for (int i = 0; i < menuButtons.Length; i++) menuButtons[i].interactable = false;

			// Check if it is the first player gameplay
			if (gameManager.AlreadyPlayed == 0)
			{
				// Enable tutorial interface game object
				tutorialObject.SetActive(true);

				// Disable tutorial interface after animation delay
				Invoke("DisableTutorial", 5f);

				// Update already played state and save data
				gameManager.AlreadyPlayed = 1;
				gameManager.SaveData();

				// Update banner down state
				bannerDown = false;
			}
			else bannerDown = (gameManager.NoAds == 0);

			// Initialize gameplay skill buttons interface
			for (int i = 0; i < skillButtons.Length; i++) skillButtons[i].SetActive(false);
			skillButtons[gameManager.CurrentShip].SetActive(true);

			// Load intersticial advertisement for each amount of played times
			if (intersticialTimes >= timesForIntersticial && gameManager.NoAds == 0)
			{
				#if !UNITY_EDITOR
				// Load intersticial advertisement
				gameManager.LoadIntersticial();
				#endif

				#if DEBUG_INFO
				Debug.Log("GameplayManager: attempting to load intersticial advertisement");
				#endif
			}

			// Play start game audio source
			startSource.Play();

			// Update gameplay controller state
			gameplayController.SetGame();

			// Disable menu game object after transition
			Invoke("DisableMenu", 1.1f);
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call StartGame() when state is not expected");
		#endif
	}

	public void BuyNoAds()
	{
		if (state == GameplayStates.MENU)
		{
			#if DEBUG_INFO
			Debug.Log("GameplayManager: attempting to buy Android no advertisements product");
			#endif

			#if !UNITY_EDITOR
			gameManager.PurchaseProduct(ProjectManager.noAdsId);
			#endif
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call BuyNoAds() when state is not expected");
		#endif
	}

	public void ShareGame()
	{
		if (state == GameplayStates.MENU)
		{
			#if DEBUG_INFO
			Debug.Log("GameplayManager: attempting to open Android native share menu");
			#endif

			#if !UNITY_EDITOR
			gameManager.ShowPreloader();
			gameManager.ShareScreenshot(ProjectManager.shareMessage);

			Invoke("DisablePreloader", 3f);
			#endif
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call ShareGame() when state is not expected");
		#endif
	}

	public void SwitchVolume()
	{
		if (state == GameplayStates.MENU)
		{
			#if DEBUG_INFO
			Debug.Log("GameplayManager: switched game volume to: " + (gameManager.Volume == 0).ToString());
			#endif

			// Switch game manager volume and save new data to disk
			gameManager.Volume = ((gameManager.Volume == 0) ? 1 : 0);
			gameManager.SaveData();

			// Update menu interface volume
			volumeObject.SetActive((gameManager.Volume == 1));

			// Apply volume state to audio listener
			AudioListener.volume = gameManager.Volume;
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call SwitchVolume() when state is not expected");
		#endif
	}

	public void Leaderboard()
	{
		if (state == GameplayStates.MENU)
		{
			#if DEBUG_INFO
			Debug.Log("GameplayManager: attempting to open Android leaderboard");
			#endif

			#if !UNITY_EDITOR
			gameManager.ShowLeaderboard(ProjectManager.leaderboardId);
			#endif
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call Leaderboard() when state is not expected");
		#endif
	}

	public void Shop()
	{
		if (state == GameplayStates.MENU)
		{
			#if DEBUG_INFO
			Debug.Log("GameplayManager: switching to shop menu");
			#endif

			// Start menu switch to shop animation
			menuController.SetShop();

			// Disable starting gameplay invisible button to avoid game flow bugs
			startPlayingButton.interactable = false;
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call Shop() when state is not expected");
		#endif
	}

	public void BackShop()
	{
		if (state == GameplayStates.MENU)
		{
			#if DEBUG_INFO
			Debug.Log("GameplayManager: switching to main menu");
			#endif

			// Start menu switch to menu animation
			menuController.SetMenu();

			// Enable starting gameplay invisible button after transition to avoid game flow bugs
			Invoke("EnableStartPlaying", 1f);
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call BackShop() when state is not expected");
		#endif
	}

	public void WatchAndWin()
	{
		if (state == GameplayStates.MENU)
		{
			#if DEBUG_INFO
			Debug.Log("GameplayManager: attempting to show rewarded video advertisement");
			#endif

			#if !UNITY_EDITOR
			gameManager.ShowPreloader();
			gameManager.ShowRewardedVideo();
			#endif
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call WatchAndWin() when state is not expected");
		#endif
	}

	public void Pause()
	{
		// Check if game is not paused yet
		if (Time.timeScale != 0f)
		{
			// Update time scale
			Time.timeScale = 0f;

			// Play tap audio source
			tapSource.Play();

			// Disable pause button and enable pause menu
			pauseButton.gameObject.SetActive(false);
			pauseObject.SetActive(true);
		}
	}

	public void Continue()
	{
		// Check if game is not paused yet
		if (Time.timeScale != 1f)
		{
			// Update time scale
			Time.timeScale = 1f;

			// Play tap audio source
			tapSource.Play();

			// Disable pause menu and enable pause button
			pauseButton.gameObject.SetActive(true);
			pauseObject.SetActive(false);
		}
	}

	#region Menu Internal Methods
	private void EnableStartPlaying()
	{
		// Reset start playing button interactable state
		startPlayingButton.interactable = true;
	}

	private void DisableMenu()
	{
		// Disable menu game objects and enable gameplay interactable buttons
		menuGroup.gameObject.SetActive(false);
		menuGroup.alpha = 0f;
		rewardButton.SetActive(false);
		for (int i = 0; i < gameplayButtons.Length; i++) gameplayButtons[i].interactable = true;

		#if !UNITY_EDITOR
		if (bannerDown)
		{
			// Show banner advertisement
			gameManager.ShowBanner();

			#if DEBUG_INFO
			Debug.Log("GameplayManager: attempting to show banner advertisement");
			#endif
		}
		#endif

		// Update pause button and menu
		pauseButton.interactable = true;
	}

	public void DisableWatchAndWin()
	{
		// Disable reward button to avoid repeat its logic per gameplay
		rewardButton.SetActive(false);
	}

	private void ExitGame()
	{
		#if DEBUG_INFO
		Debug.Log("GameplayManager: attempting to exit game");
		#endif

		// Update game state to exit
		state = GameplayStates.EXIT;

		// Update current gameplay controller state
		gameplayController.ExitGame();

		// Disable all menu buttons to avoid game flow bugs
		for (int i = 0; i < menuButtons.Length; i++) menuButtons[i].interactable = false;

		// Quit app after tap sound and menu fade out finishes
		Invoke("QuitApp", 1f);
	}

	private void QuitApp()
	{
		#if !UNITY_EDITOR
		Application.Quit();
		#else
		EditorApplication.isPlaying = false;
		#endif
	}

	#if !UNITY_EDITOR
	private void DisablePreloader()
	{
		gameManager.HidePreloader();
	}
	#endif
	#endregion
	#endregion

	#region Gameplay Methods
	public void AddScore(int amount, Vector3 position, bool playAlways)
	{
		if (state == GameplayStates.GAME)
		{
			// Update current score amount
			currentScore += amount;

			// Check if player have beaten its best score
			if (currentScore > gameManager.BestScore && gameManager.BestScore > 0 && !newRecord)
			{
				// Enable new record interface game object
				recordScale.Play();
				recordAlpha.Play();
				recordSource.Play();

				if (bonusCounter <= 0f && extraCounter <= 0f)
				{
					// Start new record background animation
					bonusCounter = bonusDuration;
					isTop = true;
				}

				// Update new record state
				newRecord = true;
			}

			#if !UNITY_EDITOR
			CheckAchievements();
			#endif

			// Play score audio source
			if (!playAlways && !scoreSource.isPlaying) scoreSource.Play();
			else if (playAlways) scoreSource.Play();

			AddDynamicElement(DynamicElementType.SCORE, position);
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call AddScore() when state is not expected");
		#endif
	}

	public void AddCoin(Vector3 position)
	{
		if (state == GameplayStates.GAME)
		{
			// Update current coins amount
			gameManager.Coins += ProjectManager.defaultCoin;

			// Play coin audio source
			coinSource.Play();

			AddDynamicElement(DynamicElementType.COIN, position);
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call AddCoin() when state is not expected");
		#endif
	}

	public void AddBonus(Vector3 position)
	{
		if (state == GameplayStates.GAME)
		{
			// Update current coins amount
			gameManager.Coins += ProjectManager.bonusCoin;

			// Play coin and bonus audio source
			coinSource.Play();
			bonusSource.Play();

			if (extraCounter <= 0f && bonusCounter <= 0f)
			{
				// Start bonus animation
				bonusCounter = bonusDuration;
				isTop = false;
			}

			AddDynamicElement(DynamicElementType.BONUS, position);
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call AddBonus() when state is not expected");
		#endif
	}

	public void AddExtra(Vector3 position)
	{
		if (state == GameplayStates.GAME)
		{
			// Update current coins amount
			gameManager.Coins += ProjectManager.extraCoin;

			// Play coin and bonus audio source
			coinSource.Play();
			extraSource.Play();

			if (bonusCounter <= 0f && extraCounter <= 0f)
			{
				// Start extra animation
				extraCounter = extraDuration;
				isTop = false;
			}

			AddDynamicElement(DynamicElementType.EXTRA, position);
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call AddExtra() when state is not expected");
		#endif
	}

	public void EnableHabilities()
	{
		// Enable habilities interface disable animator
		if (!canHability)
		{
			habilitiesGroup.interactable = true;
			canHability = true;
		}
	}

	public void UseHability(int hability)
	{
		// Check if new record interface is not showing
		if (!recordScale.IsPlaying && (bonusCounter <= 0) && (extraCounter <= 0))
		{
			// Start hability background animation
			bonusCounter = bonusDuration;
			isTop = true;

			// Play hability interface animations references
			habilities[hability].Scale.Play();
			habilities[hability].Alpha.Play();
			recordSource.Play();

			// Enable habilities interface disable animator
			if (canHability)
			{
				habilitiesGroup.interactable = false;
				canHability = false;
			}
		}
	}

	public void FinishGame()
	{
		if (state == GameplayStates.GAME)
		{
			#if DEBUG_INFO
			Debug.Log("GameplayManager: finishing current game");
			#endif

			// Save changes if needed
			if (currentScore > gameManager.BestScore)
			{
				gameManager.BestScore = currentScore;

				#if !UNITY_EDITOR
				// Submit new record score to leaderboard
				gameManager.UploadLeaderboard(ProjectManager.leaderboardId, (float)currentScore);
				#endif
			}

			// Enable habilities interface disable animator
			if (canHability)
			{
				habilitiesGroup.interactable = false;
				canHability = false;
			}

			// Update gameplay controller state
			gameplayController.FinishGame();

			// Play finish audio source
			finishSource.Play();

			// Update pause button interactable state
			pauseButton.interactable = false;

			// Update played times for time based logic
			gameManager.TotalTimes++;
			rewardedTimes++;

			// Save data to disk
			gameManager.SaveData();

			// Disable all gameplay buttons to avoid game flow bugs
			for (int i = 0; i < gameplayButtons.Length; i++) gameplayButtons[i].interactable = false;

			#if DEBUG_INFO
			Debug.Log("GameplayManager: finishing game and returning to menu");
			#endif

			Invoke("BackToMenu", backDelay);
		}
		#if DEBUG_INFO
		else Debug.LogWarning("GameplayManager: attempting to call FinishGame() when state is not expected");
		#endif
	}

	#region Gameplay Internal Methods
	private void BackToMenu()
	{
		// Update game state, enable menu game object and reset its transparency
		state = GameplayStates.MENU;
		currentScore = 0;

		// Fix menu interface values
		menuGroup.gameObject.SetActive(true);
		menuGroup.alpha = 1f;
		bubbleParticles.Play();

		#if !UNITY_EDITOR
		// Hide banner advertisement if needed
		if (bannerDown)
		{
			gameManager.HideBanner();

			#if DEBUG_INFO
			Debug.Log("GameplayManager: attempting to hide banner advertisement");
			#endif
		}

		// Show rate game dialog if needed
		if (gameManager.TotalTimes == timesForRate)
		{
			gameManager.ShowRatePopUp(ProjectManager.rateTitle, ProjectManager.rateMessage);

			#if DEBUG_INFO
			Debug.Log("GameplayManager: attempting to show rate app pop up");
			#endif
		}
		#endif

		// Show intersticial advertisement for each amount of played times
		if (intersticialTimes >= timesForIntersticial && gameManager.NoAds == 0)
		{
			// Reset played times amount
			intersticialTimes = 0;

			#if !UNITY_EDITOR
			// Show intersticial advertisement
			gameManager.ShowIntersticial();
			#endif

			#if DEBUG_INFO
			Debug.Log("GameplayManager: attempting to show intersticial advertisement");
			#endif
		}

		// Enable watch and win button for each amount of played times with a random factor
		int random = Random.Range(0, 2);
		if (random == 1 && rewardedTimes >= timesForWatch)
		{
			// Reset played times amount
			rewardedTimes = 0;
			rewardButton.SetActive(true);

			#if !UNITY_EDITOR
			// Load rewarded video to avoid waiting when pressing button
			gameManager.LoadRewardedVideo();
			#endif
		}

		// Instantiate a new player after finish delay
		gameplayController.RestartPlayer();

		// Disable menu game object after transition
		Invoke("DisableGameplay", 2f);
	}

	#if !UNITY_EDITOR
	private void CheckAchievements()
	{
		if (!gameManager.GoodAchievement && (currentScore >= ProjectManager.goodAchievementScore))
		{
			gameManager.GoodAchievement = true;
			gameManager.UploadAchievement(ProjectManager.goodAchievement);
			gameManager.SaveData();
		}

		if (!gameManager.IncreibleAchievement && (currentScore >= ProjectManager.increibleAchievementScore))
		{
			gameManager.IncreibleAchievement = true;
			gameManager.UploadAchievement(ProjectManager.increibleAchievement);
			gameManager.SaveData();
		}

		if (!gameManager.AwesomeAchievement && (currentScore >= ProjectManager.awesomeAchievementScore))
		{
			gameManager.AwesomeAchievement = true;
			gameManager.UploadAchievement(ProjectManager.awesomeAchievement);
			gameManager.SaveData();
		}

		if (!gameManager.BossAchievement && (currentScore >= ProjectManager.bossAchievementScore))
		{
			gameManager.BossAchievement = true;
			gameManager.UploadAchievement(ProjectManager.bossAchievement);
			gameManager.SaveData();
		}
	}
	#endif

	private void AddDynamicElement(DynamicElementType type, Vector3 worldPos)
	{
		// Transform world position to interface canvas position
		Vector3 interfacePos = interfaceCamera.WorldToViewportPoint(worldPos);
		interfacePos.x *= Screen.width;
		interfacePos.y *= Screen.height;
		interfacePos.z = 0f;

		// Instantiate element game object and parent to dynamic root transform
		GameObject newObject = null;
		switch (type)
		{
			case DynamicElementType.SCORE: newObject = dynamicElementsPool.AddScore(); break;
			case DynamicElementType.COIN: newObject = dynamicElementsPool.AddCoin(); break;
			case DynamicElementType.BONUS: newObject = dynamicElementsPool.AddBonus(); break;
			case DynamicElementType.EXTRA: newObject = dynamicElementsPool.AddExtra(); break;
			default: break;
		}

		if (newObject)
		{
			// Update transform values to achieve interface expected position
			Transform currentParent = newObject.transform.parent;
			newObject.transform.SetParent(null);
			newObject.transform.position = interfacePos;
			newObject.transform.SetParent(currentParent, false);
			newObject.transform.localScale = Vector3.one;

			// Transform position to interface camera
			RectTransform rectTrans = newObject.GetComponent<RectTransform>();
			Vector3 anchoredPos = rectTrans.anchoredPosition3D;
			anchoredPos.x -= canvasScaler.referenceResolution.x/2;
			anchoredPos.y -= canvasScaler.referenceResolution.y/2;
			anchoredPos.z = 0f;
			rectTrans.anchoredPosition3D = anchoredPos;

			// Initialize new dynamic element
			newObject.GetComponent<DynamicElement>().Initialize();
		}
	}

	private void DisableTutorial()
	{
		// Disable tutorial interface game object
		tutorialObject.SetActive(false);
	}

	private void DisableGameplay()
	{
		// Disable gameplay interface game objects and enable menu buttons interactable state
		gameplayGroup.gameObject.SetActive(false);
		gameplayGroup.alpha = 0f;
		for (int i = 0; i < menuButtons.Length; i++) menuButtons[i].interactable = true;
	}
	#endregion
	#endregion

	#if UNITY_EDITOR
	#region Editor Methods
    private void OnApplicationQuit() 
    {
        // Reset materials to start value
		backgroundMat.SetFloat("_Ratio", ratioInit);
		backgroundMat.SetColor("_TopColor", initColorT);
		backgroundMat.SetColor("_BottomColor", initColorB);

		Color trailColor = trailMat.GetColor("_TintColor");
		trailColor.a = 0.8f;
		trailMat.SetColor("_TintColor", trailColor);
    }
    #endregion
    #endif

	#region Properties
	public bool BannerDown
	{
		get { return bannerDown; }
	}

	public bool CanHability
	{
		get { return canHability; }
	}
	#endregion

	#region Serializable
	[System.Serializable]
	public class Hability
	{
		#region Inspector Members
		[Header("References")]
		[SerializeField] private ScaleObject scale;
		[SerializeField] private AlphaObject alpha;
		#endregion

		#region Properties
		public ScaleObject Scale
		{
			get { return scale; }
		}

		public AlphaObject Alpha
		{
			get { return alpha; }
		}
		#endregion
	}
	#endregion
}
