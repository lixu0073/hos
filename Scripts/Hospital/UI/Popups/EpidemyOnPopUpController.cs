using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;
using Hospital;
using SimpleUI;
using TMPro;
using I2.Loc;

public class EpidemyOnPopUpController : UIElement
{
#pragma warning disable 0649
    [Header("General")]
    [SerializeField]
    private Button endEpidemyButton;
    [SerializeField]
    private Animator endEpidemyAnim;
    [SerializeField]
    private RuntimeAnimatorController endEpidemyAnimDefault;
    [SerializeField]
    private RuntimeAnimatorController endEpidemyAnimBlinking;
    [SerializeField]
    private Sprite defaultAvatar;
    [SerializeField]
    private GameObject[] visitToDisable;

    [Header("Timing Section")]
    [SerializeField]
    private TextMeshProUGUI timer;

    [Header("Boxes Section")]
    [SerializeField]
    private List<EpidemyBox> boxes = new List<EpidemyBox>();

    [Header("Medicine Specific Info Section")]
    [SerializeField]
    private GameObject selectedMedicineContent;
    [SerializeField]
    private Image selectedMedicineImage;
    [SerializeField]
    private PointerDownListener selectedMedicineListener;
    [SerializeField]
    private Animator medicineAnimator;
    [SerializeField]
    private TextMeshProUGUI selectedMedicineStatus;
    [SerializeField]
    private TextMeshProUGUI coinsPartialReward;
    [SerializeField]
    private Image coinsPartialRewardImage;
    [SerializeField]
    private TextMeshProUGUI expPartialReward;
    [SerializeField]
    private Image expPartialRewardImage;
    [SerializeField]
    private Button helpButton;
    [SerializeField]
    private Button putMedicineIntoBoxButton;
    [SerializeField]
    private TextMeshProUGUI remainingHelps;
    [SerializeField]
    private Localize selectTextLocalize = null;
    //[SerializeField] private Animator rewardAnimator;
    [SerializeField]
    private Animator heliAnimator;
    [SerializeField]
    private Animator epidemyCharacter;
#pragma warning restore 0649
    [TermsPopup]
    [SerializeField]
    private string tapABoxTerm = "-";
    [TermsPopup]
    [SerializeField]
    private string readyToSendTerm = "-";

    public enum EpidemyCharacterAnimation
    {
        Completed
    }

    private Dictionary<EpidemyCharacterAnimation, string> epidemyCharacterAnimations = new Dictionary<EpidemyCharacterAnimation, string>()
    {
        { EpidemyCharacterAnimation.Completed, "EpidemyCompleted" }
    };

    //[Header("Main Reward Section")]
    //[SerializeField] private TextMeshProUGUI boxesAmount;

    [HideInInspector]
    public List<Package> Packages = new List<Package>();

    private int helpsCounter;
    private bool[] packagesFinishedStatus = new bool[12];
    private Package selectedPackage;
    private List<int> medicinesToCollectIndexes = new List<int>();

    private int packagesWithHelp;
    private const int maxPackages = 12;
    private List<MedicineDatabaseEntry> availableMedicines = new List<MedicineDatabaseEntry>();
    private bool rewardAnimations;

    private bool waitingForServer = false;
    private int pastIndex = -1;


    public class Package : CacheManager.IGetPublicSave
    {
        public int MedicineIndex;
        public int PackageID;
        public int Amount;
        public int coinsReward;
        public int expReward;
        public bool IsHelpRequested;
        public bool IsPackageFinished;
        public string HelpedByWhom;
        public bool waitingForHelpAsk = false;
        public bool helperConfirmed = true;

        public Package(int medIndex, int packPrefabId, int medAmount, int coinsRewardAmount, int expRewardAmount, bool helpRequested, bool packageFinished, string helpedByWhom, bool helperConfirmed)
        {
            this.MedicineIndex = medIndex;
            this.PackageID = packPrefabId;
            this.Amount = medAmount;
            this.coinsReward = coinsRewardAmount;
            this.expReward = expRewardAmount;
            this.IsHelpRequested = helpRequested;
            this.IsPackageFinished = packageFinished;
            this.HelpedByWhom = helpedByWhom;
            this.waitingForHelpAsk = false;
            this.helperConfirmed = helperConfirmed;
        }

        public string GetSaveID()
        {
            return HelpedByWhom;
        }
    }

    public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
    {
        yield return base.Open();
        AccountManager.OnFacebookStateUpdate += AccountManager_OnFacebookStateUpdate;
        SetRewardAnimations();
        InfoButtonUp();
        SoundsController.Instance.PlayEpidemyAmbient();

        whenDone?.Invoke();
    }

    public void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        endEpidemyAnimDefault = ReferenceHolder.Get().iphoneXAnimatorChecker.CheckAnimatorForIphoneXCompatiblity(endEpidemyAnimDefault);
        endEpidemyAnimBlinking = ReferenceHolder.Get().iphoneXAnimatorChecker.CheckAnimatorForIphoneXCompatiblity(endEpidemyAnimBlinking);

        if (!HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            Refresh();
            SetNormalModeView();
            StartBouncingAvatars();
        }
        else
        {
            PackageHelpRequestManager.Instance.GetUserPackageRequests(SaveLoadController.SaveState.ID, OnForeignHelpGet);
            SetVisitingModeView();
        }

        DeselectAllBoxes();
        HideSelectedInfo();
    }

    private void StartBouncingAvatars()
    {
        for (int i = 0; i < Packages.Count; i++)
        {
            if (!String.IsNullOrEmpty(Packages[i].HelpedByWhom) && !Packages[i].helperConfirmed)
            {
                boxes[i].SetBounceAnimation();
            }
        }
    }

    private void SetNormalModeView()
    {
        for (int i = 0; i < visitToDisable.Length; i++)
            visitToDisable[i].SetActive(true);
    }

    private void SetVisitingModeView()
    {
        for (int i = 0; i < visitToDisable.Length; i++)
            visitToDisable[i].SetActive(false);

        for (int i = 0; i < Packages.Count; i++)
        {
            if (Packages[i].IsHelpRequested)
            {
                int packageIndex = i;
                boxes[Packages[i].PackageID].MakeHelpButtonVisible();
                // ActivateButton(putMedicineIntoBoxButton, () => HelpWithPackage(Packages[packageIndex]));
                // boxes[Packages[i].PackageID].MakeHelpButtonActive(() => HelpWithPackage(Packages[packageIndex]));
            }
        }
    }

    //checks what has changed in packages since your last login
    private void OnPersonalHelpGet(List<PackageHelpRequest> requests)
    {
        CacheManager.BatchPublicSaves(GetSaveIDsToGetPublicData(requests), () =>
        {
            bool hasAnyHelpRequest = false;
            foreach (var request in requests)
            {
                if (request.helped)
                {
                    int boxId = Convert.ToInt32(request.BoxID);
                    for (int i = 0; i < Packages.Count; i++)
                    {
                        if (Packages[i].PackageID == boxId)
                        {
                            FinishPackageWithHelp(Packages[i], request.ByWhom);
                            break;
                        }
                    }
                }
                else
                {
                    hasAnyHelpRequest = true;
                }
            }
            if (hasAnyHelpRequest != Epidemy.HasAnyHelpRequest)
            {
                Epidemy.HasAnyHelpRequest = hasAnyHelpRequest;
                PublicSaveManager.Instance.UpdatePublicSave();
            }
        }, (ex) =>
        {
            Debug.LogError(ex.Message);
        });
    }

    //checks what has changed in packages in hospital which you are visiting
    private void OnForeignHelpGet(List<PackageHelpRequest> requests)
    {
        CacheManager.BatchPublicSaves(GetSaveIDsToGetPublicData(requests), () =>
        {
            HospitalAreasMapController.HospitalMap.epidemy.HelpMark.SetActive(false);
            packagesWithHelp = 0;

            foreach (var request in requests)
            {
                int boxId = Convert.ToInt32(request.BoxID);
                //marks package as finished but doesnt remove request from server
                if (request.helped)
                {
                    for (int i = 0; i < Packages.Count; i++)
                    {
                        if (Packages[i].PackageID == boxId)
                        {
                            Packages[i].IsPackageFinished = true;
                            SetSpecificClosedPackageLook(boxes[Packages[i].PackageID], request.ByWhom);
                        }
                    }
                }
                //counts packages, which requires help to find out when popup can be disabled
                else
                {
                    if (HospitalAreasMapController.HospitalMap.VisitingMode)
                    {
                        for (int i = 0; i < Packages.Count; i++)
                        {
                            if (Packages[i].PackageID == boxId)
                            {
                                Sprite medicineSprite = ResourcesHolder.Get().GetSpriteForCure(availableMedicines[Packages[i].MedicineIndex].GetMedicineRef());
                                string medicineStatus = "x" + Packages[i].Amount;
                                Package package = Packages[i];
                                int packageID = Packages[i].PackageID;
                                boxes[Packages[i].PackageID].SetOpenBoxLook(medicineSprite, medicineStatus, Packages[i].IsHelpRequested, HospitalAreasMapController.HospitalMap.VisitingMode, () => RefreshSelectedMedicine(package, packageID));
                            }
                        }
                    }
                    HospitalAreasMapController.HospitalMap.epidemy.HelpMark.SetActive(true);
                    packagesWithHelp++;
                }
                if (VisitingController.Instance.IsVisiting)
                {
                    CacheManager.UpdateEpidemyHelpRequest(SaveLoadController.SaveState.ID, packagesWithHelp > 0);
                }
            }
        }, (ex) =>
        {
            Debug.LogError(ex.Message);
        });
    }

    //sets new epidemy and resets variables
    public void SetNewEpidemy()
    {
        for (int i = 0; i < packagesFinishedStatus.Length; i++)
            packagesFinishedStatus[i] = false;

        PopulatePackages();
        //finalRewardInCoins.text = CalculateFinalCoinsReward().ToString();
        //boxesAmount.text = CalculacteBoxesRewardAmount().ToString();
        helpsCounter = 3;
        helpButton.interactable = true;
        helpButton.GetComponent<Image>().material = null;
        rewardAnimations = false;
        SetEndButton();
    }

    //TUTAJ UZUPELNIC FUNKCJE, KTORA ZWROCI ILE PACZEK MA POJAWIC SIE W NASTEPNEJ EPIDEMII
    //ile paczek ma byc w danej epidemii, najlepiej uzaleznic od levelu, na ktorym generowalo sie paczki, zmienna w skrypcie Epidemy -> levelWhileGeneratingMedicines,
    //liczba paczek nie moze byc mniejsza niz liczba roznych lekow w epidemii;
    private int CalculatePackagesAmount()
    {
        return HospitalAreasMapController.HospitalMap.epidemy.EpidemyPackageGenerator.GetNumberOfPackages(HospitalAreasMapController.HospitalMap.epidemy.LevelWhileGeneratingMedicines);
    }

    //ile przedmiotow ma byc do zebrania w jednej paczce, najlepiej uzaleznic od levelu, na ktorym generowalo sie paczki, zmienna w skrypcie Epidemy -> numberOfDifferentMedicinesInEpidemy;
    private int CalculateMedicineQuantityInPackage()
    {
        return GameState.RandomNumber(1, 5);
    }

    //ile kasy za paczke
    private int CalculateCoinRewardForPackage()
    {
        return GameState.RandomNumber(200, 500);
    }

    //ile expa za paczke
    private int CalculateExpRewardForPackage()
    {
        return GameState.RandomNumber(200, 500);
    }

    //ile kasy jako nagroda glowna, najlepiej uzalezniony od skrzynek np suma wszystkich * 1.5 zeby nie trzeba bylo robic sava tej wartości
    private int CalculateFinalCoinsReward()
    {
        return 1;
    }

    //ile boxow jako nagroda glowna, podobnie jak wyzej, najlepiej uzaleznic od skrzynek (NIE UWZGLEDNIALEM KODU, KTORY WRECZA SKRZYNKE, TRZEBA DOPISAC W METODZIE EndEventWithAllPackages)
    private int CalculacteBoxesRewardAmount()
    {
        return 1;
    }

    //creates new random packages
    private void PopulatePackages()
    {
        //availableMedicines = ResourcesHolder.Get().EnumerateKnownMedicinesForLvl(HospitalAreasMapController.Map.epidemy.LevelWhileGeneratingMedicines);
        availableMedicines = ResourcesHolder.Get().GetMedicines();
        Packages.Clear();
        medicinesToCollectIndexes = HospitalAreasMapController.HospitalMap.epidemy.SelectedMedicinesIndexes;
        List<PackageData> packages = HospitalAreasMapController.HospitalMap.epidemy.EpidemyPackageGenerator.GetPackagesForGivenMedicineType(HospitalAreasMapController.HospitalMap.epidemy.LevelWhileGeneratingMedicines, GetIncomingMedicines());
        CreateBoxesWithDifferentMedicines(packages);

        SetEndButton();
    }

    private MedicineDatabaseEntry[] GetIncomingMedicines()
    {
        MedicineDatabaseEntry[] incommingMedicines = new MedicineDatabaseEntry[HospitalAreasMapController.HospitalMap.epidemy.NumberOfDifferentMedicinesInEpidemy];
        int index = 0;
        foreach (int medicineIndex in medicinesToCollectIndexes)
        {
            incommingMedicines[index] = GetMedicineOfIndex(medicineIndex);
            ++index;
        }
        return incommingMedicines;
    }

    private MedicineDatabaseEntry GetMedicineOfIndex(int index)
    {
        return availableMedicines[index];
    }

    private int GetMedicineIndex(MedicineDatabaseEntry medicine)
    {
        return availableMedicines.IndexOf(medicine);
    }

    private List<int> CreateBoxesWithDifferentMedicines(List<PackageData> packages)
    {
        List<int> boxesWithDifferentMedicines = new List<int>();
        int PackageId = 0;
        foreach (PackageData package in packages)
        {
            int medicineIndex = GetMedicineIndex(package.Medicine);
            boxesWithDifferentMedicines.Add(medicineIndex);
            CreatePackage(PackageId, medicineIndex, package.AmountOfMedicine, package.GoldReward, package.ExpReward, false, false, "", false);
            PackageId++;
        }
        DisableUnusedBoxes(packages.Count);
        return boxesWithDifferentMedicines;
    }

    private List<int> CreateBoxesWithDifferentMedicines(int packagesAmount)
    {
        List<int> boxesWithDifferentMedicines = new List<int>();

        for (int i = 0; i < medicinesToCollectIndexes.Count; ++i)
        {
            int randomBoxIndexId;
            int medicineIndex;
            MedicineDatabaseEntry medicine;
            do
            {
                randomBoxIndexId = UnityEngine.Random.Range(0, packagesAmount);
                medicineIndex = medicinesToCollectIndexes[i];
            }
            while (boxesWithDifferentMedicines.Contains(randomBoxIndexId));
            boxesWithDifferentMedicines.Add(randomBoxIndexId);
            medicine = availableMedicines[medicineIndex];
            CreatePackage(randomBoxIndexId, medicineIndex, CalculateMedicineQuantityInPackage(), CalculateCoinRewardForPackage(), CalculateExpRewardForPackage(), false, false, "", false);
        }

        return boxesWithDifferentMedicines;
    }

    private void CreateRestOfBoxes(List<int> alreadyUsedBoxes, int packagesAmount)
    {
        for (int i = 0; i < packagesAmount; ++i)
        {
            int candidateIndex = i;
            if (!alreadyUsedBoxes.Contains(candidateIndex))
            {
                int randomMedicine = UnityEngine.Random.Range(0, medicinesToCollectIndexes.Count);
                int medicineIndex = medicinesToCollectIndexes[randomMedicine];
                int packageID = candidateIndex;
                CreatePackage(packageID, medicineIndex, CalculateMedicineQuantityInPackage(), CalculateCoinRewardForPackage(), CalculateExpRewardForPackage(), false, false, "", false);
            }
        }

        DisableUnusedBoxes(packagesAmount);
    }

    private void DisableUnusedBoxes(int numberOfUsedBoxes)
    {
        for (int i = numberOfUsedBoxes; i < maxPackages; i++)
        {
            boxes[i].SetDisabled();
        }
    }

    private bool IsHelpRequestedOnPackage(bool helpReqested, bool isFinished)
    {
        return helpReqested && !isFinished;
    }

    private void CreatePackage(int packageID, int medicineIndex, int amount, int coinsReward, int expReward, bool helpRequested, bool isFinished, string helpedByWhom, bool helperConfirmed)
    {
        Package tempPackage = new Package(medicineIndex, packageID, amount, coinsReward, expReward, helpRequested, isFinished, helpedByWhom, helperConfirmed);
        Packages.Add(tempPackage);

        if (isFinished)
        {

            packagesFinishedStatus[packageID] = true;

            boxes[packageID].SetClosedBoxWithoutHelpLook();
        }
        else
        {
            packagesFinishedStatus[packageID] = false;
            Sprite medicineSprite = availableMedicines.Count > medicineIndex && availableMedicines[medicineIndex] != null && availableMedicines[medicineIndex].GetMedicineRef() != null ? ResourcesHolder.Get().GetSpriteForCure(availableMedicines[medicineIndex].GetMedicineRef()) : null;
            string medicineStatus = "x" + amount;

            boxes[packageID].SetOpenBoxLook(medicineSprite, medicineStatus, helpRequested, HospitalAreasMapController.HospitalMap.VisitingMode, () => RefreshSelectedMedicine(tempPackage, packageID));
        }
    }

    public void Refresh()
    {
        PackageHelpRequestManager.Instance.GetMyPackageRequests(RefreshEpidemyHelpStatus);
    }

    public void RefreshMedicinesStatus()
    {
        for (int i = 0; i < CalculatePackagesAmount(); ++i)
        {
            if (i < Packages.Count && Packages[i] != null && availableMedicines.Count > Packages[i].MedicineIndex && availableMedicines[Packages[i].MedicineIndex] != null)
            {
                string medicineStatus = "x" + Packages[i].Amount;
                boxes[Packages[i].PackageID].RefreshMedicineStatus(medicineStatus);
            }
        }
    }

    private void RefreshEpidemyHelpStatus(List<PackageHelpRequest> requests)
    {
        CacheManager.BatchPublicSaves(GetSaveIDsToGetPublicData(requests), () =>
        {
            bool hasAnyHelpRequest = false;
            foreach (var request in requests)
            {
                if (request.helped)
                {
                    FinishPackageWithHelp(Packages[Convert.ToInt32(request.BoxID)], request.ByWhom);
                }
                else
                {
                    hasAnyHelpRequest = true;
                }
            }
            if (!HospitalAreasMapController.HospitalMap.VisitingMode && hasAnyHelpRequest != Epidemy.HasAnyHelpRequest)
            {
                Epidemy.HasAnyHelpRequest = hasAnyHelpRequest;
                PublicSaveManager.Instance.UpdatePublicSave();
            }
        }, (ex) =>
        {
            Debug.LogError(ex.Message);
        });
    }

    //puts and updates info about selected package
    private void RefreshSelectedMedicine(Package package, int index)
    {
        MedicineRef medicine = availableMedicines[package.MedicineIndex].GetMedicineRef();

        selectedPackage = package;

        ShowSelectedInfo();
        selectedMedicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);

        if (availableMedicines[package.MedicineIndex].GetMedicineRef().type == MedicineType.BasePlant)
        {
            selectedMedicineListener.SetDelegate(() =>
            {
                FloraTooltip.Open(medicine);
            });
        }
        else
        {
            selectedMedicineListener.SetDelegate(() =>
            {
                TextTooltip.Open(medicine);
            });
        }
        int medicineStorageCount = 0;
        if (medicine.type == MedicineType.Fake)
        {
            ResourceType resourceType = ResourceType.PositiveEnergy;

            if (ResourcesHolder.Get().medicines.cures[(int)medicine.type].medicines[medicine.id].Name == "SPECIAL_ITEMS/POSITIVE_ENERGY")
            {
                resourceType = ResourceType.PositiveEnergy;
            }
            else
            {
                Debug.LogError("Check cure Name, don't call Marian");
                return;
            }

            medicineStorageCount = GameState.Get().GetResourceAmount(resourceType);

        }
        else
        {
            medicineStorageCount = GameState.Get().GetCureCount(medicine);
        }

        if (medicineStorageCount >= package.Amount)
            selectedMedicineStatus.text = medicineStorageCount + "/" + package.Amount;
        else
            selectedMedicineStatus.text = "<color=red>" + medicineStorageCount + "</color>/" + package.Amount;

        coinsPartialReward.text = package.coinsReward.ToString();
        expPartialReward.text = package.expReward.ToString();
        remainingHelps.text = string.Format("({0}/3)", helpsCounter);
        Animator medAnim = selectedMedicineImage.gameObject.GetComponent<Animator>();
        medAnim.SetTrigger("Tap");

        if (!HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            if (!package.IsHelpRequested && helpsCounter > 0)
                ActivateYellow9SliceButton(helpButton, () => AskForHelpWithCollectingMedicine(package));
            else
                Deactivate9sliceButton(helpButton);
            //if (GameState.Get().GetCureCount(availableMedicines[package.MedicineIndex].GetMedicineRef()) >= package.Amount)
            ActivateOvalButton(false, putMedicineIntoBoxButton, () => PutMedicineIntoBox(package));
            //else
            //DeactivateButton(putMedicineIntoBoxButton);
        }
        else
        {
            if (pastIndex != index)
            {
                waitingForServer = false;
            }
            if (!waitingForServer)
            {
                ActivateOvalButton(true, putMedicineIntoBoxButton, () => HelpWithPackage(package));
            }
            helpButton.gameObject.SetActive(false);
        }
        RefreshMedicinesStatus();
        DeselectAllBoxes();
        SelectBox(index);
        pastIndex = index;
    }

    void ShowSelectedInfo()
    {
        selectedMedicineContent.SetActive(true);
        helpButton.gameObject.SetActive(true);
        putMedicineIntoBoxButton.gameObject.SetActive(true);

        selectTextLocalize.gameObject.SetActive(false);
    }

    void HideSelectedInfo()
    {
        selectedMedicineContent.SetActive(false);
        helpButton.gameObject.SetActive(false);
        putMedicineIntoBoxButton.gameObject.SetActive(false);

        selectTextLocalize.gameObject.SetActive(true);
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
            selectTextLocalize.SetTerm(tapABoxTerm);
        else if (GetFinishedPackagesCounter() == CalculatePackagesAmount())
            selectTextLocalize.SetTerm(readyToSendTerm);
        else
            selectTextLocalize.SetTerm(tapABoxTerm);
    }

    void SelectBox(int index)
    {
        boxes[index].SetSelected(true);
        SetAnimator(index, "Tap");
    }

    void DeselectAllBoxes()
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            boxes[i].SetSelected(false);
            SetAnimator(i, "Inactive");
        }
    }

    private int GetFinishedPackagesCounter()
    {
        int counter = 0;

        for (int i = 0; i < packagesFinishedStatus.Length; i++)
        {
            if (packagesFinishedStatus[i])
                counter++;
        }

        return counter;
    }

    public void SetAnimator(int index, string trigger)
    {
        boxes[index].anim.ResetTrigger("Inactive");
        boxes[index].anim.SetTrigger(trigger);
    }

    //makes help available for selected medicine
    private void AskForHelpWithCollectingMedicine(Package package)
    {
        if (helpsCounter > 0 && !package.IsHelpRequested && !package.waitingForHelpAsk)
        {
            boxes[package.PackageID].MakeHelpButtonVisible();
            package.IsHelpRequested = true;
            helpsCounter--;
            remainingHelps.text = string.Format("({0}/3)", helpsCounter);
            Deactivate9sliceButton(helpButton);

            AnalyticsController.instance.ReportSocialHelp(SocialHelpAction.RequestHelpEpidemy, null);
            package.waitingForHelpAsk = true;
            PackageHelpRequestManager.Instance.PostPackageHelpRequest(package.PackageID.ToString(), () =>
            {
                Epidemy.HasAnyHelpRequest = true;
                package.waitingForHelpAsk = false;
                PublicSaveManager.Instance.UpdatePublicSave();
                SaveSynchronizer.Instance.InstantSave();
            });
            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.AskForHelp));
            SaveSynchronizer.Instance.InstantSave();
        }
    }

    private void HelpWithPackage(Package package)
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            MedicineRef medicine = availableMedicines[package.MedicineIndex].GetMedicineRef();

            if (medicine.type == MedicineType.Fake)
            {
                ResourceType resourceType = ResourceType.PositiveEnergy;

                if (ResourcesHolder.Get().medicines.cures[(int)medicine.type].medicines[medicine.id].Name == "SPECIAL_ITEMS/POSITIVE_ENERGY")
                {
                    resourceType = ResourceType.PositiveEnergy;
                }
                else
                {
                    Debug.LogError("Check cure Name, don't call Marian");
                    return;
                }

                if (GameState.Get().GetResourceAmount(resourceType) >= package.Amount)
                {
                    waitingForServer = true;
                    PackageHelpRequestManager.Instance.FullfillPackageHelpRequest(SaveLoadController.SaveState.ID, package.PackageID.ToString(), () =>
                    {
                        GameState.Get().RemoveResources(ResourceType.PositiveEnergy, package.Amount, EconomySource.Epidemy);

                        int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                        GameState.Get().AddResource(ResourceType.Coin, package.coinsReward, EconomySource.Epidemy, false);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(coinsPartialRewardImage.transform.position), package.coinsReward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                        {
                            GameState.Get().UpdateCounter(ResourceType.Coin, package.coinsReward, currentCoinAmount);
                        });
                        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                        GameState.Get().AddResource(ResourceType.Exp, package.expReward, EconomySource.Epidemy, false);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(expPartialRewardImage.transform.position), package.expReward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                        {
                            GameState.Get().UpdateCounter(ResourceType.Exp, package.expReward, currentExpAmount);
                        });

                        package.IsPackageFinished = true;
                        packagesWithHelp--;

                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.HelpWithAntiEpidemicCentres));

                        if (packagesWithHelp <= 0)
                            HospitalAreasMapController.HospitalMap.epidemy.HelpMark.SetActive(false);
                        if (VisitingController.Instance.IsVisiting)                        
                            CacheManager.UpdateEpidemyHelpRequest(SaveLoadController.SaveState.ID, packagesWithHelp > 0);

                        SetSpecificClosedPackageLook(boxes[package.PackageID], CognitoEntry.SaveID);
                        HideSelectedInfo();
                        RefreshMedicinesStatus();
                        waitingForServer = false;

                        AnalyticsController.instance.ReportSocialHelp(SocialHelpAction.GiveHelpEpidemy, SaveLoadController.SaveState.ID);
                        GameState.Get().IncrementHelpsCounter();
                    });
                    DeactivateButton(putMedicineIntoBoxButton);
                    Deactivate9sliceButton(helpButton);
                }
                else
                {
                    UIController.get.BuyResourcesPopUp.Open(package.Amount - GameState.Get().PositiveEnergyAmount, () =>
                    {
                        HelpWithPackage(package);
                    }, null, BuyResourcesPopUp.missingResourceType.positive);
                }
            }
            else
            {
                if (GameState.Get().GetCureCount(medicine) >= package.Amount)
                {
                    waitingForServer = true;

                    PackageHelpRequestManager.Instance.FullfillPackageHelpRequest(SaveLoadController.SaveState.ID, package.PackageID.ToString(), () =>
                    {
                        GameState.Get().GetCure(medicine, package.Amount, EconomySource.EpidemyHelp);
                        int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                        GameState.Get().AddResource(ResourceType.Coin, package.coinsReward, EconomySource.Epidemy, false);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(coinsPartialRewardImage.transform.position), package.coinsReward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                        {
                            GameState.Get().UpdateCounter(ResourceType.Coin, package.coinsReward, currentCoinAmount);
                        });
                        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                        GameState.Get().AddResource(ResourceType.Exp, package.expReward, EconomySource.Epidemy, false);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(expPartialRewardImage.transform.position), package.expReward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                        {
                            GameState.Get().UpdateCounter(ResourceType.Exp, package.expReward, currentExpAmount);
                        });
                        package.IsPackageFinished = true;
                        packagesWithHelp--;

                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.HelpWithAntiEpidemicCentres));

                        if (packagesWithHelp <= 0)
                            HospitalAreasMapController.HospitalMap.epidemy.HelpMark.SetActive(false);
                        if (VisitingController.Instance.IsVisiting)                        
                            CacheManager.UpdateEpidemyHelpRequest(SaveLoadController.SaveState.ID, packagesWithHelp > 0);

                        SetSpecificClosedPackageLook(boxes[package.PackageID], CognitoEntry.SaveID);
                        HideSelectedInfo();
                        RefreshMedicinesStatus();
                        waitingForServer = false;

                        AnalyticsController.instance.ReportSocialHelp(SocialHelpAction.GiveHelpEpidemy, SaveLoadController.SaveState.ID);
                        GameState.Get().IncrementHelpsCounter();
                    });
                    DeactivateButton(putMedicineIntoBoxButton);
                    Deactivate9sliceButton(helpButton);
                }
                else
                {
                    List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines = new List<KeyValuePair<int, MedicineDatabaseEntry>>();
                    missingMedicines.Add(new KeyValuePair<int, MedicineDatabaseEntry>(package.Amount - GameState.Get().GetCureCount(availableMedicines[package.MedicineIndex].GetMedicineRef()), availableMedicines[package.MedicineIndex]));
                    UIController.get.BuyResourcesPopUp.Open(missingMedicines, false, false, false, () =>
                    {
                        HelpWithPackage(package);
                    }, null, null);
                }
            }
        }

        if (packagesWithHelp <= 0)
            HospitalAreasMapController.HospitalMap.epidemy.HelpMark.SetActive(false);
    }

    //tries to put medicine into package and closes it if requirements are met
    private void PutMedicineIntoBox(Package package)
    {
        MedicineRef medicine = availableMedicines[package.MedicineIndex].GetMedicineRef();
        if (medicine.type == MedicineType.Fake)
        {
            ResourceType resourceType = ResourceType.PositiveEnergy;

            if (ResourcesHolder.Get().medicines.cures[(int)medicine.type].medicines[medicine.id].Name == "SPECIAL_ITEMS/POSITIVE_ENERGY")
            {
                resourceType = ResourceType.PositiveEnergy;
            }
            else
            {
                Debug.LogError("Check cure Name, don't call Marian");
                return;
            }

            if (GameState.Get().GetResourceAmount(resourceType) >= package.Amount)
            {
                package.HelpedByWhom = "";

                GameState.Get().RemoveResources(ResourceType.PositiveEnergy, package.Amount, EconomySource.Epidemy);

                boxes[package.PackageID].GetComponent<EpidemyBox>().SetClosedBoxWithoutHelpLook();

                int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                GameState.Get().AddResource(ResourceType.Coin, package.coinsReward, EconomySource.Epidemy, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(coinsPartialRewardImage.transform.position), package.coinsReward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                {
                    GameState.Get().UpdateCounter(ResourceType.Coin, package.coinsReward, currentCoinAmount);
                });
                int currentExpReward = Game.Instance.gameState().GetExperienceAmount();
                GameState.Get().AddResource(ResourceType.Exp, package.expReward, EconomySource.Epidemy, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(expPartialRewardImage.transform.position), package.expReward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                {
                    GameState.Get().UpdateCounter(ResourceType.Exp, package.expReward,currentExpReward);
                });

                package.IsPackageFinished = true;
                packagesFinishedStatus[package.PackageID] = true;

                AnalyticsController.instance.ReportEpidemyPackageComplete(false, package, medicine);
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CompleteAntiEpidemicBoxes));

                if (package.IsHelpRequested)
                    PackageHelpRequestManager.Instance.DeletePackageHelpRequest(package.PackageID.ToString());

                //GetUnfinishedPackageInfo();

                SetEndButton();
                RefreshMedicinesStatus();
                HideSelectedInfo();

                DeactivateButton(putMedicineIntoBoxButton);
            }
            else
            {
                UIController.get.BuyResourcesPopUp.Open(package.Amount - GameState.Get().PositiveEnergyAmount, () =>
                {
                    PutMedicineIntoBox(package);
                }, null, BuyResourcesPopUp.missingResourceType.positive);
            }
        }
        else
        {
            if (GameState.Get().GetCureCount(medicine) >= package.Amount)
            {
                package.HelpedByWhom = "";

                GameState.Get().GetCure(medicine, package.Amount, EconomySource.Epidemy);
                boxes[package.PackageID].GetComponent<EpidemyBox>().SetClosedBoxWithoutHelpLook();

                int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                GameState.Get().AddResource(ResourceType.Coin, package.coinsReward, EconomySource.Epidemy, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(coinsPartialRewardImage.transform.position), package.coinsReward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                {
                    GameState.Get().UpdateCounter(ResourceType.Coin, package.coinsReward, currentCoinAmount);
                });
                int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                GameState.Get().AddResource(ResourceType.Exp, package.expReward, EconomySource.Epidemy, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(expPartialRewardImage.transform.position), package.expReward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                {
                    GameState.Get().UpdateCounter(ResourceType.Exp, package.expReward, currentExpAmount);
                });

                package.IsPackageFinished = true;
                packagesFinishedStatus[package.PackageID] = true;

                AnalyticsController.instance.ReportEpidemyPackageComplete(false, package, medicine);
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CompleteAntiEpidemicBoxes));

                if (package.IsHelpRequested)
                    PackageHelpRequestManager.Instance.DeletePackageHelpRequest(package.PackageID.ToString());

                //GetUnfinishedPackageInfo();

                SetEndButton();
                RefreshMedicinesStatus();
                HideSelectedInfo();

                DeactivateButton(putMedicineIntoBoxButton);
            }
            else
            {
                List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines = new List<KeyValuePair<int, MedicineDatabaseEntry>>();
                missingMedicines.Add(new KeyValuePair<int, MedicineDatabaseEntry>(package.Amount - GameState.Get().GetCureCount(medicine), availableMedicines[package.MedicineIndex]));
                UIController.get.BuyResourcesPopUp.Open(missingMedicines, false, false, false, () =>
                {
                    PutMedicineIntoBox(package);
                }, null, null);
            }
        }
    }

    private void FinishPackageWithHelp(Package package, string byWhom)
    {
        //GameState.Get().AddCoins(package.coinsReward, EconomySource.Epidemy);
        //GameState.Get().AddExperience(package.coinsReward, EconomySource.Epidemy, true);
        try
        {
            LastHelpersProvider.Instance.AddLastHelper(byWhom);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        package.IsPackageFinished = true;
        package.HelpedByWhom = byWhom;
        PackageHelpRequestManager.Instance.DeletePackageHelpRequest(package.PackageID.ToString());
        packagesFinishedStatus[package.PackageID] = true;

        SetSpecificClosedPackageLook(boxes[package.PackageID], byWhom);
        SetEndButton();

        AnalyticsController.instance.ReportEpidemyPackageComplete(true, package, null);
        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CompleteAntiEpidemicBoxes));

        SaveSynchronizer.Instance.InstantSave();
    }

    private void SetSpecificClosedPackageLook(EpidemyBox box, string helpedByWhom)
    {
        //dodac pobranie danych z metody o argumencie przyjmujacym helpedbywhom i przypisac do zmiennych
        EpidemyBox epidemyBox = box;
        epidemyBox.SetHelperInfoInactive();

        CacheManager.GetPublicSaveById(helpedByWhom, (save) =>
        {
            if (save != null && box != null)
            {
                epidemyBox.SetClosedBoxWithHelpLook(defaultAvatar, save.Level.ToString(), save.Name, Packages[boxes.IndexOf(epidemyBox)].helperConfirmed,
                    () =>
                    {
                        Package package = Packages[boxes.IndexOf(epidemyBox)];
                        package.helperConfirmed = true;
                    });

                if (AccountManager.Instance.IsFacebookConnected && !string.IsNullOrEmpty(save.FacebookID))
                {
                    CacheManager.GetUserDataByFacebookID(save.FacebookID, (login, avatar) =>
                    {
                        if (box != null)
                        {
                            epidemyBox.SetClosedBoxWithHelpLook(avatar, save.Level.ToString(), login, Packages[boxes.IndexOf(epidemyBox)].helperConfirmed,
                            () =>
                            {
                                Package package = Packages[boxes.IndexOf(epidemyBox)];
                                package.helperConfirmed = true;
                            });
                        }
                    }, (ex) =>
                    {
                        Debug.LogError(ex.Message);
                    });
                }
            }
        }, (ex) =>
        {
            Debug.LogError(ex.Message);
        });
    }

    public void SetAllClosedBoxesLook()
    {
        for (int i = 0; i < Packages.Count; i++)
        {
            if (!string.IsNullOrEmpty(Packages[i].HelpedByWhom))
            {
                SetSpecificClosedPackageLook(boxes[Packages[i].PackageID], Packages[i].HelpedByWhom);
            }
            else
            {
                for (int j = 0; j < boxes[Packages[i].PackageID].transform.GetChild(1).childCount; j++)
                {
                    boxes[Packages[i].PackageID].SetHelperInfoInactive();
                }
            }
        }
    }

    private void SetEndButton()
    {
        if (GetFinishedPackagesCounter() == CalculatePackagesAmount())
        {
            ActivateButton(endEpidemyButton, () => EndEventWithAllPackages(true));
            endEpidemyButton.GetComponent<Animator>().runtimeAnimatorController = endEpidemyAnimBlinking;
            rewardAnimations = true;
            SetRewardAnimations();
            if (epidemyCharacter != null)
            {
                epidemyCharacter.SetBool(epidemyCharacterAnimations[EpidemyCharacterAnimation.Completed], true);
            }
        }
        else
        {
            DeactivateButton(endEpidemyButton);
            endEpidemyButton.GetComponent<Animator>().runtimeAnimatorController = endEpidemyAnimDefault;
        }
    }

    public void SetRewardAnimations()
    {
        //rewardAnimator.SetTrigger("Reward");	
        if (rewardAnimations)
        {
            heliAnimator.SetTrigger("Reward");
        }
    }

    private void ActivateButton(Button button, UnityEngine.Events.UnityAction action)
    {
        UIController.SetButtonClickSoundInactiveSecure(button.gameObject, false);

        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            UIController.PlayClickSoundSecure(button.gameObject);
        });
        button.onClick.AddListener(action);

        foreach (Image image in button.GetComponentsInChildren<Image>())
        {
            image.material = null;
        }
    }

    private void ActivateYellow9SliceButton(Button button, UnityEngine.Events.UnityAction action)
    {
        UIController.SetImageSpriteSecure(button.image, ResourcesHolder.Get().yellow9SliceButton);
        ActivateButton(button, action);
    }

    private void ActivateOvalButton(bool isYellow, Button button, UnityEngine.Events.UnityAction action)
    {
        if (isYellow)
        {
            UIController.SetImageSpriteSecure(button.image, ResourcesHolder.Get().yellowOvalButton);
        }
        else
        {
            UIController.SetImageSpriteSecure(button.image, ResourcesHolder.Get().blueOvalButton);
        }
        ActivateButton(button, action);
    }

    private void Deactivate9sliceButton(Button button)
    {
        UIController.SetImageSpriteSecure(button.image, ResourcesHolder.Get().blue9SliceButton);
        DeactivateButton(button);
    }

    private void DeactivateOvalButton(Button button)
    {
        UIController.SetImageSpriteSecure(button.image, ResourcesHolder.Get().blueOvalButton);
        DeactivateButton(button);
    }

    private void DeactivateButton(Button button)
    {
        UIController.SetButtonClickSoundInactiveSecure(button.gameObject, true);

        foreach (Image image in button.GetComponentsInChildren<Image>())
        {
            image.material = ResourcesHolder.Get().GrayscaleMaterial;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            UIController.PlayClickSoundSecure(button.gameObject);
        });
        //button.interactable = false;
    }

    private void Update()
    {
        //Debug.Log("ReferenceHolder.Get().Epidemy.TimeTillEpidemyEnd: " + ReferenceHolder.Get().Epidemy.TimeTillEpidemyEnd);
        timer.text = UIController.GetFormattedTime((int)ReferenceHolder.GetHospital().Epidemy.TimeTillEpidemyEnd);
    }

    private void EndEventWithAllPackages(bool useParticle)
    {
        for (int i = 0; i < packagesFinishedStatus.Length; i++)
            packagesFinishedStatus[i] = false;

        HospitalAreasMapController.HospitalMap.epidemy.ForceEpidemyEnd();
        HospitalAreasMapController.HospitalMap.epidemy.HelpMark.SetActive(false);

        if (!VisitingController.Instance.IsVisiting)
        {   //just in case
            GivePrizeBox();
        }
        /*
        int reward = CalculateFinalCoinsReward();

        if (useParticle)
        {
            //Hospital.HospitalAreasMapController.Map.casesManager.AddCase(0); do dodania dodawanie paczki z nagrodą   
            GameState.Get().AddResource(ResourceType.Coin, reward, EconomySource.Epidemy, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(finalRewardInCoinsImage.transform.position), reward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () => {
                GameState.Get().UpdateCounter(ResourceType.Coin, reward);
            });
            HospitalAreasMapController.Map.epidemy.ForceEpidemyEnd();
        }
        else
        {
            GameState.Get().AddResource(ResourceType.Coin, reward, EconomySource.Epidemy, true);
        }
        */
    }

    private void GivePrizeBox()
    {
        ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromEpidemy = true;
        UIController.getHospital.unboxingPopUp.OpenEpidemyCasePopup();
    }

    private void AccountManager_OnFacebookStateUpdate()
    {
        Debug.LogError("OnFacebookStateUpdate");
        SetAllClosedBoxesLook();
    }

    public List<string> SavePackagesToStringList()
    {
        List<string> packagesSaveStringList = new List<string>();

        packagesSaveStringList.Add(helpsCounter.ToString());
        for (int i = 0; i < Packages.Count; i++)
            packagesSaveStringList.Add(Packages[i].MedicineIndex + "!" + Packages[i].PackageID + "!" + Packages[i].Amount + "!" + Packages[i].coinsReward + "!" + Packages[i].expReward + "!" + Packages[i].IsHelpRequested.ToString() + "!" + Packages[i].IsPackageFinished.ToString() + "!" + Packages[i].HelpedByWhom + "!" + Packages[i].helperConfirmed);

        return packagesSaveStringList;
    }

    public void LoadPackaesFromString(List<string> saveData)
    {
        bool hasAnyHelpRequest = false;
        string[] records;
        Packages.Clear();
        availableMedicines = ResourcesHolder.Get().GetMedicines();
        bool hasConfirmedHelper = false;
        // availableMedicines = ResourcesHolder.Get().EnumerateKnownMedicinesForLvl(HospitalAreasMapController.Map.epidemy.LevelWhileGeneratingMedicines);

        for (int i = 0; i < packagesFinishedStatus.Length; i++)
            packagesFinishedStatus[i] = false;

        if (saveData.Count > 3)
        {
            helpsCounter = Convert.ToInt32(saveData[6]);
            saveData.RemoveRange(0, 7);
            for (int i = 0; i < saveData.Count; i++)
            {
                records = saveData[i].Split('!');
                if (!HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    if (!hasAnyHelpRequest && IsHelpRequestedOnPackage(Convert.ToBoolean(records[5]), Convert.ToBoolean(records[6])))
                    {
                        hasAnyHelpRequest = true;
                    }
                    if (records.Length > 8) // this check is here so there was no need to create UpdateVersion class for that string only. It makes only sense during update to version whevere "tap on epidemy avatar" task was introduced
                    {
                        hasConfirmedHelper = Convert.ToBoolean(records[8]);
                    }
                }
                CreatePackage(Convert.ToInt32(records[1]), Convert.ToInt32(records[0]), Convert.ToInt32(records[2]), Convert.ToInt32(records[3]), Convert.ToInt32(records[4]), Convert.ToBoolean(records[5]), Convert.ToBoolean(records[6]), records[7], hasConfirmedHelper);
            }
        }

        DisableUnusedBoxes(Packages.Count);
        Epidemy.HasAnyHelpRequest = hasAnyHelpRequest;

        SetAllClosedBoxesLook();

        if (!HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            PackageHelpRequestManager.Instance.GetMyPackageRequests(OnPersonalHelpGet);
        }
        else
        {
            PackageHelpRequestManager.Instance.GetUserPackageRequests(SaveLoadController.SaveState.ID, OnForeignHelpGet);
        }

        SetEndButton();
    }

    public void CheckPackagesFromLastEpidemy(List<string> saveData)
    {
        if (saveData.Count > 7)
        {
            string[] records;
            int finishedPackages = 0;

            for (int i = 7; i < saveData.Count; i++)
            {
                records = saveData[i].Split('!');
                bool isFinished = Convert.ToBoolean(records[6]);

                if (isFinished)
                {
                    finishedPackages++;
                }
                else
                {
                    return;
                }
            }

            if (finishedPackages == saveData.Count)
            {
                EndEventWithAllPackages(false);
            }
        }
    }

    private List<CacheManager.IGetPublicSave> GetSaveIDsToGetPublicData(List<PackageHelpRequest> requests)
    {
        List<CacheManager.IGetPublicSave> list = new List<CacheManager.IGetPublicSave>();
        foreach (Package package in Packages)
        {
            if (package.IsPackageFinished && !string.IsNullOrEmpty(package.HelpedByWhom))
            {
                list.Add(package);
            }
        }

        foreach (var request in requests)
        {
            if (request.helped && !string.IsNullOrEmpty(request.ByWhom))
            {
                list.Add(request);
            }
        }
        return list;
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        AccountManager.OnFacebookStateUpdate -= AccountManager_OnFacebookStateUpdate;
        base.Exit(hidePopupWithShowMainUI);
    }

    public void ButtonExit()
    {
        Exit();
    }

    public Animator InfoHoverAnim;
    public void InfoButtonDown()
    {
        InfoHoverAnim.gameObject.SetActive(true);
        InfoHoverAnim.SetBool("Show", true);
        SoundsController.Instance.PlayInfoButton();
        AnalyticsController.instance.ReportButtonClick("epidemy_on", "info");
    }

    public void InfoButtonUp()
    {
        InfoHoverAnim.SetBool("Show", false);
    }
}
