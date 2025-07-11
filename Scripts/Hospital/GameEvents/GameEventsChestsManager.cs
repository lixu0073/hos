using UnityEngine;
using Hospital;
using SimpleUI;

public class GameEventsChestsManager : MonoBehaviour
{
    private long LastSpawnTime = -1;
    private int ChestsCount = 0;
    private BaseGameEventInfo EventInfo;

    public GameEventChestClicker ChestPrefab;
#pragma warning disable 0649
    [SerializeField] private ParticleSystem particles;
#pragma warning restore 0649

    public void Initialize(bool IsVisitingMode, long LastSpawnTime, int ChestsCount)
    {
        EventInfo = GameEventsController.Instance.currentEvent;
        if (!IsEventChestsEnabled())
        {
            RemoveAllChests();
            return;
        }
        this.LastSpawnTime = LastSpawnTime;
        this.ChestsCount = ChestsCount;
        RemoveAllChests();
        if (!IsVisitingMode)
            SpawnChests();
    }

    public int GetChestsCount()
    {
        return ChestsCount;
    }

    public long GetLastSpawnTime()
    {
        return LastSpawnTime;
    }

    private bool IsEventChestsEnabled()
    {
        return GameEventsController.Instance.IsAnyEventActive() && EventInfo.IsChestsEnabled();
    }

    private void RemoveAllChests()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Chest"))
        {
            Destroy(go);
        }
    }

    private void SpawnChests()
    {
        if (IsLastSpawnTimeSet())
        {
            SpawnChests(ChestsCount);
            int NumberOfChestsToRandom = (int)(((long)ServerTime.getTime() - LastSpawnTime) / EventInfo.GetChestInterval());
            NumberOfChestsToRandom = Mathf.Max(0, Mathf.Min(NumberOfChestsToRandom, EventInfo.GetMaxNumberOfChests() - ChestsCount));
            SpawnChests(NumberOfChestsToRandom, true);
        }
        else
            SpawnChestInRandomSpot(true);
    }

    private void SpawnChests(int count, bool isNew = false)
    {
        for (int i = 0; i < count; ++i)
        {
            SpawnChestInRandomSpot(isNew);
        }
    }

    private void SpawnChestInRandomSpot(bool isNew = false)
    {
        if (ChestsCount >= EventInfo.GetMaxNumberOfChests() && isNew)
            return;

        if (isNew)
        {
            LastSpawnTime = (long)ServerTime.getTime();
            ChestsCount++;
        }
        SpawnChestInPosition(HospitalAreasMapController.HospitalMap.treasureManager.RandomSpot().Value);
    }

    private void SpawnChestInPosition(Vector3 position)
    {
        GameEventChestClicker Chest = Instantiate(ChestPrefab) as GameEventChestClicker;
        Chest.gameObject.transform.SetParent(HospitalAreasMapController.HospitalMap.treasureManager.transform);
        Chest.gameObject.transform.position = position;
        Chest.gameObject.SetActive(true);
        Chest.AddOnClickListener(() =>
        {
            LastSpawnTime = (long)ServerTime.getTime();
            Destroy(Chest.gameObject);
            --ChestsCount;
            if (ChestsCount < 0)
                ChestsCount = 0;
            AddCoinReward(position);
            particles.gameObject.transform.position = position;
            particles.Play();
            SoundsController.Instance.PlayEventChest();
        });
    }

    private void AddCoinReward(Vector3 position)
    {
        int coinsReward = EventInfo.GetCoinReward();
        int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
        GameState.Get().AddResource(ResourceType.Coin, coinsReward, EconomySource.GlobalEventReward, false);
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, position + new Vector3(-.1f, .75f, 0), coinsReward, 0.1f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[0], null, () =>
        {
            GameState.Get().UpdateCounter(ResourceType.Coin, coinsReward, currentCoinAmount);
        }, false);
    }

    private bool IsLastSpawnTimeSet()
    {
        return LastSpawnTime != -1;
    }

}
