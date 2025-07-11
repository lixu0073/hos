using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;


public class SEInfoPanelUI : MonoBehaviour
{
    [SerializeField]
    private GameObject separator = null;

    [SerializeField]
    private Localize infoLoc = null;

    public void SetSeparatorActive(bool setActive)
    {
        separator.SetActive(setActive);
    }

    public void SetInfoLoc(string term)
    {
        infoLoc.SetTerm(term);
    }
}
