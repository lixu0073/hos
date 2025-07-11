using System.Collections.Generic;
using UnityEngine;
using SimpleUI;

/// <summary>
/// 特殊场合枚举，定义不同的节日或特殊活动类型
/// </summary>
public enum Occasion
{
    Valentine,
    Easter,
    None
}

/// <summary>
/// 药品礼物类，用于定义特殊礼包中包含的药品奖励
/// </summary>
[System.Serializable]
public class MedicineGift
{
    [SerializeField] private int medicineID = 0;
    [SerializeField] private int medicineAmount = 0;

    public string GetMedicineCode()
    {
        return string.Format("Medicine!Special({0})!{1}", medicineID, medicineAmount);
    }    
}

/// <summary>
/// IAP特殊礼包配置，定义各种特殊场合的应用内购买礼包内容。
/// 包括货币奖励、装饰品、药品、增益道具等多种奖励组合。
/// </summary>
[CreateAssetMenu(fileName = "SpecialPack", menuName = "IAP/SpecialPack")]
public class IAPSpecialPack : ScriptableObject
{    
    [SerializeField] public string productID;
#pragma warning disable 0649
    [SerializeField] private float coinPackMultiplier;
    [SerializeField] private int diamondAmount;
    [SerializeField] private int positiveAmount;
#pragma warning restore 0649

    [SerializeField] private int boosterID = -1;
#pragma warning disable 0649
    [SerializeField] private List<ShopRoomInfo> decorations;
    [SerializeField] private List<MedicineGift> medicines;
#pragma warning restore 0649
    [SerializeField] private Occasion occasion = Occasion.None;
    [SerializeField] private bool isStarterPack = false;

    private int coinAmount;

    public void ApplyPack()
    {
        //add coins
        if (coinPackMultiplier > 0.001f)
        {
            coinAmount = Mathf.RoundToInt(DiamondCostCalculator.GetBaseIAPCoinAmount() * coinPackMultiplier);
            int currentCoinAmont = Game.Instance.gameState().GetCoinAmount();
            Game.Instance.gameState().AddCoins(coinAmount, EconomySource.IAP, false);

            //visual effect
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, Vector3.zero, coinAmount, .4f, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), IAPController.instance.coinsSprite, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Coin, coinAmount, currentCoinAmont);
            });
        }
        
        //add diamonds
        if (diamondAmount > 0)
        {
            int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
            Game.Instance.gameState().AddDiamonds(diamondAmount, EconomySource.IAP, false, true);

            //visual effect
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, Vector3.zero, diamondAmount, 0f, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), IAPController.instance.diamondsSprite, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Diamonds, diamondAmount, currentDiamondAmount);
            });
        }

        //add decoration
        foreach(ShopRoomInfo decoration in decorations)
        {
            if (decoration != null)
            {
                //add items to players database
                Game.Instance.gameState().AddToObjectStored(decoration, 1);
                //visual effect
                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, Vector3.zero, 1, .8f, 2f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decoration.ShopImage, null, null);
            }
        }

        //add medicine
        string medicinesCode = string.Empty;
        //foreach (MedicineGift medicine in medicines)
        for (int i = 0; i < medicines.Count; ++i)
        {
            medicinesCode += medicines[i].GetMedicineCode();
            if (i < (medicines.Count - 1)) medicinesCode += "*";
        }
        if (!string.IsNullOrEmpty(medicinesCode))
        {
            SuperBundlePackage bundle = new SuperBundlePackage(medicinesCode);
            bundle.Collect(true, 0.2f);
        }

        //add positive energy
        if (positiveAmount > 0)
        {
            Game.Instance.gameState().AddPositiveEnergy(positiveAmount, EconomySource.IAP);
            //visual effect
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.PositiveEnergy, ReferenceHolder.Get().engine.MainCamera.LookingAt, positiveAmount, 1f, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), IAPController.instance.positiveSprite, null, null);
        }

        //add booster
        if (boosterID > -1)
        {
            //add items to players database
            Hospital.HospitalAreasMapController.HospitalMap.boosterManager.AddBooster(boosterID, EconomySource.IAP);
            //visual effect
            Vector2 startPoint = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition);
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Booster, startPoint, 1, 1.5f, 1.75f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), ResourcesHolder.Get().boosterDatabase.boosters[boosterID].icon, null, null);
        }

        //report stuff
        if (isStarterPack)
            AnalyticsController.instance.starterPack.ReportPurchased();

        //if occasion show popup
        switch (occasion)
        {
            case Occasion.Valentine:
                Game.Instance.gameState().IncrementIAPValentineCount();
                Game.Instance.gameState().SetIAPBoughtLately(true);

                if (Game.Instance.gameState().GetIAPValentineCount() >= 3)
                {
                    UIController.getHospital.EventPopUp.Exit();
                    if (FindObjectOfType<EventButton>())
                        FindObjectOfType<EventButton>().Setup();
                }

                if (FindObjectOfType<ValentinePopUpContent>())
                    FindObjectOfType<ValentinePopUpContent>().SetAvailableInfo();
                break;
            case Occasion.Easter:
                Game.Instance.gameState().IncrementIAPEasterCount();
                Game.Instance.gameState().SetIAPBoughtLately(true);

                if (Game.Instance.gameState().GetIAPEasterCount() >= 3)
                {
                    UIController.getHospital.EventPopUp.Exit();
                    if (FindObjectOfType<EventButton>())
                        FindObjectOfType<EventButton>().Setup();
                }

                if (FindObjectOfType<EasterPopUpContent>())
                    FindObjectOfType<EasterPopUpContent>().SetAvailableInfo();
                break;
            case Occasion.None:
                break;
            default:
                break;
        }
    }    
}
