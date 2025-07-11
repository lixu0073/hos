using Hospital;
using SimpleUI;
using System;
using UnityEngine;

public class AddFriendsPopupController : UIElement
{
    private const int REFRESH_START_TIME = 1;
    private const int COUNTDOWN_BLOCKING_TIME = 0;
    private const string CLIPPBOARD_COPY_TEMPLATE_KEY = "CODE_COPIED_TO_CLIPBOARD_1";
    private const string CLIPBOARD_COPY_FLOAT_KEY = "CODE_COPIED_TO_CLIPBOARD";
#pragma warning disable 0649
    [SerializeField] private AddFriendsPopupView addFriendsView;
#pragma warning restore 0649
    private bool shallRefresh = true;
    private int timeLeft;

    public void Update()
    {
        CalculateTime();
    }

    public void OnEnable()
    {
        InGameFriendsProvider.OnInGameFriendsChange += LoadRandomFriends;
        InGameFriendsProvider.OnRandomsUpdate += NotifyFriendLoadCompleted;
        PersonalFriendCodeProvider.OnPersonalFriendCodeGet += OnPersonalFriendCodeGet;
        ReferenceHolder.Get().personalFriendCodeProvider.GetPersonalFriendCode();
        LoadRandomFriends();
        addFriendsView.Exit = () => Exit();

        addFriendsView.PassFriendCodeClick =
            () => StartCoroutine(UIController.getHospital.passFriendCodeController.Open());
        addFriendsView.CopyCodeClick = () => CopyCode();
    }

    public void OnDisable()
    {
        InGameFriendsProvider.OnInGameFriendsChange -= LoadRandomFriends;
        InGameFriendsProvider.OnRandomsUpdate -= NotifyFriendLoadCompleted;
        PersonalFriendCodeProvider.OnPersonalFriendCodeGet -= OnPersonalFriendCodeGet;
    }

    private void LoadRandomFriends()
    {
        ReferenceHolder.Get().inGameFriendsProvider.LoadRandomFriends();
    }

    private void OnPersonalFriendCodeGet()
    {
        addFriendsView.UserCode = ReferenceHolder.Get().personalFriendCodeProvider.PersonalFriendCode();
    }

    private void NotifyFriendLoadCompleted()
    {
        shallRefresh = true;
    }

    private void CopyCode()
    {
        MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(CLIPBOARD_COPY_FLOAT_KEY));
        UniClipboard.SetText(string.Format(I2.Loc.ScriptLocalization.Get(CLIPPBOARD_COPY_TEMPLATE_KEY), addFriendsView.UserCode));
    }

    private void CalculateTime()
    {
        timeLeft = GameState.Get().RandomFriendsTimestamp - Convert.ToInt32(ServerTime.getTime());
        if (timeLeft <= REFRESH_START_TIME && shallRefresh)
        {
            shallRefresh = false;
            LoadRandomFriends();
        }
        timeLeft = timeLeft >= COUNTDOWN_BLOCKING_TIME ? timeLeft : COUNTDOWN_BLOCKING_TIME;

        ServerTime.UnixTimestampToDateTime(timeLeft);
        addFriendsView.TimeText = UIController.GetFormattedTime(timeLeft);
    }
}
