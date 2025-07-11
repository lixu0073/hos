using MovementEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableEventSystem;

public class GiftsSendController : MonoBehaviour
{

    #region Static

    private static GiftsSendController instance;

    public static GiftsSendController Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of GiftsSendController was found on scene!");
            return instance;
        }

    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of GiftsSendController entrypoint were found!");
        }
        instance = this;
    }

    private static int SECONDS_IN_MINUTE = 60;
    private static int DAY_IN_SECONDS = 3600 * 24;

    #endregion

    #region Events

    [SerializeField] GameEvent giftSentEvent;

    public delegate void OnDataChanged(string saveID = null);
    public static event OnDataChanged onUpdate;

    #endregion

    IEnumerator<float> refreshSendersCoroutine = null;
    private int AvailableGifts = 0;

    #region API

    public void Initialize()
    {
        NormalizeSendedGifts();
        NormalizeGiftsCooldownTimers(true);
        TryToRunRefreshCoroutine();
        CheckingForUnlockFeature();
    }

    public int GetAvailableGifts()
    {
        return AvailableGifts;
    }

    public int GetAmountOfGiftsToSend()
    {
        return GetMaxGiftsAmountPerDay();
    }

    public bool CanSendGiftToFriend(string saveID)
    {
        return !WasGiftSendedToFriend(saveID) && CanSendGiftWithCurrentLimit();
    }

    public bool WasGiftAlreadySendedToFriend(string saveID)
    {
        return WasGiftSendedToFriend(saveID);
    }

    public bool AreThereAnyFriendsToGiveGift()
    {
        List<Hospital.IFollower> friendsList = null;

        friendsList = FriendsDataZipper.GetFbAndIGFWithoutWise();
        for (int i = 0; i < friendsList.Count; ++i)
        {
            if (CanSendGiftToFriend(friendsList[i].GetSaveID()))
            {
                friendsList.Clear();
                friendsList = null;
                return true;
            }
        }

        friendsList = FriendsDataZipper.RemoveInGameFriendsFbfromLikes();
        for (int i = 0; i < friendsList.Count; ++i)
        {
            if (CanSendGiftToFriend(friendsList[i].GetSaveID()))
            {
                friendsList.Clear();
                friendsList = null;
                return true;
            }
        }

        friendsList.Clear();
        friendsList = null;
        return false;
    }

    public void OnSuccessGiftSendToFriend(string saveID)
    {
        if (!CanSendGiftToFriend(saveID))
            return;
        if (!GiftsSynchronizer.Instance.GetSendedGifts().ContainsKey(saveID))
        {
            GiftsSynchronizer.Instance.GetData().sendedGifts.Add(saveID, UTCNow());
        }
        else
        {
            GiftsSynchronizer.Instance.GetData().sendedGifts[saveID] = UTCNow();
        }
        GiftsSynchronizer.Instance.GetData().giftsCooldownTimers.Add(UTCNow());
        NormalizeGiftsCooldownTimers();
        NormalizeSendedGifts();
        TryToRunRefreshCoroutine();
        onUpdate?.Invoke();
        AnalyticsController.instance.ReportGiftSent();
    }

    public void KillCoroutine()
    {
        if (refreshSendersCoroutine != null)
        {
            Timing.KillCoroutine(refreshSendersCoroutine);
            refreshSendersCoroutine = null;
        }
    }

    public long GetDurationInSecondsToNextGift()
    {
        long shortestTime = -1;
        foreach (long time in GiftsSynchronizer.Instance.GetGiftsCooldownTimers())
        {
            if (shortestTime == -1 || time < shortestTime)
            {
                shortestTime = time;
            }
        }
        return shortestTime == -1 ? shortestTime : System.Math.Max(-1, ((shortestTime + DAY_IN_SECONDS) - UTCNow()));
    }

    #endregion

    #region Methods

    private void CheckingForUnlockFeature()
    {
        GameState.OnLevelUp -= GameState_OnLevelUp;
        if (Game.Instance.gameState().GetHospitalLevel() < GiftsAPI.Instance.GiftsFeatureMinLevel)
            GameState.OnLevelUp += GameState_OnLevelUp;
    }

    private void GameState_OnLevelUp()
    {
        if (Game.Instance.gameState().GetHospitalLevel() >= GiftsAPI.Instance.GiftsFeatureMinLevel)
        {
            UIController.get.FriendsDrawer.SetAvailableGiftsAmountActive(true);
            onUpdate?.Invoke();
            GameState.OnLevelUp -= GameState_OnLevelUp;
        }
    }

    private void TryToRunRefreshCoroutine()
    {
        KillCoroutine();
        if (GiftsSynchronizer.Instance.GetGiftsCooldownTimers().Count >= GetMaxGiftsAmountPerDay() || GiftsSynchronizer.Instance.GetSendedGifts().Count > 0)
        {
            refreshSendersCoroutine = Timing.RunCoroutine(RefreshCoroutine());
        }
    }

    private IEnumerator<float> RefreshCoroutine()
    {
        while (true)
        {
            NormalizeGiftsCooldownTimers();
            NormalizeSendedGifts();
            yield return Timing.WaitForSeconds(DefaultConfigurationProvider.GetConfigCData().GiftsToSendRefreshIntervalInSeconds);
        }
    }

    private Hospital.BalanceableInt amountOfGiftsToSendBalanceable;

    private int GetMaxGiftsAmountPerDay()
    {
        if (amountOfGiftsToSendBalanceable == null)
        {
            amountOfGiftsToSendBalanceable = Hospital.BalanceableFactory.CreateMaxGiftsToSendBalanceable();
        }
        return amountOfGiftsToSendBalanceable.GetBalancedValue();
    }

    private int GetCooldownToSendGiftToSpecificFriendInMinutes()
    {
        return DefaultConfigurationProvider.GetConfigCData().CooldownToSendGiftToSpecificFriendInMinutes;
    }

    private bool WasGiftSendedToFriend(string saveID)
    {
        if (!GiftsSynchronizer.Instance.GetSendedGifts().ContainsKey(saveID))
            return false;
        long lastSendedTime = GiftsSynchronizer.Instance.GetData().sendedGifts[saveID];
        return lastSendedTime > UTCNow() - GetCooldownToSendGiftToSpecificFriendInMinutes() * SECONDS_IN_MINUTE;
    }

    private bool CanSendGiftWithCurrentLimit()
    {
        if (GiftsSynchronizer.Instance.GetGiftsCooldownTimers().Count < GetMaxGiftsAmountPerDay())
            return true;
        long uTCNow = UTCNow();
        foreach (long time in GiftsSynchronizer.Instance.GetGiftsCooldownTimers())
        {
            if (uTCNow - DAY_IN_SECONDS > time)
                return true;
        }
        return false;
    }

    private void NormalizeGiftsCooldownTimers(bool forceNotifyUdpate = false)
    {
        long uTCNow = UTCNow();
        List<long> rowsToDelete = new List<long>();
        foreach (long time in GiftsSynchronizer.Instance.GetGiftsCooldownTimers())
        {
            if (time <= uTCNow - DAY_IN_SECONDS)
            {
                rowsToDelete.Add(time);
            }
        }
        AvailableGifts = Mathf.Max(0, GetAmountOfGiftsToSend() - (GiftsSynchronizer.Instance.GetGiftsCooldownTimers().Count - rowsToDelete.Count));
        bool notifyUdpate = false;
        foreach (long time in rowsToDelete)
        {
            notifyUdpate = true;
            GiftsSynchronizer.Instance.GetData().giftsCooldownTimers.Remove(time);
        }
        if (notifyUdpate || forceNotifyUdpate)
        {
            onUpdate?.Invoke();
        }
    }

    private void NormalizeSendedGifts()
    {
        long uTCNow = UTCNow();
        List<string> savesToDelete = new List<string>();
        foreach (KeyValuePair<string, long> data in GiftsSynchronizer.Instance.GetSendedGifts())
        {
            if (data.Value <= uTCNow - GetCooldownToSendGiftToSpecificFriendInMinutes() * SECONDS_IN_MINUTE)
            {
                savesToDelete.Add(data.Key);
            }
        }
        foreach (string saveID in savesToDelete)
        {
            GiftsSynchronizer.Instance.GetData().sendedGifts.Remove(saveID);
            onUpdate?.Invoke(saveID);
        }
    }

    private long UTCNow()
    {
        return (long)ServerTime.getTime();
    }


    private void OnDestroy()
    {
        GameState.OnLevelUp -= GameState_OnLevelUp;
    }
    #endregion

}
