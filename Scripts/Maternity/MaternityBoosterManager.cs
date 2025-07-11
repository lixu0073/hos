using Hospital;
using MovementEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityBoosterManager : BoosterManager
{
    protected override int NewBoostersCount
    {
        get
        {
            return base.NewBoostersCount;
        }

        set
        {
            newBoostersCount = value;
        }
    }

    public override void SetBooster(int boosterID, int duration, bool onLoad = false)
    {
        ClearBooster();
        if (duration <= 0)
        {
            return;
        }

        boosterEndTime = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds) + duration;

        currentBoosterID = boosterID;

        boosterActive = true;
        Timing.RunCoroutine(turnOffBoosterDelay(duration));

        switch (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterType)
        {
            case BoosterType.Coin:
                switch (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget)
                {
                    case BoosterTarget.MaternityPatients:
                        Debug.LogError("Jakis wplyw na pacjentow maternity");
                        break;
                    default:
                        break;
                }
                break;
            case BoosterType.Exp:
                switch (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget)
                {
                    case BoosterTarget.MaternityPatients:
                        Debug.LogError("Jakis wplyw na pacjentow maternity");
                        break;
                    default:
                        break;
                }
                break;
            case BoosterType.CoinAndExp:
                switch (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget)
                {
                    case BoosterTarget.MaternityPatients:
                        Debug.LogError("Jakis wplyw na pacjentow maternity");
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    public override void ClearBooster()
    {
        Timing.KillCoroutine(turnOffBoosterDelay().GetType());
        currentBoosterID = -1;
        boosterActive = false;
        NewBoostersCount = newBoostersCount; // show badge after booster ends
        MaternityGameState.Get().maternityBoosters.coinsMaternity = 1; //matward sprawdzic gdzie to sie powinno uzywac na podstawie GameState.HospitalBoosters
        MaternityGameState.Get().maternityBoosters.expMaternity = 1;
    }

    public override void LoadFromString(string saveString, bool visitingMode)
    {
        ClearIndicators();

        if (!visitingMode)
        {
            if (!string.IsNullOrEmpty(saveString))
            {
                var save = saveString.Split(';');

                currentBoosterID = int.Parse(save[0], System.Globalization.CultureInfo.InvariantCulture);
                boosterEndTime = int.Parse(save[1], System.Globalization.CultureInfo.InvariantCulture);
                var savedStored = save[2].Split('?');

                if (ResourcesHolder.Get().boosterDatabase.boosters.Length > savedStored.Length)
                {
                    boosterStorage = new int[ResourcesHolder.Get().boosterDatabase.boosters.Length];
                    Debug.LogError("Booster storage should be biger than in save so i created new. Convert old save to new!");
                }
                else boosterStorage = new int[savedStored.Length];

                for (int i = 0; i < boosterStorage.Length; i++)
                {
                    if (i < savedStored.Length)
                        boosterStorage[i] = int.Parse(savedStored[i], System.Globalization.CultureInfo.InvariantCulture);
                    else boosterStorage[i] = 0;
                }
                if (currentBoosterID > -1)
                {

                    int duration = boosterEndTime - Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
                    SetBooster(currentBoosterID, duration, true);
                }
            }
            else
            {
                ClearBooster();
                for (int i = 0; i < boosterStorage.Length; i++)
                {
                    boosterStorage[i] = 0;
                }
            }
        }
        else
        {
            ClearBooster();
            for (int i = 0; i < boosterStorage.Length; i++)
            {
                boosterStorage[i] = 0;
            }
        }
    }
}
