using UnityEngine;
using SimpleUI;
using IsoEngine;
using System;
using System.Collections.Generic;
using MovementEffects;
using System.Globalization;

namespace Hospital
{
    public class HospitalPatientAI : BasePatientAI, IDiagnosePatient
    {
        public static List<HospitalCharacterInfo> Patients; //pewnie do zmiany!
        static HospitalPatientAI()
        {
            Patients = new List<HospitalCharacterInfo>();
        }

        public static void ResetAllHospitalPatients()
        {
            if (Patients != null && Patients.Count > 0)
            {
                for (int i = 0; i < Patients.Count; ++i)
                {
                    if (Patients[i] != null && Patients[i].GetComponent<BasePatientAI>() != null)         
                        Patients[i].GetComponent<BasePatientAI>().IsoDestroy();
                }

                Patients.Clear();
            }
        }

        public static void ResetAllPatientsDailyQuest()
        {
            if (Patients != null && Patients.Count > 0)
            {
                for (int i = 0; i < Patients.Count; ++i)
                {
                    if (Patients[i] != null)
                    {
                        var patHCI = Patients[i].GetComponent<HospitalCharacterInfo>();

                        if (patHCI != null)
                        {
                            patHCI.WasPatientCardSwipe = false;
                            patHCI.WasPatientInfoSeen = false;
                        }
                    }
                }
            }
        }

        public StateManager Person;
        HospitalRoom destRoom;
        public bool healthy = false;
        public int spotID;
        public float diagnoseTime = 0;
        public CharacterStatus state = CharacterStatus.None;
        private int queueID = -1;

        public int GetQueueID()
        {
            return queueID;
        }

        public void SetQueueID(int newID)
        {
            queueID = newID;
        }

        public enum CharacterStatus
        {
            Diagnose,
            InQueue,
            Healed,
            None
        }

        public bool goHome = false;
        public bool GetGoHome()
        {
            return goHome;
        }

        public bool MoveWithRoom  //to tylko w hp
        {
            get;
            private set;
        }

        int bedID;

        public BaseCharacterInfo GetSprites()
        { //zbedne bez interfejsu
            return sprites;
        }

        public float GetDiagnoseTime()
        { //zbedne bez interfejsu
            return diagnoseTime;
        }

        public Component GetAI()
        { //zbedne bez interfejsu
            return gameObject.GetComponent<HospitalPatientAI>();
        }

        public void SetStateDiagnose()
        {
            state = HospitalPatientAI.CharacterStatus.Diagnose;
        }

        public void SetStateInQueue()
        {
            state = HospitalPatientAI.CharacterStatus.InQueue;
        }

        public void Initialize(Vector2i pos, HospitalRoom destRoom, int destBedID, bool spawned)
        {
            spotID = destBedID;

            base.Initialize(pos);
            this.Person = new StateManager();
            this.destRoom = destRoom;
            if (!spawned)
                GoChange();
            else
                ToBed(destBedID);
        }

        public HospitalCharacterInfo GetHospitalCharacterInfo()
        {
            return GetComponent<HospitalCharacterInfo>();
        }

        public bool IsGoHomeState()
        {
            return Person.State is HospitalPatientAI.GoHome;
        }

        public bool IsInBedState()
        {
            if (Person != null && Person.State != null)
                return (Person.State is HospitalPatientAI.InBed && MoveWithRoom);

            return false;
        }

        public bool goHomeGet()
        {
            return goHome;
        }

        public void ChangeToPajama()
        {
            var p = this.gameObject.GetComponent<HospitalCharacterInfo>();
            List<Sprite> Pajama = ReferenceHolder.GetHospital().PersonCreator.GetPijama(p.Race, p.Sex, p.IsVIP);
            for (int i = 0; i < this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts.Count; ++i)
            {
                this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts[i].sprite = Pajama[i];
            }
            return;
        }

        public override string SaveToString()
        {
            var z = GetComponent<HospitalCharacterInfo>();
            var p = HospitalDataHolder.Instance.QueueContainsPatient(this);
            return Checkers.CheckedPosition(position, z.name) + "!" + Checkers.CheckedAmount(spotID, 0, 1, z.name + " spotID: ") + "!" + Checkers.CheckedHospitalPatientStatus(state, z.name) + "!" + Checkers.CheckedBool(p) + "!" + Checkers.CheckedAmount(queueID, -1, int.MaxValue, "Queue ID: ") + "!" + z.SaveToString() + "!" + Checkers.CheckedHospitalPatientState(Person.State.SaveToString()) + "^" + Checkers.CheckedPatientBIO(z.personalBIO, z.name);
        }

        public override void IsoDestroy()
        {
            //print("deleting patient from list");
            if (this == null)
                return;

            var p = GetComponent<HospitalCharacterInfo>();
            Patients.Remove(p);
            UIController.getHospital.PatientCard.RemovePatient(p, true);
            HospitalDataHolder.Instance.RemovePatientFromQueues(this);

            p.DisaseDiagnoseType = DiseaseType.None;
            p.WasPatientCardSeen = false;
            p.WasPatientCardSwipe = false;
            p.WasPatientInfoSeen = false;
            p.isUpdatedNonDiagnosableMedicineNeeded = false;
            p.isUpdatedDiagnosableMedicineNeeded = false;
            p.lastMedicine = null;
            p.patientCardStatus = HospitalCharacterInfo.PatientCardInfoStatus.Default;

            base.IsoDestroy();
        }

        public override void Initialize(RotatableObject room, string info)
        {
            //Debug.Log(info);
            var strs = info.Split('!');
            base.Initialize(Vector2i.Parse(strs[0]));
            spotID = int.Parse(strs[1], System.Globalization.CultureInfo.InvariantCulture);
            state = (CharacterStatus)Enum.Parse(typeof(CharacterStatus), strs[2]);
            //print(info);
            destRoom = (HospitalRoom)room;
            destRoom.ReacquireTakenSpot(spotID);
            Person = new StateManager();
            var queue = bool.Parse(strs[3]);
            queueID = int.Parse(strs[4], System.Globalization.CultureInfo.InvariantCulture);
            GetComponent<HospitalCharacterInfo>().FromString(strs[5]);
            if (queue)
                HospitalDataHolder.Instance.AddToDiagnosticQueue(this);
            if (strs.Length > 6)
            {
                switch (strs[6])
                {
                    case "GTCR":
                        Person.State = new GoToChangeRoom(this);
                        break;
                    case "IB":
                        Person.State = new InBed(this);
                        break;
                    case "GTDR":
                        var p = (DiagnosticRoom)HospitalAreasMapController.HospitalMap.FindRotatableObject(strs[7]);
                        p.SetPatient(this);
                        Person.State = new GoToDiagRoom(this, p);
                        break;
                    case "H":
                        Person.State = new Healing(this, (DiagnosticRoom)HospitalAreasMapController.HospitalMap.FindRotatableObject(strs[8]), float.Parse(strs[7], CultureInfo.InvariantCulture));
                        break;
                    case "RTB":
                        state = CharacterStatus.Healed;
                        Person.State = new ReturnToBed(this);
                        break;
                    case "GHV":
                        Person.State = new GoHomeVIP(this);
                        break;
                    case "GH":
                        Person.State = new GoHome(this);
                        break;
                    case "GTR":
                        Person.State = new GoToRoom(this, int.Parse(strs[7]));
                        break;
                    default:
                        break;
                }
            }
            var tmp = GetComponent<HospitalCharacterInfo>();
            if (!Patients.Contains(tmp) && !(Person.State is GoToRoom || Person.State is GoHome || Person.State is GoHomeVIP || Person.State is GoToChangeRoom))
            {
                Patients.Add(tmp);
                if (tmp.RequiresDiagnosis)
                {
                    HospitalDataHolder.Instance.ReturnDiseaseRoom((int)(tmp.DisaseDiagnoseType)).ShowIndicator();
                }
            }
        }

        public float EmulateTime(float time)
        {
            /*if (Person.State is Healing)
            {
                int roomHealingDuration = ((Healing)Person.State).currentRoom.healingDuration;
                if (diagnoseTime + time >= roomHealingDuration)
                {
                    Debug.LogError("ForceTeleport");
                    //Person.State.Notify((int)StateNotifications.ForceTeleportToHospitalBed, null);
                    return time - (roomHealingDuration - diagnoseTime);
                }
                else
                {
                    diagnoseTime += time;
                    return -1f;
                }
            }*/
            return time;
        }

        public bool isEscapeFromRoom()
        {
            if (Person.State is GoToChangeRoom || Person.State is GoToDiagRoom)
            {
                Debug.LogWarning(" Go home or go diag room now");
                return true;
            }

            return false;
        }

        public override void Initialize(Vector2i pos)
        {
            base.Initialize(pos);
            this.Person = new StateManager();
        }

        #region Methods
        private List<Transform> lowerBodyParts;

        private void Start()
        {
            lowerBodyParts = new List<Transform>();
            Transform animatorT = gameObject.transform.GetChild(0);
            lowerBodyParts.Add(animatorT.GetChild(8));
            lowerBodyParts.Add(animatorT.GetChild(9));
            lowerBodyParts.Add(animatorT.GetChild(10));
            lowerBodyParts.Add(animatorT.GetChild(11));
            lowerBodyParts.Add(animatorT.GetChild(12));
            lowerBodyParts.Add(animatorT.GetChild(13));
            lowerBodyParts.Add(animatorT.GetChild(14));
        }

        private void SetLowerBodyPartsActive(bool active)
        {
            foreach (Transform t in lowerBodyParts)
                t.gameObject.SetActive(active);
        }

        protected override void Update()
        {
            base.Update();

            if (Person != null)
                Person.Update();
        }

        private void LateUpdate()
        {
            //hack for bug when there's no legs after deactivating and activating the gameObject
            if (GetPersonState() != 1)
            {
                SetLowerBodyPartsActive(true);
            }
        }

        public override void Notify(int id, object parameters)
        {
            if (Person.State != null)
                Person.State.Notify(id, parameters);
        }

        public void TeleportToSpot(HospitalRoom hospRoom = null, DiagnosticRoom diagRoom = null, bool val = false)
        {
            if (!val)
            {
                if (hospRoom != null)
                    LayInBed(true);
                if (diagRoom != null)
                {
                    var p = diagRoom.GetMachineObject().transform.GetChild(1).transform.position;
                    TeleportTo(p);
                }
            }
        }
        public void GoChange(float delay = 0.0f)
        {
            //Debug.LogError("GoChange");
            this.Person.State = new GoToChangeRoom(this, delay);
        }

        public void RemoveFromDiagnose() { }

        public void GoHomeSweetHome(float delay = 0f)
        {
            //Debug.LogError("GoHomeSweetHome");
            this.Person.State = new GoHome(this);
        }

        public void ToBed(int posID)
        {
            bedID = posID;

            //Vector2i pos = destRoom.ReacquireTakenSpot(posID);
            /*
            if (UIController.get.PatientCard.gameObject.activeSelf)
            {
                UIController.get.PatientCard.RefreshViewOnBed(posID);
                UIController.get.PatientCard.UpdateOtherPatients();
            }
            */
            ChangeToPijama();
            Person.State = new InBed(this);
        }

        public void RefreshPlagueStatus()
        {
            HospitalCharacterInfo currentCharacter = GetComponent<HospitalCharacterInfo>();
            HospitalCharacterInfo otherCharacter = destRoom.GetOtherPatient(currentCharacter);

            if (otherCharacter != null)
            {
                if (currentCharacter.HasBacteria)
                {
                    // CHECK THIS PATIENT HAS PLAGUE THEN UPDTE PLAGUE TIME FOR OTHER PATIENT
                    if (!otherCharacter.HasBacteria && otherCharacter.BacteriaGlobalTime == 0)
                    {
                        MedicineRef bacteria = currentCharacter.GetBacteria();
                        currentCharacter.SetBacteriaValues(bacteria.id);
                        otherCharacter.SetBacteriaValues(bacteria.id);
                        otherCharacter.BacteriaGlobalTime = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
                        Debug.Log("SET Bacteria time for patient: " + otherCharacter.Name);
                        UIController.getHospital.PatientCard.RefreshView(UIController.getHospital.PatientCard.CurrentCharacter);
                    }
                }
                else if (currentCharacter.BacteriaGlobalTime == 0 && otherCharacter.HasBacteria)  // CHECK OTHER PATIENT HAS PLAQUE THEN UPDTE PLAGUE TIME FOR HIM
                {
                    MedicineRef bacteria = otherCharacter.GetBacteria();
                    otherCharacter.SetBacteriaValues(bacteria.id);
                    currentCharacter.SetBacteriaValues(bacteria.id);

                    currentCharacter.BacteriaGlobalTime = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
                    Debug.Log("SET Bacteria time for patient: " + currentCharacter.Name);
                    UIController.getHospital.PatientCard.RefreshView(UIController.getHospital.PatientCard.CurrentCharacter);
                }
            }
        }

        public void ResetPlagueStatus() // reset plague for other patient when discharfe
        {
            HospitalCharacterInfo otherCharacter = destRoom.GetOtherPatient(GetComponent<HospitalCharacterInfo>());
            if (otherCharacter != null && !otherCharacter.HasBacteria)
            {
                otherCharacter.BacteriaGlobalTime = 0;
                //Debug.LogError("RESET Plague time for patient: " + otherCharacter.Name);
            }
        }

        public HospitalCharacterInfo GetOtherRoomPatient(HospitalCharacterInfo info)
        {
            HospitalCharacterInfo otherCharacter = destRoom.GetOtherPatient(info);

            if (otherCharacter != null)
                return otherCharacter;

            return null;
        }

        public void AddBacteriaFromOtherPatient()
        {
            HospitalCharacterInfo otherCharacter = destRoom.GetOtherPatient(GetComponent<HospitalCharacterInfo>());
            if (otherCharacter != null && otherCharacter.HasBacteria)
            {
                if (otherCharacter.requiredMedicines != null)
                {
                    for (int i = 0; i < otherCharacter.requiredMedicines.Length; ++i)
                    {
                        if (otherCharacter.requiredMedicines[i].Key.Disease.DiseaseType == DiseaseType.Bacteria)
                        {
                            GetComponent<HospitalCharacterInfo>().InfectByDisease(otherCharacter.requiredMedicines[i].Key, otherCharacter.requiredMedicines[i].Value);
                            break;
                        }
                    }
                }

                Debug.Log("SET Bacteria time for patient: " + otherCharacter.Name);
            }
        }

        public void LayInBed(bool whileMoving = false)
        {
            HospitalCharacterInfo info = GetComponent<HospitalCharacterInfo>();
            if (!goHome && !whileMoving)
            {
                GameState.Get().UpdateMedicinesNeededListWithPatient(info);
                RefreshPlagueStatus();
            }
            base.LayInBed(destRoom, destRoom.ReturnBed(spotID));

            //if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.diagnose_open_patient_card
            //    && info.RequiresDiagnosis)
            //{
            //    NotificationCenter.Instance.TutorialArrowSet.Invoke(new TutorialArrowSetEventArgs(gameObject));
            //}
            //else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_2
            //  && info.HasBacteria)
            //{
            //    NotificationCenter.Instance.TutorialArrowSet.Invoke(new TutorialArrowSetEventArgs(gameObject));
            //}
        }

        private void StartHealing(RotatableObject room)
        {
            ((DiagnosticRoom)room).StartHealingAnimation();
            ((DiagnosticRoom)room).IsHealing = true;
            ((DiagnosticRoom)room).SetIsHealingTag(true);
            ((DiagnosticRoom)room).UpdateNurseRotation();
            base.SetHealingAnimation(room);
        }

        private void StopHealing(RotatableObject room)
        {
            transform.position = new Vector3(((DiagnosticRoom)room).GetMachineSpot().x, 0, ((DiagnosticRoom)room).GetMachineSpot().y);
            // this.transform.position = ((DiagnosticRoom)room).GetMachinePositionPatient();//new Vector3(((DiagnosticRoom)room).GetMachinePosition().x, 0, ((DiagnosticRoom)room).GetMachinePosition().y);
            position = ((DiagnosticRoom)room).GetMachinePosition();

            ((DiagnosticRoom)room).IsHealing = false;
            ((DiagnosticRoom)room).SetIsHealingTag(false);
            ((DiagnosticRoom)room).UpdateNurseRotation();
        }

        public void StopDiagnose(DiagnosticRoom room)
        {
            room.StopHealingAnimation();
            transform.position = new Vector3(room.GetMachineSpot().x, 0, room.GetMachineSpot().y);
            //transform.position = new Vector3(room.GetMachinePosition().x, 0, room.GetMachinePosition().y);
            GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
            state = CharacterStatus.Healed;
            Person.State = new ReturnToBed(this, 1.5f);

            int rewardXP = ((DiagnosticRoomInfo)(room.GetRoomInfo())).CureXPReward;
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, rewardXP, EconomySource.Diagnose, false, room.Tag);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, new Vector3(position.x, 0, position.y), rewardXP, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Exp, rewardXP, currentExpAmount);
            });

            room.IsHealing = false;
            room.SetIsHealingTag(false);
            ((DiagnosticRoom)room).UpdateNurseRotation();
            //Achievement
            HospitalDataHolder.Instance.DecreaseQueueIDs(this);
            AchievementNotificationCenter.Instance.PatientDiagnosed.Invoke(new AchievementProgressEventArgs(1));
        }

        public void CancelDiagnose(DiagnosticRoom room)
        {
            room.StopHealingAnimation();
            transform.position = new Vector3(room.GetMachineSpot().x, 0, room.GetMachineSpot().y);
            //transform.position = new Vector3(room.GetMachinePosition().x, 0, room.GetMachinePosition().y);
            GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
            if ((HospitalPatientAI)(room.currentPatient) == this)
            {
                room.IsHealing = false;
                room.SetIsHealingTag(false);
                room.currentPatient = null;
                room.isTaken = false;
            }
            //room.done = true;

            HospitalDataHolder.Instance.DecreaseQueueIDs(this);
        }

        private void GetOutOfBed()
        {
            this.transform.position = new Vector3(destRoom.ReacquireTakenSpot(spotID).x, 0, destRoom.ReacquireTakenSpot(spotID).y);
            position = destRoom.ReacquireTakenSpot(spotID);
            //destRoom.UnCoverBed(spotID);
        }

        public void DischargeBed()
        {
            destRoom.DischargeBed(spotID);
            // GameState.Get().UpdateMedicinesNeededListWithAllPatients();
        }

        public void FreeBed()
        {
            destRoom.FreeBed(spotID);
        }

        public void UnCoverBed()
        {
            destRoom.UnCoverBed(spotID, this);
        }

        public bool DoneHealing()
        {
            return Person.State is ReturnToBed && !(Person.State is Healing);
        }

        public string GetRoomName()
        {
            if (destRoom != null)
                return destRoom.name;

            return "";
        }

        public HospitalRoom GetDestRoom()
        {
            return destRoom;
        }

        public bool GoingToDiag()
        {
            return Person.State is GoToDiagRoom;
        }

        public bool GoingToRoom()
        {
            return (Person.State is GoToRoom || Person.State is InBed || Person.State is ReturnToBed);
        }

        public int GetPersonState()
        {
            if (Person.State is GoToRoom) return 0;
            if (Person.State is InBed) return 1;
            if (Person.State is ReturnToBed) return 2;
            if (Person.State is GoToDiagRoom) return 3;
            if (Person.State is Healing) return 4;

            return -1;
        }

        public void StateToDiagRoom(DiagnosticRoom room)
        {
            if (!(Person.State is GoToDiagRoom) && !(Person.State is Healing) && !(Person.State is ReturnToBed))
                this.Person.State = new GoToDiagRoom(this, room);
        }

        public void ChangeToPijama()
        {
            var p = this.gameObject.GetComponent<HospitalCharacterInfo>();
            List<Sprite> Pijama = ReferenceHolder.GetHospital().PersonCreator.GetPijama(p.Race, p.Sex, p.IsVIP);
            for (int i = 0; i < this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts.Count; ++i)
            {
                this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts[i].sprite = Pijama[i];
            }
            return;
        }

        public void ChangeToOriginal()
        {
            for (int i = 0; i < this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts.Count; ++i)
            {
                this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts[i].sprite = this.gameObject.GetComponent<HospitalCharacterInfo>().OriginalClothes[i];
            }
            // for (int j = 0; j < this.gameObject.transform.GetChild(0).gameObject.transform.childCount; j++)
            //     this.gameObject.transform.GetChild(0).gameObject.transform.GetChild(j).gameObject.SetActive(true);

            return;
        }

        protected override void ReachedDestination()
        {
            base.ReachedDestination();
            if (Person.State != null)
                Person.State.Notify((int)StateStatus.FinishedMoving, null);
        }

        public bool SpeedUp(out float time, float timePassed, out bool isTaken)
        {
            time = 0;
            isTaken = false;
            if (timePassed <= 0)
            {
                time = 0;
                return false;
            }
            if (Person.State is Healing)
            {
                DiagnosticRoom currentRoom = ((Healing)Person.State).currentRoom;
                float diff = Math.Max(0, currentRoom.DiagnosisTimeMastered - diagnoseTime);
                if (diff >= timePassed)
                {
                    diagnoseTime += timePassed;
                    time = timePassed;
                    isTaken = true;
                    return false;
                }

                diagnoseTime = currentRoom.DiagnosisTimeMastered;
                time = diff;
                Person.State.Notify((int)StateNotifications.ForceTeleportToHospitalBed, null);
                return true;
            }

            if (Person.State is GoToDiagRoom || Person.State is InBed)
            {
                DiagnosticRoom currentRoom;

                if (Person.State is GoToDiagRoom)
                    currentRoom = ((GoToDiagRoom)Person.State).currentRoom;
                else
                    currentRoom = HospitalDataHolder.Instance.ReturnPatientDiagnosisRoom(this);

                if (timePassed >= currentRoom.DiagnosisTimeMastered)
                {
                    abortPath();
                    Person.State.Notify((int)StateNotifications.ForceTeleportToHospitalBed, null);
                    time = currentRoom.DiagnosisTimeMastered;
                    return true;
                }

                time = timePassed;
                diagnoseTime = timePassed;
                isTaken = true;
                Person.State = new Healing(this, currentRoom, diagnoseTime);
                return false;
            }
            return false;
        }
        #endregion

        #region States
        public class GoToChangeRoom : MainState
        {
            float delay;
            public GoToChangeRoom(HospitalPatientAI parent, float delay = 0.0f) : base(parent)
            {
                this.delay = delay;
                base.OnEnter();
                parent.ChangeToOriginal();
            }

            public override string SaveToString()
            {
                return "GTCR";
            }

            public override void OnEnter()
            {
                base.OnEnter();

                if (parent.goHome)
                {
                    if (delay == 0)
                    {
                        //parent.StartFanfareCoroutine(parent.transform/*, 1.75f*/);
                        Timing.RunCoroutine(FreeBedDelay(delay));
                        parent.destRoom.patientList.Remove(parent);
                        parent.ResetPlagueStatus();
                        parent.SetRandomHurrayAnimation();

                        //parent.ShowNamePatientcured (parent.transform.GetChild (2), parent.GetComponent<HospitalCharacterInfo>()); // it is HospitalCharacterPrefab FloatingInfo

                        if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                            Instantiate(ResourcesHolder.GetHospital().ParticleCure, parent.transform.position, Quaternion.identity);

                        Timing.RunCoroutine(GoHome(1.75f));
                        parent.healthy = true;
                        //tututu
                        HospitalCharacterInfo hearts = parent.GetComponent<HospitalCharacterInfo>();
                        //hearts.SetHeartsActive();
                    }
                    else
                    {
                        Timing.RunCoroutine(DischargeBedDelay(0));
                        parent.destRoom.patientList.Remove(parent);
                        parent.ResetPlagueStatus();
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Unmoving, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        Timing.RunCoroutine(GoHome(0f));
                        parent.healthy = false;
                    }
                }
                else
                    Timing.RunCoroutine(GoHome(0f));

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);

                //if (UIController.get.PatientCard.gameObject.activeInHierarchy){
                //	UIController.get.PatientCard.RefreshViewOnBed(parent.bedID);//MEMEME
                //	UIController.get.PatientCard.UpdateOtherPatients();//MEMEME
                //}
            }

            IEnumerator<float> FreeBedDelay(float delay)
            {
                yield return Timing.WaitForSeconds(delay);
                parent.FreeBed();
                parent.destRoom.ReturnTakenSpot(parent.spotID);
            }

            IEnumerator<float> DischargeBedDelay(float delay)
            {
                yield return Timing.WaitForSeconds(delay);
                parent.DischargeBed();
                parent.destRoom.ReturnTakenSpot(parent.spotID);
            }

            IEnumerator<float> GoHome(float delay)
            {
                //Debug.LogError("Change Room Go Home delay: " + delay);
                yield return Timing.WaitForSeconds(delay);

                // there was old code, it doesn't work without this line
                parent.GoTo(parent.position, PathType.GoHomePath);

                UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (id == (int)StateNotifications.FinishedMoving)
                {
                    if (parent.goHome)
                    {
                        parent.ChangeToOriginal();
                        if (parent.gameObject.GetComponent<HospitalCharacterInfo>().IsVIP)
                            parent.Person.State = new GoHomeVIP(parent);
                        else
                            parent.Person.State = new GoHome(parent);
                    }
                    else
                    {
                        parent.ChangeToPijama();
                        parent.Person.State = new GoToRoom(parent, -1, parent.spotID);
                    }
                }
            }
        }

        public class InBed : MainState
        {
            public InBed(HospitalPatientAI parent) : base(parent)
            {
                var tmp = parent.GetComponent<HospitalCharacterInfo>();
                if (!Patients.Contains(tmp))
                {
                    Patients.Add(tmp);

                    if (tmp.RequiresDiagnosis)
                    {
                        if (HospitalDataHolder.Instance.ReturnDiseaseRoom((int)(tmp.DisaseDiagnoseType)) != null)
                            HospitalDataHolder.Instance.ReturnDiseaseRoom((int)(tmp.DisaseDiagnoseType)).ShowIndicator();
                    }
                }
                tmp.UpdateReward();

                UIController.getHospital.PatientCard.UpdateOtherPatients();
                UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
            }

            public override string SaveToString()
            {
                return "IB";
            }

            public override void OnEnter()
            {
                base.OnEnter();
                //Debug.LogError("PatientReachedBed invoke");
                //if (TutorialController.Instance != null && TutorialController.Instance.CurrentTutorialStepIndex > 0)

                parent.transform.GetChild(1).gameObject.SetActive(false);

                if (!HospitalAreasMapController.HospitalMap.hospitalBedController.isPatientInBed(parent.destRoom, parent.spotID, parent))
                {
                    parent.destRoom.CoverBed(parent.spotID, parent);
                    parent.LayInBed();
                    parent.MoveWithRoom = true;
                    parent.ChangeToPijama();
                }
                else
                {
                    Debug.LogError("bed occupate - call MARIAN!!!");
                    int other_free_id = HospitalAreasMapController.HospitalMap.hospitalBedController.GetFreeyBedIDWithPatientFromRoom(parent.destRoom);

                    if (other_free_id != -1)
                    {
                        Debug.LogError("no free bed spots !!!");
                        parent.destRoom.CoverBed(parent.spotID, parent);
                        parent.LayInBed();
                        parent.MoveWithRoom = true;
                        parent.ChangeToPijama();
                    }
                    else parent.Person.State = new InBed(parent);
                }

                UIController.getHospital.PatientCard.RefreshViewOnBed(parent.bedID);
                NotificationCenter.Instance.PatientReachedBed.Invoke(new PatientReachedBedEventArgs(parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis, parent.GetComponent<HospitalCharacterInfo>().HasBacteria));

                //    Debug.Log("in bed");
                //parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.notActive); //bed controller should do the job
                //parent.GetComponent<IPersonCloudController> ().SetCloudMessageType (CloudsManager.MessageType.inBed);
            }

            public override void OnExit()
            {
                base.OnExit();

                parent.transform.GetChild(1).gameObject.SetActive(true);
                parent.GetOutOfBed();
                parent.MoveWithRoom = false;
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                switch ((StateNotifications)id)
                {
                    case StateNotifications.ForceTeleportToHospitalBed:
                        parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
                        HospitalDataHolder.Instance.DecreaseQueueIDs(parent);
                        this.parent.state = CharacterStatus.Healed;
                        UIController.getHospital.PatientCard.RefreshView(parent.GetComponent<HospitalCharacterInfo>());
                        UIController.getHospital.PatientCard.UpdateOtherPatients();
                        UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
                        parent.Person.State = new InBed(parent);
                        break;
                    case StateNotifications.OfficeMoved:
                        parent.TeleportToSpot(parent.destRoom, null, false);
                        parent.destRoom.CoverBed(parent.spotID, parent);
                        break;
                    case StateNotifications.OfficeAnchored:
                        if (!(bool)parameters)
                            break;
                        parent.TeleportToSpot(parent.destRoom, null, false);
                        parent.destRoom.CoverBed(parent.spotID, parent);
                        break;
                    default:
                        break;
                }
            }
        }

        private class GoToDiagRoom : MainState
        {
            private bool unAnchored = false;

            public DiagnosticRoom currentRoom;

            public GoToDiagRoom(HospitalPatientAI parent, DiagnosticRoom room) : base(parent)
            {
                currentRoom = room;
                HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedWithIDFromRoom(parent.destRoom, parent.spotID).Patient = parent;
                parent.GetComponent<HospitalCharacterInfo>().UpdateReward();
            }

            public override string SaveToString()
            {
                return "GTDR!" + currentRoom.Tag;
            }

            public override void OnEnter()
            {
                parent.UnCoverBed();
                this.parent.state = CharacterStatus.Diagnose;
                UIController.getHospital.PatientCard.RefreshView(this.parent.GetComponent<HospitalCharacterInfo>());
                UIController.getHospital.PatientCard.UpdateOtherPatients();
                UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
                parent.GoTo(currentRoom.GetMachinePosition(), PathType.Default);
                try
                { 
                    parent.anim.Play(AnimHash.Stand_Unmoving, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
                parent.ChangeToPijama();
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                switch ((StateNotifications)id)
                {
                    case StateNotifications.ForceTeleportToHospitalBed:
                        parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
                        ((DiagnosticRoom)currentRoom).IsHealing = false;
                        ((DiagnosticRoom)currentRoom).SetIsHealingTag(false);
                        HospitalDataHolder.Instance.DecreaseQueueIDs(parent);
                        this.parent.state = CharacterStatus.Healed;
                        UIController.getHospital.PatientCard.RefreshView(parent.GetComponent<HospitalCharacterInfo>());
                        UIController.getHospital.PatientCard.UpdateOtherPatients();
                        UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();

                        parent.Person.State = new InBed(parent);
                        break;
                    case StateNotifications.FinishedMoving:
                        parent.Person.State = new Healing(parent, currentRoom, parent.diagnoseTime);
                        break;
                    case StateNotifications.OfficeUnAnchored:
                        try
                        { 
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        parent.walkingStateManager.State = null;
                        unAnchored = true;
                        break;
                    case StateNotifications.OfficeMoved:
                        parent.UnCoverBed();
                        break;
                    case StateNotifications.OfficeAnchored:
                        if (parent.isMovementStopped())
                            parent.GoTo(currentRoom.GetMachinePosition(), PathType.Default);
                        parent.UnCoverBed();
                        unAnchored = false;
                        break;
                    default:
                        break;
                }
            }

            public override void OnUpdate()
            {
                if (parent.diagnoseTime < ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered && !unAnchored)
                    parent.diagnoseTime += Time.deltaTime;
                else if (!unAnchored)
                {
                    ((DiagnosticRoom)currentRoom).StopHealingAnimation();
                    parent.transform.position = new Vector3(currentRoom.GetMachinePosition().x, 0, currentRoom.GetMachinePosition().y);
                    parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
                    parent.state = CharacterStatus.Healed;
                    parent.Person.State = new ReturnToBed(parent);
                    ((DiagnosticRoom)currentRoom).IsHealing = false;
                    ((DiagnosticRoom)currentRoom).SetIsHealingTag(false);
                    HospitalDataHolder.Instance.DecreaseQueueIDs(parent);
                }
            }
        }

        private class Healing : MainState
        {
            private bool unAnchored = false;
            public DiagnosticRoom currentRoom;
            public Healing(HospitalPatientAI parent, DiagnosticRoom room, float time = 0.0f) : base(parent)
            {
                currentRoom = room;

                HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedWithIDFromRoom(parent.destRoom, parent.spotID).Patient = parent;
                this.parent.diagnoseTime = time;

                parent.GetComponent<HospitalCharacterInfo>().UpdateReward();
            }

            public override string SaveToString()
            {
                return "H!" + parent.diagnoseTime.ToString() + "!" + currentRoom.Tag;
            }

            public override void OnEnter()
            {
                base.OnEnter();
                parent.UnCoverBed();
                parent.StopMovement();
                parent.abortPath();
                parent.ChangeToPijama();
                parent.state = CharacterStatus.Diagnose;
                currentRoom.IsHealing = true;
                currentRoom.SetIsHealingTag(true);
                parent.StartHealing(currentRoom);
                //parent.MoveWithRoom = true;
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
                parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.diagnosis);

                parent.AddHappyEffect(currentRoom);

                UIController.getHospital.PatientCard.RefreshViewOnBed(parent.bedID);
            }

            public override void OnExit()
            {
                parent.StopHealing(currentRoom);
                parent.RemoveHappyEffect();
                //parent.MoveWithRoom = false;
            }
            public override void OnUpdate()
            {
                if (parent.diagnoseTime < ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered && !unAnchored)
                    parent.diagnoseTime += Time.deltaTime;
                else if (!unAnchored)
                {
                    ((DiagnosticRoom)currentRoom).StopHealingAnimation();
                    parent.transform.position = new Vector3(currentRoom.GetMachinePosition().x, 0, currentRoom.GetMachinePosition().y);
                    parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
                    parent.state = CharacterStatus.Healed;
                    parent.Person.State = new ReturnToBed(parent);
                    ((DiagnosticRoom)currentRoom).IsHealing = false;
                    ((DiagnosticRoom)currentRoom).SetIsHealingTag(false);
                    HospitalDataHolder.Instance.DecreaseQueueIDs(parent);
                }
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                switch ((StateNotifications)id)
                {
                    case StateNotifications.ForceTeleportToHospitalBed:
                        parent.diagnoseTime = ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered;
                        ((DiagnosticRoom)currentRoom).StopHealingAnimation();
                        parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
                        ((DiagnosticRoom)currentRoom).IsHealing = false;
                        ((DiagnosticRoom)currentRoom).SetIsHealingTag(false);
                        HospitalDataHolder.Instance.DecreaseQueueIDs(parent);
                        this.parent.state = CharacterStatus.Healed;
                        UIController.getHospital.PatientCard.RefreshView(parent.GetComponent<HospitalCharacterInfo>());
                        UIController.getHospital.PatientCard.UpdateOtherPatients();
                        UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
                        parent.Person.State = new InBed(parent);
                        break;
                    case StateNotifications.FinishedMoving:
                        Debug.LogError("StateNotifications.FinishedMoving in Healing State");
                        parent.Person.State = new InBed(parent);
                        break;
                    case StateNotifications.OfficeUnAnchored:
                        unAnchored = true;
                        parent.RemoveHappyEffect();
                        break;
                    case StateNotifications.OfficeAnchored:
                        unAnchored = false;
                        parent.AddHappyEffect(currentRoom);
                        parent.UnCoverBed();
                        break;
                    case StateNotifications.OfficeMoved:
                        parent.StartHealing(currentRoom);
                        parent.UnCoverBed();
                        break;
                    default:
                        break;
                }
            }
        }

        private class ReturnToBed : MainState
        {
            float delay = 0;
            //private bool unAnchored = false;

            //int posID;
            public ReturnToBed(HospitalPatientAI parent, float delay = 0)
                : base(parent)
            {
                this.delay = delay;
                HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedWithIDFromRoom(parent.destRoom, parent.spotID).Patient = parent;
                parent.GetComponent<HospitalCharacterInfo>().UpdateReward();
            }

            public override string SaveToString()
            {
                return "RTB";
            }

            public override void OnEnter()
            {
                base.OnEnter();

                if (delay == 0)
                {
                    if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                    {
                        Instantiate(ResourcesHolder.GetHospital().ParticleCure, parent.transform.position, Quaternion.identity);
                    }

                    parent.ChangeToPijama();
                    Vector2i pos = parent.destRoom.ReacquireTakenSpot(parent.spotID);
                    parent.GoTo(pos, PathType.Default);
                    try
                    { 
                        parent.anim.Play(AnimHash.Stand_Unmoving, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);

                    UIController.getHospital.PatientCard.RefreshViewOnBed(parent.bedID);
                }
                else
                {
                    parent.ChangeToPijama();
                    parent.SetRandomHurrayAnimation();

                    if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                        Instantiate(ResourcesHolder.GetHospital().ParticleCure, parent.transform.position, Quaternion.identity);

                    UIController.getHospital.PatientCard.RefreshViewOnBed(parent.bedID);

                    Timing.RunCoroutine(ReturnBedDelay(delay));
                }
            }

            IEnumerator<float> ReturnBedDelay(float delay)
            {
                yield return Timing.WaitForSeconds(delay);

                Vector2i pos = parent.destRoom.ReacquireTakenSpot(parent.spotID);
                parent.GoTo(pos, PathType.Default);
                try
                {
                    parent.anim.Play(AnimHash.Stand_Unmoving, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
                try
                {
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("GetComponent - exception: " + e.Message);
                }
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                switch ((StateNotifications)id)
                {
                    case StateNotifications.FinishedMoving:
                        this.parent.state = CharacterStatus.Healed;
                        UIController.getHospital.PatientCard.RefreshView(parent.GetComponent<HospitalCharacterInfo>());
                        UIController.getHospital.PatientCard.UpdateOtherPatients();
                        UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
                        parent.Person.State = new InBed(parent);
                        break;
                    case StateNotifications.OfficeUnAnchored:
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        parent.walkingStateManager.State = null;
                        //unAnchored = true;
                        break;
                    case StateNotifications.OfficeMoved:
                        parent.UnCoverBed();
                        break;
                    case StateNotifications.OfficeAnchored:
                        if (parent.isMovementStopped())
                        {
                            Vector2i pos = parent.destRoom.ReacquireTakenSpot(parent.spotID);
                            parent.GoTo(pos, PathType.Default);
                        }
                        parent.UnCoverBed();
                        //unAnchored = false;
                        break;
                }
            }
        }

        private class GoHomeVIP : MainState
        {
            public GoHomeVIP(HospitalPatientAI parent)
                : base(parent)
            {

            }

            public override string SaveToString()
            {
                return "GHV";
            }

            public override void OnEnter()
            {
                base.OnEnter();

                UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();

                if (parent.goHome)
                {
                    //     Debug.Log("Going home VIP");
                    parent.GoTo(HospitalAreasMapController.HospitalMap.emergency.spawnPointAmbulance, PathType.Default);
                    parent.goHome = false;
                }
                else
                {
                    parent.IsoDestroy();
                    HospitalAreasMapController.HospitalMap.emergency.ambulance.GetComponent<AmbulanceController>().DriveOutAmbulance();
                }
                if (parent.healthy)
                {
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
                    parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.cured);
                }
                else
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);

                UIController.getHospital.PatientCard.RefreshViewOnBed(parent.bedID);
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);
                if (id == (int)StateNotifications.FinishedMoving)
                {
                    parent.IsoDestroy();
                    HospitalAreasMapController.HospitalMap.emergency.ambulance.GetComponent<AmbulanceController>().DriveOutAmbulance();
                }
            }
        }

        public class GoHome : MainState
        {
            bool goOut = true;
            bool hasToGetNewPath = true;

            public GoHome(HospitalPatientAI parent, bool hasToGetNewPath = true) : base(parent)
            {
                this.hasToGetNewPath = hasToGetNewPath;
            }

            public override void OnEnter()
            {
                base.OnEnter();

                UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
                if (hasToGetNewPath)
                {
                    parent.goHome = true;
                    parent.GoTo(new Vector2i(21, 21), PathType.GoHomePath);
                }
                if (parent.healthy)
                {
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
                    parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.cured);
                    //tututu
                }
                else
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);

                UIController.getHospital.PatientCard.RefreshViewOnBed(parent.bedID);
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                switch ((StateNotifications)id)
                {
                    case StateNotifications.FinishedMoving:
                        if (hasToGetNewPath)
                        {
                            if (goOut)
                            {
                                goOut = false;
                                parent.GoTo(new Vector2i(20, 20), PathType.GoHomePath);
                                return;
                            }
                            else parent.IsoDestroy();
                        }
                        break;
                    case StateNotifications.OfficeUnAnchored:
                        parent.walkingStateManager.State = null;
                        parent.Person.State = new GoHome(parent, false);
                        break;
                    case StateNotifications.OfficeAnchored:
                        parent.walkingStateManager.State = null;
                        parent.Person.State = new GoHome(parent);
                        break;
                    default:
                        break;
                }

            }
            public override string SaveToString()
            {
                return "GH";
            }
        }

        private class GoToRoom : MainState
        {
            Vector2i pos;
            int posID;
            int destRoomBedId;

            public GoToRoom(HospitalPatientAI parent, int posID = -1, int destRoomBedId = -1)
                : base(parent)
            {
                this.posID = posID;
                this.destRoomBedId = destRoomBedId;
                parent.GetComponent<HospitalCharacterInfo>().UpdateReward();
            }

            public override string SaveToString()
            {
                return "GTR!" + posID.ToString();
            }

            public override void OnEnter()
            {
                base.OnEnter();
                parent.ChangeToPajama();
                pos = parent.destRoom.ReacquireTakenSpot(parent.spotID);
                parent.GoTo(pos, PathType.Default);
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);

                UIController.getHospital.PatientCard.RefreshViewOnBed(parent.bedID);
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                switch ((StateNotifications)id)
                {
                    case StateNotifications.FinishedMoving:
                        parent.Person.State = new InBed(parent);
                        break;
                    case StateNotifications.OfficeUnAnchored:
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);                            
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        parent.walkingStateManager.State = null;
                        break;
                    case StateNotifications.OfficeAnchored:
                        Vector2i pos = parent.destRoom.ReacquireTakenSpot(parent.spotID);
                        parent.GoTo(pos, PathType.Default);
                        break;
                    default:
                        break;
                }
            }
        }

        public class MainState : IState
        {
            protected HospitalPatientAI parent;

            public MainState(HospitalPatientAI parent)
            {
                this.parent = parent;
            }

            public virtual void Notify(int id, object parameters) { }

            public virtual void OnEnter() { }

            public virtual void OnExit() { }

            public virtual void OnUpdate() { }

            public virtual string SaveToString()
            {
                return "";
            }
        }

        private enum StateStatus
        {
            FinishedMoving,
            OfficeUnAnchored,
            OfficeAnchored
        }
        #endregion
    }
}