using UnityEngine;
using System.Collections.Generic;
using IsoEngine;
using System.Text;
using System.Linq;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;

namespace Hospital
{
    public class DrawerDatabase : ScriptableObject
    {

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Drawer/DrawerDatabase")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<DrawerDatabase>();
        }
#endif

        public List<ShopRoomInfo> DrawerItems = new List<ShopRoomInfo>();
        public List<BaseRoomInfo> AdditionalObjects = new List<BaseRoomInfo>();

        public GameObject AreaBlocker;
        public GameObject BuildBlocker;
        public GameObject PathBlocker;

        /*
        public void IncrementRequiredLevelForAll()
        {
            Debug.LogError("IncrementRequiredLevelForAll");
            for (int i = 0; i < DrawerItems.Count; i++)
            {
                if (DrawerItems[i] != null && DrawerItems[i].unlockLVL > 1)
                {
                    DrawerItems[i].unlockLVL++;
                    UnityEditor.EditorUtility.SetDirty(DrawerItems[i]);
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
        */
        public void LogItems()            
        {
            string line = "Tag\tMultiple\tMultipleMaxAmount\tMaxAmountOnLevel\t"+
                            "Cost\tCostInDiamonds\tUnlockLevel\tBuildTimeSeconds\t"+
                            "BuildXPReward\tCureMedRefType\tCureMedRefId\tCureTime\tCureXPReward\tCureCoinsReward\tCurePositiveEnergyReward\t"+
                            "IsVIPRoom\tCureTime\tPositiveEnergyCost\tCureXPReward\tProducedMedicine\tIsDecotration\tGoldIncreasePerOwnedItem\tGoldIncreasePerOwnedItem\n";
            string line2 = "";
            foreach (var item in DrawerItems)
            {
                line += item.Tag + "\t";
                line += item.multiple + "\t";
                line += item.multipleMaxAmount + "\t";
                line += (item.MaxAmountOnLVL == null) ? "null\t" : item.MaxAmountOnLVL.Count().ToString() + "\t";
                line += item.cost + "\t";
                line += item.costInDiamonds + "\t";
                line += item.unlockLVL + "\t";
                line += item.buildTimeSeconds + "\t";
                line += item.buildXPReward + "\t";
                if (item.cure == MedicineRef.invalid)
                {
                    line += "invalid\tinvalid\t";
                }
                else
                {
                    line += item.cure.type + "\t" + item.cure.id + "\t";
                }
                if (item is DoctorRoomInfo)
                {
                    var dr = item as DoctorRoomInfo;
                    line += dr.cureTime + "\t";
                    line += dr.cureXpReward + "\t";
                    line += dr.cureCoinsReward + "\t";
                    line += dr.curePositiveEnergyReward + "\t";
                }
                else
                {
                    line += "N/A\tN/A\tN/A\tN/A\t";
                }
                if (item is HospitalRoomInfo)
                {
                    var hr = item as HospitalRoomInfo;
                    line += hr.IsVIPRoom + "\t";
                }
                else
                {
                    line += "N/A\t";
                }
                if (item is DiagnosticRoomInfo)
                {
                    var dr = item as DiagnosticRoomInfo;
                    line += dr.CureTime + "\t";
                    line += dr.PositiveEnergyCost1 + "\t";
                    line += dr.CureXPReward + "\t";
                }
                else
                {
                    line += "N/A\tN/A\tN/A\t";
                }
                if (item is MedicineProductionMachineInfo)
                {
                    var mpm = item as MedicineProductionMachineInfo;
                    line += mpm.productedMedicine + "\t";
                }
                else
                {
                    line += "N/A\t";
                }
                if (item is DecorationInfo)
                {
                    var d = item as DecorationInfo;
                    line += "true\t";
                    line += "??\t";
                }
                else
                {
                    line += "N/A\tN/A\t";
                }
                if (item is CanInfo)
                {
                    var d = item as CanInfo;
                    line += d.goldIncreasePerOwnedItem + "\t";
                }
                else
                {
                    line += "N/A\t";
                }
                line += "\n";

                //if (item.MaxAmountOnLVL != null)
                //{
                //    foreach (var item2 in item.MaxAmountOnLVL)
                //    {
                //        line2 += item.Tag + "\t" + item2.Level + "\t" + item2.Amount + "\n";
                //    }
                //}
            }

            Debug.Log(line);
            Debug.Log(line2);
            line = "Tag\tMultiple\tMultipleMaxAmount\tMaxAmountOnLevel\n";
            foreach (var item in AdditionalObjects)
            {
                line += item.Tag + "\t";
                line += item.multiple + "\t";
                line += item.multipleMaxAmount + "\t";
                line += (item.MaxAmountOnLVL == null) ? "null\n" : item.MaxAmountOnLVL.Count().ToString() + "\n";
            }

            Debug.Log(line);
        }

        public Vector2i GetObjectSize(string Tag, HospitalArea area, Rotation rot)
        {
            if (DrawerItems != null && DrawerItems.Count > 0)
            {
                for (int i = 0; i < DrawerItems.Count; i++)
                {
                    if (DrawerItems[i].Area == area && DrawerItems[i].Tag == Tag)
                        return GetObjectSizeFromShopInfo(DrawerItems[i], rot);
                }
            }

            if (AdditionalObjects != null && AdditionalObjects.Count > 0)
            {
                for (int i = 0; i < AdditionalObjects.Count; i++)
                {
                    if (AdditionalObjects[i].Area == area && AdditionalObjects[i].Tag == Tag)
                        return GetObjectSizeFromShopInfo(AdditionalObjects[i], rot);
                }
            }

            return Vector2i.zero;
        }

        public ShopRoomInfo GetDecorationByTag(string Tag)
        {
            if (DrawerItems != null && DrawerItems.Count > 0)
            {
                for (int i = 0; i < DrawerItems.Count; i++)
                {
                    if (DrawerItems[i].Tag == Tag && DrawerItems[i] is DecorationInfo)
                        return DrawerItems[i];
                }
            }
            return null;
        }

        public List<ShopRoomInfo> GetDrawerObjectsOfTypes(BuildDummyType[] objectType)
        {
            List<ShopRoomInfo> shoopRooms = new List<ShopRoomInfo>();

            if (objectType != null && objectType.Length > 0)
            {
                if (DrawerItems != null && DrawerItems.Count > 0)
                {
                    for (int i = 0; i < DrawerItems.Count; i++)
                    {
                        for (int a = 0; a < objectType.Length; a++)
                        {
                            if (DrawerItems[i].dummyType == objectType[a])
                                shoopRooms.Add(DrawerItems[i]);
                        }
                    }
                }
            }

            return shoopRooms;
        }

        public bool CheckIsObjectIsDecoration(string Tag, HospitalArea area)
        {
            if (DrawerItems != null && DrawerItems.Count > 0)
            {
                for (int i = 0; i < DrawerItems.Count; i++)
                {
                    if (DrawerItems[i].Area == area && DrawerItems[i].Tag == Tag && DrawerItems[i] is DecorationInfo)
                        return true;
                }
            }

            return false;
        }

        public HospitalArea CheckObjectArea(string Tag)
        {
            if (DrawerItems != null && DrawerItems.Count > 0)
            {
                for (int i = 0; i < DrawerItems.Count; i++)
                {
                    if (DrawerItems[i].Tag == Tag)
                        return DrawerItems[i].Area;
                }
            }

            return HospitalArea.Ignore;
        }

        public Vector2i GetObjectSizeFromShopInfo(BaseRoomInfo info, Rotation rot)
        {
            switch (rot)
            {
                case Rotation.North:
                    return new Vector2i(info.NorthPrefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesX, info.NorthPrefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesY);
                case Rotation.South:
                    return new Vector2i(info.SouthPrefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesX, info.SouthPrefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesY);
                case Rotation.West:
                    return new Vector2i(info.WestPrefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesX, info.WestPrefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesY);
                case Rotation.East:
                    return new Vector2i(info.EastPrefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesX, info.EastPrefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesY);
            }

            return Vector2i.zero;
        }

        public string GetObjectNameFromShopInfo(string Tag)
        {
            if (DrawerItems != null && DrawerItems.Count > 0)
            {
                for (int i = 0; i < DrawerItems.Count; i++)
                {
                    if (DrawerItems[i].Tag == Tag)
                        return DrawerItems[i].ShopTitle;
                }
            }

            return "";
        }

        public ShopRoomInfo GetObjectInfoWithTag(string Tag)
        {
            if (DrawerItems != null && DrawerItems.Count > 0)
            {
                for (int i = 0; i < DrawerItems.Count; i++)
                {
                    if (DrawerItems[i].Tag == Tag)
                        return DrawerItems[i];
                }
            }

            return null;
        }

#if UNITY_EDITOR
        [ContextMenu("Set Deco Exp")]    //option for setting exp values only for Decos
        void SetDecoExp()
        {
            foreach (ShopRoomInfo info in DrawerItems)
            {
                if (info is DecorationInfo)
                {
                    info.buildXPReward = Mathf.CeilToInt(info.unlockLVL / 2.0f);
                }
            }
        }

        public void GetAllData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BuildingName unlockLevel xpreward cost costInDiamonds timetobuild AreaToBulid");
            foreach (var building in DrawerItems)
            {
                sb.AppendLine(building.Tag + " " + building.unlockLVL + " " + building.buildXPReward + " " + building.cost + " " + building.costInDiamonds + " " + building.buildTimeSeconds + " " + building.DrawerArea);
            }
            Debug.LogError(sb.ToString());
        }

        [ContextMenu("RemoveEmpty")]
        public void RemoveEmpty()
        {
            for (int i = DrawerItems.Count - 1; i >= 0; i--)
            {
                if (DrawerItems[i] == null)
                    DrawerItems.RemoveAt(i);
            }
        }
#endif
    }
}