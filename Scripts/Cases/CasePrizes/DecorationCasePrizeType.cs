using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationCasePrizeType
{
    public ShopRoomInfo decoration = null;
    public int amount = 0;
    public DecorationCasePrizeType(ShopRoomInfo decoration, int amount)
    {
        this.decoration = decoration;
        this.amount = amount;
    }
}
