using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ButtonElement : MonoBehaviour 
{
	#region Inspector Members
	[Header("Settings")]
	[Tooltip("Transition animation curve used in interpolation")]
	[SerializeField] private AnimationCurve curve;

	[Tooltip("Transition animation duration")]
	[SerializeField] private float duration;

	[Header("References")]
	[Tooltip("Audio source played when button is pressed")]
	[SerializeField] private AudioSource source;

	[Header("Events")]
	[Tooltip("Invoked methods when button animation is finished")]
	[SerializeField] private UnityEvent onPressed;
	#endregion

	#region Private Members
	private float timeCounter;		// Button pressed animation time counter
	#endregion

	#region Main Methods
	private void Start()
	{
		// Initialize values
		timeCounter = 0f;

		// Get references if not set yet
		if (!source) source = GetComponent<AudioSource>();
	}

	private void Update()
	{
		// Update transform based on custom curve position interpolation
		transform.localScale = Vector3.one*curve.Evaluate(timeCounter/duration);

		// Update animation time counter
		timeCounter += Time.deltaTime;

		// Check if animation is finished
		if (timeCounter >= duration)
		{
			// Reset time counter for next pressed state
			timeCounter = 0f;

			// Invoke all events in on pressed
			onPressed.Invoke();

			// Fix final local scale
			transform.localScale = Vector3.one*curve.Evaluate(1f);

			// Disable behaviour to avoid loop animation
			enabled = false;
		}
	}
	#endregion

	#region Button Methods
	public void PressButton()
	{
		// Play audio source if set
		if (source) source.Play();

		// Enable behaviour to play pressed animation
		enabled = true;
	}
	#endregion
}