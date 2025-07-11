using MovementEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Hospital.Connectors;

namespace Hospital
{

    public class InGameFriendsProvider : MonoBehaviour
    {
        public static event OnSuccess OnInGameFriendsChange;
        public static event OnSuccess OnRandomsUpdate;

        private delegate void OnSuccessList(List<IFollower> list);

        private Dictionary<string, IFollower> friends = new Dictionary<string, IFollower>();
        private Dictionary<string, bool> saveID_wasRemoved = new Dictionary<string, bool>();
        private string currentFriendKey;
        private List<IFollower> cachedRandomFollowers = null;

        IEnumerator<float> CheckingPendingInvitationsCoroutine;

        private bool isChangeDetected = false;
        private AnalyticsController analyticsController;

        public InGameFriendsProvider()
        {
            analyticsController = AnalyticsController.instance;
        }

        public List<IFollower> GetInGameFriends()
        {
            return friends
                .Select(keyValue => keyValue.Value)
                .Where(KeyValue => KeyValue.InGameFriendData.Accepted)
                .ToList();
        }

        public List<IFollower> GetPendingInvitationsSendByMe()
        {
            return friends
                .Select(keyValue => keyValue.Value)
                .Where(x => x.InGameFriendData.CanIRevoke()).ToList();
        }

        public List<IFollower> GetPendingInvitationsSendByOthers()
        {
            return friends
                .Select(keyValue => keyValue.Value)
                .Where(x => x.InGameFriendData.CanIReject()).ToList();
        }

        public IFollower GetFriendById(string id)
        {
            return friends.FirstOrDefault(x => x.Value.SaveID == id).Value;
        }

        private void StartCheckingPendingInvitations()
        {
            KillCheckingPendingInvitations();
            CheckingPendingInvitationsCoroutine = Timing.RunCoroutine(CheckingPendingInvitations());
        }

        #region API
        public void DisconnectFb()
        {
            FriendsDataZipper.ZipFbInGameFriends();
            OnInGameFriendsChange?.Invoke();
        }


        public void FetchFriends(OnSuccess onSuccess = null, OnFailure onFailure = null, Action<List<IFollower>> onSuccessCallback = null)
        {
            GetFriendships(CognitoEntry.SaveID, (followers) =>
            {
                AddPositionsAndCheckRemoved(followers);
                FriendsDataZipper.ZipFbInGameFriends();
                onSuccess?.Invoke();
                if (onSuccessCallback != null)
                    onSuccessCallback(followers);
                StartCheckingPendingInvitations();
                OnInGameFriendsChange?.Invoke();
            }, onFailure);
        }

        public void SendFriendshipAcceptance(string InvitedSaveID, OnSuccess onSuccess, OnFailure onFailure)
        {
            InGameFriendModel invitingModel = InGameFriendModel.CreateInvite(InvitedSaveID);
            invitingModel.Accepted = true;
            invitingModel.RemoveExpireTime();
            SendInvitation(invitingModel, onSuccess,
                () => analyticsController.RerportSocialFriendAdd(InvitedSaveID, friendSource.ViaCode),
                onFailure);
        }

        public void SendInvitation(string InvitedSaveID, OnSuccess onSuccess, OnFailure onFailure)
        {
            SendInvitation(InGameFriendModel.CreateInvite(InvitedSaveID), onSuccess,
                () => analyticsController.ReportSocialInviteSent(InvitedSaveID),
                onFailure);
        }

        public void RevokeInviteSentByMe(IFollower revoked, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            EndFriendshipRelation(true, revoked, onSuccess, onFailure);
        }

        public void RevokeInviteSentByOthers(IFollower revoked, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            EndFriendshipRelation(false, revoked, onSuccess, onFailure);
        }

        private async void EndFriendshipRelation(bool wasSentByMe, IFollower revoked, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            var invitingTask = InGameFriendConnector.DeleteAsync(CognitoEntry.SaveID, revoked.SaveID);
            var invitedTask = InGameFriendConnector.DeleteAsync(revoked.SaveID, CognitoEntry.SaveID);
            try
            {
                await Task.WhenAll(invitingTask, invitedTask);
                bool invitingModelResponse = invitingTask.Status == TaskStatus.RanToCompletion;
                bool invitedModelResponse = invitedTask.Status == TaskStatus.RanToCompletion;
                if (invitingModelResponse && invitedModelResponse)
                {
                    friends.Remove(revoked.InGameFriendData.SaveID);
                    FriendsDataZipper.ZipFbInGameFriends();
                    OnInGameFriendsChange?.Invoke();
                    onSuccess?.Invoke();
                    //removed from already existing Friends
                    if (wasSentByMe && revoked.InGameFriendData.Accepted)
                    {
                        analyticsController.ReportSocialInviteRemove(revoked.SaveID);
                    }
                    else if (wasSentByMe)
                    {
                        analyticsController.ReportSocialInviteCancelled(revoked.SaveID);
                    }
                    else
                    {
                        analyticsController.ReportSocialInviteReject(revoked.SaveID);
                    }
                }
            }
            catch
            {
                if (invitingTask.Exception != null)
                    onFailure?.Invoke(invitingTask.Exception);
                else if (invitedTask.Exception != null)
                    onFailure?.Invoke(invitedTask.Exception);
            }
        }

        public async void AcceptInvite(IFollower invitedFriend, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            invitedFriend.InGameFriendData.Accepted = true;
            invitedFriend.InGameFriendData.RemoveExpireTime();
            InGameFriendModel invitedModel = InGameFriendModel.CreateRevesedModel(invitedFriend.InGameFriendData);
            var invitingTask = InGameFriendConnector.SaveAsync(invitedFriend.InGameFriendData);
            var invitedTask = InGameFriendConnector.SaveAsync(invitedModel);
            try
            {
                await Task.WhenAll(invitingTask, invitedTask);
                bool invitingModelResponse = invitingTask.Status == TaskStatus.RanToCompletion;
                bool invitedModelResponse = invitedTask.Status == TaskStatus.RanToCompletion;
                if (invitingModelResponse && invitedModelResponse)
                {
                    onSuccess?.Invoke();
                    FriendsDataZipper.ZipFbInGameFriends();
                    OnInGameFriendsChange?.Invoke();
                    analyticsController.RerportSocialFriendAdd(invitedFriend.SaveID, friendSource.InviteAccepted);
                }
            }
            catch
            {
                if (invitingTask.Exception != null)
                {
                    invitedFriend.InGameFriendData.Accepted = false;
                    onFailure?.Invoke(invitingTask.Exception);
                }
                else if (invitedTask.Exception != null)
                {
                    invitedFriend.InGameFriendData.Accepted = false;
                    onFailure?.Invoke(invitingTask.Exception);
                }
            }
        }

        public void LoadRandomFriends()
        {
            void OnIdsGet(List<string> ids, bool forceUpdate)
            {
                if (forceUpdate)
                {
                    GameState.Get().RandomFriendsIds = ids;
                    GameState.Get().RandomFriendsTimestamp = Convert.ToInt32(ServerTime.getTime())
                        + DefaultConfigurationProvider.GetConfigCData().SocialRandomFriendsReloadTimeEPOCH;
                    SaveSynchronizer.Instance.InstantSave();
                }

                if (ids.Count == 0)
                {
                    OnRandomsUpdate?.Invoke();
                    return;
                }
                List<IFollower> list = new List<IFollower>();
                foreach (string id in ids)
                {
                    InGameFriend user = new InGameFriend(id);
                    list.Add(user);
                }
                SaveLoadController.SaveState.RandomFriendsIds = ids;
                cachedRandomFollowers = list;
                list = FriendsDataZipper.FilterRandom(list);
                GetBatchSaves((_) => OnRandomsUpdate?.Invoke(), (ex) => Debug.LogError(ex.Message), list);
            }

            if (GameState.Get().RandomFriendsTimestamp < Convert.ToInt32(ServerTime.getTime()))
            {
                InGameFriendsRestConnector.Instance.GetRandomFriendsIds(OnIdsGet, (ex) => Debug.LogError(ex.Message), true);
            }
            else
            {
                if (cachedRandomFollowers != null)
                {
                    OnRandomsUpdate?.Invoke();
                }
                else
                {
                    OnIdsGet(GameState.Get().RandomFriendsIds, false);
                }
            }
        }

        public List<IFollower> GetInGameRandomFriends()
        {
            if (cachedRandomFollowers == null)
                return new List<IFollower>();
            return FriendsDataZipper.FilterRandom(cachedRandomFollowers);
        }
        #endregion

        private void KillCheckingPendingInvitations()
        {
            if (CheckingPendingInvitationsCoroutine != null)
                Timing.KillCoroutine(CheckingPendingInvitationsCoroutine);
            CheckingPendingInvitationsCoroutine = null;
        }

        private IEnumerator<float> CheckingPendingInvitations()
        {
            while (true)
            {
                CreateRemovingFilter();

                GetFriendships(CognitoEntry.SaveID, (followers) =>
                {
                    isChangeDetected = false;

                    AddPositionsAndCheckRemoved(followers);

                    if (isChangeDetected)
                    {
                        FriendsDataZipper.ZipFbInGameFriends();
                        OnInGameFriendsChange?.Invoke();
                    }
                }, (ex) => Debug.LogError(ex.Message));

                yield return Timing.WaitForSeconds(DefaultConfigurationProvider.GetConfigCData().SocialFriendsPoolinSeconds);
            }
        }

        private void CreateRemovingFilter()
        {
            saveID_wasRemoved.Clear();
            foreach (string id in friends.Keys)
            {
                saveID_wasRemoved.Add(id, false);
            }
        }

        private void AddPositionsAndCheckRemoved(List<IFollower> list)
        {
            FillRemovedDictionary(list);
            FilterRemoved();

            foreach (IFollower friend in list)
            {
                currentFriendKey = friend.InGameFriendData.SaveID;
                //when new Item arives somone sent me a request
                if (!friends.ContainsKey(currentFriendKey))
                {
                    friends.Add(currentFriendKey, friend);
                    isChangeDetected = true;
                }
                //if there is difference my request was accepted
                else if (friends[currentFriendKey].InGameFriendData.Accepted != friend.InGameFriendData.Accepted)
                {
                    friends[currentFriendKey].InGameFriendData.Accepted = true;
                    isChangeDetected = true;
                }
            }
        }

        private void FilterRemoved()
        {
            foreach (string id in saveID_wasRemoved
                .Where(keyValue => keyValue.Value == true)
                .Select(keyValue => keyValue.Key))
            {
                isChangeDetected = true;
                friends.Remove(id);
            }
        }

        private void FillRemovedDictionary(List<IFollower> list)
        {
            foreach (string id in saveID_wasRemoved.Keys.ToArray())
            {
                if (list.FirstOrDefault(igf => igf.SaveID == id) == null)
                {
                    saveID_wasRemoved[id] = true;
                }
            }
        }

        private void GetBatchSaves(OnSuccessList onSuccess, OnFailure onFailure, List<IFollower> list)
        {
            CacheManager.BatchPublicSavesWithResults(list.Cast<CacheManager.IGetPublicSave>().ToList(), (publicSaves) =>
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    for (int j = 0; j < publicSaves.Count; ++j)
                    {
                        if (list[i].GetSaveID() == publicSaves[j].SaveID)
                        {
                            list[i].SetSave(publicSaves[j]);
                            break;
                        }
                    }
                }
                onSuccess?.Invoke(list);
            }, onFailure);
        }

        private async void SendInvitation(InGameFriendModel invitingModel, OnSuccess onSuccess, OnSuccess onSuccessAnalytics, OnFailure onFailure)
        {
            InGameFriendModel invitedModel = InGameFriendModel.CreateRevesedModel(invitingModel);
            var invitingTask = InGameFriendConnector.SaveAsync(invitingModel);
            var invitedTask = InGameFriendConnector.SaveAsync(invitedModel);
            try
            {
                await Task.WhenAll(invitingTask, invitedTask);
                bool invitingModelResponse = invitingTask.Status == TaskStatus.RanToCompletion;
                bool invitedModelResponse = invitedTask.Status == TaskStatus.RanToCompletion;
                if (invitingModelResponse && invitedModelResponse)
                {
                    if (invitingModel == null)
                    {
                        onFailure?.Invoke(new Exception("can't create friend"));
                        return;
                    }
                    IFollower user = new InGameFriend(invitingModel.SaveID);
                    user.InGameFriendData = invitingModel;
                    CacheManager.GetPublicSaveById(invitingModel.SaveID, (publicSave) => {
                        user.SetSave(publicSave);
                        friends[user.InGameFriendData.SaveID] = user;
                        FriendsDataZipper.ZipFbInGameFriends();
                        onSuccess?.Invoke();
                        OnInGameFriendsChange?.Invoke();
                        onSuccessAnalytics?.Invoke();
                    }, onFailure);
                }
            }
            catch
            {
                if (invitingTask.Exception != null)
                    onFailure?.Invoke(invitingTask.Exception);
                else if (invitedTask.Exception != null)
                    onFailure?.Invoke(invitedTask.Exception);
            }
        }

        private async void GetFriendships(string MeSaveID, OnSuccessList onSuccess, OnFailure onFailure)
        {
            try
            {
                var models = await InGameFriendConnector.FromQueryAndGetNextSetAsync(MeSaveID);
                if (models.Count() == 0)
                {
                    onSuccess?.Invoke(new List<IFollower>());
                    return;
                }
                List<IFollower> list = new List<IFollower>();
                foreach (InGameFriendModel model in models)
                {
                    InGameFriend user = new InGameFriend(model.SaveID);
                    user.InGameFriendData = model;
                    list.Add(user);
                }
                GetBatchSaves(onSuccess, onFailure, list);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        private void OnDestroy()
        {
            KillCheckingPendingInvitations();
        }

    }

}
