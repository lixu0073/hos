using UnityEngine;
using System.Collections.Generic;

namespace IsoEngine
{
	/// <summary>
	/// Base class to controll information about map. All more advanced structures derives from this class.
	/// </summary>
	public abstract class BaseMapController : ComponentController
	{
		// Types
		public delegate void MouseEventHandler(int levelID);
		public delegate void ChangedFloorEventHandler(int from, int to);

		// Fields
		[SerializeField]
		private float levelHeight = 1;

		public float LevelHeight
		{
			get;
			protected set;
		}
		
		public int LevelCount
		{
			get;
			protected set;
		}

		protected int currentLevel;

		public IsoTileType TileType
		{
			get;
			protected set;
		}

		protected virtual bool CheckIsoLevelControllerType(System.Type isoMapControllerType)
		{
			return typeof(BaseIsoLevelController).IsAssignableFrom(isoMapControllerType);
		}
		protected virtual System.Type GetIsoLevelControllerType()
		{
			return typeof(BaseIsoLevelController);
		}
		private System.Type isoMapControllerType;
		private System.Type IsoMapControllerType
		{
			get
			{
				if (isoMapControllerType == null)
					isoMapControllerType = GetIsoLevelControllerType();
				return isoMapControllerType;
			}
		}

		/// <summary>
		/// Level that is currently active
		/// </summary>
		public int CurrentLevel
		{
			get
			{
				return currentLevel;
			}
		}

		public bool IsCreated
		{
			get;
			private set;
		}

		/// <summary>
		/// Creates all needed data about map from data.
		/// </summary>
		/// <param name="mapData"></param>
		public virtual void CreateMap(IsoMapData mapData)
		{
			if (IsCreated)
				throw new IsoException("Map is already created");

			this.TileType = mapData.tileType;

			LevelCount = mapData.levelData.Length;
			Levels = new BaseIsoLevelController[LevelCount];

			for (int i = 0; i < Levels.Length; ++i)
			{
				GameObject gameObject = new GameObject("Map Level " + i.ToString(), IsoMapControllerType, typeof(BoxCollider));
				gameObject.transform.SetParent(transform);
				gameObject.isStatic = true;

				int levelSizeX = mapData.levelData[i].tileData.GetLength(0);
				int levelSizeY = mapData.levelData[i].tileData.GetLength(1);

				BoxCollider collider = gameObject.GetComponent<BoxCollider>();
				collider.enabled = false;
				collider.size = new Vector3(levelSizeX, 0.01f, levelSizeY);
				collider.center = new Vector3(((float)levelSizeX - 1) / 2.0f, 0.0f, ((float)levelSizeY - 1) / 2.0f);

				BaseIsoLevelController level = (BaseIsoLevelController)gameObject.GetComponent(IsoMapControllerType);
				level.Initialize();

				Levels[i] = level;

				level.CreateLevel(mapData.levelData[i]);

				int levelID = i;
				level.OnMouseDownEvent += delegate()
				{
					if (OnMouseDownEvent != null)
						OnMouseDownEvent(levelID);
				};

				level.OnMouseUpEvent += delegate()
				{
					if (OnMouseUpEvent != null)
						OnMouseUpEvent(levelID);
				};
			}

			currentLevel = 0;
			IsCreated = true;
		}

		// Properies
		public bool IsLoaded
		{
			get;
			private set;
		}

		protected BaseIsoLevelController[] Levels;

		public TLevelController GetLevel<TLevelController>(int index)
			where TLevelController : BaseIsoLevelController
		{
            if (Levels==null || index >=Levels.Length)
            {
                return null as TLevelController;
            }
			else return Levels[index] as TLevelController;
		}

		public BaseIsoLevelController GetLevel(int index)
		{
			return Levels[index];
		}

		// Methods
		internal override void Initialize()
		{
			base.Initialize();

			//IsoMapControllerType = GetIsoLevelControllerType();

			Debug.Assert(CheckIsoLevelControllerType(IsoMapControllerType), "Invalid IsoMapController type");
			
			LevelHeight = levelHeight;

			IsCreated = false;
		}

		public virtual void Load()
		{
			if (IsLoaded)
				throw new IsoException("Map is already loaded");

			Levels[currentLevel].Load();

			IsLoaded = true;
			ReferenceHolder.Get().saveLoadManager.StartSaving();
		}

		public void Unload()
		{
			if (!IsLoaded)
				throw new IsoException("Map is already unloaded");

			Levels[currentLevel].Unload();

			IsLoaded = false;
		}

		/// <summary>
		/// Completly removes map from memory.
		/// </summary>
		public override void IsoDestroy()
		{
			if (!IsCreated)
				return;

			// TMP!!!
			Debug.Log("Map destroy");

			if (Levels != null)
			{
				foreach (var level in Levels)
				{
					level.IsoDestroy();
					GameObject.Destroy(level.gameObject);
				}

				Levels = null;

				IsLoaded = false;
			}

			IsCreated = false;
		}

		public virtual void OnTilePress(int levelID, Vector2i pressedTile)
		{
			if (OnTilePressEvent != null)
				OnTilePressEvent(levelID, pressedTile);
		}
		public virtual void OnTiletouch(int levelID, Vector2i pressedTile)
		{
			if (OnTileTouchedEvent != null)
				OnTileTouchedEvent(levelID, pressedTile);
		}

		public delegate void OnTilePressEventHandler(int levelID, Vector2i tile);
		public event OnTilePressEventHandler OnTilePressEvent;
		public event OnTilePressEventHandler OnTileTouchedEvent;

		/// <summary>
		/// Changes visible lvl.
		/// </summary>
		/// <param name="levelID">ID of lvl that will be visible now</param>
		public void ChangeLevel(int levelID)
		{
			if (levelID < 0 || levelID >= Levels.Length)
				throw new IsoException("Invalid level id");

			if (levelID == currentLevel)
				throw new IsoException("Tried switch to the same level");

			// Changing camera position
			Vector3 pos = engineController.MainCamera.LookingAt;
			pos.y = Levels[levelID].ActualLevelHeight;
			engineController.MainCamera.SmoothMoveToPoint(pos, 0.5f,true);

			// Switched to lower level
			if (levelID < currentLevel)
			{
				Levels[currentLevel].Unload();

				for (int i = currentLevel - 1; i > levelID; --i)
				{
					// Unload levels
					Levels[i].Unload();
					Levels[i].LoadInterior();
				}

				Levels[levelID].LoadInterior();
			}
			// Switched to upper level
			else
			{
				Levels[currentLevel].UnloadInterior();

				for (int i = currentLevel + 1; i < levelID; ++i)
				{
					// Load levels without floor
					Levels[i].UnloadInterior();
					Levels[i].Load();
				}

				Levels[levelID].Load();
			}

			int oldLevel = currentLevel;
			currentLevel = levelID;

			if (OnChangedFloor != null)
				OnChangedFloor(oldLevel, currentLevel);
		}

		/// <summary>
		/// Returns map data in serializable object.
		/// </summary>
		/// <returns></returns>
		public virtual IsoMapData GetData()
		{
			IsoMapData mapData = new IsoMapData();

			mapData.additionalData = null;
			mapData.tileType = TileType;
			mapData.levelData = new IsoLevelData[Levels.Length];

			for (int i = 0; i < mapData.levelData.Length; ++i)
			{
				mapData.levelData[i] = Levels[i].GetData();
			}

			return mapData;
		}

		public event MouseEventHandler OnMouseDownEvent;
		public event MouseEventHandler OnMouseUpEvent;

		public event ChangedFloorEventHandler OnChangedFloor;
	}
}
