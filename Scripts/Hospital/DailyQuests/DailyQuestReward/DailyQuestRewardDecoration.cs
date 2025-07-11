using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyQuestRewardDecoration : DailyQuestReward
{

    ShopRoomInfo decoration;

    public DailyQuestRewardDecoration(int amount) : base(amount)
    {
        decoration = drawRandomDecoration();
        if (decoration == null)
        {
            decoration = ((HospitalCasesManager)AreaMapController.Map.casesManager).decorationsToDraw[0]; // outdoor flower 2 color 1 (required 4 lvl)
            Debug.LogError("Decoration has not been drawn properly. Therefore simple decoration will be selected. It is assumed that first decoration form the list lowest level required");
        }
        rewardType = DailyQuestRewardType.Decoration;
    }

    private ShopRoomInfo drawRandomDecoration()
    {
        ShopRoomInfo decoToReturn = null;
        List<ShopRoomInfo> decorationsToDraw = ((HospitalCasesManager)AreaMapController.Map.casesManager).decorationsToDraw;
        List<ShopRoomInfo> infos = new List<ShopRoomInfo>();
        for (int i = 0; i < decorationsToDraw.Count; i++)
        {
            if (!(decorationsToDraw[i].unlockLVL > Game.Instance.gameState().GetHospitalLevel()) && decorationsToDraw[i].GetType() == typeof(DecorationInfo))
            {
                infos.Add(decorationsToDraw[i]);
            }
        }

        if (infos.Count > 0)
        {
            int randomIndex = GameState.RandomNumber(0, infos.Count);
            decoToReturn = infos[randomIndex];
        }
        return decoToReturn;
    }

    public override void Collect()
    {
        if (decoration != null)
        {
            GameState.Get().AddToObjectStored(decoration, 1);
        }
    }

    public ShopRoomInfo GetDecoration()
    {
        return decoration;
    }
}
