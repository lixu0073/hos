using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using I2.Loc;

public class StandardEventPanelUI : MonoBehaviour
{
    [TermsPopup]
    [SerializeField]
    private string timerTerm = "-";

    [SerializeField]
    private TextMeshProUGUI timeLeftText = null;

    [SerializeField]
    private ButtonUI infoButton = null;

    [SerializeField]
    private Image eventArt = null;

    [SerializeField]
    private SEInfoController infoController = null;

    public void SetTimeLeftText(int timeLeft)
    {
        timeLeftText.text = string.Format(I2.Loc.ScriptLocalization.Get(timerTerm), UIController.GetFormattedShortTime(timeLeft));
    }

    public void SetEventArt(Sprite artSprite)
    {
        eventArt.sprite = artSprite;
    }

    public void SetInfoButton(UnityAction onClick)
    {
        infoButton.SetButton(onClick);
    }

    public void SetInfoPanel(SEInfoPanelData[] data)
    {
        infoController.SetInfoVisible(false);
        infoController.ClearPanel();
        if (data == null)
        {
            return;
        }
        for (int i = 0; i < data.Length; ++i)
        {
            infoController.AddView(data[i]);
        }
    }
}
