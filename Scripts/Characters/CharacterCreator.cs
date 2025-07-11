using UnityEngine;
using System.Collections.Generic;
using System.Text;
//using System;
using Hospital;
using Maternity;
using System;

/// <summary>
/// 性别类型枚举，定义角色的性别选项
/// </summary>
public enum GenderTypes { 
    /// <summary>男性</summary>
    Man, 
    /// <summary>女性</summary>
    Woman 
}

/// <summary>
/// 角色创建器，负责创建和管理游戏中所有类型的角色（病人、医生、护士等）。
/// 提供角色外观随机化、种族和性别选择、角色池管理等功能。
/// </summary>
public class CharacterCreator : MonoBehaviour
{
    public GameObject ClinicCharacterPrefab;
    public GameObject HospitalCharacterPrefab;
    public GameObject DoctorPrefab;
    public GameObject NursePrefab;
    public GameObject MotherCharacterPrefab;
    public GameObject BabyCharacterPrefab;

    public GameObject[] Pooler;

    private GenderTypes ChooseGender;

    public enum RaceTypes { White, Black, Asian };
    public RaceTypes ChooseRace;

    private enum PersonType { Doctor, Patient, Kid, Nurse, Mother, Baby }
    private PersonType ChoosePersonType;

    private SpriteRenderer AvatarHead;
    private SpriteRenderer AvatarBody;
    private SpriteRenderer HeadFront;
    private SpriteRenderer HeadBack;
    private SpriteRenderer TorsoFront;
    private SpriteRenderer TorsoBack;
    private SpriteRenderer LowerBodyFront;
    private SpriteRenderer ArmsL;
    private SpriteRenderer HandsLFront;
    private SpriteRenderer ArmsR;
    private SpriteRenderer HandsRFront;
    private SpriteRenderer ThighL;
    private SpriteRenderer CalfL;
    private SpriteRenderer FootLFront;
    private SpriteRenderer ThighR;
    private SpriteRenderer CalfR;
    private SpriteRenderer FootRFront;
    private SpriteRenderer ApronFL;
    private SpriteRenderer ApronB;
    private SpriteRenderer ApronF;
    private SpriteRenderer ApronFR;
    private SpriteRenderer pregnantBellyFront;
    private SpriteRenderer pregnatnBellyBack;
    private SpriteRenderer babyEyeLids;


    [System.Serializable]
    private class VIPTime
    {
        public int Min = 0;
        public int Max = 100;
    }

    [SerializeField]
    private VIPTime VipTimeOut = new VIPTime();
    private MainList partsList;
    BaseCharacterInfo info;

    private GameObject Character;
    private int indexRand;

    public GameObject RandomPatient(bool male = true)
    {
        return RandomClinicCharacter(GameState.RandomNumber(0, 3), male ? 0 : 1, 1, false);
    }

    public GameObject FirstEverPatient()
    {
        return DefinedClinicCharacter("0*0*1*False*0*29*NAME_BOB*SURNAME_WRITE*7*7*1");
    }

    public GameObject RandomKid()
    {
        //UnityEngine.Random.seed = (int)ServerTime.Get().GetServerTime().Ticks;
        UnityEngine.Random.InitState((int)ServerTime.Get().GetServerTime().Ticks);

        return RandomClinicCharacter(GameState.RandomNumber(0, 3), GameState.RandomNumber(0, 2), 2, true);
    }
    public GameObject RandomClinicCharacter(int race, int gender, int type, int headVal, int bodyVal, int legsVal)
    {
        Character = CharactersList.instance.GetInactiveClinicPatient();

        info = Character.GetComponent<ClinicCharacterInfo>();
        var p = RandomCharacterz(race, gender, type, headVal, bodyVal, legsVal);

        Character.SetActive(true);
        return p;
    }

    public GameObject RandomClinicCharacter(int race, int gender, int type, bool isKid)
    {
        Character = CharactersList.instance.GetInactiveClinicPatient();

        info = Character.GetComponent<ClinicCharacterInfo>();
        var p = RandomCharacterz(race, gender, type, false, isKid);
        Character.SetActive(true);
        return p;
    }

    public GameObject TimmyCharacter(int race, int gender, int type)
    {
        Character = CharactersList.instance.GetInactiveClinicPatient();

        info = Character.GetComponent<ClinicCharacterInfo>();
        var p = RandomCharacterz(race, gender, type, false, true);

        Character.SetActive(true);
        return p;
    }

    public GameObject TimmyPatient()
    {
        return DefinedClinicCharacter("0*0*1*False*0*29*Timmy*Jones*7*7*1");
    }

    public GameObject RandomHospitalCharacter(int race, int gender, int type, bool vip = false)
    {
        Character = CharactersList.instance.GetInactiveHospitalPatient();
        Character.SetActive(true);

        info = Character.GetComponent<HospitalCharacterInfo>();
        var p = RandomCharacterz(race, gender, type, vip);
        info.Initialize();

        return p;
    }

    public GameObject CreateRandomMotherPatient(int race, int type)
    {
        Character = Instantiate(MotherCharacterPrefab, Vector3.zero, Quaternion.identity);
        info = Character.GetComponent<MaternityCharacterInfo>();
        info.Initialize();
        var p = RandomMother(race, 1, type);
        return p;
    }

    public GameObject DefinedMother(string BioInfo)
    {
        Character = Instantiate(MotherCharacterPrefab, Vector3.zero, Quaternion.identity);
        info = Character.GetComponent<MaternityCharacterInfo>();
        info.Initialize();
        var p = GenerateDefinedMother(BioInfo);
        return p;
    }

    public GameObject CreateBaby(MaternityPatientAI mother)
    {
        Character = Instantiate(BabyCharacterPrefab, Vector3.zero, Quaternion.identity);
        info = Character.GetComponent<BabyCharacterInfo>();
        info.Initialize();
        var p = GenerateBaby(mother);
        return p;
    }

    public GameObject DefinedBaby(string Bioinfo)
    {
        Character = Instantiate(BabyCharacterPrefab, Vector3.zero, Quaternion.identity);
        info = Character.GetComponent<BabyCharacterInfo>();
        info.Initialize();
        var p = GenerateDefniedBaby(Bioinfo);
        return p;
    }

    private GameObject GenerateDefniedBaby(string bioinfo)
    {
        info.personalBIO = bioinfo;

        var personBIO = bioinfo.Split('*');

        HeadFront = Character.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        HeadBack = Character.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        TorsoFront = Character.transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();
        TorsoBack = Character.transform.GetChild(0).GetChild(0).GetChild(4).GetComponent<SpriteRenderer>();
        ArmsR = Character.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        HandsRFront = Character.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
        ArmsL = Character.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        HandsLFront = Character.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetComponent<SpriteRenderer>();
        LowerBodyFront = Character.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();

        ThighR = Character.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
        CalfR = Character.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
        FootRFront = Character.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();

        ThighL = Character.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
        CalfL = Character.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        FootLFront = Character.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();

        babyEyeLids = Character.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        //here is anim prop which sprite is set from anim
        AvatarHead = Character.transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        AvatarBody = Character.transform.GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();

        ChooseRace = (RaceTypes)int.Parse(personBIO[0], System.Globalization.CultureInfo.InvariantCulture);
        ChooseGender = (GenderTypes)int.Parse(personBIO[1], System.Globalization.CultureInfo.InvariantCulture);
        ChoosePersonType = (PersonType)int.Parse(personBIO[2], System.Globalization.CultureInfo.InvariantCulture);

        info.Name = personBIO[6];
        info.Surname = personBIO[7];
        info.Sex = int.Parse(personBIO[1], System.Globalization.CultureInfo.InvariantCulture);
        info.Race = int.Parse(personBIO[0], System.Globalization.CultureInfo.InvariantCulture);
        info.Age = int.Parse(personBIO[5], System.Globalization.CultureInfo.InvariantCulture);

        if (bool.Parse(personBIO[3]))
        {
            info.IsVIP = true;
            info.VIPTime = int.Parse(personBIO[4], System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            info.IsVIP = false;
            info.VIPTime = 0;
        }

        partsList = Pooler[6].GetComponent<MainList>();

        MainList.Gender temp = partsList.Races[(int)ChooseRace].Genders[(int)ChooseGender];

        MainList.Head head = temp.HeadList[int.Parse(personBIO[8], System.Globalization.CultureInfo.InvariantCulture)];
        HeadFront.sprite = head.HeadContainer.front;
        HeadBack.sprite = head.HeadContainer.back;

        MainList.Body body = temp.BodyList[int.Parse(personBIO[9], System.Globalization.CultureInfo.InvariantCulture)];
        TorsoFront.sprite = body.BodyContainer.front;
        TorsoBack.sprite = body.BodyContainer.back;
        ArmsL.sprite = body.Arms.Arm;
        HandsLFront.sprite = body.Arms.HandLeft;
        ArmsR.sprite = body.Arms.Arm;
        HandsRFront.sprite = body.Arms.HandRight;
        AvatarBody.sprite = body.Avatar;
        AvatarHead.sprite = head.Avatar;
        info.AvatarBody = body.Avatar;
        info.AvatarHead = head.Avatar;
        pregnantBellyFront.sprite = body.PregnantBodyContainer.front;
        pregnatnBellyBack.sprite = body.PregnantBodyContainer.back;

        MainList.Legs legs = temp.LegsList[int.Parse(personBIO[10])];
        LowerBodyFront.sprite = legs.LowerBodyFront;
        ThighL.sprite = legs.Thigh;
        CalfL.sprite = legs.Calf;
        FootLFront.sprite = legs.FootFront;
        ThighR.sprite = legs.Thigh;
        CalfR.sprite = legs.Calf;
        FootRFront.sprite = legs.FootFront;

        if (personBIO.Length > 13)
        {
            info.Likes = int.Parse(personBIO[11], System.Globalization.CultureInfo.InvariantCulture);
            info.Dislikes = int.Parse(personBIO[12], System.Globalization.CultureInfo.InvariantCulture);
            info.BloodType = (BloodType)int.Parse(personBIO[13], System.Globalization.CultureInfo.InvariantCulture);
        }
        return Character;
    }

    private GameObject RandomMother(int race, int gender, int type, bool vip = false, bool isKid = false)
    {
        StringBuilder builderPersonalBIO = new StringBuilder();

        if (type == 0)
        {
            Debug.LogError("CREATING DOCTOR WITH WRONG METHOD");
        }

        HeadFront = Character.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        HeadBack = Character.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        TorsoFront = Character.transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        TorsoBack = Character.transform.GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();
        ArmsR = Character.transform.GetChild(0).GetChild(4).GetComponent<SpriteRenderer>();
        HandsRFront = Character.transform.GetChild(0).GetChild(5).GetComponent<SpriteRenderer>();
        ArmsL = Character.transform.GetChild(0).GetChild(6).GetComponent<SpriteRenderer>();
        HandsLFront = Character.transform.GetChild(0).GetChild(7).GetComponent<SpriteRenderer>();
        LowerBodyFront = Character.transform.GetChild(0).GetChild(8).GetComponent<SpriteRenderer>();
        ThighR = Character.transform.GetChild(0).GetChild(9).GetComponent<SpriteRenderer>();
        CalfR = Character.transform.GetChild(0).GetChild(10).GetComponent<SpriteRenderer>();
        FootRFront = Character.transform.GetChild(0).GetChild(11).GetComponent<SpriteRenderer>();
        ThighL = Character.transform.GetChild(0).GetChild(12).GetComponent<SpriteRenderer>();
        CalfL = Character.transform.GetChild(0).GetChild(13).GetComponent<SpriteRenderer>();
        FootLFront = Character.transform.GetChild(0).GetChild(14).GetComponent<SpriteRenderer>();
        pregnantBellyFront = Character.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<SpriteRenderer>();
        pregnatnBellyBack = Character.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<SpriteRenderer>();
        //here is anim prop which sprite is set from anim
        AvatarHead = Character.transform.GetChild(0).GetChild(16).GetComponent<SpriteRenderer>();
        AvatarBody = Character.transform.GetChild(0).GetChild(17).GetComponent<SpriteRenderer>();
        ChooseRace = (RaceTypes)race;
        ChooseGender = (GenderTypes)gender;
        ChoosePersonType = (PersonType)type;

        info.Name = GetRandomName(gender, race);
        info.Surname = GetRandomSurname(race);
        info.BloodType = GetRandomBloodType();
        info.Likes = GetMotherRandomLikes();
        info.Dislikes = GetRandomDislikes();
        info.Sex = gender;
        info.Race = race;
        info.Age = GetRandomAge(isKid);
        info.Type = type;


        if (vip)
        {
            info.IsVIP = true;
            info.VIPTime = GameState.RandomNumber(VipTimeOut.Min, VipTimeOut.Max);
        }
        else
        {
            info.IsVIP = false;
            info.VIPTime = 0;
        }

        if ((int)ChoosePersonType == 0)
        {
            partsList = Pooler[0].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 1)
            partsList = Pooler[1].GetComponent<MainList>();
        else if ((int)ChoosePersonType == 2)
        {
            partsList = Pooler[2].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 3)
        {
            partsList = Pooler[3].GetComponent<MainList>();
            ChooseGender = (GenderTypes)1;
        }
        else if ((int)ChoosePersonType == 4)
        {
            partsList = Pooler[5].GetComponent<MainList>();
        }

        builderPersonalBIO.Append(race.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(gender.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(type.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(vip.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.VIPTime.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Age.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Name);
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Surname);
        builderPersonalBIO.Append("*");


        MainList.Gender temp = partsList.Races[(int)ChooseRace].Genders[(int)ChooseGender];

        if ((int)(int)ChooseRace == 0 && (int)ChooseGender == 0)
        {  // beware bad Hipster
            indexRand = BaseGameState.RandomNumber(0, temp.HeadList.Count - 1);
        }
        else
        {
            indexRand = BaseGameState.RandomNumber(0, temp.HeadList.Count);
        }

        builderPersonalBIO.Append(indexRand.ToString()); // head
        builderPersonalBIO.Append("*");

        MainList.Head head = temp.HeadList[indexRand];
        HeadFront.sprite = head.HeadContainer.front;
        HeadBack.sprite = head.HeadContainer.back;

        if ((int)ChooseRace == 0 && (int)ChooseGender == 0)
        { //beware bad Hipster
            indexRand = BaseGameState.RandomNumber(0, temp.BodyList.Count - 1);
        }
        else
        {
            indexRand = BaseGameState.RandomNumber(0, temp.BodyList.Count);
        }

        indexRand = BaseGameState.RandomNumber(0, temp.BodyList.Count);
        builderPersonalBIO.Append(indexRand.ToString()); // body
        builderPersonalBIO.Append("*");

        MainList.Body body = temp.BodyList[indexRand];
        TorsoFront.sprite = body.BodyContainer.front;
        TorsoBack.sprite = body.BodyContainer.back;
        ArmsL.sprite = body.Arms.Arm;
        HandsLFront.sprite = body.Arms.HandLeft;
        ArmsR.sprite = body.Arms.Arm;
        HandsRFront.sprite = body.Arms.HandRight;
        AvatarBody.sprite = body.Avatar;
        AvatarHead.sprite = head.Avatar;
        info.AvatarBody = body.Avatar;
        info.AvatarHead = head.Avatar;
        pregnantBellyFront.sprite = body.PregnantBodyContainer.front;
        pregnatnBellyBack.sprite = body.PregnantBodyContainer.back;

        //Debug.Log("Body Index: " + indexRand + " race: " + (int)ChooseRace + " gen: " + (int)ChooseGender);
        if (type == 1) //adult patient
            indexRand = GetLegsIndex(indexRand, (int)ChooseRace, (int)ChooseGender, temp);
        else
            indexRand = BaseGameState.RandomNumber(0, temp.LegsList.Count);

        //Debug.Log("Legs list count = " + temp.LegsList.Count);
        builderPersonalBIO.Append(indexRand.ToString()); // legs
        builderPersonalBIO.Append("*");
        //builderPersonalBIO.Append("*");
        MainList.Legs legs = temp.LegsList[indexRand];

        LowerBodyFront.sprite = legs.LowerBodyFront;
        ThighL.sprite = legs.Thigh;
        CalfL.sprite = legs.Calf;
        FootLFront.sprite = legs.FootFront;
        ThighR.sprite = legs.Thigh;
        CalfR.sprite = legs.Calf;
        FootRFront.sprite = legs.FootFront;

        builderPersonalBIO.Append(info.Likes.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Dislikes.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(((int)info.BloodType).ToString());

        info.personalBIO = builderPersonalBIO.ToString();


        return Character;
    }
    private GameObject GenerateBaby(MaternityPatientAI mother)
    {
        StringBuilder builderPersonalBIO = new StringBuilder();

        HeadFront = Character.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        HeadBack = Character.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        TorsoFront = Character.transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();
        TorsoBack = Character.transform.GetChild(0).GetChild(0).GetChild(4).GetComponent<SpriteRenderer>();
        ArmsR = Character.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        HandsRFront = Character.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
        ArmsL = Character.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        HandsLFront = Character.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetComponent<SpriteRenderer>();
        LowerBodyFront = Character.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();

        ThighR = Character.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
        CalfR = Character.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
        FootRFront = Character.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();

        ThighL = Character.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
        CalfL = Character.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        FootLFront = Character.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();

        babyEyeLids = Character.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        //here is anim prop which sprite is set from anim
        AvatarHead = Character.transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        AvatarBody = Character.transform.GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();

        MaternityCharacterInfo motherInfo = mother.GetComponent<MaternityCharacterInfo>();

        ChooseRace = (RaceTypes)motherInfo.Race;
        ChooseGender = (GenderTypes)BaseGameState.RandomNumber(0, 2);
        ChoosePersonType = PersonType.Baby;

        info.Name = GetRandomName((int)ChooseGender, (int)ChooseRace);
        info.Surname = "";// baby has no surname motherInfo.Surname;
        info.BloodType = GetRandomBloodType();
        info.Likes = GetBabyRandomLikes();
        info.Dislikes = 0;
        info.Sex = (int)ChooseGender;
        info.Race = (int)ChooseRace;
        info.Age = 0;
        info.Type = (int)ChoosePersonType;
        info.IsVIP = false;
        info.VIPTime = 0;

        partsList = Pooler[6].GetComponent<MainList>();

        builderPersonalBIO.Append(info.Race.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Sex.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Type.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.IsVIP.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.VIPTime.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Age.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Name);
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Surname);
        builderPersonalBIO.Append("*");


        MainList.Gender temp = partsList.Races[(int)ChooseRace].Genders[(int)ChooseGender];
        indexRand = BaseGameState.RandomNumber(0, temp.HeadList.Count);

        builderPersonalBIO.Append(indexRand.ToString()); // head
        builderPersonalBIO.Append("*");

        MainList.Head head = temp.HeadList[indexRand];
        HeadFront.sprite = head.HeadContainer.front;
        HeadBack.sprite = head.HeadContainer.back;
        babyEyeLids.sprite = head.EyeLids;

        indexRand = BaseGameState.RandomNumber(0, temp.BodyList.Count);

        indexRand = BaseGameState.RandomNumber(0, temp.BodyList.Count);
        builderPersonalBIO.Append(indexRand.ToString()); // body
        builderPersonalBIO.Append("*");

        MainList.Body body = temp.BodyList[indexRand];
        TorsoFront.sprite = body.BodyContainer.front;
        TorsoBack.sprite = body.BodyContainer.back;
        ArmsL.sprite = body.Arms.Arm;
        HandsLFront.sprite = body.Arms.HandLeft;
        ArmsR.sprite = body.Arms.Arm;
        HandsRFront.sprite = body.Arms.HandRight;
        AvatarBody.sprite = body.Avatar;
        AvatarHead.sprite = head.Avatar;
        info.AvatarBody = body.Avatar;
        info.AvatarHead = head.Avatar;

        indexRand = BaseGameState.RandomNumber(0, temp.LegsList.Count);

        builderPersonalBIO.Append(indexRand.ToString()); // legs
        builderPersonalBIO.Append("*");

        MainList.Legs legs = temp.LegsList[indexRand];
        LowerBodyFront.sprite = legs.LowerBodyFront;
        ThighL.sprite = legs.Thigh;
        CalfL.sprite = legs.Calf;
        FootLFront.sprite = legs.FootFront;
        ThighR.sprite = legs.Thigh;
        CalfR.sprite = legs.Calf;
        FootRFront.sprite = legs.FootFront;

        builderPersonalBIO.Append(info.Likes.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Dislikes.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(((int)info.BloodType).ToString());

        info.personalBIO = builderPersonalBIO.ToString();
        return Character;
    }

    public GameObject DefinedClinicCharacter(string BioInfo)
    {
        Character = CharactersList.instance.GetInactiveClinicPatient();
        info = Character.GetComponent<ClinicCharacterInfo>();
        var p = DefinedCharacter(BioInfo);
        Character.SetActive(true);

        return p;
    }

    public GameObject DefinedHospitalCharacter(string BioInfo)
    {
        Character = CharactersList.instance.GetInactiveHospitalPatient();
        Character.SetActive(true);

        info = Character.GetComponent<HospitalCharacterInfo>();

        var p = DefinedCharacter(BioInfo);
        info.Initialize();
        return p;
    }

    public GameObject CreateDoctor(string doctorRoomTag)
    {
        int race = 0;
        int gender = 0;
        int headIndex = 0;
        int bodyIndex = 0;
        int legsIndex = 0;

        switch (doctorRoomTag)
        {
            case "BlueDoc":
                race = 0;
                gender = 0;
                headIndex = 0;
                bodyIndex = 0;
                legsIndex = 2;
                break;
            case "YellowDoc":
                race = 1;
                gender = 1;
                headIndex = 0;
                bodyIndex = 0;
                legsIndex = 0;
                break;
            case "GreenDoc":
                race = 1;
                gender = 0;
                headIndex = 0;
                bodyIndex = 0;
                legsIndex = 3;
                break;
            case "PinkDoc":
                race = 1;
                gender = 0;
                headIndex = 1;
                bodyIndex = 1;
                legsIndex = 2;
                break;
            case "PurpleDoc":
                race = 0;
                gender = 0;
                headIndex = 1;
                bodyIndex = 1;
                legsIndex = 1;
                break;
            case "RedDoc":
                race = 0;
                gender = 0;
                headIndex = 3;
                bodyIndex = 3;
                legsIndex = 1;
                break;
            case "SkyDoc":
                race = 0;
                gender = 0;
                headIndex = 2;
                bodyIndex = 2;
                legsIndex = 2;
                break;
            case "SunnyDoc":
                race = 0;
                gender = 1;
                headIndex = 0;
                bodyIndex = 0;
                legsIndex = 0;
                break;
            case "WhiteDoc":
                race = 2;
                gender = 0;
                headIndex = 0;
                bodyIndex = 0;
                legsIndex = 3;
                break;
            default:
                break;
        }

        Character = CharactersList.instance.GetInactiveDoctor();
        Character.SetActive(true);

        Character.transform.rotation = Quaternion.Euler(new Vector3(45, 45, 0));
        info = Character.GetComponent<BaseCharacterInfo>();
        GameObject p = CreateStaff(0, race, gender, headIndex, bodyIndex, legsIndex);
        return p;
    }

    public GameObject CreateNurse(string machineTag)
    {
        int race = 0;
        int gender = 0;
        int headIndex = 0;
        int bodyIndex = 0;
        int legsIndex = 0;

        switch (machineTag)
        {
            case "Xray":
                race = 0;
                gender = 1;
                headIndex = 1;
                bodyIndex = 0;
                legsIndex = 0;
                break;
            case "UltraSound":
                race = 1;
                gender = 1;
                headIndex = 1;
                bodyIndex = 0;
                legsIndex = 0;
                break;
            case "Mri":
                race = 0;
                gender = 1;
                headIndex = 0;
                bodyIndex = 0;
                legsIndex = 0;
                break;
            case "Laser":
                race = 0;
                gender = 1;
                headIndex = 2;
                bodyIndex = 0;
                legsIndex = 0;
                break;
            case "BloodPressure":
                race = 1;
                gender = 1;
                headIndex = 0;
                bodyIndex = 0;
                legsIndex = 0;
                break;
            default:
                race = 0;
                gender = 0;
                headIndex = 0;
                bodyIndex = 0;
                legsIndex = 0;
                break;
        }

        Character = CharactersList.instance.GetInactiveNurse();
        Character.SetActive(true);

        Character.transform.rotation = Quaternion.Euler(new Vector3(45, 45, 0));
        info = Character.GetComponent<BaseCharacterInfo>();
        GameObject p = CreateStaff(3, race, gender, headIndex, bodyIndex, legsIndex);
        return p;
    }

    private GameObject RandomCharacterz(int race, int gender, int type, bool vip = false, bool isKid = false)
    {
        StringBuilder builderPersonalBIO = new StringBuilder();

        if (type == 0)
        {
            Debug.LogError("CREATING DOCTOR WITH WRONG METHOD");
        }

        HeadFront = Character.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        HeadBack = Character.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        TorsoFront = Character.transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        TorsoBack = Character.transform.GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();
        ArmsR = Character.transform.GetChild(0).GetChild(4).GetComponent<SpriteRenderer>();
        HandsRFront = Character.transform.GetChild(0).GetChild(5).GetComponent<SpriteRenderer>();
        ArmsL = Character.transform.GetChild(0).GetChild(6).GetComponent<SpriteRenderer>();
        HandsLFront = Character.transform.GetChild(0).GetChild(7).GetComponent<SpriteRenderer>();
        LowerBodyFront = Character.transform.GetChild(0).GetChild(8).GetComponent<SpriteRenderer>();
        ThighR = Character.transform.GetChild(0).GetChild(9).GetComponent<SpriteRenderer>();
        CalfR = Character.transform.GetChild(0).GetChild(10).GetComponent<SpriteRenderer>();
        FootRFront = Character.transform.GetChild(0).GetChild(11).GetComponent<SpriteRenderer>();
        ThighL = Character.transform.GetChild(0).GetChild(12).GetComponent<SpriteRenderer>();
        CalfL = Character.transform.GetChild(0).GetChild(13).GetComponent<SpriteRenderer>();
        FootLFront = Character.transform.GetChild(0).GetChild(14).GetComponent<SpriteRenderer>();
        //here is anim prop which sprite is set from anim
        AvatarHead = Character.transform.GetChild(0).GetChild(16).GetComponent<SpriteRenderer>();
        AvatarBody = Character.transform.GetChild(0).GetChild(17).GetComponent<SpriteRenderer>();
        ChooseRace = (RaceTypes)race;
        ChooseGender = (GenderTypes)gender;
        ChoosePersonType = (PersonType)type;

        info.Name = GetRandomName(gender, race);
        info.Surname = GetRandomSurname(race);
        info.BloodType = GetRandomBloodType();
        info.Likes = GetRandomLikes();
        info.Dislikes = GetRandomDislikes();
        info.Sex = gender;
        info.Race = race;
        info.Age = GetRandomAge(isKid);
        info.Type = type;


        if (vip)
        {
            info.IsVIP = true;
            info.VIPTime = GameState.RandomNumber(VipTimeOut.Min, VipTimeOut.Max);
        }
        else
        {
            info.IsVIP = false;
            info.VIPTime = 0;
        }

        if ((int)ChoosePersonType == 0)
        {
            partsList = Pooler[0].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 1)
            partsList = Pooler[1].GetComponent<MainList>();
        else if ((int)ChoosePersonType == 2)
        {
            partsList = Pooler[2].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 3)
        {
            partsList = Pooler[3].GetComponent<MainList>();
            ChooseGender = (GenderTypes)1;
        }
        else if ((int)ChoosePersonType == 4)
        {
            partsList = Pooler[5].GetComponent<MainList>();
        }

        builderPersonalBIO.Append(race.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(gender.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(type.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(vip.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.VIPTime.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Age.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Name);
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Surname);
        builderPersonalBIO.Append("*");


        MainList.Gender temp = partsList.Races[(int)ChooseRace].Genders[(int)ChooseGender];

        if ((int)(int)ChooseRace == 0 && (int)ChooseGender == 0)
        {  // beware bad Hipster
            indexRand = BaseGameState.RandomNumber(0, temp.HeadList.Count - 1);
        }
        else
        {
            indexRand = BaseGameState.RandomNumber(0, temp.HeadList.Count);
        }

        builderPersonalBIO.Append(indexRand.ToString()); // head
        builderPersonalBIO.Append("*");

        MainList.Head head = temp.HeadList[indexRand];
        HeadFront.sprite = head.HeadContainer.front;
        HeadBack.sprite = head.HeadContainer.back;

        if ((int)ChooseRace == 0 && (int)ChooseGender == 0)
        { //beware bad Hipster
            indexRand = BaseGameState.RandomNumber(0, temp.BodyList.Count - 1);
        }
        else
        {
            indexRand = BaseGameState.RandomNumber(0, temp.BodyList.Count);
        }

        indexRand = BaseGameState.RandomNumber(0, temp.BodyList.Count);
        builderPersonalBIO.Append(indexRand.ToString()); // body
        builderPersonalBIO.Append("*");

        MainList.Body body = temp.BodyList[indexRand];
        TorsoFront.sprite = body.BodyContainer.front;
        TorsoBack.sprite = body.BodyContainer.back;
        ArmsL.sprite = body.Arms.Arm;
        HandsLFront.sprite = body.Arms.HandLeft;
        ArmsR.sprite = body.Arms.Arm;
        HandsRFront.sprite = body.Arms.HandRight;
        AvatarBody.sprite = body.Avatar;
        AvatarHead.sprite = head.Avatar;
        info.AvatarBody = body.Avatar;
        info.AvatarHead = head.Avatar;

        //Debug.Log("Body Index: " + indexRand + " race: " + (int)ChooseRace + " gen: " + (int)ChooseGender);
        if (type == 1) //adult patient
            indexRand = GetLegsIndex(indexRand, (int)ChooseRace, (int)ChooseGender, temp);
        else
            indexRand = BaseGameState.RandomNumber(0, temp.LegsList.Count);

        //Debug.Log("Legs list count = " + temp.LegsList.Count);
        builderPersonalBIO.Append(indexRand.ToString()); // legs
        builderPersonalBIO.Append("*");
        //builderPersonalBIO.Append("*");
        MainList.Legs legs = temp.LegsList[indexRand];

        LowerBodyFront.sprite = legs.LowerBodyFront;
        ThighL.sprite = legs.Thigh;
        CalfL.sprite = legs.Calf;
        FootLFront.sprite = legs.FootFront;
        ThighR.sprite = legs.Thigh;
        CalfR.sprite = legs.Calf;
        FootRFront.sprite = legs.FootFront;

        builderPersonalBIO.Append(info.Likes.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Dislikes.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(((int)info.BloodType).ToString());

        info.personalBIO = builderPersonalBIO.ToString();


        return Character;
    }

    private GameObject RandomCharacterz(int race, int gender, int type, int headVal, int bodyVal, int legsVal, bool vip = false)
    {
        StringBuilder builderPersonalBIO = new StringBuilder();

        if (type == 0)
        {
            Debug.LogError("CREATING DOCTOR WITH WRONG METHOD");
        }

        HeadFront = Character.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        HeadBack = Character.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        TorsoFront = Character.transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        TorsoBack = Character.transform.GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();
        ArmsR = Character.transform.GetChild(0).GetChild(4).GetComponent<SpriteRenderer>();
        HandsRFront = Character.transform.GetChild(0).GetChild(5).GetComponent<SpriteRenderer>();
        ArmsL = Character.transform.GetChild(0).GetChild(6).GetComponent<SpriteRenderer>();
        HandsLFront = Character.transform.GetChild(0).GetChild(7).GetComponent<SpriteRenderer>();
        LowerBodyFront = Character.transform.GetChild(0).GetChild(8).GetComponent<SpriteRenderer>();
        ThighR = Character.transform.GetChild(0).GetChild(9).GetComponent<SpriteRenderer>();
        CalfR = Character.transform.GetChild(0).GetChild(10).GetComponent<SpriteRenderer>();
        FootRFront = Character.transform.GetChild(0).GetChild(11).GetComponent<SpriteRenderer>();
        ThighL = Character.transform.GetChild(0).GetChild(12).GetComponent<SpriteRenderer>();
        CalfL = Character.transform.GetChild(0).GetChild(13).GetComponent<SpriteRenderer>();
        FootLFront = Character.transform.GetChild(0).GetChild(14).GetComponent<SpriteRenderer>();
        //here is anim prop which sprite is set from anim
        AvatarHead = Character.transform.GetChild(0).GetChild(16).GetComponent<SpriteRenderer>();
        AvatarBody = Character.transform.GetChild(0).GetChild(17).GetComponent<SpriteRenderer>();
        ChooseRace = (RaceTypes)race;
        ChooseGender = (GenderTypes)gender;
        ChoosePersonType = (PersonType)type;

        info.Name = GetRandomName(gender, race);
        info.Surname = GetRandomSurname(race);
        info.Sex = gender;
        info.Race = race;
        info.Age = GetRandomAge(false);
        info.Type = type;


        if (vip)
        {
            info.IsVIP = true;
            info.VIPTime = GameState.RandomNumber(VipTimeOut.Min, VipTimeOut.Max);
        }
        else
        {
            info.IsVIP = false;
            info.VIPTime = 0;
        }

        if ((int)ChoosePersonType == 0)
        {
            partsList = Pooler[0].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 1)
            partsList = Pooler[1].GetComponent<MainList>();
        else if ((int)ChoosePersonType == 2)
        {
            partsList = Pooler[2].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 3)
        {
            partsList = Pooler[3].GetComponent<MainList>();
            ChooseGender = (GenderTypes)1;
        }

        builderPersonalBIO.Append(race.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(gender.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(type.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(vip.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.VIPTime.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Age.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Name);
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Surname);
        builderPersonalBIO.Append("*");


        MainList.Gender temp = partsList.Races[(int)ChooseRace].Genders[(int)ChooseGender];

        indexRand = headVal;
        builderPersonalBIO.Append(indexRand.ToString()); // head
        builderPersonalBIO.Append("*");

        MainList.Head head = temp.HeadList[indexRand];
        HeadFront.sprite = head.HeadContainer.front;
        HeadBack.sprite = head.HeadContainer.back;

        indexRand = bodyVal;
        builderPersonalBIO.Append(indexRand.ToString()); // body
        builderPersonalBIO.Append("*");

        MainList.Body body = temp.BodyList[indexRand];
        TorsoFront.sprite = body.BodyContainer.front;
        TorsoBack.sprite = body.BodyContainer.back;
        ArmsL.sprite = body.Arms.Arm;
        HandsLFront.sprite = body.Arms.HandLeft;
        ArmsR.sprite = body.Arms.Arm;
        HandsRFront.sprite = body.Arms.HandRight;
        AvatarBody.sprite = body.Avatar;
        AvatarHead.sprite = head.Avatar;
        info.AvatarBody = body.Avatar;
        info.AvatarHead = head.Avatar;

        Debug.Log("Body Index: " + indexRand + " race: " + (int)ChooseRace + " gen: " + (int)ChooseGender);
        if (type == 1) //adult patient
            indexRand = GetLegsIndex(indexRand, (int)ChooseRace, (int)ChooseGender, temp);
        else
            GameState.RandomNumber(0, temp.LegsList.Count);

        Debug.Log("Legs list count = " + temp.LegsList.Count);
        builderPersonalBIO.Append(indexRand.ToString()); // legs
        builderPersonalBIO.Append("*");

        indexRand = legsVal;
        MainList.Legs legs = temp.LegsList[indexRand];

        LowerBodyFront.sprite = legs.LowerBodyFront;
        ThighL.sprite = legs.Thigh;
        CalfL.sprite = legs.Calf;
        FootLFront.sprite = legs.FootFront;
        ThighR.sprite = legs.Thigh;
        CalfR.sprite = legs.Calf;
        FootRFront.sprite = legs.FootFront;

        builderPersonalBIO.Append(info.Likes.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(info.Dislikes.ToString());
        builderPersonalBIO.Append("*");
        builderPersonalBIO.Append(((int)info.BloodType).ToString());

        info.personalBIO = builderPersonalBIO.ToString();
        return Character;
    }

    int GetLegsIndex(int bodyIndex, int race, int gender, MainList.Gender temp)
    {
        //legs have some exceptions. Sometimes when a character has a suit, leg sprites cannot be randomized.
        if (race == 0 && gender == 1 && bodyIndex == 5)
            return 0;
        if (race == 0 && gender == 0 && bodyIndex == 1)
            return 2;
        if (race == 1 && gender == 0 && bodyIndex == 2)
            return 2;
        if (race == 1 && gender == 1 && bodyIndex == 3)
            return 3;
        if (race == 2 && gender == 0 && bodyIndex == 0)
            return 3;
        if (race == 2 && gender == 1 && bodyIndex == 2)
            return 2;

        return GameState.RandomNumber(0, temp.LegsList.Count);
    }
    /// <summary>
    /// Define a character to be spawned. Definitions are split by char '*'
    /// see the string <paramref name="BIOinfo"/> to define the character.
    /// </summary>
    /// <param name="BIOinfo"> 0: Race, 1: Sex, 2: Type, 3: IsVIP, 4: VIPTime, 5: Age, 6: Name, 7: Surname, 8: Head, 9: Body, 10: Legs, 11: Likes, 12: Dislikes, 13: BloodType</param>
    private GameObject DefinedCharacter(string BIOinfo)
    {
        info.personalBIO = BIOinfo;

        var personBIO = BIOinfo.Split('*');


        if (int.Parse(personBIO[2], System.Globalization.CultureInfo.InvariantCulture) == 0)
        {
            Debug.LogError("CREATING DOCTOR WITH WRONG METHOD");
        }

        HeadFront = Character.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        HeadBack = Character.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        TorsoFront = Character.transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        TorsoBack = Character.transform.GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();
        ArmsR = Character.transform.GetChild(0).GetChild(4).GetComponent<SpriteRenderer>();
        HandsRFront = Character.transform.GetChild(0).GetChild(5).GetComponent<SpriteRenderer>();
        ArmsL = Character.transform.GetChild(0).GetChild(6).GetComponent<SpriteRenderer>();
        HandsLFront = Character.transform.GetChild(0).GetChild(7).GetComponent<SpriteRenderer>();
        LowerBodyFront = Character.transform.GetChild(0).GetChild(8).GetComponent<SpriteRenderer>();
        ThighR = Character.transform.GetChild(0).GetChild(9).GetComponent<SpriteRenderer>();
        CalfR = Character.transform.GetChild(0).GetChild(10).GetComponent<SpriteRenderer>();
        FootRFront = Character.transform.GetChild(0).GetChild(11).GetComponent<SpriteRenderer>();
        ThighL = Character.transform.GetChild(0).GetChild(12).GetComponent<SpriteRenderer>();
        CalfL = Character.transform.GetChild(0).GetChild(13).GetComponent<SpriteRenderer>();
        FootLFront = Character.transform.GetChild(0).GetChild(14).GetComponent<SpriteRenderer>();
        //here is anim prop which sprite is set from anim
        AvatarHead = Character.transform.GetChild(0).GetChild(16).GetComponent<SpriteRenderer>();
        AvatarBody = Character.transform.GetChild(0).GetChild(17).GetComponent<SpriteRenderer>();
        ChooseRace = (RaceTypes)int.Parse(personBIO[0], System.Globalization.CultureInfo.InvariantCulture);
        ChooseGender = (GenderTypes)int.Parse(personBIO[1], System.Globalization.CultureInfo.InvariantCulture);
        ChoosePersonType = (PersonType)int.Parse(personBIO[2], System.Globalization.CultureInfo.InvariantCulture);

        info.Name = personBIO[6];
        info.Surname = personBIO[7];
        info.Sex = int.Parse(personBIO[1], System.Globalization.CultureInfo.InvariantCulture);
        info.Race = int.Parse(personBIO[0], System.Globalization.CultureInfo.InvariantCulture);
        info.Age = int.Parse(personBIO[5], System.Globalization.CultureInfo.InvariantCulture);

        if (bool.Parse(personBIO[3]))
        {
            info.IsVIP = true;
            info.VIPTime = int.Parse(personBIO[4], System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            info.IsVIP = false;
            info.VIPTime = 0;
        }

        if ((int)ChoosePersonType == 0)
        {
            partsList = Pooler[0].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 1)
            partsList = Pooler[1].GetComponent<MainList>();
        else if ((int)ChoosePersonType == 2)
        {
            partsList = Pooler[2].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 3)
        {
            partsList = Pooler[3].GetComponent<MainList>();
            ChooseGender = (GenderTypes)1;
        }
        else if ((int)ChoosePersonType == 4)
        {
            partsList = Pooler[5].GetComponent<MainList>();
        }

        MainList.Gender temp = partsList.Races[(int)ChooseRace].Genders[(int)ChooseGender];

        MainList.Head head = temp.HeadList[int.Parse(personBIO[8], System.Globalization.CultureInfo.InvariantCulture)];
        HeadFront.sprite = head.HeadContainer.front;
        HeadBack.sprite = head.HeadContainer.back;

        MainList.Body body = temp.BodyList[int.Parse(personBIO[9], System.Globalization.CultureInfo.InvariantCulture)];
        TorsoFront.sprite = body.BodyContainer.front;
        TorsoBack.sprite = body.BodyContainer.back;
        ArmsL.sprite = body.Arms.Arm;
        HandsLFront.sprite = body.Arms.HandLeft;
        ArmsR.sprite = body.Arms.Arm;
        HandsRFront.sprite = body.Arms.HandRight;
        AvatarBody.sprite = body.Avatar;
        AvatarHead.sprite = head.Avatar;
        info.AvatarBody = body.Avatar;
        info.AvatarHead = head.Avatar;

        MainList.Legs legs = temp.LegsList[int.Parse(personBIO[10], System.Globalization.CultureInfo.InvariantCulture)];
        LowerBodyFront.sprite = legs.LowerBodyFront;
        ThighL.sprite = legs.Thigh;
        CalfL.sprite = legs.Calf;
        FootLFront.sprite = legs.FootFront;
        ThighR.sprite = legs.Thigh;
        CalfR.sprite = legs.Calf;
        FootRFront.sprite = legs.FootFront;

        if (personBIO.Length > 13)
        {
            info.Likes = int.Parse(personBIO[11], System.Globalization.CultureInfo.InvariantCulture);
            info.Dislikes = int.Parse(personBIO[12], System.Globalization.CultureInfo.InvariantCulture);
            info.BloodType = (BloodType)int.Parse(personBIO[13], System.Globalization.CultureInfo.InvariantCulture);
        }

        return Character;
    }

    private GameObject GenerateDefinedMother(string BIOinfo)
    {
        info.personalBIO = BIOinfo;

        var personBIO = BIOinfo.Split('*');


        if (int.Parse(personBIO[2], System.Globalization.CultureInfo.InvariantCulture) == 0)
        {
            Debug.LogError("CREATING DOCTOR WITH WRONG METHOD");
        }

        HeadFront = Character.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        HeadBack = Character.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        TorsoFront = Character.transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        TorsoBack = Character.transform.GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();
        ArmsR = Character.transform.GetChild(0).GetChild(4).GetComponent<SpriteRenderer>();
        HandsRFront = Character.transform.GetChild(0).GetChild(5).GetComponent<SpriteRenderer>();
        ArmsL = Character.transform.GetChild(0).GetChild(6).GetComponent<SpriteRenderer>();
        HandsLFront = Character.transform.GetChild(0).GetChild(7).GetComponent<SpriteRenderer>();
        LowerBodyFront = Character.transform.GetChild(0).GetChild(8).GetComponent<SpriteRenderer>();
        ThighR = Character.transform.GetChild(0).GetChild(9).GetComponent<SpriteRenderer>();
        CalfR = Character.transform.GetChild(0).GetChild(10).GetComponent<SpriteRenderer>();
        FootRFront = Character.transform.GetChild(0).GetChild(11).GetComponent<SpriteRenderer>();
        ThighL = Character.transform.GetChild(0).GetChild(12).GetComponent<SpriteRenderer>();
        CalfL = Character.transform.GetChild(0).GetChild(13).GetComponent<SpriteRenderer>();
        FootLFront = Character.transform.GetChild(0).GetChild(14).GetComponent<SpriteRenderer>();
        //here is anim prop which sprite is set from anim
        AvatarHead = Character.transform.GetChild(0).GetChild(16).GetComponent<SpriteRenderer>();
        AvatarBody = Character.transform.GetChild(0).GetChild(17).GetComponent<SpriteRenderer>();
        pregnantBellyFront = Character.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<SpriteRenderer>();
        pregnatnBellyBack = Character.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<SpriteRenderer>();
        ChooseRace = (RaceTypes)int.Parse(personBIO[0], System.Globalization.CultureInfo.InvariantCulture);
        ChooseGender = (GenderTypes)int.Parse(personBIO[1], System.Globalization.CultureInfo.InvariantCulture);
        ChoosePersonType = (PersonType)int.Parse(personBIO[2], System.Globalization.CultureInfo.InvariantCulture);

        info.Name = personBIO[6];
        info.Surname = personBIO[7];
        info.Sex = int.Parse(personBIO[1], System.Globalization.CultureInfo.InvariantCulture);
        info.Race = int.Parse(personBIO[0], System.Globalization.CultureInfo.InvariantCulture);
        info.Age = int.Parse(personBIO[5], System.Globalization.CultureInfo.InvariantCulture);

        if (bool.Parse(personBIO[3]))
        {
            info.IsVIP = true;
            info.VIPTime = int.Parse(personBIO[4], System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            info.IsVIP = false;
            info.VIPTime = 0;
        }

        if ((int)ChoosePersonType == 0)
        {
            partsList = Pooler[0].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 1)
            partsList = Pooler[1].GetComponent<MainList>();
        else if ((int)ChoosePersonType == 2)
        {
            partsList = Pooler[2].GetComponent<MainList>();
        }
        else if ((int)ChoosePersonType == 3)
        {
            partsList = Pooler[3].GetComponent<MainList>();
            ChooseGender = (GenderTypes)1;
        }
        else if ((int)ChoosePersonType == 4)
        {
            partsList = Pooler[5].GetComponent<MainList>();
        }

        MainList.Gender temp = partsList.Races[(int)ChooseRace].Genders[(int)ChooseGender];

        MainList.Head head = temp.HeadList[int.Parse(personBIO[8], System.Globalization.CultureInfo.InvariantCulture)];
        HeadFront.sprite = head.HeadContainer.front;
        HeadBack.sprite = head.HeadContainer.back;

        MainList.Body body = temp.BodyList[int.Parse(personBIO[9], System.Globalization.CultureInfo.InvariantCulture)];
        TorsoFront.sprite = body.BodyContainer.front;
        TorsoBack.sprite = body.BodyContainer.back;
        ArmsL.sprite = body.Arms.Arm;
        HandsLFront.sprite = body.Arms.HandLeft;
        ArmsR.sprite = body.Arms.Arm;
        HandsRFront.sprite = body.Arms.HandRight;
        AvatarBody.sprite = body.Avatar;
        AvatarHead.sprite = head.Avatar;
        info.AvatarBody = body.Avatar;
        info.AvatarHead = head.Avatar;
        pregnantBellyFront.sprite = body.PregnantBodyContainer.front;
        pregnatnBellyBack.sprite = body.PregnantBodyContainer.back;

        MainList.Legs legs = temp.LegsList[int.Parse(personBIO[10])];
        LowerBodyFront.sprite = legs.LowerBodyFront;
        ThighL.sprite = legs.Thigh;
        CalfL.sprite = legs.Calf;
        FootLFront.sprite = legs.FootFront;
        ThighR.sprite = legs.Thigh;
        CalfR.sprite = legs.Calf;
        FootRFront.sprite = legs.FootFront;

        if (personBIO.Length > 13)
        {
            info.Likes = int.Parse(personBIO[11], System.Globalization.CultureInfo.InvariantCulture);
            info.Dislikes = int.Parse(personBIO[12], System.Globalization.CultureInfo.InvariantCulture);
            info.BloodType = (BloodType)int.Parse(personBIO[13], System.Globalization.CultureInfo.InvariantCulture);
        }

        return Character;
    }

    private GameObject CreateStaff(int type, int race, int gender, int headIndex, int bodyIndex, int legsIndex)
    {
        HeadFront = Character.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        HeadBack = Character.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        TorsoFront = Character.transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        TorsoBack = Character.transform.GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();
        ArmsR = Character.transform.GetChild(0).GetChild(4).GetComponent<SpriteRenderer>();
        HandsRFront = Character.transform.GetChild(0).GetChild(5).GetComponent<SpriteRenderer>();
        ArmsL = Character.transform.GetChild(0).GetChild(6).GetComponent<SpriteRenderer>();
        HandsLFront = Character.transform.GetChild(0).GetChild(7).GetComponent<SpriteRenderer>();
        LowerBodyFront = Character.transform.GetChild(0).GetChild(8).GetComponent<SpriteRenderer>();
        ThighR = Character.transform.GetChild(0).GetChild(9).GetComponent<SpriteRenderer>();
        CalfR = Character.transform.GetChild(0).GetChild(10).GetComponent<SpriteRenderer>();
        FootRFront = Character.transform.GetChild(0).GetChild(11).GetComponent<SpriteRenderer>();
        ThighL = Character.transform.GetChild(0).GetChild(12).GetComponent<SpriteRenderer>();
        CalfL = Character.transform.GetChild(0).GetChild(13).GetComponent<SpriteRenderer>();
        FootLFront = Character.transform.GetChild(0).GetChild(14).GetComponent<SpriteRenderer>();
        //here is anim prop which sprite is set from anim
        if (type == 0)
        {
            ApronFL = Character.transform.GetChild(0).GetChild(16).GetComponent<SpriteRenderer>();
            ApronB = Character.transform.GetChild(0).GetChild(17).GetComponent<SpriteRenderer>();
            ApronF = Character.transform.GetChild(0).GetChild(18).GetComponent<SpriteRenderer>();
            ApronFR = Character.transform.GetChild(0).GetChild(19).GetComponent<SpriteRenderer>();
        }

        ChooseRace = (RaceTypes)race;
        ChooseGender = (GenderTypes)gender;

        info.Name = GetRandomName(gender, race);
        info.Surname = GetRandomSurname(race);
        info.Sex = gender;
        info.Race = race;
        info.Age = GetRandomAge(false);
        info.IsVIP = false;
        info.VIPTime = 0;

        partsList = Pooler[type].GetComponent<MainList>();
        MainList.Gender temp = partsList.Races[(int)ChooseRace].Genders[(int)ChooseGender];

        MainList.Head head = temp.HeadList[headIndex];
        HeadFront.sprite = head.HeadContainer.front;
        HeadBack.sprite = head.HeadContainer.back;

        MainList.Body body = temp.BodyList[bodyIndex];
        TorsoFront.sprite = body.BodyContainer.front;
        TorsoBack.sprite = body.BodyContainer.back;
        ArmsL.sprite = body.Arms.Arm;
        HandsLFront.sprite = body.Arms.HandLeft;
        ArmsR.sprite = body.Arms.Arm;
        HandsRFront.sprite = body.Arms.HandRight;

        MainList.Legs legs = temp.LegsList[legsIndex];
        LowerBodyFront.sprite = legs.LowerBodyFront;
        ThighL.sprite = legs.Thigh;
        CalfL.sprite = legs.Calf;
        FootLFront.sprite = legs.FootFront;
        ThighR.sprite = legs.Thigh;
        CalfR.sprite = legs.Calf;
        FootRFront.sprite = legs.FootFront;

        if (type == 0)
        {
            MainList.Apron apron = temp.ApronList[0];
            ApronF.sprite = apron.ApronFront;
            ApronFR.sprite = apron.ApronFrontRight;
            ApronFL.sprite = apron.ApronFrontLeft;
            ApronB.sprite = apron.ApronBack;
        }
        return Character;
    }

    public List<Sprite> GetPijama(int race, int gender, bool isVIP)
    {
        int vip = 0;
        if (isVIP)
            vip = 1;

        partsList = Pooler[4].GetComponent<MainList>();
        MainList.Gender temp = partsList.Races[race].Genders[gender];
        MainList.Body body = temp.BodyList[vip];
        MainList.Legs legs = temp.LegsList[vip];

        List<Sprite> pijama = new List<Sprite>();
        pijama.Add(body.BodyContainer.front);
        pijama.Add(body.BodyContainer.back);
        pijama.Add(body.Arms.Arm);
        pijama.Add(body.Arms.HandRight);
        pijama.Add(body.Arms.Arm);
        pijama.Add(body.Arms.HandLeft);
        pijama.Add(legs.LowerBodyFront);
        pijama.Add(legs.Thigh);         //this is empty in PajamasPooler
        pijama.Add(legs.Calf);          //this is empty in PajamasPooler
        pijama.Add(legs.FootFront);
        pijama.Add(legs.Thigh);         //this is empty in PajamasPooler
        pijama.Add(legs.Calf);          //this is empty in PajamasPooler
        pijama.Add(legs.FootFront);

        return pijama;
    }



    public void SetHospitalCloth(List<SpriteRenderer> selectedCharacterSprites)
    {
        selectedCharacterSprites[0] = HeadFront;
        selectedCharacterSprites[1] = HeadBack;
        selectedCharacterSprites[2] = TorsoFront;
        selectedCharacterSprites[3] = TorsoBack;
        selectedCharacterSprites[4] = ArmsR;
        selectedCharacterSprites[5] = HandsRFront;
        selectedCharacterSprites[6] = ArmsL;
        selectedCharacterSprites[7] = HandsLFront;
        selectedCharacterSprites[8] = LowerBodyFront;
        selectedCharacterSprites[9] = ThighR;
        selectedCharacterSprites[10] = CalfR;
        selectedCharacterSprites[11] = FootRFront;
        selectedCharacterSprites[12] = ThighL;
        selectedCharacterSprites[13] = CalfL;
        selectedCharacterSprites[14] = FootLFront;
    }

    public void SetHospitalCloth(List<Sprite> selectedCharacterSprites)
    {
        selectedCharacterSprites[0] = TorsoFront.sprite;
        selectedCharacterSprites[1] = TorsoBack.sprite;
        selectedCharacterSprites[2] = ArmsR.sprite;
        selectedCharacterSprites[3] = HandsRFront.sprite;
        selectedCharacterSprites[4] = ArmsL.sprite;
        selectedCharacterSprites[5] = HandsLFront.sprite;
        selectedCharacterSprites[6] = LowerBodyFront.sprite;

        selectedCharacterSprites[7] = ThighR.sprite;
        selectedCharacterSprites[8] = CalfR.sprite;
        selectedCharacterSprites[9] = FootRFront.sprite;
        selectedCharacterSprites[10] = ThighL.sprite;
        selectedCharacterSprites[11] = CalfL.sprite;
        selectedCharacterSprites[12] = FootLFront.sprite;
        //
        //
    }

    string GetRandomName(int gender, int race)
    {
        string nameString = "";
        int namesCount = 0;
        if (gender == 0)    //male
        {
            if (race == 0)  //white
            {
                //PATIENT_NAME/
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/MALE_WHITE_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "NAME_MALE_0_" + UnityEngine.Random.Range(0, namesCount);
            }
            else if (race == 1) //black
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/MALE_BLACK_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "NAME_MALE_1_" + UnityEngine.Random.Range(0, namesCount);
            }
            else   //asian
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/MALE_ASIAN_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "NAME_MALE_2_" + UnityEngine.Random.Range(0, namesCount);
            }
        }
        else    //female
        {
            if (race == 0)  //white
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/FEMALE_WHITE_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "NAME_FEMALE_0_" + UnityEngine.Random.Range(0, namesCount);
            }
            else if (race == 1) //black
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/FEMALE_BLACK_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "NAME_FEMALE_1_" + UnityEngine.Random.Range(0, namesCount);
            }
            else   //asian
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/FEMALE_ASIAN_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "NAME_FEMALE_2_" + UnityEngine.Random.Range(0, namesCount);
            }

        }

        return nameString;
    }

    string GetRandomSurname(int race)
    {
        string surnameString = "";
        int surnamesCount = 0;

        if (race == 0)   //white
        {
            //PATIENT_SURNAME/
            surnamesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/SURNAME_WHITE_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
            surnameString = "SURNAME_0_" + UnityEngine.Random.Range(0, surnamesCount);
        }
        else if (race == 1)  //black
        {
            surnamesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/SURNAME_BLACK_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
            surnameString = "SURNAME_1_" + UnityEngine.Random.Range(0, surnamesCount);
        }
        else  //asian
        {
            surnamesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/SURNAME_ASIAN_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
            surnameString = "SURNAME_2_" + UnityEngine.Random.Range(0, surnamesCount);
        }

        return surnameString;
    }

    string GetRandomBabyName(int gender, int race)
    {
        string nameString = "";
        int namesCount = 0;
        if (gender == 0)    //male
        {
            if (race == 0)  //white
            {
                //PATIENT_NAME/
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/BABY_MALE_WHITE_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "BABY_NAME_MALE_0_" + UnityEngine.Random.Range(0, namesCount);
            }
            else if (race == 1) //black
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/BABY_MALE_BLACK_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "BABY_NAME_MALE_1_" + UnityEngine.Random.Range(0, namesCount);
            }
            else   //asian
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/BABY_MALE_ASIAN_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "BABY_NAME_MALE_2_" + UnityEngine.Random.Range(0, namesCount);
            }
        }
        else    //female
        {
            if (race == 0)  //white
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/BABY_FEMALE_WHITE_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "BABY_NAME_FEMALE_0_" + UnityEngine.Random.Range(0, namesCount);
            }
            else if (race == 1) //black
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/BABY_FEMALE_BLACK_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "BABY_NAME_FEMALE_1_" + UnityEngine.Random.Range(0, namesCount);
            }
            else   //asian
            {
                namesCount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_NAME/BABY_FEMALE_ASIAN_COUNT"), System.Globalization.CultureInfo.InvariantCulture);
                nameString = "BABY_NAME_FEMALE_2_" + UnityEngine.Random.Range(0, namesCount);
            }

        }

        return nameString;
    }

    BloodType GetRandomBloodType()
    {
        BloodType type;
        int lenghtOfBlootTypeArray = System.Enum.GetNames(typeof(BloodType)).Length;
        int index = GameState.RandomNumber(0, lenghtOfBlootTypeArray);
        type = (BloodType)index;
        return type;
    }

    int GetRandomLikes()
    {
        int likesAmount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_LIKES/LIKES_AMOUNT"), System.Globalization.CultureInfo.InvariantCulture);
        int randomLike = UnityEngine.Random.Range(0, likesAmount + 1);
        //string like = I2.Loc.ScriptLocalization.Get("PATIENT_LIKES/LIKES_"+randomLike);
        return randomLike;
    }

    int GetMotherRandomLikes()
    {
        int likesAmount = int.Parse(I2.Loc.ScriptLocalization.Get("MOTHERS_LIKES/LIKES_AMOUNT"), System.Globalization.CultureInfo.InvariantCulture);
        int randomLike = UnityEngine.Random.Range(0, likesAmount);
        //string like = I2.Loc.ScriptLocalization.Get("PATIENT_LIKES/LIKES_"+randomLike);
        return randomLike;
    }

    int GetBabyRandomLikes()
    {
        int likesAmount = int.Parse(I2.Loc.ScriptLocalization.Get("BABY_LIKES/LIKES_AMOUNT"), System.Globalization.CultureInfo.InvariantCulture);
        int randomLike = UnityEngine.Random.Range(0, likesAmount);
        //string like = I2.Loc.ScriptLocalization.Get("PATIENT_LIKES/LIKES_"+randomLike);
        return randomLike;
    }

    int GetRandomDislikes()
    {
        int dislikesAmount = int.Parse(I2.Loc.ScriptLocalization.Get("PATIENT_DISLIKES/DISLIKES_AMOUNT"), System.Globalization.CultureInfo.InvariantCulture);
        int randomDisike = UnityEngine.Random.Range(0, dislikesAmount + 1);
        //string dislike = I2.Loc.ScriptLocalization.Get("PATIENT_DISLIKES/DISLIKES_" + randomDisike);
        return randomDisike;
    }

    public int GetRandomAge(bool isKid = false)
    {
        if (isKid)
            return UnityEngine.Random.Range(6, 14);
        else return UnityEngine.Random.Range(19, 51);
    }

    public static string GetBloodTypeString(BloodType bloodType)
    {
        switch (bloodType)
        {
            case BloodType.ABm:
                return "AB-";
            case BloodType.ABp:
                return "AB+";
            case BloodType.Am:
                return "A-";
            case BloodType.Ap:
                return "A+";
            case BloodType.Bm:
                return "B-";
            case BloodType.Bp:
                return "B+";
            case BloodType.Om:
                return "O-";
            case BloodType.Op:
                return "O+";
            default:
                return "O+";
        }
    }

    public static string GetSexString(int sex)
    {
        if (sex == 1)
            return I2.Loc.ScriptLocalization.Get("SEX_FEMALE");
        else
            return I2.Loc.ScriptLocalization.Get("SEX_MALE");
    }
}