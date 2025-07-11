using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TresureChestDeltaConfig
{
    public static bool DiamondsFromTreasureChest = false;

    //private static string diamondsFromTreasureChest = "diamondsFromChest";


    public static void SetupDiamondFromTreasureChest(bool diamondsFromTreasureChest)
    {
        DiamondsFromTreasureChest = diamondsFromTreasureChest;
    }

    //private static int ExtractInt(string UnparsedData)
    //{
    //    string paramValueAsString = UnparsedData;
    //    int value = 0;
    //    Int32.TryParse(paramValueAsString, out value);
    //    return value;
    //}
}
