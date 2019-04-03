using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashUI : MonoBehaviour 
{
    #region Inspector Members
    [Header("Splash")]
    [Tooltip("Delay duration before switching to Game scene")]
    [SerializeField] private float splashDuration;

    [Tooltip("Url to open if subtitle text is touched")]
    [SerializeField] private string openUrl;
    #endregion
    
    #region Private Members
	private float timeCounter;				// Transition to game scene delay time counter
	#if !UNITY_EDITOR
	private GameManager gameManager;		// Game manager reference
	#endif
    #endregion
    
    #region Main Methods
	private void Start()
    {
		// Initialize values
		timeCounter = 0f;
		#if !UNITY_EDITOR
		gameManager = GameManager.Instance;
		gameManager.SetReferences(this);
		#endif

		// Enable v-sync
		Application.targetFrameRate = 60;
	}
	
	private void Update()
    {
    	// Update time counter
		timeCounter += Time.deltaTime;

		// Switch to Game scene after splash duration delay
		if (timeCounter >= splashDuration)
		{
			#if !UNITY_EDITOR
			gameManager.IsSplash = true;
			gameManager.InitPlayServices();
			enabled = false;
			#else
			ChangeScene();
			#endif
		}
	}
    #endregion

    #region Splash Methods
    public void ChangeScene()
    {
    	// Load game scene
		SceneManager.LoadScene("Game");
    }

    public void OpenURL()
    {
    	// Open website url in browser
    	Application.OpenURL(openUrl);
    }
    #endregion
}
