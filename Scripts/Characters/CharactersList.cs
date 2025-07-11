using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class CharactersList : MonoBehaviour
    {
        public static CharactersList instance = null;
#pragma warning disable 0649
        [SerializeField] private CharacterCreator creator;
#pragma warning restore 0649
        public List<GameObject> clinicPatients;
        public List<GameObject> hospitalPatients;
        public List<GameObject> doctors;
        public List<GameObject> nurses;

        [Space(10)]
        public List<bool> clinicTaken;
        public List<bool> hospitalTaken;
        public List<bool> doctorsTaken;
        public List<bool> nursesTaken;

        [Space(10)]
        [SerializeField] private int clinicPatientsNumber = 90;
        [SerializeField] private int hospitalPatientsNumber = 35;
        [SerializeField] private int doctorsNumber = 9;
        [SerializeField] private int nursesNumber = 5;

        [HideInInspector] public string clinicPatientName = "ClinicPatient";
        [HideInInspector] public string hospitalPatientName = "HospitalPatient";
        [HideInInspector] public string doctorName = "Doctor";
        [HideInInspector] public string nurseName = "Nurse";

        private GameObject clinicPatientsContainer;
        private GameObject hospitalPatientsContainer;
        private GameObject doctorsContainer;
        private GameObject nursesContainer;

        private const int bufferCharactersNumber = 10;

        private void Awake()
        {
            //singleton
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);

            //prepare characters lists
            PrepareCharacterContainers();
            PrepareClinicPatientsList();
            PrepareHospitalPatientsList();
            PrepareDoctorsList();
            PrepareNursesList();
        }

        private void PrepareCharacterContainers()
        {
            clinicPatientsContainer = new GameObject(string.Format("{0}s", clinicPatientName));
            hospitalPatientsContainer = new GameObject(string.Format("{0}s", hospitalPatientName));
            doctorsContainer = new GameObject(string.Format("{0}s", doctorName));
            nursesContainer = new GameObject(string.Format("{0}s", nurseName));

            clinicPatientsContainer.transform.SetParent(this.transform);
            hospitalPatientsContainer.transform.SetParent(this.transform);
            doctorsContainer.transform.SetParent(this.transform);
            nursesContainer.transform.SetParent(this.transform);
        }

        private void PrepareClinicPatientsList()
        {
            PrepareCharactersList(ref clinicPatients, clinicPatientsNumber, clinicPatientName, creator.ClinicCharacterPrefab, clinicPatientsContainer, ref clinicTaken);
        }

        private void PrepareHospitalPatientsList()
        {
            PrepareCharactersList(ref hospitalPatients, hospitalPatientsNumber, hospitalPatientName, creator.HospitalCharacterPrefab, hospitalPatientsContainer, ref hospitalTaken);
        }

        private void PrepareDoctorsList()
        {
            PrepareCharactersList(ref doctors, doctorsNumber, doctorName, creator.DoctorPrefab, doctorsContainer, ref doctorsTaken);
        }

        private void PrepareNursesList()
        {
            PrepareCharactersList(ref nurses, nursesNumber, nurseName, creator.NursePrefab, nursesContainer, ref nursesTaken);
        }

        private void PrepareCharactersList(ref List<GameObject> characterList, int characterNumber, string characterName, GameObject characterPrefab, GameObject characterParent, ref List<bool> takenList)
        {
            characterList = new List<GameObject>();
            for (int i = 0; i < characterNumber; ++i)
            {
                AddCharacter(ref characterList, i, characterName, characterPrefab, characterParent, ref takenList);                
            }
        }

        private void AddCharacter(ref List<GameObject> characterList, int characterIndex, string characterName, GameObject characterPrefab, GameObject characterParent, ref List<bool> takenList)
        {
            GameObject go = Instantiate(characterPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            go.name = string.Format("{0}_{1}", characterName, characterIndex);
            go.transform.parent = characterParent.transform;
            go.SetActive(false);
            characterList.Add(go);
            takenList.Add(false);
        }

        public GameObject GetInactiveClinicPatient()
        {
            return GetInactiveCharacter(ref clinicPatients, clinicPatientName, creator.ClinicCharacterPrefab, clinicPatientsContainer, ref clinicTaken);
        }

        public GameObject GetInactiveHospitalPatient()
        {
            return GetInactiveCharacter(ref hospitalPatients, hospitalPatientName, creator.HospitalCharacterPrefab, hospitalPatientsContainer, ref hospitalTaken);
        }

        public GameObject GetInactiveDoctor()
        {
            return GetInactiveCharacter(ref doctors, doctorName, creator.DoctorPrefab, doctorsContainer, ref doctorsTaken);
        }

        public GameObject GetInactiveNurse()
        {
            return GetInactiveCharacter(ref nurses, nurseName, creator.NursePrefab, nursesContainer, ref nursesTaken);
        }

        private GameObject GetInactiveCharacter(ref List<GameObject> characterList, string characterName, GameObject characterPrefab, GameObject characterParent, ref List<bool> takenList)
        {
            GameObject inactiveCharacter = null;
            for (int i = 0; i < characterList.Count; ++i)
            {
                if (!characterList[i].activeSelf || !takenList[i])
                {
                    inactiveCharacter = characterList[i];
                    takenList[i] = true;
                    break;
                }
            }

            if (inactiveCharacter == null)
            {
                AddCharacter(ref characterList, characterList.Count, characterName, characterPrefab, characterParent, ref takenList);
                inactiveCharacter = characterList[characterList.Count - 1];
                takenList[characterList.Count - 1] = true;

                //add extra buffer characters, so that there's always some spare characters, otherwise list can continuosly grow if characters are spawned faster than they are freed
                for (int i = 0; i < bufferCharactersNumber; ++i)
                {
                    AddCharacter(ref characterList, characterList.Count, characterName, characterPrefab, characterParent, ref takenList);
                }
            }

            return inactiveCharacter;
        }
    }
}