using UnityEngine;
using System.Collections;
using Hospital;
using System.Collections.Generic;
using System;

/// <summary>
/// Manager Chestów pod global eventy, np halloween
/// OBECNIE TYLKO ACTIVITY GE
/// nie kontroluje obecnie "zwykłych" chestów / normalnych eventów
/// </summary>
public class GlobalEventChestsManager : MonoBehaviour
{
    [HideInInspector]
    public long LastSpawntime = -1;
    public long GetLastSpawnTime { get { return LastSpawntime; } }

    [HideInInspector]
    public int CurrentChestCountLimit = 0;
    public long GetCurrentChestCountLimit { get { return CurrentChestCountLimit; } }

    private int MaxChestsCount = 0;
    public int GetMaxChestsCount() { return MaxChestsCount; }
    private long ChestInterval = 0;
    /// <summary>
    /// used as fallback in case of fuckup.
    /// </summary>
    //public GameEventChestClicker ChestPrefab;
    /// <summary>
    /// Order in this array should correlate to ActivityCollectableType enum.
    /// element 0 is fallback
    /// </summary>
    //public GameEventChestClicker[] ChestPrefabs;

    /// <summary>
    ///  list of chests made by this class instance.
    /// </summary>
    [HideInInspector]
    private Dictionary<GameEventChestClicker, int> chestInstances = new Dictionary<GameEventChestClicker, int>();
#pragma warning disable 0649
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private ParticleSystem entryParticles;
#pragma warning restore 0649
    [HideInInspector]
    private CollectOnMapActivityGlobalEvent.ActivityCollectableType activityCol = CollectOnMapActivityGlobalEvent.ActivityCollectableType.None;
    
    [HideInInspector]
    private CollectOnMapActivityGlobalEvent GE;
#pragma warning disable 0649
    Coroutine _spawnerCoroutine;
#pragma warning restore 0649

    private void OnDisable()
    {
        if (_spawnerCoroutine != null)
        {
            try
            {
                StopCoroutine(_spawnerCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    public void Initialize(
        bool IsVisitingMode,
        int MaxChestsCount,
        CollectOnMapActivityGlobalEvent ge,
        CollectOnMapActivityGlobalEvent.ActivityCollectableType activityCol = CollectOnMapActivityGlobalEvent.ActivityCollectableType.None,
        long lastSpawnTime = -1,
        int currentChestCount = 0,
        long chestInterval = 0
    )
    {
        //
        //Just a reminder: This if has actually sense, this.activityCol and activityCol are 2 different things
        if (this.activityCol == CollectOnMapActivityGlobalEvent.ActivityCollectableType.None && activityCol != CollectOnMapActivityGlobalEvent.ActivityCollectableType.None)
        {
            this.activityCol = activityCol;
            GE = ge;
        }

        if (GE == null)
        {
            StopAllCoroutines();
            RemoveAllChests();
            return;
        }

        CurrentChestCountLimit = GE.GlobalEventChestsCount;
        LastSpawntime = GE.LastGlobalEventChestSpawnTime;

        this.MaxChestsCount = MaxChestsCount;
        this.ChestInterval = chestInterval;
        StopAllCoroutines();
        RemoveAllChests();
        chestInstances.Clear();
        ForceSpawnChests();

        StartCoroutine(SpawnerCoroutine());
    }

    /// <summary>
    /// Check if Chest Events Ought to be enabled
    /// </summary>
    /// <returns>GE: True if Collecting Pumpkins is active</returns>
    public bool IsEventChestsEnabled()
    {
        if (GE != null && ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive())
            return true;
        return false;
    }

    private void RemoveAllChests()
    {
        foreach (KeyValuePair<GameEventChestClicker, int> chest in chestInstances)
        {
            HospitalAreasMapController.HospitalMap.treasureManager.ResetSpot(chest.Value);
            if (chest.Key.gameObject != null)
                Destroy(chest.Key.gameObject);
        }
        chestInstances.Clear();
    }

    public void RemoveAllGlobalEventChests()
    {
        StopAllCoroutines();
        RemoveAllChests();
    }

    private int CurrentlyTimelyAllowedNumOfChests()
    {
        if (LastSpawntime != -1)
        {
            int NumberOfPossibleChestsBecauseTimePassed = (int)(((long)ServerTime.getTime() - LastSpawntime) / ChestInterval);
            if (NumberOfPossibleChestsBecauseTimePassed > 0 && CurrentChestCountLimit < MaxChestsCount)
            {
                LastSpawntime = (long)ServerTime.getTime();
            }
            if (NumberOfPossibleChestsBecauseTimePassed + CurrentChestCountLimit >= MaxChestsCount)
            {
                CurrentChestCountLimit = MaxChestsCount;
                return MaxChestsCount;
            }
            CurrentChestCountLimit += NumberOfPossibleChestsBecauseTimePassed;

            return CurrentChestCountLimit;
        }
        CurrentChestCountLimit = MaxChestsCount;
        GE.GlobalEventChestsCount = CurrentChestCountLimit;
        LastSpawntime = (long)ServerTime.getTime();
        return MaxChestsCount;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns>-1 if false, time to </returns>
    public long GetTimeStampAfterAllChestsWillBeAvailable()
    {
        long when;
        long endtime = (long)ReferenceHolder.GetHospital().globalEventController.GetCurrentGlobalEventEndTime;
        long now = (long)ServerTime.getTime();
        if (LastSpawntime != -1)
        {
            when = LastSpawntime + (ChestInterval * (MaxChestsCount - CurrentChestCountLimit));
            if (CurrentChestCountLimit == MaxChestsCount)
            {
                long ret = now + 600;
                return (ret >= endtime) ? -1 : ret;
            }
            if (when >= endtime || when <= now)           
                return -1;
        }
        else        
            when = -1;

        return when;
    }

    private void SpawnChests(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            SpawnChestInRandomSpot();
        }
    }

    private IEnumerator SpawnerCoroutine()
    {
        while (IsEventChestsEnabled())
        {
            yield return new WaitForSeconds(ChestInterval);
            SpawnChestInRandomSpot();
        }
    }

    public void DebugSpawnAChest()
    {
        KeyValuePair<int, Vector3> spot = HospitalAreasMapController.HospitalMap.treasureManager.RandomSpot();
        SpawnChestInPosition(spot.Key, spot.Value);
    }

    public void ForceSpawnChests()
    {
        //int bla = new int();
        //bla = Random.Range(0, CurrentlyTimelyAllowedNumOfChests());
        int bla = CurrentlyTimelyAllowedNumOfChests();
        SpawnChests(bla);
    }

    public CollectOnMapActivityGlobalEvent.ActivityCollectableType GetActivityCollectableType()
    {
        return activityCol;
    }

    private void SpawnChestInRandomSpot()
    {
        if (MaxChestsCount <= chestInstances.Count)
            return;
        //if (CurrentChestCountLimit >= chestInstances.Count)
        //{
        KeyValuePair<int, Vector3> spot = HospitalAreasMapController.HospitalMap.treasureManager.RandomSpot();
        SpawnChestInPosition(spot.Key, spot.Value);
        //}
    }

    private void SpawnChestInPosition(int positionIndex, Vector3 position)
    {
        CollectOnMapGEGraphicsManager.GetInstance.ApplyCollectibleParticleSystem();
        GameEventChestClicker Chest = CollectOnMapGEGraphicsManager.GetInstance.SpawnCollectible().GetComponent<GameEventChestClicker>();  //Instantiate(ChestPrefab) as GameEventChestClicker;
        Chest.gameObject.transform.SetParent(HospitalAreasMapController.HospitalMap.treasureManager.transform);
        Chest.gameObject.transform.position = position;
        Chest.gameObject.SetActive(true);
        chestInstances.Add(Chest, positionIndex);
        Chest.AddOnClickListener(() =>
        {
            HospitalAreasMapController.HospitalMap.treasureManager.ResetSpot(chestInstances[Chest]);
            chestInstances.Remove(Chest);
            Destroy(Chest.gameObject);
            //CurrentChestCountLimit--;
            GE.GlobalEventChestsCount = CurrentChestCountLimit;
            LastSpawntime = (long)ServerTime.getTime();
            GE.LastGlobalEventChestSpawnTime = LastSpawntime;

            SoundsController.Instance.PlayPumpkinSmash(); //KEK
            GlobalEventNotificationCenter.Instance.CollectGlobalEvent.Invoke(new GlobalEventCollectProgressEventArgs());
            particles.gameObject.transform.position = position;
            particles.Play();
        });

        entryParticles.gameObject.transform.position = position;
        entryParticles.Play();
    }

    private void AddReward(CollectOnMapActivityGlobalEvent.ActivityCollectableItemRewardType reward, Vector3 pos, int amount)
    {
        switch (reward)
        {
            case CollectOnMapActivityGlobalEvent.ActivityCollectableItemRewardType.None:
                {
                    break;
                }
            case CollectOnMapActivityGlobalEvent.ActivityCollectableItemRewardType.Munaaay:
                {
                    AddCoinReward(pos);
                    particles.gameObject.transform.position = pos;
                    particles.Play();
                    break;
                }
            case CollectOnMapActivityGlobalEvent.ActivityCollectableItemRewardType.Experiance:
                {
                    AddExpReward(pos);
                    particles.gameObject.transform.position = pos;
                    particles.Play();
                    break;
                }
            default:
                {
                    Debug.LogError("This type of collectable reward for Chest is not implemented");
                    break;
                }
        }
    }
    private void AddCoinReward(Vector3 position)
    {
        /*
        if (activityCol != CollectOnMapActivityGlobalEvent.ActivityCollectableType.None)
        {
            int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
            GameState.Get().AddResource(ResourceType.Coin, awardAmount, EconomySource.GlobalEventReward, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, position + new Vector3(-.1f, .75f, 0), awardAmount, 0.1f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[0], null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Coin, awardAmount, currentCoinAmount);
            }, false);
        }*/
    }

    private void AddExpReward(Vector3 position)
    {
        /*
        if (activityCol != CollectOnMapActivityGlobalEvent.ActivityCollectableType.None)
        {
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, awardAmount, EconomySource.GlobalEventReward, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, position + new Vector3(-.1f, .75f, 0), awardAmount, 0.1f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[2], null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Exp, awardAmount, currentExpAmount);
            }, false);
        }
        */
    }
}
