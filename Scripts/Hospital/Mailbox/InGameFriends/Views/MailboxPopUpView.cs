using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using I2.Loc;

public class MailboxPopUpView : MonoBehaviour
{
    private const string REPLACE_MARK = ":";
    private const string I2_KEY = "SOCIAL_MAILBOX_TOTAL";
    StringBuilder builder = new StringBuilder();

#pragma warning disable 0649
    [SerializeField] private Image giftTab;
    [SerializeField] private Image giftIcon;
#pragma warning restore 0649
    [SerializeField] private Localize nambackLocalize = null;
    [SerializeField] private Button addFriendsButton = null;
    [SerializeField] private TextMeshProUGUI addFriendsLabel = null;

    public bool AddFriendsButtonActive
    {
        set { addFriendsButton.gameObject.SetActive(value); }
    }

    public bool AddFriendsLabelActive
    {
        set { addFriendsLabel.gameObject.SetActive(value); }
    }

    public void SetGrayscale(bool value)
    {
        giftTab.material = value ? ResourcesHolder.Get().GrayscaleMaterial : null;
        giftIcon.material = value ? ResourcesHolder.Get().GrayscaleMaterial : null;
    }

    public void SetLocalization(string key)
    {
        nambackLocalize.Term = key;
    }

    public void SetAddFriendsButton(UnityAction onClickAction)
    {
        addFriendsButton.RemoveAllOnClickListeners();
        addFriendsButton.onClick.AddListener(onClickAction);
    }
}
