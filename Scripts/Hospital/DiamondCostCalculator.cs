using UnityEngine;
using System.Collections;

public static class DiamondCostCalculator
{

    /// <summary>
    /// Calculates cost in diamonds for speeding up building rooms and machines
    /// </summary>
    /// <param name="timeRemaining">remaining build time</param>
    /// <param name="baseTime">starting build time</param>
    /// <returns></returns>
    public static int GetCostForBuilding(float timeRemaining, float baseTime, string objectTag = "")
    {
        if (objectTag.Length > 0 && (objectTag == "BlueDoc" || objectTag == "WaitingRoomBlueOrchid" || objectTag == "LabourRoomBlueOrchid" || objectTag == "SyrupLab" || objectTag == "GreenDoc" || objectTag == "ElixirLab" || objectTag == "YellowDoc"))
            return 0;

        int baseCost = 0;
        if (baseTime < 7200)
        {
            baseCost = Mathf.RoundToInt((baseTime * 0.0021667f) + 0.35f);
        }
        else
        {
            baseCost = Mathf.RoundToInt(((baseTime - 7200f) * 0.0006f) + 16f);
        }
        int finalCost = Mathf.CeilToInt(timeRemaining / baseTime * baseCost);
        if (finalCost == 0)
            finalCost = 1;
        //Debug.Log("Diamond cost for speeding up this building is: " + finalCost);
        return finalCost;
    }

    /// <summary>
    /// Calculates cost in diamonds for speeding up actions like curing patients and producing elixirs/cures
    /// </summary>
    /// <param name="timeRemaining">remaining action time</param>
    /// <param name="baseTime">starting action time</param>
    public static int GetCostForAction(float timeRemaining, float baseTime, string objectTag = "", bool isTutorial = false)
    {
        if (objectTag.Length > 0)
        {
            TutorialController tc = TutorialController.Instance;
            if (objectTag == "BlueDoc" && isTutorial)
            {
                //Debug.LogError("Speed up will be free!");
                return 0;
            }
            if (objectTag == "SyrupLab" && isTutorial)
            {
                //Debug.LogError("Speed up will be free!");
                return 0;
            }
        }

        int baseCost = 0;
        if (baseTime < 3600)
        {
            baseCost = Mathf.RoundToInt((baseTime * 0.000833f) + 1.215f);
        }
        else
        {
            baseCost = Mathf.RoundToInt(((baseTime - 3600f) * 0.0002267f) + 4.215f);
        }

        int finalCost = Mathf.RoundToInt(timeRemaining / baseTime * baseCost);
        if (finalCost == 0)
            finalCost = 1;
        return finalCost;
    }

    /// <summary>
    /// Calculates Tier1 Case cost in diamonds
    /// </summary>
    /// <param name="timeRemaining">remaining action time</param>
    /// <param name="baseTime">starting action time</param>
    public static int GetCostForCase(float timeRemaining, float baseTime)
    {
        int baseCost = 0;
        if (baseTime < 3600)
        {
            baseCost = Mathf.RoundToInt((baseTime * 0.000833f) + 1.215f);
        }
        else
        {
            baseCost = Mathf.RoundToInt(((baseTime - 3600f) * 0.0002267f) + 4.215f);
        }

        int finalCost = Mathf.RoundToInt(timeRemaining / baseTime * baseCost);
        if (finalCost == 0)
            finalCost = 1;
        return finalCost;
    }

    /// <summary>
    /// Calculates amount of coins in the smallest IAP package based on player level (higher level = more coins)
    /// This value is also used when calculating diamond cost of missing coins
    /// </summary>
    /// <param name="playerLevel"></param>
    /// <returns></returns>
    public static int GetBaseIAPCoinAmount()
    {
        int playerLevel = Game.Instance.gameState().GetHospitalLevel();
        float baseAmountThisLevel = Mathf.Log10(playerLevel + 3) * (playerLevel * playerLevel * 0.48f) + 180;
        return Mathf.RoundToInt(baseAmountThisLevel);
    }

    /// <summary>
    /// Calculates cost in diamonds for coins when player wants to buy something he can't afford
    /// </summary>
    /// <param name="missingCoinsAmount"></param>
    /// <returns></returns>
    public static int GetMissingCoinsCost(int missingCoinsAmount)
    {
        float diamondCost = (float)missingCoinsAmount / (float)GetBaseIAPCoinAmount() * 30f;
        //Debug.Log("Calculated diamond cost for : " + missingCoinsAmount + " coins is " + diamondCost);
        return Mathf.CeilToInt(diamondCost);
    }

    public static int GetMissingPositiveCost(int missingPositiveAmount)
    {
        //float positiveCost = ((float)missingPositiveAmount * 25) / (float)GetBaseIAPCoinAmount() * 30f; //25 is the coin value of positive enrgy, same value iused point calculating bed patient reward
        float positiveCost = missingPositiveAmount * 2;     //1 positive energy = 2 diamonds

        //Debug.Log("Calculated diamond cost for : " + missingCoinsAmount + " coins is " + diamondCost);
        return Mathf.CeilToInt(positiveCost);
    }

    /// <summary>
    /// Returns cost in diamonds for expanding queue slots to doctors or machines.
    /// </summary>
    /// <param name="slotIndex">Index of purchasable slot. I.E. 1 for first purchasable slot (3rd slot over all)</param>
    /// <returns></returns>
    public static int GetQueueSlotCost(int slotIndex)
    {
        int price = 3 + 3 * slotIndex;
        //Debug.Log("Queue slot price index: " + slotIndex + " is " + price);
        return price;
    }


    /// <summary>
    /// Returns a price in COINS for expanding your hospital
    /// </summary>
    /// <returns></returns>
    public static int GetExpansionCost(ExpansionType type, bool tutorialMode = false)
    {
        int unlockedAmount = 0;
        switch (type)
        {
            case ExpansionType.Clinic:
                unlockedAmount = Game.Instance.gameState().GetExpansionClinicAmount();
                break;
            case ExpansionType.Lab:
                unlockedAmount = GameState.Get().ExpansionsLab;
                unlockedAmount += 1;    //to avoid giving free first expansion in Lab (Clinic has free expansion)
                unlockedAmount *= 2;    //'double' cost for Lab
                break;
            case ExpansionType.MaternityClinic:
                unlockedAmount = Game.Instance.gameState().GetExpansionClinicAmount();
                unlockedAmount += 15; // offset for Matenrity Expansion cost in regards to hospital expansion
                break;
            default:
                unlockedAmount = 100;
                break;
        }
        if (type == ExpansionType.Clinic && tutorialMode)
        {
            return 0;
        }

        int expectedPlayerLevel = unlockedAmount + 1;
        float diaCost = 15 + unlockedAmount * 27;
        float coinCost = diaCost * (Mathf.Log10(expectedPlayerLevel + 3) * (expectedPlayerLevel * expectedPlayerLevel * 0.48f) + 180) / 30;
        //coinCost /= 2f; //THIS IS FOR TEST BUILD 2016-09-02
        coinCost /= 4f; //THIS IS A TWEAK FROM 2016-09-09

        Debug.Log("Calculated Expansion cost = " + coinCost);
        return Mathf.RoundToInt(coinCost);
    }


    public static int GetPharmacyMoreOffersCost(int salesUnlocked)
    {
        //TOFIX
        return salesUnlocked * 2 + 3;
    }
}
