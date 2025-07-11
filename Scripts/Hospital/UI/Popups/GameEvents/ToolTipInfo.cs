using TMPro;
using UnityEngine;

public class ToolTipInfo : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private TextMeshProUGUI descriptionText;
#pragma warning restore 0649

    public void SetupText(string text)
    {
        descriptionText.text = text;
    }

    public void SetupTextActive(bool setActive)
    {
        descriptionText.gameObject.SetActive(setActive);
    }
}
