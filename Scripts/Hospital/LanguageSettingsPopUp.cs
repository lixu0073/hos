using UnityEngine;
using SimpleUI;
using I2.Loc;
using System.Collections;
using System;

namespace Hospital
{
    public class LanguageSettingsPopUp : UIElement
    {
#pragma warning disable 0649
        //this is just a temp and quick way of handling selected language
        //REFACTOR THIS WHEN MORE LANGUAGES ARE ADDED!
        [SerializeField] GameObject englishSelected;
        [SerializeField] GameObject frenchSelected;
        [SerializeField] GameObject germanSelected;
        [SerializeField] GameObject italianSelected;
        [SerializeField] GameObject spanishSelected;
        [SerializeField] GameObject russianSelected;
        [SerializeField] GameObject portugueseSelected;
        [SerializeField] GameObject chineseTraditionalSelected;
        [SerializeField] GameObject chineseSimplifiedSelected;
        [SerializeField] GameObject turkishSelected;
        [SerializeField] GameObject japaneseSelected;
        [SerializeField] GameObject koreanSelected;
        [SerializeField] GameObject polishSelected;
        [SerializeField] GameObject thaiSelected;
        [SerializeField] GameObject indonesianSelected;
        [SerializeField] GameObject dutchSelected;
        [SerializeField] GameObject norwegianSelected;
        [SerializeField] GameObject swedishSelected;
        [SerializeField] GameObject danishSelected;
#pragma warning restore 0649

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null) 
        {
            yield return base.Open();
            SetSelectedLanguage();
            whenDone?.Invoke();
        }

        public void ButtonBack()
        {
            Exit(false);
            StartCoroutine(UIController.get.SettingsPopUp.Open());
        }

        public void ButtonExit()
        {
            Exit(true);
        }

        void SetSelectedLanguage()
        {
            HideAllSelections();

            switch (LocalizationManager.CurrentLanguage)
            {
                case "English":
                    englishSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("English");
                    break;
                case "French":
                    frenchSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("French");
                    break;
                case "German":
                    germanSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("German");
                    break;
                case "Italian":
                    italianSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Italian");
                    break;
                case "Spanish":
                    spanishSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Spanish");
                    break;
                case "Russian":
                    russianSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Russian");
                    break;
                case "Portuguese":
                    portugueseSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Portuguese");
                    break;
                case "Chinese traditional":
                    chineseTraditionalSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Chinese traditional");
                    break;
                case "Chinese simplified":
                    chineseSimplifiedSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Chinese simplified");
                    break;
                case "Turkish":
                    turkishSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Turkish");
                    break;
                case "Japanese":
                    japaneseSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Japanese");
                    break;
                case "Korean":
                    koreanSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Korean");
                    break;
                case "Polish":
                    polishSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Polish");
                    break;
                case "Thai":
                    thaiSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Thai");
                    break;
                case "Indonesian":
                    indonesianSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Indonesian");
                    break;
                case "Dutch":
                    dutchSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Dutch");
                    break;
                case "Norwegian":
                    norwegianSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Norwegian");
                    break;
                case "Swedish":
                    swedishSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Swedish");
                    break;
                case "Danish":
                    danishSelected.SetActive(true);
                    LanguageManager.instance.ApplyLanguage("Danish");
                    break;
                default:
                    break;
            }
            PublicSaveManager.Instance.UpdatePublicSaveForEvent();
        }

        public void ButtonEnglish()
        {
            if (LocalizationManager.HasLanguage("English"))
                LocalizationManager.CurrentLanguage = "English";

            SetSelectedLanguage();
        }

        public void ButtonFrench()
        {
            if (LocalizationManager.HasLanguage("French"))
                LocalizationManager.CurrentLanguage = "French";

            SetSelectedLanguage();
        }

        public void ButtonLanguage(string language)
        {
            if (LocalizationManager.HasLanguage(language))
                LocalizationManager.CurrentLanguage = language;

            SetSelectedLanguage();
            PlayerPrefs.SetString("userLanguage", LocalizationManager.CurrentLanguage);
        }

        void HideAllSelections()
        {
            englishSelected.SetActive(false);
            frenchSelected.SetActive(false);
            germanSelected.SetActive(false);
            italianSelected.SetActive(false);
            spanishSelected.SetActive(false);
            russianSelected.SetActive(false);
            portugueseSelected.SetActive(false);
            chineseTraditionalSelected.SetActive(false);
            chineseSimplifiedSelected.SetActive(false);
            turkishSelected.SetActive(false);
            japaneseSelected.SetActive(false);
            koreanSelected.SetActive(false);
            polishSelected.SetActive(false);
            thaiSelected.SetActive(false);
            indonesianSelected.SetActive(false);
            dutchSelected.SetActive(false);
            norwegianSelected.SetActive(false);
            swedishSelected.SetActive(false);
            danishSelected.SetActive(false);
        }
    }
}
