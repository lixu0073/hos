using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using IsoEngine;


namespace Hospital
{
    public class GameArea
    {
        public readonly HospitalArea myArea;
        AreaMapController map;
        List<IMapArea> areasToBuy;
        List<RectWallInfo> avaiableAreas;
        public List<RectWallInfo> AvailableAreas
        {
            get { return avaiableAreas; }
        }

        public List<RectWallInfo> temporaryAvailableAreas = new List<RectWallInfo>();

        public List<IMapArea> AreasToBuy
        {
            get { return areasToBuy; }
        }
        List<RotatableObject> rotatableObjects;
        internal List<int> boughtAreas;

        GameObject obj;
        GameObject GrassAndTrees;

        public int CountBoughtAreas()
        {
            if (boughtAreas == null)
                return 0;
            return boughtAreas.Count;
        }


        #region LoadSave

        public void LoadObjectsFromSave(List<string> save, TimePassedObject timePassed)
        {
            if (save != null && save.Count > 0)
            {
                var z = save;
                List<string> first = new List<string>();
                List<string> last = new List<string>();
                foreach (var str in z)
                {
                    if (str.Contains("Room"))
                    {
                        last.Add(str);
                    }
                    else
                    {
                        first.Add(str);
                    }
                }
                List<string> orderedSaves = new List<string>();
                orderedSaves.AddRange(first);
                orderedSaves.AddRange(last);
                foreach (var str in orderedSaves)
                {
                    rotatableObjects.Add(RotatableObject.GenerateSingleFromSave(str, timePassed));
                }
                foreach (var p in rotatableObjects)
                    p.transform.SetParent(obj.transform);
                for (int i = 0; i < rotatableObjects.Count; ++i)
                {
                    rotatableObjects[i].LoadFromStringAfterAllRoomsLoaded(orderedSaves[i], timePassed);
                }
                for (int i = 0; i < rotatableObjects.Count; i++)
                {
                    rotatableObjects[i].OnLoadEnded();
                }

            }
        }

        public void Notify(int id, object parameters = null)
        {
            foreach (var p in rotatableObjects)
                p.Notify(id, parameters);
        }

        public bool IsEmpty()
        {
            if (myArea == HospitalArea.Laboratory && (rotatableObjects == null || rotatableObjects.Count == 0))
            {
                return true;
            }
            return false;
        }

        public void SaveObjects(Save save)
        {
            var list = new List<string>();
            foreach (var p in rotatableObjects)
            {
                if (p.state != RotatableObject.State.fresh)
                {
                    list.Add(p.SaveObject());
                }
            }
            switch (myArea)
            {
                case HospitalArea.Clinic:
                    save.ClinicObjectsData = list;
                    break;
                case HospitalArea.Laboratory:
                    save.LaboratoryObjectsData = list;
                    break;
                case HospitalArea.Patio:
                    save.PatioObjectsData = list;
                    break;
                case HospitalArea.MaternityWardClinic:
                    save.MaternityClinicObjectsData = list;
                    break;
                case HospitalArea.MaternityWardPatio:
                    save.MaternityPatioObjectsData = list;
                    break;
                case HospitalArea.Ignore:
                    break;
                default:
                    break;
            }
        }

        public void SaveBoughtAreas(Save save)
        {
            switch (myArea)
            {
                case HospitalArea.Clinic:
                    save.UnlockedClinicAreas = CheckedBoughtAreas(boughtAreas, AreaMapController.Map.mapConfig.HospitalClinic.areas.Count, "Clinic");
                    break;
                case HospitalArea.Laboratory:
                    save.UnlockedLaboratoryAreas = CheckedBoughtAreas(boughtAreas, AreaMapController.Map.mapConfig.Laboratory.areas.Count, "Laboratory");
                    break;
                case HospitalArea.MaternityWardClinic:
                    save.UnlockedMaternityWardClinicAreas = CheckedBoughtAreas(boughtAreas, AreaMapController.Map.mapConfig.MaternityWardClinic.areas.Count, "MaternityClinic");
                    break;
                case HospitalArea.Hospital:
                    break;
                case HospitalArea.Patio:
                    break;
                case HospitalArea.MaternityWardPatio:
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region saveCheck

        private List<int> CheckedBoughtAreas(List<int> boughtAreas, int areasCount, string areaName)
        {
            for (int i = 0; i < boughtAreas.Count; i++)
            {
                if (boughtAreas[i] < 0 || boughtAreas[i] >= areasCount)
                {
                    throw new SaveErrorException("ID: " + boughtAreas[i] + "of " + areaName + " is out of range");
                }
            }
            return boughtAreas;
        }

        #endregion

        #region Objects Manipulation

        public void SetBorders(bool state)
        {
            if (rotatableObjects != null && rotatableObjects.Count > 0)
            {
                for (int i = 0; i < rotatableObjects.Count; i++)
                {
                    rotatableObjects[i].SetBorderActive(state, true);
                }
            }
        }


        public RotatableObject FindRotatableObject(string tag, bool isProbeTabForHintSystem = false)
        {
            for (int i = 0; i < rotatableObjects.Count; ++i)
            {
                // Debug.Log("MACHINE TAG: " + rotatableObjects[i].Tag);
                // if (rotatableObjects[i].Tag == tag)
                // Debug.Log("MACHINE TAG: " + rotatableObjects[i].Tag);

                if (rotatableObjects[i].Tag == tag)
                {
                    if (isProbeTabForHintSystem)
                    {
                        if (rotatableObjects[i].GetComponent<ProbeTable>().GetTableState() == TableState.waitingForUser)
                        {
                            return rotatableObjects[i];
                        }
                    }
                    else return rotatableObjects[i];
                }
            }
            return null;
        }

        public int GetRotatableObjectCounter(string tag)
        {
            int id = 0;

            for (int i = 0; i < rotatableObjects.Count; ++i)
            {
                if (rotatableObjects[i].Tag == tag)
                    id++;
            }
            return id;
        }

        public RotatableObject GetRotatableObject(Vector2i pos)
        {
            if (rotatableObjects != null && rotatableObjects.Count > 0)
            {
                for (int i = 0; i < rotatableObjects.Count; ++i)
                {
                    if (rotatableObjects[i].position == pos)
                        return rotatableObjects[i];
                }
            }
            return null;
        }

        public int GetAllRotatableObjectsSize()
        {
            int size = 0;

            if (rotatableObjects != null && rotatableObjects.Count > 0)
            {
                for (int i = 0; i < rotatableObjects.Count; ++i)
                {
                    switch ((MutalType)rotatableObjects[i].actualData.mutalTiles)
                    {
                        case MutalType.North:
                            size = size + ((rotatableObjects[i].actualData.tilesX + 1) * rotatableObjects[i].actualData.tilesY);
                            break;
                        case MutalType.South:
                            size = size + (rotatableObjects[i].actualData.tilesX * (rotatableObjects[i].actualData.tilesY + 1));
                            break;
                        case MutalType.West:
                            size = size + (rotatableObjects[i].actualData.tilesX * (rotatableObjects[i].actualData.tilesY + 1));
                            break;
                        case MutalType.East:
                            size = size + ((rotatableObjects[i].actualData.tilesX + 1) * rotatableObjects[i].actualData.tilesY);
                            break;
                        default:
                            size = size + (rotatableObjects[i].actualData.tilesX * rotatableObjects[i].actualData.tilesY);
                            break;
                    }
                }

                return size;
            }

            return -1;
        }

        public void AddRotatableObject(RotatableObject go)
        {
            rotatableObjects.Add(go);
            go.transform.SetParent(obj.transform);
        }

        public void RemoveRotatableObject(RotatableObject go)
        {
            rotatableObjects.Remove(go);
            go.transform.SetParent(null);
        }

        public GameArea(HospitalArea area, AreaMapController hospitalMap = null)
        {
            areasToBuy = new List<IMapArea>();
            avaiableAreas = new List<RectWallInfo>();
            rotatableObjects = new List<RotatableObject>();
            myArea = area;
            GameObject.Destroy(GameObject.Find("GameArea " + myArea.ToString()));
            obj = new GameObject("GameArea " + myArea.ToString());
            obj.transform.position = Vector3.zero;
            map = hospitalMap;
            if (map != null)
            {
                obj.transform.SetParent(map.gameObject.transform, false);
            }
            GrassAndTrees = new GameObject("GrassAndTrees");
            GrassAndTrees.transform.SetParent(obj.transform);
        }

        private GameArea(HospitalArea area, List<RectWallInfo> areas)
        {
            avaiableAreas = areas;
            myArea = area;
            obj = new GameObject("GameArea " + myArea.ToString());
            obj.transform.position = Vector3.zero;
        }

        public bool CanContainObject(Vector2i objPos, Vector2i objSize)
        {
            bool temp = ContainsPoint(objPos.x, objPos.y);
            if (!temp)
                return false;

            temp = ContainsPoint(objPos.x + objSize.x - 1, objPos.y);
            if (!temp)
                return false;

            temp = ContainsPoint(objPos.x, objPos.y + objSize.y - 1);
            if (!temp)
                return false;

            temp = ContainsPoint(objPos.x + objSize.x - 1, objPos.y + objSize.y - 1);
            if (!temp)
                return false;

            temp = ContainsPoint(objPos.x + (objSize.x - 1) / 2, objPos.y);
            if (!temp)
                return false;

            temp = ContainsPoint(objPos.x + (objSize.x - 1) / 2, objPos.y + objSize.y - 1);
            if (!temp)
                return false;

            temp = ContainsPoint(objPos.x + objSize.x - 1, objPos.y + (objSize.y - 1) / 2);
            if (!temp)
                return false;

            temp = ContainsPoint(objPos.x, objPos.y + (objSize.y - 1) / 2);
            return temp;

        }

        public bool CanContainObject(IsoObjectData objData, IsoObjectPrefabData prefabData)
        {
            bool temp = ContainsPoint(objData.x, objData.y);
            if (!temp)
                return false;

            temp = ContainsPoint(objData.x + prefabData.tilesX - 1, objData.y);
            if (!temp)
                return false;

            temp = ContainsPoint(objData.x, objData.y + prefabData.tilesY - 1);
            if (!temp)
                return false;

            temp = ContainsPoint(objData.x + prefabData.tilesX - 1, objData.y + prefabData.tilesY - 1);
            if (!temp)
                return false;

            temp = ContainsPoint(objData.x + (prefabData.tilesX - 1) / 2, objData.y);
            if (!temp)
                return false;

            temp = ContainsPoint(objData.x + (prefabData.tilesX - 1) / 2, objData.y + prefabData.tilesY - 1);
            if (!temp)
                return false;

            temp = ContainsPoint(objData.x + prefabData.tilesX - 1, objData.y + (prefabData.tilesY - 1) / 2);
            if (!temp)
                return false;

            temp = ContainsPoint(objData.x, objData.y + (prefabData.tilesY - 1) / 2);
            return temp;

        }

        public bool ContainsPoint(int x, int y)
        {
            return ContainsPoint(new Vector2i(x, y));
        }

        public bool ContainsPoint(Vector2i point, bool fromTemp = false)
        {
            bool temp = false;
            if (fromTemp)
            {
                for (int i = 0; i < temporaryAvailableAreas.Count; ++i)
                {
                    if (temporaryAvailableAreas[i].rect.Contains(point))
                    {
                        temp = true;
                        break;
                    }
                }
            }
            else
            {
                if (avaiableAreas != null && avaiableAreas.Count > 0)
                {
                    for (int i = 0; i < avaiableAreas.Count; i++)
                    {
                        if (avaiableAreas[i].rect.Contains(point))
                        {
                            temp = true;
                            break;
                        }
                    }
                }
            }
            return temp;
        }

        public int GetAreaSize()
        {
            if (areasToBuy.Count > 0)
            {
                int size = 0;

                if (avaiableAreas != null && avaiableAreas.Count > 0)
                {
                    for (int i = 0; i < avaiableAreas.Count; i++)
                    {
                        size = size + (avaiableAreas[i].rect.xSize * avaiableAreas[i].rect.ySize);
                    }

                    return size;
                }
            }

            return -1;
        }

        #endregion


        #region Areas Manipulation

        public void Buy(IMapArea area)
        {
            areasToBuy.Remove(area);
            boughtAreas.Add(area.GetID());
            area.IsoDestroy();
        }

        public IMapArea CheckAreaOfPoint(Vector2i pos)
        {
            return CheckAreaOfPoint(pos.x, pos.y);
        }

        public IMapArea CheckAreaOfPoint(int x, int y)
        {
            if (areasToBuy != null && areasToBuy.Count > 0)
            {
                for (int i = 0; i < areasToBuy.Count; i++)
                {
                    if (areasToBuy[i].ContainsPoint(x, y))
                        return areasToBuy[i];
                }
            }
            return null;
        }

        public void AddRect(RectWallInfo rectangle, bool updateMap = true)
        {
            if (updateMap)
                avaiableAreas.Add(rectangle);
            else
                temporaryAvailableAreas.Add(rectangle);
            if (map != null && updateMap)
            {
                map.AddStaticWallsArea(rectangle.rect.x, rectangle.rect.y, rectangle.data);
            }
        }

        public void AddAreaToBuy(IMapArea area, bool updateMap = true)
        {
            if (updateMap)
            {
                areasToBuy.Add(area);
                area.SetParent(GrassAndTrees.transform);
            }
        }

        public void RemoveRect(RectWallInfo rect)
        {
            avaiableAreas.Remove(rect);
        }

        public void RemoveAreaToBuy(IMapArea area)
        {
            areasToBuy.Remove(area);
        }

        public IMapArea GetAreaToBuy(int id)
        {
            if (areasToBuy != null && areasToBuy.Count > 0)
            {
                for (int i = 0; i < areasToBuy.Count; i++)
                {
                    if (areasToBuy[i].GetID() == id)
                    {
                        return areasToBuy[i];
                    }
                }
            }

            return null;
        }

        public List<IMapArea> GetAreasToBuy()
        {
            return areasToBuy;
        }

        #endregion

        public void IsoDestroy()
        {
            foreach (var p in rotatableObjects.Select(x => x).ToList())
            {
                p.IsoDestroy();
                GameObject.Destroy(p.gameObject);

            }
            foreach (var p in areasToBuy)
                p.IsoDestroy();
        }

    }
}
