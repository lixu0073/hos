using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;


namespace Hospital
{
	public class UpgradeUseCase_20 : BaseUpgradeUseCase, IUpgradeUseCase
	{
		public Save Upgrade(Save save,bool visitingPurpose)
		{
            ExpandlableAreaAdapter expandableAreaAdapter = new ExpandlableAreaAdapter(save);
            IMapArea expandedArea = expandableAreaAdapter.ExpandSingleExpandableToBuy();

                ElixirStoreAdapter elixirStoreAdapter = new ElixirStoreAdapter(save);
                ElixirTankAdapter elixirTankeAdapter = new ElixirTankAdapter(save);

                if (expandedArea != null)
                {
                    elixirTankeAdapter.SetPosition(expandedArea);
                }
                else
                {
                    Debug.LogError("No expendable areas to buy");

                    Vector2 foundNewPosition = expandableAreaAdapter.GetFreePositionInLaboratory();
                    if (foundNewPosition != Vector2.zero)
                    {
                        elixirTankeAdapter.SetPosition(foundNewPosition);
                    }
                    else
                    {
                        SetAllDecorationFromLaboratoryToStoredDecorationsListInSave(save);
                        Debug.LogError("Move all lab decoration to storage");

                        foundNewPosition = expandableAreaAdapter.GetFreePositionInLaboratory();
                        if (foundNewPosition != Vector2.zero)
                        {
                            elixirTankeAdapter.SetPosition(foundNewPosition);
                        }
                        else throw new IsoEngine.IsoException("Nie ma expendables do kupienia i nie ma miejsca wiec nie mozna zrobic parsowania na nowe magazyny");
                    }
                }

                RecalcStoragesLevels(save, elixirStoreAdapter, elixirTankeAdapter);
                elixirStoreAdapter.UpdateObjectInSave();

                // Nie moze dodawac na koniec bo wczesniejsze obiekty moga chciec sie do niego dostac 
                save.LaboratoryObjectsData.Add("");
                for (int i = save.LaboratoryObjectsData.Count - 1; i>2 ; i--)
                {
                    save.LaboratoryObjectsData[i] = save.LaboratoryObjectsData[i - 1];
                }
                save.LaboratoryObjectsData[2] = elixirTankeAdapter.GetDataToSave();

                //Debug.LogError("dodano expandablesa z elixirTank");
            
            return save;
		}

        public void RecalcStoragesLevels(Save save, ElixirStoreAdapter elixirStorage, ElixirTankAdapter elixirTank)
        {
            string[] p;
            Dictionary<int, int> tmpResources = new Dictionary<int, int>(save.Elixirs.Count);
            save.Elixirs.ForEach(x =>
            {
                p = x.Split('+');
                tmpResources.Add(int.Parse(p[0], System.Globalization.CultureInfo.InvariantCulture), int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture));
            });

            int curentStorageAmount = 30 + (elixirStorage.level * 10);
            int elixirAndMixtureSum = 0, otherElixirsSum = 0, allMedsSum = 0;

            foreach (var tmp in EnumerateResourcesMedRef(tmpResources))
            {
                if (ResourcesHolder.Get().GetIsTankStorageCure(tmp.Key) == true)
                    elixirAndMixtureSum = elixirAndMixtureSum + tmp.Value;
                else otherElixirsSum = otherElixirsSum + tmp.Value;
            }

            allMedsSum = elixirAndMixtureSum + otherElixirsSum;

            tmpResources.Clear();
            tmpResources = null;

            // stary ElixirStorage : 30 + (level * 10)
            // nowy ElixirStorage: 15 + (level * 10)
            // nowy ElixirTank: 15 + (level * 10)

            if (elixirAndMixtureSum <= 25)
                elixirTank.level = 1;
            else
                elixirTank.level = Mathf.CeilToInt((elixirAndMixtureSum - 15) / 10f);

            int oldElixirStorageAmount = 30 + (elixirStorage.level * 10);

            elixirStorage.level = Mathf.CeilToInt((oldElixirStorageAmount - 15) / 10f);
        }

        public void SetAllDecorationFromLaboratoryToStoredDecorationsListInSave(Save save)
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


            // convert laboratory deco to stored objects

            string tmpObjName = "";
            var z = save.LaboratoryObjectsData;
            if (save.LaboratoryObjectsData!=null && save.LaboratoryObjectsData.Count>0)
            {
                for (int i = 0; i< save.LaboratoryObjectsData.Count; i++)
                {
                    var p = z[i].Split('/');
                    if (p.Length > 0)
                    {
                        var stringData1 = p[0].Split('$');
                        if (stringData1.Length > 0)
                        {
                            tmpObjName = stringData1[0];

                            if (HospitalAreasMapController.HospitalMap.drawerDatabase.CheckIsObjectIsDecoration(tmpObjName, HospitalArea.Laboratory))
                            {
                                if (storedObjects.ContainsKey(tmpObjName))
                                    storedObjects[tmpObjName] = storedObjects[tmpObjName] + 1;
                                else
                                    storedObjects.Add(tmpObjName, 1);

                                save.LaboratoryObjectsData.RemoveAt(i);
                                i--;
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

        private List<KeyValuePair<int, int>> EnumerateResources(Dictionary<int, int> tmpResources)
        {
            return tmpResources.Where((x) => { return x.Value != 0; }).ToList();
        }

        public List<KeyValuePair<MedicineRef, int>> EnumerateResourcesMedRef(Dictionary<int, int> tmpResources)
        {
            return EnumerateResources(tmpResources).Select((x) => { return new KeyValuePair<MedicineRef, int>(new MedicineRef((MedicineType)(x.Key / 100), x.Key % 100), x.Value); }).ToList();
        }

    }
}