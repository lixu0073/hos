using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using IsoEngine;

namespace Hospital
{
	class SingleRectArea : IMapArea
	{
		private readonly GameArea gameArea;

		public static int areaCount;
		public static int GetNextID()
		{
			return areaCount++;
		}
		static SingleRectArea()
		{
			areaCount = 0;
		}

		RectWallInfo wall;
		public readonly int areaID;
        public readonly string Name;
        public readonly ExpansionType expansionType;
        GameObject debug;
		List<GameObject> trees;
		public void SetParent(Transform trans)
		{
			debug.transform.SetParent(trans);
			//foreach (var p in trees)
			//	p.transform.SetParent(trans);
		}
		public int GetID()
		{
			return areaID;
		}
		public SingleRectArea(RectWallInfo WallData, GameArea gameArea)
		{
			this.areaID = GetNextID();
			this.wall = WallData;
			this.gameArea = gameArea;
			this.trees = new List<GameObject>();
			MakeBackGround(WallData);
		}

		private void MakeBackGround(RectWallInfo rectWall)
		{
			var rect = rectWall.rect;
			//debug = GameObject.Instantiate(HospitalAreasMapController.map.GroundGrassPrefab);
			//GameObject.Destroy(debug.GetComponent<Collider>());
			//debug.transform.position = new Vector3(rect.x + rect.xSize / 2.0f - 0.5f, 0.01f, rect.y + rect.ySize / 2.0f - 0.5f);
			//debug.transform.localScale = new Vector3(rect.xSize, rect.ySize, 1);
			//debug.transform.rotation = Quaternion.Euler(Vector3.right * 90);



			debug = new GameObject("Area grass " + areaID);
			debug.transform.position = new Vector3(rect.x, 0, rect.y);
			debug.SetActive(true);

			var grass = AreaMapController.Map.wallDatabase.grass;


			GameObject p;
			for (int i = 0; i < rect.xSize; i++)
				for (int j = 0; j < rect.ySize; j++)
				{
                    //p = grass[(GrassPart)((i == 0 ? 0 : (i == rect.xSize - 1 ? 2 : 1)) * 3 + (j == 0 ? 0 : (j == rect.ySize - 1 ? 2 : 1)))];
                    p = grass[0];
                    p.transform.SetParent(debug.transform);
					p.transform.localPosition = new Vector3(i + 0.025f, 0, j + 0.1f);
				}



			if (rectWall.treePositions != null)
				foreach (var z in rectWall.treePositions)
				{
					var tempo = GameObject.Instantiate(AreaMapController.Map.treePrefabs[(int)z.z]);
					tempo.SetActive(true);
                    tempo.transform.SetParent(debug.transform);
                    //tempo.transform.position = new Vector3(rect.x + z.x, tempo.transform.position.y, rect.y + z.y);
                
                    tempo.transform.localPosition = new Vector3(z.x + BaseGameState.RandomFloat(-0.2f, 0.0f), 0, z.y + BaseGameState.RandomFloat(-0.1f, 0.0f));

                    trees.Add(tempo);

                    if (tempo.GetComponent<Animator>())
                    {
                        tempo.GetComponent<Animator>().speed = BaseGameState.RandomFloat(0.5f, 1.0f);
                    }
                }
		}
		internal SingleRectArea(RectWallInfo wallInfo, int areaId, string name, GameArea gameArea, ExpansionType expansionType, bool addToMap = true)
		{
			this.areaID = areaId;
			this.gameArea = gameArea;
			if (areaID >= areaCount)
				areaCount = areaID + 1;
			this.wall = wallInfo;
			this.Name = name;
			this.trees = new List<GameObject>();
            this.expansionType = expansionType;
            if(addToMap)
                MakeBackGround(wallInfo);
		}

		public IEnumerable<RectWallInfo> GetRectangles()
		{
			yield return wall;
		}

		public Vector3 GetRectPos()
		{
			Vector3 tmp = new Vector3 (wall.rect.x + wall.rect.xSize, 0, wall.rect.y);
			return tmp;
		}

        public Vector3 GetRectSize()
        {
            Vector3 tmp = new Vector3(wall.rect.xSize , 0, wall.rect.ySize);
            return tmp;
        }

        public Vector3 GetRectStartPoint()
        {
            Vector3 tmp = new Vector3(wall.rect.x, 0, wall.rect.y);
            return tmp;
        }

        public bool CanBuy()
		{
			bool containsYD = true;
			bool containsYU = true;

			for (int x = wall.rect.x; x < wall.rect.x + wall.rect.xSize; ++x)
			{
				if (!gameArea.ContainsPoint(x, wall.rect.y - 1))
                    containsYD = false;

				if (!gameArea.ContainsPoint(x, wall.rect.y + wall.rect.ySize))
					containsYU = false;
			}

			bool containsXL = true;
			bool containsXR = true;

			for(int y = wall.rect.y; y < wall.rect.y + wall.rect.ySize; ++y)
			{
				if (!gameArea.ContainsPoint(wall.rect.x - 1, y))
					containsXR = false;

				if (!gameArea.ContainsPoint(wall.rect.x + wall.rect.xSize, y))
					containsXL = false;
			}

			return containsYD || containsYU || containsXL || containsXR;
		}

		public bool ContainsPoint(int x, int y)
		{
			return wall.rect.Contains(x, y);
		}

		public bool ContainsPoint(Vector2i position)
		{
			return wall.rect.Contains(position);
		}
		
		public void IsoDestroy()
		{
			GameObject.Destroy(debug);
			foreach (var p in trees)
				GameObject.Destroy(p);
		}

        public string GetAreaName()
        {
            return Name;
        }

        public ExpansionType GetExpansionType()
        {
            return expansionType;
        }
    }
}
