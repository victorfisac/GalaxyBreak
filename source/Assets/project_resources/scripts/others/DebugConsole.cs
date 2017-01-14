using UnityEngine;
using System.Collections;

public class DebugConsole : MonoBehaviour
{
	#if UNITY_ANDROID && DEBUG_INFO
	#region Private Members
	private string myLog;			// Current adding string to console
	private string[] messages;		// Current messages array
	private int currentMessage;		// Message index counter
	#endregion

	#region Main Methods
	private void Start()
	{
		// Initialize values
		myLog = string.Empty;
		messages = new string[25];
		currentMessage = 0;

		// Listen to Unity console messages update
		Application.logMessageReceived += OnMessageLog;
	}
	#endregion

	#region GUI Methods
	private void OnMessageLog(string message, string trace, LogType type)
	{
		if (type != LogType.Exception)
		{
			myLog = message;
			string newString = "\n  " + myLog;

			if (currentMessage < messages.Length)
			{
				messages[currentMessage] = newString;
				currentMessage++;
			}
			else
			{
				for (int i = 0; i < messages.Length - 1; i++) messages[i] = messages[i + 1];
				messages[messages.Length - 1] = newString;
			}

			myLog = string.Empty;
			for (int i = 0; i < messages.Length; i++) myLog += messages[i];
		}
	}

	private void OnGUI() 
	{
		// Draw all logged messages from Unity console
		GUILayout.Label(myLog);
	}
	#endregion
	#endif
}