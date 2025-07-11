using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaginationSlotController : MonoBehaviour
{
    [SerializeField]
    private Image pageIndicator = null;
    [SerializeField]
    private Sprite inactiveSprite = null;
    [SerializeField]
    private Sprite activeSprite = null;

    public void SetActivePageIndicatorActive(bool setActive)
    {
        pageIndicator.sprite = setActive ? activeSprite : inactiveSprite;
    }
}
