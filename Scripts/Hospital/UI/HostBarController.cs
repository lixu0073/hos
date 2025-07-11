using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hospital
{
	public class HostBarController : MonoBehaviour
	{
		[SerializeField]
		public TextMeshProUGUI levelText;
		[SerializeField]
		public TextMeshProUGUI nameText;
#pragma warning disable 0649
        [SerializeField]
        private I2.Loc.Localize nameLocalize;
        [SerializeField]
		private GameObject followButton;
        [SerializeField]
        private GameObject buttonInteriorWhenNotFollowed;
        [SerializeField]
        private GameObject buttonInteriorWhenFollowed;
        [SerializeField]
        private HostBarFriendshipController hostBarFreindship;
        [SerializeField]
        private Image frame;
#pragma warning restore 0649
        public Image avatarImage;
        public Sprite wiseAvatar;

		public string userID;
        private string hospitalName;

        private Sprite defaultavatar;
        private Image buttonImage;
        private bool isFollowing = false;
        private bool isWise = false;

        public void Awake()
        {
            defaultavatar = ResourcesHolder.Get().frameData.defaultAvatar; 
        }

        public void Open(string userName, int lvl, string userID)
		{
            Close();

            isWise = userID == SaveLoadController.WISE;
			hospitalName = userName;
            SetHospitalName();

            levelText.text = lvl.ToString();
			this.userID = userID;

			gameObject.SetActive(true);
            //followButton.SetActive(true);
            SetFollowButtonInterior();
            buttonImage = followButton.GetComponent<Image>();
            if (isWise)
            {
                avatarImage.sprite = wiseAvatar;
                followButton.SetActive(false);              
                return;
            }
            else
            {
                followButton.SetActive(true);
                isFollowing = IsFollowing();
                if(!isFollowing && !FriendsController.Instance.IsFollowingsDownloaded())
                {
                    CheckFollowingStatus();
                }
                /*
                if (isFollowing)
                {
                    //LikeButtonDisable();
                    SetFollowButtonInterior();
                }
                */
                SetFollowButtonInterior();

                TryToShowFBAvatar();
                setFrame();
            }
        }

        private void setFrame()
        {
            IFollower friend;
            if (FriendsDataZipper.CheckIfPresent(userID))
            {
                friend = FriendsDataZipper.GetFriend(userID);
            }
            else
            {
                friend = new BaseFollower(userID);
            }
            frame.sprite = friend.GetFrame();
        }

        private void SetFollowButtonInterior()
        {
            bool following = isFollowing;

            buttonInteriorWhenFollowed.SetActive(following);
            buttonInteriorWhenNotFollowed.SetActive(!following);
        }

        private void TryToShowFBAvatar()
        {
            SetDefaultAvatar();
            SetHospitalName();
            if (AccountManager.Instance.IsFacebookConnected)
            {
                CacheManager.GetPublicSaveById(userID, (save) =>
                {
                    if(!string.IsNullOrEmpty(save.FacebookID))
                    {
                        CacheManager.GetUserDataByFacebookID(save.FacebookID, (login, avatar) =>
                        {
                            if(nameText != null)
                            {
                                //SetFBLogin(login);
                            }
                            if (avatarImage != null && avatar != null)
                            {
                                avatarImage.sprite = avatar;
                            }
                        }, (ex) =>
                        {
                            Debug.LogError(ex.Message);
                        });
                    }
                }, (ex) =>
                {
                    Debug.LogError(ex.Message);
                });
            }
        }

        private void SetDefaultAvatar()
        {
            avatarImage.sprite = defaultavatar;
        }

        private void SetFBLogin(string login)
        {
            nameText.text = login;
        }

        private void SetHospitalName()
        {
            //if visiting Wise set name to translated dr Wise name
            if(SaveLoadController.SaveState.ID.Equals(SaveLoadController.WISE))
            {
                nameLocalize.SetTerm("FRIENDS_DR_WISE");
                nameText.text = I2.Loc.ScriptLocalization.Get("FRIENDS_DR_WISE");
            }
            else
            {
                nameLocalize.SetTerm("FONT_STROKE");
                nameText.text = hospitalName;
            }
        }

		public void Close()
		{
            hostBarFreindship.setActive(false);
			gameObject.SetActive(false);
		}

        private bool IsFollowing()
        {
            return FriendsController.Instance.IsFollowing(userID);
        }

        private void CheckFollowingStatus()
        {
            FriendsController.Instance.IsFollowing(userID, () => {
                isFollowing = true;
                SetFollowButtonInterior(); //LikeButtonDisable(false);
            }, (ex) => {
                if (ex != null)
                    Debug.LogError(ex.Message);
            });
        }

        private void LikeButtonDisable(bool showFloat = true)
        {
            followButton.SetActive(false);
            if (showFloat)
            {
                MessageController.instance.ShowMessage(isFollowing ? 42 : 43);
            }
        }

        private void RefreshLikeButtonState(bool showFloat = true)
        {
            if (showFloat)
            {
                MessageController.instance.ShowMessage(isFollowing ? 42 : 43);
            }
        }

		public void Follow()
		{
			if (string.IsNullOrEmpty(userID))
			{
				Debug.LogWarning("HostBar userID is empty, should not happen - check this.");
				return;
			}
            if (isFollowing)
            {
                FriendsController.Instance.RemoveFromFollowing(userID, () =>
                {                    
                    isFollowing = false;
                    SetFollowButtonInterior();
                    RefreshLikeButtonState();
                }, (ex) =>
                {
                    Debug.LogError(ex.Message);
                });
            }
            else
            {
                FriendsController.Instance.AddToFollowing(userID, () =>
                {
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.LikeOtherHospitals));
                    isFollowing = true;
                    RefreshLikeButtonState();
                    SetFollowButtonInterior(); //LikeButtonDisable();
                    AnalyticsController.instance.ReportSocialLike(userID);
                }, (ex) =>
                {
                    Debug.LogError(ex.Message);
                });
            }
        }
	}
}
