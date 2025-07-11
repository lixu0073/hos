using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using IsoEngine;

namespace Hospital
{
    public class Playground : ExternalRoom
    {
        public static event EventHandler OnKidsUnWrap;
        private bool initialized = false;
        [SerializeField] private List<ToyInfo> toyPrefabs = null;
        [SerializeField] private Vector2i position = new Vector2i(0, 0);
#pragma warning disable 0649
        [SerializeField] Animator treeAnimator;
#pragma warning restore 0649
        private List<ChildPatientAI> dummyKids;
        private List<ChildPatientAI> kids;

        public virtual bool CanAddPositiveEnergy()
        {
            return Game.Instance.gameState().GetHospitalLevel() >= 12 && ExternalHouseState == EExternalHouseState.enabled;
        }

        public int DummyKidsCount
        {
            get
            {
                if (dummyKids != null)
                    return dummyKids.Count;

                return 0;
            }
        }

        #region DestroyMethods
        // destroy playground object
        public override void IsoDestroy()
        {
            initialized = false;

            FreeKidsList(dummyKids);
            FreeKidsList(kids);

            base.IsoDestroy();
        }

        // destroy selected childPatientList
        private void FreeKidsList(List<ChildPatientAI> kidList)
        {
            if (kidList != null && kidList.Count > 0)
            {
                int childCount = kidList.Count;
                for (int i = 0; i < kidList.Count; i++)
                {
                    kidList[i].IsoDestroy();
                }

                kidList.Clear();
            }
        }
        #endregion

        public override void OnClick()
        {
            base.OnClick();
            //SoundsController.Instance.PlayButtonClick2();
        }

        public int wallID = 0;

        public override void SetLevel(int lvl, bool showParticles = true)
        {
            base.SetLevel(lvl);

            if (!initialized && lvl != 0)
            {
                foreach (var p in toyPrefabs)
                    p.ID = ReferenceHolder.Get().engine.AddObjectToEngine(p.toyPrefab);

                initialized = true;
                actualLevel = 1;
                lvl = 1;
                treeAnimator.SetTrigger("Active");
                Adorners.SetActive(true);

                for (int i = 0; i < toyPrefabs.Count; i++)
                {
                    if (HospitalAreasMapController.HospitalMap.GetObject(toyPrefabs[i].position + position) == null)
                    {
                        if (toyPrefabs[i].toyPrefab != null)
                            HospitalAreasMapController.HospitalMap.AddObject(toyPrefabs[i].position + position, toyPrefabs[i].ID);
                    }
                }

            }

            actualLevel = lvl;
        }

        public bool CanGetKids()
        {
            var p = (actualLevel > 0);
            return p;
        }

        public ChildPatientAI AddKidDummy(ChildPatientAI patient)
        {
            if (dummyKids == null)
                dummyKids = new List<ChildPatientAI>();

            dummyKids.Add(patient);
            return patient;
        }

        public ChildPatientAI AddKid(ChildPatientAI patient)
        {
            if (kids == null)
                kids = new List<ChildPatientAI>();

            kids.Add(patient);
            return patient;
        }

        public void RemoveKid(ChildPatientAI patient)
        {
            if (kids != null)
            {
                kids.Remove(patient);
            }
        }

        public void RemoveDummyKid(ChildPatientAI patient)
        {
            if (dummyKids != null)
            {
                dummyKids.Remove(patient);
            }
        }

        public void UpdateKidWithoutRoom(DoctorRoom newRoom)
        {
            if (kids != null && kids.Count > 0)
            {
                for (int i = 0; i < kids.Count; i++)
                {
                    if (kids[i] != null && !kids[i].isGoHome && kids[i].room == null && kids[i].GetComponent<ClinicCharacterInfo>().clinicDisease.Doctor == (DoctorRoomInfo)(newRoom.GetRoomInfo()))   //FIXME: && this is the room he has been waiting for
                    {
                        kids[i].room = newRoom;
                        newRoom.AddPatientToQueue(kids[i]);
                        kids[i].SendToDoctorRoom();  //?? not sure about spots, logically first spot will be free when it's first patient
                    }
                }
            }
        }

        #region PlaygroundManagementMethods
        public override void OnClickDisabled()
        {
            UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.KidsRoom);
            //SoundsController.Instance.PlayButtonClick2();
        }

        public override void OnClickWaitingForRenew()
        {
            if (!TutorialController.Instance.tutorialEnabled || (TutorialController.Instance.tutorialEnabled && TutorialController.Instance.CurrentTutorialStepIndex >= TutorialController.Instance.GetStepId(StepTag.kids_open)))
            {
                UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.KidsRoom, true, false, () => confirmRenew());
                NotificationCenter.Instance.KidsUIOpen.Invoke(new KidsUIOpenEventArgs());
            }
            else
            {
                HospitalTutorialController.HospitalTutorialInstance.KidsClickArea.GetComponent<TutorialKidsClickArea>().OnClick();
                //TutorialController.Instance.KidsClickArea.GetComponent<TutorialKidsClickArea>().OnClick();
                //SoundsController.Instance.PlayConstruction ();
            }
        }

        public override void OnClickWaitingForUser()
        {
            base.OnClickWaitingForUser();
            //CHANGE
            SetLevel(1);
            if (!TutorialController.Instance.tutorialEnabled || (TutorialController.Instance.tutorialEnabled && TutorialController.Instance.CurrentTutorialStepIndex >= TutorialController.Instance.GetStepId(StepTag.kids_open)))
            {
                if (dummyKids != null)
                {
                    for (int i = dummyKids.Count; i < toyPrefabs.Count; i++)
                        ReferenceHolder.GetHospital().ClinicAI.SpawnKidAtPlayroom(GameState.RandomNumber(100) > 50);
                }
                else
                {
                    for (int i = 0; i < toyPrefabs.Count; i++)
                        ReferenceHolder.GetHospital().ClinicAI.SpawnKidAtPlayroom(GameState.RandomNumber(100) > 50);
                }
            }

            //CHANGE
            NotificationCenter.Instance.KidsRoomUnlocked.Invoke(new KidsRoomUnlockedEventArgs());

            //particle spawnuja sie z ExternalRoom.cs linia 198
            /*var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(position.x + 1, 0, position.y + 1) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
            fp.transform.localScale = Vector3.one * 1.4f;
            fp.SetActive(true);*/
            DissmisSomePatientsInRoomsAndSetForceSpawnKids();
            EventHandler onKidsUnWrap = OnKidsUnWrap;
            if (onKidsUnWrap != null)
            {
                OnKidsUnWrap(this, null);
            }
        }

        private void ForceSpawnKidsInNewxSpawn()
        {
            List<DoctorRoom> rooms = FindObjectsOfType<DoctorRoom>().ToList();
            if (rooms.Count == 0)
            {
                return;
            }
            if (!VisitingController.Instance.IsVisiting)
                GameState.Get().KidsToSpawn = rooms.Count;
        }

        private void DissmisSomePatientsInRoomsAndSetForceSpawnKids()
        {
            List<DoctorRoom> rooms = FindObjectsOfType<DoctorRoom>().ToList();
            if (rooms.Count == 0)
            {
                return;
            }
            foreach (DoctorRoom room in rooms)
            {
                room.DissmisPatientIfPossibleOrForceSpawnInNextSpawn();
            }
        }

        public override void OnClickEnabled()
        {
            base.OnClickEnabled();
            StartCoroutine(UIController.getHospital.ChildrenPopUp.Open(true, false, () =>
            {
                NotificationCenter.Instance.KidsUIOpen.Invoke(new KidsUIOpenEventArgs());
            }));
        }
        #endregion

        public bool SetKidsRoomFullOffKids()
        {
            //Debug.LogError("Unlock child");

            bool isNeededTofull = false;
            if (dummyKids == null)
                dummyKids = new List<ChildPatientAI>();

            if (toyPrefabs != null)
            {
                isNeededTofull = true;
            }

            return isNeededTofull;

        }

        #region PlaygroundToySpotsMethods
        public int GetToySpotCount()
        {
            if (toyPrefabs != null)
                return toyPrefabs.Count;

            return -1;
        }

        public int GetToySpotType(int spodID)
        {
            if (toyPrefabs != null && toyPrefabs.Count > 0 && spodID > -1 && spodID < toyPrefabs.Count)
                return toyPrefabs[spodID].id;

            return 1;
        }

        public Vector2i GetToyPositionForID(int spodID)
        {
            if (toyPrefabs != null && toyPrefabs.Count != 0 && spodID >= 0 && spodID < toyPrefabs.Count)
            {
                return position + toyPrefabs[spodID].spot;

            }
            return Vector2i.zero; //new Vector2i(position.x+2, position.y+2);
        }

        public int GetFreeToySpotID(ChildPatientAI kid)
        {
            int id = -1;

            foreach (ToyInfo toy in toyPrefabs)
            {
                id++;
                if (!toy.IsTaken() || (toy.GetTaken() == kid))
                {
                    SetToySpotTaken(id, kid);
                    return id;
                }
            }

            return -1;
        }

        public void SetToySpotTaken(int id, ChildPatientAI kid)
        {
            if (toyPrefabs != null && id < toyPrefabs.Count())
            {
                toyPrefabs[id].TakeBy(kid);
            }
        }
        #endregion

        public override string SaveToString()
        {
            StringBuilder saveBuilder = new StringBuilder();
            saveBuilder.Append(base.SaveToString());
            if (dummyKids != null && dummyKids.Count > 0)
            {
                saveBuilder.Append("$");
                for (int i = 0; i < dummyKids.Count; i++)
                {
                    if (dummyKids.ElementAt(i) != null && !dummyKids.ElementAt(i).iSDoctorRoomWithSickness())
                    {
                        saveBuilder.Append(dummyKids.ElementAt(i).SaveToString());
                        //if (i < patients.Count - 1) {
                        saveBuilder.Append("?");
                        //}
                    }
                }
            }
            return saveBuilder.ToString();

        }

        public override void LoadFromString(string save, TimePassedObject saveTime)
        {
            base.LoadFromString(save, saveTime);
            if (string.IsNullOrEmpty(save))
            {
                return;
            }
            var saveDat = save.Split('$');
            if (saveDat.Length < 4)
            {
                return;
            }
            var savedChildPatients = saveDat[3].Split('?');
            var spawner = ReferenceHolder.GetHospital().ClinicAI;

            for (int i = 0; i < savedChildPatients.Length - 1; i++)
            {
                if (i < toyPrefabs.Count)
                {
                    var infos = savedChildPatients[i].Split('^');
                    AddKidDummy(spawner.LoadPatient(null, infos[0], infos[1], false, null) as ChildPatientAI);
                }
            }
        }

        public override string GetBuildingTag()
        {
            return "kidsRoom";
        }
    }


    [Serializable]
    public class ToyInfo
    {
        public GameObject toyPrefab;
        public int ID;
        public int id;
        public Vector2i position;
        public Vector2i spot;
        public bool isOutside = false;
        private ChildPatientAI takenBy;

        public bool IsTaken()
        {
            if (takenBy != null)
                return true;
            else return false;
        }

        public ChildPatientAI GetTaken()
        {
            return takenBy;
        }

        public void TakeBy(ChildPatientAI kid)
        {
            takenBy = kid;
        }

        public void LetItGo()
        {
            takenBy = null;
        }
    }

    public static class ListExtender
    {
        public static T Random<T>(this List<T> list)
        {
            return list[GameState.RandomNumber(0, list.Count)];
        }

        public static T RandomOther<T>(this List<T> list, float seed)
        {

            return list[(int)Mathf.Floor(seed * list.Count)];
        }
    }
}