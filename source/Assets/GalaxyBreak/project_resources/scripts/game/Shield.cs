using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour 
{
	#region Inspector Members
	[Header("Settings")]
	[Tooltip("Shield translation speed")]
	[SerializeField] private float shieldSpeed;

	[Tooltip("Time to wait until destroying shield game object")]
	[SerializeField] private float destroyDelay;

	[Header("References")]
	[Tooltip("Shield rigidbody reference")]
	[SerializeField] private Rigidbody rb;

	[Tooltip("Shield collider reference")]
	[SerializeField] private Collider coll;

	[Tooltip("Shield visual game object")]
	[SerializeField] private GameObject visualObject;

	[Tooltip("Shield trail renderer reference")]
	[SerializeField] private TrailRenderer trail;

	[Tooltip("Shield detection game object")]
	[SerializeField] private GameObject detectionObject;

	[Tooltip("Explosion particles game object")]
	[SerializeField] private GameObject explosionObject;
	#endregion

	#region Private Members
	private Color trailColor;			// Trail color during explosion fade out
	private Color trailColorInit;		// Trail renderer color by default
	#endregion

	#region Main Methods
	private void Start()
	{
		// Initialize values
		trailColorInit = trail.material.GetColor("_TintColor");
		gameObject.SetActive(false);
	}

	private void Update()
	{
		if (explosionObject.activeSelf)
		{
			// Get current trail material color
			trailColor = trail.material.GetColor("_TintColor");

			// Update alpha to fade out trail color
			if (trailColor.a > 0)
			{
				trailColor.a -= Time.deltaTime*2f;
				trail.material.SetColor("_TintColor", trailColor);
			}
			else enabled = false;
		}
	}
	#endregion

	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Killer" && other.gameObject.name.Contains("GameplayLimits")) Explode();
	}
	#endregion

	#region Shield Methods
	public void Initialize(Vector3 position, Quaternion rotation)
	{
		// Initialize values
		transform.position = position;
		transform.rotation = rotation;
		rb.isKinematic = false;
		rb.velocity = transform.TransformDirection(Vector3.up).normalized*shieldSpeed;
		coll.enabled = true;
		trail.material.SetColor("_TintColor", trailColorInit);
		trail.Clear();
		detectionObject.SetActive(true);
		visualObject.SetActive(true);
		explosionObject.SetActive(false);
		enabled = true;
		gameObject.SetActive(true);

		// Cancel explode invoke if needed and reinvoke it
		if (IsInvoking("Explode")) CancelInvoke("Explode");
		Invoke("Explode", destroyDelay);
	}

	public void Explode()
	{
		// Cancel explode invoke if needed
		if (IsInvoking("Explode")) CancelInvoke("Explode");

		// Update rigidbody kinematic, collider enabled states and disable visual game object
		rb.isKinematic = true;
		coll.enabled = false;
		visualObject.SetActive(false);
		detectionObject.SetActive(false);

		// Enable explosion game object
		explosionObject.SetActive(true);
		explosionObject.transform.LookAt(Vector3.forward);

		Invoke("Unitialize", 3f);
	}

	#region Shield Internal Methods
	private void Unitialize()
	{
		// Disable shield game object
		gameObject.SetActive(false);
	}
	#endregion
	#endregion

	#region Properties
	public bool IsDestroyed
	{
		get { return explosionObject.activeSelf; }
	}
	#endregion
}
