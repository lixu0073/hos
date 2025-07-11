using UnityEngine;
using System.Collections;


namespace IsoEngine
{
	/// <summary>
	/// Represents ground texture displayed on a single tile. Single tile can have multiple layers stacked on top of each other.
	/// </summary>
	public abstract class IsoLayer : IsoElement, ILoadable
	{
		// Fields
		/// <summary>
		/// Identifies how high the layer is stacked on a tile.
		/// </summary>
		public readonly int Height;

		/// <summary>
		/// The ID of an texture specified in the engine.
		/// </summary>
		public readonly int SpriteID;

		/// <summary>
		/// Tells weather the layer is transparent or not.
		/// When set to false the layer will cover all the layers beneath on the layer stack of a tile.
		/// </summary>
		public readonly bool IsTransparent;

		/// <summary>
		/// The tile that the layer is displayed on.
		/// </summary>
		public readonly IsoTile isoTile;

		/// <summary>
		/// Tells whether the tile is the part of an game object.
		/// When set to true the layer is managed by the object, not the tile.
		/// </summary>
		public readonly bool IsObjectPart;

		public bool IsCreated
		{
			get;
			private set;
		}

		internal abstract int SortingHeight
		{
			get;
			set;
		}

		protected GameObject layerObject;

		// Properties
		// Properties
		/// <summary>
		/// Tells whether the layer is loaded into memoory or not.
		/// When set to false the layer is not displayed.
		/// </summary>
		public bool IsLoaded
		{
			get;
			protected set;
		}

		static protected float Spacing = 0.01f;

		// Methods
		internal IsoLayer(EngineController engineController, IsoTile isoTile, IsoLayerData tileData, bool isObjectPart = false)
			: base(engineController)
		{
			this.isoTile = isoTile;

			this.Height = tileData.h;

			this.SpriteID = tileData.textureID;
			this.IsTransparent = tileData.transparent;

			
			this.IsObjectPart = isObjectPart;

			this.IsLoaded = false;

			this.IsCreated = true;
		}

		/// <summary>
		/// Loads the layer to the memory. It makes it visible on the map.
		/// Throws IsoException when invoked on already loaded layer.
		/// </summary>
		public abstract void Load();

		/// <summary>
		/// Unloads the layer from the memory. It makes it invisible on the map.
		/// Throws IsoException when invoked on already unloaded layer.
		/// </summary>
		public abstract void Unload();

		/// <summary>
		/// Destroys layer and unloads it from memory. It is forbidden to use the destroyed layer as it's data might be inconsistent.
		/// </summary>
		public virtual void IsoDestroy()
		{
			if (!IsCreated)
				throw new IsoException("Layer is already destroyed");

			this.IsCreated = false;
		}

		/// <summary>
		/// Returns IsoLayerData object containing data of the layer ready to be used in serialization or creating other layers.
		/// </summary>
		/// <returns>IsoLayerData object containing data of the layer</returns>
		public IsoLayerData GetData()
		{
			IsoLayerData layerData = new IsoLayerData();

			layerData.textureID = SpriteID;
			layerData.transparent = IsTransparent;
			layerData.h = Height;

			return layerData;
		}
	}
}