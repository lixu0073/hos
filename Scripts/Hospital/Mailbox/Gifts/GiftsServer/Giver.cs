using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Giver : BaseUserModel
{

    private GiftModel giftModel = null;
    public GiftReward reward = null;

    public Giver(string SaveID, GiftModel giftModel) : base(SaveID)
    {
        this.giftModel = giftModel;
    }

    public string GetGiftId()
    {
        return giftModel.ID;
    }

    public bool IsThankYouGift()
    {
        return giftModel.IsThankYouGift;
    }

}
