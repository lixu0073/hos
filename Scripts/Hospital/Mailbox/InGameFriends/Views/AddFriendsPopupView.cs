using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AddFriendsPopupView : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button exitButton;
    [SerializeField] private Button passFriendCodeButton;
    [SerializeField] private Button copyCodeButton;

    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI userCode;
#pragma warning restore 0649

    public UnityAction Exit
    {
        set
        {
            exitButton.RemoveAllOnClickListeners();
            exitButton.onClick.AddListener(value);
        }
    }

    public UnityAction PassFriendCodeClick
    {
        set
        {
            passFriendCodeButton.RemoveAllOnClickListeners();
            passFriendCodeButton.onClick.AddListener(value);
        }
    }

    public UnityAction CopyCodeClick
    {
        set
        {
            copyCodeButton.RemoveAllOnClickListeners();
            copyCodeButton.onClick.AddListener(value);
        }
    }

    public string TimeText
    {
        set { timeText.text = value; }
    }

    public string UserCode
    {
        get { return userCode.text; }
        set { userCode.text = value; }
    }
}
