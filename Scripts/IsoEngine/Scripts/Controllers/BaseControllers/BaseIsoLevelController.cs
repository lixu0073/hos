using UnityEngine;
using System.Collections.Generic;
using Hospital;

namespace IsoEngine
{
    /// <summary>
    /// Basic controller of single level on map. 
    /// </summary>
    public class BaseIsoLevelController : ComponentController, ILoadable
    {
        private IsoTile[,] tiles;
        private List<IsoQuadBackground> backgrounds;
        private List<IsoObject> objects;
        private Plane plane;

        private int tilesWidth = 0;
        private int tilesHeight = 0;

        public int ObjectsCount
        {
            get
            {
                return objects.Count;
            }
        }

        public IsoObject GetObject(int index)
        {
            return objects[index];
        }

        /// <summary>
        /// Returns object that occupy place above tile specified by coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public IsoObject GetObject(int x, int y)
        {
            if ((x > Width - 1) || (y > Height - 1) || (x <= 0 && y <= 0))
            {
                return null;
            }
            else return tiles[x, y].IsoObject;
        }

        public bool isObjectExistOnPos(int x, int y)
        {
            if ((x > Width - 1) || (y > Height - 1) || (x <= 0 && y <= 0))
            {
                return true;
            }
            else
            {
                if (tiles[x, y].IsoObject != null)
                    return true;
                else return false;
            }
        }

        public IsoObject GetObjectOfType(int id)
        {
            if (objects != null && objects.Count > 0)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i].ID == id)
                        return objects[i];
                }
            }
            return null;
        }

        public IsoObject[] GetObjectsOfType(int id)
        {
            var ret = new List<IsoObject>();

            if (objects != null && objects.Count > 0)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i].ID == id)
                        ret.Add(objects[i]);
                }
            }

            return ret.ToArray();
        }

        public IsoObject[] GetObjectsOfArea(Hospital.HospitalArea type)
        {
            var ret = new List<IsoObject>();

            if (objects != null && objects.Count > 0)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i].Area == type)
                        ret.Add(objects[i]);
                }
            }

            return ret.ToArray();
        }

        public List<Vector2i> GetAllDoorObjectOfArea(Hospital.HospitalArea type)
        {
            var doors = new List<Vector2i>();

            Hospital.HospitalArea type2;

            if (type == Hospital.HospitalArea.Clinic)
            {
                type2 = Hospital.HospitalArea.Hospital;
            }
            else if (type == Hospital.HospitalArea.Hospital)
            {
                type2 = Hospital.HospitalArea.Clinic;
            }
            else type2 = type;

            if (objects != null && objects.Count > 0)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (((objects[i].Area == type) || (objects[i].Area == type2)) && objects[i].GetSpotOfType(0) != null)
                    {
                        doors.Add(new Vector2i(objects[i].X + objects[i].GetSpotOfType(0).X, objects[i].Y + objects[i].GetSpotOfType(0).Y));
                    }
                }
            }

            return doors;
        }

        public Vector2i GetAnyDoorOfType(Hospital.HospitalArea type)
        {
            Hospital.HospitalArea type2;

            if (type == Hospital.HospitalArea.Clinic)
            {
                type2 = Hospital.HospitalArea.Hospital;
            }
            else if (type == Hospital.HospitalArea.Hospital)
            {
                type2 = Hospital.HospitalArea.Clinic;
            }
            else type2 = type;

            if (objects != null && objects.Count > 0)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (((objects[i].Area == type) || (objects[i].Area == type2)) && objects[i].GetSpotOfType(0) != null)
                    {
                        return new Vector2i(objects[i].X + objects[i].GetSpotOfType(0).X, objects[i].Y + objects[i].GetSpotOfType(0).Y);
                    }
                }
            }

            return Vector2i.zero;
        }


        // Properties
        /// <summary>
        /// Level width in number of tiles.
        /// </summary>
        public int Width
        {
            get
            {
                if (tiles != null)
                    return tiles.GetLength(0);

                return 0;
            }
        }
        /// <summary>
        /// Level Height in number of tiles.
        /// </summary>
        public int Height
        {
            get
            {
                if (tiles != null)
                    return tiles.GetLength(1);

                return 0;
            }
        }


        public int LevelID
        {
            get;
            private set;
        }

        /// <summary>
        /// Distance between levels.
        /// </summary>
        public int LevelHeight
        {
            get;
            private set;
        }

        public float ActualLevelHeight
        {
            get
            {
                return transform.localPosition.y;
            }
        }

        /// <summary>
        /// X offset in relation to coordinates on Lvl0.
        /// </summary>
        public int X
        {
            get;
            private set;
        }

        /// <summary>
        /// Y offset in relation to coordinates on Lvl0.
        /// </summary>
        public int Y
        {
            get;
            private set;
        }

        public bool IsLoaded
        {
            get;
            private set;
        }

        public bool IsInteriorLoaded
        {
            get;
            private set;
        }


        // Methods
        internal override void Initialize()
        {
            base.Initialize();

            IsInteriorLoaded = true;
            IsLoaded = false;

            objects = new List<IsoObject>();
            backgrounds = new List<IsoQuadBackground>();
            tilesWidth = 0;
            tilesHeight = 0;

        }

        public bool IsCreated
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates entire level based on provided information.
        /// </summary>
        /// <param name="levelData"></param>
        public virtual void CreateLevel(IsoLevelData levelData)
        {
            if (IsCreated)
                throw new IsoException("Level is already created");

            this.LevelID = levelData.levelID;
            this.LevelHeight = levelData.levelHeight;

            this.X = levelData.x;
            this.Y = levelData.y;

            int width = levelData.tileData.GetLength(0);
            int height = levelData.tileData.GetLength(1);

            gameObject.transform.localPosition = new Vector3(X, LevelHeight * engineController.Map.LevelHeight, Y);
            plane = new Plane(Vector3.up, transform.position);

            tilesWidth = width;
            tilesHeight = height;

            bool tilesPoolExists = (engineController.tilesPool != null);
            if (!tilesPoolExists) engineController.tilesPool = new IsoTile[width, height];

            IsoTileData[,] tileData = levelData.tileData;

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (!tilesPoolExists) engineController.tilesPool[i, j] = new IsoTile(engineController, LevelID, tileData[i, j]);
                    else engineController.tilesPool[i, j].Init(engineController, LevelID, tileData[i, j]);
                }
            }

            tiles = engineController.tilesPool;

            if (levelData.objectsData != null)
            {
                foreach (var isoObject in levelData.objectsData)
                {
                    AddObject(isoObject);
                }
            }

            if (levelData.backgroundData != null)
            {
                foreach (var p in levelData.backgroundData)
                {
                    AddBackground(p.x, p.y, p.width, p.height, p.materialID);
                }
            }

            IsCreated = true;
        }

        /// <summary>
        /// Checks if tile with provided coordinates is inside or outside of building
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsTileInterior(int x, int y)
        {
            return tiles[x, y].IsInterior;
        }

        /// <summary>
        /// Loads level into unity memory and makes it visible.
        /// </summary>
        public virtual void Load()
        {
            if (IsLoaded)
                throw new IsoException("Level is already loaded");

            if (IsInteriorLoaded)
            {
                // Load all tiles
                foreach (var tile in tiles)
                    tile.Load();

                // Load all objects
                foreach (var isoObject in objects)
                    isoObject.Load();

                // Load backgrounds
                foreach (var background in backgrounds)
                    background.Load();
            }
            else
            {
                // Load tiles outside
                foreach (var tile in tiles)
                    if (!tile.IsInterior)
                        tile.Load();

                // Load objects outside
                foreach (var isoObject in objects)
                    if (!isoObject.IsInterior)
                        isoObject.Load();
            }

            // Load collider
            GetComponent<BoxCollider>().enabled = true;

            IsLoaded = true;

        }

        /// <summary>
        /// Sets whole level invisible on the map and removes it from Unity memory. Until Load level will not receive any updates.
        /// </summary>
        public virtual void Unload()
        {
            if (!IsLoaded)
                throw new IsoException("Level is already unloaded");

            if (IsInteriorLoaded)
            {
                // Unload all tiles
                foreach (var tile in tiles)
                    tile.Unload();

                // Unload all objects
                foreach (var isoObject in objects)
                    isoObject.Unload();

                // Unload background
                foreach (var background in backgrounds)
                    background.Unload();
            }
            else
            {
                // Unload tiles outside
                foreach (var tile in tiles)
                    if (!tile.IsInterior)
                        tile.Unload();

                // Unload objects outside
                foreach (var isoObject in objects)
                    if (!isoObject.IsInterior)
                        isoObject.Unload();
            }

            // Unload collider
            GetComponent<BoxCollider>().enabled = false;

            IsLoaded = false;
        }

        /// <summary>
        /// Removes every information about lvl from game memory. 
        /// </summary>
        public override void IsoDestroy()
        {
            if (!IsCreated)
                throw new IsoException("Level is already destroyed");

            // TMP!!!
            Debug.Log("Level destroy");
            if (tiles != null)
                foreach (var tile in tiles)
                    tile.IsoDestroy();

            tiles = null;

            foreach (var isoObject in objects)
                isoObject.IsoDestroy();

            foreach (var background in backgrounds)
                background.IsoDestroy();

            objects.Clear();
            backgrounds.Clear();
            objects = null;
            backgrounds = null;

            IsCreated = false;
        }

        /// <summary>
        /// Loads only interior of building and makes it visible.
        /// </summary>
        public virtual void LoadInterior()
        {
            if (IsInteriorLoaded)
                throw new IsoException("Floor is already loaded");

            if (IsLoaded)
            {
                // Load tiles
                foreach (var tile in tiles)
                    if (tile.IsInterior)
                        tile.Load();

                // Load objects
                foreach (var isoObject in objects)
                    if (isoObject.IsInterior)
                        isoObject.Load();

                // Load background
                foreach (var background in backgrounds)
                    background.Load();

                // Load collider
                GetComponent<BoxCollider>().enabled = true;
            }

            IsInteriorLoaded = true;
        }

        /// <summary>
        /// Unloads interior information from Unity memory and makes it disappear from screen.
        /// </summary>
        public virtual void UnloadInterior()
        {
            if (!IsInteriorLoaded)
                throw new IsoException("Floor is already unloaded");

            if (IsLoaded)
            {
                // Unload tiles
                foreach (var tile in tiles)
                    if (tile.IsInterior)
                        tile.Unload();

                // Unload objects
                foreach (var isoObject in objects)
                    if (isoObject.IsInterior)
                        isoObject.Unload();

                // Unload background
                foreach (var background in backgrounds)
                    background.Unload();

                // Unload collider
                GetComponent<BoxCollider>().enabled = false;
            }

            IsInteriorLoaded = false;
        }

        public void AddBackground(int x, int y, int xsize, int ysize, int materialID)
        {
            IsoBackgroundData backgroundData = new IsoBackgroundData(x, y, xsize, ysize, materialID);

            IsoQuadBackground background = new IsoQuadBackground(engineController, LevelID, backgroundData);

            if (IsLoaded)
                background.Load();

            backgrounds.Add(background);
        }

        /// <summary>
        /// Adds provided lvl to tile specified by x,y coordinates
        /// </summary>
        /// <param name="x">X coordinate in level space</param>
        /// <param name="y">Y coordinate in level space</param>
        /// <param name="layerData">Data about layer being added</param>
        public void AddLayer(int x, int y, IsoLayerData layerData)
        {
            tiles[x, y].AddLayer(layerData);
        }

        /// <summary>
        /// Removes layer being on provided height on specified tile.
        /// </summary>
        /// <param name="x">x coordinate of tile</param>
        /// <param name="y">y coordinate of tile</param>
        /// <param name="height">height of layer being removed</param>
        public void RemoveLayer(int x, int y, int height)
        {
            tiles[x, y].RemoveLayer(height);
        }

        /// <summary>
        /// Can the object be added to the level.
        /// </summary>
        /// <param name="x">x position of the object</param>
        /// <param name="y">y position of the object</param>
        /// <param name="width">object width</param>
        /// <param name="height">object height</param>
        /// <param name="prefabData">prefab data</param>
        /// <returns>True if object can be added</returns>
        public virtual bool CanAddObject(int x, int y, int width, int height, IsoObjectPrefabData prefabData, bool isExistedBefore = false, bool isTemporary = false, int id = -1)
        {
            bool isOK = false;

            // check is object can be placed in position
            if (IsPointInBounds(x, y) && IsPointInBounds(x + width + 1, y) && IsPointInBounds(x, y + height + 1) && IsPointInBounds(x + width + 1, y + height + 1))
            {
                for (int w = 0; w < width; ++w)
                {
                    int tmpX = x + w;

                    for (int h = 0; h < height; ++h)
                    {
                        int tmpY = y + h;

                        if (((tiles[tmpX, tmpY].IsoObject != null) && ((tiles[tmpX, tmpY].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<AreaMapController>().pathBlockerId)
                            || (tiles[tmpX, tmpY].IsoObject.objectID == ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().pathBlockerId && tiles[tmpX, tmpY].IsoObject.isObjectHaveAnySpot() == false)))
                            || (tiles[tmpX, tmpY].IsoObject != null && tiles[tmpX, tmpY].IsoObject.objectID == ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().buildBlockerId))
                        {
                            if (!isTemporary)
                            {
                                if (prefabData.area != Hospital.HospitalArea.Ignore)
                                {
                                    if (tiles[tmpX, tmpY].IsoObject.isObjectHaveAnySpot())
                                        MessageController.instance.ShowMessageWithoutStacking(20);
                                    else MessageController.instance.ShowMessageWithoutStacking(21);
                                }
                                else MessageController.instance.ShowMessageWithoutStacking(20);
                            }
                            return false;
                        }
                    }
                }

                // if door exist then check that entrance is available at this position
                if ((prefabData.area == Hospital.HospitalArea.Clinic || prefabData.area == Hospital.HospitalArea.Hospital
                    || prefabData.area == HospitalArea.MaternityWardClinic) && (prefabData.spotsData.Length > 0) && AreaMapController.Map != null)
                {
                    foreach (var p in prefabData.spotsData)
                    {
                        if (p.id == 0)
                        {
                            Vector2i pos = Vector2i.empty;

                            switch (prefabData.rotation)
                            {
                                case Hospital.Rotation.North:
                                    pos = new Vector2i(x + p.x + 1, y + p.y);
                                    if (!IsPointInBounds(x + p.x + 1, y + p.y) || (!AreaMapController.Map.IsPosInsideClinic(new Vector2i(x + p.x + 1, y + p.y))))
                                    {
                                        MessageController.instance.ShowMessageWithoutStacking(20);
                                        return false;
                                    }
                                    break;
                                case Hospital.Rotation.South:
                                    pos = new Vector2i(x + p.x - 1, y + p.y);
                                    if (!IsPointInBounds(x + p.x - 1, y + p.y) || (!AreaMapController.Map.IsPosInsideClinic(new Vector2i(x + p.x - 1, y + p.y))))
                                    {
                                        MessageController.instance.ShowMessageWithoutStacking(20);
                                        return false;
                                    }
                                    break;
                                case Hospital.Rotation.East:
                                    pos = new Vector2i(x + p.x, y + p.y - 1);
                                    if (!IsPointInBounds(x + p.x, y + p.y - 1) || (!AreaMapController.Map.IsPosInsideClinic(new Vector2i(x + p.x, y + p.y - 1))))
                                    {
                                        MessageController.instance.ShowMessageWithoutStacking(20);
                                        return false;
                                    }
                                    break;
                                case Hospital.Rotation.West:
                                    pos = new Vector2i(x + p.x, y + p.y + 1);
                                    if (!IsPointInBounds(x + p.x, y + p.y + 1) || (!AreaMapController.Map.IsPosInsideClinic(new Vector2i(x + p.x, y + p.y + 1))))
                                    {
                                        MessageController.instance.ShowMessageWithoutStacking(20);
                                        return false;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            //Can we make a path from the door to all of the exits?
                            if (id != -1 && pos != Vector2i.empty && !AreaMapController.Map.NoPathCalculationObjectIds.Contains(id))
                            {
                                if (!AreaMapController.Map.CheckExitAvailabilityFromNotPlacedRoom(pos, new Vector2i(x, y), new Vector2i(width, height)))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                isOK = true;
            }

            // check is object in mutal position
            if (!isExistedBefore) // don't calc while loading
            {
                if (prefabData.area != HospitalArea.Ignore && CheckIsObjectCollideWithMutal(x, y, width, height, prefabData))
                {
                    MessageController.instance.ShowMessageWithoutStacking(21);
                    return false;
                }
            }

            return isOK;
        }

        public virtual bool CanAddDecoration(int x, int y, bool haveSpot)
        {
            if (((tiles[x, y].IsoObject != null) && ((tiles[x, y].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().pathBlockerId) || (tiles[x, y].IsoObject.objectID == ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().pathBlockerId && haveSpot == false))) || (tiles[x, y].IsoObject != null && tiles[x, y].IsoObject.objectID == ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().buildBlockerId))
            {
                return false;
            }

            return true;

        }

        public bool CheckIsObjectCollideWithMutal(int x, int y, int width, int height, IsoObjectPrefabData currentPrefab)
        {
            Vector2i pos = new Vector2i(x, y);
            Vector2i size = new Vector2i(width, height);

            if (currentPrefab != null)
            {
                int max = (width > height ? width : height);

                for (int indexs = 0; indexs < max; indexs++)
                {
                    // for width

                    // Areas in East
                    if (indexs < width)
                    {
                        if (IsPointInMutablArea(x + indexs, y - 1, pos, size))
                            return true;

                        if ((currentPrefab.mutalTiles == Hospital.MutalType.East) && (tiles[x + indexs, y - 1].IsoObject != null)
                            && (tiles[x + indexs, y - 1].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<AreaMapController>().pathBlockerId)
                            && (tiles[x + indexs, y - 1].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().buildBlockerId))
                            return true;

                        if (IsPointInMutablArea(x + indexs, y + height, pos, size))
                            return true;

                        if ((currentPrefab.mutalTiles == Hospital.MutalType.West) && (tiles[x + indexs, y + height].IsoObject != null)
                            && (tiles[x + indexs, y + height].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().pathBlockerId)
                            && (tiles[x + indexs, y + height].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().buildBlockerId))
                            return true;
                    }

                    // for height
                    if (indexs < height)
                    {
                        if (IsPointInMutablArea(x + width, y + indexs, pos, size))
                            return true;

                        if ((currentPrefab.mutalTiles == Hospital.MutalType.North) && (tiles[x + width, y + indexs].IsoObject != null)
                            && (tiles[x + width, y + indexs].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().pathBlockerId)
                            && (tiles[x + width, y + indexs].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().buildBlockerId))
                            return true;

                        if (IsPointInMutablArea(x - 1, y + indexs, pos, size))
                            return true;

                        if ((currentPrefab.mutalTiles == Hospital.MutalType.South) && (tiles[x - 1, y + indexs].IsoObject != null)
                            && (tiles[x - 1, y + indexs].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().pathBlockerId)
                            && (tiles[x - 1, y + indexs].IsoObject.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().buildBlockerId))
                            return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public bool IsPointInMutablArea(int x, int y, Vector2i pos, Vector2i size)
        {
            IsoObject tmpIso = null;
            try { tmpIso = tiles[x, y].IsoObject; }
            catch { return false; }

            if (tmpIso == null)
                return false;

            var tempo = engineController.objects[tmpIso.objectID].GetComponent<IsoObjectPrefabController>().prefabData;

            if (tempo != null && tmpIso.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().pathBlockerId
                && tmpIso.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().buildBlockerId)
            {
                if (tempo.mutalTiles != Hospital.MutalType.None)
                {
                    var area = GetMutalArea(tmpIso.X, tmpIso.Y, tempo.tilesX, tempo.tilesY, tempo.mutalTiles);
                    foreach (var tmp in area)
                    {
                        if ((tmp.x >= pos.x && tmp.x < pos.x + size.x) && (tmp.y >= pos.y && tmp.y < pos.y + size.y) && tmp != Vector2i.zero)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected virtual IEnumerable<Vector2i> GetMutalArea(int x, int y, int width, int height, Hospital.MutalType mutalType)
        {
            if (mutalType == Hospital.MutalType.North)
            {
                for (int h = 0; h < height; h++)
                {
                    yield return new Vector2i(x + width, y + h);
                }
            }
            else if (mutalType == Hospital.MutalType.South)
            {
                for (int h = 0; h < height; h++)
                {
                    yield return new Vector2i(x - 1, y + h);
                }
            }
            else if (mutalType == Hospital.MutalType.East)
            {
                for (int w = 0; w < width; w++)
                {
                    yield return new Vector2i(x + w, y - 1);
                }
            }
            else if (mutalType == Hospital.MutalType.West)
            {
                for (int w = 0; w < width; w++)
                {
                    yield return new Vector2i(x + w, y + height);
                }
            }

            yield return Vector2i.zero;
        }

        /// <summary>
        /// Adds provided object to level.
        /// </summary>
        /// <param name="objectData">Data of object</param>
        public IsoObject AddObjectWithoutValidation(IsoObjectData objectData, Hospital.PathType[] pathTypes = null)
        {
            IsoObjectPrefabData prefabData = engineController.objects[objectData.objectID].GetComponent<IsoObjectPrefabController>().prefabData;

            if (pathTypes != null)
                prefabData.pathTypes = pathTypes;
            else
                prefabData.pathTypes = new Hospital.PathType[] { Hospital.PathType.Default };


            IsoObject isoObject = new IsoObject(engineController, LevelID, objectData, prefabData);
            AddObjectData(objectData, isoObject, prefabData.pathTypes);

            return isoObject;
        }

        public IsoObject AddObject(IsoObjectData objectData, Hospital.PathType[] pathTypes = null)
        {

            IsoObjectPrefabData prefabData = engineController.objects[objectData.objectID].GetComponent<IsoObjectPrefabController>().prefabData;

            if (pathTypes != null)
                prefabData.pathTypes = pathTypes;
            else
                prefabData.pathTypes = new Hospital.PathType[] { Hospital.PathType.Default };

            if ((objectData.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().pathBlockerId)
                && (objectData.objectID != ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().buildBlockerId))
            {
                if (!CanAddObject(objectData.x, objectData.y, prefabData.tilesX, prefabData.tilesY, prefabData, false, false, objectData.objectID))
                {
                    return null;
                }
            }

            IsoObject isoObject = new IsoObject(engineController, LevelID, objectData, prefabData);
            AddObjectData(objectData, isoObject, prefabData.pathTypes);

            return isoObject;
        }

        public IsoObject AddObjectFromExisting(IsoObjectData objectData, GameObject gameObject, Hospital.PathType[] pathTypes = null)
        {
            IsoObjectPrefabData prefabData = engineController.objects[objectData.objectID].GetComponent<IsoObjectPrefabController>().prefabData;
            if (!CanAddObject(objectData.x, objectData.y, prefabData.tilesX, prefabData.tilesY, prefabData, true, false, objectData.objectID))
            {
                return null;
            }

            if (pathTypes == null)
                pathTypes = new Hospital.PathType[] { Hospital.PathType.Default };

            IsoObject isoObject = new IsoObject(engineController, LevelID, objectData, prefabData, gameObject);
            AddObjectData(objectData, isoObject, pathTypes);

            return isoObject;
        }

        protected virtual void AddObjectData(IsoObjectData objectData, IsoObject isoObject, Hospital.PathType[] pathTypes = null)
        {
            if (pathTypes == null)
                pathTypes = new Hospital.PathType[] { Hospital.PathType.Default };

            objects.Add(isoObject);

            for (int i = 0; i < isoObject.W; ++i)
                for (int j = 0; j < isoObject.H; ++j)
                {
                    tiles[isoObject.X + i, isoObject.Y + j].IsoObject = isoObject;
                    tiles[isoObject.X + i, isoObject.Y + j].pathTypes = pathTypes;
                }

            if (IsLoaded)
                isoObject.Load();
        }

        /// <summary>
        /// Removes object that occupy place above tile specified by coordinates.
        /// </summary>
        public void RemoveObject(IsoObject isoObject)
        {
            RemoveObjectData(isoObject);

            isoObject.IsoDestroy();
        }

        public GameObject LightRemoveObject(IsoObject isoObject)
        {
            if (isoObject != null)
            {
                RemoveObjectData(isoObject);

                return isoObject.LightDestroy();
            }
            return null;
        }

        protected virtual void RemoveObjectData(IsoObject isoObject)
        {
            for (int i = 0; i < isoObject.W; ++i)
                for (int j = 0; j < isoObject.H; ++j)
                {
                    tiles[isoObject.X + i, isoObject.Y + j].IsoObject = null;
                }

            objects.Remove(isoObject);
        }

        /// <summary>
        /// Returns data about level packed into serializable structure.
        /// </summary>
        /// <returns></returns>
        public virtual IsoLevelData GetData()
        {
            IsoLevelData levelData = new IsoLevelData(tiles.GetLength(0), tiles.GetLength(1));

            levelData.additionalData = null;
            levelData.x = X;
            levelData.y = Y;
            levelData.levelHeight = LevelHeight;
            levelData.levelID = LevelID;

            for (int i = 0; i < levelData.tileData.GetLength(0); ++i)
            {
                for (int j = 0; j < levelData.tileData.GetLength(1); ++j)
                {
                    levelData.tileData[i, j] = tiles[i, j].GetData();
                }
            }

            levelData.objectsData = new IsoObjectData[objects.Count];
            for (int i = 0; i < levelData.objectsData.Length; ++i)
            {
                levelData.objectsData[i] = objects[i].GetData();
            }

            levelData.backgroundData = new IsoBackgroundData[backgrounds.Count];
            for (int i = 0; i < levelData.backgroundData.Length; ++i)
            {
                levelData.backgroundData[i] = backgrounds[i].GetData();
            }
            return levelData;
        }

        public Vector2i ScreenToTile(Vector3 screenPoint)
        {
            Ray ray = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenPointToRay(screenPoint);

            Vector3 hitPoint;

            float distance = 0.0f;
            if (plane.Raycast(ray, out distance))
            {
                hitPoint = ray.GetPoint(distance);

                return new Vector2i((int)(hitPoint.x + 0.5f) - X, (int)(hitPoint.z + 0.5f) - Y);
            }
            return Vector2i.zero;
        }

        public Vector2 ScreenToMap(Vector3 screenPoint)
        {
            Ray ray = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenPointToRay(screenPoint);

            Vector3 hitPoint;

            float distance = 0.0f;
            if (plane.Raycast(ray, out distance))
            {
                hitPoint = ray.GetPoint(distance);

                return new Vector2((hitPoint.x + 0.5f) - X, (hitPoint.z + 0.5f) - Y);
            }
            return new Vector2();
        }

        public bool IsMousePressed
        {
            get;
            private set;
        }
        public Vector2 MouseStartPosition
        {
            get;
            private set;
        }
        private Vector3 mouseStartPosition;

        private const float mouseMoveMargin = 10;
        /// <summary>
        /// Is position in bounds of the whole game area?
        /// </summary>
        public bool IsPointInBounds(int x, int y)
        {
            return x >= 0 && x < tilesWidth && y >= 0 && y < tilesHeight;
        }
        /// <summary>
        /// Is point inside the whole game area and is Interior
        /// </summary>
        public bool IsPointInterior(int x, int y)
        {
            return IsPointInBounds(x, y) && tiles[x, y].IsInterior;
        }

        public delegate void MouseEventHandler();

        public event MouseEventHandler OnMouseDownEvent;
        public event MouseEventHandler OnMouseUpEvent;

        private void OnMouseDown()
        {
            if (Utils.IsPointerOverInterface())
                return;
            IsMousePressed = true;
            mouseStartPosition = Input.mousePosition;
            MouseStartPosition = ScreenToMap(mouseStartPosition);

            if (OnMouseDownEvent != null)
                OnMouseDownEvent();
        }

        private void OnMouseUp()
        {
            if (Utils.IsPointerOverInterface())
                return;
            if (!IsMousePressed)
                return;

            if ((mouseStartPosition - Input.mousePosition).magnitude < mouseMoveMargin)
            {
                PressedTile = ScreenToTile(Input.mousePosition);
                OnTilePress(PressedTile);
            }

            if (OnMouseUpEvent != null)
                OnMouseUpEvent();

            IsMousePressed = false;
        }

        public virtual void OnTilePress(Vector2i pressedTile)
        {
            engineController.Map.OnTilePress(LevelID, pressedTile);
        }
        public virtual void OnTileTouch(Vector2i touchedtile)
        {
            engineController.Map.OnTiletouch(LevelID, touchedtile);
        }

        public Vector2i PressedTile
        {
            get;
            private set;
        }
    }
}