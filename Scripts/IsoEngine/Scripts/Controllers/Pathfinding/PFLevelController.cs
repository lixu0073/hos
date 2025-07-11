using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections.Specialized;

namespace IsoEngine
{
    [Serializable]
    public class PFLevelAdditionalData : IsoLevelAdditionalData
    {
        public PFLevelAdditionalData(IsoLevelAdditionalData previousData)
            : base(previousData)
        {
        }

        public IsoPersonData[] personsData;
    }

    public class ExtendedTileData : IHeapItem<ExtendedTileData>
    {
        public bool closed = false;
        public bool passable = true;
        public bool mutal = false;
        public bool spotQueues = false;
        public Hospital.PathType[] pathTypes = new Hospital.PathType[1] { Hospital.PathType.Default };

        public float distance = 0;
        public Vector2i previous = new Vector2i();

        public List<Hospital.BasePatientAI> persons = new List<Hospital.BasePatientAI>();

        // Pathfinding data
        public int X { get; set; }
        public int Y { get; set; }
        public int gCost { get; set; }
        public int hCost { get; set; }
        public int fCost { get { return gCost + hCost; } }
        public int HeapIndex { get; set; }
        public ExtendedTileData Parent { get; set; }
        public int CompareTo(ExtendedTileData tileToCompare)
        {
            int comparisonResult = fCost.CompareTo(tileToCompare.fCost);
            if (comparisonResult == 0)
                comparisonResult = hCost.CompareTo(tileToCompare.hCost);
            return -comparisonResult;
        }
    }
    /// <summary>
    /// Controls one level of map.Derives from IsoLevelController. 
    /// Extend it by adding informtions about passability of tiles and methods used for pathfinding.
    /// </summary>
    public class PFLevelController : BaseIsoLevelController
    {
        public bool debug = false;
        public int debugIterations = 0;

        private List<Vector2i> disabledPassableTiles;

        public class TilePersons : IEnumerable<Hospital.BasePatientAI>
        {
            private List<Hospital.BasePatientAI> persons;

            public TilePersons(List<Hospital.BasePatientAI> persons)
            {
                this.persons = persons;
            }

            public int Count
            {
                get
                {
                    return persons.Count;
                }
            }

            public Hospital.BasePatientAI this[int index]
            {
                get
                {
                    return persons[index];
                }
            }

            public IEnumerator<Hospital.BasePatientAI> GetEnumerator()
            {
                for (int i = 0; i < persons.Count; ++i)
                    yield return persons[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        //Used for patients
        private ExtendedTileData[,] tilesData;
        //used for other calculations
        private ExtendedTileData[,] nodesData;

        public int PersonsCount
        {
            get
            {
                return persons.Count;
            }
        }

        public void Update()
        {
#if UNITY_EDITOR && DEBUG
            if (debug)
                for (int i = 0; i < tilesData.GetLength(0); i++)
                {
                    for (int j = 0; j < tilesData.GetLength(1); j++)
                    {
                        if (IsPassable(i, j))
                            DrawRect(new Vector2i(i, j));
                    }
                }
#endif

        }
        private void DrawRect(Vector2i pos)
        {
#if UNITY_EDITOR && DEBUG
            Debug.DrawLine(new Vector3(pos.x - 0.5f, 0, pos.y - 0.5f), new Vector3(pos.x - 0.5f, 0, pos.y + 0.5f), Color.red);
            Debug.DrawLine(new Vector3(pos.x - 0.5f, 0, pos.y + 0.5f), new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f), Color.red);
            Debug.DrawLine(new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f), new Vector3(pos.x + 0.5f, 0, pos.y - 0.5f), Color.red);
            Debug.DrawLine(new Vector3(pos.x + 0.5f, 0, pos.y - 0.5f), new Vector3(pos.x - 0.5f, 0, pos.y - 0.5f), Color.red);
#endif
        }

        private void DrawRect(Vector2i pos, Color color, float duration)
        {
#if UNITY_EDITOR && DEBUG
            Debug.DrawLine(new Vector3(pos.x - 0.5f, 0, pos.y - 0.5f), new Vector3(pos.x - 0.5f, 0, pos.y + 0.5f), color, duration);
            Debug.DrawLine(new Vector3(pos.x - 0.5f, 0, pos.y + 0.5f), new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f), color, duration);
            Debug.DrawLine(new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f), new Vector3(pos.x + 0.5f, 0, pos.y - 0.5f), color, duration);
            Debug.DrawLine(new Vector3(pos.x + 0.5f, 0, pos.y - 0.5f), new Vector3(pos.x - 0.5f, 0, pos.y - 0.5f), color, duration);
#endif
        }

        public bool IsPassable(int x, int y)
        {
            return tilesData[x, y].passable;
        }

        public bool IsPassableWithPathType(int x, int y, Hospital.PathType pathType)
        {
            bool existSpecifiedPathInList = false;

            for (int i = 0; i < tilesData[x, y].pathTypes.Length; i++)
            {
                if (tilesData[x, y].pathTypes[i] == pathType || tilesData[x, y].pathTypes[i] == Hospital.PathType.Default)
                {
                    existSpecifiedPathInList = true;
                }
            }
            return existSpecifiedPathInList;
        }

        public bool IsFree(int x, int y)
        {
            return tilesData[x, y].passable && !tilesData[x, y].spotQueues;
        }

        public TilePersons[] GetNeighbourTiles(int x, int y)
        {
            List<TilePersons> ret = new List<TilePersons>();

            for (int i = -1; i <= 1; ++i)
                for (int j = -1; j <= 1; ++j)
                    if (IsPointInBounds(x + i, y + j))
                        ret.Add(new TilePersons(tilesData[x + i, y + j].persons));

            return ret.ToArray();
        }

        /// <summary>
        /// Creates level from data delivered in IsoLevelData. Also creates information about passability. 
        /// </summary>
        /// <param name="levelData">Level is generated from data from this object</param>
        public override void CreateLevel(IsoLevelData levelData)
        {
            int width = levelData.tileData.GetLength(0);
            int height = levelData.tileData.GetLength(1);

            tilesData = new ExtendedTileData[width, height];
            nodesData = new ExtendedTileData[width, height];

            for (int i = 0; i < tilesData.GetLength(0); ++i)
            {
                for (int j = 0; j < tilesData.GetLength(1); ++j)
                {
                    tilesData[i, j] = new ExtendedTileData();
                    tilesData[i, j].passable = true;
                    nodesData[i, j] = new ExtendedTileData();
                }
            }

            persons = new List<Hospital.BasePatientAI>();
            this.InteriorPersonsCount = 0;
            if (heapPool == null)
                CreateHeapPool();

            base.CreateLevel(levelData);
        }

        public void AddSpotQueue(QueueableSpot.SpotData item)
        {
            if (item.LevelID != LevelID)
                throw new IsoException("Invalid spot level");

            //if (tilesData[item.X, item.Y].spotQueues == true)
            //    throw new IsoException("There is already spot on tile " + item.X + " " + item.Y);

            tilesData[item.X, item.Y].spotQueues = true;

            //  TMP!!!
            //	var layerData = new IsoLayerData();
            //	layerData.h = 0;
            //	layerData.transparent = false;
            //	layerData.textureID = 1;
            //	AddLayer(item.X, item.Y, layerData);
        }

        public void RemoveSpotQueue(QueueableSpot.SpotData item)
        {
            if (item.LevelID != LevelID)
                throw new IsoException("Invalid spot level");

            //if (tilesData[item.X, item.Y].spotQueues == false)
            //    throw new IsoException("There is no spot on tile " + item.X + " " + item.Y);

            tilesData[item.X, item.Y].spotQueues = false;

            //  TMP!!!
            //	RemoveLayer(item.X, item.Y, 0);
        }

        private List<Hospital.BasePatientAI> persons;

        public void ConfigurePersonLoaded(BasePersonController person)
        {
            if (IsLoaded)
            {
                if (person.IsInterior)
                {
                    if (IsInteriorLoaded)
                    {
                        if (!person.IsLoaded)
                            person.Load();
                    }
                    else
                    {
                        if (person.IsLoaded)
                            person.Unload();
                    }
                }
                else
                {
                    if (!person.IsLoaded)
                        person.Load();
                }
            }
            else
            {
                if (person.IsLoaded)
                    person.Unload();
            }
        }

        public virtual void UpdatePersonPosition(Hospital.BasePatientAI person, int oldX, int oldY)
        {
            if (tilesData[oldX, oldY].persons.Contains(person))
                tilesData[oldX, oldY].persons.Remove(person);

            tilesData[person.position.x, person.position.y].persons.Add(person);
        }

        public virtual void UpdatePatientPosition(Hospital.BasePatientAI person, Vector2i old, Vector2i dest)
        {
            if (person != null && tilesData != null && tilesData.Length > 0 && old.x >= 0 && old.y >= 0 && dest.x >= 0 && dest.y >= 0)
            {
                if ((person.destinationTilePos.x < tilesData.GetLength(0) && person.destinationTilePos.y < tilesData.GetLength(1)) && (person.position.x < tilesData.GetLength(0) && person.position.y < tilesData.GetLength(1))) // fix for spots out of maps
                {
                    if (old != Vector2i.zero && (old.x < tilesData.GetLength(0) && old.y < tilesData.GetLength(1)))
                    {
                        if (tilesData[old.x, old.y].persons.Contains(person))
                            tilesData[old.x, old.y].persons.Remove(person);

                        tilesData[person.position.x, person.position.y].persons.Add(person);
                    }

                    if (dest != Vector2i.zero && (dest.x < tilesData.GetLength(0) && dest.y < tilesData.GetLength(1)))
                    {
                        //Debug.Log("dest.x: " + dest.x + " dest.y: " + dest.y);
                        if (tilesData[dest.x, dest.y].persons.Contains(person))
                            tilesData[dest.x, dest.y].persons.Remove(person);

                        if (tilesData[person.destinationTilePos.x, person.destinationTilePos.y].persons != null)
                        {
                            tilesData[person.destinationTilePos.x, person.destinationTilePos.y].persons.Add(person);
                        }
                    }
                }
            }
        }

        public virtual Hospital.BasePatientAI GetNearPatient(Vector2i pos, int range)
        {
            for (int i = -1; i <= range; i++)
                for (int j = -1; j <= range; j++)
                {
                    if (i != 0 && j != 0)
                    {
                        if (tilesData[pos.x + i, pos.y + j].persons != null && tilesData[pos.x + i, pos.y + j].persons.Count > 0)
                            return tilesData[pos.x + i, pos.y + j].persons[0];
                    }
                }

            return null;
        }

        /*
        public virtual BasePersonController AddPerson(IsoPersonData personData)
        {
            Debug.Assert(typeof(BasePersonController).IsAssignableFrom(personData.controller), "Invalid person controller");

            GameObject go = GameObject.Instantiate(engineController.persons[personData.prefabID]);
            go.SetActive(true);

            BasePersonController personControler = (BasePersonController)go.AddComponent(personData.controller);

            persons.Add(personControler);

            go.name = personData.name;
            go.transform.SetParent(transform);

            personControler.Initialize();

            personControler.CreatePerson(personData);

            if (IsLoaded)
            {
                personControler.Load();
            }

            return personControler;
        }

        public virtual void RemovePerson(BasePersonController person)
        {
            persons.Remove(person);
            tilesData[person.X, person.Y].persons.Remove(person);
            person.IsoDestroy();
            GameObject.Destroy(person.gameObject);
        
    */

        public override void IsoDestroy()
        {
            foreach (var person in persons)
            {
                person.IsoDestroy();
                GameObject.Destroy(person.gameObject);
            }
            tilesData = null;
            nodesData = null;

            base.IsoDestroy();
        }


        protected override void AddObjectData(IsoObjectData objectData, IsoObject isoObject, Hospital.PathType[] pathTypes = null)
        {
            Hospital.PathType[] newPathTypes = { Hospital.PathType.Default };
            if (pathTypes != null)
                base.AddObjectData(objectData, isoObject, pathTypes);
            else base.AddObjectData(objectData, isoObject, newPathTypes);

            for (int i = 0; i < isoObject.W; ++i)
            {
                for (int j = 0; j < isoObject.H; ++j)
                {
                    tilesData[isoObject.X + i, isoObject.Y + j].passable = isoObject.GetTileData(isoObject.X + i, isoObject.Y + j).isPassable;
                    tilesData[isoObject.X + i, isoObject.Y + j].mutal = isoObject.GetTileData(isoObject.X + i, isoObject.Y + j).isMutal;

                    if (IsConstaintType(tilesData[isoObject.X + i, isoObject.Y + j].pathTypes, Hospital.PathType.Default))
                        tilesData[isoObject.X + i, isoObject.Y + j].pathTypes = pathTypes;
                }
            }

            for (int i = 0; i < isoObject.SpotsCount; ++i)
            {
                QueueableSpot spot = isoObject.GetSpot(i);
                AddSpotQueue(new QueueableSpot.SpotData(spot.LevelID, isoObject.X + spot.X, isoObject.Y + spot.Y, spot.Direction));
            }
        }

        public bool IsConstaintType(Hospital.PathType[] listOfPath, Hospital.PathType findingType)
        {
            if (listOfPath.Length > 0 && listOfPath != null)
            {
                for (int i = 0; i < listOfPath.Length; i++)
                {
                    if (listOfPath[i] == findingType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void RemoveObjectData(IsoObject isoObject)
        {
            if (isoObject != null)
            {
                for (int i = 0; i < isoObject.W; ++i)
                {
                    for (int j = 0; j < isoObject.H; ++j)
                    {
                        tilesData[isoObject.X + i, isoObject.Y + j].passable = true;
                    }
                }

                for (int i = 0; i < isoObject.SpotsCount; ++i)
                {
                    QueueableSpot spot = isoObject.GetSpot(i);

                    RemoveSpotQueue(new QueueableSpot.SpotData(spot.LevelID, isoObject.X + spot.X, isoObject.Y + spot.Y, spot.Direction));
                }

                base.RemoveObjectData(isoObject);
            }
        }

        /// <summary>
        /// Returns path from /From/ to /To/ inclusive.
        /// All calculations are made in invoking thread.
        /// </summary>
        public PathInfo GetPath(Vector2i From, Vector2i To, Hospital.PathType pathType, bool isPathForPatient = true)
        {
            if (isPathForPatient)
                return GetPath(From, To, pathType, tilesData);
            else
                return GetPath(From, To, pathType, nodesData);
        }


        /// <summary>
        /// Set the area passable. This is used to set disable area while doing calculations for pathfinding.
        /// Remember to set area back to earlier state after adjusting the walkable by setPassable true.
        /// </summary>
        /// <param name="pos">Position of the object</param>
        /// <param name="scale">Scale of the object</param>
        /// <param name="setPassable">Boolean to enable or disable the area</param>
        public void SetAreaPassable(Vector2i pos, Vector2i scale, bool setPassable = true)
        {
            if (disabledPassableTiles == null)
            {
                disabledPassableTiles = new List<Vector2i>();
            }
            //Go through each node and set passable accordingly
            foreach (var node in nodesData)
            {
                Vector2i position = new Vector2i(node.X, node.Y);
                //Check that the point is on the wanted grid
                if ((node.X >= pos.x && node.X < pos.x + scale.x) && (node.Y >= pos.y && node.Y < pos.y + scale.y))
                {
                    //Enable passable
                    if (!node.passable && setPassable)
                    {
                        if (disabledPassableTiles.Contains(position))
                        {
                            disabledPassableTiles.Remove(position);
                            node.passable = true;
#if UNITY_EDITOR && DEBUG
                            if (debug)
                                DrawRect(position, Color.green, 0.5f);
#endif
                        }
                    }
                    //Disable passable
                    else if (node.passable && !setPassable)
                    {
                        //If the position is already set to not passable we can continue.
                        if (disabledPassableTiles.Contains(position))
                        {
                            node.passable = false;
                            continue;
                        }
                        disabledPassableTiles.Add(position);
                        node.passable = false;
#if UNITY_EDITOR && DEBUG
                        if (debug)
                            DrawRect(position, Color.blue, 0.5f);
#endif
                    }
                }
            }
            //This is a failsafe
            if (setPassable && disabledPassableTiles.Count > 0)
            {
                Debug.Log("Error. disabledPassableTiles is bigger than 0. set all walkable");
                foreach (var node in nodesData)
                {
                    foreach (var passableTile in disabledPassableTiles)
                    {
                        if (passableTile.x == node.X && passableTile.y == node.Y)
                        {
                            node.passable = true;
                            disabledPassableTiles.Remove(passableTile);
                        }
                    }
                }
                if (setPassable && disabledPassableTiles.Count > 0)
                {
                    Debug.LogError("disabledPassableTiles error. " + disabledPassableTiles);
                    //TODO Add ddna implementation?
                }
            }
        }

        int heapSize = 200;
        int poolSize = 3;
        [ThreadStatic] static Queue<Heap<ExtendedTileData>> heapPool;
        [ThreadStatic] static HashSet<Vector2> blockedTiles;

        public void CreateHeapPool()
        {
            blockedTiles = new HashSet<Vector2>();
            for (int i = 22; i <= 23; i++)
                for (int j = 44; j < 54; j++)
                    blockedTiles.Add(new Vector2(i, j));

            var tiles = Hospital.HospitalAreasMapController.HospitalMap.mapConfig.ImpassableTileData;
            for (int i = 0; i < tiles.Length / 2; i++)
            {
                blockedTiles.Add(new Vector2(tiles[i * 2 + 0], tiles[0 * 2 + 1]));
            }

            heapPool = new Queue<Heap<ExtendedTileData>>();
            for (int i = 0; i < poolSize; i++)
            {
                heapPool.Enqueue(new Heap<ExtendedTileData>(heapSize));
            }
        }

        public PathInfo GetPath(Vector2i From, Vector2i To, Hospital.PathType pathType, ExtendedTileData[,] tempData)
        {
            Heap<ExtendedTileData> openSet;
            if (heapPool == null)
                CreateHeapPool();

            if (heapPool.Count > 0)
            {
                openSet = heapPool.Dequeue();
            }
            else
            {
                Debug.LogError("Creating new heap!");
                openSet = new Heap<ExtendedTileData>(tempData.Length);
            }
            try
            {
                openSet.Clear();

                ExtendedTileData startTile = tempData[From.x, From.y];
                ExtendedTileData targetTile = tempData[To.x, To.y];
                openSet.Add(startTile);

                for (int i = 0; i < tempData.GetLength(0); i++)
                {
                    for (int j = 0; j < tempData.GetLength(1); j++)
                    {
                        tempData[i, j].closed = false;
                        tempData[i, j].distance = int.MaxValue;
                        tempData[i, j].previous = Vector2i.zero;
                        tempData[i, j].X = i;
                        tempData[i, j].Y = j;
                    }
                }

                tempData[From.x, From.y].distance = 0;
                while (openSet.Count > 0)
                {
                    var currentTile = openSet.RemoveFirst();
                    tempData[currentTile.X, currentTile.Y].closed = true;

                    if (currentTile.X == targetTile.X && currentTile.Y == targetTile.Y)
                    {
                        openSet.Clear();
                        heapPool.Enqueue(openSet);
                        return RetracePath(startTile, targetTile);
                    }
                    //Check neighbouring tiles
                    foreach (var neighbour in GetNeighbours(new Vector2i(currentTile.X, currentTile.Y), pathType))
                    {
                        ExtendedTileData neighbourTile = tempData[neighbour.x, neighbour.y];
                        if (neighbourTile.closed || !neighbourTile.passable ||
                                                    blockedTiles.Contains(new Vector2(neighbourTile.X, neighbourTile.Y)))
                            continue;

                        int newMovementCostToNeighbour = currentTile.gCost + HowFarInt(currentTile, neighbourTile);
                        if (newMovementCostToNeighbour < neighbourTile.gCost || !openSet.Contains(neighbourTile))
                        {
                            neighbourTile.gCost = newMovementCostToNeighbour;
                            neighbourTile.hCost = HowFarInt(neighbourTile, targetTile);
                            neighbourTile.Parent = currentTile;

                            if (!openSet.Contains(neighbourTile))
                                openSet.Add(neighbourTile);
                        }
                    }
                    #region Old A* implementation
                    /*
                    float foundDistance = int.MaxValue;

                    //found = open.First().Value;
                    found = (Vector2i)openOrdered[0];
                    foreach (DictionaryEntry entry in openOrdered)
                    {
                        Vector2i p = (Vector2i)entry.Value;
                        var node = tempData[p.x, p.y];

                        if (node.closed)
                            continue;

                        float howFarToEnd = HowFar(p, To);
                        float dist = node.distance + howFarToEnd;

                        if (dist <= foundDistance)
                        {
                            if (dist < foundDistance)
                            {
                                found = p;
                                foundDistance = dist;
                            }
                            else if (howFarToEnd < HowFar(found, To))
                            {
                                found = p;
                                foundDistance = dist;
                            }
                        }
                    } 
                    // Reached target point
                    if (found.x == To.x && found.y == To.y)
                        break;

                    openOrdered.Remove(found.x + 1000 * found.y);
                    tempData[found.x, found.y].closed = true;

                    foreach (var p in GetNeighbours(found, pathType))
                    {
                        if (!tempData[p.x, p.y].closed)
                        {
                            int key = p.x + 1000 * p.y;

                            if (!openOrdered.Contains(key))//Key(key))
                                openOrdered.Add(key, p);

                            float dist = tempData[found.x, found.y].distance + HowFar(found, p);

                            if (tempData[p.x, p.y].distance == int.MaxValue || tempData[p.x, p.y].distance > dist)
                            {
                                tempData[p.x, p.y].distance = dist;
                                tempData[p.x, p.y].previous = found;
                            }
                        }
                    }*/
                    #endregion
                }
                openSet.Clear();
                heapPool.Enqueue(openSet);
            }
            catch (Exception e)
            {
                openSet.Clear();
                heapPool.Enqueue(openSet);
                Debug.LogError("Error in get path: " + e.Message + " :: " + e.StackTrace);
                //GetPath(From, To, pathType, tempData);
            }
            return null;
            // No path found
            /*if (found.x != To.x && found.y != To.y)
                return null;

            // Found path
            pathInfo = new PathInfo();
            pathInfo.lvlID = LevelID;
            pathInfo.path = new List<Vector2i>();

            var temp = To;
            if (temp == null)
            {
                Debug.LogError("Problem in finding the path!");
                return null;
            }

            int whileTmpLoopCounter = 0;
            while (temp != From)
            {
                pathInfo.path.Add(temp);
                temp = tempData[temp.x, temp.y].previous;

                if (whileTmpLoopCounter > 2500)
                    return null;

                whileTmpLoopCounter++;
            }

            pathInfo.path.Add(temp);
            pathInfo.path.Reverse();
            dictionaryPool.Enqueue(openOrdered);

            return pathInfo;*/
        }

        PathInfo RetracePath(ExtendedTileData startTile, ExtendedTileData targetTile)
        {
            PathInfo pathInfo = new PathInfo();
            pathInfo.lvlID = LevelID;
            pathInfo.path = new List<Vector2i>();
            ExtendedTileData currentTile = targetTile;
            while (currentTile != startTile)
            {
                pathInfo.path.Add(new Vector2i(currentTile.X, currentTile.Y));
                currentTile = currentTile.Parent;
            }
            pathInfo.path.Add(new Vector2i(startTile.X, startTile.Y));
            pathInfo.path.Reverse();
            return pathInfo;
        }

        int HowFarInt(ExtendedTileData tileA, ExtendedTileData tileB)
        {
            int distX = Mathf.Abs(tileA.X - tileB.X);
            int distY = Mathf.Abs(tileA.Y - tileB.Y);
            if (distX > distY)
                return 14 * distY + 10 * (distX - distY);
            else
                return 14 * distX + 10 * (distY - distX);
        }
        /// <summary>
        /// Check all neighbouring tiles and return all that are same path type(or Default) and passable.
        /// Neighbouring in horizontal, vertical and diagonal tiles.
        /// </summary>
        protected virtual IEnumerable<Vector2i> GetNeighbours(Vector2i elem, Hospital.PathType pathType = Hospital.PathType.Default)
        {
            var tab = new bool[4];
            var ret = new Vector2i();

            // Add all side neighbours to the returnable list
            for (int i = 0; i < 4; ++i)
            {
                int p = i % 2;
                ret.x = elem.x + (1 - p) * (i == 0 ? -1 : 1);
                ret.y = elem.y + p * (i == 3 ? -1 : 1);

                var currentTile = tilesData[ret.x, ret.y];

                if (pathType != Hospital.PathType.Default)
                {
                    bool isSamePathType = false;

                    for (int a = 0; a < currentTile.pathTypes.Length; a++)
                    {
                        if (currentTile.pathTypes[a] == pathType || currentTile.pathTypes[a] == Hospital.PathType.Default)
                        {
                            isSamePathType = true;
                            break;
                        }
                    }

                    tab[i] = (IsPointInBounds(ret.x, ret.y) && currentTile.passable && isSamePathType);
                }
                else
                {
                    tab[i] = (IsPointInBounds(ret.x, ret.y) && currentTile.passable);
                }

                if (tab[i])
                    yield return ret;
            }

            // Add all diagonal neighbour tiles to returnable list
            for (int i = 0; i < 4; ++i)
            {
                ret.x = elem.x + (i % 3 == 0 ? -1 : 1);
                ret.y = elem.y + (i > 1 ? -1 : 1);

                if (tab[i] && tab[(i + 1) % 4] && tilesData[ret.x, ret.y].passable)
                    yield return ret;
            }
            yield break;
        }

        private float HowFar(Vector2i from, Vector2i to) // Faster than light version
        {
            // algorithm with 31 is faster than MathfAbs, and ^ is faster than Mathf.Pow

            var absX = ((to.x - from.x) ^ ((to.x - from.x) >> 31)) - ((to.x - from.x) >> 31);
            var absY = ((to.y - from.y) ^ ((to.y - from.y) >> 31)) - ((to.y - from.y) >> 31);

            //var absX = Mathf.Abs(to.x - from.x);
            //var absY = Mathf.Abs(to.y - from.y);
            return Mathf.Sqrt(absX * absX + absY * absY);
        }

        private int interiorPersonsCount;

        public virtual int InteriorPersonsCount
        {
            get
            {
                return interiorPersonsCount;
            }

            protected set
            {
                interiorPersonsCount = value;
            }
        }

        public void UpdatePersonInterior(BasePersonController person)
        {
            if (person.IsInterior)
                InteriorPersonsCount += 1;
            else
                InteriorPersonsCount -= 1;

            ConfigurePersonLoaded(person);
        }

        public bool isAnyPersonOnTile(Vector2i pos)
        {
            if (tilesData[pos.x, pos.y].persons != null && tilesData[pos.x, pos.y].persons.Count > 0)
            {
                return true;
            }
            else return false;
        }
    }
}
