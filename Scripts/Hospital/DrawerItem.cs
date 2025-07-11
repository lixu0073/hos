using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

namespace Hospital
{
    public class DrawerItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private const string REQUIRED_ROOM_KEY = "MATERNITY_WAITING_ROOM_REQUIRED";

        UIDrawerController parent;
        //private float time;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public Image image;
        public Image arrow;
        public Image locked;
        public GameObject costGO;
        public GameObject storedGO;
        public GameObject ownedGO;
        public GameObject usedGO;
        public TextMeshProUGUI cost;
        public TextMeshProUGUI amount;
        public TextMeshProUGUI storedAmount;
        public GameObject badge;
        public Image highlight;
        public GameObject sticker;
        public Image background;
        public TextMeshProUGUI roomTitle;
        public Image nambackBackground;

        [NonSerialized] public DrawerItem depeningItem;

#pragma warning disable 0649
        [SerializeField] private RuntimeAnimatorController animatorHighlightAnimator;
#pragma warning restore 0649
        private Animator highlightAnimator = null;

        private Rotations rot;
        private ShopRoomInfo infos;
        private GameObject container;
        bool shouldBuild = false;
        bool building = false;
        private bool islimitReached = false;
        private bool unlocked;
        Vector3 firstPos;
        int buildAmount = 0;
        private bool lvlPriceVis = false;
        private bool storPriceVis = false;
        //private bool isDiamondNeeded = false;
        private bool isTemporaryObject = false;
        private bool isItemDependent = false;
        private bool isParentUnlocked;

        IEnumerator<float> turnOnHighlighCoroutine;

        private bool unlockedInDrawer;

        public Image adornerImage;
        public GameObject adonerParticles;
        private HospitalAreaInDrawer areaInDrawer;

        public Rotations GetInfo { get { return rot; } }

        public void Initialize(Rotations info, UIDrawerController parent, HospitalAreaInDrawer areaInDrawer)
        {
            rot = info;
            this.parent = parent;
            infos = (ShopRoomInfo)info.infos;
            this.title.SetText(I2.Loc.ScriptLocalization.Get(infos.ShopTitle));
            this.image.sprite = infos.ShopImage;
            this.highlight.sprite = infos.ShopImage;
            this.areaInDrawer = areaInDrawer;

            if (infos.GetType() == typeof(CanInfo))
            {
                this.isTemporaryObject = true;
                this.adornerImage.sprite = (infos as CanInfo).adornerImage;
                this.adornerImage.gameObject.SetActive(true);

                var particles = (infos as CanInfo).adornerParticles;

                if (particles != null)
                {
                    adonerParticles = GameObject.Instantiate(particles);
                    adonerParticles.transform.SetParent(image.transform);
                    adonerParticles.GetComponent<RectTransform>().transform.localPosition = new Vector3(0, -11.2f, 0);
                }
            }
            else
                this.isTemporaryObject = false;

            badge.SetActive(false);
            highlight.gameObject.SetActive(false);
            image.transform.localScale = new Vector3(1, 1, 1);
            Destroy(highlightAnimator);

            if (info.infos is MaternityWaitingRoomInfo roomInfo)
            {
                nambackBackground.sprite = roomInfo.nambackBackground;
                roomTitle.text = I2.Loc.ScriptLocalization.Get(roomInfo.RoomCategory);
            }
        }

        public void UpdateNames()
        {
            this.title.SetText(I2.Loc.ScriptLocalization.Get(infos.ShopTitle));
            if (roomTitle != null)
                roomTitle.text = I2.Loc.ScriptLocalization.Get(((MaternityWaitingRoomInfo)rot.infos).RoomCategory);

            if (infos.unlockLVL <= Game.Instance.gameState().GetHospitalLevel())
                this.description.SetText(I2.Loc.ScriptLocalization.Get(isItemDependent ? REQUIRED_ROOM_KEY : infos.ShopDescription));
            else            
                this.description.SetText(I2.Loc.ScriptLocalization.Get("UNLOCKS_AT") + " " + infos.unlockLVL);

            if (infos.depeningRoom != null)
            {
                if (unlockedInDrawer)            
                    SetUnlocked();
                else
                    SetLocked();
            }
        }

        public void Start()
        {
            UpdateItemInfo();
        }

        public void ShowBadge(int count = 0)
        {
            this.badge.SetActive(true);
            highlight.gameObject.SetActive(true);
            if (!TryGetComponent<Animator>(out highlightAnimator))
                highlightAnimator = gameObject.AddComponent<Animator>();

            highlightAnimator.runtimeAnimatorController = animatorHighlightAnimator;
        }

        public bool isBadge()
        {
            return this.badge.activeSelf;
        }

        public void HideBadge()
        {
            if (this.badge.activeSelf)
            {
                this.badge.SetActive(false);
                highlight.gameObject.SetActive(false);
                image.transform.localScale = new Vector3(1, 1, 1);
                Destroy(highlightAnimator);
                UIController.get.drawer.DecrementMainButtonBadge();
            }
        }

        public void ChangePrice(int newPrice)
        {
            this.cost.text = newPrice.ToString();
        }

        public void UpdateDecoPrices()
        {
            if (GameState.StoredObjects != null && AreaMapController.Map.decoAmountMap != null && AreaMapController.Map.decoAmountMap.Count > 0 && infos.costInDiamonds == 0)
            {
                DecorationInfo decoInfo = infos as DecorationInfo;
                int baseCost = infos.cost;
                BaseGameState.StoredObjects.TryGetValue(infos.Tag, out int amountOfDecoOnPlayerPosess);
                AreaMapController.Map.decoAmountMap.TryGetValue(infos.Tag, out int amountOfDecoOnMaps);
                amountOfDecoOnPlayerPosess += amountOfDecoOnMaps;
                int totalCost = baseCost + amountOfDecoOnPlayerPosess * decoInfo.goldIncreasePerOwnedItem;
                this.cost.text = totalCost.ToString();
            }
        }

        public void UpdateHospitalRoomPrices()
        {
            int totalCost = AlgorithmHolder.GetCostInGoldForHospitalRoom();
            this.cost.text = totalCost.ToString();
        }

        public void UpdateCanPrice()
        {
            if (((ShopRoomInfo)rot.infos).costInDiamonds == 0)
            {
                //int boughtCans = ReferenceHolder.Get().customizationController.GetOwnedFloorColor().Count - 1 - ReferenceHolder.Get().customizationController.PremiumFloorColorCounter;
                //if (boughtCans < 0)
                //    boughtCans = 0;
                //int totalCost = infos.cost + (infos as CanInfo).goldIncreasePerOwnedItem * boughtCans;
                int totalCost = AlgorithmHolder.GetCostInGoldForPaint();
                this.cost.text = totalCost.ToString();
            }
        }

        public void SetUnlocked()
        {
            this.title.SetText(I2.Loc.ScriptLocalization.Get(infos.ShopTitle));
            if (infos.depeningRoom != null)
            {
                DrawerItem dependentItem = UIController.get.drawer.FindDependentItem(infos.depeningRoom).drawerItem;
                dependentItem.SetUiDependend(GameState.BuildedObjects.ContainsKey(infos.Tag), true);
                unlockedInDrawer = true;
            }

            this.description.SetText(I2.Loc.ScriptLocalization.Get(infos.ShopDescription));

            if (islimitReached && isNotPatio())
                return;

            if (isTemporaryObject)
            {
                Vector4 color = (infos as CanInfo).canColor;
                this.image.material = Instantiate(ResourcesHolder.Get().drawerDragableOwnColorMaterial);
                this.image.material.SetVector("_HSVAAdjust", new Vector4(color.x, color.y, color.z, color.w));
            }
            else
                this.image.material = null;

            this.arrow.material = null;
            lvlPriceVis = true;

            if (((ShopRoomInfo)rot.infos).costInDiamonds == 0)
            {
                this.cost.text = ((ShopRoomInfo)rot.infos).cost.ToString();
                costGO.transform.GetChild(1).gameObject.SetActive(true);
                costGO.transform.GetChild(2).gameObject.SetActive(false);
                if (infos.Tag == "WaitingRoomBlueOrchid" || infos.Tag == "LabourRoomBlueOrchid")
                {
                    this.cost.text = I2.Loc.ScriptLocalization.Get("FREE").ToUpper();
                    costGO.transform.GetChild(1).gameObject.SetActive(false);
                    costGO.transform.GetChild(2).gameObject.SetActive(false);
                }
                //isDiamondNeeded = false;
            }
            else
            {
                this.cost.text = ((ShopRoomInfo)rot.infos).costInDiamonds.ToString();
                costGO.transform.GetChild(1).gameObject.SetActive(false);
                costGO.transform.GetChild(2).gameObject.SetActive(true);
                //isDiamondNeeded = true;
            }

            if (isTemporaryObject)
            {
                amount.text = "";
            }
            else if (!rot.infos.multiple)
            {
                amount.text = "0/1";
            }
            else
            {
                if (infos.multipleMaxAmount < 0)
                    amount.text = "";
                else
                {
                    amount.text = buildAmount + "/" + infos.multipleMaxAmount;
                }
            }
            unlocked = true;
        }

        private bool isNotPatio()
        {
            return areaInDrawer != HospitalAreaInDrawer.Patio && areaInDrawer != HospitalAreaInDrawer.MaternityPatio;
        }

        public void SetBuildedAmount(int amount)
        {
            buildAmount = amount;
        }

        public void SetLocked()
        {
            this.image.material = ResourcesHolder.Get().GrayscaleMaterial;
            this.arrow.material = ResourcesHolder.Get().GrayscaleMaterial;
            this.title.SetText(I2.Loc.ScriptLocalization.Get(infos.ShopTitle));

            if (isItemDependent && infos.unlockLVL <= Game.Instance.gameState().GetMaternityLevel())
                this.description.SetText(I2.Loc.ScriptLocalization.Get(REQUIRED_ROOM_KEY));
            else
                this.description.SetText(I2.Loc.ScriptLocalization.Get("UNLOCKS_AT") + " " + infos.unlockLVL);

            this.cost.text = "";
            lvlPriceVis = false;
            amount.text = "";
            unlocked = false;
            if (infos.depeningRoom != null)
                UIController.get.drawer.FindDependentItem(infos.depeningRoom).drawerItem.SetUiDependend(false, false);

            unlockedInDrawer = false;
        }

        public void LockBuild()
        {
            islimitReached = true;
            if (BaseGameState.BuildedObjects.ContainsKey(infos.Tag))
                buildAmount = BaseGameState.BuildedObjects[infos.Tag];
            if (!infos.multiple)
            {
                this.description.SetText(I2.Loc.ScriptLocalization.Get(infos.ShopDescription));
                this.image.material = null;
                this.arrow.material = null;
                this.cost.text = "";
                lvlPriceVis = false;
                amount.text = "1/1";
                this.unlocked = false;
            }
            else
            {
                if (infos.multipleMaxAmount > 0)
                    amount.text = buildAmount + "/" + infos.multipleMaxAmount;

                if (infos.multipleMaxAmount > 0 && infos.multipleMaxAmount <= buildAmount)
                {
                    this.image.material = null;
                    this.arrow.material = null;
                    unlocked = false;
                }
                this.description.SetText(I2.Loc.ScriptLocalization.Get(infos.ShopDescription));
            }
        }

        public void SetContainer(GameObject Container)
        {
            container = Container;
        }

        public void OnMouseDown()
        {
            shouldBuild = false;
            building = false;
            UpdateItemInfo();
            UpdatePriceVisible();

            if (unlocked)
            {
                firstPos = Input.mousePosition;
                transform.localScale = Vector3.one * 1.2f;
            }
        }

        public void OnMouseDrag()
        {
            if (Input.mousePosition.x > firstPos.x + 7 && !building)
            {
                shouldBuild = true;

                if (adornerImage == null)
                    return;

                if (adornerImage.sprite != null)
                    adornerImage.gameObject.SetActive(false);

                if (adonerParticles != null)
                    adonerParticles.SetActive(false);
            }
        }

        public void OnMouseUp()
        {
            //time = -1;
            shouldBuild = false;
            building = false;
            transform.localScale = Vector3.one;

            if (adornerImage == null)
                return;

            if (adornerImage.sprite != null)
                adornerImage.gameObject.SetActive(true);

            if (adonerParticles != null)
                adonerParticles.SetActive(true);
        }

        public void OnMouseExit()
        {
            //time = -1;
            //shouldBuild = false;
            //building = false;
            transform.localScale = Vector3.one;
        }

        public void UpdateItemInfo()
        {
            UpdateAmount();
            UpdateStored();
            UpdatePrice();
        }

        public void Update()
        {
            UpdatePriceVisible();

            if (!unlocked)
                return;

            if (Mathf.Abs(Input.mousePosition.y - firstPos.y) > 35)
                shouldBuild = false;

            if (shouldBuild && unlocked && !building)
            {
                building = true;

                var p = Instantiate(ReferenceHolder.Get().elements.DraggableDrawerItem);
                p.transform.SetParent(UIController.get.canvas.transform);
                p.transform.SetAsLastSibling();
                p.GetComponent<DrawerDraggableItem>().Initialize(infos, parent, rot, () =>
                {
                    CancelInvoke("ShowDrawerImage");
                    Invoke("ShowDrawerImage", 0.1f);
                }, ShowDrawerImage);
                image.enabled = false;
                arrow.enabled = false;
                p.SetActive(true);
                //time = -1;
            }
        }

        public void SetUiDependend(bool isUnlocked, bool isParentUnlocked)
        {
            isItemDependent = true;
            this.isParentUnlocked = isParentUnlocked;
            sticker.SetActive(false);
            background.enabled = false;
            if (isUnlocked)
            {
                locked.gameObject.SetActive(!isUnlocked);
                SetUnlocked();
            }
            else
            {
                locked.gameObject.SetActive(isParentUnlocked);
                SetLocked();
            }
        }

        public void UpdateAmount()
        {
            var tmpCount = GetAmountOnLevel();

            if (BaseGameState.BuildedObjects.ContainsKey(infos.Tag))
                buildAmount = BaseGameState.BuildedObjects[infos.Tag];

            if (tmpCount != -1)
            {
                if (infos.unlockLVL <= Game.Instance.gameState().GetLevelSceneDependent() || (BaseGameState.StoredObjects.ContainsKey(infos.Tag) && BaseGameState.StoredObjects[infos.Tag] > 0))
                {
                    if (infos.Tag == "ProbTab" || infos.Tag == "ElixirLab" || infos.Tag == "2xBedsRoom")
                    {
                        if ((TutorialController.Instance != null && TutorialController.Instance.GetAllFullProbeTablesCount() == 0) && (infos.Tag != "ElixirLab") && (infos.Tag != "2xBedsRoom"))
                        {
                            this.unlocked = false;
                            this.image.material = ResourcesHolder.Get().GrayscaleMaterial;
                            this.arrow.material = ResourcesHolder.Get().GrayscaleMaterial;
                            amount.text = 6 + "/" + tmpCount;
                        }
                        else if (buildAmount < GetAmountOnLevel())
                        {
                            if (!unlocked)
                                SetUnlocked();

                            amount.text = buildAmount + "/" + tmpCount;
                            islimitReached = false;
                        }
                        else
                        {
                            amount.text = buildAmount + "/" + tmpCount;

                            this.unlocked = false;
                            lvlPriceVis = false;
                            this.image.material = null;
                            this.arrow.material = null;
                        }
                    }
                    else if (buildAmount >= tmpCount)
                    {
                        this.unlocked = false;
                        lvlPriceVis = false;
                        this.image.material = null;
                        this.arrow.material = null;
                    }
                    else
                    {
                        if (!unlocked)
                            SetUnlocked();
                    }
                }
                else
                {
                    if (unlocked)
                    {
                        amount.text = buildAmount + "/" + tmpCount;
                        SetLocked();
                    }
                }

                // jesli przeladowanie poziomu to zla ilosc build amount wiec ten if - fix
                if (BaseGameState.BuildedObjects.ContainsKey(infos.Tag))
                {
                    int amount_in_save = BaseGameState.BuildedObjects[infos.Tag];
                    if (buildAmount > amount_in_save)
                        buildAmount = amount_in_save;
                }
            }
        }

        public void UpdateStored()
        {
            if (isTemporaryObject)
            {
                if (ReferenceHolder.Get() != null && ReferenceHolder.Get().floorControllable.IsFloorColorBought(infos.Tag))
                {
                    storPriceVis = false;

                    storedGO.SetActive(false);
                    //matward uzupelnic o maternityward
                    string LaboratoryColorTag = ReferenceHolder.Get().floorControllable.GetCurrentFloorColorName(HospitalArea.Laboratory);
                    string HospitalColorTag = ReferenceHolder.Get().floorControllable.GetCurrentFloorColorName(HospitalArea.Clinic);

                    if ((HospitalColorTag == infos.Tag && areaInDrawer == HospitalAreaInDrawer.Clinic) || (LaboratoryColorTag == infos.Tag && areaInDrawer == HospitalAreaInDrawer.Laboratory))
                    {
                        if (!usedGO.activeSelf)
                        {
                            usedGO.SetActive(true);
                            ownedGO.SetActive(false);
                        }
                    }
                    else
                    {
                        usedGO.SetActive(false);
                        ownedGO.SetActive(true);
                    }

                    SetUnlocked();
                }
                else
                {
                    storPriceVis = true;
                    storedGO.SetActive(false);
                    ownedGO.SetActive(false);
                    usedGO.SetActive(false);
                }

            }
            else if (BaseGameState.StoredObjects.ContainsKey(infos.Tag))
            {
                storPriceVis = false;

                if (!storedGO.activeSelf)
                    storedGO.SetActive(true);

                ownedGO.SetActive(false);
                usedGO.SetActive(false);

                int stored = BaseGameState.StoredObjects[infos.Tag];

                if (int.Parse(storedAmount.text, System.Globalization.CultureInfo.InvariantCulture) != stored)
                    storedAmount.text = BaseGameState.StoredObjects[infos.Tag].ToString();

                SetUnlocked();
            }
            else
            {
                storPriceVis = true;
                storedGO.SetActive(false);
                ownedGO.SetActive(false);
                usedGO.SetActive(false);
            }
        }

        public void UpdatePrice()
        {
            if (infos.Tag == "ElixirLab")
            {
                if (BaseGameState.BuildedObjects.ContainsKey("ElixirLab"))
                {
                    if (BaseGameState.BuildedObjects["ElixirLab"] == 1)
                    {
                        if (cost.text != "3200")
                            cost.text = "3200";
                    }
                }
            }
        }

        public void UpdatePriceVisible()
        {
            bool costGOActive = lvlPriceVis && storPriceVis;
            if (costGO.activeSelf != costGOActive)
            {
                costGO.SetActive(costGOActive);
            }
        }

        public int GetAmountOnLevel()
        {
            if (infos.MaxAmountOnLVL.Length > 0)
            {
                int output_amount = 0;
                foreach (ObjectLevelAmount am in infos.MaxAmountOnLVL)
                {
                    if (am.Level <= Game.Instance.gameState().GetLevelSceneDependent())
                        output_amount = am.Amount;
                    else
                        return output_amount;
                }
                return output_amount;
            }

            return infos.multipleMaxAmount;
        }

        public int GetAmountOnLevel(int lvl)
        {
            if (infos.MaxAmountOnLVL.Length > 0)
            {
                int output_amount = 0;
                foreach (ObjectLevelAmount am in infos.MaxAmountOnLVL)
                {
                    if (am.Level <= lvl)
                        output_amount = am.Amount;
                    else
                        return output_amount;
                }
                return output_amount;
            }

            return infos.multipleMaxAmount;
        }

        public void ShowDrawerImage()
        {
            image.enabled = true;
            arrow.enabled = true;

            if (adornerImage == null) return;

            if (adornerImage.sprite != null)
                adornerImage.gameObject.SetActive(true);

            if (adonerParticles != null)
                adonerParticles.SetActive(true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            container.SendMessage("OnBeginDrag", eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            container.SendMessage("OnEndDrag", eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            container.SendMessage("OnDrag", eventData);
        }

        public bool isStored()
        {
            BaseGameState.StoredObjects.TryGetValue(infos.Tag, out int counter);

            if (infos.dummyType == BuildDummyType.Can)
                return (ReferenceHolder.Get().floorControllable.IsFloorColorBought(infos.Tag));

            return counter > 0;
        }
    }
}
