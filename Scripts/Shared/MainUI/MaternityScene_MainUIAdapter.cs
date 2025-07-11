using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityScene_MainUIAdapter : IMainUIAdapter
{
    public void Excucute(UIMainScale mainUI)
    {
        mainUI.boosterAnchor.gameObject.SetActive(false);
        mainUI.hintAnchor.gameObject.SetActive(false);
        mainUI.giftboxAnchor.gameObject.SetActive(false);
        mainUI.dailyQuestsAnchor.gameObject.SetActive(false);
        mainUI.objectivesAnchor.gameObject.SetActive(false);
        mainUI.friends.gameObject.SetActive(false);
    }
}
