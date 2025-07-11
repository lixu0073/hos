using UnityEngine;
using System.Collections.Generic;
using IsoEngine;
using System;
using TutorialSystem;

namespace Hospital
{
    public class ClinicAISpawner : MonoBehaviour
    {
        public CharacterCreator creator;
        public List<Vector2i> startingPoints;
        private LinkedList<GameObject> toActivate;
        private int patientCount = 0;
        private float lastTime;
        public void Reset()
        {
            RemoveAllSpawnedPatient();
            toActivate.Clear();
        }

        public void RemoveAllSpawnedPatient()
        {
            if (gameObject != null && gameObject.transform.childCount > 0)
            {
                foreach (Transform t in gameObject.transform)
                {
                    if (t.GetComponent<ClinicPatientAI>() != null)
                    {
                        // Debug.LogError("X" + t.gameObject.name);
                        t.GetComponent<ClinicPatientAI>().IsoDestroy();
                    }
                }
            }
        }

        [TutorialTriggerable]
        public void SpawnFirstEverClinicPatient()
        {
            if (ClinicPatientAI.Patients.Count > 0)
            {
                return;
            }
            var p = SpawnPatient(null, "A" + new Vector2i(21, 56).ToString(), ResourcesHolder.GetHospital().GetClinicDisease(0), true) as ClinicPatientAI;
            try { 
                p.anim.Play(AnimHash.Headache_2, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            ClinicPatientAI.Patients.Add(p);
            p.SetAIState(new ClinicPatientAI.GoToReceptionState(p, false, 0, -1, true));
            TenjinController.instance.ReportLevelUp(1);
        }

        public BasePatientAI SpawnPatient(DoctorRoom destinationRoom, string info, ClinicDiseaseDatabaseEntry clinicDisease = null, bool isFirstEver = false)
        {
            GameObject go;
            BasePatientAI p;
            if (info[0] == 'A')
            {
                if (isFirstEver)
                    go = creator.FirstEverPatient();
                else
                    go = creator.RandomPatient(true);

                p = go.AddComponent<ClinicPatientAI>();
                go.GetComponent<ClinicPatientAI>().isFirstEver = isFirstEver;
                go.name = "Patient_" + patientCount;
            }
            else
            {
                go = creator.RandomKid();
                p = go.AddComponent<ChildPatientAI>();
                go.name = "Child_Patient_" + patientCount;
            }
            p.transform.rotation = Quaternion.Euler(45, 45, 0);

            if (clinicDisease == null && destinationRoom != null)
            {
                clinicDisease = ((DoctorRoomInfo)(destinationRoom.GetRoomInfo())).CuredDisease;
            }
            ClinicCharacterInfo charInfo = go.GetComponent<ClinicCharacterInfo>();
            charInfo.Initialize(clinicDisease);

            p.Initialize(destinationRoom, info);

            ++patientCount;
            go.SetActive(true);

            return p;
        }

        public BasePatientAI LoadPatient(DoctorRoom destinationRoom, string info, string extendedInfo, bool isFirstEver = false, ClinicDiseaseDatabaseEntry clinicDisease = null)
        {

            GameObject go;
            BasePatientAI p;
            go = creator.DefinedClinicCharacter(extendedInfo);

            if (info[0] == 'A')
            {
                p = go.AddComponent<ClinicPatientAI>();
                go.GetComponent<ClinicPatientAI>().isFirstEver = isFirstEver;
                go.name = "Patient_" + patientCount;
            }
            else
            {
                p = go.AddComponent<ChildPatientAI>();
                go.name = "Loaded_Child_Patient_" + patientCount;
                //HospitalAreasMapController.Map.playgroud.AddKid ((ChildPatientAI)p);
            }


            p.transform.rotation = Quaternion.Euler(45, 45, 0);


            if (clinicDisease == null && destinationRoom != null)
            {
                clinicDisease = ((DoctorRoomInfo)(destinationRoom.GetRoomInfo())).CuredDisease;
            }
            ClinicCharacterInfo charInfo = go.GetComponent<ClinicCharacterInfo>();
            charInfo.Initialize(clinicDisease);

            if (info[0] != 'A') // for old kids age
            {
                if (charInfo.Age > 14)
                    charInfo.Age = creator.GetRandomAge(true);
            }

            p.Initialize(destinationRoom, info);

            ++patientCount;
            go.SetActive(true);

            return p;
        }


        public ClinicPatientAI SpawnPatient(DoctorRoom destinationRoom, int startingPointID, bool male)
        {
            if (startingPointID < 0)
            {
                //System.Random tmpRand = new System.Random(DateTime.Now.Second);
                startingPointID = GameState.RandomNumber(0, startingPoints.Count);
            }


            GameObject go = creator.RandomPatient(male);
            //Debug.Log("Adding disease");
            ClinicCharacterInfo charInfo = go.GetComponent<ClinicCharacterInfo>();
            charInfo.Initialize(((DoctorRoomInfo)(destinationRoom.GetRoomInfo())).CuredDisease);

            var p = go.AddComponent<ClinicPatientAI>();
            p.transform.rotation = Quaternion.Euler(45, 45, 0);
            //p.transform.position = new Vector3(startingPoints[startingPointID].x, 0, startingPoints[startingPointID].y);

            p.Initialize(startingPoints[startingPointID], destinationRoom);
            p.startingPoint = startingPoints[startingPointID];
            go.name = "Patient_" + patientCount;
            patientCount++;
            toActivate.AddLast(go);

            ClinicPatientAI.Patients.Add(p);
            return p;
        }

        public ChildPatientAI SpawnKid(bool male)
        {
            // UnityEngine.Random.seed = DateTime.Now.Second;
            GameObject go = creator.RandomClinicCharacter(GameState.RandomNumber(0, 3), male ? 0 : 1, 2, true);
            var p = go.AddComponent<ChildPatientAI>();
            p.transform.rotation = Quaternion.Euler(45, 45, 0);

            p.Initialize(new Vector2i(21, 19), -1);

            go.name = "Patien_" + patientCount + "_child_";
            ++patientCount;
            toActivate.AddLast(go);
            ClinicCharacterInfo charInfo = go.GetComponent<ClinicCharacterInfo>();
            charInfo.Initialize(null);
            HospitalAreasMapController.HospitalMap.playgroud.AddKid(p);

            return p;
        }

        public ChildPatientAI SpawnKid(DoctorRoom destinationRoom, bool male)
        {
            //UnityEngine.Random.seed = DateTime.Now.Second;
            GameObject go = creator.RandomClinicCharacter(GameState.RandomNumber(0, 3), male ? 0 : 1, 2, true);
            var p = go.AddComponent<ChildPatientAI>();
            p.transform.rotation = Quaternion.Euler(45, 45, 0);

            go.name = "Patien_" + patientCount + "_child_";
            ++patientCount;
            toActivate.AddLast(go);

            ClinicCharacterInfo charInfo = go.GetComponent<ClinicCharacterInfo>();
            charInfo.Initialize(((DoctorRoomInfo)(destinationRoom.GetRoomInfo())).CuredDisease);

            p.Initialize(new Vector2i(26, 51), destinationRoom);
            HospitalAreasMapController.HospitalMap.playgroud.AddKid(p);

            p.gameObject.SetActive(true);
            return p;
        }

        public ChildPatientAI SpawnKidAtPlayroom(bool male)
        {
            // UnityEngine.Random.seed = DateTime.Now.Second;
            GameObject go = creator.RandomClinicCharacter(GameState.RandomNumber(0, 3), male ? 0 : 1, 2, true);
            var p = go.AddComponent<ChildPatientAI>();
            p.transform.rotation = Quaternion.Euler(45, 45, 0);
            var spot = HospitalAreasMapController.HospitalMap.playgroud.GetFreeToySpotID(p);

            p.Initialize(HospitalAreasMapController.HospitalMap.playgroud.GetToyPositionForID(spot), spot);

            go.name = "Dummy_" + patientCount + "_child_";
            ++patientCount;
            toActivate.AddLast(go);
            ClinicCharacterInfo charInfo = go.GetComponent<ClinicCharacterInfo>();
            charInfo.Initialize(null);

            HospitalAreasMapController.HospitalMap.playgroud.AddKidDummy(p);
            p.gameObject.SetActive(true);
            p.SetPlaying();
            return p;
        }

        public ChildPatientAI SpawnKidOnPosition(bool male, Vector2i posKid)
        {
            //UnityEngine.Random.seed = DateTime.Now.Second;
            GameObject go = creator.RandomClinicCharacter(GameState.RandomNumber(0, 3), male ? 0 : 1, 2, true);
            var p = go.AddComponent<ChildPatientAI>();
            p.transform.rotation = Quaternion.Euler(45, 45, 0);
            //var spot = HospitalAreasMapController.Map.playgroud.GetFreeAvaiableSpotID(p);
            p.Initialize(posKid, -1);

            go.name = "Patien_" + patientCount + "_child_";
            ++patientCount;
            toActivate.AddLast(go);
            ClinicCharacterInfo charInfo = go.GetComponent<ClinicCharacterInfo>();
            charInfo.Initialize(null);

            HospitalAreasMapController.HospitalMap.playgroud.AddKid(p);
            //p.SetPlaying();
            p.gameObject.SetActive(true);
            p.SetWaitInFronOfPlayroom();
            return p;
        }

        [TutorialTriggerable]
        public ChildPatientAI SpawnTimmyOnPosition()
        {
            if (HospitalAreasMapController.HospitalMap.playgroud.DummyKidsCount != 0)
                return null;

            //Check for tutorial skipped child spawn
            if (!TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.kids_open, true))
            {
                //action call to call this function when level up
                BaseGameState.OnLevelUp -= TimmySpawnAction;
                BaseGameState.OnLevelUp += TimmySpawnAction;
                return null;
            }
            BaseGameState.OnLevelUp -= TimmySpawnAction;
            //UnityEngine.Random.seed = DateTime.Now.Second;
            Vector2i posKid = new Vector2i(27, 48);
            GameObject go = creator.RandomClinicCharacter(0, 0, 2, 1, 0, 0);
            var p = go.AddComponent<ChildPatientAI>();
            p.transform.rotation = Quaternion.Euler(45, 45, 0);
            //var spot = HospitalAreasMapController.Map.playgroud.GetFreeAvaiableSpotID(p);
            p.Initialize(posKid, -1);

            go.name = "Patien_" + patientCount + "_child_";
            ++patientCount;
            toActivate.AddLast(go);
            ClinicCharacterInfo charInfo = go.GetComponent<ClinicCharacterInfo>();
            charInfo.Initialize(null);

            HospitalAreasMapController.HospitalMap.playgroud.AddKidDummy(p);

            p.gameObject.SetActive(true);
            p.SetWaitInFronOfPlayroom();
            return p;
        }
        private void TimmySpawnAction()
        {
            SpawnTimmyOnPosition();
        }

        public DoctorController SpawnDoctor(DoctorRoom destinationRoom)
        {
            GameObject go = creator.CreateDoctor(destinationRoom.Tag);
            var p = go.GetComponent<DoctorController>();
            p.transform.rotation = Quaternion.Euler(45, 45, 0);
            go.name = "Doctor " + destinationRoom.name;
            p.Initialize(destinationRoom);
            return p;
        }

        public NurseController SpawnNurse(DiagnosticRoom destinationRoom)
        {
            GameObject go = creator.CreateNurse(destinationRoom.Tag);
            var p = go.GetComponent<NurseController>();
            p.transform.rotation = Quaternion.Euler(45, 45, 0);
            go.name = "Nurse " + destinationRoom.name;

            p.Initialize(ResourcesHolder.GetHospital().NursePaths[(int)destinationRoom.actualData.rotation].StartingPosition + new Vector2i(destinationRoom.position.x, destinationRoom.position.y), destinationRoom);
            return p;
        }

        public void Start()
        {
            patientCount = 0;
            lastTime = Time.time;
            toActivate = new LinkedList<GameObject>();

        }
        void Update()
        {
            if (Time.time - lastTime > 1.0f)
            {
                lastTime = Time.time;
                if (toActivate.Count > 0)
                {
                    var p = toActivate.First;
                    p.Value.SetActive(true);
                    toActivate.RemoveFirst();
                }
            }
        }
    }
}