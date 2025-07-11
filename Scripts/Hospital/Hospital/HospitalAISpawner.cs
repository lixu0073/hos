using UnityEngine;
using System.Collections;
using IsoEngine;
using System.Collections.Generic;

namespace Hospital
{
	public class HospitalAISpawner : MonoBehaviour
	{
        public CharacterCreator creator;
        public float RegularWaitTime;
        public float VipWaitTime;
        private LinkedList<GameObject> spawnQueue;
        private LinkedList<GameObject> VIPSpawnQueue;
        private int count;
        public float vipcounter;
        private float regcounter;

        public void RemoveAllSpawnedPatient()
        {
            if (gameObject != null && gameObject.transform.childCount > 0)
            {
                foreach (Transform t in gameObject.transform)
                {
                    if (t.GetComponent<ClinicPatientAI>() != null)
                    {
                        t.GetComponent<ClinicPatientAI>().IsoDestroy();
                    }
                }
            }
        }

        public void Reset()
		{
            RemoveAllSpawnedPatient();
            spawnQueue.Clear();
			VIPSpawnQueue.Clear();
		}
		public HospitalPatientAI SpawnPerson(HospitalRoom room, string info)
		{
			bool vip = info[0] == 'V';
			GameObject character = creator.RandomHospitalCharacter(Random.Range(0, 3), Random.Range(0, 2), 1, vip); //Spawn ludzików typu pacjent
			var AI = character.AddComponent<HospitalPatientAI>();
            character.GetComponent<HospitalCharacterInfo>().Randomize(room);
            AI.transform.rotation = Quaternion.Euler(45, 45, 0);
			AI.Initialize(room, info);

			character.name = "Patient" + count.ToString();
			character.SetActive(true);
			++count;

            //if (indicator)
            //    ShowIndicator(character);

            return AI;
		}
        public HospitalPatientAI SpawnPerson(Vector2i position, HospitalRoom destRoom, bool vip, bool isFirstEver = false)
        {
            GameObject character;
            if (isFirstEver)
                character = creator.DefinedHospitalCharacter("0*1*1*False*0*24*NAME_OLIVIA*SURNAME_AVILO*0*3*0"); //Spawn ludzików typu pacjent
            else
                character = creator.RandomHospitalCharacter(Random.Range(0, 3), Random.Range(0, 2), 1, vip); //Spawn ludzików typu pacjent

            var AI = character.AddComponent<HospitalPatientAI>();
			character.GetComponent<HospitalCharacterInfo>().Randomize(destRoom);


            AI.transform.rotation = Quaternion.Euler(45, 45, 0);
            creator.SetHospitalCloth(character.GetComponent<HospitalCharacterInfo>().OriginalClothes);
            AI.Initialize(position, destRoom,0,false);


            character.name = "Patient" + count.ToString();
            character.SetActive(false);
            ++count;
            if(!vip)
            {
                spawnQueue.AddLast(character);
            }
            else
            {
				
				VIPSpawnQueue.AddFirst(character);

            }

            //if (indicator)
            //    ShowIndicator(character);

            return AI;
        }

        public HospitalPatientAI SpawnPersonGoToBedWithID(Vector2i position, HospitalRoom destRoom, int localBedId, bool vip, bool isFirstEver = false)
        {
            Debug.Log("SpawnPersonGoToBedWithID" + localBedId + " vip " + vip + " isFirstEver " + isFirstEver + " destRoom " + destRoom.name + " position " + position);
            GameObject character;
            if (isFirstEver)
                character = creator.DefinedHospitalCharacter("0*1*1*False*0*24*NAME_OLIVIA*SURNAME_AVILO*0*3*0"); // 0: Race, 1: Sex, 2: Type, 3: IsVIP, 4: VIPTime, 5: Age, 6: Name, 7: Surname, 8: Head, 9: Body, 10: Legs, 11: Likes, 12: Dislikes, 13: BloodType
            else
                character = creator.RandomHospitalCharacter(Random.Range(0, 3), Random.Range(0, 2), 1, vip); //Spawn ludzików typu pacjent

            var AI = character.AddComponent<HospitalPatientAI>();

            character.GetComponent<HospitalCharacterInfo>().Randomize(destRoom);
            AI.transform.rotation = Quaternion.Euler(45, 45, 0);
            creator.SetHospitalCloth(character.GetComponent<HospitalCharacterInfo>().OriginalClothes);
            AI.Initialize(position, destRoom, localBedId, false);
            character.name = "Patient" + count.ToString();// + " to bed id: " + localBedId;
            character.SetActive(false);
            ++count;
            if (!vip)
            {
                spawnQueue.AddLast(character);
            }
            else
            {
                VIPSpawnQueue.AddFirst(character);
            }

            return AI;
        }

        public HospitalPatientAI LoadPerson(HospitalRoom room, string info, string extendedInfo){
			bool vip = info[0] == 'V';
			GameObject character = creator.DefinedHospitalCharacter(extendedInfo); //Spawn ludzików typu pacjent
			var AI = character.AddComponent<HospitalPatientAI>();
			character.GetComponent<HospitalCharacterInfo>().Randomize(room,true);
			AI.transform.rotation = Quaternion.Euler(45, 45, 0);
			AI.Initialize(room, info);
			character.name = "Patient" + count.ToString();
			character.SetActive(true);
			++count;

			//if (indicator)
			//	ShowIndicator(character);

			return AI;
		}

        public HospitalPatientAI SpawnPersonToBed(Vector2i position, HospitalRoom destRoom,int bedID, bool vip, bool spawned)
        {
            GameObject character = creator.RandomHospitalCharacter(Random.Range(0, 3), Random.Range(0, 2), 1, vip); //Spawn ludzików typu pacjent
            var AI = character.AddComponent<HospitalPatientAI>();
            character.GetComponent<HospitalCharacterInfo>().Randomize(destRoom);
            AI.transform.rotation = Quaternion.Euler(45, 45, 0);
            AI.Initialize(position, destRoom, bedID, spawned);

            character.name = "Patient" + count.ToString();
            ++count;
            character.SetActive(true);

            return AI;
        }

        void Start()
        {
            count = 0;
            spawnQueue = new LinkedList<GameObject>();
            VIPSpawnQueue = new LinkedList<GameObject>();
            vipcounter = VipWaitTime;
            regcounter = RegularWaitTime;           
        }

        void Update()
        {
            if(spawnQueue!=null && spawnQueue.Count > 0)
            {

                if (regcounter > 0)
                {
                    regcounter -= Time.deltaTime;
                }
                else
                {
                    spawnQueue.First.Value.SetActive(true);
                    spawnQueue.RemoveFirst();
                    regcounter = RegularWaitTime;
                }

            }

            if(VIPSpawnQueue.Count > 0 && !HospitalAreasMapController.HospitalMap.emergency.ambulance.gameObject.activeSelf)
            {
                if (vipcounter > 0 )
                {
                    vipcounter -= Time.deltaTime;
                }
                else if(!HospitalAreasMapController.HospitalMap.emergency.ambulance.gameObject.activeSelf)
                {
                   HospitalAreasMapController.HospitalMap.emergency.ambulance.GetComponent<AmbulanceController>().SpawnAmbulance();
                }
            }

        }

        public void SpawnAmbulanceVIP()
        {
            VIPSpawnQueue.First.Value.SetActive(true);
            VIPSpawnQueue.RemoveFirst();
            vipcounter = VipWaitTime;
        }
	}
}
