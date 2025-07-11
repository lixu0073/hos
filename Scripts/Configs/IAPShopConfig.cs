using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

public static class IAPShopConfig
{
    public static Dictionary<IAPShopSection, int> sections = new Dictionary<IAPShopSection, int>();
    public static Dictionary<IAPShopBundleID, IAPShopBundleData> bundles = new Dictionary<IAPShopBundleID, IAPShopBundleData>();
    public static Dictionary<IAPShopCoinPackageID, IAPShopCoinPackageData> coinPackages = new Dictionary<IAPShopCoinPackageID, IAPShopCoinPackageData>();

    public static void InitializeSections(IAPShopOrderCData iAPShopOrderCData)
    {
        sections.Clear();
        foreach (KeyValuePair<string, int> pair in iAPShopOrderCData.parameters)
        {
            try
            {
                IAPShopSection section = (IAPShopSection)Enum.Parse(typeof(IAPShopSection), pair.Key);
                int order = pair.Value;
                if (sections.ContainsKey(section))
                {
                    sections[section] = order;
                }
                else
                {
                    sections.Add(section, order);
                }
            }
            catch (ArgumentException e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    public static void InitializeBundles(IAPShopBundlesCData iAPShopBundlesCData)
    {
        bundles.Clear();
        foreach (KeyValuePair<string, string> pair in iAPShopBundlesCData.parameters)
        {
            try
            {
                IAPShopBundleID bundle = (IAPShopBundleID)Enum.Parse(typeof(IAPShopBundleID), pair.Key);
                string unparsedData = pair.Value;
                IAPShopBundleData data = IAPShopBundleData.Parse(unparsedData, bundle);
                if (data == null)
                    break;
                if (bundles.ContainsKey(bundle))
                {
                    bundles[bundle] = data;
                }
                else
                {
                    bundles.Add(bundle, data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                continue;
            }
        }
    }

    public static void InitializeCoinsPackages(IAPShopCoinsCData iAPShopCoinsCData)
    {
        coinPackages.Clear();
        foreach (KeyValuePair<string, string> pair in iAPShopCoinsCData.parameters)
        {
            try
            {
                IAPShopCoinPackageID bundle = (IAPShopCoinPackageID)Enum.Parse(typeof(IAPShopCoinPackageID), pair.Key);
                string unparsedData = pair.Value;
                IAPShopCoinPackageData data = IAPShopCoinPackageData.Parse(unparsedData, bundle);
                if (data == null)
                    break;
                if (coinPackages.ContainsKey(bundle))
                {
                    coinPackages[bundle] = data;
                }
                else
                {
                    coinPackages.Add(bundle, data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                continue;
            }
        }
    }
}

public class IAPShopBundleData
{
    public IAPShopBundleID ID { get; private set; }
    public IAPShopBundleColor color { get; private set; }
    public int orderInFeaturesSection { get; private set; } // -1 hide in section
    public int orderInSpecialOffersSection { get; private set; }  // -1 hide in section
    public int costInDiamonds { get; private set; }
    public string iapProductId { get; private set; }
    public string decisionPoint { get; private set; }

    public IAPShopBundleData(IAPShopBundleID ID, int orderInFeaturesSection, int orderInSpecialOffersSection, int costInDiamonds, string iapProductId = null, string decisionPoint = null, IAPShopBundleColor color = IAPShopBundleColor.none)
    {
        this.ID = ID;
        this.orderInFeaturesSection = orderInFeaturesSection;
        this.orderInSpecialOffersSection = orderInSpecialOffersSection;
        this.costInDiamonds = costInDiamonds;
        this.color = color;
        this.iapProductId = iapProductId;
        this.decisionPoint = decisionPoint;
    }

    public bool IsCreative()
    {
        return !string.IsNullOrEmpty(iapProductId) && !string.IsNullOrEmpty(decisionPoint);
    }

    private static char SEPARATOR = ';';

    private static int MIN_UNPARSED_DATA_ARRAY_LENGTH = 3;
    public enum INDEX
    {
        orderInFeatureSection = 0,
        orderInSpecialSection = 1,
        costInDiamonds = 2,
        backgroundColor = 3,
        iapProductID = 4,
        decisionPoint = 5
    }

    public static IAPShopBundleData Parse(string unparsedData, IAPShopBundleID ID)
    {
        string[] array = unparsedData.Split(SEPARATOR);
        if (array.Length < MIN_UNPARSED_DATA_ARRAY_LENGTH)
        {
            Debug.LogError("Incomplete Data");
            return null;
        }
        IAPShopBundleColor c = IAPShopBundleColor.none;
        if (array.Length > (int)INDEX.backgroundColor)
        {
            try
            {
                c = (IAPShopBundleColor)Enum.Parse(typeof(IAPShopBundleColor), array[(int)INDEX.backgroundColor]);
            }
            catch (Exception e)
            {
                Debug.LogError("color: " + e.Message);
            }
        }
        string iapProductId = null;
        if (array.Length > (int)INDEX.iapProductID)
        {
            try
            {
                iapProductId = array[(int)INDEX.iapProductID];
            }
            catch (Exception e)
            {
                Debug.LogError("iapProductId: " + e.Message);
            }
        }
        string decisionPoint = null;
        if (array.Length > (int)INDEX.decisionPoint)
        {
            try
            {
                decisionPoint = array[(int)INDEX.decisionPoint];
            }
            catch (Exception e)
            {
                Debug.LogError("decisionPoint: " + e.Message);
            }
        }
        return new IAPShopBundleData(ID, int.Parse(array[(int)INDEX.orderInFeatureSection], System.Globalization.CultureInfo.InvariantCulture), int.Parse(array[(int)INDEX.orderInSpecialSection], System.Globalization.CultureInfo.InvariantCulture), int.Parse(array[(int)INDEX.costInDiamonds], System.Globalization.CultureInfo.InvariantCulture), iapProductId, decisionPoint, c);
    }
}

public class IAPShopCoinPackageData
{
    public IAPShopCoinPackageID ID { get; private set; }
    public int order { get; private set; } // -1 hide in section
    public int costInDiamonds { get; private set; }
    public float multiplier { get; private set; }
    public string iapProductId { get; private set; }
    public string decisionPoint { get; private set; }

    public IAPShopCoinPackageData(IAPShopCoinPackageID ID, int order, int costInDiamonds, float multiplier, string iapProductId, string decisionPoint)
    {
        this.ID = ID;
        this.order = order;
        this.multiplier = multiplier;
        this.costInDiamonds = costInDiamonds;
        this.iapProductId = iapProductId;
        this.decisionPoint = decisionPoint;
    }

    public static IAPShopCoinPackageData Parse(string unparsedData, IAPShopCoinPackageID ID)
    {
        string[] array = unparsedData.Split(';');
        if (array.Length < 3)
        {
            Debug.LogError("Incomplete Data");
            return null;
        }
        string iapProductID = String.Empty;
        string decisionPoint = String.Empty;
        if (array.Length > 3 && !String.IsNullOrEmpty(array[3]))
        {
            iapProductID = array[3];
        }
        if (array.Length > 4 && !String.IsNullOrEmpty(array[4]))
        {
            decisionPoint = array[4];
        }

        return new IAPShopCoinPackageData(ID, int.Parse(array[0]), int.Parse(array[1]), float.Parse(array[2], CultureInfo.InvariantCulture), iapProductID, decisionPoint);
    }

    public bool IsCreative()
    {
        return !string.IsNullOrEmpty(decisionPoint); //!string.IsNullOrEmpty(iapProductId) && !string.IsNullOrEmpty(decisionPoint);
    }

}

public enum IAPShopBundleColor
{
    none,
    yellow,
    green,
    red
}

public enum IAPShopCoinPackageID
{
    packOfCoinsForVideo,
    packOfCoins1,
    packOfCoins2,
    packOfCoins3,
    packOfCoins4,
    packOfCoins5,
    packOfCoins6
}

public enum IAPShopBundleID
{
    bundleBreastCancerDeal = 0,
    bundlePositiveEnergy50 = 1,
    bundleShovels9 = 2,
    bundleSpecialPack = 3,
    bundleSuperBundle4 = 4,
    bundleTapjoy = 5,   // Obsolete.
    bundleCreative1 = 6,
    bundleCreative2 = 7,
    bundleCreative3 = 8,
    bundleCreative4 = 9,
    bundleCreative5 = 10,
    bundleCreative6 = 11,
    bundleCreative7 = 12,
    bundleCreative8 = 13,
    bundleCreative9 = 14,
    bundleCreative10 = 15,
}

public enum IAPShopSection
{
    Default,
    sectionDiamonds,
    sectionCoins,
    sectionFeatures,
    sectionSpecialOffers
}
