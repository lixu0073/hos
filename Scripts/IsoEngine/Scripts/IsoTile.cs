using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace IsoEngine
{

	/// <summary>
	/// Represents tile of the level. Stores information about particular tile.
	/// </summary>
	public class IsoTile : IsoElement, ILoadable
	{
		// Fields
		/// <summary>
		/// The ID of the level that the tile belongs to
		/// </summary>
		public int LevelID;

		/// <summary>
		/// The x coordinate of the tile on a level.
		/// </summary>
		public int X;

		/// <summary>
		/// The y coordinate of the object on a level.
		/// </summary>
		public int Y;

		/// <summary>
		/// When set to true the tile will be unloaded when the floor it belongs to is not the currently loaded floor.
		/// Whet set to false the tile will stay loaded even when it belongs to the floor other than currently loaded one.
		/// </summary>
		public bool IsInterior;

		private SortedList<int, IsoLayer> layers;


		private IsoObject isoObject;
		private int[] isoObjectLayersHeight;

        // Properties
        /// <summary>
        /// Tells whether the tile is loaded into memoory or not.
        /// When set to false the tile is not displayed and updated.
        /// </summary>
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

        public Hospital.PathType[] pathTypes = new Hospital.PathType[1] { Hospital.PathType.Default };

        /// <summary>
        /// Object that is located on the tile.
        /// </summary>
        public IsoObject IsoObject
		{
			get
			{
				return isoObject;
			}

			internal set
			{
				if (isoObject == value)
					return;

				// Remove layers from old object
				if (isoObject != null)
				{
                    pathTypes = new Hospital.PathType[1] {Hospital.PathType.Default};
                    foreach (int layerHeight in isoObjectLayersHeight)
						RemoveLayer(layerHeight);
				}

				isoObject = value;

				// Add layers from new object
				if (isoObject != null)
				{
                    pathTypes = new Hospital.PathType[1] {Hospital.PathType.Default};
                    IsoObjectPrefabData.TileData tileData = isoObject.GetTileData(this.X, this.Y);

					if (tileData.layers == null)
						return;

					isoObjectLayersHeight = new int[tileData.layers.Length];

					int maxH = -1;
					if (layers.Count > 0)
						maxH = layers.Keys[layers.Count - 1];

					for (int i = 0; i < tileData.layers.Length; ++i)
					{
						IsoLayerData layerData = new IsoLayerData();
						layerData.h = (++maxH);
						layerData.textureID = tileData.layers[i].spriteID;
						layerData.transparent = tileData.layers[i].isTransparent;

						AddLayer(CreateLayer(layerData, true));

						isoObjectLayersHeight[i] = layerData.h;
					}
				}
			}
		}

		private IsoLayer CreateLayer(IsoLayerData layerData, bool objectPart = false)
		{
			switch (engineController.Map.TileType)
			{
				case IsoTileType.Sprites:
					{
						return new IsoSpriteLayer(engineController, this, layerData);
					}

				case IsoTileType.Quads:
					{
						return new IsoQuadLayer(engineController, this, layerData);
					}

				default:
					{
						throw new IsoException("Invalid tile type");
					}
			}
		}

		// Methods
		internal IsoTile(EngineController engineController, int levelID, IsoTileData tileData)
			: base(engineController)
		{
			this.layers = new SortedList<int, IsoLayer>();

			this.LevelID = levelID;

			this.X = tileData.x;
			this.Y = tileData.y;

			this.IsInterior = tileData.isInterior;

			this.IsLoaded = false;

			// Create layers
			if (tileData.isoLayers != null)
				foreach (var layerData in tileData.isoLayers)
					AddLayer(layerData);

			this.IsCreated = true;
		}

        //write data to an existing tile
        internal void Init(EngineController engineController, int levelID, IsoTileData tileData)
        {
            this.engineController = engineController;

            this.layers = new SortedList<int, IsoLayer>();

            this.LevelID = levelID;

            this.X = tileData.x;
            this.Y = tileData.y;

            this.IsInterior = tileData.isInterior;

            this.IsLoaded = false;

            // Create layers
            if (tileData.isoLayers != null)
                foreach (var layerData in tileData.isoLayers)
                    AddLayer(layerData);

            this.IsCreated = true;
        }

		/// <summary>
		/// Loads the tile to the memory. It makes it visible on the map. The tile will start getting updates.
		/// Throws IsoException when invoked on already loaded tile.
		/// </summary>
		public void Load()
		{
			if (IsLoaded)
				throw new IsoException("Tile is already loaded");

			int solidIndex = 0;
			for (int i = layers.Count - 1; i >= 0; --i)
			{
				layers[i].Load();

				// Break if found first solid layer
				if (!layers[i].IsTransparent)
				{
					solidIndex = i;
					break;
				}
			}

			// Set heights
			for (int i = solidIndex; i < layers.Count; ++i)
				layers[i].SortingHeight = i - solidIndex;

			IsLoaded = true;
		}

		/// <summary>
		/// Unloads the tile from the memory. It makes it invisible on the map. The tile will not get any updates.
		/// Throws IsoException when invoked on already unloaded object.
		/// </summary>
		public void Unload()
		{
			if (!IsLoaded)
				throw new IsoException("Tile is already unloaded");

			for (int i = layers.Count - 1; i >= 0; --i)
			{
				// Break if found first unloaded
				if (!layers[i].IsLoaded)
					break;

				layers[i].Unload();
			}

			IsLoaded = false;
		}

		/// <summary>
		/// Destroys tile and unloads it from memory. It is forbidden to use the destroyed tile as it's data might be inconsistent.
		/// </summary>
		public void IsoDestroy()
		{
			if (!IsCreated)
				throw new IsoException("Tile is already destroyed");

			this.isoObject = null;
			this.isoObjectLayersHeight = null;

			for (int i = 0; i < layers.Count; ++i)
				layers.Values[i].IsoDestroy();

			layers.Clear();

			IsCreated = false;
		}

		private void AddLayer(IsoLayer layer)
		{
			if (layers.Keys.Contains(layer.Height))
				throw new IsoException("Layer with the specified height already exists");

			layers.Add(layer.Height, layer);

			// Return if tile is unloaded
			if (!IsLoaded)
				return;

			layer.SortingHeight = 0;

			// Find inserted index
			int currentIndex = layers.IndexOfKey(layer.Height);

			// Find lower solid layer
			int lowerIndex = currentIndex - 1;
			while (lowerIndex >= 0 && layers.Values[lowerIndex].IsTransparent)
				lowerIndex -= 1;

			// Find upper solid layer
			int upperIndex = currentIndex + 1;
			while (upperIndex < layers.Values.Count && layers.Values[upperIndex].IsTransparent)
				upperIndex += 1;

			// There is no solid layer above
			if (upperIndex >= layers.Count)
			{
				if (lowerIndex < 0)
					lowerIndex = 0;

				// If it is solid
				int h = currentIndex - lowerIndex;
				if (!layer.IsTransparent)
				{
					// Cover layers bellow inserted
					h = 0;
					if (IsLoaded)
						for (int i = lowerIndex; i < currentIndex; ++i)
							layers.Values[i].Unload();
				}

				// Set sorting order to transparent layers above inserted
				for (int i = currentIndex; i < layers.Count; ++i, ++h)
					layers.Values[i].SortingHeight = h;

				if (IsLoaded)
					layer.Load();
			}
		}




		/// <summary>
		/// Adds new layer to the tile.
		/// The IsoExeption is thrown when there is already a layer present on specified height.
		/// </summary>
		/// <param name="layerData">Object describing the layer.</param>
		public void AddLayer(IsoLayerData layerData)
		{
			AddLayer(CreateLayer(layerData));
		}

		/// <summary>
		/// Removes a layer from specified height
		/// The IsoException is thrown when there is no layers on a specified height.
		/// </summary>
		/// <param name="height">The height of a layer to be removed</param>
		public void RemoveLayer(int height)
		{
			if (!layers.Keys.Contains(height))
				throw new IsoException("Layer with specified height does not exist");

			// Find layer to remove
			int currentIndex = layers.IndexOfKey(height);
			IsoLayer layer = layers.Values[currentIndex];

			layer.IsoDestroy();

			if (IsLoaded)
			{
				// Find lower solid layer
				int lowerIndex = currentIndex - 1;
				while (lowerIndex >= 0 && layers.Values[lowerIndex].IsTransparent)
					lowerIndex -= 1;

				// Find upper solid layer
				int upperIndex = currentIndex + 1;
				while (upperIndex < layers.Count && layers.Values[upperIndex].IsTransparent)
					upperIndex += 1;

				// There is no layer above removed layer
				if (upperIndex >= layers.Count)
				{
					if (lowerIndex < 0)
						lowerIndex = 0;

					if (!layer.IsTransparent)
					{
						// Uncover all alpha layers between lower and found
						for (int i = lowerIndex; i < currentIndex; ++i)
							layers.Values[i].Load();
					}

					// Correct all the alpha layers heights above lower
					int h = 0;

					for (int i = lowerIndex; i < currentIndex; ++i, ++h)
						layers.Values[i].SortingHeight = h;

					for (int i = currentIndex + 1; i < layers.Values.Count; ++i, ++h)
						layers.Values[i].SortingHeight = h;
				}
			}

			layers.RemoveAt(currentIndex);
		}

		/// <summary>
		/// Returns IsoTileData object containing data of the tile ready to be used in serialization or creating other tiles.
		/// </summary>
		/// <returns>IsoTileData object containing data of the tile</returns>
		public IsoTileData GetData()
		{
			IsoTileData tileData = new IsoTileData(X, Y);

			tileData.isInterior = IsInterior;

			if (layers.Count != 0)
			{
				List<IsoLayerData> layersData = new List<IsoLayerData>();
				for (int i = 0; i < layers.Values.Count; ++i)
					if (!layers.Values[i].IsObjectPart)
						layersData.Add(layers.Values[i].GetData());

				tileData.isoLayers = layersData.ToArray();
			}
			else
			{
				tileData.isoLayers = null;
			}

			return tileData;
		}
	}
}