using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hospital
{

    public class BaseFollower : CacheManager.IGetPublicSave, IFollower
    {

        protected PublicSaveModel save;
        protected Sprite avatar = null;
        protected string avatarURL;
        protected string facebookLogin;
        protected bool isFacebookDataDownloaded = false;
        protected bool isFacebookAvatarDownloaded = false;


        protected string saveID;
        protected string facebookID;

        protected string name;
        protected int level;
        protected long lastActivity = -1;
        protected bool hasAnyHelpRequest;
        protected bool hasPlantationHelpRequest;
        protected bool hasEpidemyHelpRequest;
        protected bool hasTreatmentHelpRequest;
        protected BubbleBoyReward reward = null;
        protected InGameFriendModel inGameFriendModel;

        public delegate void OnDataChangedEventHandler();
        public event OnDataChangedEventHandler OnDataChanged;

        public BaseFollower()
        {

        }

        public BaseFollower(string saveID)
        {
            SaveID = saveID;
        }

        public bool IsFaceBookFriend
        {
            get { return this is AccountManager.FacebookFriend; }
        }

        public string FacebookID
        {
            get
            {
                if (save != null)
                {
                    return save.FacebookID;
                }
                return facebookID;
            }

            set
            {
                if (save != null)
                {
                    save.FacebookID = value;
                }
                facebookID = value;
            }
        }

        public string SaveID
        {
            get
            {
                if (save != null)
                {
                    return save.SaveID;
                }
                return saveID;
            }

            set
            {
                if (save != null)
                {
                    save.SaveID = value;
                }
                saveID = value;
            }
        }

        public int Level
        {
            get
            {
                if (save != null)
                {
                    return save.Level;
                }
                return level;
            }

            set
            {
                if (save != null)
                {
                    save.Level = value;
                }
                level = value;
            }
        }
        public bool HasAnyHelpRequest
        {
            get
            {
                return HasPlantationHelpRequest || HasTreatmentHelpRequest || HasEpidemyHelpRequest;
            }
        }

        public bool HasPlantationHelpRequest
        {
            get
            {
                if (save != null)
                {
                    return save.PlantationHelp;
                }
                return hasPlantationHelpRequest;
            }

            set
            {
                if (save != null)
                {
                    save.PlantationHelp = value;
                }
                hasPlantationHelpRequest = value;
            }
        }

        public bool HasEpidemyHelpRequest
        {
            get
            {
                if (save != null)
                {
                    return save.EpidemyHelp;
                }
                return hasEpidemyHelpRequest;
            }

            set
            {
                if (save != null)
                {
                    save.EpidemyHelp = value;
                }
                hasEpidemyHelpRequest = value;
            }
        }

        public bool HasTreatmentHelpRequest
        {
            get
            {
                if (save != null)
                {
                    return save.TreatmentHelp;
                }
                return hasTreatmentHelpRequest;
            }

            set
            {
                if (save != null)
                {
                    save.TreatmentHelp = value;
                }
                hasTreatmentHelpRequest = value;
            }
        }

        public BubbleBoyReward Reward
        {
            get
            {
                if (save != null)
                {
                    return save.BestWonItem;
                }
                return reward;
            }

            set
            {
                if (save != null)
                {
                    save.BestWonItem = value;
                }
                reward = value;
            }
        }

        public long LastActivity
        {
            get
            {
                if (save != null)
                {
                    return save.lastActivity;
                }
                return lastActivity;
            }

            set
            {
                if (save != null)
                {
                    save.lastActivity = value;
                }
                lastActivity = value;
            }
        }

        public InGameFriendModel InGameFriendData
        {
            get
            {
                return inGameFriendModel;
            }

            set
            {
                inGameFriendModel = value;
            }
        }

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(facebookLogin))
                {
                    return facebookLogin;
                }
                if (save == null)
                {
                    return name;
                }
                return save.Name;
            }
            set
            {
                facebookLogin = value;
            }
        }

        public string FacebookLogin
        {
            get
            {
                return facebookLogin;
            }
            set
            {
                facebookLogin = value;
            }
        }

        public Sprite Avatar
        {
            get
            {
                if (avatar == null)
                {
                    return ResourcesHolder.Get().friendsDefaultAvatar;
                }
                else
                {
                    return avatar;
                }
            }

            set
            {
                avatar = value;
            }
        }

        public string AvatarURL
        {
            get
            {
                return avatarURL;
            }

            set
            {
                avatarURL = value;
            }
        }

        public delegate void OnHelpRequestStatusChanged(bool isPlantationHelpRequested, bool isEpidemyHelpRequested, bool isTreatmentHelpRequested);

        public event OnHelpRequestStatusChanged onHelpRequestStatusChanged;

        public void NotifyOnHelpRequestStatusChanged(bool isPlantationHelpRequested, bool isEpidemyHelpRequested, bool isTreatmentHelpRequested)
        {
            onHelpRequestStatusChanged?.Invoke(isPlantationHelpRequested, isEpidemyHelpRequested, isTreatmentHelpRequested);
        }



        public virtual bool IsHelpRequested()
        {
            return save.EpidemyHelp || save.PlantationHelp || save.TreatmentHelp;
        }

        public virtual bool IsPlantationHelpRequested()
        {
            return save.PlantationHelp;
        }

        public virtual bool IsEpidemyHelpRequested()
        {
            return save.EpidemyHelp;
        }

        public virtual bool IsTreatmentHelpRequested()
        {
            return save.TreatmentHelp;
        }

        public bool IsFacebookDataDownloaded()
        {
            return isFacebookDataDownloaded;
        }

        public bool IsFacebookAvatarDownloaded()
        {
            return isFacebookAvatarDownloaded;
        }

        public void SetIsFacebookAvatarDownloaded(bool isFacebookAvatarDownloaded)
        {
            this.isFacebookAvatarDownloaded = isFacebookAvatarDownloaded;
        }


        public void SetIsFacebookDataDownloaded(bool IsFacebookDataDownloaded)
        {
            isFacebookDataDownloaded = IsFacebookDataDownloaded;
        }

        public void DownloadAvatar(OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (string.IsNullOrEmpty(avatarURL) || IsFacebookAvatarDownloaded())
            {
                onFailure?.Invoke(null);
                return;
            }
            AccountManager.Instance.DownloadFacebookAvatar(this, onSuccess, onFailure);
        }

        public void DownloadFacebookData(OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (string.IsNullOrEmpty(FacebookID) || IsFacebookDataDownloaded())
            {
                onFailure?.Invoke(null);
            }
            GetFacebookUserDataUseCase getFacebookUserDataUseCase = new GetFacebookUserDataUseCase();
            getFacebookUserDataUseCase.Execute((user) =>
            {
                isFacebookDataDownloaded = true;
                isFacebookAvatarDownloaded = true;
                Avatar = (user.avatar);
                FacebookLogin = (user.Name);
                SetIsFacebookDataDownloaded(true);
                SetIsFacebookAvatarDownloaded(true);
                onSuccess?.Invoke();
            }, (ex) =>
            {
                Debug.LogError(ex.Message);
                onFailure?.Invoke(ex);
            }, FacebookID);
        }


        public virtual void SetSave(PublicSaveModel publicSave)
        {
            save = publicSave;
            OnDataChanged?.Invoke();
        }


        public virtual string GetMeSaveID()
        {
            return CognitoEntry.SaveID;
        }

        public virtual string GetSaveID()
        {
            return SaveID;
        }

        public virtual PublicSaveModel GetSave()
        {
            return save;
        }


        public bool IsFacebookConnected()
        {
            return !string.IsNullOrEmpty(FacebookID);
        }

        public virtual Sprite GetFrame()
        {
            if (FriendsDataZipper.CheckIfPresent(SaveID))
            {
                return FriendsDataZipper.GetFriend(SaveID).GetFrame();
            }
            return ResourcesHolder.Get().frameData.basicFrame;
        }
    }
}