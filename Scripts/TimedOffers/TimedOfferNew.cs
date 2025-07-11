using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedOfferNew
{
    //TODO
    //public DecisionPointCalss dpc = null;

    public bool isTickerOnPopup = false;
    public string decisionPoint;
    public int timedOfferEndDate;
    public int offerOrderNo;
    public int timedOfferPriority = -1;

    public List<string> iapProducts;
    public Coroutine endOfferCoroutine = null;
}
