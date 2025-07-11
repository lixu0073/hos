using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contributor : BaseUserModel
{
    public FriendContributor.FriendStatus friendStatus;
    public int score = 0;

    public Contributor(string SaveID, int score) : base(SaveID)
    {
        this.score = score;
    }

    public bool IsFaked()
    {
        return string.IsNullOrEmpty(GetSaveID());
    }

    public int GetScore()
    {
        return score;
    }

}
