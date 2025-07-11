using System.Collections.Generic;
using System;

public class EventEndedPopupInitializer : BaseUIInitializer<EventEndedPopupData, EventEndedPopupController>
{
    public void Initialize(Action OnSuccess, Action OnFailure, bool openFromCode)
    {
        base.Initialize(OnSuccess, OnFailure);

        if (openFromCode)
        {
            if (UIController.get.ActivePopUps.Count > 0 || TutorialUIController.Instance.IsFullscreenActive())
                return;
        }

        EventEndedPopupData data = PreparePopupData();

        if (data != null)
        {
            popupController.GetPopup().Open().MoveNext();
            popupController.Initialize(data);

            OnSuccess?.Invoke();
        }
        else
        {
            OnFailure?.Invoke();
        }
    }

    protected override void AddPopupControllerRuntime()
    {
        popupController = gameObject.GetComponent<EventEndedPopupController>();
    }

    protected override EventEndedPopupData PreparePopupData()
    {
        EventEndedPopupData data = new EventEndedPopupData();

        List<KeyValuePair<string, GlobalEventRewardModel>> rewards = GlobalEventSynchronizer.Instance.GetGlobalEventRewardForReloadSpawn();

        if (rewards == null || rewards.Count < 1 || rewards[rewards.Count - 1].Value == null)
        {
            return null;
        }

        KeyValuePair<string, GlobalEventRewardModel> rewardToShow = rewards[rewards.Count - 1];

        data.prizeSprite = rewardToShow.Value.GetGlobalEventGift().GetMainImageForGift();

        data.onClaimButtonClick = OnClaimButtonAction;

        if (rewardToShow.Value.GetGlobalEventGift().rewardType == BaseGiftableResourceFactory.BaseResroucesType.bundle)
        {
            data.strategy = new BundleRewardEventEndedPopupViewStrategy();
        }
        else
        {
            data.strategy = new SingleRewardEventEndedPopupViewStrategy();
            data.rewardAmount = rewardToShow.Value.GetGlobalEventGift().GetGiftAmount();
        }

        GlobalEventAPI.GetTopContributorsForPreviousEvent(rewardToShow.Key, (contributors) =>
        {
            if (contributors == null || contributors.Count < 1)
            {
                return;
            }

            Contributor playerContributor = contributors.Find((x) => x.GetSaveID() == SaveLoadController.SaveState.ID);
            if (playerContributor == null)
            {
                return;
            }

            data.playerScore = playerContributor.score;

            contributors.Sort((x, y) => y.score.CompareTo(x.score));

            data.playerPosition = contributors.FindIndex((x) => x.score == playerContributor.score) + 1;

            Refresh(data);
        }, null, DefaultConfigurationProvider.GetGlobalEventsCData().LeaderboardLength);

        return data;
    }

    private void OnClaimButtonAction()
    {
        DeInitialize();

        if (popupController != null)
        {
            popupController.GetPopup().Exit();
        }

        UIController.getHospital.unboxingPopUp.OpenGlobalEventPrize();
    }

    protected override void Refresh(EventEndedPopupData dataType)
    {
        popupController.RefreshDataWhileOpened(dataType);
    }
}
