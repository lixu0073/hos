using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleUI;
using IsoEngine;
using Hospital;
using UnityEngine.EventSystems;

public class AllHailTheEmperor : MonoBehaviour
{
	//public HospitalAreasMapController map;
	//public EngineController eng;
	//public IsoObjectPrefabData pref;
	//public GameObject rotatableObject;
	//public string FileName;
	//public int x;
	//public int y;
	public Borders brd;
	public int x, y;

	public string save = "test save";
	//System.Random rand;

	private static AllHailTheEmperor instance;


    void Update()
    {
#if UNITY_EDITOR
        //GameState.Get().AddPositiveEnergy(GameState.Get().PositiveEnergyAmount, EconomySource.TestModeIAP);
#endif
    }
	public void TestAWSLoad()
	{
		SaveSynchronizer.Instance.LoadGame();
	}

	public void TestAWSSave()
	{
		SaveSynchronizer.Instance.MarkToSave(SavePriorities.SaveThreshold);
	}

	void Awake()
	{
		instance = this;
	}
	public static AllHailTheEmperor GetInstance()
	{
		if (instance == null)
			throw new IsoException("Fatal Failure of Reference Holder system. Delete your project and start again :v ");
		return instance;
	}

	public void LoadTest()
    {
		// here will be load from aws(Translated from Polish)
    }

	public void SaveTest()
	{
		var Id = CognitoEntry.UserID; //this needs to be changed when adding user identification and logging in via some services.(Translated from Polish)
        var p = new Save();
		p.ID = Id;

		HospitalAreasMapController.HospitalMap.SaveGame(p);
		SaveLoadController.Get().SaveGame(p);
	}

    public void ChangeVisitingMode()
    {
        //HospitalAreasMapController.Map.VisitingMode = !HospitalAreasMapController.Map.VisitingMode;
    }
	public void LoadMediumMap()
	{
		LoadMapFromSave("medium");
	}

	public void LoadDoctorWiseMap()
	{
		LoadMapFromSave("big");
        HospitalAreasMapController.HospitalMap.epidemy.LoadEpidemyDataFromString(null, new NullableTimePassedObject(0,0), false);
    }

    public void LoadMapFromSave(string fileName)
	{
		Debug.Log("LoadSaveFormFile button clicked");
		if (SaveLoadController.Get().HasSaveInResources(fileName))
		{
			Debug.Log("Trying to load from Resources !!");
			HospitalAreasMapController.HospitalMap.IsoDestroy();
			SaveLoadController.Get().LoadGameFromResource(fileName);
            HospitalAreasMapController.HospitalMap.StartGame();
		}
	}

	public void AddResources()
	{
		foreach (var type in ResourcesHolder.Get().medicines.cures)
		{
			foreach (var medicine in type.medicines)
			{
				GameState.Get().AddResource(medicine.GetMedicineRef(), 1, true, EconomySource.TestModeIAP);
			}
		}
	}
    
	// SAVE TUTORIAL FUNCTIONS
	public void LoadTutorial(int id)
	{
		HospitalAreasMapController.HospitalMap.IsoDestroy();
        SaveLoadController.Get().LoadGameForStepTutorial(id);
		HospitalAreasMapController.HospitalMap.StartGame();
	}

	public void SaveTutorial(int id)
	{
        HospitalAreasMapController.HospitalMap.SaveGame(SaveLoadController.SaveState);
    }
	public void testBorders()
	{
		brd.SetBorderSize(x, y);
	}
}
