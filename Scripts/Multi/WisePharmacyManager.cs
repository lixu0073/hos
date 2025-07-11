using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Hospital
{

    public class WisePharmacyManager
    {

        public int RefreshInterval = 360;
        private long LastRefreshTime = -1; 

        private List<WiseOrder> offers = null;

        public void Initialize(Save save)
        {
            var unparsedOffers = save.WiseOffers;
            offers = unparsedOffers == null ? null : unparsedOffers.Select((x) => WiseOrder.Parse(x)).ToList();
            LastRefreshTime = save.LastRefreshWiseOffers == 0 ? -1 : save.LastRefreshWiseOffers;
        }

        public void SaveState(Save save)
        {
            if (offers != null && offers.Count > 0)
            {
                save.WiseOffers = offers.Select(x => x.ToString()).ToList();
            }
            else
            {
                save.WiseOffers = null;
            }
            save.LastRefreshWiseOffers = LastRefreshTime;
        }

        public void SetBouthOfferAtSortOrder(int sortOrder)
        {
            if(offers == null)
            {
                return;
            }
            foreach(WiseOrder offer in offers)
            {
                if(offer.sortOrder == sortOrder)
                {
                    offer.bought = true;
                    break;
                }
            }
        }

        public List<WiseOrder> GetOffers()
        {
            if(ShouldGenerateOffers())
            {
                offers = GenerateOffers();
            }
            return offers;
        }

        private bool ShouldGenerateOffers()
        {
            return offers == null || LastRefreshTime + RefreshInterval * 60 < ServerTime.getTime();
        }

        private List<WiseOrder> GenerateOffers()
        {
            UpdateLastRefreshTime();
            int level = Game.Instance.gameState().GetHospitalLevel();
            List<WiseOrder> newOffers = new List<WiseOrder>();
            TryToAddOffer(GetRandomCureOfferForLevel(level, 0, newOffers), newOffers);
            TryToAddOffer(GetRandomCureOfferForLevel(level, 1, newOffers), newOffers);
            TryToAddOffer(GetRandomCureOfferForLevel(level, 2, newOffers), newOffers);
            TryToAddOffer(GetRandomCureOfferForLevel(level, 3, newOffers), newOffers);
            TryToAddOffer(GetRandomCureOfferForLevel(level, 4, newOffers), newOffers);
            TryToAddOffer(GetRandomSpecialItemOfferForLevel(level, 5, newOffers), newOffers);
            return newOffers;
        }

        private void TryToAddOffer(WiseOrder offer, List<WiseOrder> list)
        {
            if(offer == null)
            {
                return;
            }
            list.Add(offer);
        }

        public static WiseOrder GetRandomCureOfferForLevel(int level, int sortOrder, List<WiseOrder> tempOffers = null)
        {
            List<MedicineRef> medicines = new List<MedicineRef>();
            foreach (MedicineType type in Enum.GetValues(typeof(MedicineType)))
            {
                if(type == MedicineType.Special)
                {
                    break;
                }
                for (int j = 0; j < ResourcesHolder.Get().medicines.cures[(int)type].medicines.Count; ++j)
                {
                    if (type == MedicineType.BaseElixir && j == 3)
                    {
                        continue;
                    }
                    if (level >= ResourcesHolder.Get().medicines.cures[(int)type].medicines[j].minimumLevel)
                    {
                        MedicineRef med = new MedicineRef(type, j);
                        bool canAdd = true;
                        if (tempOffers != null)
                        {
                            for (int i=0; i< tempOffers.Count; ++i)
                            {
                                if(tempOffers[i].medicine.Equals(med))
                                {
                                    canAdd = false;
                                    break;
                                }
                            }
                        }
                        if (canAdd)
                        {
                            medicines.Add(med);
                        }
                    }
                }
            }
            return WisePharmacyManager.RandOffer(medicines, sortOrder);
        }

        public static WiseOrder GetRandomSpecialItemOfferForLevel(int level, int sortOrder, List<WiseOrder> tempOffers = null)
        {
            List<MedicineRef> medicines = new List<MedicineRef>();
           
            for (int j = 0; j < ResourcesHolder.Get().medicines.cures[(int)MedicineType.Special].medicines.Count; ++j)
            {
                if (level >= ResourcesHolder.Get().medicines.cures[(int)MedicineType.Special].medicines[j].minimumLevel)
                {
                    MedicineRef med = new MedicineRef(MedicineType.Special, j);
                    bool canAdd = true;
                    if (tempOffers != null)
                    {
                        for (int i = 0; i < tempOffers.Count; ++i)
                        {
                            if (tempOffers[i].medicine.Equals(med))
                            {
                                canAdd = false;
                                break;
                            }
                        }
                    }
                    if (canAdd)
                    {
                        medicines.Add(med);
                    }
                }
            }
            return WisePharmacyManager.RandOffer(medicines, sortOrder);
        }

        private static WiseOrder RandOffer(List<MedicineRef> medicines, int sortOrder)
        {
            int medicinesCount = medicines.Count;
            if (medicinesCount == 0)
            {
                return null;
            }
            int ID = GameState.RandomNumber(medicinesCount);
            MedicineRef medicine = medicines[ID];
            WiseOrder offer = new WiseOrder();
            offer.medicine = medicine;
            int amount = (medicine.type == MedicineType.BaseElixir || medicine.type == MedicineType.AdvancedElixir) ? 3 : 1;
            offer.amount = amount;
            offer.pricePerUnit = ResourcesHolder.Get().medicines.cures[(int)medicine.type].medicines[medicine.id].defaultPrice * amount;
            offer.bought = false;
            offer.sortOrder = sortOrder;
            return offer;
        }

        private void UpdateLastRefreshTime()
        {
            if(IsFirstRefreshEver())
            {
                LastRefreshTime = (long)ServerTime.getTime();
            }
            else
            {
                LastRefreshTime = LastRefreshTime + ((int)((long)ServerTime.getTime() - LastRefreshTime) / (RefreshInterval * 60)) * (RefreshInterval * 60);
            }
        }

        private bool IsFirstRefreshEver()
        {
            return LastRefreshTime == -1;
        }

    }
}
