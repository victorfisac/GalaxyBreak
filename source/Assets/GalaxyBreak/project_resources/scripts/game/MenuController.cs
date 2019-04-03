using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour 
{
	#region Inspector Members
	[Header("Motion")]
	[Tooltip("Transition animation curve used in interpolation")]
	[SerializeField] private AnimationCurve curve;

	[Tooltip("Transition animation duration")]
	[SerializeField] private float duration;

	[Header("Positions")]
	[Tooltip("Default menu position for animation")]
	[SerializeField] private Vector2 defaultPosition;

	[Tooltip("Shop menu position for animation")]
	[SerializeField] private Vector2 shopPosition;

	[Tooltip("World default menu position for animation")]
	[SerializeField] private Vector3 worldDefaultPosition;

	[Tooltip("World shop menu position for animation")]
	[SerializeField] private Vector3 worldShopPosition;

	[Header("References")]
	[Tooltip("Updated transform in transition animation")]
	[SerializeField] private RectTransform trans;

	[Tooltip("Updated world transform in transition animation")]
	[SerializeField] private Transform worldTrans;

	[Tooltip("Menu game object disabled when current menu is shop")]
	[SerializeField] private GameObject menuObject;

	[Tooltip("Shop game object disabled when current menu is default")]
	[SerializeField] private GameObject shopObject;

	[Tooltip("Menu transitions animator reference")]
	[SerializeField] private Animator menuAnim;
	#endregion

	#region Private Members
	private bool inTransition;		// Menu transition state
	private int currentMenu;		// Current menu index
	private float timeCounter;		// Transition time counter
	#endregion

	#region Main Methods
	private void Start()
	{
		// Initialize values
		inTransition = false;
		currentMenu = 0;
		trans.localPosition = defaultPosition;

		// Disable shop game object by default
		if (shopObject.activeSelf) shopObject.SetActive(false);
		if (!menuObject.activeSelf) menuObject.SetActive(true);
	}

	private void Update() 
	{
		if (inTransition)	// Check if menu is currently in transition
		{
			// Update transform based on custom curve position interpolation
			if (currentMenu == 1)
			{
				trans.anchoredPosition = Vector3.Lerp(defaultPosition, shopPosition, curve.Evaluate(timeCounter/duration));
				worldTrans.localPosition = Vector3.Lerp(worldDefaultPosition, worldShopPosition, curve.Evaluate(timeCounter/duration));
			}
			else
			{
				trans.anchoredPosition = Vector3.Lerp(shopPosition, defaultPosition, curve.Evaluate(timeCounter/duration));
				worldTrans.localPosition = Vector3.Lerp(worldShopPosition, worldDefaultPosition, curve.Evaluate(timeCounter/duration));
			}

			// Update animation time counter
			timeCounter += Time.deltaTime;

			// Check if animation is finished
			if (timeCounter >= duration)
			{
				// Fix final menu position to avoid float precision errors
				if (currentMenu == 1)
				{
					trans.anchoredPosition = shopPosition;
					worldTrans.localPosition = worldShopPosition;
					menuObject.SetActive(false);	// Disable menu game object if current menu is shop
				}
				else
				{
					trans.anchoredPosition = defaultPosition;
					worldTrans.localPosition = worldDefaultPosition;
					shopObject.SetActive(false);	// Disable shop game object if current menu is default
				}

				// Reset transition state and time counter values
				inTransition = false;
				timeCounter = 0f;
			}
		}
	}
	#endregion

	#region Menu Methods
	public void SetShop()
	{
		if (!inTransition)	// Check if menu is already in transition to avoid flow bugs
		{
			timeCounter = 0f;
			inTransition = true;
			currentMenu = 1;

			// Enable shop game object if transition is to shop
			shopObject.SetActive(true);
		}
	}

	public void SetMenu()
	{
		if (!inTransition)	// Check if menu is already in transition to avoid flow bugs
		{
			timeCounter = 0f;
			inTransition = true;
			currentMenu = 0;

			// Enable menu game object if transition is to menu
			menuObject.SetActive(true);
			menuAnim.Play("anim_menu", 0, 1f);
		}
	}
	#endregion

	#region Properties
	public int CurrentMenu
	{
		get { return currentMenu; }
	}
	#endregion
}
