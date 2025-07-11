using System.Collections.Generic;
using UnityEngine;
using IsoEngine;
//matward make sure you need to do this. inheritance and riding. (Translated from Polish)
namespace Hospital
{
    public class HospitalAreasLevelController : PFLevelController
    {
        private TileWalls[,] walls;
        public AreaMapController map;
        public GameObject corners;
        private GameObject LevelWalls;
        //public List<Doors> doors;

        private float wallScaleY = 1.75f; //this is y scale of Wall(Left/Right) prefab used for correct positioning of the walls

        public override void OnTilePress(Vector2i pressedTile)
        {
            base.OnTilePress(pressedTile);
            map.OnTilePress(pressedTile);
        }

        protected override IEnumerable<Vector2i> GetNeighbours(Vector2i elem, PathType pathType)
        {
            bool south = walls[elem.x, elem.y].CheckWall(Rotation.South);
            bool north = walls[elem.x, elem.y].CheckWall(Rotation.North);
            bool west = walls[elem.x, elem.y].CheckWall(Rotation.West);
            bool east = walls[elem.x, elem.y].CheckWall(Rotation.East);


            if (north && CanBePassed(elem.x + 1, elem.y, pathType))
                yield return new Vector2i(elem.x + 1, elem.y);
            if (south && CanBePassed(elem.x - 1, elem.y, pathType))
                yield return new Vector2i(elem.x - 1, elem.y);
            if (west && CanBePassed(elem.x, elem.y + 1, pathType))
                yield return new Vector2i(elem.x, elem.y + 1);
            if (east && CanBePassed(elem.x, elem.y - 1, pathType))
                yield return new Vector2i(elem.x, elem.y - 1);

            if (north && west && CanBePassed(elem.x + 1, elem.y + 1, pathType) && walls[elem.x + 1, elem.y + 1].CheckWall(Rotation.South) && walls[elem.x + 1, elem.y + 1].CheckWall(Rotation.East))
                yield return new Vector2i(elem.x + 1, elem.y + 1);
            if (north && east && CanBePassed(elem.x + 1, elem.y - 1, pathType) && walls[elem.x + 1, elem.y - 1].CheckWall(Rotation.South) && walls[elem.x + 1, elem.y - 1].CheckWall(Rotation.West))
                yield return new Vector2i(elem.x + 1, elem.y - 1);
            if (south && west && CanBePassed(elem.x - 1, elem.y + 1, pathType) && walls[elem.x - 1, elem.y + 1].CheckWall(Rotation.North) && walls[elem.x - 1, elem.y + 1].CheckWall(Rotation.East))
                yield return new Vector2i(elem.x - 1, elem.y + 1);
            if (south && east && CanBePassed(elem.x - 1, elem.y - 1, pathType) && walls[elem.x - 1, elem.y - 1].CheckWall(Rotation.North) && walls[elem.x - 1, elem.y - 1].CheckWall(Rotation.West))
                yield return new Vector2i(elem.x - 1, elem.y - 1);

        }
        private bool CanBePassed(int x, int y, PathType pathType)
        {
            return IsPointInBounds(x, y) && IsPassable(x, y) && IsPassableWithPathType(x, y, pathType);
        }

        protected override void AddObjectData(IsoObjectData objectData, IsoObject isoObject, PathType[] pathTypes = null)
        {
            //if (!map.reconstructed)
            //{
            //	map.ReconstructFromMapData();
            //}
            PathType[] newPathTypes = { PathType.Default };

            if (pathTypes != null)
                base.AddObjectData(objectData, isoObject, pathTypes);
            else
                base.AddObjectData(objectData, isoObject, newPathTypes);

            IsoObjectPrefabData tempo = engineController.objects[objectData.objectID].GetComponent<IsoObjectPrefabController>().prefabData;
            if (tempo.hasWalls && tempo.walls != null)
            {
                CreateWallsAround(isoObject.X, isoObject.Y, tempo.walls);
            }
        }
        enum Wallz
        {
            wall,
            window,
            door,
        }
        private void AddWallAreaElement(int x, int y, Rotation rotation, Priorities priority, WallType walltype, Wallz type, int doorID = 1, int doorElem = 0, bool near_door = false)
        {

            if (walltype != WallType.None)
            {
                switch (type)
                {
                    case Wallz.wall:
                        AddWall(x, y, rotation, (int)priority, map.wallDatabase.GetWallObject(walltype, (int)rotation % 2 == 0 ? wallType.right : wallType.left), walltype);
                        break;
                    case Wallz.window:
                        AddWall(x, y, rotation, (int)priority, map.wallDatabase.GetWallObject(walltype, (int)rotation % 2 == 0 ? wallType.right : wallType.left), walltype);
                        break;
                    case Wallz.door:

                        GameObject door_tmp;

                        if ((int)rotation % 2 == 0)
                            door_tmp = Instantiate(map.doorPrefabs[doorID].list[doorElem].right);
                        else
                            door_tmp = Instantiate(map.doorPrefabs[doorID].list[doorElem].left);

                        if (door_tmp != null)
                        {
                            door_tmp.GetComponent<Doors>().near_door = near_door;
                            door_tmp.GetComponent<Doors>().display_centerFrame = false;

                            door_tmp.GetComponent<Doors>().position = new Vector2i(x, y);
                            door_tmp.GetComponent<Doors>().wallRotation = rotation;

                            if (doorID == 2 || doorID == 3)
                                door_tmp.GetComponent<Doors>().door_mode = Doors.DoorType.Automatic;
                            else door_tmp.GetComponent<Doors>().door_mode = Doors.DoorType.Classic;

                            AddWall(x, y, rotation, (int)priority, door_tmp, walltype, true);
                            AreaMapController.Map.AddDoorToMap(door_tmp.GetComponent<Doors>());
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        private int AddWallAreaWall(int xFrom, int yFrom, int xTo, int yTo, Rotation rotation, Priorities priority, IsoWallData wall, int actualWall, bool doorsBlockingObjects = false, bool replaceStatic = false)
        {

            int from = 0;
            int to = 0;
            int by = 1;
            bool ifX = true;
            if (xFrom == xTo)
            {
                ifX = false;
                from = yFrom;
                to = yTo;
            }
            else
            {
                from = xFrom;
                to = xTo;
            }
            by = (to - from) > 0 ? 1 : -1;
            //if (by < 0)
            //	print("from " + from + " to " + to + " by " + by + " actual " + actualWall);

            int setedDoor = 0;
            bool doorSideRight = false;

            for (int i = from; i != to + by; i += by)
            {
                if (replaceStatic && walls[ifX ? i : xFrom, ifX ? yFrom : i].CheckStaticWall(rotation))
                {
                    DeleteStaticWall(ifX ? i : xFrom, ifX ? yFrom : i, rotation);
                    actualWall++;
                    continue;
                }
                if (wall.doors.Contains(actualWall))
                {
                    for (int j = 0; j < map.doorPrefabs[wall.DoorID].list.Count; j++)
                    {
                        if (replaceStatic)
                        {
                            if (setedDoor % 2 == 0) doorSideRight = false;
                            else doorSideRight = true;
                        }
                        else if (wall.doors.Count > 0)
                        {
                            if (setedDoor % 2 == 0) doorSideRight = false;
                            else doorSideRight = true;

                        }
                        else if (setedDoor == 0)
                        {
                            doorSideRight = true;
                        }

                        if ((wall.doors.Count > 0) || (wall.OuterWallType == WallType.BrickWalls) || (wall.OuterWallType == WallType.GlassWalls))
                        {
                            if (rotation == Rotation.North || rotation == Rotation.East)
                            {
                                doorSideRight = !doorSideRight;
                                AddWallAreaElement(ifX ? i : xFrom, ifX ? yFrom : i, rotation, priority, rotation == Rotation.East ? wall.OuterWallType : wall.InnerWallType, Wallz.door, wall.DoorID, j, doorSideRight);
                            }
                            else AddWallAreaElement(ifX ? i : xFrom, ifX ? yFrom : i, rotation, priority, rotation == Rotation.South ? wall.OuterWallType : wall.InnerWallType, Wallz.door, wall.DoorID, j, doorSideRight);
                        }
                        else AddWallAreaElement(ifX ? i : xFrom, ifX ? yFrom : i, rotation, priority, (rotation == Rotation.East || rotation == Rotation.South) ? wall.OuterWallType : wall.InnerWallType, Wallz.door, wall.DoorID, j, false);

                        if ((wall.doorPathTypes != null && wall.doorPathTypes.Length > 1) || (replaceStatic))
                        {
                            if (rotation == Rotation.North)
                            {
                                AreaMapController.Map.BlockPath((ifX ? i : xFrom) + 1, ifX ? yFrom : i, wall.doorPathTypes);
                                AreaMapController.Map.BlockBuilding((ifX ? i : xFrom), ifX ? yFrom : i, new PathType[] { PathType.Default });
                            }
                            else AreaMapController.Map.BlockPath(ifX ? i : xFrom, ifX ? yFrom : i, wall.doorPathTypes);
                        }

                        if (doorsBlockingObjects)
                        {
                            //  var id = ((int)rotation % 2 == 0) ? map.reception.impassable1x2ID : map.reception.impassable2x1ID;
                            //  var p = map.AddObject((ifX ? i : xFrom) - (rotation == Rotation.South ? 1 : 0), (ifX ? yFrom : i) - (rotation == Rotation.East ? 1 : 0), id);

                        }
                        actualWall++;
                        i += by;
                        setedDoor++;
                    }
                    i -= by;
                }
                else if (wall.windows.Contains(actualWall))
                {
                    var type = (rotation == Rotation.East || rotation == Rotation.South) ? wall.OuterWallType : wall.InnerWallType;
                    foreach (var p in AreaMapController.Map.wallDatabase.GetWindowParts(type, (int)rotation % 2 == 0 ? wallType.rightWindow : wallType.leftWindow, (rotation == Rotation.East || rotation == Rotation.South) ? wall.OuterWindowID : wall.InnerWindowID))
                    {
                        AddWall(ifX ? i : xFrom, ifX ? yFrom : i, rotation, (int)priority, p, type);
                        actualWall++;
                        i += by;
                    }
                    i -= by;
                }
                else
                {
                    AddWallAreaElement(ifX ? i : xFrom, ifX ? yFrom : i, rotation, priority, (rotation == Rotation.East || rotation == Rotation.South) ? wall.OuterWallType : wall.InnerWallType, wall.windows.Contains(actualWall) ? Wallz.window : Wallz.wall, wall.DoorID, 0, false);
                    ++actualWall;
                }
            }
            //if (by < 0)
            //	print("ended at"+actualWall);
            return actualWall;
        }
        public void AddSpecialStaticWallArea(int x, int y, IsoWallData wall, bool blockDoors = true)
        {
            int actualWall = 0;
            int tilesX = (int)(wall.wallsTo.x - wall.wallsfrom.x);
            int tilesY = (int)(wall.wallsTo.y - wall.wallsfrom.y);

            actualWall = AddWallAreaWall(x, y, x + tilesX - 1, y, Rotation.East, Priorities.SpecialStaticOuter, wall, actualWall, blockDoors);

            actualWall = AddWallAreaWall(x + tilesX - 1, y, x + tilesX - 1, y + tilesY - 1, Rotation.North, Priorities.SpecialStaticInner, wall, actualWall, blockDoors);

            actualWall = AddWallAreaWall(x + tilesX - 1, y + tilesY - 1, x, y + tilesY - 1, Rotation.West, Priorities.SpecialStaticInner, wall, actualWall, blockDoors);

            actualWall = AddWallAreaWall(x, y + tilesY - 1, x, y, Rotation.South, Priorities.SpecialStaticOuter, wall, actualWall, blockDoors);

        }
        public void CreateWallsAround(int x, int y, IsoWallData wall)
        {
            int actualWall = 0;
            x = x + wall.wallsfrom.x;
            y = y + wall.wallsfrom.y;
            int tilesX = (int)(wall.wallsTo.x - wall.wallsfrom.x) + 1;
            int tilesY = (int)(wall.wallsTo.y - wall.wallsfrom.y) + 1;

            actualWall = AddWallAreaWall(x, y, x + tilesX - 1, y, Rotation.East, Priorities.OuterWalls, wall, actualWall);

            actualWall = AddWallAreaWall(x + tilesX - 1, y, x + tilesX - 1, y + tilesY - 1, Rotation.North, Priorities.InnerWals, wall, actualWall);

            actualWall = AddWallAreaWall(x + tilesX - 1, y + tilesY - 1, x, y + tilesY - 1, Rotation.West, Priorities.InnerWals, wall, actualWall);

            actualWall = AddWallAreaWall(x, y + tilesY - 1, x, y, Rotation.South, Priorities.OuterWalls, wall, actualWall);
        }

        public void AddStaticWallArea(int x, int y, IsoWallData wall)
        {
            int actualWall = 0;
            int tilesX = (int)(wall.wallsTo.x - wall.wallsfrom.x);
            int tilesY = (int)(wall.wallsTo.y - wall.wallsfrom.y);
            actualWall = AddWallAreaWall(x, y, x + tilesX - 1, y, Rotation.East, Priorities.OuterStaticWalls, wall, actualWall, true, true);

            actualWall = AddWallAreaWall(x + tilesX - 1, y, x + tilesX - 1, y + tilesY - 1, Rotation.North, Priorities.InnerStaticWalls, wall, actualWall, true, true);

            actualWall = AddWallAreaWall(x + tilesX - 1, y + tilesY - 1, x, y + tilesY - 1, Rotation.West, Priorities.InnerStaticWalls, wall, actualWall, true, true);

            actualWall = AddWallAreaWall(x, y + tilesY - 1, x, y, Rotation.South, Priorities.OuterStaticWalls, wall, actualWall, true, true);
        }
        protected override void RemoveObjectData(IsoObject isoObject)
        {
            if (isoObject != null)
            {
                base.RemoveObjectData(isoObject);

                IsoObjectPrefabData tempo = engineController.objects[isoObject.objectID].GetComponent<IsoObjectPrefabController>().prefabData;

                if (tempo.hasWalls)
                {
                    int objX = isoObject.X + tempo.walls.wallsfrom.x;
                    int objY = isoObject.Y + tempo.walls.wallsfrom.y;
                    int tilesX = tempo.walls.wallsTo.x - tempo.walls.wallsfrom.x + 1;
                    int tilesY = tempo.walls.wallsTo.y - tempo.walls.wallsfrom.y + 1;
                    for (int i = 0; i < tilesX; i++)
                        DeleteWall(objX + i, objY, Rotation.East, (int)Priorities.OuterWalls);

                    for (int i = 0; i < tilesY; i++)
                        DeleteWall(objX + tilesX - 1, objY + i, Rotation.North, (int)Priorities.InnerWals);

                    for (int i = tilesX - 1; i >= 0; --i)
                        DeleteWall(objX + i, objY + tilesY - 1, Rotation.West, (int)Priorities.InnerWals);

                    for (int i = tilesY - 1; i >= 0; --i)
                        DeleteWall(objX, objY + i, Rotation.South, (int)Priorities.OuterWalls);
                }
            }
        }

        public bool CheckIsPositionInsideWalls(IsoObject isoObject, Vector2i pos)
        {
            IsoObjectPrefabData tempo = engineController.objects[isoObject.objectID].GetComponent<IsoObjectPrefabController>().prefabData;
            if (tempo.hasWalls)
            {
                int fromX = isoObject.X + tempo.walls.wallsfrom.x;
                int fromY = isoObject.Y + tempo.walls.wallsfrom.y;
                int toX = isoObject.X + tempo.walls.wallsTo.x;
                int toY = isoObject.Y + tempo.walls.wallsTo.y;

                if ((pos.x >= fromX) && (pos.y >= fromY) && (pos.x <= toX) && (pos.y <= toY))
                {
                    // Debug.LogWarning("|" + fromX + "," + fromY + " : " + toX + "," + toY + " || " + pos.x + "," + pos.y);
                    return true;
                }
            }
            return false;
        }

        public Vector2i GetDoorPosition(IsoObjectPrefabData tempo)
        {
            foreach (var p in tempo.spotsData)
            {
                switch ((SpotTypes)p.id)
                {
                    case SpotTypes.Door:
                        return new Vector2i(p.x, p.y);
                }
            }

            return new Vector2i(-1, -1);
        }

        public Vector2i GetDoorPosition(IsoObject isoObject)
        {
            IsoObjectPrefabData tempo = engineController.objects[isoObject.objectID].GetComponent<IsoObjectPrefabController>().prefabData;

            foreach (var p in tempo.spotsData)
            {
                switch ((SpotTypes)p.id)
                {
                    case SpotTypes.Door:
                        return new Vector2i(p.x, p.y);
                }
            }

            return new Vector2i(-1, -1);
        }

        public void AddWall(int x, int y, Rotation rotation, int priority, GameObject wallObj, WallType wallID, bool isDoor = false)
        {
            int x2 = x;
            int y2 = y;
            if (rotation == Rotation.North)
                x2 += 1;
            if (rotation == Rotation.South)
                x2 -= 1;
            if (rotation == Rotation.East)
                y2 -= 1;
            if (rotation == Rotation.West)
                y2 += 1;
            wallObj.transform.position = new Vector3((x + x2) / 2.0f, wallScaleY / 2, (y + y2) / 2.0f);
            wallObj.transform.rotation = Quaternion.Euler(0, ((int)rotation + 1) % 2 * 90, 0);
            wallObj.transform.SetParent(LevelWalls.transform);
            Wall temp = new Wall(priority, wallObj, wallID, isDoor);
            //Debug.Log("Add wall at " + x.ToString() + " " + y.ToString() + " rot: " + rotation.ToString());
            walls[x, y].AddWall(rotation, temp);
            //Debug.Log("Add wall at " + x2.ToString() + " " + y2.ToString() + " rot: " + ((Rotation)(((int)rotation + 2) % 4)).ToString());
            if (x2 >= 0 && y2 >= 0 && x2 < Width && y2 < Height)
                walls[x2, y2].AddWall((Rotation)(((int)rotation + 2) % 4), temp);

            bool isGlass = false;
            if (wallID == WallType.GlassWalls)
                isGlass = true;
            if (rotation == Rotation.North)
            {
                CheckCorners(new Vector2i(x, y), isGlass);
                CheckCorners(new Vector2i(x, y - 1), isGlass);
            }
            if (rotation == Rotation.West)
            {
                CheckCorners(new Vector2i(x, y), isGlass);
                CheckCorners(new Vector2i(x - 1, y), isGlass);
            }
            if (x2 >= 0 && y2 >= 0 && x2 < Width && y2 < Height)
            {
                if (rotation == Rotation.South)
                {
                    CheckCorners(new Vector2i(x2, y2), isGlass);
                    CheckCorners(new Vector2i(x2, y2 - 1), isGlass);
                }
                if (rotation == Rotation.East)
                {
                    CheckCorners(new Vector2i(x2, y2), isGlass);
                    CheckCorners(new Vector2i(x2 - 1, y2), isGlass);
                }
            }

        }
        private void CheckCorners(Vector2i pos, bool isGlass = false)
        {
            if (isGlass)
                return;

            var east = !walls[pos.x, pos.y].CheckWall(Rotation.North, false);
            var south = !walls[pos.x, pos.y].CheckWall(Rotation.West, false);
            var north = !walls[pos.x + 1, pos.y].CheckWall(Rotation.West, false);
            var west = !walls[pos.x, pos.y + 1].CheckWall(Rotation.North, false);
            if (east && west && south && north)
            {
                walls[pos.x, pos.y].SetCorner(CornerType.quadruple, pos);
                return;
            }
            if ((!(east || west || north || south)) || ((east && west) && (!(north || south))) || ((!(east || west) && (north && south))))
            {
                walls[pos.x, pos.y].SetCorner(CornerType.none, pos);
                return;
            }
            if ((east && !(west || north || south))
                || (west && !(east || north || south))
                    || (north && !(south || west || east))
                        || (south && !(north || west || east)))
            {
                walls[pos.x, pos.y].SetCorner(CornerType.none, pos);
                return;
            }

            //if (east && south && !(north || west))
            //{
            //	walls[pos.x, pos.y].SetCorner(CornerType.cornerTop, pos);
            //	print(pos + " n:" + north + " s:" + south + " e:" + east + " w" + west);
            //}
            //else
            //	walls[pos.x, pos.y].SetCorner(CornerType.none, pos);
            if (east)
            {
                if (west)
                {

                    if (south)
                    {
                        if (north)
                        {
                            walls[pos.x, pos.y].SetCorner(CornerType.quadruple, pos);
                        }
                        walls[pos.x, pos.y].SetCorner(CornerType.tripleNoNorth, pos);
                    }
                    else
                    {
                        if (north)
                            walls[pos.x, pos.y].SetCorner(CornerType.tripleNoSouth, pos);
                        else
                            return;
                    }
                }
                else
                {
                    if (north)
                    {
                        if (south)
                        {
                            if (east)
                                walls[pos.x, pos.y].SetCorner(CornerType.tripleNoWest, pos);
                            else
                                return;
                        }
                        else
                            walls[pos.x, pos.y].SetCorner(CornerType.cornerLeft, pos);
                    }
                    else
                    {
                        walls[pos.x, pos.y].SetCorner(CornerType.cornerTop, pos);
                    }
                }
            }
            else
            {
                if (north)
                {
                    if (south)
                        walls[pos.x, pos.y].SetCorner(CornerType.tripleNoEast, pos);
                    else
                        walls[pos.x, pos.y].SetCorner(CornerType.cornerBotton, pos);
                }
                else
                {
                    walls[pos.x, pos.y].SetCorner(CornerType.cornerRight, pos);
                }
            }
        }
        public void DeleteStaticWall(int x, int y, Rotation rotation)
        {
            int x2 = x;
            int y2 = y;
            if (rotation == Rotation.North)
                x2 += 1;
            if (rotation == Rotation.South)
                x2 -= 1;
            if (rotation == Rotation.East)
                y2 -= 1;
            if (rotation == Rotation.West)
                y2 += 1;
            walls[x, y].DeleteStaticWall(rotation);
            if (x2 >= 0 && y2 >= 0 && x2 < Width && y2 < Height)
                walls[x2, y2].DeleteStaticWall((Rotation)(((int)rotation + 2) % 4));
            if (rotation == Rotation.North)
            {
                CheckCorners(new Vector2i(x, y));
                CheckCorners(new Vector2i(x, y - 1));
            }
            if (rotation == Rotation.West)
            {
                CheckCorners(new Vector2i(x, y));
                CheckCorners(new Vector2i(x - 1, y));
            }
            if (x2 >= 0 && y2 >= 0 && x2 < Width && y2 < Height)
            {
                if (rotation == Rotation.South)
                {
                    CheckCorners(new Vector2i(x2, y2));
                    CheckCorners(new Vector2i(x2, y2 - 1));
                }
                if (rotation == Rotation.East)
                {
                    CheckCorners(new Vector2i(x2, y2));
                    CheckCorners(new Vector2i(x2 - 1, y2));
                }
            }
        }

        public void DeleteWall(int x, int y, Rotation rotation, int priority)
        {
            int x2 = x;
            int y2 = y;
            if (rotation == Rotation.North)
                x2 += 1;
            if (rotation == Rotation.South)
                x2 -= 1;
            if (rotation == Rotation.East)
                y2 -= 1;
            if (rotation == Rotation.West)
                y2 += 1;

            //Debug.Log("Destroying at " + x.ToString() + " " + y.ToString() + " rot: " + rotation.ToString());
            var temp = walls[x, y].DeleteWall(rotation, priority);
            //Debug.Log("Destroying at " + x2.ToString() + " " + y2.ToString() + " rot: " + ((Rotation)(((int)rotation + 2) % 4)).ToString());
            if (x2 >= 0 && y2 >= 0 && x2 < Width && y2 < Height)
                walls[x2, y2].DeleteWall((Rotation)(((int)rotation + 2) % 4), priority);
            if (rotation == Rotation.North)
            {
                CheckCorners(new Vector2i(x, y));
                CheckCorners(new Vector2i(x, y - 1));
            }
            if (rotation == Rotation.West)
            {
                CheckCorners(new Vector2i(x, y));
                CheckCorners(new Vector2i(x - 1, y));
            }
            if (x2 >= 0 && y2 >= 0 && x2 < Width && y2 < Height)
            {
                if (rotation == Rotation.South)
                {
                    CheckCorners(new Vector2i(x2, y2));
                    CheckCorners(new Vector2i(x2, y2 - 1));
                }
                if (rotation == Rotation.East)
                {
                    CheckCorners(new Vector2i(x2, y2));
                    CheckCorners(new Vector2i(x2 - 1, y2));
                }
            }
            if (temp != null)
                temp.Destroy();
        }

        public override void IsoDestroy()
        {
            if (walls != null)
                foreach (var p in walls)
                    p.DestroyAll();
            walls = null;
            base.IsoDestroy();
        }
        internal override void Initialize()
        {
            map = GetComponentInParent<AreaMapController>();
            corners = new GameObject("Corners");
            LevelWalls = new GameObject("Walls");
            corners.transform.SetParent(transform);
            LevelWalls.transform.SetParent(transform);
            base.Initialize();

        }
        public WallType GetTileWallID(Vector2i pos, Rotation rotation)
        {
            return walls[pos.x, pos.y].GetWallID(rotation);
        }
        public override void CreateLevel(IsoLevelData levelData)
        {
            TileWalls.lvl = this;
            if (walls != null)
                throw new IsoException("Previous walls not destroyed!");
            walls = new TileWalls[levelData.tileData.GetLength(0), levelData.tileData.GetLength(1)];
            base.CreateLevel(levelData);

        }

    }
    struct TileWalls
    {
        public static HospitalAreasLevelController lvl;
        Wall[] worseWalls;
        Wall[] walls;
        GameObject upCorner;
        GameObject downCorner;

        public void AddWall(Rotation rotation, Wall wall)
        {
            if (walls == null)
                walls = new Wall[4];
            if (worseWalls == null)
                worseWalls = new Wall[4];
            if (walls[(int)rotation] != null && worseWalls[(int)rotation] != null && walls[(int)rotation].priority < 4 && worseWalls[(int)rotation].priority < 4)
                throw new IsoException("There already are walls in this place!");

            if (walls[(int)rotation] != null)
            {
                if (wall.priority > 3 && walls[(int)rotation].priority > 3)
                {
                    DeleteWall(rotation, walls[(int)rotation].priority);
                    return;
                }
                if (wall.priority > 3)
                {
                    var temp = walls[(int)rotation];
                    wall.SetActive(true);
                    walls[(int)rotation] = wall;
                    temp.SetActive(false);

                    if (temp.priority == (int)Priorities.InnerStaticWalls || temp.priority == (int)Priorities.OuterStaticWalls)
                    {
                        temp.Destroy();
                        return;
                    }
                    var tempo = worseWalls[(int)rotation];
                    if (tempo != null)
                    {

                        tempo.Destroy();
                    }
                    worseWalls[(int)rotation] = temp;
                    return;
                }
                if (walls[(int)rotation].priority == 4 && wall.priority < 2 && !walls[(int)rotation].isDoor && (rotation == Rotation.East || rotation == Rotation.West))
                {
                    DeleteWall(rotation, walls[(int)rotation].priority);
                    //Debug.Log("deleting");
                    wall.SetActive(true);
                    walls[(int)rotation] = wall;

                    return;
                }
                if (walls[(int)rotation].priority > wall.priority)
                {
                    worseWalls[(int)rotation] = wall;
                    wall.SetActive(false);
                    return;
                }
                else
                {
                    if (walls[(int)rotation].priority > 3)
                    {
                        if (wall.priority == (int)Priorities.InnerStaticWalls || wall.priority == (int)Priorities.OuterStaticWalls)
                        {
                            wall.Destroy();
                            return;
                        }
                        else
                        {
                            worseWalls[(int)rotation] = wall;
                        }
                    }
                    worseWalls[(int)rotation] = walls[(int)rotation];
                    worseWalls[(int)rotation].SetActive(false);
                    walls[(int)rotation] = wall;
                    wall.SetActive(true);
                }
            }
            else
            {
                walls[(int)rotation] = wall;
                wall.SetActive(true);
            }
        }

        public void SetCorner(CornerType type, Vector2i position)
        {
            if (upCorner != null)
                GameObject.Destroy(upCorner);
            if (downCorner != null)
                GameObject.Destroy(downCorner);
            if (type == CornerType.none)
                return;

            if (type == CornerType.cornerBotton)
            {
                downCorner = AreaMapController.Map.wallDatabase.GetWallObject(lvl.GetTileWallID(new Vector2i(position.x + 1, position.y), Rotation.West), wallType.outerCorner);
                downCorner.transform.position = new Vector3(position.x + 0.5f, 0, position.y + 0.5f);
            }
            if (type == CornerType.quadruple || type == CornerType.tripleNoNorth || type == CornerType.tripleNoWest || type == CornerType.cornerTop)
            {
                downCorner = AreaMapController.Map.wallDatabase.GetWallObject(walls[(int)Rotation.North].wallID, wallType.innerCorner);
                downCorner.transform.position = new Vector3(position.x + 0.5f, 0, position.y + 0.5f);
            }
            if (type == CornerType.cornerLeft)
            {
                downCorner = AreaMapController.Map.wallDatabase.GetWallObject(walls[(int)Rotation.North].wallID, wallType.leftCorner);
                downCorner.transform.position = new Vector3(position.x + 0.5f, 0, position.y + 0.455f);
            }
            if (type == CornerType.cornerRight)
            {
                downCorner = AreaMapController.Map.wallDatabase.GetWallObject(walls[(int)Rotation.West].wallID, wallType.rightCorner);
                downCorner.transform.position = new Vector3(position.x + 0.455f, 0, position.y + 0.5f);
            }


            if (downCorner != null)
            {
                downCorner.transform.SetParent(lvl.corners.transform);
                downCorner.SetActive(true);


                /*  CODE FOR HIDING CORNERS IF THEY'RE ON GLASS WALLS. SADLY IT DOES NOT WORK. TO FIX
                Rotation rot;
                if (type == CornerType.cornerLeft || type == CornerType.cornerTop)
                    rot = Rotation.North;
                else
                    rot = Rotation.West;
                    
                Debug.Log("Rotation = " + rot);
                Debug.Log("Position = " + position);
                Debug.Log("Type = " + type);
                Debug.LogError("GetTileWallID result = " + lvl.GetTileWallID(position, rot));
                
                if (type != CornerType.cornerBotton && lvl.GetTileWallID(position, rot) == WallType.GlassWalls)
                    return;
                */
                upCorner = GameObject.Instantiate(AreaMapController.Map.WallTop[type]);
                upCorner.transform.SetParent(lvl.corners.transform);
                upCorner.transform.position = new Vector3(position.x + 0.555f, 1.75f + 0.01f, position.y + 0.555f);   //1.75f is from wallScaleY float
            }
        }

        public WallType GetWallID(Rotation rotation)
        {
            return walls[(int)rotation].wallID;
        }


        public Wall DeleteWall(Rotation rotation, int priority)
        {
            if (walls != null && walls[(int)rotation] != null)
            {
                if (walls[(int)rotation].priority == priority)
                {
                    walls[(int)rotation].SetActive(false);
                    var temp = walls[(int)rotation];
                    walls[(int)rotation] = null;

                    if (worseWalls != null && worseWalls[(int)rotation] != null)
                    {
                        walls[(int)rotation] = worseWalls[(int)rotation];
                        worseWalls[(int)rotation] = null;
                        walls[(int)rotation].SetActive(true);
                    }
                    return temp;
                }
                else
                {
                    if (worseWalls[(int)rotation] != null && worseWalls[(int)rotation].priority == priority)
                    {
                        var temp = worseWalls[(int)rotation];
                        worseWalls[(int)rotation] = null;
                        return temp;
                    }
                }
            }

            return null;
            // throw new IsoException("There is no such a wall here:" + rotation.ToString());
        }

        public Wall GetWall(int priority)
        {
            if (walls != null)
            {
                foreach (Wall w in walls)
                {
                    if (w != null)
                        if (w.priority == priority)
                        {
                            return w;
                        }
                }

                foreach (Wall wr in worseWalls)
                {
                    if (wr != null)
                        if (wr.priority == priority)
                        {
                            return wr;
                        }
                }
            }
            return null;
        }

        public bool CheckWall(Rotation rotation, bool countDoors = true)
        {
            return ((walls == null || (walls[(int)rotation] == null && worseWalls[(int)rotation] == null)) || (walls[(int)rotation].isDoor && countDoors));
        }

        public bool CheckWallIsDoor()
        {
            if (walls != null)
            {
                foreach (Wall w in walls)
                {
                    if (w != null)
                    {
                        if (w.isDoor)
                            return true;
                    }
                }
                return false;
            }
            else
                return false;
        }
        public bool CheckStaticWall(Rotation rotation)
        {
            bool var1 = !(walls == null || walls[(int)rotation] == null || (walls[(int)rotation].priority != (int)Priorities.InnerStaticWalls) && (walls[(int)rotation].priority != (int)Priorities.OuterStaticWalls));
            bool var2 = !(worseWalls == null || worseWalls[(int)rotation] == null || (worseWalls[(int)rotation].priority != (int)Priorities.InnerStaticWalls) && (worseWalls[(int)rotation].priority != (int)Priorities.OuterStaticWalls));
            return var1 || var2;
        }

        public Wall DeleteStaticWall(Rotation rotation)
        {
            if (walls != null)
            {
                if (walls[(int)rotation] == null)
                    return null;
                if (walls[(int)rotation].priority == (int)Priorities.InnerStaticWalls || walls[(int)rotation].priority == (int)Priorities.OuterStaticWalls)
                {
                    var temp = walls[(int)rotation];
                    walls[(int)rotation] = worseWalls[(int)rotation];
                    worseWalls[(int)rotation] = null;
                    if (walls[(int)rotation] != null)
                        walls[(int)rotation].SetActive(true);
                    temp.SetActive(false);
                    return temp;
                }
                if (worseWalls[(int)rotation] != null && (worseWalls[(int)rotation].priority == (int)Priorities.InnerStaticWalls || worseWalls[(int)rotation].priority == (int)Priorities.OuterStaticWalls))
                {
                    var temp = worseWalls[(int)rotation];
                    worseWalls[(int)rotation] = null;
                    temp.SetActive(false);
                    return temp;
                }
            }
            return null;
        }
        public void DestroyAll()
        {
            if (walls != null)
            {
                if (walls[0] != null)
                    walls[0].Destroy();

                if (walls[1] != null)
                    walls[1].Destroy();

                if (walls[2] != null)
                    walls[2].Destroy();

                if (walls[3] != null)
                    walls[3].Destroy();
            }
            if (worseWalls != null)
            {
                if (worseWalls[0] != null)
                    worseWalls[0].Destroy();

                if (worseWalls[1] != null)
                    worseWalls[1].Destroy();

                if (worseWalls[2] != null)
                    worseWalls[2].Destroy();

                if (worseWalls[3] != null)
                    worseWalls[3].Destroy();
            }
            walls = null;
            worseWalls = null;
        }
    }
    public enum CornerType
    {
        cornerLeft,
        cornerRight,
        cornerTop,
        cornerBotton,
        tripleNoNorth,
        tripleNoSouth,
        tripleNoWest,
        tripleNoEast,
        quadruple,
        none
    }
}
