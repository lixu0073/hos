using Hospital;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Maternity;

public class SampleView : MonoBehaviour
{
    #region static

    private static SampleView instance;

    public static SampleView Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of SampleView was found on scene!");
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of SampleView were found!");
        }
        else
            instance = this;
    }

    #endregion

    public Text HospitalName;
    public Text CoinsText;
    public Text DiamondsText;

    public GameObject Menu;
    public GameObject MenuLevels;

    public InputField SampleValueInput;

    public void Save()
    {
        SaveSynchronizer.Instance.InstantSave();
        //MaternityAreasMapController.MaternityMap.Save(); //matward do wyjebania
    }

    public void RedirectToMainScene()
    {
        Save();
        LocalNotificationController.Instance.CacheNotifications();
        AnalyticsController.instance.ReportChangeScene(true, "MainScene");
        UIController.get.LoadingPopupController.Open(0, 0, 0);
        AreaMapController.Map.IsoDestroy();
        SceneManager.LoadScene("RedirectToMainScene");
    }

    public void ShowTestAd()
    {
        AdsController.instance.ShowAd(AdsController.AdType.rewarded_ad_billboard, () =>
        {
            Debug.LogError("Success ad load");
        }, (ex) =>
        {
            Debug.LogError(ex.Message);
        });
    }

    public void Test_OpenBox()
    {
        UIController.getMaternity.boxOpeningPopupUI.Open(new MaternityHealingRewardBoxModel(MaternityHealingRewardBoxModel.Gender.Girl, new List<GiftReward>()
        {
            new GiftRewardPositiveEnergy(5),
            new GiftRewardMixture(3, MedicineRef.Parse("BaseElixir(1)")),
            new GiftRewardDiamond(20),
            new GiftRewardBooster(3, 1),
            new GiftRewardShovel(10),
            new GiftRewardStorageUpgrader(10, 1),
            new GiftRewardCoin(200)
        }, null, null));

        /*UIController.getMaternity.boxOpeningPopupUI.Open(new LootBoxModel(Hospital.LootBox.Box.xmas, "AWESOME", new List<GiftReward>()
        {
            new GiftRewardPositiveEnergy(5),
            new GiftRewardMixture(3, MedicineRef.Parse("BaseElixir(1)")),
            new GiftRewardDiamond(20),
            new GiftRewardBooster(3, 1),
            new GiftRewardShovel(10),
            new GiftRewardStorageUpgrader(10, 1),
            new GiftRewardCoin(200)
        }));*/
    }

    public void Test_AddMedicineToTank()
    {
        string[] data = new string[3];
        data[2] = UnityEngine.Random.Range(1, 5).ToString();
        data[1] = "BaseElixir(1)";
        SuperBundleRewardMedicine medBundle = SuperBundleRewardMedicine.GetInstance(data);
        medBundle.Collect();
    }

    public void TestLocalNotifications()
    {
        LocalNotificationController.Instance.Test();
    }

    public void ToggleMenu()
    {
        Menu.SetActive(!Menu.activeSelf);
    }

    public void ToggleMenuLevels()
    {
        MenuLevels.SetActive(!MenuLevels.activeSelf);
    }

    internal void AddSampleValueChangedListener()
    {
        SampleValueInput.onValueChanged.AddListener(delegate { OnSampleValueChanged(); });
    }

    private void OnSampleValueChanged()
    {
        MaternityGameState.Get().SampleVariable = SampleValueInput.text.ToString();
    }

    public void OpenNurseRoomCard()
    {
        StartCoroutine(UIController.getMaternity.nurseRoomCardController.Open());
    }

    public void RemoveRandomMother()
    {
        List<MaternityPatientAI> patients = MaternityPatientsHolder.Instance.GetPatientsList();
        if (patients.Count <= 0)
            return;
        MaternityPatientAI p = patients.Random();
        p.IsoDestroy();
    }

}
