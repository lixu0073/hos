using UnityEngine;

namespace Hospital
{
public class FacebookSectionController : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] FacebookSectionView view;
#pragma warning restore 0649

        public void OnEnable()
        {
            AccountManager.OnFacebookConnected += UpdateFacebook;
            view.FacebookConnect = () => ConnectToFacebook();
            UpdateFacebook();
        }

        public void OnDisable()
        {
            AccountManager.OnFacebookConnected -= UpdateFacebook;
        }

        private void UpdateFacebook()
        {
            bool isFacebookConnected = AccountManager.Instance.IsFacebookConnected;
            view.SetReawardGameobjectActive(!GameState.Get().FBRewardConnectionClaimed);
            view.SetConnectFacebookButtonActive(!isFacebookConnected);
        }

        private void ConnectToFacebook()
        {
            AccountManager.Instance.ConnectToFacebook(true, SocialEntryPoint.AddFriends);
        }

        public void FbCallback()//(IAppInviteResult result)
        {
           // if (!result.Cancelled && string.IsNullOrEmpty(result.Error))
            //{
                view.SetReawardGameobjectActive(false);
                AnalyticsController.instance.ReportSocialConnect(SocialServiceAction.Invite, SocialEntryPoint.FriendsDrawer, SocialServiceType.Facebook);
                TenjinController.instance.ReportFBInvite(GameState.Get().hospitalLevel);
            //}
        }
    }
}