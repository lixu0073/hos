using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Hospital;

public class GELeaderboardEntryData
{
    public GELeaderboardEntryViewStrategy strategy = null;

    public IFollower contributor = null;
    public int score = 0;
    public UnityAction onAvatarClicked = null;
    public Sprite bgSprite = null;
    public Sprite contributionItemSprite = null;
    public Sprite prizeSprite = null;
}
