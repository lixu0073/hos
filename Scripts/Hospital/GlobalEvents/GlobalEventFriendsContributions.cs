using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Hospital;
using System.Linq;

[System.Serializable]
public class FriendContributor
{
    public enum ContributionRelation
    {
        More,
        Less,
        Same
    }

    public enum FriendStatus
    {
        NotFriend, InGameFriend, FacebookFriend
    }

    public IFollower Friend { get; private set; }
    public Contributor contributor;
    [SerializeField] private string name;
    [SerializeField] private string saveID;
    [SerializeField] private int contribution;
    [SerializeField] private ContributionRelation relation;

    public FriendStatus friendStatus = FriendStatus.NotFriend;

    public int Score
    { get { return contribution; } }
    public string SaveID { get { return saveID; } }
    public ContributionRelation Relation { get { return relation; } }

    public FriendContributor(IFollower _friend, string _name, string _saveID)
    {
        Friend = _friend;
        name = _name;
        saveID = _saveID;
        contribution = 0;
        relation = ContributionRelation.Same;
    }

    public void SetContributionWithRelation(int _contribution, int referenceContribution)
    {
        contribution = _contribution;

        if (contribution > referenceContribution) relation = ContributionRelation.More;
        else if (contribution < referenceContribution) relation = ContributionRelation.Less;
        else if (contribution == referenceContribution) relation = ContributionRelation.Same;
    }
}

public class GlobalEventFriendsContributions : MonoBehaviour
{
    #region static

    private static GlobalEventFriendsContributions instance;

    public static GlobalEventFriendsContributions Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of GlobalEventFriendsContributions was found on scene!");
            return instance;
        }

    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of GlobalEventFriendsContributions entrypoint were found!");
        }
        instance = this;
    }

    #endregion

    [SerializeField] private string globalEventID;

    [Space(10)]
    [SerializeField] private int playerContribution;
    [SerializeField] private List<FriendContributor> friendsAndPlayerContributions;

    public List<FriendContributor> FriendsAndPlayerContributions
    {
        get { return friendsAndPlayerContributions; }
    }

    public void UpdateContributionData(GlobalEventAPI.OnSuccessContributorsGet onSucces = null)
    {
        GetFriendsData();
        GetContributions(() =>
        {
            //friendsAndPlayerContributions.RemoveAll((x) => x.contributor.score == 0);

            List<Contributor> contributors = new List<Contributor>();

            for (int i = 0; i < friendsAndPlayerContributions.Count; ++i)
            {
                if (friendsAndPlayerContributions[i].contributor == null)
                    contributors.Add(new Contributor(friendsAndPlayerContributions[i].SaveID, friendsAndPlayerContributions[i].Score));
                else
                    contributors.Add(friendsAndPlayerContributions[i].contributor);

                contributors[contributors.Count - 1].friendStatus = friendsAndPlayerContributions[i].friendStatus;
            }

            GlobalEventAPI.BindPublicSaves(contributors, onSucces, null);
        });
    }

    private void GetFriendsData()
    {
        if (friendsAndPlayerContributions == null)
            friendsAndPlayerContributions = new List<FriendContributor>();
        friendsAndPlayerContributions.Clear();
        List<IFollower> friends = FriendsDataZipper.GetFbAndIGFWithoutWise();

        Contributor player = new Contributor(SaveLoadController.SaveState.ID, 0);
        player.Name = SaveLoadController.SaveState.HospitalName;
        player.Level = SaveLoadController.SaveState.Level;

        friends.Add(player);

        foreach (IFollower friend in friends)
        {
            FriendContributor fc = new FriendContributor(friend, friend.Name, friend.SaveID);
            if (AccountManager.Instance.FbFriends
                    .Any(
                    x =>
                    x.SaveID == friend.SaveID))
                fc.friendStatus = FriendContributor.FriendStatus.FacebookFriend;
            else if (friend.InGameFriendData != null)
                fc.friendStatus = FriendContributor.FriendStatus.InGameFriend;
            friendsAndPlayerContributions.Add(fc);
        }
    }

    private void GetContributions(UnityAction onSucces = null)
    {
        globalEventID = ReferenceHolder.GetHospital().globalEventController.GetCurrentGlobalEventID;
        if (!string.IsNullOrEmpty(globalEventID))
        {
            playerContribution = ReferenceHolder.GetHospital().globalEventController.GlobalEventPersonalProgress;

            for (int i = 0; i < friendsAndPlayerContributions.Count; ++i)
            {
                if (i < friendsAndPlayerContributions.Count - 1)
                {
                    AddSingleContribution(globalEventID, friendsAndPlayerContributions[i], null);
                }
                else
                {
                    AddSingleContribution(globalEventID, friendsAndPlayerContributions[i], onSucces);
                }
            }
        }
    }

    private void AddSingleContribution(string eventID, FriendContributor fc, UnityAction onSuccess = null)
    {
        GlobalEventAPI.GetSingleContribution(
                    eventID,
                    fc.SaveID,
                    (contribution) =>
                    {
                        int friendContribution;
                        if (contribution != null)
                        {
                            friendContribution = contribution.amount;
                            fc.contributor = new Contributor(contribution.SaveID, contribution.amount);
                        }
                        else
                        {
                            friendContribution = 0;
                            fc.contributor = new Contributor(fc.SaveID, 0);
                        }
                        fc.SetContributionWithRelation(friendContribution, playerContribution);
                        onSuccess?.Invoke();
                    },
                    (ex) =>
                    {
                        Debug.LogError(ex.Message);
                    });
    }
}
