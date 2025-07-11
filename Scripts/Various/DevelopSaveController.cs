using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class DevelopSaveController : MonoBehaviour
{

    public Dropdown VersionDropdown;
    public Dropdown SavesDropdown;

    public static bool IsEnabled = false;

    [Serializable]
    public struct VersionSave
    {
        public string Version;
        public string[] Saves;
    }
    public VersionSave[] Versions;

    private DevelopSaveLoadController Loader = new DevelopSaveLoadController();

    void Start()
    {
        IsEnabled = true;
        VersionDropdown.ClearOptions();
        SetVersionsOptions();
        VersionDropdown.onValueChanged.AddListener(delegate
        {
            OnVersionSelect(VersionDropdown.value);
        });
	}

    private void SetVersionsOptions()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        foreach(string version in Loader.GetVersions(Versions))
        {
            options.Add(new Dropdown.OptionData(version));
        }
        VersionDropdown.AddOptions(options);
        if(options.Count > 0)
        {
            OnVersionSelect(0);
        }
    }

    private void OnVersionSelect(int index)
    {
        SavesDropdown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        foreach (string save in Loader.GetSaves(VersionDropdown.options[index].text, Versions))
        {
            options.Add(new Dropdown.OptionData(save));
        }
        SavesDropdown.AddOptions(options);
    }

    public void ChooseSave()
    {
        int versionIndex = VersionDropdown.value;
        int saveIndex = SavesDropdown.value;
        string version = VersionDropdown.options[versionIndex].text;
        string saveName = SavesDropdown.options[saveIndex].text;
        DevelopSaveHolder.Instance.ForcedCognito = "_" + version + "_" + saveName;
        DevelopSaveHolder.Instance.SaveName = saveName;
        DevelopSaveHolder.Instance.Version = version;
        RunNormal();
    }

    public void RunNormal()
    {
        SceneManager.LoadScene("LoadingScene");
    }

}
