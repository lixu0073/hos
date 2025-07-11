using UnityEngine;
using System.Collections.Generic;

namespace Hospital
{
	public class WallPrefabsDatabase : ScriptableObject
	{
		[SerializeField]
		public GameObject left;
		[SerializeField]
		public GameObject right;
		[SerializeField]
		public GameObject leftCorner;
		[SerializeField]
		public GameObject rightCorner;
		[SerializeField]
		public GameObject outerCorner;
		[SerializeField]
		public GameObject innerCorner;
		[SerializeField]
		public GameObject grassPrefab;

		public Grass grass;

		public Material materialPrefab;
		public Material windowMaterial;
		public Material glassWallMaterial;



#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Walls/WallsDatabase")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<WallPrefabsDatabase>();
		}
#endif
		public List<WallDatabaseThing> WallPrefabs;
		public WallPrefabsDatabase()
		{
			WallPrefabs = new List<WallDatabaseThing>();
			for (int i = 0; i < System.Enum.GetValues(typeof(WallType)).Length; i++)
			{
				WallPrefabs.Add(new WallDatabaseThing(((WallType)i).ToString()));
			}
		}
		public IEnumerable<GameObject> GetWindowParts(WallType type, wallType part, int windowID)
		{
			if (part != wallType.leftWindow && part != wallType.rightWindow)
				yield break;
			var len = WallPrefabs[(int)type].wallPrefabs.CheckWindowLenght(windowID);
			for (int i = len-1; i >=0; i--)
				yield return GetWallObject(type, part, windowID, i);
		}
		public GameObject GetWallObject(WallType type, wallType part, int windowID = 0,int windowPart=0)
		{
			return WallPrefabs[(int)type].wallPrefabs[part, windowID, windowPart];
		}

	}
	[System.Serializable]

	public class WallDatabaseThing
	{
		public string name;
		public WallDatabaseEntry wallPrefabs;
		public WallDatabaseThing(string Name)
		{
			this.name = Name;
		}
	}
	//DO NOT CHANGE ORDER OF ITEMS!! :) if you need new then add this to end of list.
	public enum WallType
	{
		BlueDoctor,
		GreenDoctor,
		PinkDoctor,
		PurpleDoctor,
		RedDoctor,
		SkyBlueDoctor,
		SunnyYellowDoctor,
		WhiteDoctor,
		YellowDoctor,
		ClinicCorridor,
		ProductionCorridor,
		HospitalCorridor,
		Bedroom,
		DiagnosisRooms,
		BrickWalls,
		UnderConstruction,
		Reception,
		Playground,
		GlassWalls,
        None,
        MaternityCorridor,
        MaternityOutDoor,
        MaternityWaitingRoomRose,
        MaternityLabourRoomRose,
        MaternityBloodTesetRoom,
        MaternityWaitingRoomSunflower,
        MaternityWaitingRoomBlueOrchid,
        MaternityLabourRoomBlueOrchid,
        MaternityLabourRoomSunflower,
		MaternityLabourRoomLavender,
		MaternityWaitingRoomLavender,
		MaternityLabourRoomTulip,
		MaternityWaitingRoomTulip,
		MaternityNurseRoom,
	}
}