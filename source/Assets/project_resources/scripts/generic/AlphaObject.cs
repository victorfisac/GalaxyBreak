using UnityEngine;
using System.Collections;

public class AlphaObject : MonoBehaviour 
{
	#region Inspector Members
	[Header("Settings")]
	[Tooltip("Alpha animation curve")]
	[SerializeField] private AnimationCurve curve;

	[Tooltip("Scale animation duration")]
	[SerializeField] private float duration;

	[Tooltip("Animation loop state")]
	[SerializeField] private bool loop;

	[Tooltip("Play animation on start")]
	[SerializeField] private bool onStart;

	[Header("References")]
	[SerializeField] private CanvasGroup canvasGroup;
	#endregion

	#region Private Members
	private float timeCounter;		// Animation time counter
	private bool isPlaying;			// Animation current is playing state
	#endregion

	#region Main Methods
	private void Start()
	{
		// Initialize values
		timeCounter = 0f;
		isPlaying = onStart;
	}

	private void Update()
	{
		if (isPlaying)
		{
			// Update local scale based on animation curve
			canvasGroup.alpha = curve.Evaluate(timeCounter/duration);

			// Update time counter
			timeCounter += Time.deltaTime;

			if (timeCounter > duration)
			{
				if (loop) timeCounter = 0f;
				else
				{
					canvasGroup.alpha = curve.Evaluate(1f);
					isPlaying = false;
				}
			}
		}
	}
	#endregion

	#region Scale Methods
	public void Play()
	{
		// Reset time counter to start animation
		timeCounter = 0f;
		isPlaying = true;
	}
	#endregion

	#region Properties
	public bool IsPlaying
	{
		get { return isPlaying; }
	}
	#endregion
}
