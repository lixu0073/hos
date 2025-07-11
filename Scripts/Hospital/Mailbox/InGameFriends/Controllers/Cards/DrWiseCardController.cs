using Hospital;
using UnityEngine;
using UnityEngine.UI;


//Szmury
//This fucking class is compilation of darkest shit of drWise.
//It is for sure not reusable it is mix of all unique stuff
public class DrWiseCardController : BaseFriendCardController
{
#pragma warning disable 0649
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private ScrollRect scrollRect;
#pragma warning restore 0649

    public void Start()
    {
        friendUI = gameObject.GetComponent<FriendCardUI>();
        AddOnImageClick(VisitingEntryPoint.Friends);
    }

    protected override void AddAdditionalBehaviours(IFollower person) { }

    protected override void OnVisiting() { }

    protected override void AddOnImageClick(VisitingEntryPoint visitingEntryPoint)
    {
        Button button = gameObject.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (!VisitingController.Instance.canVisit)
                return;

            if (GameState.Get().hospitalLevel < 4) // wizytowanie Wise na mniejszym niz 4 level nie robi czestego save (visiting Wise below level 4 doesn't save much)
                return;

            if (!VisitingController.Instance.IsVisiting || (VisitingController.Instance.IsVisiting && SaveLoadController.SaveState.ID != "SuperWise"))
            {
                SaveSynchronizer.Instance.InstantSave();
                VisitingController.Instance.VisitWiseHospital();
                NotificationCenter.Instance.WiseHospitalLoaded.Invoke(new BaseNotificationEventArgs());
            }
        });
    }

    public void BlinkDrWise()
    {
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.blink_friends && !VisitingController.Instance.IsVisiting)
        {
            UIController.get.FriendsDrawer.Exit();
            scrollRect.horizontalNormalizedPosition = 0;
            TutorialUIController.Instance.BlinkImage(friendUI.GetAvatarContainer().GetComponent<Image>(), 1.2f);
        }
    }

    protected override void OnDestroy() { }
}
