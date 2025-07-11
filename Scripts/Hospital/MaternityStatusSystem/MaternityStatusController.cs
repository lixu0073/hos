using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityStatusController : MonoBehaviour
{
    public const int AMOUNT_OF_VITAMINES_IN_GAME = 4;
    private MaternityStatusController Instance;
    private MaternityStatusTimerController timerController;
    private int[] vitaminAmount;
    private Dictionary<Maternity.PatientStates.MaternityPatientStateTag, int> motherStateAmountMap;
    private List<GenderTypes> babiesGendersInWCFRPhase;
    private Coroutine TimerCoroutine = null;
    private float refreshTime = 2.0f;
    private const string WAITING_ROOM_STRING_NAME = "WaitingRoom";
    private long currentIDTimeLeft = -1;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        timerController = new MaternityStatusTimerController();
        vitaminAmount = new int[AMOUNT_OF_VITAMINES_IN_GAME]
        {
            0,0,0,0
        };
        motherStateAmountMap = new Dictionary<Maternity.PatientStates.MaternityPatientStateTag, int>();
        babiesGendersInWCFRPhase = new List<GenderTypes>();
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public MaternityStatusController Get()
    {
        return Instance;
    }

    public void OnOpenBUttonClick()
    {
        UIController.getHospital.MaternityStatusPopup.InitializeData(PreparePopupData());
        StartCoroutine(UIController.getHospital.MaternityStatusPopup.Open(true, true));
    }

    public void OnExitButtonClicked()
    {
        UIController.getHospital.MaternityStatusPopup.ButtonExit();
    }

    public void LoadFromString(Save saveData)
    {
        if (motherStateAmountMap != null)
            motherStateAmountMap.Clear();
        else
            motherStateAmountMap = new Dictionary<Maternity.PatientStates.MaternityPatientStateTag, int>();

        List<string> maternityPatientData = saveData.MaternityPatients;
        if (maternityPatientData != null && maternityPatientData.Count > 0)
        {
            int timePassed = (int)((long)ServerTime.getTime() - saveData.MaternitySaveDateTime);
            for (int i = 0; i < maternityPatientData.Count; i++)
            {
                string patientMainInfo = String.Empty;
                string relativeInfo = String.Empty;

                string[] basePatientInfo = maternityPatientData[i].Split('^');
                if (basePatientInfo.Length > 0)
                    patientMainInfo = basePatientInfo[0];

                if (basePatientInfo.Length > 2)                
                    relativeInfo = basePatientInfo[2];

                if (!String.IsNullOrEmpty(patientMainInfo))
                {
                    MaternityPatientParsedGeneralData data = Maternity.MaternityPatientAI.ParsePatientGeneralData(patientMainInfo);
                    switch (data.stateTag)
                    {
                        case Maternity.PatientStates.MaternityPatientStateTag.NULL:
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.GTWR:
                            AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.WFSTD);
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.WFSTD:
                            AddMotherToDictionary(data.stateTag);
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.GO:
                            List<string> maternityClinicObjectData = saveData.MaternityClinicObjectsData;
                            Maternity.PatientStates.MaternityPatientGoingOutStateParsedData GO_data = Maternity.PatientStates.MaternityPatientGoingOutState.Parse(patientMainInfo);
                            CheckTimeForTimer(data.stateTag, timePassed, GO_data.timeLeft, () =>
                            {
                                AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.WFSTD);
                                UpdatePopupIfActive();
                            });
                            if (maternityClinicObjectData != null && maternityClinicObjectData.Count > 0)
                            {
                                for (int j = 0; j < maternityClinicObjectData.Count; j++)
                                {
                                    if (maternityClinicObjectData[j].Split('$')[0].Contains(WAITING_ROOM_STRING_NAME))
                                    {
                                        string[] waitingRoomData = maternityClinicObjectData[j].Split(';');
                                        string unparsedTag = waitingRoomData[2].Split('/')[0];
                                        MaternityWaitingRoomBed.State bedState = (MaternityWaitingRoomBed.State)Enum.Parse(typeof(MaternityWaitingRoomBed.State), unparsedTag);
                                        if (bedState == MaternityWaitingRoomBed.State.WFP)
                                        {
                                            float timeLeft = float.Parse(waitingRoomData[1], System.Globalization.CultureInfo.InvariantCulture);
                                            CheckTimeForTimer(data.stateTag, timePassed, timeLeft, () =>
                                            {
                                                AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.WFSTD);
                                                UpdatePopupIfActive();
                                            });
                                        }
                                    }
                                }
                            }
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.WFC:
                            string[] medicineInfo = patientMainInfo.Split('!')[3].Split('@')[4].Split('*');
                            foreach (string requiredUnparsedCure in medicineInfo)
                            {
                                if (string.IsNullOrEmpty(requiredUnparsedCure))
                                    continue;
                                string[] requiredUnparsedCureArray = requiredUnparsedCure.Split('#');
                                if (requiredUnparsedCureArray.Length >= 2)
                                {
                                    vitaminAmount[MedicineRef.Parse(requiredUnparsedCureArray[0]).id] = int.Parse(requiredUnparsedCureArray[1], System.Globalization.CultureInfo.InvariantCulture);
                                }
                            }
                            AddMotherToDictionary(data.stateTag);
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.WFL:
                            Maternity.PatientStates.MaternityPatientWaitingForLabourParsedData WFL_Data = Maternity.PatientStates.MaternityPatientWaitingForLaborState.Parse(patientMainInfo);
                            CheckTimeForTimer(data.stateTag, timePassed, WFL_Data.timeLeft, () =>
                            {
                                AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.RFL);
                                UpdatePopupIfActive();
                            });
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.RFL:
                            AddMotherToDictionary(data.stateTag);
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.IDQ:
                            Maternity.PatientStates.MaternityPatientInDiagnoseQueueStateData IDQ_data = Maternity.PatientStates.MaternityPatientInDiagnoseQueueState.Parse(patientMainInfo);
                            AddMotherToDictionary(data.stateTag);
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.ID:
                            Maternity.PatientStates.MaternityPatientInDiagnoseStateData ID_Data = Maternity.PatientStates.MaternityPatientInDiagnoseState.Parse(patientMainInfo);
                            currentIDTimeLeft = (long)ID_Data.timeLeft;
                            CheckTimeForTimer(data.stateTag, timePassed, ID_Data.timeLeft, () =>
                            {
                                AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.WFDR);
                                UpdatePopupIfActive();
                            });
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.WFDR:
                            AddMotherToDictionary(data.stateTag);
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.IL:
                            Maternity.PatientStates.MaternityPatientInLabourStateParsedData IL_Data = Maternity.PatientStates.MaternityPatientInLaborState.Parse(patientMainInfo);
                            CheckTimeForTimer(data.stateTag, timePassed, IL_Data.timeLeft, () =>
                            {
                                AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.LF);
                                UpdatePopupIfActive();
                            });
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.LF:
                            AddMotherToDictionary(data.stateTag);
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.RTWR:
                            Maternity.PatientStates.MaternityPatientReturnToWaitingRoomStateParsedData RTWR_Data = Maternity.PatientStates.MaternityPatientReturnToWaitingRoomState.Parse(patientMainInfo);
                            CheckTimeForTimer(data.stateTag, timePassed, RTWR_Data.timeLeft, () =>
                            {
                                AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.WFCR);
                                UpdatePopupIfActive();
                            });
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.B:
                            Maternity.PatientStates.MaternityPatientBondingStateParsedData B_Data = Maternity.PatientStates.MaternityPatientBondingState.Parse(patientMainInfo);
                            if (!String.IsNullOrEmpty(relativeInfo))
                            {
                                int genderIndex = int.Parse(relativeInfo.Split('|')[1].Split('*')[1], System.Globalization.CultureInfo.InvariantCulture);
                                CheckTimeForTimer(data.stateTag, timePassed, B_Data.timeLeft, () =>
                                {
                                    AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.WFCR);
                                    UpdatePopupIfActive();
                                    babiesGendersInWCFRPhase.Add((GenderTypes)genderIndex);
                                });
                            }
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.GTLR:
                            Maternity.PatientStates.MaternityPatientGoToLaborStateParsedData GTLR_Data = Maternity.PatientStates.MaternityPatientGoToLaborRoomState.Parse(patientMainInfo);
                            CheckTimeForTimer(data.stateTag, timePassed, GTLR_Data.timeLeft, () =>
                            {
                                AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.LF);
                                UpdatePopupIfActive();
                            });
                            break;
                        case Maternity.PatientStates.MaternityPatientStateTag.WFCR:
                            AddMotherToDictionary(data.stateTag);
                            if (!String.IsNullOrEmpty(relativeInfo))
                            {
                                int genderIndex = int.Parse(relativeInfo.Split('|')[1].Split('*')[1], System.Globalization.CultureInfo.InvariantCulture);
                                babiesGendersInWCFRPhase.Add((GenderTypes)genderIndex);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            SetupTimersForIDQPatients(saveData.MaternitySaveDateTime);
            TimerCoroutine = StartCoroutine(UpdateTimers());
        }
    }

    private void SetupTimersForIDQPatients(long maternitySaveTime)
    {
        if (currentIDTimeLeft != -1 && TryToGetMotherAmountForStatus(Maternity.PatientStates.MaternityPatientStateTag.IDQ) > 0)
        {
            long timeWhenDiagnosisEnded = maternitySaveTime + currentIDTimeLeft;
            long timeToEmulate = (long)ServerTime.getTime() - timeWhenDiagnosisEnded;
            for (int i = 0; i < TryToGetMotherAmountForStatus(Maternity.PatientStates.MaternityPatientStateTag.IDQ); i++)
            {
                timeToEmulate = timeToEmulate - Maternity.MaternityCoreLoopParametersHolder.BloodTestTime; //BundleManager.GetMaternityBloodTestTime();
                if (timeToEmulate < 0)
                {
                    CheckTimeForTimer(Maternity.PatientStates.MaternityPatientStateTag.IDQ, 0, Mathf.Abs(timeToEmulate), () =>
                    {
                        AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.WFDR);
                        UpdatePopupIfActive();
                    });
                }
                else
                {
                    AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag.WFDR);
                    UpdatePopupIfActive();
                    continue;
                }
            }
            motherStateAmountMap[Maternity.PatientStates.MaternityPatientStateTag.IDQ] = 0;
        }
    }

    private void CheckTimeForTimer(Maternity.PatientStates.MaternityPatientStateTag stateTag, float timePassed, float timeLeftToFinishCure, Action onTimerEnd)
    {
        float timeForTimer = Mathf.Clamp(timeLeftToFinishCure - timePassed, 0, float.MaxValue);
        if (timeForTimer == 0)
        {
            onTimerEnd?.Invoke();
        }
        else
        {
            timerController.RequestTimerForState(stateTag, timeForTimer, refreshTime, onTimerEnd);
        }
    }

    private void AddMotherToDictionary(Maternity.PatientStates.MaternityPatientStateTag motherState)
    {
        if (motherStateAmountMap.ContainsKey(motherState))
            motherStateAmountMap[motherState] += 1;
        else
            motherStateAmountMap.Add(motherState, 1);
    }

    private void RemoveMotherFromDictionary(Maternity.PatientStates.MaternityPatientStateTag motherState)
    {
        if (!motherStateAmountMap.ContainsKey(motherState))
            return;

        motherStateAmountMap[motherState] -= 1;
        if (motherStateAmountMap[motherState] < 0)
            motherStateAmountMap[motherState] = 0;
    }

    private IEnumerator UpdateTimers()
    {
        while (timerController.Update())
        {
            yield return new WaitForSeconds(refreshTime);
        }
        try
        { 
            if (TimerCoroutine != null)        
                StopCoroutine(TimerCoroutine);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        TimerCoroutine = null;
    }

    private void UpdatePopupIfActive()
    {
        if (UIController.getHospital.MaternityStatusPopup.isActiveAndEnabled)        
            UIController.getHospital.MaternityStatusPopup.InitializeData(PreparePopupData());
    }

    private MaternityStatusData PreparePopupData()
    {
        MaternityStatusData data = new MaternityStatusData()
        {
            vitData = new MaternityStatusVitaminPanelData(),
            motherData = new MaternityStatusMotherPanelData(),
            goTomaternityButtonAction = new UnityEngine.Events.UnityAction(delegate { HospitalAreasMapController.HospitalMap.maternityWard.OnClickFromButton(); }),
        };
        GetVitaminRequirementsInfo(data);
        GetMotherStatusInfo(data);
        FindWhatPresentColor(data);
        return data;
    }

    private void GetVitaminRequirementsInfo(MaternityStatusData data)
    {
        for (int i = 0; i < MaternityStatusController.AMOUNT_OF_VITAMINES_IN_GAME; i++)
        {
            MedicineRef vitamin = new MedicineRef(MedicineType.Vitamins, i);

            data.vitData.vitaminAmountRequired[i] = vitaminAmount[i];
            if (vitaminAmount[i] > 0)            
                data.mainPopupStrategy = new MaternityStatusPopupNotEmptyStrategy();

            data.vitData.diamondCost[i] = ResourcesHolder.Get().GetMedicineInfos(new MedicineRef(MedicineType.Vitamins, i)).diamondPrice * Mathf.Clamp(vitaminAmount[i] - Game.Instance.gameState().GetCureCount(vitamin), 0, int.MaxValue);

            int diamondPrice = data.vitData.diamondCost[i];
            int amountToBuy = data.vitData.vitaminAmountRequired[i];
            int currentDiamondAMount = Game.Instance.gameState().GetDiamondAmount();
            bool isTankStorageitem = vitamin.IsMedicineForTankElixir();
            data.vitData.buyForDiamondsButtonActions[i] = new UnityEngine.Events.UnityAction(delegate
            {
                if (currentDiamondAMount >= diamondPrice)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(diamondPrice, delegate
                    {
                        UIController.get.storageCounter.Add(isTankStorageitem);
                        Game.Instance.gameState().RemoveDiamonds(diamondPrice, EconomySource.MissingResourcesStorage);
                        Game.Instance.gameState().AddResource(vitamin, amountToBuy, true, EconomySource.MissingResourcesBuy);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(SimpleUI.GiftType.Medicine, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), amountToBuy, .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(vitamin), null, () =>
                        {
                            UIController.get.storageCounter.Remove(amountToBuy, isTankStorageitem);
                        });
                        UpdatePopupIfActive();
                    }, UIController.getHospital.MaternityStatusPopup);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            });
        }
    }

    private void GetMotherStatusInfo(MaternityStatusData data)
    {
        for (int i = 0; i < Enum.GetValues(typeof(Maternity.PatientStates.MaternityPatientStateTag)).Length; i++)
        {
            Maternity.PatientStates.MaternityPatientStateTag tag = (Maternity.PatientStates.MaternityPatientStateTag)i;
            data.motherData.SetAmountOfMothersForStatus(tag, TryToGetMotherAmountForStatus(tag));
            if (TryToGetMotherAmountForStatus(tag) > 0)            
                data.mainPopupStrategy = new MaternityStatusPopupNotEmptyStrategy();
        }
    }

    private int TryToGetMotherAmountForStatus(Maternity.PatientStates.MaternityPatientStateTag status)
    {
        motherStateAmountMap.TryGetValue(status, out int valueToReturn);
        return valueToReturn;
    }

    private void FindWhatPresentColor(MaternityStatusData data)
    {
        if (babiesGendersInWCFRPhase.Count > 0)
        {
            int maleBabies = 0;
            int femaleBabies = 0;
            for (int i = 0; i < babiesGendersInWCFRPhase.Count; i++)
            {
                if (babiesGendersInWCFRPhase[i] == GenderTypes.Man)
                    ++maleBabies;
                else                
                    ++femaleBabies;
            }
            if (maleBabies != femaleBabies)            
                data.motherData.presentType = maleBabies > femaleBabies ? GenderTypes.Man : GenderTypes.Woman;
            else            
                data.motherData.presentType = (GenderTypes)BaseGameState.RandomNumber(0, 2);
        }
    }

#if UNITY_EDITOR
#pragma warning disable 0649
    [SerializeField] private List<long> maternitySaveTimes;
    [SerializeField] private int timeIndex;
#pragma warning restore 0649

    [ContextMenu("Load Test Data for maternity")]
    public void TestLadowaniaDanych()
    {
        Save save = new Save();
        save.MaternityPatients = new List<string>()
        {
            "1530884750846WaitingRoomBlueOrchid!(53,52)!-1!5508@10729@87476@1@Vitamins(0)#3*Vitamins(1)#6*Vitamins(2)#9*Vitamins(3)#12!WFC!WaitingRoomBlueOrchid^0*1*4*False*0*49*NAME_FEMALE_0_9*SURNAME_0_13*2*0*0*3*7*5^",
            "1530884800305WaitingRoomLavender!(53,44)!-1!4524@11072@90683@1@Vitamins(0)#510*Vitamins(1)#622*Vitamins(2)#568*Vitamins(3)#589!WFL!WaitingRoomLavender!4386.356^0*1*4*False*0*23*NAME_FEMALE_0_42*SURNAME_0_8*2*0*0*6*3*6^",
            "1530884816940WaitingRoomRose!(53,36)!-1!8770@9695@90488@1@!ID!WaitingRoomRose!3441.082^0*1*4*False*0*38*NAME_FEMALE_0_25*SURNAME_0_29*0*0*0*13*16*5^",
            "1530884836707WaitingRoomSunflower!(44,35)!-1!13873@5704@93285@1@!IDQ!WaitingRoomSunflower!1530884905862^2*1*4*False*0*45*NAME_FEMALE_2_17*SURNAME_2_9*1*0*0*10*4*2^",
            "1530884851596WaitingRoomTulip!(39,39)!-1!8725@11345@80129@0@!WFSTD!WaitingRoomTulip^1*1*4*False*0*24*NAME_FEMALE_1_25*SURNAME_1_3*0*0*0*8*8*5^",
            "1530884800305WaitingRoomLavender!(53,44)!-1!4524@11072@90683@1@Vitamins(0)#510*Vitamins(1)#622*Vitamins(2)#568*Vitamins(3)#589!RFL!WaitingRoomLavender^0*1*4*False*0*23*NAME_FEMALE_0_42*SURNAME_0_8*2*0*0*6*3*6^",
            "1530884836707WaitingRoomSunflower!(44,35)!-1!13873@5704@93285@1@!WFDR!WaitingRoomSunflower^2*1*4*False*0*45*NAME_FEMALE_2_17*SURNAME_2_9*1*0*0*10*4*2^",
            "1530884800305WaitingRoomLavender!(53,40)!-1!4524@11072@90683@1@Vitamins(0)#510*Vitamins(1)#622*Vitamins(2)#568*Vitamins(3)#589!IL!LabourRoomLavender!11044^0*1*4*False*0*23*NAME_FEMALE_0_42*SURNAME_0_8*2*0*0*6*3*6^",
            "1530884800305WaitingRoomLavender!(53,40)!-1!4524@11072@90683@1@Vitamins(0)#510*Vitamins(1)#622*Vitamins(2)#568*Vitamins(3)#589!LF!LabourRoomLavender!LabourRoomLavender^0*1*4*False*0*23*NAME_FEMALE_0_42*SURNAME_0_8*2*0*0*6*3*6^",
            "1530884750846WaitingRoomBlueOrchid!(53,52)!-1!5508@10729@87476@1@Vitamins(0)#581*Vitamins(1)#672*Vitamins(2)#511*Vitamins(3)#538!B!WaitingRoomBlueOrchid!87458.52^0*1*4*False*0*49*NAME_FEMALE_0_9*SURNAME_0_13*2*0*0*3*7*5^Baby/null|0*0*5*False*0*0*NAME_MALE_0_32**0*0*0*11*0*3",
            "1530884750846WaitingRoomBlueOrchid!(53,52)!-1!5508@10729@87476@1@Vitamins(0)#581*Vitamins(1)#672*Vitamins(2)#511*Vitamins(3)#538!WFCR!WaitingRoomBlueOrchid^0*1*4*False*0*49*NAME_FEMALE_0_9*SURNAME_0_13*2*0*0*3*7*5^Baby/null|0*0*5*False*0*0*NAME_MALE_0_32**0*0*0*11*0*3",
        };
        save.MaternitySaveDateTime = maternitySaveTimes[timeIndex];
        LoadFromString(save);
    }

    [ContextMenu("Test Maternity Status Popup")]
    public void TestOtwieraniaPopoup()
    {
        UIController.getHospital.MaternityStatusPopup.InitializeData(PreparePopupData());
        StartCoroutine(UIController.getHospital.MaternityStatusPopup.Open(true, true));
    }
#endif
}
