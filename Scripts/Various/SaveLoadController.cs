using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Hospital;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Text;

class SaveLoadController : MonoBehaviour
{
    public const string WISE = "SuperWise";

    public string path = "";
    private string basePath = "";
    private static Save saveState;
    public static Save SaveState
    {
        get
        {
            if (saveState == null)
            {
                if (AnalyticsController.instance != null)
                    AnalyticsController.instance.ReportBug("hospital_reset");

                throw new IsoEngine.IsoException("Fatal Failure of SaveState. Tried to reset hospital to DefaultSave!");
            }
            return saveState;
        }
        private set
        {
            saveState = value;
        }
    }

    public void Awake()
    {
        path = Application.persistentDataPath + "/ciekawagra.zapis";
        basePath = Application.persistentDataPath + "/";
    }

    private string GetPath(string fileName)
    {
        return basePath + fileName + ".txt";
    }

    void Start()
    {
        instance = this;
    }

    //private Dictionary<string, string> GenerateData()
    //{
    //	return null;
    //}

    //public void StartNewGame()
    //{
    //	LoadGame(GenerateData());
    //}
    //public void LoadGame()
    //{
    //	var stream = File.Open(path, FileMode.Open);

    //	Dictionary<string, string> data;

    //	XmlSerializer serializer = new XmlSerializer(typeof(List<KeyValuePair<string, string>>));
    //	data = new Dictionary<string, string>();
    //	foreach (var p in (List<KeyValuePair<string, string>>)serializer.Deserialize(stream))
    //		data.Add(p.Key, p.Value);
    //	stream.Close();
    //	print("loading....");
    //	LoadGame(data);

    //}
    //matward dodac defaultsave do maternity
    public static Save GenerateDefaultSave()
    {
        var save = new Save();
        save.ID = CognitoEntry.UserID;
        save.FacebookID = "";
        save.version = LoadingGame.version;

        save.gameVersion = Application.version;
        save.maxGameVersion = Application.version;

        save.saveDateTime = (long)ServerTime.getTime();
        save.MaternitySaveDateTime = (long)ServerTime.getTime();
        var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        save.Level = 1;
        AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.Level.ToString(), 1, 1.ToString());

        save.LaboratoryObjectsData = new List<string>()
        {
            "PanCollector$(56,54)/West/working;0/0",
            "EliStore$(62,54)/South/working",
            "EliTank$(62,50)/South/working",
            "ProbTabCover$(58,52)/North/working",
        };
        save.ClinicObjectsData = new List<string>()
        {
            "2xBedsRoom$(35,29)/South/working;0?0%0?WaitForPatientSpawn%WaitForPatientSpawn"
        };

        save.PatioObjectsData = new List<string>()
        {
             "pond$(47,40)/North/working/null/",
             //"removable_patio_pack_1$(49,44)/North/working/null/",
             //"removable_patio_pack_2$(50,38)/North/working/null/",
             //"removable_patio_pack_3$(57,45)/North/working/null/",
             //"removable_patio_pack_4$(56,38)/North/working/null/",
             //"removable_patio_pack_5$(54,42)/North/working/null/",
        };

        save.TutorialStepTag = StepTag.zero.ToString();
        save.IsArrowAnimationNeededForWhiteElixir = false;
        save.ShowEmergencyIndicator = false;
        save.ShowSignIndicator = false;
        save.ShowPaintBadgeClinic = false;
        save.ShowPaintBadgeLab = false;
        save.Experience = 0;
        save.CoinAmount = 650;
        save.DiamondAmount = 40;

        //double currency for devices which had preinstalled My Hospital
        if (PlayerPrefs.GetInt("NTT") > 0)
        {
            save.CoinAmount = 1300;
            save.DiamondAmount = 80;
        }

        save.KidsToSpawn = 0;
        save.PlaygroundLevel = 0;
        save.AchievementsDone = 0;

        save.NoMoreRemindFBConnection = false;
        save.FBConnectionRewardEnabled = false;
        save.FBRewardConnectionClaimed = false;
        save.HomePharmacyVisited = false;
        save.EverLoggedInFB = false;

        HospitalAreasMapController.HospitalMap.epidemy.GenerateDefaultSave();
        save.EpidemyData = HospitalAreasMapController.HospitalMap.epidemy.SaveEpidemyDataToString();

        DailyQuestSynchronizer.Instance.GenerateDefaultSave();
        save.DailyQuest = DailyQuestSynchronizer.Instance.SaveToString();

        save.NonLinearCompletion = "";
        save.Elixirs = new List<string>();
        save.PharmacySlots = 6;

        save.AdvancedPatientCounter = GameState.Get().PatientsCount.Save();
        save.AdvancedCuresCounter = GameState.Get().CuresCount.Save();
        return save;
    }

    public bool HasSaveInResources(string fileName)
    {
        return Resources.Load(fileName) != null;
    }
    /// <summary>
    /// Returns true if the given game version is higher than the given max version. 
    /// </summary>
    public static bool CheckIfGameUpgraded(string currBundleVer, string maxBundleVer, bool maxCanBeZeroInThisCase = false)
    {
        try
        {
            if (string.IsNullOrEmpty(maxBundleVer))
                return true;
            currBundleVer = String.Join("", currBundleVer.Split('.'));
            maxBundleVer = String.Join("", maxBundleVer.Split('.'));
            int currVer = 0;
            int.TryParse(currBundleVer, out currVer);
            int maxVer = 0;
            int.TryParse(maxBundleVer, out maxVer);
            if ((maxVer <= 0 || currVer <= 0) && maxCanBeZeroInThisCase == false)
            {
                //Debug.LogError("Version not upgraded.");
                return false;
            }
            else if (currVer > maxVer)
            {
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool SaveVersionIsNewerThanGameVerion(string saveGameVersion)
    {
        if (string.IsNullOrEmpty(saveGameVersion))
        {
            Debug.LogError("saveGameVersion empty or null");
            return false;
        }
        var saveGameVersionNumbers = saveGameVersion.Split('.');
        var appVersionNumbers = Application.version.Split('.');

        for (int i = 0; i < saveGameVersionNumbers.Length && i < appVersionNumbers.Length; i++)
        {
            int saveDigit = 0;
            int appDigit = 0;

            int.TryParse(saveGameVersionNumbers[i], out saveDigit);
            int.TryParse(appVersionNumbers[i], out appDigit);

            if (saveDigit > appDigit)
                return true;
            else if (saveDigit < appDigit)
                return false;
        }

        return false;
        /*
        string saveVersion = String.Join("", saveGameVersion.Split('.'));
        string clientVersion = String.Join("", Application.version.Split('.'));
        int saveVer = 0;
        int.TryParse(saveVersion, out saveVer);
        int clientVer = 0;
        int.TryParse(clientVersion, out clientVer);
        if (saveVer > clientVer)
        {
            return true;
        }
        return false;
        */
    }

    public Save LoadGameFromResourceJSON(string fileName)
    {
        TextAsset asset = Resources.Load(fileName) as TextAsset;
        if (asset != null)
        {
            return JsonUtility.FromJson<Save>(asset.text);
        }
        return null;
    }

    public Save LoadGameFromResource(string fileName)
    {
        TextAsset asset = Resources.Load(fileName) as TextAsset;
        if (asset != null)
        {
            Stream stream = new MemoryStream(asset.bytes);
            Save data;
            XmlSerializer serializer = new XmlSerializer(typeof(Save));
            data = (Save)serializer.Deserialize(stream);
            stream.Close();
            return data;
        }
        return null;
    }

    public Save LoadSaveFromFile(string fileName)
    {
        ObscuredString cryptedSave = ObscuredPrefs.GetString(fileName);

        if (string.IsNullOrEmpty(cryptedSave))
        {
            return null;
        }
        ObscuredString key = "";

        string encryptedKey = Encoding.UTF8.GetString(Convert.FromBase64String(cryptedSave));
        key.SetEncrypted(encryptedKey);

        return JsonUtility.FromJson<Save>(key);
    }

    public void LoadGame(Save Data)
    {
        SaveState = Data;
        //Application.LoadLevel("LabScene");
    }

    public static void SaveGame(Save data, string fileName)
    {
        ObscuredString key = JsonUtility.ToJson(data);
        string encryptedKey = key.GetEncrypted();
        string encodedKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedKey));
        ObscuredPrefs.SetString(data.ID, encodedKey);
        ObscuredPrefs.Save();
    }

    public void SaveGame(Save data)
    {
        if (!File.Exists(path))
        {
            var p = File.Create(path);
            p.Close();
        }
        XmlSerializer serializer = new XmlSerializer(typeof(Save));
        TextWriter writer = new StreamWriter(path, false);
        serializer.Serialize(writer, data);
        writer.Close();
    }

    private static SaveLoadController instance = null;
    public static SaveLoadController Get()
    {
        if (instance == null)
        {
            var z = GameObject.FindObjectOfType<SaveLoadController>();
            if (z == null)
            {
                instance = new GameObject("SaveLoadController").AddComponent<SaveLoadController>();
            }
            else
                instance = z.GetComponent<SaveLoadController>();
        }
        return instance;
    }


    // METHODS FOR TUTORIAL

    public void LoadGameForStepTutorial(int id)
    {
        string tutorial_path = Application.persistentDataPath + "/" + id + ".zapis";
        var stream = File.Open(tutorial_path, FileMode.Open);

        XmlSerializer serializer = new XmlSerializer(typeof(Save));
        var data = (Save)serializer.Deserialize(stream);
        stream.Close();
        print("loading....");

        LoadGame(data);

    }

    public void ClearAllSaveStepsTutorial()
    {
        string tutorial_path = Application.persistentDataPath;

        DirectoryInfo dir = new DirectoryInfo(tutorial_path);
        FileInfo[] info = dir.GetFiles("*.zapis*");
        foreach (FileInfo f in info)
        {
            string name = f.Name;
            name = name.Replace(".zapis", "");
            float num = 0;

            if (float.TryParse(name, out num))
            {
                if (((int)num) > 1)
                    File.Delete(tutorial_path + "/" + (int)num + ".zapis");
            }
        }
    }

    public void ClearSaveStepTutorial(int id)
    {
        if (CheckTutorialSaveExist(id))
        {
            string tutorial_path = Application.persistentDataPath + "/" + id + ".zapis";
            File.Delete(tutorial_path);
        }
    }

    public bool CheckTutorialSaveExist(int id)
    {
        string tutorial_path = Application.persistentDataPath + "/" + id + ".zapis";


        if (File.Exists(tutorial_path))
        {
            return true;
        }
        else
            return false;
    }

    public Save GetSaveState()
    {
        return SaveState;
    }
}
