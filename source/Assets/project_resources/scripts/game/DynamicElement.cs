using UnityEngine;
using System.Collections;

public class DynamicElement : MonoBehaviour 
{
	#region Inspector Members
	[Header("Motion")]
	[Tooltip("Up translation animation curve")]
	[SerializeField] private AnimationCurve motionCurve;

	[Tooltip("Up translation animation duration")]
	[SerializeField] private float motionDuration;

	[Tooltip("Up translation animation scale")]
	[SerializeField] private float motionScale;

	[Header("Mirror")]
	[Tooltip("Minimum position in X axis to mirror element due to screen limits")]
	[SerializeField] private float minPosX;

	[Tooltip("Position in X axis to move element due to screen limits")]
	[SerializeField] private float mirrorPos;

	[Header("References")]
	[Tooltip("Canvas group reference for alpha animation")]
	[SerializeField] private CanvasGroup canvasGroup;

	[Tooltip("Transform rectangle for up translation animation")]
	[SerializeField] private RectTransform trans;
	#endregion

	#region Private Members
	private float timeCounter;		// Dynamic animation time counter
	private Vector2 initPos;		// Dynamic transform rectangle position
	#endregion

	#region Main Methods
	public void Initialize()
	{
		// Initialize values
		timeCounter = 0f;
		initPos = trans.anchoredPosition;

		if (initPos.x < minPosX)
		{
			initPos.x += mirrorPos;
			trans.anchoredPosition = initPos;
		}

		trans.localScale = Vector3.one;
		trans.GetChild(0).localScale = Vector3.one;

		// Enable game object
		gameObject.SetActive(true);
	}

	private void Update()
	{
		// Update position up
		trans.anchoredPosition = initPos + Vector2.up*motionCurve.Evaluate(timeCounter/motionDuration)*motionScale;

		// Update canvas group alpha
		canvasGroup.alpha = 1f - Mathf.Clamp01(timeCounter/motionDuration);

		// Update time counter
		timeCounter += Time.deltaTime;

		if (timeCounter >= motionDuration) Unitialize();
	}

	#region Main Internal Methods
	private void Unitialize()
	{
		// Reset values
		timeCounter = 0f;
		initPos = Vector2.zero;

		// Disable game object
		gameObject.SetActive(false);
	}
	#endregion
	#endregion
}
