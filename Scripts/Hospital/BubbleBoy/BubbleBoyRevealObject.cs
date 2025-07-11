using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hospital;

[System.Serializable]
public class BubbleBoyRevealObject : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI rewardAmountText;
#pragma warning restore 0649
    public int bubbleID = 0;

    PointerDownListener pointerDownListener;
    BubbleBoyReward reward;
    

    public void SetRevealObject(BubbleBoyReward reward)
    {
        this.reward = reward;

        itemImage.sprite = reward.sprite;
        rewardAmountText.text = reward.amount.ToString();

        AdjustSize();
        SetTooltip();
    }

    void AdjustSize()
    {
        if (reward.rewardType == BubbleBoyPrizeType.Coin || reward.rewardType == BubbleBoyPrizeType.Diamond)
        {
            rewardAmountText.alignment = TextAlignmentOptions.Bottom;
        }
        else if (reward.rewardType == BubbleBoyPrizeType.Decoration)
        {
            rewardAmountText.alignment = TextAlignmentOptions.Right;
        }
        else
        {
            rewardAmountText.alignment = TextAlignmentOptions.Right;
        }
    }

    void SetTooltip()
    {
        if(pointerDownListener == null)
            pointerDownListener = GetComponent<PointerDownListener>();

        switch (reward.rewardType)
        {
            case BubbleBoyPrizeType.Coin:
                pointerDownListener.SetDelegate(() =>
                {
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("COINS"), I2.Loc.ScriptLocalization.Get("COINS_DESCRIPTION"));
                });
                break;
            case BubbleBoyPrizeType.Diamond:
                pointerDownListener.SetDelegate(() =>
                {
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("DIAMONDS"), I2.Loc.ScriptLocalization.Get("DIAMONDS_DESCRIPTION"));
                });
                break;
            case BubbleBoyPrizeType.Medicine:
                pointerDownListener.SetDelegate(() =>
                {
                    TextTooltip.Open(((BubbleBoyRewardMedicine)reward).GetMedicineRef(), false, true, false);
                });
                break;
            case BubbleBoyPrizeType.Decoration:
                switch (((BubbleBoyRewardDecoration)reward).GetShopRoomInfo().DrawerArea)
                {
                    case Hospital.HospitalAreaInDrawer.Clinic:
                            pointerDownListener.SetDelegate(() =>
                            {
                                TextTooltip.Open((I2.Loc.ScriptLocalization.Get(((BubbleBoyRewardDecoration)reward).GetShopRoomInfo().ShopTitle)), I2.Loc.ScriptLocalization.Get("SHOP_DESCRIPTIONS/CLINIC_DECORATION_INFO"));
                            });
                        break;
                    case Hospital.HospitalAreaInDrawer.Laboratory:
                            pointerDownListener.SetDelegate(() =>
                            {
                                TextTooltip.Open((I2.Loc.ScriptLocalization.Get(((BubbleBoyRewardDecoration)reward).GetShopRoomInfo().ShopTitle)), I2.Loc.ScriptLocalization.Get("SHOP_DESCRIPTIONS/LAB_DECORATION_INFO"));
                            });
                        break;
                    case Hospital.HospitalAreaInDrawer.Patio:
                            pointerDownListener.SetDelegate(() =>
                            {
                                TextTooltip.Open((I2.Loc.ScriptLocalization.Get(((BubbleBoyRewardDecoration)reward).GetShopRoomInfo().ShopTitle)), I2.Loc.ScriptLocalization.Get("SHOP_DESCRIPTIONS/PATIO_DECORATION_INFO"));
                            });
                        break;
                }
                break;
            case BubbleBoyPrizeType.Booster:
                pointerDownListener.SetDelegate(() =>
                {
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("BOOSTERS/BOOSTER"), I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[((BubbleBoyRewardBooster)reward).GetBoosterID()].shortInfo));
                });
        break;
        }
    }
}
