using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SimpleUI;
using TMPro;

public class DailyDealConfirmationUI : UIElement
{
    [SerializeField]
    private RectTransform content = null;
    [SerializeField]
    private TextMeshProUGUI cost = null;
    [SerializeField]
    private GameObject itemPrefab = null;

    private int newPrice;
    private bool isTankStorageItem; 
    private int amount; 
    private Sprite itemImage;
    private Vector2 pos;

    public IEnumerator Open()
    {
        yield return base.Open(true, true);
        Initialize();
    }

    private void Initialize()
    {
        ClearContent();

        newPrice = Mathf.RoundToInt(ResourcesHolder.Get().GetDiamondPriceForCure(ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item) * ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Amount * ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Discount);
        cost.SetText(newPrice.ToString());

        GameObject temp = Instantiate(itemPrefab);
        temp.transform.SetParent(content);
        temp.transform.localScale = Vector3.one;
        
        temp.GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item);
        temp.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().SetText(ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Amount.ToString());

        isTankStorageItem = ResourcesHolder.Get().GetIsTankStorageCure(ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item);
        amount = ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Amount;
        itemImage = ResourcesHolder.Get().GetSpriteForCure(ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item);
        pos = new Vector2((content.gameObject.transform.position.x - Screen.width / 2) / UIController.get.transform.localScale.x, (content.gameObject.transform.position.y - Screen.height / 2) / UIController.get.transform.localScale.y);
    }

    private void ClearContent()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        base.Exit(hidePopupWithShowMainUI);
        transform.SetAsLastSibling();
    }

    public void ButtonBuy()
    {
        ReferenceHolder.GetHospital().dailyDealController.BuyDailyDealOffer(this);
    }

    public void DailyDealParticleForSpecialItem(GiftType giftType)
    {
        Invoke("ButtonExit", 0.1f);

        UIController.get.storageCounter.Add(isTankStorageItem, true);

        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(giftType, pos, amount, 0, 1.0f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), itemImage, null, () =>
        {
            UIController.get.storageCounter.Remove(amount, isTankStorageItem);
            //Exit();
        });
        
    }    

    public void ButtonExit()
    {
        Exit();
    }
}
