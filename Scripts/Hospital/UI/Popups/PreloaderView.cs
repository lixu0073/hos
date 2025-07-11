using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PreloaderView : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI loadingText;

    public void Open(PreloadViewMode mode)
    {
        switch (mode)
        {
            case PreloadViewMode.Ads:
                loadingText.text = I2.Loc.ScriptLocalization.Get("LOADING_VIDEO");
                break;
            case PreloadViewMode.DeltaDnaPopup:
                loadingText.text = I2.Loc.ScriptLocalization.Get("LOADING");
                break;
            default:
                loadingText.text = I2.Loc.ScriptLocalization.Get("LOADING");
                break;
        }

        if (gameObject.activeSelf)
            return;
        if(DefaultConfigurationProvider.GetConfigCData().PreloaderBeforeAdsEnabled || mode == PreloadViewMode.DeltaDnaPopup)
            gameObject.SetActive(true);
    }

    public void Exit()
    {
        gameObject.SetActive(false);
    }


    public enum PreloadViewMode
    {
        Defauult,
        Ads,
        DeltaDnaPopup,
    }
}
