using UnityEngine;
using UnityEngine.UI;
using SimpleUI;
using TMPro;

public class LabourDayPopUpContent : MonoBehaviour
{
    public ShopRoomInfo LabourDayDecoration1;
    public ShopRoomInfo LabourDayDecoration2;

    public TextMeshProUGUI availableCount;
    public TextMeshProUGUI priceText;

    [SerializeField] private Image decoImage1 = null;
    [SerializeField] private Image decoImage2 = null;

    private Vector2 pos;
   //private Sprite loadedSignSprtite = null;    

    void OnEnable()
    {
        SetPrice();
        SetAvailableInfo();
    }

    void OnDisable() { }

    void SetPrice()
    {
        priceText.text = IAPController.instance.GetLabourDayPrize();
    }

    public void SetAvailableInfo()
    {
        int availableAmount = 3 - GameState.Get().IAPLabourDayCount;
        if (availableAmount < 0)
            availableAmount = 0;

        availableCount.text = I2.Loc.ScriptLocalization.Get("EVENTS/AVAILABLE") + " " + availableAmount.ToString();
    }

    public void ButtonIAP()
    {
        IAPController.instance.BuyProductID("labourday_1");
    }

    public void SpawnDecorationGiftParticle() {
        Vector2 pos1 = new Vector2((decoImage1.gameObject.transform.position.x - Screen.width / 2) / UIController.get.transform.localScale.x, (decoImage1.gameObject.transform.position.y - Screen.height / 2) / UIController.get.transform.localScale.y);
        Vector2 pos2 = new Vector2((decoImage2.gameObject.transform.position.x - Screen.width / 2) / UIController.get.transform.localScale.x, (decoImage2.gameObject.transform.position.y - Screen.height / 2) / UIController.get.transform.localScale.y);

        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, pos1, 1, 1.6f, 2f, new Vector3(4f, 4f, 1), new Vector3(2, 2, 1), LabourDayDecoration1.ShopImage, null, null);
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, pos2, 1, 0.8f, 2f, new Vector3(4f, 4f, 1), new Vector3(2, 2, 1), LabourDayDecoration2.ShopImage, null, null);
    }
}
