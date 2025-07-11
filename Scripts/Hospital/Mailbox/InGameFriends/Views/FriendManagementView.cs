using Hospital;
using I2.Loc;
using SimpleUI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FriendManagementView : UIElement
{
    private const string REQUEST_NAMBACK = "SOCIAL_REQUEST_CONFIRM_TITLE";
    private const string REQUEST_BODY = "SOCIAL_REQUEST_CONFIRM_BODY";
    private const string REQUEST_BUTTON_NO = "SOCIAL_REQUEST_CONFIRM_BUTTON_B";
    private const string REQUEST_BUTTON_YES = "SOCIAL_REQUEST_CONFIRM_BUTTON_A";

    private const string REMOVE_NAMBACK = "SOCIAL_UNFRIEND_TITLE";
    private const string REMOVE_BODY = "SOCIAL_UNFRIEND_BODY";
    private const string REMOVE_BUTTON_NO = "SOCIAL_UNFRIEND_BUTTON_B";
    private const string REMOVE_BUTTON_YES = "SOCIAL_UNFRIEND_BUTTON_A";

    public const string ARE_YOU_SURE_ACCEPT = "Accept invite from ";
    public const string ARE_YOU_SURE_REVOKE = "Revoke invitation ";
    public const string ARE_YOU_SURE_REMOVE = "Do you want to unfriend ";
#pragma warning disable 0649
    [Header("Confirm popup")]
    [SerializeField] private TextMeshProUGUI confirmPopUpText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private Image facebookImage;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Localize")]
    [SerializeField] private TextMeshProUGUI namback;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI noButton;
    [SerializeField] private TextMeshProUGUI yesButton;
#pragma warning restore 0649

    public string ConfirmPopUpText
    {
        set { confirmPopUpText.text = value; }
    }

    public UnityAction Accept
    {
        set
        {
            acceptButton.RemoveAllOnClickListeners();
            acceptButton.onClick.AddListener(value);
        }
    }

    public UnityAction Reject
    {
        set
        {
            rejectButton.RemoveAllOnClickListeners();
            rejectButton.onClick.AddListener(value);
        }
    }

    public void Exit()
    {
        base.Exit();
    }

    public void SetUnfriendUI(IFollower friend)
    {
        namback.text = ScriptLocalization.Get(REMOVE_NAMBACK);
        bodyText.text = string.Format(ScriptLocalization.Get(REMOVE_BODY), friend.Name);
        yesButton.text = ScriptLocalization.Get(REMOVE_BUTTON_YES);
        noButton.text = ScriptLocalization.Get(REMOVE_BUTTON_NO);
        FillLevelAndImage(friend);
    }

    public void SetRequestUI(IFollower friend)
    {
        namback.text = ScriptLocalization.Get(REQUEST_NAMBACK);
        bodyText.text = string.Format(ScriptLocalization.Get(REQUEST_BODY), friend.Name);
        yesButton.text = ScriptLocalization.Get(REQUEST_BUTTON_YES);
        noButton.text = ScriptLocalization.Get(REQUEST_BUTTON_NO);
        FillLevelAndImage(friend);
    }

    private void FillLevelAndImage(IFollower friend)
    {
        facebookImage.sprite = friend.Avatar;
        levelText.text = friend.Level.ToString();
    }
}
