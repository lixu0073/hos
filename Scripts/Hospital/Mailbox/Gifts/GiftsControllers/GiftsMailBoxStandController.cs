using Hospital;
using System.Collections.Generic;
using UnityEngine;

public class GiftsMailBoxStandController : MonoBehaviour
{
    public enum MailBoxState
    {
        Empty,
        Fill,
        MaxFull
    }

    #region private Serialized fields
#pragma warning disable 0649
    [SerializeField] private GiftsMailBoxStandObject mailGiftBoxObject;
#pragma warning restore 0649
    #endregion

    #region private fields
    private MailBoxState state;
    private int currentGiftCount = 0;
    private InGameFriendsProvider igfProvider;
    private bool areAnyFriendsRequest = false;
    #endregion

    private void Start()
    {
        igfProvider = ReferenceHolder.Get().inGameFriendsProvider;
        GiftsReceiveController.onGiversUpdate += OnGiftUpdate;
        InGameFriendsProvider.OnInGameFriendsChange += OnInGameFriendsChange;

        GameState.OnLevelUp -= GameState_OnLevelUp;
        if (!GiftsAPI.Instance.IsFeatureUnlocked())
            GameState.OnLevelUp += GameState_OnLevelUp;
    }

    private void GameState_OnLevelUp()
    {
        if (GiftsAPI.Instance.IsFeatureUnlocked())
        {
            GiftsReceiveController.Instance.FetchGifts();
            GameState.OnLevelUp -= GameState_OnLevelUp;
        }
    }

    private void OnGiftUpdate(List<Giver> gifts)
    {
        currentGiftCount = gifts.Count;
        MailboxChange();
    }

    private void OnInGameFriendsChange()
    {
        areAnyFriendsRequest = igfProvider.GetPendingInvitationsSendByOthers().Count > 0;
        MailboxChange();
    }

    private void MailboxChange()
    {
        bool isSomthink = IswisGiftAble() || areAnyFriendsRequest;
        UIController.get.FriendsDrawer.SetBadgeNewNotification(isSomthink);
        UIController.getHospital.giftUI.SetNotificationBadge(IswisGiftAble());

        if (!isSomthink)
        {
            state = MailBoxState.Empty;
        }
        else if (currentGiftCount == GiftsAPI.Instance.MaxGifts)
        {
            state = MailBoxState.MaxFull;
        }
        else
        {
            state = MailBoxState.Fill;
        }
        SetupMailBoxGameObject();
    }

    private bool IswisGiftAble()
    {
        return (currentGiftCount != 0 || GiftsReceiveController.Instance.ShouldAddGiftFromWise())
            && GiftsAPI.Instance.IsFeatureUnlocked() ;
    }

    private void SetupMailBoxGameObject()
    {
        mailGiftBoxObject.UpdateMailBox(state);
    }

    private void OnDestroy()
    {
        GiftsReceiveController.onGiversUpdate -= OnGiftUpdate;
        GameState.OnLevelUp -= GameState_OnLevelUp;
        InGameFriendsProvider.OnInGameFriendsChange -= OnInGameFriendsChange;
    }
}
