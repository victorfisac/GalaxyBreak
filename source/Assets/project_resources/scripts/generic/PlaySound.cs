using UnityEngine;
using System.Collections;

public class PlaySound : MonoBehaviour 
{
	#region Enums
	public enum PlayType { START, ENABLED };
	#endregion

    #region Inspector Members
    [Header("Settings")]
    [Tooltip("Play sound just on start or everytime game object is enabled")]
    [SerializeField] private PlayType type;

    [Header("References")]
    [Tooltip("Audio source to play when starting or enabling game object")]
    [SerializeField] private AudioSource source;
    #endregion
    
    #region Main Methods
	private void Start ()
    {
    	// Play audio source if start state is active
		if (type == PlayType.START) source.Play();
	}
	
	private void OnEnable ()
    {
		// Play audio source if enabled state is active
		if (type == PlayType.ENABLED) source.Play();
	}
    #endregion
}
