using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using I2.Loc;

public class Newspaper : MonoBehaviour
{
    public static StepTag newspaperStepTag = StepTag.newspaper_1;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI articleText;
    public Image articleImage;
    public Sprite[] articleSprites;

    void OnEnable()
    {
        SetupData();
        SoundsController.Instance.PlayPatientCardOpen ();
    }

    void SetupData()
    {
        switch (newspaperStepTag)
        {
            case StepTag.newspaper_1:
                titleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/HOSPITAL_NAME_TITLE"), GameState.Get().HospitalName);
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/HOSPITAL_NAME_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.newspaper_2:
                //this is unused since 1.1.0. This texts have been moved to NL_newspaper_lvl_10
                titleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/WISE_BOOSTER_TITLE"), GameState.Get().HospitalName);
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/WISE_BOOSTER_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.epidemy_newspaper:
                titleText.text = ScriptLocalization.Get("NEWSPAPERS/TUTORIAL_EPIDEMY_TITLE");
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/TUTORIAL_EPIDEMY_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.bacteria_newspaper:
                titleText.text = ScriptLocalization.Get("NEWSPAPERS/TUTORIAL_BACTERIA_TITLE");
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/TUTORIAL_BACTERIA_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_lvl_10:
                titleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/WISE_BOOSTER_TITLE"), GameState.Get().HospitalName);
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/WISE_BOOSTER_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_lvl_11:
                titleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL11_TITLE");
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL11_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_lvl_13:
                titleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL13_TITLE");
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL13_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_lvl_19:
                titleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/LVL19_TITLE"), GameState.Get().HospitalName);
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL19_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_lvl_20:
                titleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL20_TITLE");
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL20_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_lvl_30:
                titleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL30_TITLE");
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL30_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_lvl_42:
                titleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL42_TITLE");
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/LVL42_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_epidemy:
                titleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/EPIDEMY_TITLE"), GameState.Get().HospitalName);
                articleText.text = ScriptLocalization.Get("NEWSPAPERS/EPIDEMY_ARTICLE");
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_diagnosis:
                titleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/DIAGNOSIS_TITLE"), GameState.Get().HospitalName);
                articleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/DIAGNOSIS_ARTICLE"), GameState.Get().HospitalName);
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_probe_tables:
                titleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/PROBE_TABLES_TITLE"), GameState.Get().HospitalName);
                articleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/PROBE_TABLES_ARTICLE"), GameState.Get().HospitalName);
                articleImage.sprite = articleSprites[0];
                break;
            case StepTag.NL_newspaper_patio_decos:
                titleText.text = ScriptLocalization.Get("NEWSPAPERS/PATIO_DECO_TITLE");
                articleText.text = string.Format(ScriptLocalization.Get("NEWSPAPERS/PATIO_DECO_ARTICLE"), GameState.Get().HospitalName);
                articleImage.sprite = articleSprites[0];
                break;
            default:
                Debug.LogError("INCORRECT NEWSPAPER STEP TAG! " + newspaperStepTag);
                break;
        }
    }
}