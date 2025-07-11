using UnityEngine;
using Hospital;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMainScale : MonoBehaviour
{
    public Transform xpBar;
    public Transform counters;
    public Transform[] buttons;
    public Transform friends;
    public Transform drawerShop;

    public RectTransform hintAnchor;
    public RectTransform boosterAnchor;
    public RectTransform giftboxAnchor;
    public RectTransform dailyQuestsAnchor;
    public RectTransform starterPackAnchor;
    public RectTransform timedOfferAnchor;
    public RectTransform eventAnchor;
    public RectTransform objectivesAnchor;
    public RectTransform reputationAnchor;

    public Image IPhoneXSpecialOfferButtonImage;
    public Image IPhoneXTimedOfferButtonImage;
    public Sprite IPhoneXSpecialOfferSprite;

    private int GiftsBadgeUnlockLevel = 8;
#pragma warning disable 0649
    [SerializeField]
    private GameObject GiftBadgeGameObject;
#pragma warning restore 0649
    [TutorialTriggerable] public void ShowDrawerButton() { drawerShop.gameObject.SetActive(true); }

    void Awake()
    {
        if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
        {
            try
            {
                ScaleForPhone();
            }
            catch (Exception e)
            {
                Debug.Log("what? Scaling crash: " + e.Message);
            }
        }
        //else
        //Debug.Log("NOT A PHONE. NOT SCALING THE UI");
    }

    void Start()
    {
        GiftsSendController.onUpdate += GiftsSendController_onUpdate;
        FriendsController.updateGiftsBadge += GiftsSendController_onUpdate;
        AccountManager.updateGiftsBadge += GiftsSendController_onUpdate;
        FriendsDataZipper.onFriendsZipped += GiftsSendController_onUpdate;

        if (ExtendedCanvasScaler.HasNotch())
        {
            SetUIForIphoneX();
        }
        IMainUIAdapter mainUIAdapter = ObjectFactory.GetMainUIAdapter();
        if (mainUIAdapter != null)
            mainUIAdapter.Excucute(this);

        if (SceneManager.GetActiveScene().name != "MaternityScene")
            drawerShop.gameObject.SetActive(false);
    }

    private void GiftsSendController_onUpdate()
    {
        GiftsSendController_onUpdate(null);
    }

    private void GiftsSendController_onUpdate(string saveID)
    {
        bool hasEnoughGifts = GiftsSendController.Instance.GetAvailableGifts() > 0;
        bool canGIveGiftToFriend = GiftsSendController.Instance.AreThereAnyFriendsToGiveGift();
        bool levelIsOk = Game.Instance.gameState().GetHospitalLevel() >= GiftsBadgeUnlockLevel;
        GiftBadgeGameObject.SetActive(hasEnoughGifts && canGIveGiftToFriend && levelIsOk);
    }

    void OnDestroy()
    {
        GiftsSendController.onUpdate -= GiftsSendController_onUpdate;
        FriendsController.updateGiftsBadge -= GiftsSendController_onUpdate;
        AccountManager.updateGiftsBadge -= GiftsSendController_onUpdate;
        FriendsDataZipper.onFriendsZipped -= GiftsSendController_onUpdate;
    }

    void ScaleForPhone()
    {
        Vector3 targetScaleDefault = new Vector3(1.4f, 1.4f, 1f);
        Vector3 targetScaleSmaller = new Vector3(1.25f, 1.25f, 1f);

        try
        {
            xpBar.localScale = targetScaleDefault;
            counters.localScale = targetScaleDefault;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].localScale.x < 0)
                    buttons[i].localScale = new Vector3(-targetScaleDefault.x, targetScaleDefault.y, targetScaleDefault.z);
                else                
                    buttons[i].localScale = targetScaleDefault;
            }

            friends.localScale = targetScaleSmaller;
            drawerShop.localScale = targetScaleSmaller;
            hintAnchor.localScale = targetScaleSmaller;
            boosterAnchor.localScale = targetScaleSmaller;
            giftboxAnchor.localScale = targetScaleSmaller;
            starterPackAnchor.localScale = targetScaleSmaller;
            timedOfferAnchor.localScale = targetScaleSmaller;
            eventAnchor.localScale = Vector3.one;
            reputationAnchor.localScale = targetScaleSmaller;

            hintAnchor.anchorMin = new Vector2(0, .5f);
            hintAnchor.anchorMax = new Vector2(0, .5f);
            hintAnchor.anchoredPosition = new Vector2(0, 110f);

            starterPackAnchor.anchorMin = new Vector2(1, .65f);
            starterPackAnchor.anchorMax = new Vector2(1, .65f);
            starterPackAnchor.anchoredPosition = Vector2.zero;
     
            timedOfferAnchor.anchorMin = new Vector2(1, .65f);
            timedOfferAnchor.anchorMax = new Vector2(1, .65f);
            timedOfferAnchor.anchoredPosition = Vector2.zero;
       
            boosterAnchor.anchorMin = new Vector2(1, .475f);
            boosterAnchor.anchorMax = new Vector2(1, .475f);
            boosterAnchor.anchoredPosition = Vector2.zero;
      
            giftboxAnchor.anchorMin = new Vector2(1, .3f);
            giftboxAnchor.anchorMax = new Vector2(1, .3f);
            giftboxAnchor.anchoredPosition = Vector2.zero;
        
            dailyQuestsAnchor.anchorMin = new Vector2(0, .3f);
            dailyQuestsAnchor.anchorMax = new Vector2(0, .3f);
            dailyQuestsAnchor.anchoredPosition = Vector2.zero;
       
            objectivesAnchor.anchorMin = new Vector2(0, .5f);
            objectivesAnchor.anchorMin = new Vector2(0, .5f);
            objectivesAnchor.anchoredPosition = new Vector2(0, 72.5f);
        }
        catch (Exception e)
        {
            Debug.Log("what? Scaling crash: " + e.Message);
        }
    }

    private void SetUIForIphoneX()
    {
        RectTransform thisRecTransform = gameObject.GetComponent<RectTransform>();
        //These are old values
        //thisRecTransform.offsetMin = new Vector2(46.5f, 31.586f); //left bottom
        //thisRecTransform.offsetMax = new Vector2(-46.5f, -0f); // -right -top

        thisRecTransform.offsetMin = new Vector2(37.2f, 31.586f); //left bottom
        thisRecTransform.offsetMax = new Vector2(-35f, -7f); // -right -top
        IPhoneXSpecialOfferButtonImage.sprite = IPhoneXSpecialOfferSprite;
        IPhoneXTimedOfferButtonImage.sprite = IPhoneXSpecialOfferSprite;
        buttons[0].localScale = new Vector3(1.4f, 1.4f, 1);
    }

    public void CheckButtons()  //triggered from animator
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "MainScene")
        {
            ReferenceHolder.Get().giftSystem.SetUIElementsPos();
            ((HospitalCasesManager)AreaMapController.Map.casesManager).CheckAlert();
        }
    }
}
