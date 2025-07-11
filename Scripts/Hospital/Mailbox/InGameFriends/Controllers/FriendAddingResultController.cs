using SimpleUI;
using UnityEngine;

namespace Hospital
{
    public class FriendAddingResultController : UIElement
    {
        private const string OK_KEY = "OK";
        private const string VISIT_KEY = "VISIT";
#pragma warning disable 0649
        [SerializeField] private FriendAddingResultView view;
        [SerializeField] InGameFriendResultPopupController friendController;
#pragma warning restore 0649
        private IFollower currentFriend;        

        private void OnEnable()
        {
            string friendId = ReferenceHolder.Get().personalFriendCodeProvider.RecentlyAdddedFriendSaveID;
            if (string.IsNullOrEmpty(friendId))
            {
                currentFriend = null;
                view.Succes = false;
                view.ConfirmButtonText = OK_KEY;
                view.VisitingOkAction = () =>
                {
                    Exit();
                };
            }
            else
            {
                view.Succes = true;
               
                view.ConfirmButtonText = VISIT_KEY;
                currentFriend = ReferenceHolder.Get().inGameFriendsProvider.GetFriendById(friendId);
                friendController.Initialize(currentFriend, VisitingEntryPoint.FriendsPopup);       
                view.VisitingOkAction = () => 
                {
                    friendController.Visit(VisitingEntryPoint.FriendsPopup);
                };
            }
            view.Exit = () => Exit();
        }


        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            UIController.getHospital.addFriendsPopupController.transform.SetAsLastSibling();
            base.Exit(hidePopupWithShowMainUI);
        }
    }
}

