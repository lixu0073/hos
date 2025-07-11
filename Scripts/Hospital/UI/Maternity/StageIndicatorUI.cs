using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageIndicatorUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI stageNumber = null;

    [SerializeField]
    private Image stageIcon = null;

    public void SetStageIndicatorActive(bool setActive)
    {
        gameObject.SetActive(setActive);
    }

    public void SetStageIndicator(string stageNo, Sprite stageImage)
    {
        stageNumber.text = stageNo;
        stageIcon.sprite = stageImage;
    }
}
