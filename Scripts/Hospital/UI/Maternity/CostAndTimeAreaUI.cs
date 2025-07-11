using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CostAndTimeAreaUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI costText = null;
    [SerializeField]
    private TextMeshProUGUI timeText = null;

    public void SetCostAndTimeAreaActive(bool setActive)
    {
        gameObject.SetActive(setActive);
    }

    public void SetCostAndTimeArea(string cost, string time)
    {
        costText.text = cost;
        timeText.text = time;
    }
}
