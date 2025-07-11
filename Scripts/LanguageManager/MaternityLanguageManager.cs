using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityLanguageManager : LanguageManager
{

    public override void LanguageChanged()
    {
            UIController.get.drawer.UpdateTranslation();
            UIController.get.SettingsPopUp.UpdateTranslation();
            TutorialUIController.Instance.UpdateInGameCloudTranslation();
    }
}
