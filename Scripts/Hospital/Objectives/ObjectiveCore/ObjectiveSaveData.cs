using System.Collections.Generic;

public class ObjectivesSaveData {

    public List<Objective> objectives = new List<Objective>();
    public Dictionary<ObjectiveGeneratorDataType, ObjectiveGeneratorData> objectivesGeneratorData = new Dictionary<ObjectiveGeneratorDataType, ObjectiveGeneratorData>();

    public int objectivesCounter;
    public bool dynamicObjective;
    public bool rewardSeen;

    public ObjectivesSaveData()
    {
        objectivesCounter = 0;
        dynamicObjective = false;
        rewardSeen = true;
    }

    public enum ObjectiveGeneratorDataType
    {
        Default,
        BuildRotatable,
        RenovateSpecial,
        ExpandArea,
        CurePatientSingleDoctorRoom,
        CurePatientAnyDoctorRoom,
        CureKid,
        CurePatientHospitalRoomWithDisease,
        CureAnyPatientHospitalRoom,
        CurePatientHospitalRoomWithGender,
        CurePatientHospitalRoomWithBloodType,
        CurePatientHospitalRoomWithPlant,
        CureVIP,
        CurePatientHospitalRoomWithBacteria,
        DiagnosePatientWithSingleMachine,
        DiagnosePatientWithAnyMachine,
        LevelUp,
        TreatmentHelpRequest
    }
}
