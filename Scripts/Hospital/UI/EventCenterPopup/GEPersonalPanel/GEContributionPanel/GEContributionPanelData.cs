using UnityEngine;
using UnityEngine.Events;

public class GEContributionPanelData
{
    public GEContributionPanelViewStrategy strategy = null;

    public Sprite imageSprite = null;//

    public bool isImageGrayscale = false;

    public UnityAction onMinusButtonDown = null;
    public UnityAction onMinusButtonUp = null;
    public UnityAction onPlusButtonDown = null;
    public UnityAction onPlusButtonUp = null;
    public UnityAction onContributeButtonClick = null;

    public int itemsCount = 0;
}
