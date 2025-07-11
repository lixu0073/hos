using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityPatientCardAnimationObjects : MonoBehaviour {

    public GameObject bloodSample;
    public GameObject bloodSampleAndMagnifier;
    public GameObject resoultsDelivered;
    public GameObject stork;
    public GameObject readyForLabour;
    public GameObject babySleeping;
    public GameObject babyAwaken;
    public GameObject giftBoy;
    public GameObject giftGirl;



    public void Clear()
    {
        bloodSample.SetActive(false);
        bloodSampleAndMagnifier.SetActive(false);
        resoultsDelivered.SetActive(false);
        stork.SetActive(false);
        readyForLabour.SetActive(false);
        babySleeping.SetActive(false);
        babyAwaken.SetActive(false);
        giftBoy.SetActive(false);
        giftGirl.SetActive(false);
    }
}
