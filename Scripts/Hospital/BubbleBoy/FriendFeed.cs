using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleUI;
using TMPro;
using MovementEffects;
using System;
using Hospital;

[System.Serializable]
public class FriendFeed
{
    public Sprite avatar;
    public Sprite frame;
    public int level;
    public string itemname;
    public string name;

    public FriendFeed(IFollower friend)
    {
        this.avatar = friend.Avatar;
        this.level = friend.Level;
        this.name = friend.Name;
        this.frame = friend.GetFrame();
        var item = friend.Reward;
        if (item != null)
        {
            this.itemname = item.GetName();
        }
    }

    public FriendFeed(int id = 0)
    {
        this.avatar = ResourcesHolder.Get().friendsDefaultAvatar;
        this.frame = ResourcesHolder.GetHospital().frameData.basicFrame;

        this.name = I2.Loc.ScriptLocalization.Get("STRANGER");
        this.level = GameState.RandomNumber(6, 40);

        int rnd = GameState.RandomNumber(100);
        if (rnd < 25)
            this.itemname = I2.Loc.ScriptLocalization.Get("COINS");
        else if ((rnd >= 25) && (rnd < 50))
              this.itemname = I2.Loc.ScriptLocalization.Get("DIAMONDS");
        else if ((rnd >= 50) && (rnd < 75))
            this.itemname = I2.Loc.ScriptLocalization.Get("DIAMONDS");
        else
        {
            int indx = GameState.RandomNumber(0, ResourcesHolder.Get().boosterDatabase.boosters.Length-2);
            this.itemname = I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[indx].shortInfo);
        }
    }

    public Sprite GetAvatar()
    {
        return this.avatar;
    }

    public int GetLevel()
    {
        return this.level;
    }

    public string GetItemName()
    {
        return this.itemname;
    }

    public string GetName()
    {
        return this.name;
    }

    public Sprite GetFrame()
    {
        return frame;
    }
}