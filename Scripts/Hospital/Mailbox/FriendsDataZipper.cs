using Hospital;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;

public static class FriendsDataZipper
{
    public static int FriendsCount
    {
        private set;
        get;        
    }
    public static int InvitedFriends
    {
        private set;
        get;
    }
    public static int PendingFriends
    {
        private set;
        get;
    }

    private static Dictionary<string, IFollower> zippedFollowers = new Dictionary<string, IFollower>();
    private static Dictionary<string, IFollower> fbAndInGameFriends = new Dictionary<string, IFollower>();
    private static List<IFollower> zippedFbAndInGameFriendsList = new List<IFollower>();
    private static List<string> zippedIds;

    private static DrWiseFriend wiseFriend;

    private static InGameFriendsProvider inGameFriendsProvider;
    private static AccountManager fbprovider;
    private static LastHelpersProvider helperProvider;
    private static FriendsController friendsController;

    public static UnityAction onFriendsZipped = null;

    public static void Intialize()
    {
        inGameFriendsProvider = ReferenceHolder.Get().inGameFriendsProvider;
        fbprovider = AccountManager.Instance;
        helperProvider = LastHelpersProvider.Instance;
        friendsController = FriendsController.Instance;
    }

    public static bool CheckIfPresent(string saveID)
    {
        return fbAndInGameFriends.ContainsKey(saveID);
    }

    public static IFollower GetFriend(string saveID)
    {
        return fbAndInGameFriends[saveID];
    }

    public static List<IFollower> GetFbAndInGameFriends()
    {
        return zippedFbAndInGameFriendsList;
    }

    public static List<IFollower> GetFbAndIGFWithoutWise()
    {
        List<IFollower> listWithoutWise = new List<IFollower>(zippedFbAndInGameFriendsList);
        listWithoutWise.Remove(GetDrWise());
        return listWithoutWise;
    }

    public static void ZipFbInGameFriends()
    {
        fbAndInGameFriends.Clear();
        AddIfDontExist(fbAndInGameFriends, fbprovider.FbFriends);
        AddIfDontExist(fbAndInGameFriends, inGameFriendsProvider.GetInGameFriends());
        zippedFbAndInGameFriendsList = fbAndInGameFriends
            .Values
            .OrderByDescending(x => x.LastActivity)
            .ThenByDescending(x => x.Level)
            .ToList();
        InvitedFriends = ReferenceHolder.Get().inGameFriendsProvider.GetPendingInvitationsSendByMe().Count;
        PendingFriends = ReferenceHolder.Get().inGameFriendsProvider.GetPendingInvitationsSendByOthers().Count;
        FriendsCount = zippedFbAndInGameFriendsList.Count;
        zippedFbAndInGameFriendsList.Insert(0, GetDrWise());

        onFriendsZipped?.Invoke();
    }

    public static List<IFollower> ZipFbAndIgfWithHelpRequests()
    {
        return zippedFbAndInGameFriendsList           
            .Where(x => x.HasAnyHelpRequest)
            .OrderByDescending(x => x.LastActivity)
            .ThenByDescending(x => x.Level)
            .ToList();
    }

    public static List<IFollower> ZipFbInGameFriendsAndLiked()
    {
        zippedFollowers.Clear();
        AddIfDontExist(zippedFollowers, zippedFbAndInGameFriendsList);
        AddIfDontExist(zippedFollowers, friendsController.likedFollowers);
        return zippedFollowers
            .Values
            .OrderByDescending(x => x.LastActivity)
            .ThenByDescending(x => x.Level)
            .ToList();
    }

    public static List<IFollower> ReplaceStandardWithFacebookAndInGameFriends(List<IFollower> followers)
    {
        zippedFollowers.Clear();
        AddIfDontExist(zippedFollowers, followers);
        ReplaceIfExists(zippedFollowers, zippedFbAndInGameFriendsList);

        return zippedFollowers
            .Values
            .ToList();
    }

    public static List<IFollower> RemoveInGameFriendsFbfromLikes()
    {
        zippedFollowers.Clear();
        AddIfDontExist(zippedFollowers, friendsController.likedFollowers);
        RemoveIfExists(zippedFollowers, zippedFbAndInGameFriendsList);
        return zippedFollowers
            .Values
            .ToList();
    }

    public static List<string> RemoveFriendsAndFb(List<string> ids)
    {
        foreach(IFollower follower in zippedFbAndInGameFriendsList)
        {
            ids.Remove(follower.SaveID);
        }
        ids.CTShuffle();
        return ids.Take(DefaultConfigurationProvider.GetConfigCData().SocialRandomFriendsDisplayLimit).ToList<string>();
    }

    public static List<IFollower> FilterRandom(List<IFollower> followers)
    {
        zippedFollowers.Clear();
        AddIfDontExist(zippedFollowers, followers);
        RemoveIfExists(zippedFollowers, zippedFbAndInGameFriendsList);
        ReplaceIfExists(zippedFollowers, inGameFriendsProvider.GetPendingInvitationsSendByMe());
        return zippedFollowers
            .Values
            .ToList();

    }


    public static IFollower GetDrWise()
    {

        if (wiseFriend == null)
        {
            wiseFriend = new DrWiseFriend();
        }
        return wiseFriend;
    }

    private static void AddIfDontExist(Dictionary<string, IFollower> dict, List<IFollower> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].SaveID != null && !dict.ContainsKey(list[i].SaveID))
            {
                dict.Add(list[i].SaveID, list[i]);
            }
        }
    }

    private static void ReplaceIfExists(Dictionary<string, IFollower> dict, List<IFollower> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (dict.ContainsKey(list[i].SaveID))
            {
                dict[list[i].SaveID] = (list[i]);
            }
        }
    }

    private static void RemoveIfExists(Dictionary<string, IFollower> dict, List<IFollower> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (dict.ContainsKey(list[i].SaveID))
            {
                dict.Remove(list[i].SaveID);
            }
        }
    }
}
