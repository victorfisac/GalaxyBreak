//----------------------------------------------
//            Marvelous Techniques
// Copyright © 2015 - Arto Vaarala, Kirnu Interactive
// http://www.kirnuarp.com
//----------------------------------------------
using UnityEngine;
using System.Collections;

public class FpsCounter : MonoBehaviour 
{
	#region Private Members
	private string label;		// Displaying text string
	private float fps;			// Current frames per second
	private GUIStyle style;		// GUI style used to display text in screen
	private Rect rect;			// GUI rectangle used to display text in screen
	#endregion

	#region Main Methods
	private void Start ()
	{
		// Initialize values
		label = string.Empty;
		fps = 0f;
		style = new GUIStyle();
		style.normal.textColor = Color.white;
		GUI.depth = 2;
		rect = new Rect (5, 10, 100, 25);
	}

	private void Update()
	{
		fps = (1 / Time.deltaTime);
		label = "FPS :" + (Mathf.Round(fps));
	}
	#endregion

	#region GUI Methods
	private void OnGUI ()
	{
		// Display text using GUI
		GUI.Label(rect, label, style);
	}
	#endregion
}
