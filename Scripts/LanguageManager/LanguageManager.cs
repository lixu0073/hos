using UnityEngine;
using UnityEngine.SceneManagement;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager instance;
    public I2.Loc.SetLanguage Settings;

    public delegate void OnLanguageChanged();
    public static event OnLanguageChanged OnLanguageChangedEvent;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public virtual void LanguageChanged()
    {
        if (SceneManager.GetActiveScene().name != "LoadingScene")
        { 
            UIController.getHospital.AchievementsPopUp.UpdateTranslation();
            UIController.get.drawer.UpdateTranslation();
            UIController.get.SettingsPopUp.UpdateTranslation();
            UIController.get.UpdateCrossPromotionUI();
            TutorialUIController.Instance.UpdateInGameCloudTranslation();
        }
    }

    public void ApplyLanguage(string language, bool updateTranslation = true)
    {
        Settings._Language = language;
        Settings.ApplyLanguage();
        if (updateTranslation)
        {
            LanguageManager.instance.LanguageChanged();
            OnLanguageChangedEvent?.Invoke();
        }
    }
}
