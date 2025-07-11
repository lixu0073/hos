using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using SimpleUI;
using Facebook.Unity;
using AppsFlyerSDK;

/// <summary>
/// 应用内购买控制器，负责管理游戏中所有的IAP功能。
/// 包括商品初始化、购买处理、收据验证、价格获取等核心IAP操作。
/// 支持多种商品类型：货币包、钻石包、特殊礼包等。
/// </summary>
public class IAPController : MonoBehaviour, IStoreListener, IAppsFlyerValidateReceipt
{
    public static IAPController instance;
    public static bool isTimedOffer;
    public static bool isPauseBlocked;

    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
    List<UnityEngine.Purchasing.Product> pendingProducts;
    public Sprite diamondsSprite;
    public Sprite coinsSprite;
    public Sprite positiveSprite;

    Coroutine _saveAfterIAP;

    // Product identifiers for all products capable of being purchased: 
    // "convenience" general identifiers for use with Purchasing, and their store-specific identifier 
    // counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers 
    // also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

    // General product identifiers for the consumable, non-consumable, and subscription products.
    // Use these handles in the code to reference which product to purchase. Also use these values 
    // when defining the Product Identifiers on the store. Except, for illustration purposes, the 
    // kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
    // specific mapping to Unity Purchasing's AddProduct, below.

    static readonly string iapCoinPack1 = "coins1";
    static readonly string iapCoinPack2 = "coins2";
    static readonly string iapCoinPack3 = "coins3";
    static readonly string iapCoinPack4 = "coins4";
    static readonly string iapCoinPack5 = "coins5";
    static readonly string iapCoinPack6 = "coins6";
    static readonly string iapCoinPack2_33off = "coins2_33off";
    static readonly string iapCoinPack5_50off = "coins5_50off";
    static readonly string iapCoinPack5_90off = "coins5_90off";
    static readonly string iapCoinPack6_50off = "coins6_50off";

    static readonly string iapDiamondPack1 = "diamonds1";
    static readonly string iapDiamondPack2 = "diamonds2";
    static readonly string iapDiamondPack3 = "diamonds3";
    static readonly string iapDiamondPack4 = "diamonds4";
    static readonly string iapDiamondPack5 = "diamonds5";
    static readonly string iapDiamondPack6 = "diamonds6";
    static readonly string iapDiamondPack1_50off = "diamonds1_50off";   //deprecated
    static readonly string iapDiamondPack1_33off = "diamonds1_33off";
    static readonly string iapDiamondPack1_67off = "diamonds1_67off";
    static readonly string iapDiamondPack2_20off = "diamonds2_20off";
    static readonly string iapDiamondPack2_40off = "diamonds2_40off";
    static readonly string iapDiamondPack2_60off = "diamonds2_60off";
    static readonly string iapDiamondPack2_80off = "diamonds2_80off";
    static readonly string iapDiamondPack3_20off = "diamonds3_20off";
    static readonly string iapDiamondPack3_30off = "diamonds3_30off";
    static readonly string iapDiamondPack3_50off = "diamonds3_50off";
    static readonly string iapDiamondPack3_70off = "diamonds3_70off";
    static readonly string iapDiamondPack3_80off = "diamonds3_80off";
    static readonly string iapDiamondPack3_90off = "diamonds3_90off";
    static readonly string iapDiamondPack4_20off = "diamonds4_20off";
    static readonly string iapDiamondPack4_25off = "diamonds4_25off";
    static readonly string iapDiamondPack4_30off = "diamonds4_30off";
    static readonly string iapDiamondPack4_50off = "diamonds4_50off";
    static readonly string iapDiamondPack4_75off = "diamonds4_75off";
    static readonly string iapDiamondPack4_90off = "diamonds4_90off";
    static readonly string iapDiamondPack5_20off = "diamonds5_20off";
    static readonly string iapDiamondPack5_25off = "diamonds5_25off";
    static readonly string iapDiamondPack5_30off = "diamonds5_30off";
    static readonly string iapDiamondPack5_50off = "diamonds5_50off";
    static readonly string iapDiamondPack5_75off = "diamonds5_75off";
    static readonly string iapDiamondPack5_90off = "diamonds5_90off";
    static readonly string iapDiamondPack6_20off = "diamonds6_20off";
    static readonly string iapDiamondPack6_25off = "diamonds6_25off";
    static readonly string iapDiamondPack6_30off = "diamonds6_30off";
    static readonly string iapDiamondPack6_50off = "diamonds6_50off";
    static readonly string iapDiamondPack6_75off = "diamonds6_75off";
    static readonly string iapDiamondPack6_90off = "diamonds6_90off";
    static readonly string iapDiamondPack5_65off = "diamonds5_65off";

    static readonly string iapSpecialPack1 = "specialpack_1";     //aka StarterPack: iapCoinPack1 + iapDiamondPack2 + decoration(Golden Statue)
    static readonly string iapSpecialPack2 = "specialpack_2";     //aka StarterPack: iapCoinPack3 + iapDiamondPack2
    static readonly string iapBundlePack1 = "bundlepack_1";       //iapCoinPack2 + 50 diamonds + 65 positive energy + booster happy hour
    static readonly string iapBundlePack2 = "bundlepack_2";       //iapCoinPack3 + iapDiamondPack3 + 65 positive energy + booster 6h power boost
    static readonly string iapBundlePack3 = "bundlepack_3";       //iapDiamondPack4 + 285 positive energy + booster 6h power boost
    static readonly string iapBundlePack4 = "bundlepack_4";       //iapDiamondPack3 + 137 positive energy
    static readonly string iapValentines1 = "valentines_1";       //150 diamonds + decoration(Amor)
    static readonly string iapEaster1 = "easter_1";               //100 diamonds + decoration(Bloom Tree)
    static readonly string iapLabourDay1 = "Labor_Day_Special_Offer";         //to do description
    static readonly string iapSuperBundle1 = "super_bundle_1";    //20 positive, 5 shovels, 500 coins for 65 diamonds (old: 20 positive + 2 shovels for 35 diamonds)
    static readonly string iapSuperBundle2 = "super_bundle_2";    //12 sholves + 2 screwdrives + 2 spanners + 2 hammers + 2 planks + 2 pipes + 2 washers for 160 diamonds (old: 100 positive + 10 shovels for 100 diamonds)
    static readonly string iapSuperBundle3 = "super_bundle_3";    //20 screwdrives + 20 spanners + 20 hammers + 20 planks + 20 pipes + 20 washers
    static readonly string iapSuperBundle4 = "super_bundle_4";    //200 positive + 25 shoves + 2000 coins
    static readonly string iapSuperBundle5 = "super_bundle_5";    //300 positive + zen fountain + claw machine + 10000 coins
    static readonly string iapBBQBundle1 = "bbq_bundle_offer_1";         //2 decos (grill and bbq) + 150 hearts + ?
    static readonly string iapHalloween1 = "halloween_bundle_offer_1"; // 1.1.38
    static readonly string iapPinkRibbonDeco1 = "pink_ribbon_deco_1";  // 1.1.38
    static readonly string iapWHOBundle1 = "who_bundle_offer_1"; //who_decoration + 275 diamonds
    static readonly string iapFireWorksBundle1 = "fireworks_bundle";
    static readonly string iapValentines2 = "valentines_2";
    static readonly string iapValentines3 = "valentines_3";

    static readonly string iapSuperBundle3_50off = "super_bundle_3_50off";          // 20 Screwdrivers + 20 Spanners + 20 Hammers + 20 Planks + 20 Pipes + 20 Washers
    static readonly string iapSuperBundle6 = "super_bundle_6";                      // 5 Screwdrivers + 5 Spanners + 5 Hammers + 5 Planks + 5 Pipes + 5 Washers
    static readonly string iapSuperBundle6_50off = "super_bundle_6_50off";          // 5 Screwdrivers + 5 Spanners + 5 Hammers + 5 Planks + 5 Pipes + 5 Washers
    static readonly string iapSuperBundle7 = "super_bundle_7";		                // 10 Screwdrivers + 10 Hammers + 10 Planks
    static readonly string iapSuperBundle8 = "super_bundle_8";		                // 10 Spanners + 10 Pipes + 10 Washers
    static readonly string iapSuperBundle4_hearts = "super_bundle_4_hearts";        // 200 Positive Energy + 25 Shovels + 300 Diamonds
    static readonly string iapSuperBundle4_hearts_50off = "super_bundle_4_hearts_50off";  // 200 Positive Energy + 25 Shovels + 300 Diamonds

    public static readonly string iapBox1 = "box_1";
    public static readonly string iapBox1_50off = "box_1_50off";
    public static readonly string iapBox2 = "box_2";

    public static readonly string iapUpgradeHelipad1 = "upgrade_helipad_1";
    public static readonly string iapUpgradeHelipad2 = "upgrade_helipad_2";
    public static readonly string iapUpgradeHelipad3 = "upgrade_helipad_3";
    public static readonly string iapUpgradeHelipad4 = "upgrade_helipad_4";
    public static readonly string iapUpgradeHelipad5 = "upgrade_helipad_5";

    public static readonly string iapUpgradeVipWard1 = "upgrade_vip_ward_1";
    public static readonly string iapUpgradeVipWard2 = "upgrade_vip_ward_2";
    public static readonly string iapUpgradeVipWard3 = "upgrade_vip_ward_3";
    public static readonly string iapUpgradeVipWard4 = "upgrade_vip_ward_4";
    public static readonly string iapUpgradeVipWard5 = "upgrade_vip_ward_5";

    private string[] lootBoxIDS = { iapBox1, iapBox1_50off, iapBox2 };

    private string[] coinProductIDs = { iapCoinPack1, iapCoinPack2, iapCoinPack3, iapCoinPack4, iapCoinPack5, iapCoinPack6 };
    private string[] promoCoinProductIDs = { iapCoinPack2_33off, iapCoinPack5_50off, iapCoinPack5_90off, iapCoinPack6_50off };

    private string[] diamondProductIDs = { iapDiamondPack1, iapDiamondPack2, iapDiamondPack3, iapDiamondPack4, iapDiamondPack5, iapDiamondPack6 };
    private string[] promoDiamondProductIDs = { iapDiamondPack1_50off, iapDiamondPack1_33off, iapDiamondPack1_67off,
                                                iapDiamondPack2_20off, iapDiamondPack2_40off, iapDiamondPack2_60off, iapDiamondPack2_80off,
                                                iapDiamondPack3_20off, iapDiamondPack3_30off, iapDiamondPack3_50off, iapDiamondPack3_70off, iapDiamondPack3_80off, iapDiamondPack3_90off,
                                                iapDiamondPack4_20off, iapDiamondPack4_25off, iapDiamondPack4_30off, iapDiamondPack4_50off, iapDiamondPack4_75off, iapDiamondPack4_90off,
                                                iapDiamondPack5_20off, iapDiamondPack5_25off, iapDiamondPack5_30off, iapDiamondPack5_50off, iapDiamondPack5_75off, iapDiamondPack5_90off,
                                                iapDiamondPack6_20off, iapDiamondPack6_25off, iapDiamondPack6_30off, iapDiamondPack6_50off, iapDiamondPack6_75off, iapDiamondPack6_90off,
                                                iapDiamondPack5_65off
                                          };

    private string[] specialPacksIDs = { iapSpecialPack1, iapSpecialPack2, iapBundlePack1, iapBundlePack2, iapBundlePack3, iapBundlePack4,
                                         iapValentines1, iapEaster1, iapLabourDay1, iapSuperBundle1, iapSuperBundle2, iapSuperBundle3, iapSuperBundle4, iapSuperBundle5, iapBBQBundle1, iapHalloween1, iapPinkRibbonDeco1,
                                         iapSuperBundle3_50off, iapSuperBundle6, iapSuperBundle6_50off, iapSuperBundle7, iapSuperBundle8, iapSuperBundle4_hearts, iapSuperBundle4_hearts_50off, iapWHOBundle1, iapFireWorksBundle1,iapValentines2,iapValentines3};
    /*
    private string[] upgradeHelipadIDs = { iapUpgradeHelipad1, iapUpgradeHelipad2, iapUpgradeHelipad3, iapUpgradeHelipad4, iapUpgradeHelipad5 };
    private string[] upgradeVipWardIDs = { iapUpgradeVipWard1, iapUpgradeVipWard2, iapUpgradeVipWard3, iapUpgradeVipWard4, iapUpgradeVipWard5 };
    */

    private int[] coinPackageAmounts;
    private int[] diamondPackageAmounts = { 75, 130, 275, 570, 1500, 4000 };
    private float[] coinPackageMultipliers = { 1f, 3.16667f, 8f, 19.5f, 41f, 132.33333f };
    private Dictionary<string, int> promoPackagesDict;

    public List<IAPSpecialPack> specialPacks;

    public UnityAction onIapInitialized = null;

    private static readonly string gpLicenseKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjAWvuFsbDKEARCJEHI2Ah+/F/Yu5QsBSihjFyQJv12r+1WNyth2mry7RIvFgQ/RNZwVmgiGCToLNWKCcdspULMgEAEEzcIBCHMiaUwcry5XhPFZ901BL5e4KoyKVNKJI2HKRXQ3rk/bqcMoMRYZhVb1fzA7UND5A6GuIx+7kX9BbWrAy6y0heuhoa/n97vfRPgD/AAcqeIBOVydTr/neix7OkerGK1qYBoDLSgvqsFpb6L71CI63dzRPchan6JDPSLs7ydYUR2wSgrqKhIGeAmiU03cF+4+3le6ag612UgPoarRC3kqaOyO8z61i55zj+rcMgsoyme0Zc2hsy4u0NwIDAQAB";

    void Start()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnDisable()
    {
        if (_saveAfterIAP != null) {
            try
            {
                StopCoroutine(_saveAfterIAP);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    public void Initialize()
    {
        CreatePromoPackagesDictionary();
        UpdateCoinAmounts();
        if (m_StoreController == null)
            InitializePurchasing();
    }

    void CreatePromoPackagesDictionary()
    {
        promoPackagesDict = new Dictionary<string, int>();
        promoPackagesDict.Add(iapDiamondPack1_50off, 0);    //deprecated
        promoPackagesDict.Add(iapDiamondPack1_33off, 0);
        promoPackagesDict.Add(iapDiamondPack1_67off, 0);
        promoPackagesDict.Add(iapDiamondPack2_20off, 1);
        promoPackagesDict.Add(iapDiamondPack2_40off, 1);
        promoPackagesDict.Add(iapDiamondPack2_60off, 1);
        promoPackagesDict.Add(iapDiamondPack2_80off, 1);
        promoPackagesDict.Add(iapDiamondPack3_20off, 2);
        promoPackagesDict.Add(iapDiamondPack3_30off, 2);
        promoPackagesDict.Add(iapDiamondPack3_50off, 2);
        promoPackagesDict.Add(iapDiamondPack3_70off, 2);
        promoPackagesDict.Add(iapDiamondPack3_80off, 2);
        promoPackagesDict.Add(iapDiamondPack3_90off, 2);
        promoPackagesDict.Add(iapDiamondPack4_20off, 3);
        promoPackagesDict.Add(iapDiamondPack4_25off, 3);
        promoPackagesDict.Add(iapDiamondPack4_30off, 3);
        promoPackagesDict.Add(iapDiamondPack4_50off, 3);
        promoPackagesDict.Add(iapDiamondPack4_75off, 3);
        promoPackagesDict.Add(iapDiamondPack4_90off, 3);
        promoPackagesDict.Add(iapDiamondPack5_20off, 4);
        promoPackagesDict.Add(iapDiamondPack5_25off, 4);
        promoPackagesDict.Add(iapDiamondPack5_30off, 4);
        promoPackagesDict.Add(iapDiamondPack5_50off, 4);
        promoPackagesDict.Add(iapDiamondPack5_75off, 4);
        promoPackagesDict.Add(iapDiamondPack5_90off, 4);
        promoPackagesDict.Add(iapDiamondPack6_20off, 5);
        promoPackagesDict.Add(iapDiamondPack6_25off, 5);
        promoPackagesDict.Add(iapDiamondPack6_30off, 5);
        promoPackagesDict.Add(iapDiamondPack6_50off, 5);
        promoPackagesDict.Add(iapDiamondPack6_75off, 5);
        promoPackagesDict.Add(iapDiamondPack6_90off, 5);

        promoPackagesDict.Add(iapCoinPack2_33off, 1);
        promoPackagesDict.Add(iapCoinPack5_50off, 4);
        promoPackagesDict.Add(iapCoinPack5_90off, 4);
        promoPackagesDict.Add(iapCoinPack6_50off, 5);
        promoPackagesDict.Add(iapDiamondPack5_65off, 4);
    }

    void InitializePurchasing()
    {
        if (IsInitialized())
            return;

        // Create a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add a product to sell / restore by way of its identifier, associating the general identifier
        // with its store-specific identifiers.
        for (int i = 0; i < coinProductIDs.Length; i++)
            builder.AddProduct(coinProductIDs[i], ProductType.Consumable);

        for (int i = 0; i < diamondProductIDs.Length; i++)
            builder.AddProduct(diamondProductIDs[i], ProductType.Consumable);

        for (int i = 0; i < promoCoinProductIDs.Length; i++)
            builder.AddProduct(promoCoinProductIDs[i], ProductType.Consumable);

        for (int i = 0; i < promoDiamondProductIDs.Length; i++)
            builder.AddProduct(promoDiamondProductIDs[i], ProductType.Consumable);

        for (int i = 0; i < specialPacksIDs.Length; i++)
            builder.AddProduct(specialPacksIDs[i], ProductType.Consumable);

        for (int i = 0; i < lootBoxIDS.Length; ++i)
            builder.AddProduct(lootBoxIDS[i], ProductType.Consumable);

        /*
        for (int i = 0; i < upgradeHelipadIDs.Length; ++i)
            builder.AddProduct(upgradeHelipadIDs[i], ProductType.NonConsumable);

        for (int i = 0; i < upgradeVipWardIDs.Length; ++i)
            builder.AddProduct(upgradeVipWardIDs[i], ProductType.NonConsumable);
            */
        // Continue adding the non-consumable product.
        //builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);

        // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
        // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
        UnityPurchasing.Initialize(this, builder);
    }

    public bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    //Coin amount in IAP packs depends on user level and is recalculated each time user opens IAP Pop Up.
    public void UpdateCoinAmounts()
    {
        //Debug.Log("Updating coin amounts. Player level = " + Game.Instance.gameState().GetHospitalLevel());
        coinPackageAmounts = new int[6];
        for (int i = 0; i < 6; i++)
        {
            coinPackageAmounts[i] = Mathf.RoundToInt(DiamondCostCalculator.GetBaseIAPCoinAmount() * coinPackageMultipliers[i]);
        }
    }

    public int GetCoinAmount(float coinPackageMultiplier)
    {
        return Mathf.RoundToInt(DiamondCostCalculator.GetBaseIAPCoinAmount() * coinPackageMultiplier);
    }

    public int[] GetCoinPackageAmounts()
    {
        return coinPackageAmounts;
    }

    public int[] GetDiamondPackageAmounts()
    {
        return diamondPackageAmounts;
    }

    public void BuyCoinPackage(int id)
    {
        // Buy the consumable product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        try
        {
            IAPController.isTimedOffer = false;
            BuyProductID(coinProductIDs[id]);
        }
        catch
        {
            Debug.LogError("GIVEN IAP PRODUCT ID NOT FOUND!");
            NativeAlerts.ShowNativeAlert("IAP Product not found! Please report this issue to support.", "OK");
        }
    }

    public void BuyDiamondPackage(int id)
    {
        // Buy the consumable product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        try
        {
            IAPController.isTimedOffer = false;
            BuyProductID(diamondProductIDs[id]);
        }
        catch
        {
            Debug.LogError("GIVEN IAP PRODUCT ID NOT FOUND!");
            NativeAlerts.ShowNativeAlert("IAP Product not found! Please report this issue to support.", "OK");
        }
    }

    /*
    public void BuyHelipadUpgrade(int id)
    {
        // Buy product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        try
        {
            IAPController.isTimedOffer = false;
            BuyProductID(upgradeHelipadIDs[id]);
        }
        catch
        {
            Debug.LogError("GIVEN IAP PRODUCT ID NOT FOUND!");
            NativeAlerts.ShowNativeAlert("IAP Product not found! Please report this issue to support.", "OK");
        }
    }

    public void BuyVipWardUpgrade(int id)
    {
        // Buy product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        try
        {
            IAPController.isTimedOffer = false;
            BuyProductID(upgradeVipWardIDs[id]);
        }
        catch
        {
            Debug.LogError("GIVEN IAP PRODUCT ID NOT FOUND!");
            NativeAlerts.ShowNativeAlert("IAP Product not found! Please report this issue to support.", "OK");
        }
    }
    */
    public void BuyProductID(string productId, bool isTimedOffer = false, string dataToCache = null)
    {
        IAPController.isPauseBlocked = true;
        IAPController.isTimedOffer = isTimedOffer;

        switch (AnalyticsController.currentIAPFunnel)
        {
            case CurrentIAPFunnel.MissingResources:
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPMissingResources.ToString(), (int)FunnelStepIAPMissingResources.StartPurchase, FunnelStepIAPMissingResources.StartPurchase.ToString());
                break;
            case CurrentIAPFunnel.MissingDiamonds:
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPMissingDiamonds.ToString(), (int)FunnelStepIAPMissingDiamonds.StartPurchase, FunnelStepIAPMissingDiamonds.StartPurchase.ToString());
                break;
            case CurrentIAPFunnel.PopUp:
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPPopUp.ToString(), (int)FunnelStepIAPPopUp.StartPurchase, FunnelStepIAPPopUp.StartPurchase.ToString());
                break;
            case CurrentIAPFunnel.VGP:
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPVGP.ToString(), (int)FunnelStepIAPVGP.StartPurchase, FunnelStepIAPVGP.StartPurchase.ToString());
                break;
        }
        if (IsInitialized())
        {
            ReferenceHolder.Get().iapFade.Show();

            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            UnityEngine.Purchasing.Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id)); // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 

                // asynchronously.
                foreach (string prodId in lootBoxIDS)
                {
                    if (prodId == productId && !string.IsNullOrEmpty(dataToCache))
                        if (!ReferenceHolder.Get().lootBoxManager.PurchaseStarted(prodId, dataToCache))
                        {
                            Debug.LogError("Another pending box purchase!!!");
                            return;
                        }
                }
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                NativeAlerts.ShowNativeAlert("Purchase Failed\n IAP not found or not available for purchase!", "OK");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
            NativeAlerts.ShowNativeAlert("Purchase Failed\n IAP not initialized!", "OK");
        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            NativeAlerts.ShowNativeAlert("Restore Purchases Failed\n IAP not initialized!", "OK");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");
            ReferenceHolder.Get().iapFade.Show();

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) =>
            {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    //  
    // --- IStoreListener
    //
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[IAP] OnInitialized: PASS");
        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;

        GetPrices();

        onIapInitialized?.Invoke();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("IAPController OnInitializeFailed InitializationFailureReason:" + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError("IAPController OnInitializeFailed InitializationFailureReason:" + error + " Message:" + message);
    }

    public bool IsAnyPendingLootBoxTransactions()
    {
        foreach (UnityEngine.Purchasing.Product product in pendingProducts)
        {
            for (int i = 0; i < lootBoxIDS.Length; ++i)
            {
                if (lootBoxIDS[i] == product.definition.id)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public string GetPrizeByProductID(string productID)
    {
        if (IsInitialized())
        {
            UnityEngine.Purchasing.Product p = m_StoreController.products.WithID(productID);
            if (p == null)
                return null;
            return p.metadata.localizedPriceString;
        }
        return null;
    }

    public string GetDiamondPriceByIndex(int index)
    {
        if (index >= diamondProductIDs.Length)
            return null;

        if (IsInitialized())
        {
            return m_StoreController.products.WithID(diamondProductIDs[index]).metadata.localizedPriceString;
        }
        return null;
    }
    /*
    public string GetHelipadUpgradePriceByIndex(int index)
    {
        if (index >= upgradeHelipadIDs.Length)
            return null;

        if (IsInitialized())
        {
            return m_StoreController.products.WithID(upgradeHelipadIDs[index]).metadata.localizedPriceString;
        }
        return null;
    }

    public string GetVipWardPriceByIndex(int index)
    {
        if (index >= upgradeVipWardIDs.Length)
            return null;

        if (IsInitialized())
        {
            return m_StoreController.products.WithID(upgradeVipWardIDs[index]).metadata.localizedPriceString;
        }
        return null;
    }
    */
    public void GetPrices()
    {
        string[] coinPrices = new string[coinProductIDs.Length];
        string[] diamondPrices = new string[diamondProductIDs.Length];
        string cancerFoundationPrice = "";

        if (IsInitialized())
        {
            for (int i = 0; i < coinProductIDs.Length; i++)
                coinPrices[i] = m_StoreController.products.WithID(coinProductIDs[i]).metadata.localizedPriceString;

            for (int i = 0; i < diamondProductIDs.Length; i++)
                diamondPrices[i] = m_StoreController.products.WithID(diamondProductIDs[i]).metadata.localizedPriceString;

            cancerFoundationPrice = m_StoreController.products.WithID(iapDiamondPack3_20off).metadata.localizedPriceString;
        }
        else
        {
            Debug.LogError("IAP system is not initialized. Cannot get prices for packages!");
        }
        UIController.get.breastCancerPopup.UpdatePrice(cancerFoundationPrice);
    }

    public string GetPriceForProduct(string iapProduct)
    {
        if (m_StoreController.products.WithID(iapProduct) == null)
        {
            Debug.LogError("IAP Product with that name does not exist or is not initialized! " + iapProduct);
            return "";
        }

        return m_StoreController.products.WithID(iapProduct).metadata.localizedPriceString;
    }

    public string GetPriceForLootBox(string ID)
    {
        return m_StoreController.products.WithID(ID).metadata.localizedPriceString;
    }

    public string GetValentinePrice()
    {
        return m_StoreController.products.WithID(iapValentines1).metadata.localizedPriceString;
    }

    public string GetEasterPrize()
    {
        return m_StoreController.products.WithID(iapEaster1).metadata.localizedPriceString;
    }

    public string GetLabourDayPrize()
    {
        return m_StoreController.products.WithID(iapLabourDay1).metadata.localizedPriceString;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        ReferenceHolder.Get().iapFade.Hide();

        UpdateCoinAmounts();

        //check if this transaction has been already saved on aws (to prevent double IAP bug)
        bool wasTransactionCompleted = WasTransactionCompleted(args.purchasedProduct.transactionID, args.purchasedProduct.definition.id);
        Debug.Log(string.Format("Product {0} with transactionID {1} has been already added {2}", args.purchasedProduct.transactionID, args.purchasedProduct.definition.id, wasTransactionCompleted));
        if (wasTransactionCompleted)
        {
            IAPController.isPauseBlocked = false;
            return PurchaseProcessingResult.Complete;
        }
#if !UNITY_EDITOR
        ProcessPurchaseAppsFlyer(args);
#endif
#region receipt_validation
        bool validPurchase = true; // Presume valid for platforms with no R.V.
                                   // Unity IAP's validation logic is only included on these platforms.
#if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
        // Prepare the validator with the secrets we prepared in the Editor
        // obfuscation window.
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
            AppleTangle.Data(), Application.identifier);

        try
        {
            // On Google Play, result has a single product ID.
            // On Apple stores receipts contain multiple products.
            var result = validator.Validate(args.purchasedProduct.receipt);
            // For informational purposes, we list the receipt(s)
            Debug.Log("Receipt is valid. Contents:");
            foreach (IPurchaseReceipt productReceipt in result)
            {
                Debug.Log(productReceipt.productID);
                Debug.Log(productReceipt.purchaseDate);
                Debug.Log(productReceipt.transactionID);
            }
        }
        catch (IAPSecurityException)
        {
#if !UNITY_EDITOR
            Debug.Log("Invalid receipt, not unlocking content");
            AnalyticsController.instance.ReportIAPTransaction(IAPResult.FRAUD, args.purchasedProduct);
#endif
            validPurchase = false;

#if !UNITY_EDITOR
            IAPController.isPauseBlocked = false;
            return PurchaseProcessingResult.Complete;
#endif
        }
#endif
#if UNITY_EDITOR
        validPurchase = true;
#endif
#endregion
        if (validPurchase)
        {
            TimedOffersController.Instance.OnIAPSuccessBought(args.purchasedProduct.definition.id);

#region adding_items

            //coin packs
            for (int i = 0; i < coinProductIDs.Length; i++)
            {
                if (String.Equals(args.purchasedProduct.definition.id, coinProductIDs[i], StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("ProcessPurchase: SUCCESS. Product: '{0}'", args.purchasedProduct.definition.id));
                    int coinAmount = coinPackageAmounts[i];
                    int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                    Game.Instance.gameState().AddCoins(coinAmount, EconomySource.IAP, false);
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, Vector3.zero, coinAmount, 0f, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), coinsSprite, null, () =>
                    {
                        Game.Instance.gameState().UpdateCounter(ResourceType.Coin, coinAmount, currentCoinAmount);
                    });
                }
            }
            //diamond packs
            for (int i = 0; i < diamondProductIDs.Length; i++)
            {
                if (String.Equals(args.purchasedProduct.definition.id, diamondProductIDs[i], StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("ProcessPurchase: SUCCESS. Product: '{0}'", args.purchasedProduct.definition.id));
                    int diamondAmount = diamondPackageAmounts[i];

                    int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
                    Game.Instance.gameState().AddDiamonds(diamondAmount, EconomySource.IAP, false, true);
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, Vector3.zero, diamondAmount, 0f, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), diamondsSprite, null, () =>
                    {
                        Game.Instance.gameState().UpdateCounter(ResourceType.Diamonds, diamondAmount, currentDiamondAmount);
                    });
                }
            }

            //promo coin packs
            for (int i = 0; i < promoCoinProductIDs.Length; i++)
            {
                if (String.Equals(args.purchasedProduct.definition.id, promoCoinProductIDs[i], StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("ProcessPurchase: SUCCESS. Product: '{0}'", args.purchasedProduct.definition.id));
                    int coinAmount = coinPackageAmounts[promoPackagesDict[promoCoinProductIDs[i]]];
                    int currentCoinAmout = Game.Instance.gameState().GetCoinAmount();
                    Game.Instance.gameState().AddCoins(coinAmount, EconomySource.IAP, false);
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, Vector3.zero, coinAmount, 0f, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), coinsSprite, null, () =>
                    {
                        Game.Instance.gameState().UpdateCounter(ResourceType.Coin, coinAmount, currentCoinAmout);
                    });
                }
            }

            //promo diamond packs
            for (int i = 0; i < promoDiamondProductIDs.Length; i++)
            {
                if (String.Equals(args.purchasedProduct.definition.id, promoDiamondProductIDs[i], StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("ProcessPurchase: SUCCESS. Product: '{0}'", args.purchasedProduct.definition.id));
                    int diamondAmount = diamondPackageAmounts[promoPackagesDict[promoDiamondProductIDs[i]]];
                    int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
                    Game.Instance.gameState().AddDiamonds(diamondAmount, EconomySource.IAP, false, true);
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, Vector3.zero, diamondAmount, 0f, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), diamondsSprite, null, () =>
                    {
                        Game.Instance.gameState().UpdateCounter(ResourceType.Diamonds, diamondAmount, currentDiamondAmount);
                    });
                }
            }

            for (int i = 0; i < lootBoxIDS.Length; ++i)
            {
                if (String.Equals(args.purchasedProduct.definition.id, lootBoxIDS[i], StringComparison.Ordinal))
                {
                    ReferenceHolder.Get().lootBoxManager.CompletePurchase(args.purchasedProduct.definition.id);
                }
            }

            string productID = "";
            productID = args.purchasedProduct.definition.id;

            //special packs (mixed coin/diamond/decoration/positive energy)
            for (int i = 0; i < specialPacksIDs.Length; i++)
            {
                if (String.Equals(productID, specialPacksIDs[i], StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("ProcessPurchase: SUCCESS. Product: '{0}'", args.purchasedProduct.definition.id));
                    bool specialPackFound = false;
                    foreach (IAPSpecialPack specialPack in specialPacks)
                    {
                        if (productID == specialPack.productID)
                        {
                            specialPack.ApplyPack();
                            specialPackFound = true;
                            break;
                        }
                    }

                    if(!specialPackFound)
                    {
                        if (BundleManager.Instance.superBundleManager.GetPackage(productID) != null)
                        {
                            BundleManager.Instance.superBundleManager.GetPackage(productID).Collect();
                            //UIController.get.AddCurrencyPopUp.Exit();
                        }
                        else
                        {
                            Debug.LogError("THIS SPECIAL PRODUCT IS NOT HANDLED! FIX THIS! Product ID: " + productID);
                        }
                    }
                }
            }
            /*
            for (int i = 0; i < upgradeHelipadIDs.Length; ++i)
            {
                if (String.Equals(args.purchasedProduct.definition.id, upgradeHelipadIDs[i], StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("ProcessPurchase: SUCCESS. Product: '{0}'", productID));
                    if (productID == iapUpgradeHelipad1)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.heliMastership.SetMasteryLevel(1);
                    }
                    else if (productID == iapUpgradeHelipad2)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.heliMastership.SetMasteryLevel(2);
                    }
                    else if (productID == iapUpgradeHelipad3)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.heliMastership.SetMasteryLevel(3);
                    }
                    else if (productID == iapUpgradeHelipad4)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.heliMastership.SetMasteryLevel(4);
                    }
                    else if (productID == iapUpgradeHelipad5)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.heliMastership.SetMasteryLevel(5);
                    }
                }
            }

            for (int i = 0; i < upgradeVipWardIDs.Length; ++i)
            {
                if (String.Equals(args.purchasedProduct.definition.id, upgradeVipWardIDs[i], StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("ProcessPurchase: SUCCESS. Product: '{0}'", productID));
                    if (productID == iapUpgradeVipWard1)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.vipMastership.SetMasteryLevel(1);
                    }
                    else if (productID == iapUpgradeVipWard2)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.vipMastership.SetMasteryLevel(2);
                    }
                    else if (productID == iapUpgradeVipWard3)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.vipMastership.SetMasteryLevel(3);
                    }
                    else if (productID == iapUpgradeVipWard4)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.vipMastership.SetMasteryLevel(4);
                    }
                    else if (productID == iapUpgradeVipWard5)
                    {
                        ReferenceHolder.GetHospital().vipSystemManager.vipMastership.SetMasteryLevel(5);
                    }
                }
            }
            */
#endregion
#region analytics
            Game.Instance.gameState().IncrementIAPPurchasesCount();
            Game.Instance.gameState().SetIAPBoughtLately(true);
            Game.Instance.gameState().SetLastSuccessfulPurchase(args.purchasedProduct.definition.id + " " + DateTime.UtcNow.ToString());

            switch (AnalyticsController.currentIAPFunnel)
            {
                case CurrentIAPFunnel.MissingResources:
                    AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPMissingResources.ToString(), (int)FunnelStepIAPMissingResources.PurchaseComplete, FunnelStepIAPMissingResources.PurchaseComplete.ToString());
                    break;
                case CurrentIAPFunnel.MissingDiamonds:
                    AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPMissingDiamonds.ToString(), (int)FunnelStepIAPMissingDiamonds.PurchaseComplete, FunnelStepIAPMissingDiamonds.PurchaseComplete.ToString());
                    break;
                case CurrentIAPFunnel.PopUp:
                    AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPPopUp.ToString(), (int)FunnelStepIAPPopUp.PurchaseComplete, FunnelStepIAPPopUp.PurchaseComplete.ToString());
                    break;
                case CurrentIAPFunnel.VGP:
                    AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPVGP.ToString(), (int)FunnelStepIAPVGP.PurchaseComplete, FunnelStepIAPVGP.PurchaseComplete.ToString());
                    break;
            }
            AnalyticsController.instance.ReportIAPTransaction(IAPResult.PURCHASE, args.purchasedProduct);

            TenjinController.instance.ReportIAP(args.purchasedProduct);
            ReportIAPFB(args.purchasedProduct);

            if (Game.Instance.gameState().GetIAPPurchasesCount() == 1)
                TenjinController.instance.SendFirstPurchaseEvent();
#endregion

            SaveTransaction(args.purchasedProduct.transactionID, args.purchasedProduct.definition.id);
            Hospital.SaveSynchronizer.Instance.InstantSave();

        }

        _saveAfterIAP = StartCoroutine(SaveAfterIAP());

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        if (pendingProducts == null)
            pendingProducts = new List<UnityEngine.Purchasing.Product>();
        pendingProducts.Add(args.purchasedProduct);

        IAPController.isPauseBlocked = false;
        return PurchaseProcessingResult.Pending;
    }

    // Send IAP analytics to AppsFlyer
    public void ProcessPurchaseAppsFlyer(PurchaseEventArgs args)
    {
        string prodID = args.purchasedProduct.definition.storeSpecificId;
        string price = args.purchasedProduct.metadata.localizedPrice.ToString();
        string currency = args.purchasedProduct.metadata.isoCurrencyCode;
        if (!args.purchasedProduct.hasReceipt)
        {
            return;
        }
        string receipt = args.purchasedProduct.receipt;
        var recptToJSON = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize(args.purchasedProduct.receipt);
        var transactionID = (string)recptToJSON["TransactionID"];
#if UNITY_IOS
        AppsFlyeriOS.validateAndSendInAppPurchase(prodID, price, currency, transactionID, null, this);
#endif
#if UNITY_ANDROID
        var payload = (string)recptToJSON["Payload"];
        var gpDetails = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
        var gpJson = (string)gpDetails["json"];
        var gpSig = (string)gpDetails["signature"];
        AppsFlyerAndroid.validateAndSendInAppPurchase(gpLicenseKey, gpSig, gpJson, price, currency, null, this);
#endif
    }

    public void TestSuperBundleResult()
    {
        string iapSuperBundle3_50offContent = "Diamond!!300*PositiveEnergy!!200*Medicine!Special(3)!25";
        SuperBundlePackage bundle = new SuperBundlePackage(iapSuperBundle3_50offContent);
        bundle.Collect(true, 0.2f);
    }

    private void ReportIAPFB(UnityEngine.Purchasing.Product product)
    {
        try
        {
            if (FB.IsInitialized)
            {
                object productID = product.definition.id;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("id", productID);
                FB.LogPurchase((float)product.metadata.localizedPrice, product.metadata.isoCurrencyCode, parameters);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

    }

    IEnumerator SaveAfterIAP()
    {
        //this is to make sure that save is done AFTER return PurchaseProcessingResult.Pending
        yield return null;
        yield return null;
        Hospital.SaveSynchronizer.Instance.InstantSave();
    }

    public void ConfirmPendingPurchase()
    {
        if (pendingProducts == null || pendingProducts.Count == 0)
        {
            //Debug.LogError("There are no pending IAP products waiting to be marked as completed!");
            return;
        }

        //Debug.LogError("There are no pending IAP products waiting to be marked as completed!");
        for (int i = 0; i < pendingProducts.Count; i++)
            m_StoreController.ConfirmPendingPurchase(pendingProducts[i]);

        pendingProducts.Clear();
        //ReferenceHolder.Get().iapFade.Hide();
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
    {
        ReferenceHolder.Get().iapFade.Hide();

        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        //NativeAlerts.ShowNativeAlert(string.Format("Purchase Failed\n, Reason: {0}", failureReason), "OK");

        AnalyticsController.instance.ReportIAPTransaction(IAPResult.CANCEL, product);


        for (int i = 0; i < lootBoxIDS.Length; ++i)
        {
            if (String.Equals(product.definition.id, lootBoxIDS[i], StringComparison.Ordinal))
            {
                ReferenceHolder.Get().lootBoxManager.AbortPurchase(product.definition.id);
                break;
            }
        }

        IAPController.isPauseBlocked = false;
    }

#region saved_transactions
    Dictionary<string, string> completedTransactions;

    bool WasTransactionCompleted(string transactionId, string productId)
    {
        if (completedTransactions.ContainsKey(transactionId) && completedTransactions[transactionId] == productId)
            return true;
        else
            return false;
    }

    void SaveTransaction(string transactionId, string productId)
    {
        //Debug.Log("Adding transaction to save. transactionId: " + transactionId + ", product: " + productId);

        if (completedTransactions != null)
            completedTransactions.Add(transactionId, productId);
        else
        {
            Debug.LogError("completedTransactions has not been loaded yet!");
            AnalyticsController.instance.ReportException("completed_transactions_is_null", new Exception());
        }
    }

    public void LoadTransactions(string parsedDict)
    {
        if (completedTransactions == null)
            completedTransactions = new Dictionary<string, string>();
        else
            completedTransactions.Clear();

        completedTransactions = ParseTransactionsDictionary(parsedDict);
    }

    Dictionary<string, string> ParseTransactionsDictionary(string parsedDict)
    {
        //Debug.Log("Parsing dictionary from string: " + parsedDict);
        Dictionary<string, string> tempDict = new Dictionary<string, string>();

        if (parsedDict == null || parsedDict.Length == 0)
            return tempDict;

        string[] temp = parsedDict.Split('*');
        for (int i = 0; i < temp.Length - 1; i += 2)
        {
            tempDict.Add(temp[i], temp[i + 1]);
        }

        //Debug.Log("Finished parsing dictionary. Count: " + tempDict.Count);
        return tempDict;
    }

    public string GetParsedTransactionDictionary()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (KeyValuePair<string, string> pair in completedTransactions)
        {
            sb.Append(pair.Key);
            sb.Append("*");
            sb.Append(pair.Value);
            sb.Append("*");
        }

        //Debug.Log("Parsed Dictionary = " + sb.ToString());
        return sb.ToString();
    }
    #endregion

    #region IAppsFlyerValidateReceipt
    public void didFinishValidateReceipt(string result)
    {
        AppsFlyer.AFLog("didFinishValidateReceipt", result);
    }

    public void didFinishValidateReceiptWithError(string error)
    {
        AppsFlyer.AFLog("didFinishValidateReceiptWithError", error);
    }
    #endregion
}