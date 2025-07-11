using UnityEngine;
using UnityEngine.Events;

public class StandardEventPanelData
{
    public StandardEventPanelViewStrategy strategy = null;

    public int timeLeft = 0;
    public Sprite artSprite = null;
    public UnityAction onInfoButtonClick = null;
    public SEInfoPanelData[] infoPanelData = null;
}
