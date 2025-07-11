using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;

public class FriendCardController : BaseFriendCardController
{
    protected override void AddAdditionalBehaviours(IFollower person)
    {
        FriendCardUI card = gameObject.GetComponent<FriendCardUI>();
        card.SetFriendIDAndAddListener(person.GetSaveID());
        card.RefreshGiftStatus();
        card.giftButton.onClick.RemoveAllListeners();
        card.giftButton.onClick.AddListener(() =>
        {
            if (!GiftsAPI.Instance.IsFeatureUnlocked())
            {
                ShowGiftsLockedNotification();
                return;
            }
            if (GiftsSendController.Instance.CanSendGiftToFriend(person.GetSaveID()))
            {
                card.SetAnimateGift(true);
                GiftsSendController.Instance.OnSuccessGiftSendToFriend(person.GetSaveID());
                string message = "";
                if (GetAvailableGiftsAmount() > 0)
                {
                    message = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFT_FLOAT_AMOUNT").Replace("{0}", GetAvailableGiftsAmount().ToString());
                }
                else
                {
                    message = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFTS_WAIT_TO_SEND").Replace("{0}", GetFormatedTimeToNextGift());
                }
                MessageController.instance.ShowMessage(message);

                GiftsAPI.Instance.SendGift(person.GetSaveID(), () =>
                {
                    Debug.Log("Success gift sended to: " + person.GetSaveID());

                }, (ex) =>
                {
                    Debug.LogError("Failure gift send: " + ex.Message);
                });
            }
            else
            {
                ShowNoGiftsToSendNotification();
            }
        });
    }

    private void ShowGiftsLockedNotification()
    {
        string message = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFT_FLOAT_SEND").Replace("{0}", GiftsAPI.Instance.GiftsFeatureMinLevel.ToString());
        MessageController.instance.ShowMessage(message);
        Debug.LogError("Za maly level");
    }

    private void ShowNoGiftsToSendNotification()
    {
        string message = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFTS_WAIT_TO_SEND").Replace("{0}", GetFormatedTimeToNextGift());
        MessageController.instance.ShowMessage(message);
        Debug.LogError("Nie ma dostępnych prezentów");
    }

    private string GetFormatedTimeToNextGift()
    {
        int time = (int)Mathf.Max(GiftsSendController.Instance.GetDurationInSecondsToNextGift(), 0);
        return UIController.GetFormattedTime(time);
    }

    private int GetAvailableGiftsAmount()
    {
        return GiftsSendController.Instance.GetAvailableGifts();
    }

    protected override void OnVisiting() { }
}
