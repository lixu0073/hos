using UnityEngine;
using System.Collections.Generic;

public class TabManager : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    List<Transform> bookmarks;
#pragma warning restore 0649    
    public Sprite inactiveTabBg;
    public Sprite activeTabBg;

    void OnEnable()
    {
        ShowTab(0);
    }
    
    public void ShowTab(int tabID)
    {
        SetTabsInactive();
        SetTabActive(bookmarks[tabID].gameObject);
    }

    void SetTabActive(GameObject bookmark)
    {
        bookmark.GetComponent<BookmarkController>().SetTabVisible();
    }

    void SetTabsInactive()
    {
        for (int i = 0; i < bookmarks.Count; ++i)
        {
            bookmarks[i].GetComponent<BookmarkController>().SetTabInvisible();
        }
    }
}
