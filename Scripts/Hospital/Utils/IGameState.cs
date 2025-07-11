using Hospital;
using System.Collections.Generic;
using UnityEngine;

public interface IGameState
{
    ElixirStorage ElixirStore
    {
        get;
        set;
    }
    ElixirTank ElixirTank
    {
        get;
        set;
    }
    string LootBoxTransaction
    {
        get;
        set;
    }
    bool AddLootBoxReward
    {
        get;
        set;
    }
    string PersonalFriendCode
    {
        get;
        set;
    }

    List<string> RandomFriendsIds
    {
        get;
        set;
    }

    int RandomFriendsTimestamp
    {
        get;
        set;
    }
    int CoinAmountFromIAP
    {
        get;
        set;
    }


    int DiamondAmountFromIAP
    {
        get;
        set;
    }
    int PositiveEnergyAmount
    {
        get;
        set;
    }

    bool EverOpenedCrossPromotionPopup
    {
        get;
        set;
    }

    bool CrossPromotionCompleted
    {
        get;
        set;
    }

    bool IsMaternityFirstLoopCompleted
    {
        get;
        set;
    }

    bool GetCure(MedicineRef type, int amount, EconomySource source);
    int GetHospitalLevel();
    int GetMaternityLevel();
    int GetLevelSceneDependent();
    bool IsFBRewardConnectionClaimed();
    bool IsNoMoreRemindFBConnection();
    bool IsFBConnectionRewardEnabled();
    bool IsEverLoggedInFB();
    bool IsHomePharmacyVisited();
    bool CheckCommunityRewardState(int ID);
    bool IsStarterPackUsed();
    int GetCoinAmount();
    int GetDiamondAmount();
    int GetCurrencyAmount(ResourceType type);
    int GetPositiveEnergyAmount();
    int GetExperienceAmount();
    int GetExpansionClinicAmount();
    long GetTimedOfferEndDate();
    long GetTimedOfferPurchaseDate();
    bool IsAdRewardActive();
    string GetAdType();
    void IncrementAdsWatched();
    int GetIAPValentineCount();
    int GetIAPEasterCount();
    string GetLastSuccessfulPurchase();
    int GetIAPPurchasesCount();
    int GetTankSorageLeftCapacity();
    #region Setters
    void SetStarterPackUsed(bool value);
    void SetNoMoreRemindFBConnection(bool value);
    void SetFBRewardConnectionClaimed(bool value);
    void SetFBConnectionRewardEnabled(bool value);
    void SetEverLoggedInFB(bool value);
    void SetCommunityRewardState(int ID, bool state);
    void SetSaveFilePrepared(bool value);
    void SetExpansionClinicAmount(int value);
    void SetTimedOfferEndDate(long time);
    void SetTimedOfferPurchaseDate(long time);
    #endregion
    void AddResource(ResourceType type, int amount, EconomySource source, bool updateCounter, string buildingTag = null);
    int AddResource(MedicineRef type, int amount, bool canExceedLimit, EconomySource source);
    void UpdateCounter(ResourceType type, int amount, int CurrentAmount);
    string GetHospitalName();
    bool HasAnyFollowings();
    void SetHasAnyFollowings(bool hasAny);
    bool HasAnyFollowers();
    void SetHasAnyFollowers(bool hasAny);
    bool HasAnyFriends();
    void SetHasAnyFriends(bool hasAny);
    void SaveData(Save save, bool isVisiting = false);
    void LoadData(Save save);
    void AddCoins(int amount, EconomySource source, bool updateCounter = true, string buildingTag = null);
    void AddDiamonds(int amount, EconomySource source, bool updateCounter = true, bool fromIAP = false, string buildingTag = null);
    void SetIAPBoughtLately(bool buyLately);
    void SetAdType(string AdType);
    void SetIsAdRewardActive(bool AddAdsReward);
    void LoadDrawerData(Save save);
    bool CheckIfLevelUP();
    void GiveLevelUpGifts();
    void RefreshXPBar();
    void CheckExpDependantTutorial(BaseNotificationEventArgs eventArgs);
    int GetExpForLevel(int level);
    void RemoveCoins(int amount, EconomySource source, bool updateCounter = true, string buildingTag = null);
    void RemoveDiamonds(int amount, EconomySource source, string buildingTag = null);
    void AddToObjectStored(ShopRoomInfo info, int amount = 1);
    void AddPositiveEnergy(int amount, EconomySource source, string buildingTag = null);
    void IncrementIAPValentineCount();
    void IncrementIAPEasterCount();
    void IncrementIAPPurchasesCount();
    void SetLastSuccessfulPurchase(string lastSuccessfulPurchase);
    float GetGameplayTimer();
    MasterableProperties GetMasterableProductionMachine(MedicineDatabaseEntry cure);
    BaseGameState.hospitalBoosters GetHospitalBoosterSystem();
    void UpdateExtraGift(Vector3 position, bool isDoctor, SpecialItemTarget tank = SpecialItemTarget.All);
    int CanAddResource(MedicineRef medToAdd, int amount, bool overflowStorage);
    int GetCureCount(MedicineRef medicine);
    void RemoveBadge(Rotations rotation);
    /// <summary>
    /// Only for dev purposes.
    /// </summary>
    /// <param name="coinsAmountToSet"></param>
    void SetCoinAmount(int coinsAmountToSet);
    void SetDiamondAmount(int diamondAmount);
    void LevelUp();
}
