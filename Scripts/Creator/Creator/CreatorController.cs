#if UNITY_EDITOR
using UnityEngine;

using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using Hospital;
using IsoEngine;


public enum Side
{
    None = -1,
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public class CreatorController : MonoBehaviour
	{
		public GameObject tile;
		public GameObject arrow;

		public Sprite defaultSprite;
		[Tooltip("Size of object on X axis")]
		public int tilesX = 1;
		[Tooltip("Size of object on Z axis")]
		public int tilesZ = 1;
		[Tooltip("width of tiles that cannot store objects")]
		public int margin = 1;

		public Rotation rotation;
		public bool HasWalls;
		public Vector2 WallsFrom;
		public Vector2 WallsTo;
		public int InnerWallID;
		public int OuterWallID;
		public int DoorID;
		public int DoorPosition;
		public List<int> WindowsPosition;
		public int InnerWindowsID;
		public int OuterWindowID;
        public HospitalArea area;
        public Vector2 rotationPoint;

    [HideInInspector]
		public string PrefabName = "Prefab";

		[HideInInspector]
		public GameObject LoadingPrefab;

        [HideInInspector]
        public Side deleteSide = Side.None;
        public Side addSide = Side.None;

        private TileController[,] tiles;
		private List<ObjectController> objects = new List<ObjectController>();

        private string loaded_path;
		[System.Serializable]
		public class SpriteData
		{
			public string name;
			public int spriteID;
			public bool isTransparent;
			public Sprite sprite;
		}

		public SpriteData[] sprites;

        [HideInInspector]
        public List<int> transformPos = new List<int> {0, 0};

        private void Start()
		{
			print("Trying to rebuild");

			int x = -1;
			int z = -1;

			List<TileController> tmpTiles = new List<TileController>();

			for (int i = 0; i < transform.childCount; ++i)
			{
				GameObject child = transform.GetChild(i).gameObject;

				TileController tile = child.GetComponent<TileController>();
				ObjectController obj = child.GetComponent<ObjectController>();

				if (tile != null)
				{
					tmpTiles.Add(tile);

					if (x < tile.x)
						x = tile.x;

					if (z < tile.z)
						z = tile.z;
				}
				else if (obj != null)
				{
					AddObject(obj);
				}
			}

			tiles = new TileController[x + 1, z + 1];

			foreach (var tile in tmpTiles)
			{
				tiles[tile.x, tile.z] = tile;
			}

			tmpTiles.Clear();
		}

		public void Reset(int mode = 0)
		{
            if (mode == 0)
			    tiles = null;

			objects.Clear();

			// Destroy all children
			int limit = transform.childCount;
			while (transform.childCount > 0 && limit >= 0)
			{
				limit -= 1;
				Object.DestroyImmediate(transform.GetChild(0).gameObject);
			}

			if (limit == -1)
				Debug.Log("Error");

            if (mode == 0)
            {
                var tmp = GameObject.FindObjectOfType<IsoObjectPrefabController>();
                Object.DestroyImmediate(tmp);
            }

        }

		public void GeneraterWalls()
		{
			if (HasWalls)
			{
				if (gameObject.transform.Find("Walls") == null)
					print("false");
				if (WallsFrom.y < 0)
					WallsFrom.y = 0;
				if (WallsTo.y > tilesZ - 1)
					WallsTo.y = tilesZ - 1;
				if (WallsFrom.x < 0)
					WallsFrom.x = 0;
				if (WallsTo.x > tilesX - 1)
					WallsTo.x = tilesX - 1;

				GameObject temp = new GameObject("Walls");
				temp.transform.SetParent(gameObject.transform);
				temp.transform.position = Vector3.zero;
				temp.AddComponent<ObjectController>();
				temp.GetComponent<ObjectController>().tilesX = 0;
				temp.GetComponent<ObjectController>().tilesX = 0;
				float x = WallsFrom.x - 0.5f;
				float y = WallsFrom.y - 0.5f;
				CreateWall(x, WallsTo.x + 0.5f, y, y, false, 3, temp);
				CreateWall(x, x, y, WallsTo.y + 0.5f, true, 3, temp);
				CreateWall(WallsTo.x + 0.5f, WallsTo.x + 0.5f, y, WallsTo.y + 0.5f, true, 3, temp);
				CreateWall(x, WallsTo.x + 0.5f, WallsTo.y + 0.5f, WallsTo.y + 0.5f, false, 3, temp);

			}

		}

		private GameObject CreateWall(float xfrom, float xto, float yfrom, float yto, bool vertical, int height, GameObject parent)
		{
			GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
			if (!vertical)
				temp.transform.position = new Vector3((xfrom + xto) / 2, height / 4.0f, yfrom);
			else
				temp.transform.position = new Vector3(xfrom, height / 4.0f, (yfrom + yto) / 2);

			if (vertical)
				temp.transform.rotation = Quaternion.Euler(0, 90, 0);
			temp.transform.localScale = new Vector3(Mathf.Abs(xto - xfrom + yto - yfrom), height / 2.0f, 1);
			temp.transform.SetParent(parent.transform);

			return temp;
		}


		public void Update()
		{
			if (tiles != null)
			{
				foreach (var tile in tiles)
				{
					tile.GameObject = null;
				}

				foreach (var obj in objects)
				{
					obj.isValid = false;

					if (obj.startX < margin || obj.startX + margin + obj.tilesX > tiles.GetLength(0) || obj.startZ < margin || obj.startZ + margin + obj.tilesZ > tiles.GetLength(1))
						continue;

					// Check if can be added
					bool isFree = true;
					for (int x = 0; x < obj.tilesX; ++x)
					{
						for (int z = 0; z < obj.tilesZ; ++z)
						{
							if (tiles[x + obj.startX, z + obj.startZ].GameObject != null)
							{
								isFree = false;
								break;
							}
						}
					}

					if (!isFree)
						continue;

					obj.isValid = true;

					// Add
					for (int x = 0; x < obj.tilesX; ++x)
					{
						for (int y = 0; y < obj.tilesZ; ++y)
						{
							tiles[x + obj.startX, y + obj.startZ].GameObject = obj;
						}
					}
				}
			}
		}

		public void AddObject(ObjectController objController)
		{
			objects.Add(objController);
			objController.isValid = false;

			Update();
		}

		public void RemoveObject(ObjectController objController)
		{
			objects.Remove(objController);
			objController.isValid = false;

			Update();
		}

		public void Allign()
		{
			foreach (var obj in objects)
			{
				if (!obj.isValid || obj.name == "Walls")
					continue;

				float x = obj.startX + (obj.tilesX - 2) * 0.5f + 0.5f;
				float z = obj.startZ + (obj.tilesZ - 2) * 0.5f + 0.5f;

				obj.transform.position = new Vector3(x, obj.transform.position.y, z);
			}
		}

		public void CreateObject()
		{
			Reset();

			tiles = new TileController[tilesX, tilesZ];

			for (int i = 0; i < tiles.GetLength(0); ++i)
			{
				for (int j = 0; j < tiles.GetLength(1); ++j)
				{
					GameObject gameObject = (GameObject)Object.Instantiate(tile, new Vector3(i, 0, j), tile.transform.rotation);

					gameObject.transform.SetParent(transform);
					gameObject.name = "Tile [" + i + "," + j + ",0]";

					gameObject.SetActive(true);

					TileController tileController = tiles[i, j] = gameObject.GetComponent<TileController>();

					tileController.x = i;
					tileController.z = j;
					tileController.creatorController = this;
				}
			}
		}

        public TileController[,] CreateObjectForCopy(int width, int height)
        {

            TileController[,] tiles2 = new TileController[width,height];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    GameObject gameObject = (GameObject)Object.Instantiate(tile, new Vector3(i, 0, j), tile.transform.rotation);

                    gameObject.transform.SetParent(transform);
                    gameObject.name = "Tile [" + i + "," + j + ",0]";

                    gameObject.SetActive(true);

                    tiles2[i, j] = gameObject.GetComponent<TileController>();

                    tiles2[i, j].x = i;
                    tiles2[i, j].z = j;
                    tiles2[i, j].creatorController = this;
                }
            }

            return tiles2;
        }


    public void SavePrefab(int mode = 0)
		{
            string current_path = AssetDatabase.GetAssetPath(LoadingPrefab);

            string name = PrefabName;

			IsoObjectPrefabData prefabData = ScriptableObject.CreateInstance<IsoObjectPrefabData>();


			List<IsoObjectPrefabData.SpotData> spotsData = new List<IsoObjectPrefabData.SpotData>();

                prefabData.tilesX = tiles.GetLength(0);
                prefabData.tilesY = tiles.GetLength(1);
                prefabData.hasWalls = HasWalls;
                prefabData.rotation = rotation;
                IsoWallData walls = new IsoWallData();
                walls.wallsfrom = new Vector2i((int)WallsFrom.x, (int)WallsFrom.y);
                walls.wallsTo = new Vector2i((int)WallsTo.x, (int)WallsTo.y);
                walls.InnerWallType = (WallType)InnerWallID;
                walls.OuterWallType = (WallType)OuterWallID;
                walls.DoorID = DoorID;
                walls.doors = new List<int> { DoorPosition };
                walls.InnerWindowID = InnerWindowsID;
                walls.OuterWindowID = OuterWindowID;
                walls.windows = WindowsPosition;
                prefabData.walls = walls;
                prefabData.area = area;
                prefabData.rotationPoint = rotationPoint;

            prefabData.tilesData = new IsoObjectPrefabData.TileData[prefabData.tilesX * prefabData.tilesY];
			for (int i = 0; i < tiles.GetLength(0); ++i)
			{
				for (int j = 0; j < tiles.GetLength(1); ++j)
				{
					Sprite sprite = tiles[i, j].GetComponent<SpriteRenderer>().sprite;

					IsoObjectPrefabData.TileData tileData = new IsoObjectPrefabData.TileData();
					tileData.isPassable = tiles[i, j].IsPassable;
                    prefabData.tilesData[i * prefabData.tilesY + j] = tileData;

					if (sprite != defaultSprite)
					{
						SpriteData spriteData = null;
						foreach (var s in sprites)
						{
							if (s.sprite == sprite)
							{
								spriteData = s;
								break;
							}
						}

						if (spriteData != null)
						{
							tileData.layers = new IsoObjectPrefabData.LayerData[1];

							tileData.layers[0] = new IsoObjectPrefabData.LayerData();

							tileData.layers[0].isTransparent = spriteData.isTransparent;
							tileData.layers[0].spriteID = spriteData.spriteID;
						}
					}

					if (tiles[i, j].spotDirection != TileController.SpotDirection.None)
					{
						IsoObjectPrefabData.SpotData spotData = new IsoObjectPrefabData.SpotData();

						spotData.x = i;
						spotData.y = j;
						spotData.id = (int)tiles[i, j].spotID;
						switch (tiles[i, j].spotDirection)
						{
							case TileController.SpotDirection.NegativeX:
								spotData.direction = new Vector2(-1, 0);
								break;

							case TileController.SpotDirection.PositiveX:
								spotData.direction = new Vector2(1, 0);
								break;

							case TileController.SpotDirection.NegativeZ:
								spotData.direction = new Vector2(0, -1);
								break;

							case TileController.SpotDirection.PositiveZ:
								spotData.direction = new Vector2(0, 1);
								break;

							default:
								spotData.direction = new Vector2(0, 0);
								break;
						}

						spotsData.Add(spotData);
					}
				}
			}
			prefabData.spotsData = spotsData.ToArray();

            if (mode == 0)
            {
                AssetDatabase.CreateAsset(prefabData, "Assets/" + name + ".asset");

                GameObject prefab = new GameObject(name, typeof(IsoObjectPrefabController));
                prefab.GetComponent<IsoObjectPrefabController>().prefabData = prefabData;
                prefab.SetActive(true);

                if (mode == 0)
                {
                    foreach (var obj in objects)
                    {
                        if (!obj.isValid)
                            continue;

                        GameObject gameObject = GameObject.Instantiate(obj.gameObject);
                        Component.DestroyImmediate(gameObject.GetComponent<ObjectController>());
                        gameObject.name = obj.name;
                        gameObject.transform.SetParent(prefab.transform);
                    }
                }

                AssetDatabase.SaveAssets();

                PrefabUtility.CreatePrefab("Assets/" + prefab.name + ".prefab", prefab, ReplacePrefabOptions.ReplaceNameBased);




                GameObject.DestroyImmediate(prefab);

            }
            else
            {
                AssetDatabase.CreateAsset(prefabData, loaded_path + ".asset");
                AssetDatabase.SaveAssets();


                GameObject prefab = new GameObject(name, typeof(IsoObjectPrefabController));
                prefab.GetComponent<IsoObjectPrefabController>().prefabData = prefabData;
                prefab.SetActive(true);
                foreach (var obj in objects)
                {
                    if (obj.GetComponent<ObjectController>() != null)
                    {
                        GameObject gameObject = GameObject.Instantiate(obj.gameObject);
                        Component.DestroyImmediate(gameObject.GetComponent<ObjectController>());
                        gameObject.name = obj.name;
                        gameObject.transform.SetParent(prefab.transform);
                    }
                }
                PrefabUtility.CreatePrefab(loaded_path + ".prefab", prefab, ReplacePrefabOptions.ReplaceNameBased);

                GameObject.DestroyImmediate(prefab);
        }


            if (mode != 0) LoadingPrefab =  (GameObject)AssetDatabase.LoadAssetAtPath(current_path, typeof(GameObject));
        }


		public void LoadPrefab()
		{
			GameObject gameObject = GameObject.Instantiate(LoadingPrefab);
			IsoObjectPrefabData prefabData = gameObject.GetComponent<IsoObjectPrefabController>().prefabData;

            loaded_path = AssetDatabase.GetAssetPath(LoadingPrefab);

            string tmpName = ".asset";
            loaded_path = loaded_path.Remove(loaded_path.Length - (tmpName.Length + 1));
            Debug.Log("Loaded: " + loaded_path);

            this.tilesX = prefabData.tilesX;
			this.tilesZ = prefabData.tilesY;
			CreateObject();

            prefabData.tilesX = tiles.GetLength(0);
            prefabData.tilesY = tiles.GetLength(1);
            HasWalls = prefabData.hasWalls;
            rotation = prefabData.rotation;
            WallsFrom = new Vector2(prefabData.walls.wallsfrom.x, prefabData.walls.wallsfrom.y);
            WallsTo = new Vector2(prefabData.walls.wallsTo.x, prefabData.walls.wallsTo.y);
            InnerWallID = (int)prefabData.walls.InnerWallType;
            OuterWallID = (int)prefabData.walls.OuterWallType;
            DoorID = prefabData.walls.DoorID;
            DoorPosition = prefabData.walls.doors[0];
            InnerWindowsID = prefabData.walls.InnerWindowID;
            OuterWindowID = prefabData.walls.OuterWindowID;
            WindowsPosition = prefabData.walls.windows;
            area = prefabData.area;
            rotationPoint = prefabData.rotationPoint;
        // Set tile data
            for (int i = 0; i < this.tilesX; ++i)
			{
				for (int j = 0; j < tilesZ; ++j)
				{
					IsoObjectPrefabData.TileData tileData = prefabData.tilesData[i * this.tilesZ + j];

					tiles[i, j].IsPassable = tileData.isPassable;

                    Sprite sprite = null;
					foreach (var s in sprites)
					{
						if (tileData.layers.Length > 0)
							if (s.spriteID == tileData.layers[0].spriteID)
							{
								sprite = s.sprite;
								break;
							}
					}

					if (sprite != null)
						tiles[i, j].GetComponent<SpriteRenderer>().sprite = sprite;
				}
			}

			// Set spots
			foreach (var spot in prefabData.spotsData)
			{
				if (spot.direction.x == 0.0f && spot.direction.y == 1.0f)
				{
					tiles[spot.x, spot.y].spotDirection = TileController.SpotDirection.PositiveZ;
				}
				if (spot.direction.x == 0.0f && spot.direction.y == -1.0f)
				{
					tiles[spot.x, spot.y].spotDirection = TileController.SpotDirection.NegativeZ;
				}
				if (spot.direction.x == 1.0f && spot.direction.y == 0.0f)
				{
					tiles[spot.x, spot.y].spotDirection = TileController.SpotDirection.PositiveX;
				}
				if (spot.direction.x == -1.0f && spot.direction.y == 0.0f)
				{
					tiles[spot.x, spot.y].spotDirection = TileController.SpotDirection.NegativeX;
				}

				tiles[spot.x, spot.y].SetSpot();
                tiles[spot.x, spot.y].spotID = (SpotTypes)spot.id;
            }

        // Set objects

            Vector3 centerPoint = new Vector3(5, 0, 0);

            while (gameObject.transform.childCount > 0)
			{
                gameObject.transform.position = centerPoint;
                GameObject child = gameObject.transform.GetChild(0).gameObject;

				child.AddComponent<ObjectController>();
				child.transform.SetParent(transform);
                //child.transform.position = new Vector3(child.transform.position.x - 25, 0, child.transform.position.z - 20);
			}

			GameObject.DestroyImmediate(gameObject);
		}

    public void TransformContent(int x, int y, bool removeTileX = false, bool removeTileY = false)
    {
        Reset();

        GameObject gameObject = GameObject.Instantiate(LoadingPrefab);
        IsoObjectPrefabData prefabData = gameObject.GetComponent<IsoObjectPrefabController>().prefabData;

        int removeXval = 0;
        int removeYval = 0;

        if (removeTileX == false)
        {
            removeXval = 0;
        }
        else
        {
            removeXval = -1;
        }


        if (removeTileY == false)
        {
            removeYval = 0;
        }
        else
        {
            removeYval = -1;
        }


        this.tilesX = prefabData.tilesX + removeXval;
        this.tilesZ = prefabData.tilesY + removeYval;

        CreateObject();

        // Set tile data
        for (int i = (x >= 0 ? 0 : 1); i < (x > 0 ? tilesX - x : tilesX); ++i)
        {
            for (int j = (y >= 0 ? 0 : 1); j < (y>0 ? tilesZ - y : tilesZ); ++j)
            {
                IsoObjectPrefabData.TileData tileData = prefabData.tilesData[i * this.tilesZ + j];

                tiles[i + (x > 0 ? 1 : (x != 0) ? - 1 : 0), j + (y > 0 ? 1 : (y != 0) ? -1 : 0)].IsPassable = tileData.isPassable;
                Sprite sprite = null;
                foreach (var s in sprites)
                {
                    if (tileData.layers.Length > 0)
                        if (s.spriteID == tileData.layers[0].spriteID)
                        {
                            sprite = s.sprite;
                            break;
                        }
                }

                if (sprite != null)
                    tiles[i + (x > 0 ? 1 : (x != 0) ? -1 : 0), j + (y > 0 ? 1 : (y != 0) ? -1 : 0)].GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }

 
        // Set spots
        foreach (var spot in prefabData.spotsData)
        {
            try
            {
                if (y > 0 && spot.y >= this.tilesZ - 1) continue;
                if (y < 0 && spot.y <= 0) continue;
                if (x > 0 && spot.x >= this.tilesX - 1) continue;
                if (x < 0 && spot.x <= 0) continue;

                if (spot.direction.x == 0.0f && spot.direction.y == 1.0f)
                {
                    tiles[spot.x + (x > 0 ? 1 : (x != 0) ? -1 : 0), spot.y + (y > 0 ? 1 : (y != 0) ? -1 : 0)].spotDirection = TileController.SpotDirection.PositiveZ;
                }
                if (spot.direction.x == 0.0f && spot.direction.y == -1.0f)
                {
                    tiles[spot.x + (x > 0 ? 1 : (x != 0) ? -1 : 0), spot.y + (y > 0 ? 1 : (y != 0) ? -1 : 0)].spotDirection = TileController.SpotDirection.NegativeZ;
                }
                if (spot.direction.x == 1.0f && spot.direction.y == 0.0f)
                {
                    tiles[spot.x + (x > 0 ? 1 : (x != 0) ? -1 : 0), spot.y + (y > 0 ? 1 : (y != 0) ? -1 : 0)].spotDirection = TileController.SpotDirection.PositiveX;
                }
                if (spot.direction.x == -1.0f && spot.direction.y == 0.0f)
                {
                    tiles[spot.x + (x > 0 ? 1 : (x != 0) ? -1 : 0), spot.y + (y > 0 ? 1 : (y != 0) ? -1 : 0)].spotDirection = TileController.SpotDirection.NegativeX;
                }

                tiles[spot.x + (x > 0 ? 1 : (x != 0) ? -1 : 0), spot.y + (y > 0 ? 1 : (y != 0) ? -1 : 0)].SetSpot();
                tiles[spot.x + (x > 0 ? 1 : (x != 0) ? -1 : 0), spot.y + (y > 0 ? 1 : (y != 0) ? -1 : 0)].spotID = (SpotTypes)spot.id;

            }
            catch(System.IndexOutOfRangeException ex)
            {
                Debug.Log("Deledted over array: " + ex.ToString());
            }
        }


        // Set objects
        while (gameObject.transform.childCount > 0)
        {
            GameObject child = gameObject.transform.GetChild(0).gameObject;

            child.AddComponent<ObjectController>();
            child.transform.SetParent(transform);
        }

        GameObject.DestroyImmediate(gameObject);

    }

    public void ChangeContentLineOnSide(Side side, bool doDelete = true)
    {
        if (side != Side.None)
        {
            if (side == Side.East) TransformContent(0, -1);
            if (side == Side.South) TransformContent(-1, 0);
            //if (side == DeleteSide.West) TransformContent(0, 1);

            Reset(1);

            TileController[,] tiles2;

            if (side == Side.West || side == Side.East)
            {
                if (doDelete)
                    tilesZ--;
                else tilesZ++;
            }

            if (side == Side.North || side == Side.South)
            {
                if (doDelete)
                    tilesX--;
                else tilesX++;
            }

            tiles2 = CreateObjectForCopy(tilesX, tilesZ);

            for (int i = 0; i < tilesX; ++i)
            {
                for (int j = 0; j < tilesZ; ++j)
                {
                    tiles2[i, j].creatorController = tiles[i, j].creatorController;
                    tiles2[i, j].x = tiles[i, j].x;
                    tiles2[i, j].z = tiles[i, j].z;
                    tiles2[i, j].IsPassable = tiles[i, j].IsPassable;

                    tiles2[i, j].spotDirection = tiles[i, j].spotDirection;
                    tiles2[i, j].spotID = tiles[i, j].spotID;

                    if (tiles[i, j].spotDirection != TileController.SpotDirection.None)
                    {
                        tiles2[i, j].SetSpot();
                    }
                }
            }

            tiles = tiles2;
        }
    }



}
#endif