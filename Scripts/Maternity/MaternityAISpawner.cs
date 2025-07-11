using Hospital;
using IsoEngine;
using Maternity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityAISpawner : MonoBehaviour
{

    #region static

    private static MaternityAISpawner instance;

    public static MaternityAISpawner Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of MaternityAISpawner was found on scene!");
            return instance;
        }

    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of MaternityAISpawner entrypoint were found!");
        }
        instance = this;
    }

    #endregion

    //matward TODO
    public CharacterCreator creator;
    public List<Vector2i> startingPoints;

    private int motherPatientCounter = 0;
    private int babyPatientCounter = 0;

    public MaternityPatientAI LoadMother(RotatableObject rotatableObject, string info, string extendedInfo, string relativeInfo)
    {
        GameObject goPatient = creator.DefinedMother(extendedInfo);
        goPatient.GetComponent<MaternityCharacterInfo>().FromString(info.Split('!')[3]);
        var AI = goPatient.AddComponent<MaternityPatientAI>();
        AI.transform.rotation = Quaternion.Euler(45, 45, 0);
        if (!string.IsNullOrEmpty(relativeInfo))
        {
            AI.LoadRelatives(relativeInfo);
        }
        AI.Initialize(rotatableObject, info);
        goPatient.name = "Mother" + motherPatientCounter.ToString();
        goPatient.transform.SetParent(this.transform);
        goPatient.SetActive(true);
        ++motherPatientCounter;
        return AI;
    }

    public void LoadMotherRelative(MaternityPatientAI mother, RelativesType relativeType, string relativeInfo)
    {
        string[] relativeDetailedData = relativeInfo.Split('|');
        switch (relativeType)
        {
            case RelativesType.Baby:
                LoadBaby(mother, relativeDetailedData[0], relativeDetailedData[1]);
                break;
            default:
                break;
        }
    }

    public MaternityPatientAI SpawnMother(Vector2i position, MaternityWaitingRoom destRoom)
    {
        GameObject goPatient = creator.CreateRandomMotherPatient(BaseGameState.RandomNumber(0, 3), 4);
        var AI = goPatient.AddComponent<MaternityPatientAI>();
        goPatient.GetComponent<MaternityCharacterInfo>().Randomize();
        AI.Initialize(position, destRoom);
        AI.transform.rotation = Quaternion.Euler(45, 45, 0);
        goPatient.name = "Mother" + motherPatientCounter.ToString();
        goPatient.transform.SetParent(this.transform);
        goPatient.SetActive(true);
        ++motherPatientCounter;
        return AI;
    }

    public BabyPatient SpawnBaby(MaternityPatientAI mother)
    {
        GameObject goPatient = creator.CreateBaby(mother);
        SetupBabyPositoin(mother, goPatient);
        var AI = goPatient.AddComponent<BabyPatient>();
        goPatient.name = "Baby" + babyPatientCounter.ToString();
        goPatient.SetActive(true);
        goPatient.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        goPatient.transform.localPosition = Vector3.zero;
        AI.Initialize(mother);
        ++babyPatientCounter;
        mother.AddRelative(AI);
        return AI;
    }

    public BabyPatient LoadBaby(MaternityPatientAI mother, string info, string extendedInfo)
    {
        GameObject goBaby = creator.DefinedBaby(extendedInfo);
        SetupBabyPositoin(mother, goBaby);
        var AI = goBaby.AddComponent<BabyPatient>();
        goBaby.name = "Baby" + babyPatientCounter.ToString();
        goBaby.SetActive(true);
        goBaby.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        goBaby.transform.localPosition = Vector3.zero;
        AI.InitializeFromString(mother, info);
        mother.AddRelative(AI);
        ++babyPatientCounter;
        return AI;
    }

    private static void SetupBabyPositoin(MaternityPatientAI mother, GameObject baby)
    {
        baby.transform.rotation = Quaternion.Euler(45, 45, 0);
        baby.transform.SetParent(mother.GetMotherBabyPosition());
        baby.transform.localPosition = Vector3.zero;
    }

    public MaternityPatientAI SpawnMother(MaternityWaitingRoom destRoom)
    {
        return SpawnMother(MaternityCoreLoopParametersHolder.GetMotherSpawnPosition(), destRoom);
    }

    public void OnButtonDevTest()
    {
        SpawnMother(MaternityCoreLoopParametersHolder.GetMotherSpawnPosition(), null);
    }
}
