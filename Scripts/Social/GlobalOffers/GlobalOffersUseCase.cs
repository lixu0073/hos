using Hospital;
using Hospital.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GlobalOffersUseCase : MonoBehaviour
{
    private enum Operator
    {
        LessOrEqual,
        GreaterOrEqual,
        None
    };

    #region static
    private static GlobalOffersUseCase instance;

    public static GlobalOffersUseCase Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of GlobalOffersUseCase was found on scene!");
            return instance;
        }
    }

    private const int QueryLimit = 2000;

    #endregion

    public List<PharmacyOrderAdvertised> Offers { get; private set; } = new List<PharmacyOrderAdvertised>();
    private List<PromotionData> promos = new List<PromotionData>();


    void Awake()
    {
        if (instance != null && instance != this)        
            Debug.LogWarning("Multiple instances of GlobalOffersUseCase entrypoint were found!");

        instance = this;
    }

    public async void GetOffers(OnSuccess onSuccess = null, OnFailure onFailure = null)
    {
        int maxLevel = Game.Instance.gameState().GetHospitalLevel();
        Offers.Clear();

        var randomTask = GetRandomOffers(maxLevel);
        var friendsTask = GetFriendsOffers(maxLevel);

        try
        {
            await Task.WhenAll(randomTask, friendsTask);
            bool successRandom = randomTask.Status == TaskStatus.RanToCompletion;
            bool successFriends = friendsTask.Status == TaskStatus.RanToCompletion;
            if (successRandom && successFriends)
                onSuccess?.Invoke();
        }
        catch (Exception e)
        {
            onFailure?.Invoke(e);
        }
    }

    private async Task GetRandomOffers(int maxLevel)
    {
        Guid guid = Guid.NewGuid();
        string uuid = guid.ToString();
        Operator firstOperator = RandomOperator();
        promos.Clear();

        await QueryRandomOffers(CognitoEntry.SaveID, uuid, firstOperator, maxLevel);
        if (!HasAllOffers())
            await QueryRandomOffers(CognitoEntry.SaveID, uuid, GetOppositeOperator(firstOperator), maxLevel);

        if (promos.Count > 0)
            await LoadOffers();
    }

    private Task GetFriendsOffers(int maxLevel)
    {
        var t = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        FriendsOffersUseCase.Instance.GetFriendsOffers(GameState.Get().GetFriendsOffersSlotsCount(), maxLevel, (friendsOffers) => {
            foreach (PharmacyOrderAdvertised offer in friendsOffers)
                Offers.Add(offer);
            t.SetResult(true);
        }, (ex) => {
            t.SetException(ex);
        });
        return t.Task;
    }

    private Operator RandomOperator()
    {
        return UnityEngine.Random.Range(0, 2) == 0 ? Operator.LessOrEqual : Operator.GreaterOrEqual;
    }

    private Operator GetOppositeOperator(Operator op)
    {
        if (op == Operator.None)
            return RandomOperator();
        return op == Operator.LessOrEqual ? Operator.GreaterOrEqual : Operator.LessOrEqual;
    }

    private bool HasAllOffers()
    {
        return promos.Count == DefaultConfigurationProvider.GetConfigCData().GlobalOffersMaxSlotsCount;
    }

    private async Task QueryRandomOffers(string userID, string uuid, Operator uuidOperator, int maxLevel)
    {
        PromotionConnector.FromQueryAsync(uuid, uuidOperator == Operator.LessOrEqual, maxLevel, QueryLimit);
        while (!PromotionConnector.IsSearchDone() && promos.Count < DefaultConfigurationProvider.GetConfigCData().GlobalOffersMaxSlotsCount)
        {
            var result = await PromotionConnector.GetNextSetAsync();
            if (result != null)
            {
                AddNewPromos(result, userID);
            }
            else
            {
                PromotionConnector.ClearSearch();
                throw new Exception("No Result");
            }
        }
        PromotionConnector.ClearSearch();
    }

    private async Task LoadOffers()
    {
        var keys = new List<(string, string)>();
        foreach (PromotionData promo in promos)
            keys.Add((promo.UserID, promo.OrderID));

        try
        {
            var result = await PharmacyOrderConnector.BatchGetAdvertisedAsync(keys);
            Offers.AddRange(result);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void AddNewPromos(List<PromotionData> newPromos, string userID)
    {
        foreach (PromotionData newPromo in newPromos)
        {
            if (HasAllOffers())
                break;
            if (newPromo.UserID == userID)
                continue;
            if (promos.Any(promo => promo.UserID == newPromo.UserID))
                continue;
            promos.Add(newPromo);
        }
    }
}
