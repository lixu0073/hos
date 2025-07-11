using System.Collections.Generic;
using UnityEngine;

public class GELeaderboardController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    StickyLeaderboardEntry topStickyEntry;
    [SerializeField]
    StickyLeaderboardEntry bottomStickyEntry;
#pragma warning restore 0649
    [SerializeField]
    private GELeaderboardEntryController entryPrefab = null;

    [SerializeField]
    private Transform content = null;

    private List<GELeaderboardEntryController> items = new List<GELeaderboardEntryController>();

    public void ClearPanel()
    {
        for (int i = items.Count - 1; i > -1; --i)
        {
            Destroy(items[i].gameObject);
        }
        items.Clear();
    }

    public void AddView(GELeaderboardEntryData data)
    {
        if (data == null || data.contributor == null || string.IsNullOrEmpty(data.contributor.SaveID) || data.contributor.Level == 0)
            return;

        items.Add(Instantiate(entryPrefab, content));
        items[items.Count - 1].Initialize(data);

        if (data.contributor.SaveID == Hospital.CognitoEntry.SaveID && topStickyEntry != null && bottomStickyEntry != null)
        {
            topStickyEntry.GetComponent<GELeaderboardEntryController>().Initialize(data);
            topStickyEntry.SetEntryToFollow(items[items.Count - 1].GetComponent<RectTransform>());
            bottomStickyEntry.GetComponent<GELeaderboardEntryController>().Initialize(data);
            bottomStickyEntry.SetEntryToFollow(items[items.Count - 1].GetComponent<RectTransform>());
        }
    }
}
