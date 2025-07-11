using System.Collections.Generic;
using UnityEngine;

public class DailyRewardCardLayoutController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private List<DailyRewardCardLayout> dailyCards;
#pragma warning restore 0649

    public void Initialize(List<DailyRewardCardData> cardData)
    {
        for (int i = 0; i < cardData.Count; ++i)
        {
            if (i <= dailyCards.Count - 1)
            {
                cardData[i].cardStrategy.SetupPopupAccordingToData(cardData[i], dailyCards[i]);
            }
        }
    }

}
