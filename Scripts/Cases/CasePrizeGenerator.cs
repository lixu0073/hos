using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static CasePrizesParams;

public class CasePrizeGenerator
{

    CasePrizesParams casePrizesParams;
    List<ShopRoomInfo> decorationsToDraw;
    List<GiftReward> lootBoxRewards;

    public CasePrizeGenerator(CasePrizesParams parameters, List<ShopRoomInfo> decorationsToDraw, List<GiftReward> lootBoxRewards)
    {
        casePrizesParams = parameters;
        this.decorationsToDraw = decorationsToDraw;
        this.lootBoxRewards = lootBoxRewards;
    }

    public CasePrize GetCasePrize(int caseTier, CaseType type, EconomySource economySource, int casesStack, List<RewardPackage> dailyQuestRewards = null, BundleGiftableResource bundledGift = null)
    {
        switch (type)
        {
            case CaseType.ordinary:
                return GetCasePrizeFromBundleGift(bundledGift, economySource);
            case CaseType.VIP:
                return PrepareCasePrizeForVip(caseTier, type);
            case CaseType.FACEBOOK:
                return PrepareCasePrizeForFacebook(caseTier, type);
            case CaseType.EPIDEMY:
                return StandardPrize(caseTier, type, economySource);
            case CaseType.TREASURE:
                return PrepareCasePrizeForTreasure(caseTier, type, economySource);
            case CaseType.DAILY_QUEST:
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CourierPackage));
                return GetCasePrizeForDailyQuest(dailyQuestRewards);
            case CaseType.DAILY_REWARD:
                return GetCasePrizeFromBundleGift(bundledGift, economySource);
            default:
                return StandardPrize(caseTier, type, EconomySource.GiftBoxPrize);
        }
    }

    private CasePrize GetCasePrizeForDailyQuest(List<RewardPackage> dailyquestrewardStack)
    {
        if (dailyquestrewardStack != null && dailyquestrewardStack.Count > 0)
        {
            RewardPackage rewardPackage = dailyquestrewardStack[0];
            CasePrizeCreateInput input = new CasePrizeCreateInput(EconomySource.DailyQuestReward, 0, rewardPackage.GetCoinAmount(), rewardPackage.GetDiamondAmount(), 0, rewardPackage.GetMedicines(), rewardPackage.GetDecorations(), rewardPackage.GetBoosters());
            CasePrize caseToReturn = new CasePrize(input);
            return caseToReturn;
        }
        return null;
    }

    public CasePrize GetCasePrizeFromSingleGift(BaseGiftableResource giftableResource, EconomySource economySource)
    {
        if (giftableResource == null)
            return null;

        CasePrizeCreateInput input = null;
        int coinAmount = 0;
        int diamondAmount = 0;
        int positiveEnergyAmount = 0;
        List<BoosterItemCasePrizeType> boosters = new List<BoosterItemCasePrizeType>();
        List<ItemCasePrizeType> medicines = new List<ItemCasePrizeType>();
        List<DecorationCasePrizeType> decorations = new List<DecorationCasePrizeType>();
        
        switch (giftableResource.rewardType)
        {
            case BaseGiftableResourceFactory.BaseResroucesType.coin:
                coinAmount = giftableResource.GetGiftAmount();
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.exp:
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.diamond:
                diamondAmount = giftableResource.GetGiftAmount();
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.positiveEnergy:
                positiveEnergyAmount = giftableResource.GetGiftAmount();
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.booster:
                BoosterResource giftAsBooster = giftableResource as BoosterResource;
                boosters.Add(new BoosterItemCasePrizeType(giftAsBooster.GetBoosterID(), giftAsBooster.GetGiftAmount()));
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.medicine:
                MedicineResourceGiftableResource giftAsMedicine = giftableResource as MedicineResourceGiftableResource;
                medicines.Add(new ItemCasePrizeType(giftAsMedicine.GetMedicine(), giftAsMedicine.GetGiftAmount()));
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.decoration:
                DecorationGiftableResource giftAsDecoration = giftableResource as DecorationGiftableResource;
                decorations.Add(new DecorationCasePrizeType(giftAsDecoration.GetDecoInfo(), giftAsDecoration.GetGiftAmount()));
                break;
            default:
                break;
        }

        input = new CasePrizeCreateInput(economySource, 0, coinAmount, diamondAmount, positiveEnergyAmount, medicines, decorations, boosters);
        if (input != null)
        {
            CasePrize caseToReturn = new CasePrize(input);
            return caseToReturn;
        }

        return null;
    }

    private CasePrize GetCasePrizeFromBundleGift(BundleGiftableResource bundledGift, EconomySource economySource)
    {
        if (bundledGift != null && bundledGift.GetBundledGifts().Count > 0)
        {
            //BaseGiftableResource rewardPackage = dailyRewardGifts[0];
            CasePrizeCreateInput input = null;
            int coinAmount = 0;
            int diamondAmount = 0;
            int positiveEnergyAmount = 0;
            List<BoosterItemCasePrizeType> boosters = new List<BoosterItemCasePrizeType>();
            List<ItemCasePrizeType> medicines = new List<ItemCasePrizeType>();
            List<DecorationCasePrizeType> decorations = new List<DecorationCasePrizeType>();
            for (int i = 0; i < bundledGift.GetBundledGifts().Count; ++i)
            {
                switch (bundledGift.GetBundledGifts()[i].rewardType)
                {
                    case BaseGiftableResourceFactory.BaseResroucesType.coin:
                        coinAmount = bundledGift.GetBundledGifts()[i].GetGiftAmount();
                        break;
                    case BaseGiftableResourceFactory.BaseResroucesType.exp:
                        break;
                    case BaseGiftableResourceFactory.BaseResroucesType.diamond:
                        diamondAmount = bundledGift.GetBundledGifts()[i].GetGiftAmount();
                        break;
                    case BaseGiftableResourceFactory.BaseResroucesType.positiveEnergy:
                        positiveEnergyAmount = bundledGift.GetBundledGifts()[i].GetGiftAmount();
                        break;
                    case BaseGiftableResourceFactory.BaseResroucesType.booster:
                        BoosterResource giftAsBooster = bundledGift.GetBundledGifts()[i] as BoosterResource;
                        boosters.Add(new BoosterItemCasePrizeType(giftAsBooster.GetBoosterID(), giftAsBooster.GetGiftAmount()));
                        break;
                    case BaseGiftableResourceFactory.BaseResroucesType.medicine:
                        MedicineResourceGiftableResource giftAsMedicine = bundledGift.GetBundledGifts()[i] as MedicineResourceGiftableResource;
                        medicines.Add(new ItemCasePrizeType(giftAsMedicine.GetMedicine(), giftAsMedicine.GetGiftAmount()));
                        break;
                    case BaseGiftableResourceFactory.BaseResroucesType.decoration:
                        DecorationGiftableResource giftAsDecoration = bundledGift.GetBundledGifts()[i] as DecorationGiftableResource;
                        decorations.Add(new DecorationCasePrizeType(giftAsDecoration.GetDecoInfo(), giftAsDecoration.GetGiftAmount()));
                        break;
                    default:
                        break;
                }
            }

            input = new CasePrizeCreateInput(economySource, 0, coinAmount, diamondAmount, positiveEnergyAmount, medicines, decorations, boosters);
            if (input != null)
            {
                CasePrize caseToReturn = new CasePrize(input);
                return caseToReturn;
            }
        }
        return null;
    }

    private BalanceableInt treasureChestDiamondsAfterIAPBalanceable;
    private int TreasureChestDiamondsAfterIAP
    {
        get
        {
            if (treasureChestDiamondsAfterIAPBalanceable == null)
                treasureChestDiamondsAfterIAPBalanceable = BalanceableFactory.CreateTreasureChestDiamondsAfterIAPBalanceable();
            return treasureChestDiamondsAfterIAPBalanceable.GetBalancedValue();
        }
    }

    private CasePrize PrepareCasePrizeForTreasure(int caseTier, CaseType type, EconomySource economySource)
    {
        int coinsAmount = 0;
        int diamondsAmount = 0;
        int positiveEnergyAmount = 0;
        List<ItemCasePrizeType> items = null;

        float r = BaseGameState.RandomFloat(0, 1);
        if (r < AreaMapController.Map.treasureManager.weightRanges[0])
        {
            //Coins
            coinsAmount = coinGambling(caseTier);
        }
        else if (r < AreaMapController.Map.treasureManager.weightRanges[1] && TresureChestDeltaConfig.DiamondsFromTreasureChest)
        {
            //Diamonds
            diamondsAmount = diamondGambling(caseTier, type);
        }
        else
        {
            //Tools
            int itemAmount = casePrizesParams.itemAmount[caseTier];
            items = itemGambling(caseTier, GetShovelSource(caseTier, type), itemAmount);
        }

        if (TresureChestDeltaConfig.DiamondsFromTreasureChest)
        {
            if (GameState.Get().IAPBoughtLately)
            {
                if (items != null && items.Count > 0)
                    items.Clear();

                coinsAmount = 0;
                diamondsAmount = TreasureChestDiamondsAfterIAP;
                GameState.Get().IAPBoughtLately = false;
                GameState.Get().DiamondUsedLately = false;
            }
            else if (GameState.Get().DiamondUsedLately)
            {
                if (items != null && items.Count > 0)
                    items.Clear();

                coinsAmount = 0;
                diamondsAmount = 1;
                GameState.Get().DiamondUsedLately = false;
            }
        }

        CasePrizeCreateInput input = new CasePrizeCreateInput(economySource, caseTier, coinsAmount, diamondsAmount, positiveEnergyAmount, items);
        return new CasePrize(input);
    }

    private CasePrize StandardPrize(int caseTier, CaseType type, EconomySource economySource)
    {
        int coinsAmount = coinGambling(caseTier);
        int diamondsAmount = diamondGambling(caseTier, type);
        int positiveEnergyAmount = positiveEnergyGambling(caseTier);
        int itemAmount = returnNewItemsAmount(caseTier);
        List<ItemCasePrizeType> items = itemGambling(caseTier, GetShovelSource(caseTier, type), itemAmount);
        List<DecorationCasePrizeType> decorations = decorationGambling(caseTier, casePrizesParams.decoAmount[caseTier]);
        List<BoosterItemCasePrizeType> boosters = boosterGambling(caseTier);
        CasePrizeCreateInput input = new CasePrizeCreateInput(economySource, caseTier, coinsAmount, diamondsAmount, positiveEnergyAmount, items, decorations, boosters);
        return new CasePrize(input);
    }

    private CasePrize PrepareCasePrizeForFacebook(int caseTier, CaseType type)
    {
        int diamondAmount = diamondGambling(caseTier, type);
        CasePrizeCreateInput inputForFacebook = new CasePrizeCreateInput(EconomySource.GiftBoxPrize, 0, 0, diamondAmount, 0);
        return new CasePrize(inputForFacebook);
    }

    private CasePrize PrepareCasePrizeForVip(int caseTier, CaseType type)
    {
        int diamondsAmount = diamondGambling(caseTier, CaseType.VIP);
        int itemamount = returnNewItemsAmount(caseTier);
        List<ItemCasePrizeType> items = itemGambling(caseTier, GetShovelSource(caseTier, type), itemamount);
        List<DecorationCasePrizeType> decorations = decorationGambling(caseTier, casePrizesParams.decoAmount[caseTier]);
        CasePrizeCreateInput intpuForVip = new CasePrizeCreateInput(EconomySource.GiftBoxPrize, caseTier, 0, diamondsAmount, 0, items, decorations);
        return new CasePrize(intpuForVip);
    }

    public CasePrize GetGiftFromGlobalEvent(int globalEventCaseTier)
    {
        return StandardPrize(globalEventCaseTier, CaseType.ordinary, EconomySource.GlobalEventReward);
    }

    public CasePrize GetPrizeFromGifts(List<GiftReward> lootBoxRewards, EconomySource lootBox)
    {
        if (lootBoxRewards.Count > 0)
        {
            int coinsAmount = 0;
            int diamondsAmount = 0;
            int positiveEnergyAmount = 0;
            List<ItemCasePrizeType> items = new List<ItemCasePrizeType>();
            List<BoosterItemCasePrizeType> boosters = new List<BoosterItemCasePrizeType>();

            foreach (GiftReward prize in lootBoxRewards)
            {
                switch (prize.rewardType)
                {
                    case GiftRewardType.Coin:
                        coinsAmount += prize.amount;
                        break;
                    case GiftRewardType.Diamond:
                        diamondsAmount += prize.amount;
                        break;
                    case GiftRewardType.PositiveEnergy:
                        positiveEnergyAmount += prize.amount;
                        break;
                    case GiftRewardType.Mixture:
                        var giftItem = (prize as GiftRewardMixture);
                        if (giftItem != null)
                            items.Add(new ItemCasePrizeType(giftItem.GetRewardMedicineRef(), giftItem.amount));
                        break;
                    case GiftRewardType.StorageUpgrader:
                        var storageItem = (prize as GiftRewardStorageUpgrader);
                        if (storageItem != null)
                            items.Add(new ItemCasePrizeType(storageItem.GetRewardMedicineRef(), storageItem.amount));
                        break;
                    case GiftRewardType.Shovel:
                        var shovelItem = (prize as GiftRewardShovel);
                        if (shovelItem != null)
                            items.Add(new ItemCasePrizeType(shovelItem.GetRewardMedicineRef(), shovelItem.amount));
                        break;
                    case GiftRewardType.Booster:
                        var booster = (prize as GiftRewardBooster);
                        if (booster != null)
                            boosters.Add(new BoosterItemCasePrizeType(booster.GetBoosterId(), booster.amount));
                        break;
                    default:
                        break;
                }
            }
            CasePrizeCreateInput input = new CasePrizeCreateInput(lootBox, 0, coinsAmount, diamondsAmount, 0, items, null, boosters);
            return new CasePrize(input);
        }
        else
            return StandardPrize(5, CaseType.ordinary, lootBox);
    }

    public CasePrize GetCasePrizeFromString(string parameters)
    {
        return CasePrize.Parse(parameters);
    }

    #region RewardGamling
    private List<BoosterItemCasePrizeType> boosterGambling(int caseTier)
    {
        List<BoosterItemCasePrizeType> boosterList = new List<BoosterItemCasePrizeType>();
        BoosterProbabilities boosterProb;
        for (int i = 0; i < 3; ++i)
        {
            if (casePrizesParams.boosterProbabilities.Length <= caseTier)
            {
                boosterProb = casePrizesParams.boosterProbabilities[casePrizesParams.boosterProbabilities.Length - 1];
                Debug.LogError("<color=red>MHP-290</color>: BoosterGambling out of range: " + caseTier + "/" + casePrizesParams.boosterProbabilities.Length);
            }
            else
                boosterProb = casePrizesParams.boosterProbabilities[caseTier];

            if (BaseGameState.RandomFloat(0, 1) < boosterProb.boosterProbability[i])
            {
                int boosterID = (int)Mathf.Round((BaseGameState.RandomFloat(-0.49f, ResourcesHolder.Get().boosterDatabase.boosters.Length - 3 + 0.49f)));
                int index = boosterList.FindIndex(x => x.boosterID == boosterID);
                if (index == -1)
                {
                    BoosterItemCasePrizeType mItemPrize = new BoosterItemCasePrizeType(boosterID, 1);
                    boosterList.Add(mItemPrize);
                }
                else
                {
                    boosterList[index].amount++;
                }
            }
        }
        return boosterList;
    }

    public int coinGambling(int caseTier)
    {
        int coinAmount;

        if (casePrizesParams.coinRanges.Length > caseTier)
        {
            coinAmount = (int)BaseGameState.RandomFloat(casePrizesParams.coinRanges[caseTier].x, casePrizesParams.coinRanges[caseTier].y);
            coinAmount = (coinAmount / 5) * 5 + (int)Mathf.Round(coinAmount % 5 / (float)5) * 5;
        }
        else
        {
            Debug.LogError("<color=red>MHP-290</color>: CoinGambling out of range: " + caseTier + "/" + casePrizesParams.coinRanges.Length);
            coinAmount = 50; // default value to show something
        }

        return coinAmount;
    }

    public int diamondGambling(int caseTier, CaseType caseType)
    {
        int diamondAmount;
        if (caseType == CaseType.FACEBOOK)
            diamondAmount = 5;
        else
        {
            if (casePrizesParams.diamondRanges.Length > caseTier)
                diamondAmount = (int)BaseGameState.RandomFloat(casePrizesParams.diamondRanges[caseTier].x, casePrizesParams.diamondRanges[caseTier].y);
            else
            {
                Debug.LogError("<color=red>MHP-290</color>: DiamongGambling out of range: " + caseTier + "/" + casePrizesParams.diamondRanges.Length);
                diamondAmount = 3; // default value to show something
            }
        }
        return diamondAmount;
    }

    public int positiveEnergyGambling(int caseTier)
    {
        int positiveEnergyAmount;

        if (casePrizesParams.positiveEnergyRanges.Length > caseTier)
        {
            positiveEnergyAmount = (int)BaseGameState.RandomFloat(casePrizesParams.positiveEnergyRanges[caseTier].x, casePrizesParams.positiveEnergyRanges[caseTier].y);
            positiveEnergyAmount = (positiveEnergyAmount / 5) * 5 + (int)Mathf.Round(positiveEnergyAmount % 5 / (float)5) * 5;
        }
        else
        {
            Debug.LogError("<color=red>MHP-290</color>: PositiveEnergyGambling out of range: " + caseTier + "/" + casePrizesParams.positiveEnergyRanges.Length);
            positiveEnergyAmount = 1; // default value to show something
        }        

        return positiveEnergyAmount;
    }

    private int returnNewItemsAmount(int caseTier)
    {
        if (casePrizesParams.itemAmount.Length > caseTier)
            return casePrizesParams.itemAmount[caseTier];
        else
        {
            Debug.LogError("<color=red>MHP-290</color>: returnNewItemsAmount out of range: " + caseTier + "/" + casePrizesParams.itemAmount.Length);
            return 1; // defatult value to show something
        }
    }

    public List<ItemCasePrizeType> itemGambling(int caseTier, GameState.DrawShovelSource drawShovelSource = GameState.DrawShovelSource.Standard, int amount = -1)
    {
        List<ItemCasePrizeType> itemList = new List<ItemCasePrizeType>();
        int itemAmount;
        if (casePrizesParams.itemAmount.Length > caseTier)
            itemAmount = casePrizesParams.itemAmount[caseTier];
        else
        {
            Debug.LogError("<color=red>MHP-290</color>: ItemGambling out of range: " + caseTier + "/" + casePrizesParams.itemAmount.Length);
            itemAmount = 1;
        }

        for (int i = 0; i < ((amount > -1) ? amount : itemAmount/*casePrizesParams.itemAmount[caseTier]*/); ++i)
        {
            MedicineRef item = GameState.Get().GetRandomSpecial(drawShovelSource);//ResourcesHolder.Get().medicines.cures[15].medicines[GameState.RandomNumber(0, ResourcesHolder.Get().medicines.cures[15].medicines.Count - 1)].GetMedicineRef();
            if (ResourcesHolder.Get().medicines.cures[(int)item.type].medicines[item.id].Name == "SPECIAL_ITEMS/POSITIVE_ENERGY")
            { //najlepiej wywalić positive energy ze special items ale może to coś popsuć
                i--;
            }
            else if (ResourcesHolder.Get().medicines.cures[(int)item.type].medicines[item.id].Name == "SPECIAL_ITEMS/SHOVEL" && Game.Instance.gameState().GetHospitalLevel() < ResourcesHolder.Get().medicines.cures[(int)item.type].medicines[item.id].minimumLevel)
            {
                i--;
            }
            else
            {
                int index = itemList.FindIndex(x => x.item.id == item.id);
                if (index == -1)
                {
                    ItemCasePrizeType mItemPrize = new ItemCasePrizeType(item, 1);
                    itemList.Add(mItemPrize);
                }
                else
                {
                    itemList[index].amount++;
                }
            }
        }

        return itemList;
    }

    private BaseGameState.DrawShovelSource GetShovelSource(int caseTier, CaseType type = CaseType.ordinary)
    {
        switch (type)
        {
            case CaseType.VIP:
                return BaseGameState.DrawShovelSource.VIPBox;
            case CaseType.TREASURE:
                return BaseGameState.DrawShovelSource.TreasureChest;
            case CaseType.EPIDEMY:
                return BaseGameState.DrawShovelSource.EpidemyBox;
            case CaseType.ordinary:
                if (caseTier == 0)
                    return BaseGameState.DrawShovelSource.GoodieBox1;
                if (caseTier == 1)
                    return BaseGameState.DrawShovelSource.GoodieBox2;
                if (caseTier == 2)
                    return BaseGameState.DrawShovelSource.GoodieBox3;
                return BaseGameState.DrawShovelSource.Standard;
            default:
                return BaseGameState.DrawShovelSource.Standard;
        }
    }

    public List<DecorationCasePrizeType> decorationGambling(int caseTier, int amount = -1)
    {
        List<DecorationCasePrizeType> decorationList = new List<DecorationCasePrizeType>();
        List<ShopRoomInfo> infos = new List<ShopRoomInfo>();
        for (int i = 0; i < decorationsToDraw.Count; ++i)
        {
            if (!(decorationsToDraw[i].unlockLVL > Game.Instance.gameState().GetHospitalLevel()) && decorationsToDraw[i].GetType() == typeof(DecorationInfo)/* && (decorationsToDraw[i].costInDiamonds == 0)*/)
            {
                infos.Add(decorationsToDraw[i]);
            }
        }

        int itemAmount;
        if (casePrizesParams.itemAmount.Length > caseTier)
            itemAmount = casePrizesParams.itemAmount[caseTier];
        else
        {
            Debug.LogError("<color=red>MHP-290</color>: DecorationGambling out of range: " + caseTier + "/" + casePrizesParams.itemAmount.Length);
            itemAmount = 1;
        }

        for (int i = 0; i < ((amount > -1) ? amount : itemAmount/*casePrizesParams.itemAmount[caseTier]*/); ++i)
        {
            ShopRoomInfo info = infos[BaseGameState.RandomNumber(0, infos.Count - 1)];

            int index = decorationList.FindIndex(x => x.decoration.name == info.name);
            if (index == -1)
            {
                DecorationCasePrizeType mItemPrize = new DecorationCasePrizeType(info, 1);
                decorationList.Add(mItemPrize);
            }
            else
            {
                decorationList[index].amount++;
            }
        }

        return decorationList;
    }

    private void GenerateDefaultPrize(CasePrize casePrize)
    {

    }
    #endregion


    #region LootBoxIAPCaseData
    #endregion

}
