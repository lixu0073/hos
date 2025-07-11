using System;

namespace IsoEngine
{
	public enum IsoTileType
	{
		Sprites,
		Quads,
	}

	[Serializable]
	public abstract class IsoLevelAdditionalData
	{
		private IsoLevelAdditionalData previousData;

		public IsoLevelAdditionalData(IsoLevelAdditionalData previousData)
		{
			this.previousData = previousData;
		}

		public static TAdditionalData PackData<TAdditionalData>(IsoLevelData levelData)
			where TAdditionalData : IsoLevelAdditionalData
		{
			TAdditionalData data = (TAdditionalData)Activator.CreateInstance(typeof(TAdditionalData), levelData.additionalData);
			levelData.additionalData = data;

			return data;
		}

		public static TAdditionalData UnpackData<TAdditionalData>(IsoLevelData levelData)
			where TAdditionalData : IsoLevelAdditionalData
		{
			TAdditionalData data = (TAdditionalData)levelData.additionalData;
			levelData.additionalData = data.previousData;

			return data;
		}
	}

	[Serializable]
	public abstract class IsoMapAdditionalData
	{
		private IsoMapAdditionalData previousData;

		public IsoMapAdditionalData(IsoMapAdditionalData previousData)
		{
			this.previousData = previousData;
		}

		public static TAdditionalData PackData<TAdditionalData>(IsoMapData mapData)
			where TAdditionalData : IsoMapAdditionalData
		{
			TAdditionalData data = (TAdditionalData)Activator.CreateInstance(typeof(TAdditionalData), mapData.additionalData);
			mapData.additionalData = data;

			return data;
		}

		public static TAdditionalData UnpackData<TAdditionalData>(IsoMapData mapData)
			where TAdditionalData : IsoMapAdditionalData
		{
			TAdditionalData data = (TAdditionalData)mapData.additionalData;
			mapData.additionalData = data.previousData;

			return data;
		}
	}

	[Serializable]
	public abstract class IsoPersonAdditionalData
	{
		private IsoPersonAdditionalData previousData;

		public IsoPersonAdditionalData(IsoPersonAdditionalData previousData)
		{
			this.previousData = previousData;
		}

		public static TAdditionalData PackData<TAdditionalData>(IsoPersonData personData)
			where TAdditionalData : IsoPersonAdditionalData
		{
			TAdditionalData data = (TAdditionalData)Activator.CreateInstance(typeof(TAdditionalData), personData.additionalData);
			personData.additionalData = data;

			return data;
		}

		public static TAdditionalData UnpackData<TAdditionalData>(IsoPersonData personData)
			where TAdditionalData : IsoPersonAdditionalData
		{
			TAdditionalData data = (TAdditionalData)personData.additionalData;
			personData.additionalData = data.previousData;

			return data;
		}
	}

	[Serializable]
	public abstract class IsoObjectExtensionData
	{
		public int extensionType;
	}

	[Serializable]
	public class IsoPersonData
	{
		public IsoPersonAdditionalData additionalData;

		public int prefabID;

		public int levelID; 
		public int x;
		public int y;

		public int directionX;
		public int directionY;

		public string name;

		public System.Type controller;
	}

	/// <summary>
	/// Contains all the information describing the map.
	/// It is used to create new map or load and save it to\from file.
	/// </summary>
	[Serializable]
	public class IsoMapData
	{
		public IsoTileType tileType = IsoTileType.Sprites;

		public IsoMapAdditionalData additionalData;

		/// <summary>
		/// Information about all the levels in the map.
		/// </summary>
		public IsoLevelData[] levelData;
	}

	/// <summary>
	/// Contains all the information describing the level.
	/// It is used to create new level or load and save it to\from file.
	/// </summary>
	[Serializable]
	public class IsoLevelData
	{
		public IsoLevelAdditionalData additionalData;

		/// <summary>
		/// X coordinate of the level in tiles relative to the the map position.
		/// </summary>
		public int x=0;

		/// <summary>
		/// Y coordinate of the level in tiles relative to the the map position.
		/// </summary>
		public int y=0;

		/// <summary>
		/// ID of the level. It is unique for each level.
		/// </summary>
		public int levelID=0;

		/// <summary>
		/// The height of the level in unit specified in the map controller.
		/// </summary>
		public int levelHeight=0;

		/// <summary>
		/// Two dimensional array containing the information about tiles on the level.
		/// </summary>
		public readonly IsoTileData[,] tileData;

		/// <summary>
		/// Array containing information about all the objects on the level.
		/// </summary>
		public IsoObjectData[] objectsData;

		public IsoBackgroundData[] backgroundData=null;

		// Methods
		/// <summary>
		/// Creates level data.
		/// </summary>
		/// <param name="width">Width of the level in tiles</param>
		/// <param name="height">Height of the level in tiles</param>
		public IsoLevelData(int width, int height)
		{
			tileData = new IsoTileData[width, height];
		}
	}

	[Serializable]
	public class IsoBackgroundData
	{
		public readonly int x;
		public readonly int y;
		public readonly int width;
		public readonly int height;
		public readonly int materialID;
		
		public IsoBackgroundData(int x, int y, int width, int height, int materialID)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			this.materialID = materialID;
		}
	}

	/// <summary>
	/// Contains all the information describing the tile.
	/// It is used to create new tile or load and save it to\from file.
	/// </summary>
	[Serializable]
	public class IsoTileData
	{
		/// <summary>
		/// X coordinate of the tile in the level.
		/// </summary>
		public readonly int x;

		/// <summary>
		/// Y coordinate of the tile in the level.
		/// </summary>
		public readonly int y;

		/// <summary>
		/// Specifies whether the tile should be active when switching levels.
		/// When set to true it will be unloaded when changing levels.
		/// </summary>
		public bool isInterior;

		/// <summary>
		/// Array containing all the layers on the tile.
		/// </summary>
		public IsoLayerData[] isoLayers;

		// Methods
		internal IsoTileData(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	/// <summary>
	/// Contains all the information describing the layer.
	/// It is used to create new layer or load and save it to\from file.
	/// </summary>
	[Serializable]
	public class IsoLayerData
	{
		/// <summary>
		/// The height of the layer on the layer stack of a tile.
		/// </summary>
		public int h;

		/// <summary>
		/// The ID of a texture specified in the engine.
		/// </summary>
		public int textureID;

		/// <summary>
		/// Tells whether the layer is transparent or not.
		/// When set to false the layer will cover all the layers beneath.
		/// </summary>
		public bool transparent;
	}

	/// <summary>
	/// Contains all the information describing the object.
	/// It is used to create new object or load and save it to\from file.
	/// </summary>
	[Serializable]
	public class IsoObjectData
	{
		/// <summary>
		/// The ID of an object specified in the engine.
		/// </summary>
		public int objectID;

		/// <summary>
		/// The X coordinate of a tile on which the lower left corner of the object is located.
		/// </summary>
		public int x;

		/// <summary>
		/// The Y coordinate of a tile on which the lower left corner of the object is located.
		/// </summary>
		public int y;

		/// <summary>
		/// Specifies whether the object should be active when switching levels.
		/// When set to true it will be unloaded when changing levels.
		/// </summary>
		public bool isInterior;

		public IsoObjectExtensionData extensionData;
	}
}