using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using IsoEngine;


namespace Hospital
{
    public class UpgradeUseCase_69 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        ExpandlableAreaAdapter expandableAreaAdapter;
        public Save Upgrade(Save save, bool visitingPurpose)
        {
            expandableAreaAdapter = new ExpandlableAreaAdapter(save);
            SetAllClinicDecorationsFromLaboratoryToStoredDecorationsListInSave(save);
            return save;
        }

        public void SetAllClinicDecorationsFromLaboratoryToStoredDecorationsListInSave(Save save)
        {
            // Get All stored Object

            Dictionary<string, int> storedObjects = new Dictionary<string, int>();

            if (!string.IsNullOrEmpty(save.StoredItems))
            {
                var items = save.StoredItems.Split(';');
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i].Split('/');
                    //var oData = GameState.BuildedObjects;
                    if (storedObjects.ContainsKey(item[0]))
                    {
                        storedObjects[item[0]] = int.Parse(item[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                        storedObjects.Add(item[0], int.Parse(item[1], System.Globalization.CultureInfo.InvariantCulture));
                }
            }


            // convert clinic deco to stored objects

            string tmpObjName = "";
            Vector2i tmpObjPosition;
            var z = save.ClinicObjectsData;
            if (save.ClinicObjectsData != null && save.ClinicObjectsData.Count > 0)
            {
                for (int i = 0; i < save.ClinicObjectsData.Count; i++)
                {
                    var p = z[i].Split('/');
                    if (p.Length > 0)
                    {
                        var stringData1 = p[0].Split('$');
                        if (stringData1.Length > 0)
                        {

                            tmpObjName = stringData1[0];

                            if (HospitalAreasMapController.HospitalMap.drawerDatabase.CheckIsObjectIsDecoration(tmpObjName, HospitalArea.Clinic) || HospitalAreasMapController.HospitalMap.drawerDatabase.CheckIsObjectIsDecoration(tmpObjName, HospitalArea.Hospital))
                            {
                                tmpObjPosition = Vector2i.Parse(stringData1[1]);

                                if (expandableAreaAdapter.LabContainsPosition(tmpObjPosition, true)) { 
                                    if (storedObjects.ContainsKey(tmpObjName))
                                        storedObjects[tmpObjName] = storedObjects[tmpObjName] + 1;
                                    else
                                        storedObjects.Add(tmpObjName, 1);

                                    save.ClinicObjectsData.RemoveAt(i);
                                    i--;
                                }
                                
                            }
                        }
                    }
                }
            }

            // save new stored object list

            if (storedObjects != null && storedObjects.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < storedObjects.Count; i++)
                {
                    string Key = storedObjects.Keys.ElementAt(i);
                    builder.Append(Key);
                    builder.Append('/');
                    builder.Append(Checkers.CheckedAmount(storedObjects[Key], 0, int.MaxValue, "Stored objects " + Key + "amount: ").ToString());
                    if (i < storedObjects.Count - 1)
                    {
                        builder.Append(';');
                    }
                }
                save.StoredItems = builder.ToString();
            }
        }
        
    }
}