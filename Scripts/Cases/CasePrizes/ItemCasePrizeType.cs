using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCasePrizeType
{
    public MedicineRef item = null;
    public int amount = 0;
    public ItemCasePrizeType(MedicineRef item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}
