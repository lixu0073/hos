using UnityEngine;
using UnityEngine.UI;

public class BookmarkController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    Image bg;
    [SerializeField]
    GameObject tab;    
    [SerializeField]
    TabManager tabManager;
#pragma warning restore 0649

    public void SetTabVisible()
    {
        bg.sprite = tabManager.activeTabBg;
        tab.SetActive(true);
    }

    public void SetTabInvisible()
    {
        bg.sprite = tabManager.inactiveTabBg;
        tab.SetActive(false);
    }

    public void OnClick(int tabID)
    {
        tabManager.ShowTab(tabID);
    }
}
