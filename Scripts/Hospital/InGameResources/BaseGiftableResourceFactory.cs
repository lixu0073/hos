using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

public static class BaseGiftableResourceFactory
{
    public enum BaseResroucesType
    {
        coin,
        diamond,
        positiveEnergy,
        booster,
        medicine,
        decoration,
        bundle,
        exp,
    }

    public static BaseGiftableResource CreateCoinGiftableResource(int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource sourceOfGift)
    {
        return new SoftCurrencyGiftableResource(amount, spriteType, sourceOfGift);
    }

    public static BaseGiftableResource CreateExpGiftableResource(int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource sourceOfGift)
    {
        return new ExpGiftableResource(amount, spriteType, sourceOfGift);
    }

    public static BaseGiftableResource CreateDiamondGiftableResource(int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource sourceOfGift)
    {
        return new HardCurrencyGiftableResource(amount, spriteType, sourceOfGift);
    }

    public static BaseGiftableResource CreatePositiveEnergyGiftableResource(int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource sourceOfGift)
    {
        return new PositiveEnergyGiftableResource(amount, spriteType, sourceOfGift);
    }

    public static BaseGiftableResource CreateBoosterGiftableResource(int boosterID, int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource sourceOfGift)
    {
        return new BoosterResource(boosterID, amount, spriteType, sourceOfGift);
    }

    public static BaseGiftableResource CreateMedicineGiftableResource(MedicineRef medicine, int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource sourceOfGift)
    {
        return new MedicineResourceGiftableResource(medicine, amount, spriteType, sourceOfGift);
    }

    public static BaseGiftableResource CreateDecorationGiftableResource(ShopRoomInfo decoration, int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource sourceOfGift)
    {
        return new DecorationGiftableResource(decoration, amount, spriteType, sourceOfGift);
    }

    public static BaseGiftableResource CreateBundleGiftableResource(List<BaseGiftableResource> bundledGifts, int amount, EconomySource sourceOfGift, string GiftLocalizationKey, Hospital.BundledRewardTypes boxType, SingleBoxSpriteData.BundledResourceSprite boxSpriteType)
    {
        return new BundleGiftableResource(bundledGifts, amount, BaseResourceSpriteData.SpriteType.dynamic, sourceOfGift, GiftLocalizationKey, boxType, boxSpriteType);
    }

    public static BaseGiftableResource CreateDailyRewardGiftableFromString(string giftableString, int dayNo, EconomySource economySource)
    {
        return DailyRewardResourceParser.CreateGiftableFromString(giftableString, dayNo, economySource);
    }

    public static BaseGiftableResource CreateGiftableFromString(string giftableString, EconomySource economySource)
    {
        return BaseResourceParser.CreateGiftableFromString(giftableString, economySource);
    }
}

public static class BaseResourceParser
{
    private const char MAIN_SEPARATOR = ';';
    private const char INTERIA_SEPARATOR = '#';
    private const char BUNDLE_INTERIA_SEPARATOR = '@';
    private const char RANDOM_AMOUNT_SEPARATOR = '|';
    private const char RANDOM_ITEM_TYPE = 'R';
    private const string RANDOM_STANDARD_BOOSTER = "RSB";
    private const string RANDOM_PREMIUM_BOOSTER = "RPB";
    private const string RANDOM_STORAGE_TOOL = "RST";
    private const string RANDOM_TANK_TOOL = "RTT";
    private const string RANDOM_SPECIAL = "RS";
    private static readonly int[] standardBoosterIndexes = new int[7] { 0, 1, 2, 3, 4, 5, 6, };
    private static readonly int[] premiumBoosterIndexes = new int[2] { 7, 8 };
    private static readonly int[] storageToolIndexes = new int[3] { 0, 1, 2 };
    private static readonly int[] tankToolIndexes = new int[3] { 4, 5, 6 };
    private static readonly int[] specialToolIndexes = new int[7] { 0, 1, 2, 3, 4, 5, 6 };

    public static BaseGiftableResource CreateGiftableFromString(string giftableString, EconomySource economySource)
    {
        string[] mainTextData = giftableString.Split(MAIN_SEPARATOR);
        BaseGiftableResourceFactory.BaseResroucesType giftType;
        try
        {
            giftType = (BaseGiftableResourceFactory.BaseResroucesType)Enum.Parse(typeof(BaseGiftableResourceFactory.BaseResroucesType), mainTextData[0]);
        }
        catch (Exception)
        {
            Debug.LogError("Cannot parse type of giftable resource");
            return null;
        }
        if (mainTextData.Length > 1)
        {
            switch (giftType)
            {
                case BaseGiftableResourceFactory.BaseResroucesType.coin:
                    string[] coinInteriaData = mainTextData[1].Split(INTERIA_SEPARATOR);
                    if (coinInteriaData.Length > 0)
                    {
                        string[] randomAmountData = coinInteriaData[0].Split(RANDOM_AMOUNT_SEPARATOR);
                        if (randomAmountData.Length > 2)
                        {
                            int coinAmount = 0;
                            coinAmount = GetRandomAmountOfGift(randomAmountData);
                            if (coinAmount > 0)
                            {
                                BaseResourceSpriteData.SpriteType spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                if (coinInteriaData.Length > 1)
                                {
                                    try
                                    {
                                        spriteTyp = (BaseResourceSpriteData.SpriteType)Enum.Parse(typeof(BaseResourceSpriteData.SpriteType), coinInteriaData[1]);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError("Cannot parse sprite type. System will use dynamic. " + e.Message);
                                        Debug.LogError("coinInteriaData.Length: " + coinInteriaData.Length + " | SpriteType: " + typeof(BaseResourceSpriteData.SpriteType).ToString() +
                                                        "\\||// coinInteriaData[1]: " + coinInteriaData[1]);
                                        spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                    }
                                }
                                return BaseGiftableResourceFactory.CreateCoinGiftableResource(coinAmount, spriteTyp, economySource);
                            }
                        }
                        else
                        {
                            Debug.LogError("Not enought data about randomness for given gift. No gift is craeted");
                        }
                    }
                    break;
                case BaseGiftableResourceFactory.BaseResroucesType.exp:
                    string[] expInteriaData = mainTextData[1].Split(INTERIA_SEPARATOR);
                    if (expInteriaData.Length > 0)
                    {
                        string[] randomAmountData = expInteriaData[0].Split(RANDOM_AMOUNT_SEPARATOR);
                        if (randomAmountData.Length > 2)
                        {
                            int expAmount = 0;
                            expAmount = GetRandomAmountOfGift(randomAmountData);
                            if (expAmount > 0)
                            {
                                BaseResourceSpriteData.SpriteType spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                if (expInteriaData.Length > 1)
                                {
                                    try
                                    {
                                        spriteTyp = (BaseResourceSpriteData.SpriteType)Enum.Parse(typeof(BaseResourceSpriteData.SpriteType), expInteriaData[1]);
                                    }
                                    catch (Exception)
                                    {
                                        Debug.LogError("Cannot parse sprite type. System will use dynamic");
                                        spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                    }
                                }
                                return BaseGiftableResourceFactory.CreateExpGiftableResource(expAmount, spriteTyp, economySource);
                            }
                        }
                        else
                        {
                            Debug.LogError("Not enought data about randomness for given gift. No gift is craeted");
                        }
                    }
                    break;
                case BaseGiftableResourceFactory.BaseResroucesType.diamond:
                    string[] diamondInteriaData = mainTextData[1].Split(INTERIA_SEPARATOR);
                    if (diamondInteriaData.Length > 0)
                    {
                        string[] randomAmountData = diamondInteriaData[0].Split(RANDOM_AMOUNT_SEPARATOR);
                        if (randomAmountData.Length > 2)
                        {
                            int diamondAmount = 0;
                            diamondAmount = GetRandomAmountOfGift(randomAmountData);
                            if (diamondAmount > 0)
                            {
                                BaseResourceSpriteData.SpriteType spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                if (diamondInteriaData.Length > 1)
                                {
                                    try
                                    {
                                        spriteTyp = (BaseResourceSpriteData.SpriteType)Enum.Parse(typeof(BaseResourceSpriteData.SpriteType), diamondInteriaData[1]);
                                    }
                                    catch (Exception)
                                    {
                                        Debug.LogError("Cannot parse sprite type. System will use dynamic");
                                        spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                    }
                                }
                                return BaseGiftableResourceFactory.CreateDiamondGiftableResource(diamondAmount, spriteTyp, economySource);
                            }
                        }
                        else
                        {
                            Debug.LogError("Not enought data about randomness for given gift. No gift is craeted");
                        }
                    }
                    break;
                case BaseGiftableResourceFactory.BaseResroucesType.positiveEnergy:
                    string[] positiveInteriaData = mainTextData[1].Split(INTERIA_SEPARATOR);
                    if (positiveInteriaData.Length > 0)
                    {
                        string[] randomAmountData = positiveInteriaData[0].Split(RANDOM_AMOUNT_SEPARATOR);
                        if (randomAmountData.Length > 2)
                        {
                            int positiveEnergyAmount = 0;
                            positiveEnergyAmount = GetRandomAmountOfGift(randomAmountData);
                            if (positiveEnergyAmount > 0)
                            {
                                BaseResourceSpriteData.SpriteType spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                if (positiveInteriaData.Length > 1)
                                {
                                    try
                                    {
                                        spriteTyp = (BaseResourceSpriteData.SpriteType)Enum.Parse(typeof(BaseResourceSpriteData.SpriteType), positiveInteriaData[1]);
                                    }
                                    catch (Exception)
                                    {
                                        Debug.LogError("Cannot parse sprite type. System will use dynamic");
                                        spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                    }
                                }
                                return BaseGiftableResourceFactory.CreatePositiveEnergyGiftableResource(positiveEnergyAmount, spriteTyp, economySource);
                            }
                        }
                        else
                        {
                            Debug.LogError("Not enought data about randomness for given gift. No gift is craeted");
                        }
                    }
                    break;
                case BaseGiftableResourceFactory.BaseResroucesType.booster:
                    string[] boosterInteriaData = mainTextData[1].Split(INTERIA_SEPARATOR);
                    int boosterID = 0;
                    if (boosterInteriaData.Length > 0)
                    {
                        string[] randomAmountData = boosterInteriaData[0].Split(RANDOM_AMOUNT_SEPARATOR);
                        if (randomAmountData.Length > 2)
                        {
                            int boosterAmount = 0;
                            boosterAmount = GetRandomAmountOfGift(randomAmountData);
                            if (boosterAmount > 0)
                            {
                                if (boosterInteriaData.Length > 1)
                                {
                                    if (String.Equals(boosterInteriaData[1], RANDOM_STANDARD_BOOSTER, StringComparison.OrdinalIgnoreCase))
                                    {
                                        boosterID = standardBoosterIndexes[UnityEngine.Random.Range(0, standardBoosterIndexes.Length - 1)];
                                    }
                                    else if (String.Equals(boosterInteriaData[1], RANDOM_PREMIUM_BOOSTER, StringComparison.OrdinalIgnoreCase))
                                    {
                                        boosterID = premiumBoosterIndexes[UnityEngine.Random.Range(0, premiumBoosterIndexes.Length - 1)];
                                    }
                                    else
                                    {
                                        int.TryParse(boosterInteriaData[1], out boosterID);
                                    }
                                    BaseResourceSpriteData.SpriteType spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                    if (boosterInteriaData.Length > 2)
                                    {
                                        try
                                        {
                                            spriteTyp = (BaseResourceSpriteData.SpriteType)Enum.Parse(typeof(BaseResourceSpriteData.SpriteType), boosterInteriaData[2]);
                                        }
                                        catch (Exception)
                                        {
                                            Debug.LogError("Cannot parse sprite type. System will use dynamic");
                                            spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                                        }
                                        return BaseGiftableResourceFactory.CreateBoosterGiftableResource(boosterID, boosterAmount, spriteTyp, economySource);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError("Not enought data about randomness for given gift. No gift is craeted");
                        }
                    }
                    break;
                case BaseGiftableResourceFactory.BaseResroucesType.medicine:
                    string[] medicineInteriaData = mainTextData[1].Split(INTERIA_SEPARATOR);
                    MedicineRef medicine = null;
                    if (medicineInteriaData.Length > 0)
                    {
                        if (medicineInteriaData.Length > 1)
                        {
                            string[] randomAmountData = medicineInteriaData[0].Split(RANDOM_AMOUNT_SEPARATOR);
                            if (randomAmountData.Length > 2)
                            {
                                int medicineAmount = 0;
                                medicineAmount = GetRandomAmountOfGift(randomAmountData);
                                if (medicineAmount > 0)
                                {
                                    if (String.Equals(medicineInteriaData[1], RANDOM_ITEM_TYPE.ToString(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        List<Hospital.MedicineDatabaseEntry> unlockedMedicines = ResourcesHolder.Get().EnumerateKnownMedicinesForLvl(Game.Instance.gameState().GetHospitalLevel());
                                        for (int i = unlockedMedicines.Count - 1; i >= 0; i--)
                                        {
                                            if (unlockedMedicines[i].GetMedicineRef().type == MedicineType.Special)                                            
                                                unlockedMedicines.RemoveAt(i);
                                        }
                                        int randomIndex = UnityEngine.Random.Range(0, unlockedMedicines.Count - 1);
                                        medicine = unlockedMedicines[randomIndex].GetMedicineRef();
                                    }
                                    else if (String.Equals(medicineInteriaData[1], RANDOM_STORAGE_TOOL, StringComparison.OrdinalIgnoreCase))
                                    {
                                        medicine = GameState.Get().GetRandomSpecial(SpecialItemTarget.Storage);
                                        //List<Hospital.MedicineDatabaseEntry> unlockedMedicines = ResourcesHolder.Get().GetMedicinesOfType(MedicineType.Special);
                                        //int randomToolIndex = storageToolIndexes[UnityEngine.Random.Range(0, storageToolIndexes.Length - 1)];
                                        //medicine = unlockedMedicines[randomToolIndex].GetMedicineRef();
                                    }
                                    else if (String.Equals(medicineInteriaData[1], RANDOM_TANK_TOOL, StringComparison.OrdinalIgnoreCase))
                                    {
                                        medicine = GameState.Get().GetRandomSpecial(SpecialItemTarget.Tank);
                                        //List<Hospital.MedicineDatabaseEntry> unlockedMedicines = ResourcesHolder.Get().GetMedicinesOfType(MedicineType.Special);
                                        //int randomToolIndex = tankToolIndexes[UnityEngine.Random.Range(0, tankToolIndexes.Length - 1)];
                                        //medicine = unlockedMedicines[randomToolIndex].GetMedicineRef();
                                    }
                                    else if (String.Equals(medicineInteriaData[1], RANDOM_SPECIAL, StringComparison.OrdinalIgnoreCase))
                                    {
                                        medicine = GameState.Get().GetRandomSpecial(SpecialItemTarget.All);
                                    }
                                    else
                                    {
                                        string systemMedicineName = medicineInteriaData[1];
                                        if (!String.IsNullOrEmpty(systemMedicineName))
                                        {
                                            medicine = MedicineRef.Parse(systemMedicineName);
                                        }
                                    }
                                    if (medicine != null)
                                    {
                                        BaseResourceSpriteData.SpriteType spriteType = BaseResourceSpriteData.SpriteType.dynamic;
                                        if (medicineInteriaData.Length > 2)
                                        {
                                            try
                                            {
                                                spriteType = (BaseResourceSpriteData.SpriteType)Enum.Parse(typeof(BaseResourceSpriteData.SpriteType), medicineInteriaData[2]);
                                            }
                                            catch (Exception)
                                            {
                                                Debug.LogError("Cannot parse sprite type. System will use dynamic");
                                                spriteType = BaseResourceSpriteData.SpriteType.dynamic;
                                            }
                                            return BaseGiftableResourceFactory.CreateMedicineGiftableResource(medicine, medicineAmount, spriteType, economySource);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogError("Not enought data about randomness for given gift. No gift is craeted");
                            }
                        }
                    }
                    break;
                case BaseGiftableResourceFactory.BaseResroucesType.decoration:
                    string[] decorationInteriaData = mainTextData[1].Split(INTERIA_SEPARATOR);
                    ShopRoomInfo decoration = null;
                    if (decorationInteriaData.Length > 0)
                    {
                        string[] randomAmountData = decorationInteriaData[0].Split(RANDOM_AMOUNT_SEPARATOR);
                        if (randomAmountData.Length > 2)
                        {
                            int decorationAmount = 0;
                            decorationAmount = GetRandomAmountOfGift(randomAmountData);
                            if (decorationAmount > 0)
                            {
                                if (decorationInteriaData.Length > 1)
                                {
                                    if (String.Equals(decorationInteriaData[1], RANDOM_ITEM_TYPE.ToString(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        int randomIndex = UnityEngine.Random.Range(0, Hospital.AreaMapController.Map.casesManager.decorationsToDraw.Count - 1);
                                        decoration = Hospital.AreaMapController.Map.casesManager.decorationsToDraw[randomIndex];
                                    }
                                    else
                                    {
                                        string systemDecorationName = decorationInteriaData[1];
                                        decoration = Hospital.AreaMapController.Map.drawerDatabase.DrawerItems.Find(x => x.Tag == systemDecorationName);
                                    }
                                    if (decoration != null)
                                    {

                                        BaseResourceSpriteData.SpriteType spriteType = BaseResourceSpriteData.SpriteType.dynamic;
                                        if (decorationInteriaData.Length > 1)
                                        {
                                            try
                                            {
                                                spriteType = (BaseResourceSpriteData.SpriteType)Enum.Parse(typeof(BaseResourceSpriteData.SpriteType), decorationInteriaData[2]);
                                            }
                                            catch (Exception)
                                            {
                                                Debug.LogError("Cannot parse sprite type. System will use dynamic");
                                                spriteType = BaseResourceSpriteData.SpriteType.dynamic;
                                            }
                                        }
                                        return BaseGiftableResourceFactory.CreateDecorationGiftableResource(decoration, decorationAmount, spriteType, economySource);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError("Not enought data about randomness for given gift. No gift is craeted");
                        }
                    }
                    break;
                case BaseGiftableResourceFactory.BaseResroucesType.bundle:
                    Hospital.BundledRewardTypes bundleRewardType = Hospital.BundledRewardTypes.none;
                    string[] partialUnparsedBundleRewardData = giftableString.Split(BUNDLE_INTERIA_SEPARATOR);
                    string unparsedBoxType = partialUnparsedBundleRewardData[1];
                    try
                    {
                        bundleRewardType = (Hospital.BundledRewardTypes)Enum.Parse(typeof(Hospital.BundledRewardTypes), unparsedBoxType);
                    }
                    catch (Exception)
                    {
                        Debug.LogError("Type of box not found");
                    }
                    if (bundleRewardType != Hospital.BundledRewardTypes.none)
                    {
                        string fullBundleUnparsedString = partialUnparsedBundleRewardData[0] + BUNDLE_INTERIA_SEPARATOR + Hospital.BundledRewardDefinitionConfig.RecoverRemainingBundleDefinition(bundleRewardType);
                        string[] bundleInteriaData = fullBundleUnparsedString.Split(BUNDLE_INTERIA_SEPARATOR);
                        string locKey = "";
                        SingleBoxSpriteData.BundledResourceSprite boxSpriteType = SingleBoxSpriteData.BundledResourceSprite.none;
                        if (bundleInteriaData.Length > 0)
                        {
                            string[] randomAmountData = bundleInteriaData[0].Split(MAIN_SEPARATOR)[1].Split(RANDOM_AMOUNT_SEPARATOR);
                            if (randomAmountData.Length > 2)
                            {
                                int amount = 0;
                                amount = GetRandomAmountOfGift(randomAmountData);
                                if (amount > 0)
                                {
                                    if (bundleInteriaData.Length > 1)
                                    {
                                        locKey = bundleInteriaData[1];
                                        if (bundleInteriaData.Length > 2)
                                        {
                                            try
                                            {
                                                boxSpriteType = (SingleBoxSpriteData.BundledResourceSprite)Enum.Parse(typeof(SingleBoxSpriteData.BundledResourceSprite), bundleInteriaData[2]);
                                            }
                                            catch (Exception)
                                            {
                                                Debug.LogError("Wrong box type.");
                                                boxSpriteType = SingleBoxSpriteData.BundledResourceSprite.dailyRewardBoxWeek1;
                                            }
                                        }
                                    }
                                    List<BaseGiftableResource> bundledGifts = new List<BaseGiftableResource>();
                                    for (int i = 3; i < bundleInteriaData.Length; i++)
                                    {
                                        BaseGiftableResource gift = CreateGiftableFromString(bundleInteriaData[i], economySource);                                        

                                        if (gift != null)
                                        {
                                            BaseGiftableResource duplicatedGift = bundledGifts.Find(x => x.IsSameAs(gift));
                                            if (duplicatedGift != null)
                                            {
                                                duplicatedGift.SetGiftAmount(duplicatedGift.GetGiftAmount() + gift.GetGiftAmount());
                                            }
                                            else
                                            {
                                                bundledGifts.Add(gift);
                                            }
                                        }
                                    }
                                    if (boxSpriteType != SingleBoxSpriteData.BundledResourceSprite.none)
                                    {
                                        return BaseGiftableResourceFactory.CreateBundleGiftableResource(bundledGifts, amount, economySource, locKey, bundleRewardType, boxSpriteType);
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogError("Not enought data about randomness for given gift. No gift is craeted");
                            }
                        }
                    }

                    break;
                default:
                    Debug.LogError("No resource has been created. There is no resource type");
                    return null;
            }
        }
        return null;
    }

    public static string GiftableToString(this BaseGiftableResource giftableResource)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(giftableResource.rewardType);
        RandomAmountDataToString(giftableResource, sb);
        switch (giftableResource.rewardType)
        {
            case BaseGiftableResourceFactory.BaseResroucesType.coin:
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableResource.GetSpriteType().ToString());
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.exp:
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableResource.GetSpriteType().ToString());
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.diamond:
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableResource.GetSpriteType().ToString());
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.positiveEnergy:
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableResource.GetSpriteType().ToString());
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.booster:
                BoosterResource giftableAsBooster = giftableResource as BoosterResource;
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableAsBooster.GetBoosterID());
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableResource.GetSpriteType().ToString());
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.medicine:
                MedicineResourceGiftableResource giftableAsMedicine = giftableResource as MedicineResourceGiftableResource;
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableAsMedicine.GetMedicine().ToString());
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableResource.GetSpriteType().ToString());
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.decoration:
                DecorationGiftableResource giftableAsDeco = giftableResource as DecorationGiftableResource;
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableAsDeco.GetDecoInfo().Tag);
                sb.Append(INTERIA_SEPARATOR);
                sb.Append(giftableResource.GetSpriteType().ToString());
                break;
            case BaseGiftableResourceFactory.BaseResroucesType.bundle:
                BundleGiftableResource giftAsBundle = giftableResource as BundleGiftableResource;
                sb.Append(BUNDLE_INTERIA_SEPARATOR);
                sb.Append(giftAsBundle.GetGiftBoxType().ToString());
                break;

            default:
                break;
        }
        return sb.ToString();
    }

    private static void RandomAmountDataToString(BaseGiftableResource giftableResource, StringBuilder sb)
    {
        sb.Append(MAIN_SEPARATOR);
        sb.Append(String.Empty);
        sb.Append(RANDOM_AMOUNT_SEPARATOR);
        sb.Append(string.Empty);
        sb.Append(RANDOM_AMOUNT_SEPARATOR);
        sb.Append(giftableResource.GetGiftAmount());
    }

    private static int GetRandomAmountOfGift(string[] randomAmountData)
    {
        int giftAmount = 0;
        if (!String.IsNullOrEmpty(randomAmountData[2]) && int.TryParse(randomAmountData[2], out giftAmount))
            return giftAmount;
        else
        {
            int minRandom = 0;
            int maxRandom = 0;
            float chanceToHave = 0.0f;
            int.TryParse(randomAmountData[0], out minRandom);
            int.TryParse(randomAmountData[1], out maxRandom);
            float.TryParse(randomAmountData[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out chanceToHave);
            float random = UnityEngine.Random.Range(0.0f, 1.0f);
            if (random <= chanceToHave)            
                giftAmount = BaseGameState.RandomNumber(minRandom, maxRandom + 1);

            return giftAmount;
        }
    }
}

public static class DailyRewardResourceParser
{
    private const char MAIN_SEPARATOR = ';';
    private const char INTERIA_SEPARATOR = '#';
    private const char BUNDLE_INTERIA_SEPARATOR = '@';
    private const char RANDOM_AMOUNT_SEPARATOR = '|';
    private const char RANDOM_ITEM_TYPE = 'R';
    private const string RANDOM_STANDARD_BOOSTER = "RSB";
    private const string RANDOM_PREMIUM_BOOSTER = "RPB";
    private const string RANDOM_STORAGE_TOOL = "RST";
    private const string RANDOM_TANK_TOOL = "RTT";
    private const string RANDOM_SPECIAL = "RS";
    private static readonly int[] standardBoosterIndexes = new int[7] { 0, 1, 2, 3, 4, 5, 6, };
    private static readonly int[] premiumBoosterIndexes = new int[2] { 7, 8 };
    private static readonly int[] storageToolIndexes = new int[3] { 0, 1, 2 };
    private static readonly int[] tankToolIndexes = new int[3] { 4, 5, 6 };
    private static readonly int[] specialToolIndexes = new int[7] { 0, 1, 2, 3, 4, 5, 6 };

    public static BaseGiftableResource CreateGiftableFromString(string giftableString, int dayNo, EconomySource economySource)
    {
        string[] mainTextData = giftableString.Split(MAIN_SEPARATOR);
        BaseGiftableResourceFactory.BaseResroucesType giftType;
        try
        {
            giftType = (BaseGiftableResourceFactory.BaseResroucesType)Enum.Parse(typeof(BaseGiftableResourceFactory.BaseResroucesType), mainTextData[0]);
        }
        catch (Exception)
        {
            Debug.LogError("Cannot parse type of giftable resource");
            return null;
        }
        if (mainTextData.Length > 1)
        {
            switch (giftType)
            {
                case BaseGiftableResourceFactory.BaseResroucesType.coin:
                    int coinAmount = 0;
                    coinAmount = GetAmountOfCoins(dayNo);
                    string[] coinInteriaData = mainTextData[1].Split(INTERIA_SEPARATOR);
                    if (coinInteriaData.Length > 1)
                    {
                        if (coinAmount > 0)
                        {
                            BaseResourceSpriteData.SpriteType spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                            try
                            {
                                spriteTyp = (BaseResourceSpriteData.SpriteType)Enum.Parse(typeof(BaseResourceSpriteData.SpriteType), coinInteriaData[1]);
                            }
                            catch (Exception)
                            {
                                Debug.LogError("Cannot parse sprite type. System will use dynamic");
                                spriteTyp = BaseResourceSpriteData.SpriteType.dynamic;
                            }
                            return BaseGiftableResourceFactory.CreateCoinGiftableResource(coinAmount, spriteTyp, economySource);
                        }
                        else
                        {
                            Debug.LogError("Not enought data about randomness for given gift. No gift is craeted");
                        }
                    }
                    break;
                case BaseGiftableResourceFactory.BaseResroucesType.medicine:
                    string[] medicineInteriaData = mainTextData[1].Split(INTERIA_SEPARATOR);
                    MedicineRef medicine = null;
                    if (medicineInteriaData.Length > 1)
                    {
                        if (String.Equals(medicineInteriaData[1], RANDOM_ITEM_TYPE.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            string[] amountData = medicineInteriaData[0].Split(RANDOM_AMOUNT_SEPARATOR);
                            if (amountData.Length > 3)
                            {
                                int minMedicinePrice = int.Parse(amountData[0]);
                                int maxMedicinesPrice = int.Parse(amountData[1]);
                                int maxMedicinesAmount = int.Parse(amountData[3]);

                                List<Hospital.MedicineDatabaseEntry> unlockedMedicines = ResourcesHolder.Get().EnumerateKnownMedicinesForLvl(Game.Instance.gameState().GetHospitalLevel());
                                for (int i = unlockedMedicines.Count - 1; i >= 0; i--)
                                {
                                    if (unlockedMedicines[i].GetMedicineRef().type == MedicineType.Special || unlockedMedicines[i].diamondPrice < minMedicinePrice || unlockedMedicines[i].diamondPrice > maxMedicinesPrice)
                                    {
                                        unlockedMedicines.RemoveAt(i);
                                    }
                                }

                                int randomIndex = UnityEngine.Random.Range(0, unlockedMedicines.Count - 1);
                                medicine = unlockedMedicines[randomIndex].GetMedicineRef();

                                if (medicine != null)
                                {
                                    int medicineAmount = Mathf.Min(maxMedicinesPrice/unlockedMedicines[randomIndex].diamondPrice, maxMedicinesAmount);
                                                                        
                                    BaseResourceSpriteData.SpriteType spriteType = BaseResourceSpriteData.SpriteType.dynamic;
                                    if (medicineInteriaData.Length > 2)
                                    {
                                        try
                                        {
                                            spriteType = (BaseResourceSpriteData.SpriteType)Enum.Parse(typeof(BaseResourceSpriteData.SpriteType), medicineInteriaData[2]);
                                        }
                                        catch (Exception)
                                        {
                                            Debug.LogError("Cannot parse sprite type. System will use dynamic");
                                            spriteType = BaseResourceSpriteData.SpriteType.dynamic;
                                        }
                                        return BaseGiftableResourceFactory.CreateMedicineGiftableResource(medicine, medicineAmount, spriteType, economySource);
                                    }
                                }
                            }
                        }
                        else
                        {
                            return BaseResourceParser.CreateGiftableFromString(giftableString, economySource);
                        }
                    }
                    break;
                default:
                    return BaseResourceParser.CreateGiftableFromString(giftableString, economySource);
            }
        }
        return null;
    }

    public static int GetAmountOfCoins(int dayNo)
    {
        int playerLevel = Game.Instance.gameState().GetHospitalLevel();
        float baseAmountThisLevel = Mathf.Log10(playerLevel + 3) * (playerLevel * playerLevel * 0.48f) + 180;
        return Mathf.RoundToInt(baseAmountThisLevel * DailyRewardParser.incrementValue[dayNo]);
    }
}
