using UnityEngine;
using System.Collections.Generic;
//using Hospital;

namespace IsoEngine
{
	/// <summary>
	/// Represents game object in the engine. Stores information about particular instance of an object.
	/// </summary>
	public class IsoObject : IsoElement, ILoadable
	{
		public abstract class Extension : ILoadable
		{
			protected GameObject gameObject
			{
				get
				{
					return isoObject.gameObject;
				}

				set
				{
					isoObject.gameObject = value;
				}
			}
			protected IsoObject isoObject;

			public bool IsLoaded
			{
				get;
				private set;
			}

			public bool IsCreated
			{
				get;
				private set;
			}

			public virtual void Load()
			{
				if (IsLoaded)
					throw new IsoException("Extension is already loaded");

				IsLoaded = true;
			}

			public virtual void Unload()
			{
				if (!IsLoaded)
					throw new IsoException("Extension is already unloaded");

				IsLoaded = false;
			}

			public virtual void IsoDestroy()
			{
				if (!IsCreated)
					throw new IsoException("Extesion is already destroyed");

				IsCreated = false;
			}

			public Extension(IsoObjectExtensionData extensionData, IsoObject isoObject)
			{
				this.isoObject = isoObject;
				this.IsCreated = true;
			}

			public abstract IsoObjectExtensionData GetData();
		}

		// Fields
		/// <summary>
		/// The ID of the level that the object belongs to.
		/// </summary>
		public readonly int LevelID;

		/// <summary>
		/// The x coordinate of the object on a level.
		/// </summary>
		public readonly int X;

		/// <summary>
		/// Y coordinate of the object on a level.
		/// </summary>
		public readonly int Y;

		/// <summary>
		/// Width of the object in tiles.
		/// </summary>
		public int W
		{
			get
			{
				return prefabData.tilesX;
			}
		}

		/// <summary>
		/// Height of the object in tiles.
		/// </summary>
		public int H
		{
			get
			{
				return prefabData.tilesY;
			}
		}

        public Hospital.HospitalArea Area
        {
            get
            {
                return prefabData.area;
            }
        }

        public bool IsCreated
		{
			get;
			private set;
		}

		public int ID
		{
			get
			{
				return prefabData.id;
			}
		}

		/// <summary>
		/// When set to true the object will be unloaded when the floor it belongs to is not the currently loaded floor.
		/// Whet set to false the object will stayed loaded even when it belongs to the floor other than currently loaded one.
		/// </summary>
		public readonly bool IsInterior;

		/// <summary>
		/// The ID of an object prefab specified in the engine.
		/// </summary>
		public readonly int objectID;

		private GameObject gameObject;
		private IsoObjectPrefabData prefabData;

		private QueueableSpot[] spots;

		private Extension objectExtension;

		public QueueableSpot GetSpot(int i)
		{
			return spots[i];
		}

		public GameObject GetGameObject()
		{
			return gameObject;
		}

        public bool isObjectHaveAnySpot()
        {
            if (spots == null || (spots != null && spots.Length == 0))
                return false;

            return true;
        }

        public QueueableSpot GetSpotOfType(int id)
		{
			foreach (var spot in spots)
			{
				if (spot.ID == id)
					return spot;
			}

			return null;
		}

		public QueueableSpot[] GetSpotsOfType(int id)
		{
			List<QueueableSpot> ret = new List<QueueableSpot>();

			foreach (var spot in spots)
			{
				if (spot.ID == id)
					ret.Add(spot);
			}

			return ret.ToArray();
		}

		public int SpotsCount
		{
			get
			{
				return spots.Length;
			}
		}

		// Properties
		/// <summary>
		/// Tells whether the object is loaded into memoory or not.
		/// When set to false the object is not displayed and updated.
		/// </summary>
		public bool IsLoaded
		{
			get;
			private set;
		}

		// Methods
		internal IsoObject(EngineController engineController, int levelID, IsoObjectData objectData, IsoObjectPrefabData prefabData, GameObject createFrom = null)
			: base(engineController)
		{
			this.LevelID = levelID;
			this.X = objectData.x;
			this.Y = objectData.y;

			this.prefabData = prefabData;

			this.IsInterior = objectData.isInterior;

			this.objectID = objectData.objectID;

			if (createFrom == null)
				this.gameObject = Object.Instantiate(engineController.objects[objectID]);
			else
				this.gameObject = createFrom;
			
			this.gameObject.transform.SetParent(engineController.Map.GetLevel(levelID).transform);
			this.gameObject.transform.localPosition = new Vector3(X, 0, Y);
			this.gameObject.SetActive(false);
			
			this.spots = new QueueableSpot[prefabData.spotsData.Length];
			for (int i = 0; i < this.spots.Length; ++i)
				this.spots[i] = new QueueableSpot(engineController, this, prefabData.spotsData[i]);
			
			this.objectExtension = null;

			this.IsLoaded = false;
			this.IsCreated = true;
		}

		/// <summary>
		/// Loads the object to the memory. It makes it visible on the map. The object will start getting updates.
		/// Throws IsoException when invoked on already loaded object.
		/// </summary>
		/// 

		/*public RotatableObject GetRotatableObject(){
			return gameObject.GetComponent<RotatableObject> ();
		}*/

		public void Load()
		{
			if (IsLoaded)
				throw new IsoException("The object is already loaded");

			if(objectExtension != null)
				objectExtension.Load();
			
			gameObject.SetActive(true);

			IsLoaded = true;
		}

		/// <summary>
		/// Unloads the object from the memory. It makes it invisible on the map. The object will not get any updates.
		/// Throws IsoException when invoked on already unloaded object.
		/// </summary>
		public void Unload()
		{
			if (!IsLoaded)
				throw new IsoException("The object is already unloaded");

			gameObject.SetActive(false);

			if(objectExtension != null)
				objectExtension.Unload();
			
			IsLoaded = false;
		}

		/// <summary>
		/// Destroys object and unloads it from memory. It is forbidden to use the destroyed object as it's data might be inconsistent.
		/// </summary>
		public void IsoDestroy()
		{
			GameObject.Destroy(LightDestroy());
		}

		/// <summary>
		/// Destroys object and unloads it from memory. It is forbidden to use the destroyed object as it's data might be inconsistent.
		/// </summary>
		public GameObject LightDestroy()
		{
			if (!IsCreated)
				throw new IsoException("Object is already destroyed");

			if (objectExtension != null)
				objectExtension.IsoDestroy();

			this.IsCreated = false;

			return gameObject;
		}

		public void SetExtension(Extension objectExtension)
		{
			if (this.objectExtension != null)
				throw new IsoException("Object extension already present");

			if (IsLoaded)
				objectExtension.Load();

			this.objectExtension = objectExtension;
		}

		public Extension Get()
		{
			return objectExtension;
		}

		public TIsoObjectExtension Get<TIsoObjectExtension>()
			where TIsoObjectExtension : Extension
		{
			return objectExtension as TIsoObjectExtension;
		}

		/// <summary>
		/// Returns IsoObjectData object containing data of the object ready to be used in serialization or creating other objects.
		/// </summary>
		/// <returns>IsoObjectData object containing data of the object</returns>
		public IsoObjectData GetData()
		{
			IsoObjectData objectData = new IsoObjectData();

			objectData.x = X;
			objectData.y = Y;
			objectData.isInterior = IsInterior;
			objectData.objectID = objectID;

			if (objectExtension != null)
				objectData.extensionData = objectExtension.GetData();

			return objectData;
		}


		/// <summary>
		/// Returns the TileData object containing information about the tile specified by the x and y coordinates. The x and y coordinates have to be global coordinates on the tile map.
		/// </summary>
		/// <param name="x">The global x coordiante of a tile</param>
		/// <param name="y">The global y coordinate of a tile</param>
		/// <returns>TileData object containing information about the tile</returns>
		internal IsoObjectPrefabData.TileData GetTileData(int x, int y)
		{
			x -= this.X;
			y -= this.Y;

			return prefabData.tilesData[x * prefabData.tilesY + y];
		}
	}
}