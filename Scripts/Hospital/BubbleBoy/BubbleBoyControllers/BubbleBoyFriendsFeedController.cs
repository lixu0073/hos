using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleUI;
using TMPro;
using MovementEffects;
using System;
using Hospital;


public class BubbleBoyFriendsFeedController : MonoBehaviour {

    private List<FriendFeed> friendsFeedList = new List<FriendFeed>();

    public int timeToNextFeed = 90;
    public int friendsFeedSize = 3;
    public int currentFeedVisibleID = 0;

    public void InitFriendsFeed(List<IFollower> FbFriends)
    {
        if (friendsFeedList != null && friendsFeedList.Count > 0)
            friendsFeedList.Clear();
        int added = 0;

        if (FbFriends != null && FbFriends.Count > 0)
        {
            for (int i = 0; i < FbFriends.Count; i++) // SET FRIENDS FROM FACEBOOK
            {
                var tmp = FbFriends[i].Reward;
                if (tmp != null && CanBeAddedDependsOfTime(tmp.expireTime))
                {
                    friendsFeedList.Add(new FriendFeed(FbFriends[i]));
                    added++;
                    if (added > 2) break;
                }
            }

            if (added < friendsFeedSize) // FILL MISSING FRIENDS WITH GENERATED ONES
            {
                for (int i = 0; i < friendsFeedSize - added; i++)
                {
                        friendsFeedList.Add(new FriendFeed(i));
                }
            }
        }
        else // GENERATE FRIENDS
        {
            for (int i = 0; i < friendsFeedSize; i++)
                friendsFeedList.Add(new FriendFeed(i));
        }

        currentFeedVisibleID = BaseGameState.RandomNumber(0, friendsFeedSize);
    }

    private bool CanBeAddedDependsOfTime(int time)
    {
        int timePassed = time - Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
        if (timePassed <= 0)
            return false;
        else return true;
    }

    public FriendFeed GetCurrentFriendFeed()
    {
        return friendsFeedList[currentFeedVisibleID];
    }

}
