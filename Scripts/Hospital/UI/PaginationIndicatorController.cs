using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaginationIndicatorController : MonoBehaviour
{
    [SerializeField]
    private PaginationSlotController paginationSlot = null;

    [SerializeField]
    private Transform paginationParent = null;

    private List<PaginationSlotController> slots = new List<PaginationSlotController>();

    private int prevPageCount = 0;

    public void SetPaginationIndicator(int currentPageId, int pagesCount)
    {
        currentPageId = Mathf.Clamp(currentPageId, 0, pagesCount);

        if (prevPageCount != pagesCount) // should refresh 
        {
            for (int i = slots.Count - 1; i > -1; --i)
                Destroy(slots[i].gameObject);

            slots.Clear();

            for (int i = 0; i < pagesCount; ++i)
                slots.Add(Instantiate(paginationSlot, paginationParent));

            prevPageCount = pagesCount;
        }

        for (int i = 0; i < pagesCount; ++i)
            slots[i].SetActivePageIndicatorActive(false);

        slots[currentPageId].SetActivePageIndicatorActive(true);
    }

    private void OnDestroy()
    {
        prevPageCount = 0;
    }
}
