using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTutorials : MonoBehaviour
{
    public GameState GameState;
    private Toggle toggle;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        UpdateToggle();
        toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(toggle); });
    }
    private void OnEnable()
    {
        UpdateToggle();
        TutorialSystem.TutorialController.OnTutorialSkipToggleUpdate += UpdateToggle;
    }
    private void OnDisable()
    {
        TutorialSystem.TutorialController.OnTutorialSkipToggleUpdate -= UpdateToggle;
    }
    private void ToggleValueChanged(Toggle toggle1)
    {
        PlayerPrefs.SetInt(TutorialSystem.TutorialController.Name, toggle1.isOn ? 1 : 2);
        PlayerPrefs.Save();
        Debug.LogFormat("Show tutorials: " + (toggle1.isOn ? "<color=red>" : "<color=green>") + !toggle1.isOn + "</color>");

        if (toggle1.isOn) //INFO SKIP ALL TUTORIALS
        {
            TutorialSystem.TutorialController.ShowTutorials = false;
            //Clear all tutorials
            if (TutorialUIController.Instance)
                TutorialUIController.Instance.StopShowCoroutines();
            if (TutorialSystem.TutorialController.CurrentStep != null && TutorialSystem.TutorialModule.Controller)
            {
                TutorialSystem.TutorialModule.Controller.StartTutorialSkip(true);
            }
            //If toggle pressed while optional restart is shown, close popup.
            if (UIController.get.alertPopUp && UIController.get.alertPopUp.Type == Hospital.AlertType.OPTIONAL_RESTART)
            {
                UIController.get.alertPopUp.Exit();
            }
        }
        else //INFO RESUME TUTORIALS
        {
            if (TutorialSystem.TutorialController.CurrentStep != null && TutorialSystem.TutorialModule.Controller)
            {
                TutorialSystem.TutorialModule.Controller.StopTutorialSkip();
                TutorialSystem.TutorialModule.Controller.ReloadTutorials();
            }
        }
    }
    private void UpdateToggle()
    {
        Debug.Log("Update Toggle");
        if (toggle)
            toggle.isOn = PlayerPrefs.GetInt(TutorialSystem.TutorialController.Name) == 1;
    }


}