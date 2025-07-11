using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenPicker : MonoBehaviour
{
    private int _idx = 0;

    public List<Sprite> backgrounds = new List<Sprite>();
    public List<Sprite> LoadingArts = new List<Sprite>();
    public GameObject backgroundObj = null;
    public GameObject loadingArtObj = null;
    [SerializeField] private bool KeepPreviousLoadingScreen = false;    

    void Start()
    {
        SetLoadingAssets(true);
    }

    public void ManualStart()
    {
        SetLoadingAssets(false);
    }

    private void SetLoadingAssets(bool check)
    {
        if (!check || !KeepPreviousLoadingScreen)
        {
            _idx = PlayerPrefs.GetInt("loadingScreenIdx", 0);
            PlayerPrefs.SetInt("loadingScreenIdx", (_idx + 1) % backgrounds.Count);
        }
        else
            _idx = (PlayerPrefs.GetInt("loadingScreenIdx", 0) - 1 + backgrounds.Count) % backgrounds.Count;

        backgroundObj.GetComponent<Image>().sprite = backgrounds[_idx];
        loadingArtObj.GetComponent<Image>().sprite = LoadingArts[_idx];
        Debug.LogFormat("<color=cyan>[LOADING SCREEN] {0} out of {1}</color> Scene name: {2}", _idx, (backgrounds.Count - 1), gameObject.scene.name);
    }

}
