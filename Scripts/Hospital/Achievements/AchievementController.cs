using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleUI;
using UnityEngine.UI;
using UnityEngine;
using Hospital;

public class AchievementController
{
	// Lista przechowujaca AchievementInfo i statystyki achievementu
	public List<Achievement> AchievementList;

	[HideInInspector] public bool achievementChecked = true;
	// Metoda tworzy CZYSTĄ liste achievementów 
	public List<Achievement> initializeAchievementList(AchievementDatabase achievementDatabase)
	{
		AchievementList = new List<Achievement>();
		int ite = 0;
		foreach (AchievementInfo ai in achievementDatabase.AchievementItem)
		{
			Achievement achievement = new Achievement(ai.achievementID, ai, ite);
			if (!achievement.completed)
				achievement.RemoveListener ();
				achievement.AddListener();
			AchievementList.Add(achievement);
			ite++;
		}

		return AchievementList;

		// Hospital.AchievementPopUpController apc = new Hospital.AchievementPopUpController();
		// apc.initializePopUp(AchievementList);
	}

	// Metoda tworzy WCZYTANĄ liste achievementow
	public List<Achievement> initializeAchievementListFromSave(AchievementDatabase ad) // (string path)
	{
		AchievementTestDataStorage atds = new AchievementTestDataStorage();

		AchievementList = atds.TestLoadAchievementList();

		foreach (Achievement item in AchievementList)
		{
            if (!item.completed)
            {
                item.RemoveListener();
                item.AddListener();
            }

			foreach (AchievementInfo ai in ad.AchievementItem)
			{
				if (item.id.Equals(ai.achievementID))
				{
					item.setAchievementInfo(ai);
				}
			}
		}

		return AchievementList;
	}

	public List<string> SaveToStringList()
    {
		List<string> saveStringList = new List<string> ();
		StringBuilder builder = new StringBuilder();

		saveStringList.Add (achievementChecked.ToString());

		for (int i = 0; i < AchievementList.Count; i++)
        {
			builder.Append (AchievementList[i].id);
			builder.Append (";");
			builder.Append (Checkers.CheckedAmount(AchievementList[i].stage, 0, 3, AchievementList[i].id + " stage: ").ToString());
			builder.Append (";");
			builder.Append (AchievementList[i].completed.ToString());
			builder.Append (";");
			builder.Append (Checkers.CheckedAmount(AchievementList[i].progress, 0, AchievementList[i].achievementInfo.requiredValues[Mathf.Min(AchievementList[i].stage, 2)], AchievementList[i].id + " progress: ").ToString());
			builder.Append (";");
			for (int j = 0; j < AchievementList [i].time.Count; j++)
            {
				builder.Append (Checkers.CheckedAmount(AchievementList [i].time [j], 0, int.MaxValue, AchievementList [i].id + " time: ").ToString ());
				if (j < AchievementList [i].time.Count - 1)                
					builder.Append ("!");
			}
			builder.Append (';');
			if (AchievementList [i].arrivedTime != null)
            {
				for (int j = 0; j < AchievementList [i].arrivedTime.Count; j++)
                {
					builder.Append (AchievementList [i].arrivedTime.Keys.ElementAt(j).ToString());
					builder.Append ("?");
					builder.Append (AchievementList [i].arrivedTime.Values.ElementAt(j).ToString());
					if (j < AchievementList [i].arrivedTime.Count - 1)
						builder.Append ("!");
				}
			}
			builder.Append (';');
			builder.Append (AchievementList[i].Collected.ToString());
			builder.Append (';');
			builder.Append (Checkers.CheckedAmount(AchievementList[i].ToCollect, 0, 3, AchievementList[i].id + " ToCollect: ").ToString());
			saveStringList.Add (builder.ToString ());
			builder.Length = 0;
			builder.Capacity = 0;
		}
		return saveStringList;
	}

	public void LoadFromStringList(List<string> saveStringList){

        if (HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            UIController.getHospital.achievementIndicator.Activate(false);
            return;
        }

        try
        {
            if (saveStringList != null && saveStringList.Count > 0)
            {
                achievementChecked = bool.Parse(saveStringList[0]);
                if (achievementChecked)
                    UIController.getHospital.achievementIndicator.Activate(false);
                else                
                    UIController.getHospital.achievementIndicator.Activate(!HospitalAreasMapController.HospitalMap.VisitingMode);

                for (int i = 1; i < saveStringList.Count && i <= AchievementList.Count; i++) {
                    var save = saveStringList[i].Split(';');
                    AchievementList[i - 1].setID(save[0]);
                    AchievementList[i - 1].setStage(int.Parse(save[1], System.Globalization.CultureInfo.InvariantCulture));
                    AchievementList[i - 1].setCompleted(bool.Parse(save[2]));
                    AchievementList[i - 1].setProgress(int.Parse(save[3], System.Globalization.CultureInfo.InvariantCulture));

                    if (!string.IsNullOrEmpty(save[4]))
                    {
                        var saveTimes = save[4].Split('!');
                        AchievementList[i - 1].time.Clear();
                        for (int j = 0; j < saveTimes.Length; j++)
                        {
                            AchievementList[i - 1].time.Add(int.Parse(saveTimes[j], System.Globalization.CultureInfo.InvariantCulture));
                        }
                    }

                    if (AchievementList[i - 1].arrivedTime != null)
                    {
                        if (!string.IsNullOrEmpty(save[5]))
                        {
                            var savePairs = save[5].Split('!');
                            AchievementList[i - 1].arrivedTime.Clear();
                            for (int j = 0; j < savePairs.Length; j++)
                            {
                                var savePair = savePairs[j].Split('?');
                                AchievementList[i - 1].arrivedTime.Add(savePair[0], System.Convert.ToInt32(savePair[1]));
                            }
                        }
                    }

                    AchievementList[i - 1].Collected = bool.Parse(save[6]);
                    AchievementList[i - 1].ToCollect = int.Parse(save[7], System.Globalization.CultureInfo.InvariantCulture);
                    AchievementList[i - 1].ValidateProgress(); // if values are wrong or over max then do stuff in this method
                    UIController.getHospital.AchievementsPopUp.UpdateByID(save[0]);
                }
            } else {
                GenerateDefault();
            }
        }
        catch (FormatException)
        {
            BaseUIController.ShowCriticalProblemPopup(UIController.get, "Person has old save format/content for achievement");
            GenerateDefault();
            // throw new Exception("Person has old save format/content for achievement");
        }
    }

	public void GenerateDefault()
    {
		for (int i = 0; i < AchievementList.Count; i++)
        {
			AchievementList [i].setStage (0);
			AchievementList [i].setCompleted (false);
			AchievementList [i].setProgress (0);
			AchievementList [i].time.Clear ();
			AchievementList [i].Collected = true;
		}
	}

	public void TESTSAVE()
	{
		AchievementTestDataStorage atds = new AchievementTestDataStorage();
		atds.TestSaveAchievementList(AchievementList);
	}

	public void UpdateAchievements()
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)    
            return;

		AchievementNotificationCenter.Instance.ElixirCollected.InvokeUpdate();
		AchievementNotificationCenter.Instance.TreatmentCenterPatientCured.InvokeUpdate();
		AchievementNotificationCenter.Instance.BubblePopped.InvokeUpdate();
        AchievementNotificationCenter.Instance.CureProduced.InvokeUpdate();
        AchievementNotificationCenter.Instance.PatientInClinicCured.InvokeUpdate();
        AchievementNotificationCenter.Instance.CoinsMadeOutOfPharmacySales.InvokeUpdate();
        AchievementNotificationCenter.Instance.CoinsMadeByCuringClinicPatients.InvokeUpdate();
        AchievementNotificationCenter.Instance.PatientDiagnosed.InvokeUpdate();
        AchievementNotificationCenter.Instance.KidCured.InvokeUpdate();
        AchievementNotificationCenter.Instance.VIPArrived.InvokeUpdate();
        AchievementNotificationCenter.Instance.VIPCured.InvokeUpdate();
        AchievementNotificationCenter.Instance.CureLabBuilt.InvokeUpdate();
        AchievementNotificationCenter.Instance.TrailerWatched.InvokeUpdate();
        AchievementNotificationCenter.Instance.PanaceaCollectorUpgraded.InvokeUpdate();
        AchievementNotificationCenter.Instance.AdPlaced.InvokeUpdate();
        AchievementNotificationCenter.Instance.ClinicRoomBuilt.InvokeUpdate();
        AchievementNotificationCenter.Instance.CoinsInvestedInDecorating.InvokeUpdate();
        AchievementNotificationCenter.Instance.MedicalPlantsPicked.InvokeUpdate();
        AchievementNotificationCenter.Instance.MedicalFungiPicked.InvokeUpdate();
        AchievementNotificationCenter.Instance.HospitalExpanded.InvokeUpdate();
        AchievementNotificationCenter.Instance.NoYouDont.InvokeUpdate();

        AccountManager.Instance.UpdateAchievements(AchievementList);
    }

}

public class Achievement
{
	private bool collected = true;

	private int listID = 0;
	private int toCollect = 0;
	
    public AchievementInfo achievementInfo;

    public string id { get; private set; }
    public int progress { get; private set; }
    public List<int> time { get; private set; }
    public int stage { get; private set; }
    public bool completed { get; private set; }
	public bool Collected { get { return collected;} set { collected = value;}}
	public int ToCollect {get { return toCollect;} set { toCollect = value;}}

    public Dictionary<string, int> arrivedTime { get; private set; }

    public void setAchievementInfo(AchievementInfo ai) { achievementInfo = ai; }

    public void setID(string newId) { id = newId; }
    public void setProgress(int newProgress) { progress = newProgress; }
    public void setTime(List<int> newTime) { time = newTime; }
    public List<int> getTime() { return time; }
    public void setStage(int newStage) { stage = newStage; }
    public void setCompleted(bool newCompleted) { completed = newCompleted; }
    public void setArrivedTime(Dictionary<string, int> newArrivedTime) { arrivedTime = newArrivedTime; }

    public void removeFromTime(int item) { time.Remove(item); }

    public Achievement() { }
    public Achievement(string newid, AchievementInfo ai, int listID)
    {
		this.listID = listID;
        achievementInfo = ai;
        id = newid;
        progress = 0;
        time = new List<int>();
        stage = 0;
        completed = false;
    }

    public void initializeFromSave(string data)
    {
        string[] parameters = data.Split(';');

        setID(parameters[0]);
        setProgress(int.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture));
        setStage(int.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture));
        setCompleted(bool.Parse(parameters[3]));

        List<int> timeList = new List<int>();
        Dictionary<string, int> arrivedList = new Dictionary<string, int>();

        int arrivedPos = parameters.Length;

        for (int i = 4; i < parameters.Length; i++)
        {
            if (parameters[i] == "arrived")
                arrivedPos = i;
        }

        for (int i = 4; i < arrivedPos; i++)
        {
            timeList.Add(int.Parse(parameters[i], System.Globalization.CultureInfo.InvariantCulture));
        }

        setTime(timeList);
        
        if (arrivedPos != parameters.Length)
        {
            for (int i = arrivedPos; i < parameters.Length; i++)
            {
                arrivedList.Add(parameters[i].Split(':')[0], Convert.ToInt32(parameters[i].Split(':')[1]));
            }

            setArrivedTime(arrivedList);
        }
		int span = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);

		if (time.Count > 0)
        {
			if (span - time [0] >= 172800)
				setProgress (0);
		}
        else
			setProgress (0);

        /*
        int arrivedPos = parameters.Length;
        Debug.Log(arrivedPos);
        if (arrivedPos != 4)
        {
            for (int i = 4; i < parameters.Length; i++)
            {
                if (parameters[i] == "arrived")
                {
                    arrivedPos = i;
                }
            }

            for (int i = 4; i < arrivedPos; i++)
            {
                timeList.Add(int.Parse(parameters[i], System.Globalization.CultureInfo.InvariantCulture));
            }

            setTime(timeList);

            if (arrivedPos != parameters.Length)
            {
                for (int i = arrivedPos+1; i < parameters.Length; i++)
                {
                    arrivedList.Add(parameters[i].Split(':')[0], Convert.ToInt32(parameters[i].Split(':')[1]));
                }
            }

            setArrivedTime(arrivedList);
        }

        if (time!= null) Debug.Log(time[20]);*/

    }

    public string prepareDataToSave()
    {
		string saveString = id + ";" + progress + ";" + stage + ";" + completed.ToString();

        foreach (int item in getTime())
        {
            saveString += ";" + item.ToString();
        }
      
        if (arrivedTime != null)
        {
            saveString += ";arrived";
            foreach (var item in arrivedTime)
            {
                saveString += ";" + item.Key.ToString() + ":" + item.Value.ToString();
            }
        }

        return saveString;
    }

	public void ClaimReward()
    {
		Collected = true;
        
		for (int i = stage - toCollect; i < stage; i++)
        {
            GameState.Get().AchievementsDone++;
            SendRewards (i);
		}

        toCollect = 0;
        UIController.getHospital.AchievementsPopUp.UpdateByID(achievementInfo.achievementID);
        UIController.getHospital.AchievementsPopUp.GetComponent<AchievementPopUpController>().Exit();
    }

	void SendRewards(int id)
    {
        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
        GameState.Get().AddResource(ResourceType.Exp, achievementInfo.starRewards[id], EconomySource.Achievement, false);
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, UIController.getHospital.achievementIndicator.transform.position, achievementInfo.starRewards[id], 0f, 1.75f, Vector3.one, new Vector3 (1, 1, 1), null, null, () =>
		{
            GameState.Get().UpdateCounter(ResourceType.Exp, achievementInfo.starRewards[id], currentExpAmount);
        });

        int diamondAmountAmount = Game.Instance.gameState().GetDiamondAmount();
        GameState.Get().AddResource(ResourceType.Diamonds, achievementInfo.diamondRewards[id], EconomySource.Achievement, false);
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Diamond, UIController.getHospital.achievementIndicator.transform.position, achievementInfo.diamondRewards[id], 0f, 1.75f, Vector3.one, new Vector3 (1, 1, 1), null, null, () =>
        {
            GameState.Get().UpdateCounter(ResourceType.Diamonds, achievementInfo.diamondRewards[id], diamondAmountAmount);
        });
	}

    public void AddListener()
    {
		AchievementNotificationCenter anc = AchievementNotificationCenter.Instance;

        switch (id)
        {
		    case "ad_placer": anc.AdPlaced.AchieveUpdate += UpdateProgresChanged;
                anc.AdPlaced.Notification += ChangeProgress;
                break;
			
		    case "go_green": anc.MedicalPlantsPicked.AchieveUpdate += UpdateProgresChanged;
                anc.MedicalPlantsPicked.Notification += ChangeProgress;
                break;
			
		    case "elixirs_elixirs_everywhere": anc.ElixirCollected.AchieveUpdate += UpdateProgresTimedWithReset;
                anc.ElixirCollected.Notification += ChangeProgressTimedWithReset;
                break;
			
		    case "good_looking": anc.CoinsInvestedInDecorating.AchieveUpdate += UpdateProgresChanged;
                anc.CoinsInvestedInDecorating.Notification += ChangeProgress;
                break;
			
		    case "had_a_look": anc.TrailerWatched.AchieveUpdate += UpdateProgresChanged;
                anc.TrailerWatched.Notification += ChangeProgress;
                break;
			
		    case "hospital_tycoon": anc.ClinicRoomBuilt.AchieveUpdate += UpdateProgresChanged;
                anc.ClinicRoomBuilt.Notification += ChangeProgress;
                break;
			
		    case "i_love_bubbles": anc.BubblePopped.AchieveUpdate += UpdateProgressDaily; 
                anc.BubblePopped.Notification += ChangeProgressDaily;
                break;
			
		    case "in_n_out": anc.CoinsMadeByCuringClinicPatients.AchieveUpdate += UpdateProgresChanged;
                anc.CoinsMadeByCuringClinicPatients.Notification += ChangeProgress;
                break;
			
		    case "lab_works": anc.CureProduced.AchieveUpdate += UpdateProgresChanged; 
                anc.CureProduced.Notification += ChangeProgress;
                break;
			
		    case "lets_take_a_look": anc.PatientDiagnosed.AchieveUpdate += UpdateProgresChanged;
                anc.PatientDiagnosed.Notification += ChangeProgress;
                break;
			
		    case "money_well_spent": anc.CureLabBuilt.AchieveUpdate += UpdateProgresChanged;
                anc.CureLabBuilt.Notification += ChangeProgress;
                break;
			
		    case "oh_how_cute": anc.KidCured.AchieveUpdate += UpdateProgresChanged; 
                anc.KidCured.Notification += ChangeProgress;
                break;
			
		    case "panacea_flow": anc.PanaceaCollectorUpgraded.AchieveUpdate += UpdateProgresChanged; 
                anc.PanaceaCollectorUpgraded.Notification += ChangeProgress;
                break;
			
		    case "please_come_again": anc.CoinsMadeOutOfPharmacySales.AchieveUpdate += UpdateProgresChanged; 
                anc.CoinsMadeOutOfPharmacySales.Notification += ChangeProgress;
                break;
			
		    case "the_mashroom_picker": anc.MedicalFungiPicked.AchieveUpdate += UpdateProgresChanged; 
                anc.MedicalFungiPicked.Notification += ChangeProgress;
                break;
			
		    case "think_big_get_big": anc.HospitalExpanded.AchieveUpdate += UpdateProgresChanged;
                anc.HospitalExpanded.Notification += ChangeProgress;
                break;
			
		    case "turbo_doc": anc.TreatmentCenterPatientCured.AchieveUpdate += UpdateProgresTimedWithReset; 
                anc.TreatmentCenterPatientCured.Notification += ChangeProgressTimedWithReset;
                break;

            case "vip_lonuge":
			    //anc.VIPArrived.Notification += ChangeVIPArrivedTimes;
                anc.VIPArrived.Notification += ChangeVIPArrivedTimes;
		        //	anc.VIPCured.Notification += ChangeVIPCured;
                anc.VIPCured.Notification += ChangeVIPCured;
                break;

		    case "whats_up_doc": anc.PatientInClinicCured.AchieveUpdate += UpdateProgresChanged; 
                anc.PatientInClinicCured.Notification += ChangeProgress;
                break;

            case "no_you_dont":
                anc.NoYouDont.AchieveUpdate += UpdateProgresChanged;
                anc.NoYouDont.Notification += ChangeProgress;
                break;

            default:
                break;
        }
    }

    public void RemoveListener()
    {
		AchievementNotificationCenter anc = AchievementNotificationCenter.Instance;

        switch (id)
        {
            case "ad_placer":
		        //anc.AdPlaced.AchieveUpdate -= UpdateProgresChanged;
			    anc.AdPlaced.Notification -= ChangeProgress;
			break;
            case "go_green": 
		        //anc.MedicalPlantsPicked.AchieveUpdate -= UpdateProgresChanged; 
			    anc.MedicalPlantsPicked.Notification -= ChangeProgress; 
			break;
			case "elixirs_elixirs_everywhere":
		        //anc.ElixirCollected.AchieveUpdate -= UpdateProgresTimedWithReset;
			    anc.ElixirCollected.Notification -= ChangeProgressTimedWithReset;
			break;
            case "good_looking": 
		        //anc.CoinsInvestedInDecorating.AchieveUpdate -= UpdateProgresChanged;
			    anc.CoinsInvestedInDecorating.Notification -= ChangeProgress;
			break;
            case "had_a_look": 
		        //anc.TrailerWatched.AchieveUpdate -= UpdateProgresChanged;
			    anc.TrailerWatched.Notification -= ChangeProgress;
			break;
            case "hospital_tycoon": 
		        //anc.ClinicRoomBuilt.AchieveUpdate -= UpdateProgresChanged;
			    anc.ClinicRoomBuilt.Notification -= ChangeProgress;
			break;
            case "i_love_bubbles": 
		        //	anc.BubblePopped.AchieveUpdate -= UpdateProgressDaily;
			    anc.BubblePopped.Notification -= ChangeProgressDaily;
			break;
            case "in_n_out": 
		        //anc.CoinsMadeByCuringClinicPatients.AchieveUpdate -= UpdateProgresChanged;
			    anc.CoinsMadeByCuringClinicPatients.Notification -= ChangeProgress;
			break;
            case "lab_works":
		        //anc.CureProduced.AchieveUpdate -= UpdateProgresChanged;
			    anc.CureProduced.Notification -= ChangeProgress;
			break;
            case "lets_take_a_look": 
		        //anc.PatientDiagnosed.AchieveUpdate -= UpdateProgresChanged;
			    anc.PatientDiagnosed.Notification -= ChangeProgress;
			break;
            case "money_well_spent": 
		        //anc.CureLabBuilt.AchieveUpdate -= UpdateProgresChanged;
			    anc.CureLabBuilt.Notification -= ChangeProgress;
			break;
            case "oh_how_cute":
		        //anc.KidCured.AchieveUpdate -= UpdateProgresChanged;
			    anc.KidCured.Notification -= ChangeProgress;
			break;
            case "panacea_flow": 
		        //anc.PanaceaCollectorUpgraded.AchieveUpdate -= UpdateProgresChanged;
			    anc.PanaceaCollectorUpgraded.Notification -= ChangeProgress;
			break;
            case "please_come_again":
		        //anc.CoinsMadeOutOfPharmacySales.AchieveUpdate -= UpdateProgresChanged;
			    anc.CoinsMadeOutOfPharmacySales.Notification -= ChangeProgress;
			break;
            case "the_mashroom_picker": 
		        //anc.MedicalFungiPicked.AchieveUpdate -= UpdateProgresChanged;
			    anc.MedicalFungiPicked.Notification -= ChangeProgress;
			break;
            case "think_big_get_big": 
		        //anc.HospitalExpanded.AchieveUpdate -= UpdateProgresChanged;
			    anc.HospitalExpanded.Notification -= ChangeProgress;
			break;
		    case "turbo_doc": 
		        //anc.TreatmentCenterPatientCured.AchieveUpdate -= UpdateProgresTimedWithReset;
			    anc.TreatmentCenterPatientCured.Notification -= ChangeProgressTimedWithReset;
			break;
            case "vip_lounge":			
			    anc.VIPArrived.Notification -= ChangeVIPArrivedTimes;
                anc.VIPCured.Notification -= ChangeVIPCured;
            break;
            case "whats_up_doc":
		        //anc.PatientInClinicCured.AchieveUpdate -= UpdateProgresChanged; 
			    anc.PatientInClinicCured.Notification -= ChangeProgress; 
			break;
            case "no_you_dont":
                //	anc.NoYouDont.AchieveUpdate -= UpdateProgresChanged; 
                anc.NoYouDont.Notification -= ChangeProgress;
            break;

            default: break;
        }
    }
		
    public void ChangeProgress(AchievementProgressEventArgs args)
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
            return;

        if (!completed)
        {
           // int previousProgress = progress;
            int newProgress = progress + args.amount;

			if (newProgress >= achievementInfo.requiredValues [stage])// && previousProgress < achievementInfo.requiredValues[stage])
            {
			    if (achievementInfo.achievementType == AchievementInfo.AchievementType.Standard)                
					setProgress (0);

				if (stage == achievementInfo.requiredValues.Count - 1)
                {
					setProgress(achievementInfo.requiredValues[stage]);
					setCompleted (true);
					RemoveListener ();
				}
				UIController.getHospital.AchievementsInfoPopUp.Open (I2.Loc.ScriptLocalization.Get(achievementInfo.titleString));
				setStage (stage + 1);
				toCollect++;
				Collected = false;
				UIController.getHospital.AchievementsPopUp.ac.achievementChecked = false;
				UIController.getHospital.achievementIndicator.Activate(!HospitalAreasMapController.HospitalMap.VisitingMode);
			}
            else if(achievementInfo.achievementType != AchievementInfo.AchievementType.StandardIncremental && achievementInfo.achievementType != AchievementInfo.AchievementType.Upgrade)
            {
				setProgress(newProgress);
			}

			if(achievementInfo.achievementType == AchievementInfo.AchievementType.StandardIncremental || achievementInfo.achievementType == AchievementInfo.AchievementType.Upgrade)
            {
				setProgress(newProgress);
			}

			UIController.getHospital.AchievementsPopUp.UpdateByID(achievementInfo.achievementID);
       }
    }

    public void UpdateProgresChanged()
    {
		UIController.getHospital.AchievementsPopUp.UpdateByID(achievementInfo.achievementID);
	}

    public void ValidateProgress()
    {
        if (!completed && toCollect == 0)
        {
            if (stage < achievementInfo.requiredValues.Count)
            {
                int stagesToUpgrade = 0;

                if (progress > achievementInfo.requiredValues[stage])
                {
                    if (stage != achievementInfo.requiredValues.Count)
                    {
                        // CALC PROGRESS AND STAGE FOR CURRENT ACHIEVEMENT VALUES
                        if (achievementInfo.achievementType == AchievementInfo.AchievementType.Standard)
                        {
                            int newProgress = progress;

                            for (int i = stage; i < achievementInfo.requiredValues.Count; i++)
                            {
                                if (progress >= achievementInfo.requiredValues[i])
                                {
                                    stagesToUpgrade++;

                                    if (i < achievementInfo.requiredValues.Count - 1)
                                        newProgress = newProgress - achievementInfo.requiredValues[i];
                                }
                            }
                            progress = newProgress;
                        }
                        else
                        {
                            for (int i = stage; i < achievementInfo.requiredValues.Count; i++)
                            {
                                if (progress >= achievementInfo.requiredValues[i])
                                    stagesToUpgrade++;
                            }
                        }

                        // Set Max value for achievement if progress is over max progress 
                        int maxValue = achievementInfo.requiredValues[achievementInfo.requiredValues.Count - 1];

                        if (progress >= maxValue && (stage + stagesToUpgrade) >= achievementInfo.requiredValues.Count)
                        {
                            progress = achievementInfo.requiredValues[(stage + stagesToUpgrade) - 1];
                            setCompleted(true);
                            RemoveListener();
                        }

                        setProgress(progress);

                        setStage(stage + stagesToUpgrade);
                        toCollect += stagesToUpgrade;
                        Collected = false;
                        UIController.getHospital.AchievementsPopUp.ac.achievementChecked = false;
                        UIController.getHospital.achievementIndicator.Activate(!HospitalAreasMapController.HospitalMap.VisitingMode);

                        if (achievementInfo.achievementType == AchievementInfo.AchievementType.StandardIncremental || achievementInfo.achievementType == AchievementInfo.AchievementType.Upgrade)
                            setProgress(progress);

                        Debug.LogError("There was an error with value in single achievement then I validate it.");
                    }
                    else
                    {
                        progress = achievementInfo.requiredValues[achievementInfo.requiredValues.Count - 1];
                        setCompleted(true);
                        RemoveListener();
                        setProgress(progress);
                        Debug.LogError("There was an error with value in single achievement then I validate it.");
                    }
                }
            }
            else
            {
                if (progress > achievementInfo.requiredValues[achievementInfo.requiredValues.Count - 1])
                {
                    progress = achievementInfo.requiredValues[achievementInfo.requiredValues.Count - 1];
                    setCompleted(true);
                    RemoveListener();
                    setProgress(progress);
                    Debug.LogError("There was an error with value in single achievement then I validate it.");
                }
            }
        }
    }

    public void ChangeProgressTimed(TimedAchievementProgressEventArgs args)
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
            return;

        if (!completed)
        {
            int span = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);

            List<int> indexesOfOldItems = new List<int>();

            // clear items that fall out of required time
            foreach (var item in time)
            {
                if ((span - item) >= (achievementInfo.requiredTimes[stage] * 60)) indexesOfOldItems.Add(item);
            }

            foreach (var item in indexesOfOldItems)
            {
                removeFromTime(item);
            }

            // add set of new items that amount is equal to "amount" (e.g more then 1 thing is produced, we have few items with the same time)
            for (int i = 0; i < args.amount; i++)
            {
                time.Add(args.occurred);
            }

            setProgress(time.Count);

            //Debug.Log(time.Count);

            if (time.Count >= achievementInfo.requiredValues[stage])
            {
                // give rewards
               // Debug.Log("give rewards " + achievementInfo.diamondRewards[stage].ToString() + " stage " + stage);

                if (stage == achievementInfo.requiredValues.Count - 1)
                {
                 //   Debug.Log("COMPLETED");
					setProgress(achievementInfo.requiredValues[stage]);
                    setCompleted(true);
                    RemoveListener();
                }
				UIController.getHospital.AchievementsInfoPopUp.Open (I2.Loc.ScriptLocalization.Get(achievementInfo.titleString));
				setStage(stage + 1);
				//GameState.Get().AchievementsDone++;
				toCollect++;
				Collected = false;

				UIController.getHospital.AchievementsPopUp.ac.achievementChecked = false;
				UIController.getHospital.achievementIndicator.Activate(!HospitalAreasMapController.HospitalMap.VisitingMode);
            }
        }
        UIController.getHospital.AchievementsPopUp.UpdateByID(achievementInfo.achievementID);
    }

    public void ChangeProgressTimedWithReset(TimedAchievementProgressEventArgs args)
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
            return;

        if (!completed)
        {
            int span = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);

            List<int> indexesOfOldItems = new List<int>();

            // clear items that fall out of required time
            foreach (var item in time)
            {
                if ((span - item) >= (achievementInfo.requiredTimes[stage] * 60))
                    indexesOfOldItems.Add(item);
            }

            foreach (var item in indexesOfOldItems)
            {
                    removeFromTime(item);
            }

            // add set of new items that amount is equal to "amount" (e.g more then 1 thing is produced, we have few items with the same time)
            for (int i = 0; i < args.amount; i++)
            {
                time.Add(args.occurred);
            }

            setProgress(time.Count);

            //  Debug.Log(time.Count);

            if (time.Count >= achievementInfo.requiredValues[stage])
            {
                // give rewards
               // Debug.Log("give rewards " + achievementInfo.diamondRewards[stage].ToString() + " stage " + stage);

                if (stage == achievementInfo.requiredValues.Count - 1)
                {
                   // Debug.Log("COMPLETED");
					setProgress(achievementInfo.requiredValues[stage]);
                    setCompleted(true);
                    RemoveListener();                   
                }
				UIController.getHospital.AchievementsInfoPopUp.Open (I2.Loc.ScriptLocalization.Get(achievementInfo.titleString));
				setStage(stage + 1);
				//GameState.Get().AchievementsDone++;
				toCollect++;
				Collected = false;

				UIController.getHospital.AchievementsPopUp.ac.achievementChecked = false;
				UIController.getHospital.achievementIndicator.Activate(!HospitalAreasMapController.HospitalMap.VisitingMode);
            }
        }
        UIController.getHospital.AchievementsPopUp.UpdateByID(achievementInfo.achievementID);
    }

	public void UpdateProgresTimedWithReset()
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)    
            return;

        if (!completed && time != null && time.Count > 0)
        {
			int span = Convert.ToInt32 ((ServerTime.Get().GetServerTime().Subtract (new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);

			List<int> indexesOfOldItems = new List<int> ();

			foreach (var item in time)
            {
				if ((span - item) >= (achievementInfo.requiredTimes [stage] * 60))
					indexesOfOldItems.Add (item);
			}

			foreach (var item in indexesOfOldItems)
            {
				removeFromTime (item);
			}

			setProgress (time.Count);

		}
		UIController.getHospital.AchievementsPopUp.UpdateByID (achievementInfo.achievementID);
	}

    public void ChangeProgressDaily(TimedAchievementProgressEventArgs args)
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
            return;

        if (!completed)
        {
//ostro do zmiany
			int span = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
			//muszę dodać zapis ostatniego czasu przy savie
			if (time.Count == 0)
            {
				setProgress(1);
				time.Add(args.occurred);
			}
            else
            {
				if (span - time[0] >= 43200 && span - time[0] < 86400) setProgress(progress + 1);
				else if(span - time[0] >= 86400) setProgress(1);

				time[0] = args.occurred;
			}

            if (progress == achievementInfo.requiredValues[stage])
            {
                //give rewards
                //  Debug.Log("give rewards " + achievementInfo.starRewards[stage] + "    " + stage);               

				if (stage == achievementInfo.requiredValues.Count - 1)
                {
					Debug.Log ("COMPLETED");
					setCompleted (true);
					RemoveListener ();
				}
				UIController.getHospital.AchievementsInfoPopUp.Open (I2.Loc.ScriptLocalization.Get(achievementInfo.titleString));
				setStage (stage + 1);

			    //	GameState.Get().AchievementsDone++;
				toCollect++;
				Collected = false;

				UIController.getHospital.AchievementsPopUp.ac.achievementChecked = false;
				UIController.getHospital.achievementIndicator.Activate(!HospitalAreasMapController.HospitalMap.VisitingMode);
            }
        }
        UIController.getHospital.AchievementsPopUp.UpdateByID(achievementInfo.achievementID);
    }

	public void UpdateProgressDaily()
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)    
            return;

        if (!completed && time != null && time.Count > 0)
        {
			int span = Convert.ToInt32 ((ServerTime.Get().GetServerTime().Subtract (new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
			if (span - time [0] >= 172800)
				setProgress (0);			
		}
		UIController.getHospital.AchievementsPopUp.UpdateByID (achievementInfo.achievementID);
	}

    public void ChangeProgressTimedWithRequirements(TimedAchievementProgressEventArgs args)
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
            return;

        if (!completed)
        {
            if (time.Count == 0)
                time.Add(args.occurred);
            else
            {
                int span = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
                if (span - time[0] >= achievementInfo.requiredTimes[stage])
                {
                    setProgress(0);
                    time.Clear();
                }
                else
                {
                    if (progress >= achievementInfo.requiredValues[stage])
                    {
                        // give rewards
                        //   Debug.Log("Give rewards " + achievementInfo.starRewards[stage] + " stage " + stage);

                        if (stage == achievementInfo.requiredValues.Count - 1)
                        {
							setProgress(achievementInfo.requiredValues[stage]);
                            // Debug.Log("COMPLETED");
							setProgress(achievementInfo.requiredValues[stage]);
                            setCompleted(true);
                            RemoveListener();                           
                        }
						UIController.getHospital.AchievementsInfoPopUp.Open (I2.Loc.ScriptLocalization.Get(achievementInfo.titleString));
						setStage(stage + 1);
					    //	GameState.Get().AchievementsDone++;
						toCollect++;
						Collected = false;

						UIController.getHospital.AchievementsPopUp.ac.achievementChecked = false;
						UIController.getHospital.achievementIndicator.Activate(!HospitalAreasMapController.HospitalMap.VisitingMode);
                    }
                }
            }
        }
        UIController.getHospital.AchievementsPopUp.UpdateByID(achievementInfo.achievementID);
    }
    
    // metody dla Vip Lounge, nie jestem pewny czy dzialaja.. 
    public void ChangeVIPArrivedTimes(AchievementVIPInfoEventArgs args)
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
            return;

        //if (arrivedTime != null) {
        arrivedTime.Add (args.info.tag, args.occurred);
		//}
    }

    public void ChangeVIPCured(AchievementVIPInfoEventArgs args)
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
            return;

        if (!completed)
        {
            foreach (var item in arrivedTime)
            {
                if (item.Key == args.info.tag)
                {
                    int span = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
                    if (span - arrivedTime[item.Key] >= achievementInfo.requiredTimes[stage])
                    {
                        arrivedTime.Remove(item.Key);
                    }
                    else
                    {
                        if (progress >= achievementInfo.requiredValues[stage])
                        {
                            // give rewards
                            // Debug.Log("Give rewards " + achievementInfo.starRewards[stage] + " stage " + stage);

                            if (stage == achievementInfo.requiredValues.Count - 1)
                            {
                                // Debug.Log("COMPLETED");
								setProgress(achievementInfo.requiredValues[stage]);
                                setCompleted(true);
                                RemoveListener();                               
                            }
							UIController.getHospital.AchievementsInfoPopUp.Open (I2.Loc.ScriptLocalization.Get(achievementInfo.titleString));
							setStage(stage + 1);
						    //	GameState.Get().AchievementsDone++;
							toCollect++;
							Collected = false;

							UIController.getHospital.AchievementsPopUp.ac.achievementChecked = false;
							UIController.getHospital.achievementIndicator.Activate(!HospitalAreasMapController.HospitalMap.VisitingMode);
                        }
                    }
                }
            }
        }
    }

	public void ReceptionLvlUp()
    {		
	}

}
