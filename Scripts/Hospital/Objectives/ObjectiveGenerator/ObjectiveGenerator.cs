using UnityEngine;
using System.Collections.Generic;
using System;

public class ObjectivesGenerator
{
    public ObjectivesGenerator()
    {

    }

    public List<Objective> GetObjectives(int id)
    {
        if (id > 2)
        {
            if (ObjectivesSynchronizer.Instance.ObjectivesCounter == 0)
            {
                ObjectivesSynchronizer.Instance.SetObjectivesCounter(Mathf.Clamp(id, 0, 10));
            }

            if (ObjectiveParser.Instance == null)
            {
                Debug.LogError("Can't start objectives'cuz you don't start game from loading screen");
                return null;
            }

            var objectivesData = ObjectiveParser.Instance.GetObjectives(id);

            List<Objective> objectives = new List<Objective>();

            if (objectivesData != null)
            {
                object obj;
                for (int i = 0; i < objectivesData.Count; ++i)
                {
                    obj = Activator.CreateInstance(Type.GetType(objectivesData[i].Type));

                    if ((obj as Objective).Init(objectivesData[i]))
                        objectives.Add(obj as Objective);
                }
            }

            return objectives;
        }

        return null;
    }

    public List<Objective> GetDynamicObjectives()
    {
        if (Game.Instance.gameState().GetHospitalLevel() > 9)
        {
            if (ObjectivesSynchronizer.Instance.ObjectivesCounter < 10)
                ObjectivesSynchronizer.Instance.SetObjectivesCounter(10);

            if (ObjectiveParser.Instance == null)
            {
                Debug.LogError("Can't start objectives'cuz you don't start game from loading screen");
                return null;
            }

            var canBeObjectivesGenerated = ObjectiveParser.Instance.IsObjectivesCanBeDynamicGenerates();

            if (canBeObjectivesGenerated)
            {
                List<Objective> objectives = new List<Objective>();

                // HARD OBJECTIVES
                Objective firstObjective = RandomFirstObjective();
                if (firstObjective != null)
                {
                    firstObjective.AddListener();
                    objectives.Add(firstObjective);
                }

                // 16
                LevelUpObjective levelUpObjective = new LevelUpObjective();
                if (levelUpObjective.InitWithRandom())
                {
                    levelUpObjective.AddListener();
                    objectives.Add(levelUpObjective);
                }

                // FILL OBJECTIVES LIST WITH OTHER AVAILABLE
                var otherObjectives = RandomOtherObjectives(5 - objectives.Count);

                for (int i = 0; i < otherObjectives.Count; ++i)
                {
                    otherObjectives[i].AddListener();
                    objectives.Add(otherObjectives[i]);
                }

                return objectives;
            }
        }

        return null;
    }

    private Objective RandomFirstObjective()
    {
        var objectivesList = new List<Objective>();

        // 1. build Rotatable Objective
        BuildRotatableObjective buildRotatableObjective = new BuildRotatableObjective();
        if (buildRotatableObjective.InitWithRandom())
            objectivesList.Add(buildRotatableObjective);

        // 2. renovate objective
        RenovateSpecialObjective renovateObjective = new RenovateSpecialObjective();
        if (renovateObjective.InitWithRandom())
            objectivesList.Add(renovateObjective);

        // 3. expand area
        ExpandAreaObjective expandObjective = new ExpandAreaObjective();
        if (expandObjective.InitWithRandom())
            objectivesList.Add(expandObjective);

        if (objectivesList.Count>0)
            return objectivesList[GameState.RandomNumber(0, objectivesList.Count)];

        return null;
    }

    private List<Objective> RandomOtherObjectives(int howManyToRandom)
    {
        List<Objective> allAvailableObjectives = new List<Objective>();

        // 4. cure Patient in Special Doctor Room
        CurePatientDoctorRoomObjective curePatientSpecialDocObjective = new CurePatientDoctorRoomObjective();
        if (curePatientSpecialDocObjective.InitWithRandom(CurePatientDoctorRoomObjective.DoctorPatientType.SingleRoom))
            allAvailableObjectives.Add(curePatientSpecialDocObjective);

        // 5. cure Patient in Any Doctor Room
        CurePatientDoctorRoomObjective curePatientAnyDocObjective = new CurePatientDoctorRoomObjective();
        if (curePatientAnyDocObjective.InitWithRandom(CurePatientDoctorRoomObjective.DoctorPatientType.AnyRoom))
            allAvailableObjectives.Add(curePatientAnyDocObjective);

        if (allAvailableObjectives.Count == 2)
            allAvailableObjectives.RemoveAt(UnityEngine.Random.Range(0, allAvailableObjectives.Count));

        // 6. cure Kids
        CurePatientDoctorRoomObjective cureKidsObjective = new CurePatientDoctorRoomObjective();
        if (cureKidsObjective.InitWithRandom(CurePatientDoctorRoomObjective.DoctorPatientType.Kid))
            allAvailableObjectives.Add(cureKidsObjective);

        // 7. cure Patient in Hospital Room With Disease
        CurePatientHospitalRoomObjective curePatientHospWithDisease = new CurePatientHospitalRoomObjective();
        if (curePatientHospWithDisease.InitWithRandom(CurePatientHospitalRoomObjective.HospitalPatientType.RandomDisease))
            allAvailableObjectives.Add(curePatientHospWithDisease);

        // 8. cure Any Patient in Hospital Room 
        CurePatientHospitalRoomObjective cureAnyPatientHosp = new CurePatientHospitalRoomObjective();
        if (cureAnyPatientHosp.InitWithRandom(CurePatientHospitalRoomObjective.HospitalPatientType.AnyPatient))
            allAvailableObjectives.Add(cureAnyPatientHosp);

        // 9. cure Patient in Hospital Room With Gender
        CurePatientHospitalRoomObjective curePatientHospWithGender = new CurePatientHospitalRoomObjective();
        if (curePatientHospWithGender.InitWithRandom(CurePatientHospitalRoomObjective.HospitalPatientType.RandomGender))
            allAvailableObjectives.Add(curePatientHospWithGender);

        // 10. cure Patient in Hospital Room With BloodType
        CurePatientHospitalRoomObjective curePatientHospWithBloodType = new CurePatientHospitalRoomObjective();
        if (curePatientHospWithBloodType.InitWithRandom(CurePatientHospitalRoomObjective.HospitalPatientType.RandomBloodType))
            allAvailableObjectives.Add(curePatientHospWithBloodType);

        // 11. cure Patient in Hospital Room With Plant
        CurePatientHospitalRoomObjective curePatientHospWithPlant = new CurePatientHospitalRoomObjective();
        if (curePatientHospWithPlant.InitWithRandom(CurePatientHospitalRoomObjective.HospitalPatientType.Plant))
            allAvailableObjectives.Add(curePatientHospWithPlant);

        // 12. cure VIP Patient 
        CurePatientHospitalRoomObjective cureVIPPatient = new CurePatientHospitalRoomObjective();
        if (cureVIPPatient.InitWithRandom(CurePatientHospitalRoomObjective.HospitalPatientType.VIP))
            allAvailableObjectives.Add(cureVIPPatient);

        // 13. cure Patient in Hospital Room With Bacteria
        CurePatientHospitalRoomObjective curePatientHospWithBacteria = new CurePatientHospitalRoomObjective();
        if (curePatientHospWithBacteria.InitWithRandom(CurePatientHospitalRoomObjective.HospitalPatientType.Bacteria))
            allAvailableObjectives.Add(curePatientHospWithBacteria);
        
        // 14. diagnose Patient in Hospital Room With Specialized DiagnosticMachine
        bool diagnosePatientObjectiveInSingleMachineIsAvailable = false;
        DiagnosePatientObjective diagnosePatientObjective = new DiagnosePatientObjective();
        if (diagnosePatientObjective.InitWithRandom(DiagnosePatientObjective.DiagnosePatientType.SingleMachine))
        {
            diagnosePatientObjectiveInSingleMachineIsAvailable = true;
        }

        // 15. diagnose Patient in Hospital Room With Any DiagnosticMachine
        bool diagnosePatientInAnyMachineObjectiveInAnyMachineIsAvailable = false;
        DiagnosePatientObjective diagnosePatientInAnyMachineObjective = new DiagnosePatientObjective();
        if (diagnosePatientInAnyMachineObjective.InitWithRandom(DiagnosePatientObjective.DiagnosePatientType.AnyMachine))
            diagnosePatientInAnyMachineObjectiveInAnyMachineIsAvailable = true;

        if (diagnosePatientObjectiveInSingleMachineIsAvailable && diagnosePatientInAnyMachineObjectiveInAnyMachineIsAvailable)
        {
            if (0 == UnityEngine.Random.Range(0, 2))
            {
                allAvailableObjectives.Add(diagnosePatientObjective);
            }
            else
            {
                allAvailableObjectives.Add(diagnosePatientInAnyMachineObjective);
            }
        }
        else if (diagnosePatientObjectiveInSingleMachineIsAvailable)
        {
            allAvailableObjectives.Add(diagnosePatientObjective);
        }
        else if (diagnosePatientInAnyMachineObjectiveInAnyMachineIsAvailable)
        {
            allAvailableObjectives.Add(diagnosePatientInAnyMachineObjective);
        }

        int toRemove = allAvailableObjectives.Count - howManyToRandom;

        if (toRemove > 0)
        {
            for (int i = 0; i < toRemove; ++i)
            {
                allAvailableObjectives.RemoveAt(BaseGameState.RandomNumber(0, allAvailableObjectives.Count));
            }
        }

        return allAvailableObjectives;
    }
}
