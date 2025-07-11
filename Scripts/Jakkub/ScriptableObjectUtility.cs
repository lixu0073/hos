using UnityEngine;

using System.IO;

public static class ScriptableObjectUtility
{
	/// <summary>
	//	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// </summary>
	public static void CreateAsset<T> () where T : ScriptableObject
	{
#if UNITY_EDITOR_WIN
		T asset = ScriptableObject.CreateInstance<T> ();
		
		string path = UnityEditor.AssetDatabase.GetAssetPath (UnityEditor.Selection.activeObject);
		if (path == "") 
		{
			path = "Assets";
		} 
		else if (Path.GetExtension (path) != "") 
		{
			path = path.Replace (Path.GetFileName (UnityEditor.AssetDatabase.GetAssetPath (UnityEditor.Selection.activeObject)), "");
		}
		
		string assetPathAndName = UnityEditor.AssetDatabase.GenerateUniqueAssetPath (path + "/New " + typeof(T).ToString() + ".asset");
		
		UnityEditor.AssetDatabase.CreateAsset (asset, assetPathAndName);
		
		UnityEditor.AssetDatabase.SaveAssets ();
		UnityEditor.AssetDatabase.Refresh();
		UnityEditor.EditorUtility.FocusProjectWindow ();
		UnityEditor.Selection.activeObject = asset;
#endif
	}
}