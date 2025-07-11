using SimpleUI;
using System.Collections.Generic;
using UnityEngine;

public class UIElementTabController : UIElement
{
    public enum DailyQuestAndRewardIndexes
    {
        DailyQuest = 0,
        DailyRewardStandard = 1,
    }
#pragma warning disable 0649
    [Tooltip("List of gameobjects wrapping entire popup content and also containig ITabControllerClient component")]
    [SerializeField] private List<GameObject> listOfPopupWrappers;
    [SerializeField] private List<PopupBookmark> listOfTabs;
#pragma warning restore 0649
    private int currentIndex = -1;

    private List<ITabControllerClient> listOfContent;

    public void OpenTabContent(int newIndex)
    {
        CoroutineInvoker.Instance.StartCoroutine(base.Open(true, false, () =>
        {            
            if (listOfContent == null)
            {
                listOfContent = new List<ITabControllerClient>();
                for (int i = 0; i < listOfPopupWrappers.Count; i++)
                {
                    listOfContent.Add(listOfPopupWrappers[i].GetComponent<ITabControllerClient>());
                }
            }
            
            if (currentIndex != newIndex)
            {
                if (currentIndex != -1)
                {
                    listOfContent[currentIndex].DeactiveTabContent();
                }
                listOfContent[newIndex].SetTabContentActive(() =>
                {
                    SetBookmarkSelected(newIndex);
                    SetWrapperActive(newIndex);
                    currentIndex = newIndex;
                }, () => { });
            }
        }));
    }

    private void SetWrapperActive(int index)
    {
        listOfPopupWrappers[index].SetActive(true);
        for (int i = 0; i < listOfPopupWrappers.Count; i++)
        {
            if (i != index)
            {
                listOfPopupWrappers[i].SetActive(false);
            }
        }
    }

    private void SetBookmarkSelected(int index)
    {
        listOfTabs[index].SetBookmarkSelected(true);
        for (int i = 0; i < listOfTabs.Count; i++)
        {
            if (i != index)
            {
                listOfTabs[i].SetBookmarkSelected(false);
            }
        }
    }

    private void OnDisable()
    {
        currentIndex = -1;
    }

    public void ButtonExit()
    {
        Exit();
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        if (currentIndex != -1)
        {
            listOfContent[currentIndex].DeactiveTabContent();
            if (this.GetComponentInChildren<DailyRewardPopup>() != null)
                NotificationCenter.Instance.DailyQuestPopUpClosed.Invoke(new BaseNotificationEventArgs());
        }
        base.Exit(hidePopupWithShowMainUI);
    }

    public PopupBookmark GetBookmark(int index)
    {
        if (index > 0 && index < listOfTabs.Count)
            return listOfTabs[index];

        return null;
    }
}
