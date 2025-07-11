using UnityEngine;
using System.Collections.Generic;
using IsoEngine;
using SimpleUI;

namespace Hospital
{
    public class ProductionRotatable : RotatableWithoutBuilding
    {
        protected int actualLvl = 1;
        public int id;
        ProductionMachineInfo _infoz = null;
        private ProductionMachineInfo infoz
        {
            get
            {
                if (_infoz == null)
                    Init();
                return _infoz;
            }
            set
            {
                _infoz = value;
            }
        }
        private List<Rotations> upgrades;
        public int actualAmount
        {
            get;
            protected set;
        }
        public int actualLevel
        {
            get
            {
                return actualLvl;
            }
        }

        protected virtual int GetMaxLevel()
        {
            if (_infoz)
            {
                if (_infoz.Levels.Count > 0)
                {
                    return _infoz.Levels.Count;
                }
                else
                    return 1;
            }
            else
                return 1;
        }

        protected int capacityPerLevel = 25;

        public int GetActualCapacity()
        {
            return maximumAmount;
        }
        protected override string SaveToString()
        {
            return base.SaveToString() + ";" + Checkers.CheckedAmount(actualLvl, 0/*1*/, GetMaxLevel(), Tag + " level: ") + "/" + Checkers.CheckedAmount(actualAmount, 0, int.MaxValue, Tag + " currentAmount: ");
        }

        protected override void LoadFromString(string save, TimePassedObject timePassed, int ActionsDone = 0)
        {
            base.LoadFromString(save, timePassed);


            var str = save.Split(';');

            if (str.Length > 1)
            {
                var p = str[1].Split('/');
                int lvl = int.Parse(p[0], System.Globalization.CultureInfo.InvariantCulture);
                int amount = int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture);
                SetLevel(lvl);
                actualAmount = amount;
            }
            else
            {
                actualLvl = 1;
                actualAmount = 0;
            }
            if (this.GetType() == typeof(ElixirStorage))
            {
                if (!HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    if (GameState.Get().ElixirStoreToUpgrade > 0)
                    {
                        Upgrade(GameState.Get().ElixirStoreToUpgrade);
                        GameState.Get().ElixirStoreToUpgrade = 0;
                    }
                }
            }
            else if (this.GetType() == typeof(ElixirTank))
            {
                if (!HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    if (GameState.Get().ElixirTankToUpgrade > 0)
                    {
                        Upgrade(GameState.Get().ElixirTankToUpgrade);
                        GameState.Get().ElixirTankToUpgrade = 0;
                    }
                }
            }

            SetIndicator();
        }

        protected virtual void Init()
        {
            if (_infoz != null)
                return;
            infoz = (ProductionMachineInfo)info.infos;

            upgrades = new List<Rotations>();
            foreach (var g in infoz.VisualLevels)
                upgrades.Add(new Rotations(eng.AddObjectToEngine(g.north), eng.AddObjectToEngine(g.east), eng.AddObjectToEngine(g.south), eng.AddObjectToEngine(g.west), infoz));

        }

        public int maximumAmount
        {
            get
            {
                return GetAmountOnLevel();
            }
        }

        public void Upgrade(int byLevel)
        {
            print("try to upgrade");
            if (infoz == null || actualLvl == GetMaxLevel())
            {
                print("canno't upgrade");
                return;
            }
            if (actualLvl + byLevel > GetMaxLevel())
                byLevel = GetMaxLevel() - actualLvl;

            actualLvl += byLevel;

            if (this.GetType() == typeof(PanaceaCollector))
            {
                for (int i = 0; i < byLevel; i++)
                {
                    AddExp(EconomySource.PanaceaUpgrade);
                }

                Debug.Log("Replenishing collector after upgrading");
                PanaceaCollector pc = GameState.Get().PanaceaCollector;
                pc.Reset(pc.maximumAmount);
            }
            else if (this.GetType() == typeof(ElixirStorage))
            {
                if (VisitingController.Instance.IsVisiting)
                {
                    GameState.Get().ElixirStoreToUpgrade += byLevel;
                }
                else
                {
                    for (int i = 0; i < byLevel; i++)
                        AddExp(EconomySource.ElixirStorageUpgrade);
                }
            }
            else if (this.GetType() == typeof(ElixirTank))
            {
                if (VisitingController.Instance.IsVisiting)
                {
                    GameState.Get().ElixirTankToUpgrade += byLevel;
                }
                else
                {
                    for (int i = 0; i < byLevel; i++)
                        AddExp(EconomySource.ElixirStorageUpgrade);
                }
            }
            AnalyticsController.instance.ReportBuilding(AnalyticsBuildingAction.Upgrade, Tag, area, false, false, actualLevel);


            if (!VisitingController.Instance.IsVisiting)
                GetNewRotation();

            SetIndicator();

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.StorageUpgraded);
        }
        private void AddExp(EconomySource source)
        {
            int expReward = 0;
            if (source == EconomySource.PanaceaUpgrade)
                expReward = _infoz.Levels[actualLevel - 1].UpgradeExp;
            else
                expReward = -8 + (actualLevel * 5);     //storage

            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, expReward, source, false, Tag);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, transform.position, expReward, 0.5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
            });
        }

        private void GetNewRotation()
        {
            if (infoz.VisualLevels != null)
            {
                for (int i = infoz.VisualLevels.Count - 1; i >= 0; i--)
                {
                    if (_infoz.Levels.Count > 0)
                    {
                        if (GetVisualOnLevel() >= infoz.VisualLevels[i].lvlFrom)
                        {
                            SetRotations(upgrades[i]);
                            return;
                        }
                    }
                    else
                    {
                        if (actualLvl >= infoz.VisualLevels[i].lvlFrom)
                        {
                            SetRotations(upgrades[i]);
                            return;
                        }
                    }
                }
            }
        }

        public int GetVisualLevel()
        {
            int lvl = 0;
            if (infoz.VisualLevels != null)
            {
                for (int i = 0; i < infoz.VisualLevels.Count; i++)
                {
                    if (actualLvl >= infoz.VisualLevels[i].lvlFrom)
                    {
                        lvl = i;
                    }
                }
            }
            return lvl;
        }

        public void SetRotations(Rotations rot)
        {

            if (!ReferenceEquals(info, rot))
            {
                //print("changing prefab!");
                SetAnchored(false);
                RemoveFromMap();

                if (temporaryObj != null)
                    GameObject.Destroy(temporaryObj);
                temporaryObj = null;

                info = rot;
                AddToMap();
                SetAnchored(true);
            }
        }
        protected override void AddToMap()
        {
            base.AddToMap();
            SetIndicator();
        }
        public virtual void SetLevel(int lvl, int MaximumLvl = -1)
        {
            actualLvl = lvl;
            GetNewRotation();
        }

        protected void SetIndicator()
        {
            SetIndicator(actualAmount / (float)maximumAmount);
        }
        protected void SetIndicator(float percent)
        {
            var p = infoz.GetIndicatorForPercent(id, percent);
            if (p != null && isoObj != null && isoObj.GetGameObject() != null)
            {
                if (isoObj.GetGameObject().transform.childCount > 0 && isoObj.GetGameObject().transform.GetChild(0).gameObject.transform.childCount > 0 && isoObj.GetGameObject().transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>() != null)
                    isoObj.GetGameObject().transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = p;
            }
        }

        public Sprite GetCurrentIndicator()
        {
            var p = infoz.GetIndicatorForPercent(id, actualAmount / (float)maximumAmount);
            if (p != null && isoObj != null)
            {
                return p;
            }
            return null;
        }

        public virtual int GetAmountOnLevel(bool next = false)
        {
            if (_infoz && _infoz.Levels.Count > 0)
            {
                if (actualLvl + (next ? 1 : 0) > _infoz.Levels.Count)
                {
                    return _infoz.Levels[_infoz.Levels.Count].VisualLevel;// - 1].Amount;
                }
                return _infoz.Levels[Mathf.Clamp(actualLvl + (next ? 1 : 0), 0, _infoz.Levels.Count - 1)].Amount;// - 1].Amount;
            }
            return (actualLvl + (next ? 1 : 0) + 1) * capacityPerLevel;
        }

        public int GetUpgradeCost()
        {
            if (_infoz && _infoz.Levels.Count > 0)
            {
                if (actualLvl + 1 <= _infoz.Levels.Count)
                    return _infoz.Levels[actualLvl].Amount;
            }
            return 0;
        }

        public int GetVisualOnLevel()//
        {
            if (_infoz && _infoz.Levels.Count > 0)
            {
                if (actualLvl >= _infoz.Levels.Count)
                {
                    return _infoz.Levels[_infoz.Levels.Count - 1].VisualLevel;// - 1].VisualLevel;
                }
                return _infoz.Levels[Mathf.Clamp(actualLvl, 0, _infoz.Levels.Count - 1)].VisualLevel;// - 1].VisualLevel;
            }
            else
                return actualLvl;
        }

        public int GetCollectionRateOnLevel(bool next = false)
        {
            if (_infoz && _infoz.Levels.Count > 0)
            {
                if (actualLvl + (next ? 1 : 0) > _infoz.Levels.Count)
                {
                    return _infoz.Levels[_infoz.Levels.Count].CollectionRate;// - 1].CollectionRate;
                }
                return _infoz.Levels[Mathf.Clamp(actualLvl + (next ? 1 : 0), 0, _infoz.Levels.Count - 1)].CollectionRate;// - 1].CollectionRate;
            }
            return 0;
        }

        public int GetMaxCollectionRate()
        {
            if (_infoz && _infoz.Levels.Count > 0)
            {
                return infoz.Levels[_infoz.Levels.Count].CollectionRate;// - 1].CollectionRate;
            }
            else return 0;
        }

        public int GetMaxCapacity()
        {
            if (_infoz && _infoz.Levels.Count > 0)
            {
                return infoz.Levels[_infoz.Levels.Count].Amount;// - 1].Amount;
            }
            else return 0;
        }

        public int GetFreeCapacity()
        {
            return Mathf.Clamp(maximumAmount - actualAmount, 0, 9999);
        }
    }
}