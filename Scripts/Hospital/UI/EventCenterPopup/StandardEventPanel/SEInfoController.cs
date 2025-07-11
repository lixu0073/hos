using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEInfoController : MonoBehaviour
{
    [SerializeField]
    private SEInfoPanelController entryPrefab = null;

    [SerializeField]
    private Transform content = null;

    [SerializeField]
    private Animator infoAnimator = null;

    [SerializeField]
    private string animationParameter_Show = "Show";

    private List<SEInfoPanelController> items = new List<SEInfoPanelController>();

    private bool isInfoVisible = false;

    public void ToggleInfoVisible()
    {
        isInfoVisible = !isInfoVisible;
        infoAnimator.SetBool(animationParameter_Show, isInfoVisible);
    }

    public void SetInfoVisible(bool setVisible)
    {
        isInfoVisible = setVisible;
        infoAnimator.SetBool(animationParameter_Show, isInfoVisible);
    }

    public void ClearPanel()
    {
        for (int i = items.Count - 1; i > -1; --i)
        {
            Destroy(items[i].gameObject);
        }
        items.Clear();
    }

    public void AddView(SEInfoPanelData data)
    {
        items.Add(Instantiate(entryPrefab, content));
        items[items.Count - 1].Initialize(data);
    }
}
