using IsoEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hospital
{
	[System.Serializable]
	public class Section : ScriptableObject
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/MapConfigurator/Section")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<Section>();
		}
#endif
		public List<Area> defaultAreas;
		public List<Area> areas;
		public WallType wallType;
		public WallType outsideWallType;

		[ContextMenu("Sort Areas")]
		void SortThings()
		{
			areas = areas.OrderBy(x => x.name).ToList();
			for (int i = 0;i< areas.Count; i++)
			{
				areas[i].areaID = i;
			}
		}
		[ContextMenu("Randomize Flora and stones")]
		void RandomizeTrees()
		{
			int treeCount = 8;
			int maxTrees = 3;
			//System.Random rand = new System.Random();
			int i;
			foreach(var p in areas)
			{
				maxTrees = p.size.x * p.size.y / 5;
				p.trees.Clear();
				for(i=0; i<maxTrees;i++)
				{
					p.trees.Add(new Vector3i(GameState.RandomNumber(p.size.x), GameState.RandomNumber(p.size.y), GameState.RandomNumber(treeCount)));
				}
					
			}
		}
	}
}
