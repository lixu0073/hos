using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterItemCasePrizeType
{
    public int boosterID = -1;
    public int amount = 0;
    public BoosterItemCasePrizeType(int boosterID, int amount)
    {
        this.boosterID = boosterID;
        this.amount = amount;
    }
}
