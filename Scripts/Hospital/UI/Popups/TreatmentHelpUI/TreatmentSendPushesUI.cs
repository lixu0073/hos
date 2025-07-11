using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SimpleUI;
using Hospital;

public class TreatmentSendPushesUI : UIElement
{
    [SerializeField] private GameObject treatmentFriendPanelPrefab = null;

    [SerializeField] private Transform friendsListContent = null;

    [SerializeField] private Button askForHelpButton = null;
#pragma warning disable 0414
    [SerializeField] private Button moreFriendsButton = null;
#pragma warning restore 0414
    [SerializeField] private Toggle selectAllToggle = null;

    public delegate void ActionOnClickOnFriendToggle(IFollower friend, TreatmentFriendPanel friendView);
    public delegate void OnExit();

    private List<Toggle> friendsToggles = new List<Toggle>();
    private OnExit onExit;
    UnityAction<bool> onAllToggle = null;

    public void ExitButton()
    {
        Exit();
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        onExit?.Invoke();
        base.Exit(hidePopupWithShowMainUI);
    }

    public void Open(List<IFollower> friends, ActionOnClickOnFriendToggle onFriendToggle, UnityAction<bool> onAllToggle, UnityAction onAskForHelpButtonClick, UnityAction onMoreFriendsButtonClick, OnExit onExit)
    {
        gameObject.SetActive(true);
        StartCoroutine(base.Open(true, false, () =>
        {
            this.onExit = onExit;
            SetFriendsList(friends, onFriendToggle);

            this.onAllToggle = onAllToggle;
            SetSellectAllToggleOnToggle(this.onAllToggle);

            SetAskForHelpButtonOnClick(onAskForHelpButtonClick);
            SetAllSelected(true);
            SetAllTogle(true, true);
        }));
    }

    public void SetAllSelected(bool setSelected)
    {
        if (friendsToggles == null)
        {
            Debug.LogError("friendsToggles list is null");
            return;
        }
        for (int i = 0; i < friendsToggles.Count; ++i)
        {
            friendsToggles[i].isOn = setSelected;
        }
    }

    public void SetAllTogle(bool isOn, bool onOpen = false)
    {
        if (onOpen)
            selectAllToggle.isOn = isOn;
        else
        {
            selectAllToggle.onValueChanged.RemoveAllListeners();
            selectAllToggle.isOn = isOn;
            SetSellectAllToggleOnToggle(this.onAllToggle);
        }
    }

    private void SetFriendsList(List<IFollower> friends, ActionOnClickOnFriendToggle onToggle)
    {
        if (friendsToggles == null)
            friendsToggles = new List<Toggle>();
        else
            friendsToggles.Clear();

        for (int i = 0; i < friendsListContent.childCount; ++i)
        {
            Destroy(friendsListContent.GetChild(i).gameObject);
        }

        if (friends == null)
        {
            Debug.LogError("medicines list is null");
            return;
        }

        for (int i = 0; i < friends.Count; ++i)
        {
            GameObject providerPanel = Instantiate(treatmentFriendPanelPrefab, friendsListContent);
            TreatmentFriendPanel view = providerPanel.GetComponent<TreatmentFriendPanel>();
            if (view != null)
            {
                int tmp = i;

                view.SetTreatmentFriendPanel(friends[tmp], (x) => {
                    onToggle(friends[tmp], view);
                });
                friendsToggles.Add(view.selectFriendToggle);
            }
        }
    }

    private void SetSellectAllToggleOnToggle(UnityAction<bool> onToggle)
    {
        if (selectAllToggle == null)
        {
            Debug.LogError("selectAllToggle is null");
            return;
        }

        selectAllToggle.onValueChanged.RemoveAllListeners();
        selectAllToggle.onValueChanged.AddListener((selected) => {
            UIController.PlayClickSoundSecure(selectAllToggle.gameObject);
            onToggle(selected);
        });
    }

    private void SetAskForHelpButtonOnClick(UnityAction onClick) {
        UIController.SetButtonOnClickActionSecure(askForHelpButton, () => {
            UIController.PlayClickSoundSecure(askForHelpButton.gameObject);
            onClick();
        });
    }

}
