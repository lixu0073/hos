using Hospital.LootBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IAPShopData
{
    private const int INVISIBLE = -1;

    public IAPShopSection[] Orderedsection { get; private set; }
    public Dictionary<IAPShopBundleID, BundleOfferCard> SpecialOfferCards { get; private set; }
    public Dictionary<IAPShopBundleID, BundleOfferCard> FeatureCards { get; private set; }
    public Dictionary<IAPShopCoinPackageID, CoinOfferCard> CoinCards { get; private set; }

    public IAPShopData() { }

    public void SetIAPShopModuleOrder(IAPShopSection[] orderedSection)
    {
        Orderedsection = orderedSection;
    }

    public void SetCoinPackages(Dictionary<IAPShopCoinPackageID, IAPShopCoinPackageData> packages)
    {
        CoinCards = new Dictionary<IAPShopCoinPackageID, CoinOfferCard>();
        Dictionary<IAPShopCoinPackageID, IAPShopCoinPackageData> coinOffers = packages.OrderBy(x => x.Value.order).ToDictionary(x => x.Key, x => x.Value);

        if (coinOffers.Count == 0)
        {
            // add default packages
            coinOffers.Add(IAPShopCoinPackageID.packOfCoinsForVideo, new IAPShopCoinPackageData(IAPShopCoinPackageID.packOfCoinsForVideo, 1, 0, 0, String.Empty, String.Empty));
            coinOffers.Add(IAPShopCoinPackageID.packOfCoins1, new IAPShopCoinPackageData(IAPShopCoinPackageID.packOfCoins1, 2, 100, 3.16667f, "coins2", String.Empty));
            coinOffers.Add(IAPShopCoinPackageID.packOfCoins2, new IAPShopCoinPackageData(IAPShopCoinPackageID.packOfCoins2, 3, 200, 8f, "coins3", String.Empty));
            coinOffers.Add(IAPShopCoinPackageID.packOfCoins3, new IAPShopCoinPackageData(IAPShopCoinPackageID.packOfCoins3, 4, 990, 41f, "coins4", String.Empty));
        }

        int order = 1;
        foreach (KeyValuePair<IAPShopCoinPackageID, IAPShopCoinPackageData> offer in coinOffers)
        {
            if (offer.Value.order != INVISIBLE)
            {

                CoinCards.Add(offer.Key, offer.Value.IsCreative() ?
                    new CreativeCoinOfferCard(offer.Value.iapProductId, offer.Value.decisionPoint, offer.Value.costInDiamonds, offer.Value.ID, offer.Value.multiplier, order, GetSectionOrder(IAPShopSection.sectionCoins)) :
                    new CoinOfferCard(offer.Value.iapProductId,offer.Value.costInDiamonds, offer.Value.ID, offer.Value.multiplier, order, GetSectionOrder(IAPShopSection.sectionCoins))
                    );
                ++order;
            }
        }
    }

    public int GetSectionOrder(IAPShopSection section)
    {
        for (int i = 0; i < Orderedsection.Length; ++i)
        {
            if (Orderedsection[i] == section)
                return i + 1;
        }
        return -1;
    }

    public void SetBundleOfferData(Dictionary<IAPShopBundleID, IAPShopBundleData> bundles)
    {
        SpecialOfferCards = new Dictionary<IAPShopBundleID, BundleOfferCard>();
        FeatureCards = new Dictionary<IAPShopBundleID, BundleOfferCard>();

        Dictionary<IAPShopBundleID, IAPShopBundleData> specialOffers = bundles.OrderBy(x => x.Value.orderInSpecialOffersSection).ToDictionary(x => x.Key, x => x.Value);
        Dictionary<IAPShopBundleID, IAPShopBundleData> featureOffers = bundles.OrderBy(x => x.Value.orderInFeaturesSection).ToDictionary(x => x.Key, x => x.Value);

        int order = 1;
        foreach (KeyValuePair<IAPShopBundleID, IAPShopBundleData> specialOffer in specialOffers)
        {
            if (specialOffer.Value.orderInSpecialOffersSection != INVISIBLE)
            {
                BundleOfferCard cardToAdd = null;

                if (specialOffer.Value.IsCreative())
                {
                    if ((specialOffer.Value.iapProductId == IAPController.iapBox1 ||
                        specialOffer.Value.iapProductId == IAPController.iapBox2 ||
                        specialOffer.Value.iapProductId == IAPController.iapBox1_50off))

                    {
                        if (ReferenceHolder.Get().lootBoxManager.ShowLootBoxOnIapShop())
                        {
                            cardToAdd = new LootBoxOfferCard(0, specialOffer.Value.ID, IAPShopSection.sectionSpecialOffers, order, GetSectionOrder(IAPShopSection.sectionSpecialOffers), specialOffer.Value.iapProductId, specialOffer.Value.decisionPoint);

                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        cardToAdd = new CreativeOfferCard(0, specialOffer.Value.ID, IAPShopSection.sectionSpecialOffers, order, GetSectionOrder(IAPShopSection.sectionSpecialOffers), specialOffer.Value.iapProductId, specialOffer.Value.decisionPoint);
                    }
                }
                else
                {
                    cardToAdd = new BundleOfferCard(specialOffer.Value.costInDiamonds, specialOffer.Value.ID, IAPShopSection.sectionSpecialOffers, order, GetSectionOrder(IAPShopSection.sectionSpecialOffers), specialOffer.Value.color);
                }

                if (cardToAdd != null)
                {
                    SpecialOfferCards.Add(specialOffer.Key, cardToAdd);
                    ++order;
                }
            }
        }

        order = 1;
        foreach (KeyValuePair<IAPShopBundleID, IAPShopBundleData> featureOffer in featureOffers)
        {
            if (featureOffer.Value.orderInFeaturesSection != INVISIBLE)
            {
                BundleOfferCard cardToAdd = null;

                if (featureOffer.Value.IsCreative())
                {
                    if ((featureOffer.Value.iapProductId == IAPController.iapBox1 ||
                        featureOffer.Value.iapProductId == IAPController.iapBox2 ||
                        featureOffer.Value.iapProductId == IAPController.iapBox1_50off))
                    {
                        if (ReferenceHolder.Get().lootBoxManager.ShowLootBoxOnIapShop())
                        {
                            cardToAdd = new LootBoxOfferCard(0, featureOffer.Value.ID, IAPShopSection.sectionSpecialOffers, order, GetSectionOrder(IAPShopSection.sectionSpecialOffers), featureOffer.Value.iapProductId, featureOffer.Value.decisionPoint);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        cardToAdd = new CreativeOfferCard(0, featureOffer.Value.ID, IAPShopSection.sectionSpecialOffers, order, GetSectionOrder(IAPShopSection.sectionSpecialOffers), featureOffer.Value.iapProductId, featureOffer.Value.decisionPoint);
                    }
                }
                else
                {
                    cardToAdd = new BundleOfferCard(featureOffer.Value.costInDiamonds, featureOffer.Value.ID, IAPShopSection.sectionSpecialOffers, order, GetSectionOrder(IAPShopSection.sectionSpecialOffers), featureOffer.Value.color);
                }

                if (cardToAdd != null)
                {
                    FeatureCards.Add(featureOffer.Key, cardToAdd);
                    ++order;
                }
                //FeatureCards.Add(featureOffer.Key,
                //    featureOffer.Value.IsCreative() ?
                //    new CreativeOfferCard(0, featureOffer.Value.ID, IAPShopSection.sectionSpecialOffers, order, GetSectionOrder(IAPShopSection.sectionSpecialOffers), featureOffer.Value.iapProductId, featureOffer.Value.decisionPoint) :
                //    new BundleOfferCard(featureOffer.Value.costInDiamonds, featureOffer.Value.ID, IAPShopSection.sectionFeatures, order, GetSectionOrder(IAPShopSection.sectionFeatures), featureOffer.Value.color));
                //++order;
            }
        }
    }

}
