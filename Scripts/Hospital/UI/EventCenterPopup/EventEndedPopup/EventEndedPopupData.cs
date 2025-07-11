using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventEndedPopupData
{
    public EventEndedPopupViewStrategy strategy = null; //

    public int playerScore = 0;
    public int playerPosition = 0;
    public int rewardAmount = 0; //
    public Sprite prizeSprite = null; //
    public UnityAction onClaimButtonClick = null; //
}
