using UnityEngine;
using System.Collections;

public class CarsDatabase : ScriptableObject {
	#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/CarsDatabase")]
	public static void CreateAsset(){
		ScriptableObjectUtility.CreateAsset<CarsDatabase>();
	}
	#endif

	public Car[] cars;
	public Wheel[] wheels;

	[System.Serializable]
	public class Car{
		public Sprite[] carSprites;
		public Sprite carShadow;
		public Vector3 carShadowPosition;
		public Vector3 carShadowScale;
		public WheelType wheelType;
		public Vector3[] carWheelsPositions;
		public float speed;
	}

	[System.Serializable]
	public class Wheel{
		public Sprite[] wheelStates;
	}

	public Car GetCarOfType(CarsManager.CarType carType){
		return cars[(int)carType];
	}

	public Wheel GetWheelOfType(WheelType wheelType){
		return wheels[(int)wheelType];
	}

	public enum WheelType{
		normal
	}
}
