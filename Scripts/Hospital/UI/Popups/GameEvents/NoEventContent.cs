using TMPro;
using UnityEngine;

public class NoEventContent : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] TextMeshProUGUI decriptionTextTitle;
    [SerializeField] TextMeshProUGUI decriptionText;
#pragma warning restore 0649

    public void Show(bool showLowLevelText)
    {
        gameObject.SetActive(true);

        if (showLowLevelText)
        {
            decriptionTextTitle.gameObject.SetActive(true);
            decriptionText.SetText(I2.Loc.ScriptLocalization.Get("EVENTS/EVENT_LOW_LEVEL_INFO"), StandardEventConfig.GetMinimumLevelFromPartialEventData());
        }
        else
        {
            decriptionText.SetText(I2.Loc.ScriptLocalization.Get("EVENTS/NO_EVENT"));
        }

    }

    public void Hide()
    {
        decriptionTextTitle.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
