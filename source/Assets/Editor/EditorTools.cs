using UnityEngine;
using UnityEditor;
using System.Collections;

public class EditorTools : MonoBehaviour 
{
	#region Data
	[MenuItem("Workflow/Delete Player Prefs")]
	public static void DeletePlayerPrefs()
	{
		Debug.Log("Editor: Player Preferences deleted successfully");
		PlayerPrefs.DeleteAll();
	}
	#endregion

	#region Transform
	[MenuItem("Workflow/Count GameObject Childs")]
	public static void CountGameObjectChilds()
	{
		Debug.Log("Editor: current selected GameObject childs: " + Selection.activeGameObject.transform.childCount);
	}
	#endregion
}
