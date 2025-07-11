using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GiftRewardStrategies
{
    public abstract GiftReward GetGiftReward(GiftRewardType giftType);
}
