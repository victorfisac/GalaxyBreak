using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour 
{
	#region Enums
	public enum DynamicType { TRANSFORM, RIGIDBODY };
	#endregion

	#region Inspector Members
	[Header("Settings")]
	[Tooltip("Rotation type can be using Transform or Rigidbody")]
	[SerializeField] private DynamicType type;

	[Tooltip("Rotation speed based on delta time in euler angles")]
	[SerializeField] private Vector3 speed;

	[Header("References")]
	[Tooltip("Updated rigidbody in rotation behaviour if using Rigidbody type")]
	[SerializeField] private Rigidbody rb;
	#endregion

	#region Main Methods
	private void Start()
	{
		// Get references if not set yet
		if (!rb && type == DynamicType.RIGIDBODY) rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (type == DynamicType.TRANSFORM) transform.Rotate(speed*Time.deltaTime);
	}

	private void FixedUpdate()
	{
		if (type == DynamicType.RIGIDBODY) rb.MoveRotation(Quaternion.Euler(transform.rotation.eulerAngles + speed*Time.fixedDeltaTime));
	}
	#endregion
}
