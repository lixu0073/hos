using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Hospital;

public class SceneLoader : MonoBehaviour
{

    AsyncOperation asyncOperation = null;

    public Image slider;
    public TextMeshProUGUI LoadingText;

    public float maxSlider;
    public string sceneName;

    protected virtual void Start()
    {
        LoadingText.text = I2.Loc.ScriptLocalization.Get("LOADING");
        Redirect();
    }

    public void Redirect()
    {
        MaternityUIController.get = null;
        MaternityAreasMapController.ClearInstance();
        MaternityBloodTestRoomController.ClearInstance();
        MaternityLabourRoomController.ClearInstance();
        MaternityTutorialController.ClearInstance();
        MaternityWaitingRoomController.ClearInstance();
        MaternityPatientsHolder.ClearInstance();

        Resources.UnloadUnusedAssets();
        asyncOperation = SceneManager.LoadSceneAsync(sceneName);
    }

    protected virtual void Update()
    {
        if(asyncOperation != null)
        {
            ProgressChanged(asyncOperation.progress);
        }
    }
    
    private void ProgressChanged(float value)
    {
        value = Normalize(value);
        slider.fillAmount = value;
    }

    private float Normalize(float value)
    {
        return maxSlider * value;
    }

}
