using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardAreaUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI expRewardAmount = null;

    public void SetRewardAreaActive(bool setActive)
    {
        gameObject.SetActive(setActive);
    }

    public void SetExpRewardAmount(string amount)
    {
        expRewardAmount.text = amount;
    }
}
