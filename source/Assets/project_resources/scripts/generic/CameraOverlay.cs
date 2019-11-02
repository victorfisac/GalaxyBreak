//----------------------------------------------
//            Marvelous Techniques
// Copyright © 2015 - Arto Vaarala, Kirnu Interactive
// http://www.kirnuarp.com
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent (typeof (Camera))]
public class CameraOverlay : MonoBehaviour 
{
	#region Inspector Members
	[Header("References")]
	[Tooltip("Displaying camera reference")]
	[SerializeField] private Camera cam;

	[Tooltip("Transform references to adapt screen")]
	[SerializeField] private Transform[] trans;
	#endregion

	#region Main Methods
	private void Start() 
	{
		// Check if camera reference is not set
		if (!cam) cam = GetComponent<Camera>();
	}

	private void Update()
	{
		// Check if camera projection is orthographic or perspective
		if(cam.orthographic)
		{
			// Calculate camera height and bounds based on orthographic size
			float cameraHeight = cam.orthographicSize*2f;
			Vector3 bounds = new Vector3(cameraHeight*(float)Screen.width/(float)Screen.height, cameraHeight, 0);

			// Update background quad scale based on calculated bounds
			for (int i = 0; i < trans.Length; i++) trans[i].localScale = new Vector3 (bounds.x, bounds.y, 1);
		}
		else
		{
			// Update background quad scale based on calculated bounds
			for (int i = 0; i < trans.Length; i++)
			{
				// Calculate distance between camera position and quad position
				float distance = Vector3.Distance(trans[i].position, cam.transform.position);

				// Update quad rotation to look to camera position
				trans[i].LookAt (cam.transform);

				// Ensure quad position is in camera planes range
				if (distance < cam.nearClipPlane)
				{
					trans[i].Translate(cam.nearClipPlane*trans[i].forward*1.1f);
					distance = Vector3.Distance(trans[i].position, cam.transform.position);
				}

				// Calculate frustrum dimensions
				float height = (Mathf.Tan(cam.fieldOfView*Mathf.Deg2Rad*0.5f)*distance*2f);
				float frustumWidth = height*cam.aspect;

				// Update quad scale based on calculated dimensions
				trans[i].localScale = new Vector3(frustumWidth, height, 1.0f);
			}
		}
	}
	#endregion
}
