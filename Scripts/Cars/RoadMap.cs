using UnityEngine;
using UnityEngine.Events;
using System.Collections;


public class RoadMap : ScriptableObject {
	#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/CarsDatabase")]
	public static void CreateAsset(){
		ScriptableObjectUtility.CreateAsset<RoadMap>();
	}
	#endif

	public Stage[] stages;

	[System.Serializable]
	public class Stage
	{
		public Vector3 checkpoint = Vector3.zero;
		public UnityEvent[] ActionsToDo = null;
		//public int waitTime = 0;
	}
}
