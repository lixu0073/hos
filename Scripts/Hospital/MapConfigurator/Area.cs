using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using IsoEngine;

namespace Hospital
{
	[System.Serializable]
	public class Area : ScriptableObject
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/MapConfigurator/Area")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<Area>();
		}
#endif
		public int areaID;
	    public string areaname;
        public ExpansionType expansionType = ExpansionType.Other;
		public Vector2i position;
		public Vector2i size;
		public List<int> windowPositions;
		public List<int> doorPositions;
		public int doorPosition=-1;
		public int doorType = -1;
        public PathType[] doorPathTypes = {PathType.Default};
        public int windowType = -1;
		public List<Vector3i> trees;

	}
}
