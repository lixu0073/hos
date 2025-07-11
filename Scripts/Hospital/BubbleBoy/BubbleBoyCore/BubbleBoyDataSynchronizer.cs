using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MovementEffects;
using System;
using SimpleUI;

public class BubbleBoyDataSynchronizer {

    private BubbleBoySaveData data = new BubbleBoySaveData();

    private static BubbleBoyDataSynchronizer instance = null;

    public static BubbleBoyDataSynchronizer Instance
    {
        get
        {
            if (instance == null)
                instance = new BubbleBoyDataSynchronizer();

            return instance;
        }
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        int nextFreeEntryDate = Mathf.Max(Convert.ToInt32(data.nextFreeEntryDate), 0);
        builder.Append(Checkers.CheckedAmount(nextFreeEntryDate, 0, int.MaxValue, "BubbleBoy lastFreeEntryDate: ").ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(data.freeEntries, 0, int.MaxValue, "BubbleBoy freeEntries: ").ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(data.entriesThisSession, 0, int.MaxValue, "BubbleBoy entriesThisSession: ").ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(TotalEntries, 0, int.MaxValue, "BubbleBoy totalEntries: ").ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedBool(RefundExist).ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(data.refundCurrency, 0, int.MaxValue, "BubbleBoy refundCurrency: ").ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(data.refundAmount, 0, int.MaxValue, "BubbleBoy refundAmount: ").ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedExternalRoomState(Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState).ToString());
        // TO DO, ADD : Save BubbleBoyReward bestWonItem;

        return builder.ToString();

    }

    public void OnGameEventActivate()
    {
        if (IsFreeEntryAvailable())
            return;
        int newWaitTime = WaitTime;
        int now = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
        if (NextFreeEntryDate - now > newWaitTime)
        {
            NextFreeEntryDate = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds) + newWaitTime;
        }
    }

    public void LoadFromString(string saveString, bool visitingMode)
    {
        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split(';');
            data.nextFreeEntryDate = int.Parse(save[0], System.Globalization.CultureInfo.InvariantCulture);
            data.freeEntries = int.Parse(save[1], System.Globalization.CultureInfo.InvariantCulture);
            data.entriesThisSession = int.Parse(save[2], System.Globalization.CultureInfo.InvariantCulture);
            data.totalEntries = int.Parse(save[3], System.Globalization.CultureInfo.InvariantCulture);
            data.refundExist = Convert.ToBoolean(save[4], System.Globalization.CultureInfo.InvariantCulture);
            data.refundCurrency = int.Parse(save[5], System.Globalization.CultureInfo.InvariantCulture);
            data.refundAmount = int.Parse(save[6], System.Globalization.CultureInfo.InvariantCulture);
            data.bubbleOpened = 0;

            if (save.Length > 7)
            {
                Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState = (Hospital.ExternalRoom.EExternalHouseState)Enum.Parse(typeof(Hospital.ExternalRoom.EExternalHouseState), save[7]);
            }
            else {
                TutorialController tc = TutorialController.Instance;



                if (Game.Instance.gameState().GetHospitalLevel() >= Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.roomInfo.UnlockLvl && tc.GetStepId((StepTag)Enum.Parse(typeof(StepTag), SaveLoadController.SaveState.TutorialStepTag)) > tc.GetStepId(StepTag.bubble_boy_intro))
                {
                    Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState = Hospital.ExternalRoom.EExternalHouseState.enabled;
                }
                else {
                    Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState = Hospital.ExternalRoom.EExternalHouseState.disabled;
                }

            }
            // TO DO, ADD : Load BubbleBoyReward bestWonItem;
        }
        else
        {
            // for old saves

            TutorialController tc = TutorialController.Instance;

            if (Game.Instance.gameState().GetHospitalLevel() >= Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.roomInfo.UnlockLvl && tc.GetStepId((StepTag)Enum.Parse(typeof(StepTag), SaveLoadController.SaveState.TutorialStepTag)) > tc.GetStepId(StepTag.bubble_boy_intro))
            {
                Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState = Hospital.ExternalRoom.EExternalHouseState.enabled;
            }
            else {
                Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState = Hospital.ExternalRoom.EExternalHouseState.disabled;
            }
        }

        if (visitingMode)
        {
            ReferenceHolder.GetHospital().bubbleBoyCharacterAI.statusIndicator.HideFreeIndicator();
        }
    }

    public void GenerateDefaultSave()
    {
        Debug.LogError("Generating default save for BubbleBoy.");

        data.nextFreeEntryDate = 0;
        data.freeEntries = 0;
        data.entriesThisSession = 0;
        data.totalEntries = 0;
        data.refundExist = false;
        data.refundCurrency = 0;
        data.refundAmount = 0;
        data.bubbleOpened = 0;

        TutorialController tc = TutorialController.Instance;
        if (Game.Instance.gameState().GetHospitalLevel() >= Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.roomInfo.UnlockLvl && tc.GetStepId(tc.CurrentTutorialStepTag) > tc.GetStepId(StepTag.bubble_boy_intro))
        {
            Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState = Hospital.ExternalRoom.EExternalHouseState.enabled;
        }
        else {
            Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState = Hospital.ExternalRoom.EExternalHouseState.disabled;
        }
    }

    public bool IsBubbleBoyEnabled()
    {
        TutorialController tc = TutorialController.Instance;
        return Game.Instance.gameState().GetHospitalLevel() >= Hospital.HospitalAreasMapController.HospitalMap.bubbleBoy.roomInfo.UnlockLvl && tc.GetStepId(tc.CurrentTutorialStepTag) > tc.GetStepId(StepTag.bubble_boy_intro);
    }

    public void IncreaseTotalEntriesCount()
    {
        data.totalEntries++;
    }

    public bool RefundExist
    {
        get { return data.refundExist; }
        set
        {
            data.refundExist = value;
        }
    }

    public int TotalEntries
    {
        get { return data.totalEntries; }
        private set
        {
            data.totalEntries = value;
        }
    }

    public int RefundAmount
    {
        get { return data.refundAmount; }
        set
        {
            data.refundAmount = value;
        }
    }

    public int NextFreeEntryDate
    {
        get { return data.nextFreeEntryDate; }
        set
        {
            data.nextFreeEntryDate = value;
        }
    }

    public int FreeEntries
    {
        get { return data.freeEntries; }
        set
        {
            data.freeEntries = value;
        }
    }

    public int WaitTime
    {
        private set { }
        get
        {
            switch (BubbleBoyDataSynchronizer.Instance.FreeEntries)
            {
                case 0:
                    return 0;
                case 1:
                    return ToSeconds(0, 5, 0);  //5 * 60; // 5 min
                case 2:
                    return ToSeconds(0, 30, 0); //defaultValue = 30 * 60; // 30 min
                case 3:
                    return ToSeconds(0, 0, 1); //defaultValue = 60 * 60; // 1 hr
                case 4:
                    return ToSeconds(0, 0, 3); //defaultValue = 3 * 60 * 60; // 3 hr
                case 5:
                    return ToSeconds(0, 0, 6); //defaultValue = 6 * 60 * 60; // 6 hr                    
                default:
                    return NextFreeBubbleBoy; //defaultValue = 8 * 60 * 60; // 8 hr

            }
        }
    }

    private Hospital.BalanceableInt nextFreeBubbleBoyBalanceable;
    private int NextFreeBubbleBoy
    {
        get
        {
            if(nextFreeBubbleBoyBalanceable == null)
            {
                nextFreeBubbleBoyBalanceable = Hospital.BalanceableFactory.CreateNextFreeBubbleBoyBalanceable();
            }

            return nextFreeBubbleBoyBalanceable.GetBalancedValue();
        }
    }

    private int ToSeconds(int seconds, int minutes = 0, int hours = 0)
    {
        return
            seconds + 60 * (minutes + 60 * hours);
    }

    public bool IsFreeEntryAvailable()
    {
        int timePassed= NextFreeEntryDate - Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
        if (timePassed <= 0 || BubbleBoyDataSynchronizer.Instance.FreeEntries == 0)
            return true;

        return false;
    }

    public void ResetRefund()
    {
        RefundExist = false;
        RefundAmount = 0;
    }

    public int BubbleOpened
    {
        get { return data.bubbleOpened; }
        set
        {
            data.bubbleOpened = value;
        }
    }

}