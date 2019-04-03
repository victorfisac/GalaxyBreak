using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
	#region Inspector Members
	[Header("Settings")]
	[Tooltip("Coin duration before explode (0 = unlimited)")]
	[SerializeField] private float duration;

	[Tooltip("Delay after pick up to destroy explosion game object")]
	[SerializeField] private float destroyDelay;

	[Header("References")]
	[Tooltip("Coin detection collider")]
	[SerializeField] private Collider coll;

	[Tooltip("Visual elements game object reference")]
	[SerializeField] private GameObject visualObject;

	[Tooltip("Coin pick up explosion game object reference")]
	[SerializeField] private GameObject explosionObject;

	[Tooltip("Scale object animation reference")]
	[SerializeField] private ScaleObject scaleObject;

	[Tooltip("Coin spawn audio source")]
	[SerializeField] private AudioSource coinSource;
	#endregion

	#region Main Methods
	private void Start()
	{
		// Disable coin game object by default
		gameObject.SetActive(false);
	}
	#endregion

	#region Coin Methods
	public void Initialize(Vector3 position)
	{
		// Enable coin game object and initialize active states
		transform.position = position;
		transform.localScale = Vector3.zero;
		coll.enabled = true;
		visualObject.SetActive(true);
		explosionObject.SetActive(false);
		gameObject.SetActive(true);
		scaleObject.Play();
		coinSource.Play();

		// Invoke explode method if duration is not unlimited for bonus coins
		if (duration > 0f) Invoke("Explode", duration);
	}

	public void Explode()
	{
		// Cancel method invoke if needed
		if (IsInvoking("Explode")) CancelInvoke("Explode");

		// disable detection collider and Update game object states
		coll.enabled = false;
		visualObject.SetActive(false);
		explosionObject.SetActive(true);

		// Unitialize coin game object after explosion delay
		Invoke("Unitialize", destroyDelay);
	}

	#region Coin Internal Methods
	private void Unitialize()
	{
		// Disable game object
		gameObject.SetActive(false);
	}
	#endregion
	#endregion
}
