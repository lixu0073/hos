using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using IsoEngine;

namespace Hospital
{
    public abstract class BaseUpgradeUseCase : MonoBehaviour
    {
        public class ElixirTankAdapter : BaseAdapter
        {
            public ElixirTankAdapter(Save save) : base(save) {}

            public string Tag = "EliTank";
            public int positionX, positionY;
            public string state = "working";
            public string rotation = "South";
            public int level = 1;
            public int capacity = 10;

            protected override void Initialize(){}

            public void SetPosition(IMapArea expandable)
            {
                Vector3 v = expandable.GetRectStartPoint();
                positionX = (int)v.x + 1;
                positionY = (int)v.z + 1;
            }

            public void SetPosition(Vector2 pos)
            {
                positionX = (int)pos.x;
                positionY = (int)pos.y;
            }

            public string GetDataToSave()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Tag);
                builder.Append("$");
                builder.Append("(" + positionX + "," + positionY + ")");
                builder.Append("/");
                builder.Append(rotation);
                builder.Append("/");
                builder.Append(state);
                builder.Append("/null;");
                builder.Append(level);
                builder.Append("/");
                builder.Append(capacity);
                return builder.ToString();
            }
        }

        public class ElixirStoreAdapter : BaseAdapter
        {
            public ElixirStoreAdapter(Save save) : base(save) { }

            public string Tag = "EliStore";
            public string state, rotation;
            public int positionX, positionY, level, capacity;

            protected override void Initialize()
            {
                var z = save.LaboratoryObjectsData;
                string elixirStorage = null;
                foreach (var str in z)
                {
                    if (str.Contains("EliStore"))
                    {
                        elixirStorage = str;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(elixirStorage))
                {
                    var p = elixirStorage.Split('/');
                    if (p.Length > 0)
                    {
                        var stringData1 = p[0].Split('$');
                        if (stringData1.Length > 0)
                        {
                            var posData = stringData1[1].Split(',');
                            if (posData.Length > 0)
                            {
                                positionX = int.Parse(posData[0].Replace("(", ""), System.Globalization.CultureInfo.InvariantCulture);
                                positionY = int.Parse(posData[1].Replace(")", ""), System.Globalization.CultureInfo.InvariantCulture);
                            }
                        }
                        rotation = p[1];
                        state = p[2];
                        var stringData2 = p[3].Split(';');
                        if (stringData2.Length > 0)
                        {
                            level = int.Parse(stringData2[1], System.Globalization.CultureInfo.InvariantCulture);
                        }

                        capacity = int.Parse(p[4], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else throw new IsoException("Can't get Storage info from save for storages conversion");
                }
            }

            public string GetDataToSave()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Tag);
                builder.Append("$");
                builder.Append("(" + positionX + "," + positionY + ")");
                builder.Append("/");
                builder.Append(rotation);
                builder.Append("/");
                builder.Append(state);
                builder.Append("/null;");
                builder.Append(level);
                builder.Append("/");
                builder.Append(capacity);
                return builder.ToString();
            }

            public void UpdateObjectInSave()
            {
                var z = save.LaboratoryObjectsData;
                for (int a = 0; a < z.Count; a++)
                {
                    if (z[a].Contains("EliStore"))
                    {
                        z[a] = GetDataToSave();
                        return;
                    }
                }
            }
        }

        public class ExpandlableAreaAdapter : BaseAdapter
        {
            private List<SingleRectArea> ExpandablesToBuy = new List<SingleRectArea>();
            private GameArea area;

            public ExpandlableAreaAdapter(Save save) : base(save) {}

            protected override void Initialize()
            {
                Section source = HospitalAreasMapController.HospitalMap.mapConfig.Laboratory;
                List<int> boughtAreas = save.UnlockedLaboratoryAreas;
                area = new GameArea(HospitalArea.Laboratory);
                area.temporaryAvailableAreas.Clear();

                foreach (var p in source.defaultAreas)
                    area.AddRect(new RectWallInfo(p, source.wallType, source.outsideWallType), false);

                var lb = source.areas;

                for (int i = 0; i < lb.Count; ++i)
                    if (boughtAreas.Contains(lb[i].areaID))
                        area.AddRect(new RectWallInfo(lb[i], source.wallType, source.outsideWallType), false);
                    else
                        area.AddAreaToBuy(new SingleRectArea(new RectWallInfo(lb[i], source.wallType, source.outsideWallType), lb[i].areaID, lb[i].areaname, area, lb[i].expansionType, false), false);

                area.boughtAreas = boughtAreas;
            }

            public Vector2 GetFreePositionInLaboratory()
            {
                // ramka laboratorium
                Rectangle labRect = new Rectangle(53,49,17,13);
                // ramka nowego storage
                Rectangle newStorageRect = new Rectangle(1, 1, 3, 3);

                // ramki wszystkich obiektów w laboratorium

                List<Rectangle> labObjectsRects = new List<Rectangle>();

                    string tmpObjName ="";
                    int tmpPosX = 53, tmpPosY = 49;
                    Rotation tmpRot;

                    var z = save.LaboratoryObjectsData;
                    foreach (var str in z)
                    {
                        var p = str.Split('/');
                        if (p.Length > 0)
                        {
                            var stringData1 = p[0].Split('$');
                            if (stringData1.Length > 0)
                            {
                                tmpObjName = stringData1[0];

                                var posData = stringData1[1].Split(',');
                                if (posData.Length > 0)
                                {
                                    tmpPosX = int.Parse(posData[0].Replace("(", ""), System.Globalization.CultureInfo.InvariantCulture);
                                    tmpPosY = int.Parse(posData[1].Replace(")", ""), System.Globalization.CultureInfo.InvariantCulture);
                                }
                            }

                            tmpRot = (Rotation)Enum.Parse(typeof(Rotation), p[1]);

                            Vector2i size = HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectSize(tmpObjName, HospitalArea.Laboratory, tmpRot);
                            for (int i = 0; i < size.x; i++)
                            {
                                for (int j = 0; j < size.y; j++)
                                {
                                    labObjectsRects.Add(new Rectangle(tmpPosX, tmpPosY, size.x, size.y));
                                    //labField[tmpPosX + i - 1, tmpPosY + j - 1] = 1;
                                }
                            }
                        }
                    }

                // sprawdzenie czy moge gdzies wpisac nowy storage
                int a, b;

                for (a = labRect.x; a <= labRect.x + labRect.xSize; a++)
                {
                    for (b = labRect.y; b <= labRect.y + labRect.ySize; b++)
                    {
                        newStorageRect.x = a;
                        newStorageRect.y = b;

                        if (!ContainsRectangle(labObjectsRects, newStorageRect))
                        {
                            return new Vector2(newStorageRect.x, newStorageRect.y);
                        }
                     }
                }

                return Vector2.zero;
            }

            public bool ContainsRectangle(List<Rectangle> allRects, Rectangle rect)
            {
                if (allRects != null && allRects.Count > 0)
                {
                    for (int i = 0; i < allRects.Count; i++)
                    {
                        if (allRects[i].Contains(rect))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public IMapArea ExpandSingleExpandableToBuy()
            {
                foreach(IMapArea expandable in area.AreasToBuy)
                {
                    if(expandable.CanBuy())
                    {
                        area.Buy(expandable);
                        // save.ExpansionsLab++;
                        return expandable;
                    }
                }
                return null;
            }

            public bool LabContainsPosition(Vector2i pos, bool fromTemp = false)
            {
                if (area != null)
                {
                    if (area.ContainsPoint(pos, fromTemp))
                        return true;
                }

                return false;
            }
        }

        public abstract class BaseAdapter
        {
            protected Save save;

            public BaseAdapter(Save save)
            {
                this.save = save;
                Initialize();
            }

            protected abstract void Initialize();

        }

    }
}
