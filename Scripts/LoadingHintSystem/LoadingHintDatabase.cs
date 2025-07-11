using UnityEngine;
using System.Collections;

public class LoadingHintDatabase : ScriptableObject {
	#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/LoadingHintDatabase")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<LoadingHintDatabase>();
	}
	#endif

	public string[] keys;

}
