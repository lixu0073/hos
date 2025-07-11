using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using MovementEffects;
using Hospital;

[System.Serializable]
public class BubbleBoyRewardUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] Image rewardIcon;
    [SerializeField] TextMeshProUGUI rewardAmountText;
    [SerializeField] Animator animator;
    [SerializeField] Button button;
	[SerializeField] ParticleSystem particles;
	[SerializeField] ParticleSystem cureReadyParticles;
#pragma warning restore 0649
    public ItemState rewardUIState = ItemState.New;
    private BubbleBoyReward reward;
    

    public void Setup(BubbleBoyReward reward, bool canBeCollected = true, bool shufle = false, bool canInteractableBeSet = false)
    {
        this.reward = reward;

        if (this.reward != null)
        {
            rewardIcon.sprite = reward.sprite;
            rewardAmountText.text = reward.amount.ToString();

            if (reward.collected)
            {
                gameObject.SetActive(false);
                rewardUIState = ItemState.Collected;
            }
            else
            {
                gameObject.SetActive(true);
                rewardUIState = shufle ? ItemState.Invisible : ItemState.New;
            }
        }
        else
        {
            gameObject.SetActive(false);
            rewardUIState = ItemState.Collected;
        }

        reward.collected = !canBeCollected;

        if (canInteractableBeSet)
            button.interactable = canBeCollected;
    }

    public BubbleBoyReward GetReward()
    {
        return reward;
    }

    public void CollectReward()
    {
        if (rewardUIState == ItemState.Visible)
        {
            button.interactable = !reward.collected;

            if (BubbleBoyDataSynchronizer.Instance.BubbleOpened < 7)
                UIController.getHospital.bubbleBoyMinigameUI.EnableExitAndPlayAgain();

            //SoundsController.Instance.PlayReward();
			SoundsController.Instance.PlayShortBubble ();
            Timing.RunCoroutine(DelayedCollect());
        }

        if (rewardUIState == ItemState.New || rewardUIState == ItemState.Invisible)
        {
            UIController.getHospital.bubbleBoyMinigameUI.DisableAllBubblesActive();
			UIController.getHospital.bubbleBoyMinigameUI.PlayRewardBubbles ();
            button.interactable = true;
            reward.Collect();

            if (reward.rewardType == BubbleBoyPrizeType.Medicine)
                HospitalAreasMapController.HospitalMap.hospitalBedController.UpdateAllBedsIndicators(true);

            animator.SetTrigger("OnTap");
            rewardUIState = ItemState.Visible;
            UIController.getHospital.bubbleBoyMinigameUI.SetInfoText(I2.Loc.ScriptLocalization.Get("BUBBLE_BOY_MINIGAME_CLAIM_REWARD"), true);
			SoundsController.Instance.PlayPopUp();
        }
    }

    public void ShowMark()
    {
        button.interactable = false;
        rewardUIState = ItemState.Invisible;
        if (!reward.collected)
            animator.SetTrigger("ShowMark");
    }

    public void DisableButton()
    {
        button.interactable = false;
    }


    public void ShowItem()
    {
        if (!reward.collected)
            animator.SetTrigger("ShowItem");
    }

    public void IdleState()
    {
        Timing.KillCoroutine(DelayedCollect().GetType());
        button.interactable = false;
        animator.SetTrigger("IdleItem");
    }

    void OnDestroy()
    {
        Timing.KillCoroutine(DelayedCollect().GetType());
        UIController.getHospital.bubbleBoyMinigameUI.EnableExitAndPlayAgain();
    }

    public void ShowTooltip() {
        if (rewardUIState != ItemState.New)        
            return;

        switch (reward.rewardType)
        {
            case BubbleBoyPrizeType.Coin:
                TextTooltip.Open(I2.Loc.ScriptLocalization.Get("COINS"), I2.Loc.ScriptLocalization.Get("COINS_DESCRIPTION"));
                break;
            case BubbleBoyPrizeType.Diamond:
                TextTooltip.Open(I2.Loc.ScriptLocalization.Get("DIAMONDS"), I2.Loc.ScriptLocalization.Get("DIAMONDS_DESCRIPTION"));
                break;
            case BubbleBoyPrizeType.Medicine:
                TextTooltip.Open(((BubbleBoyRewardMedicine)reward).GetMedicineRef(), false, true, false);
                break;
            case BubbleBoyPrizeType.Decoration:
                switch (((BubbleBoyRewardDecoration)reward).GetShopRoomInfo().DrawerArea)
                {
                    case Hospital.HospitalAreaInDrawer.Clinic:
                        TextTooltip.Open((I2.Loc.ScriptLocalization.Get(((BubbleBoyRewardDecoration)reward).GetShopRoomInfo().ShopTitle)), I2.Loc.ScriptLocalization.Get("SHOP_DESCRIPTIONS/CLINIC_DECORATION_INFO"));
                        break;
                    case Hospital.HospitalAreaInDrawer.Laboratory:
                        TextTooltip.Open((I2.Loc.ScriptLocalization.Get(((BubbleBoyRewardDecoration)reward).GetShopRoomInfo().ShopTitle)), I2.Loc.ScriptLocalization.Get("SHOP_DESCRIPTIONS/LAB_DECORATION_INFO"));
                        break;
                    case Hospital.HospitalAreaInDrawer.Patio:
                        TextTooltip.Open((I2.Loc.ScriptLocalization.Get(((BubbleBoyRewardDecoration)reward).GetShopRoomInfo().ShopTitle)), I2.Loc.ScriptLocalization.Get("SHOP_DESCRIPTIONS/PATIO_DECORATION_INFO"));
                        break;
                }
                break;
            case BubbleBoyPrizeType.Booster:
                TextTooltip.Open(I2.Loc.ScriptLocalization.Get("BOOSTERS/BOOSTER"), I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[((BubbleBoyRewardBooster)reward).GetBoosterID()].shortInfo));
                break;            
        }       
    }

    public void HideTooltip() { }

    IEnumerator<float> DelayedCollect()
    {
        animator.SetTrigger("OnCollect");
		particles.Play ();

        Canvas canvas = UIController.get.canvas;
        Vector2 startPoint = new Vector2((gameObject.transform.position.x - Screen.width / 2) / canvas.transform.localScale.x, (gameObject.transform.position.y - Screen.height / 2) / canvas.transform.localScale.y);
        reward.SpawnParticle(startPoint);

		yield return Timing.WaitForSeconds (0.15f);
           
		if (HospitalBedController.isNewCureAvailable)
        {
            SoundsController.Instance.PlayAlert();
			cureReadyParticles.Play ();
            Debug.LogError("DING DING DING DING");
        }

		HospitalBedController.isNewCureAvailable = false;

        yield return Timing.WaitForSeconds(1f);
        gameObject.SetActive(false);

        if (BubbleBoyDataSynchronizer.Instance.BubbleOpened >= 7)
            UIController.getHospital.bubbleBoyEntryOverlayUI.ButtonExit();

        yield return 0;
    }

	public void PointerDownAnimation()
	{
		animator.SetTrigger ("PointerDown");
	}

	public void PointerExitAnimation()
	{
		animator.SetTrigger ("PointerExit");
	}

    public enum ItemState
    {
        New,
        Visible,
        Collected,
        Invisible,
    }
}
