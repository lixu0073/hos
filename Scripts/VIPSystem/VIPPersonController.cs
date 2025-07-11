using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using MovementEffects;
using TMPro;
using System.Globalization;

namespace Hospital
{
    public class VIPPersonController : BasePatientAI, IDiagnosePatient
    {

        public static List<HospitalCharacterInfo> Patients;
        static VIPPersonController()
        {
            Patients = new List<HospitalCharacterInfo>();
        }

        public static void ResetAllVIPPatients()
        {
            if (Patients != null && Patients.Count > 0)
            {
                for (int i = 0; i < Patients.Count; i++)
                {
                    if (Patients[i] != null && Patients[i].GetComponent<BasePatientAI>() != null)
                        Patients[i].GetComponent<BasePatientAI>().IsoDestroy();
                }

                Patients.Clear();
            }
        }

        public static void ResetAllVIPPatientsDailyQuest()
        {
            if (Patients != null && Patients.Count > 0)
            {
                for (int i = 0; i < Patients.Count; i++)
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

        public bool VIPdeparted = false;
        public string definition;
        public StateManager Person;
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
        public bool goHome = false;
        public bool GetGoHome()
        {
            return goHome;
        }
        public BaseCharacterInfo GetSprites()
        {
            return sprites;
        }

        public float GetDiagnoseTime()
        {
            return diagnoseTime;
        }

        public Component GetAI()
        {

            return gameObject.GetComponent<VIPPersonController>();
        }

        public void StateToDiagRoom(DiagnosticRoom room) //tutaj to odpalenie podróży do diag roomu
        {
            //this.Person.State = new GoToDiagRoom(this, room);
            SetGoToElevatorState(room);

        }

        public bool DoneHealing()
        {
            return state == CharacterStatus.Healed;
        }

        public bool pajamaOn = false;

        public override void Initialize(string info, int timeFromSave)
        {
            var strs = info.Split('!');
            base.Initialize(Vector2i.Parse(strs[0]));
            //destTilePos = Vector2i.Parse (strs [0]);
            state = (CharacterStatus)Enum.Parse(typeof(CharacterStatus), strs[1]);

            Person = new StateManager();
            var queue = bool.Parse(strs[2]);
            queueID = int.Parse(strs[3], System.Globalization.CultureInfo.InvariantCulture);
            HospitalCharacterInfo haracterInfo = GetComponent<HospitalCharacterInfo>();
            haracterInfo.FromString(strs[4]);
            haracterInfo.VIPTime -= timeFromSave;
            haracterInfo.VIPTime = Mathf.Max(haracterInfo.VIPTime, 0);


            if (haracterInfo.VIPTime == 0 && haracterInfo.requiredMedicines.Length > 0)
            {
                //HospitalAreasMapController.Map.vipRoom.gameObject.GetComponent<VIPSystemManager>().DepartVIP(/*true, (int)haracterInfo.VIPTime*/);
                HospitalAreasMapController.HospitalMap.vipRoom.gameObject.GetComponent<VIPSystemManager>().StartCountingExtraSeconds();
                HospitalAreasMapController.HospitalMap.vipRoom.gameObject.GetComponent<VIPSystemManager>().LastVIPHealed = false;
            }

            if (queue)
                HospitalDataHolder.Instance.AddToDiagnosticQueue(this);
            if (strs.Length > 5)
            {
                switch (strs[5])
                {
                    case "GTR":
                        Person.State = new GoToRoom(this, bool.Parse(strs[6]), float.Parse(strs[7], CultureInfo.InvariantCulture));
                        break;
                    case "IB":
                        Person.State = new InBed(this);
                        break;
                    case "GTE":
                        Person.State = new GoToElevator(this, (DiagnosticRoom)HospitalAreasMapController.HospitalMap.FindRotatableObject(strs[6]));
                        break;
                    case "GTDR":
                        var p = (DiagnosticRoom)HospitalAreasMapController.HospitalMap.FindRotatableObject(strs[6]);
                        p.SetPatient(this);
                        Person.State = new GoToDiagRoom(this, p, float.Parse(strs[7], CultureInfo.InvariantCulture));
                        break;
                    case "H":
                        Person.State = new Healing(this, (DiagnosticRoom)HospitalAreasMapController.HospitalMap.FindRotatableObject(strs[7]), float.Parse(strs[6], CultureInfo.InvariantCulture));
                        break;
                    case "RTB":
                        state = CharacterStatus.Healed;
                        Person.State = new ReturnToBed(this, bool.Parse(strs[7]), float.Parse(strs[6], CultureInfo.InvariantCulture));
                        break;
                    case "GH":
                        Person.State = new GoHome(this, float.Parse(strs[6], CultureInfo.InvariantCulture));
                        break;
                    default:
                        break;
                }
            }
            var tmp = GetComponent<HospitalCharacterInfo>();
            if (!Patients.Contains(tmp) && !(Person.State is GoToRoom || Person.State is GoHome || Person.State is InBed || Person.State is GoToElevator || Person.State is GoToDiagRoom || Person.State is Healing || Person.State is ReturnToBed))
            {
                Patients.Add(tmp);
                if (tmp.RequiresDiagnosis)
                {
                    HospitalDataHolder.Instance.ReturnDiseaseRoom((int)(tmp.DisaseDiagnoseType)).ShowIndicator();
                }
            }
        }

        public override string SaveToString()
        {
            var z = GetComponent<HospitalCharacterInfo>();
            var p = HospitalDataHolder.Instance.QueueContainsPatient(this);
            return Checkers.CheckedPosition(position, z.name) + "!" + state + "!" + Checkers.CheckedBool(p) + "!" + Checkers.CheckedAmount(queueID, -1, int.MaxValue, "Queue ID: ") + "!" + z.SaveToString() + "!" + Person.State.SaveToString();
        }
        /// <summary>
        /// Stop diagnose and change person to healed state
        /// </summary>
        /// <param name="room"></param>
        public void StopDiagnose(DiagnosticRoom room)
        {
            room.StopHealingAnimation();
            if (Person.State is Healing)
            {
                transform.position = new Vector3(room.GetMachineSpot().x, 0, room.GetMachineSpot().y);
            }
            //transform.position = new Vector3(room.GetMachinePosition().x, 0, room.GetMachinePosition().y);
            GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
            state = CharacterStatus.Healed;

            room.IsHealing = false;
            room.SetIsHealingTag(false);
            HospitalDataHolder.Instance.DecreaseQueueIDs(this);
            //Achievement
            AchievementNotificationCenter.Instance.PatientDiagnosed.Invoke(new AchievementProgressEventArgs(1));

            if (Person.State is GoToElevator || Person.State is InBed || Person.State is GoToRoom)
            {
                Person.State = new GoToRoom(this, goHome);
            }
            else
            {
                Person.State = new ReturnToBed(this, goHome);
            }
        }

        public void SetStateDiagnose()
        {
            state = VIPPersonController.CharacterStatus.Diagnose;
        }

        public void SetStateInQueue()
        {
            state = VIPPersonController.CharacterStatus.InQueue;
        }

        public void AddBacteriaFromOtherPatient() { }

        public override void Initialize(Vector2i pos)
        {
            base.Initialize(pos);
            this.Person = new StateManager();
        }

        protected override void Update()
        {
            base.Update();
            Person.Update();
        }


        public void SetTutorialTeaseGreetingsState()
        {
            Person.State = new TutorialTeaseGreetings(this);
        }

        public void SetTutorialGoToHeliState()
        {
            Person.State = new TutorialTeaseGoToHeli(this);
        }

        public void SetJustArrivedState()
        {
            Person.State = new GoToRoom(this, goHome);
            SaveSynchronizer.Instance.MarkToSave(SavePriorities.VIPArrived);
            if (!HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                StartVIPCounting();
            }
        }

        public void SetInBedState()
        {
            Person.State = new InBed(this);
            if (!HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                StartVIPCounting();
            }
        }

        public void StartVIPCounting()
        {
            Timing.RunCoroutine(VIPCounting());
        }

        public void SetGoToElevatorState(DiagnosticRoom room = null)
        {
            Person.State = new GoToElevator(this, room);
        }

        public void SetGoToDiagRoomState(DiagnosticRoom room)
        {
            Person.State = new GoToDiagRoom(this, room);
        }

        public void DepartVIP(float delay)
        {
            VIPdeparted = true;
            if (Person.State is GoHome)
            {
                return;
            }
            else if (Person.State is InBed || Person.State is GoToRoom || Person.State is GoToElevator)
            {
                Person.State = new GoHome(this, delay);
            }
            else
            {
                state = CharacterStatus.Healed;
                Person.State = new ReturnToBed(this, goHome, delay);
            }
        }

        public void ChangeToPajama()
        {
            //var p = this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts;
            pajamaOn = true;
            List<Sprite> Pajama = ReferenceHolder.GetHospital().vipSpawner.GetPajama(definition);
            for (int i = 0; i < this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts.Count; ++i)
            {
                this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts[i].sprite = Pajama[i];
            }
            return;
        }

        public void GetClothesOn()
        {
            //var p = this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts;
            //List<Sprite> Clothes = ReferenceHolder.Get().vipSpawner.GetClothesOn(definition);
            pajamaOn = false;

            for (int i = 0; i < this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts.Count; ++i)
            {
                this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts[i].sprite = this.gameObject.GetComponent<HospitalCharacterInfo>().OriginalClothes[i];
            }
            return;
        }

        private void StartHealing(RotatableObject room)
        {
            ((DiagnosticRoom)room).StartHealingAnimation();
            ((DiagnosticRoom)room).IsHealing = true;
            ((DiagnosticRoom)room).SetIsHealingTag(true);
            base.SetHealingAnimation(room);
        }

        private void StopHealing(RotatableObject room)
        {
            transform.position = new Vector3(((DiagnosticRoom)room).GetMachineSpot().x, 0, ((DiagnosticRoom)room).GetMachineSpot().y);

            // this.transform.position = ((DiagnosticRoom)room).GetMachinePositionPatient();//new Vector3(((DiagnosticRoom)room).GetMachinePosition().x, 0, ((DiagnosticRoom)room).GetMachinePosition().y);
            position = ((DiagnosticRoom)room).GetMachinePosition();
            ((DiagnosticRoom)room).IsHealing = false;
            ((DiagnosticRoom)room).SetIsHealingTag(false);
        }



        public void LayInBed()
        {
            HospitalCharacterInfo info = GetComponent<HospitalCharacterInfo>();
            if (!goHome)
                GameState.Get().UpdateMedicinesNeededListWithPatient(info);

            // transform.position = new Vector3(ResourcesHolder.Get().VIPspots[1].x, transform.position.y, ResourcesHolder.Get().VIPspots[1].y - 1f);
            transform.localPosition = new Vector3(0.45f, 0.6f, -1.7f);
            try { 
                anim.Play(AnimHash.VIP_Bed_Idle, 0, 0.0f);
                anim.SetFloat("tile_X", -1);
                anim.SetFloat("tile_Y", 0);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }

        public void FreeBed()
        {
            HospitalAreasMapController.HospitalMap.vipRoom.FreeVIPBed(0);
        }
        public void DischargeBed()
        {
            HospitalAreasMapController.HospitalMap.vipRoom.DischargeVIPBed(0);
        }

        public enum CharacterStatus
        {
            Diagnose,
            InQueue,
            Healed,
            None,
            TutorialWaitForPopUpClose,
        }

        public bool IsInDiagnose()
        {
            return Person.State is Healing || Person.State is GoToDiagRoom || Person.State is GoToElevator;
        }

        public int GetTimeToEndDiagnose()
        {
            if (Person.State is Healing)
            {
                Healing healingState = (Healing)Person.State;
                return healingState.GetTimeToEndDiagnose();
            }
            if (Person.State is GoToDiagRoom)
            {
                GoToDiagRoom goToDiagRoom = (GoToDiagRoom)Person.State;
                return goToDiagRoom.GetTimeToEndDiagnose();
            }
            if (Person.State is GoToElevator)
            {
                GoToElevator goToElevator = (GoToElevator)Person.State;
                return goToElevator.GetTimeToEndDiagnose();
            }
            return -1;
        }
        /// <summary>
        /// Counting for how long VIP is in hospital. After time is up, VIP will leave hospital (and will be furious).
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> VIPCounting()
        {
            //float Time = transform.GetComponent<HospitalCharacterInfo> ().VIPTime;
            while (transform.GetComponent<HospitalCharacterInfo>().VIPTime > 0)
            {
                yield return Timing.WaitForSeconds(1);
                if (this != null)
                {
                    transform.GetComponent<HospitalCharacterInfo>().VIPTime--;
                    transform.GetComponent<HospitalCharacterInfo>().VIPTime = Math.Max(transform.GetComponent<HospitalCharacterInfo>().VIPTime, 0);
                }
                else
                {
                    yield break;
                }
            }
            if (this != null)
            {
                DepartVIP(VariableHolder.Get().BedDelay);
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.VIP.ToString(), (int)FunnelStepVip.VipNotCured, FunnelStepVip.VipNotCured.ToString());
#if MH_DEVELOP || MH_QA
                Debug.Log("VIP is in diagnosis queue. Removing them from queue.");
#endif
                if (UIController.getHospital.GetActiveHover() != null && UIController.getHospital.GetActiveHover() == SimpleUI.DiagnosticHover.GetActive())
                {
                    Debug.Log("Diagnostics hover is active. Updating it.");
                    SimpleUI.DiagnosticHover.GetActive().RefreshHoverCounter();
                }
                else
                    Debug.Log("Diagnostic hover is not active");

                //int stepID = TutorialController.Instance.GetStepId(TutorialController.Instance.CurrentTutorialStepTag);
                //if (stepID >= TutorialController.Instance.GetStepId(StepTag.Vip_Leo_Sick_1) && stepID <= TutorialController.Instance.GetStepId(StepTag.vip_speedup_2) && ReferenceHolder.GetHospital().vipSystemManager.GetSecondsToLeave() <= 0)
                //{
                //    TutorialController.Instance.SetStep(StepTag.level_10);
                //    PatientCardController pc = UIController.getHospital.PatientCard;
                //    if (pc.IsVisible)
                //    {
                //        pc.isExitBlocked = false;
                //        pc.Exit();
                //    }
                //}
            }


        }

        protected override void ReachedDestination()
        {
            base.ReachedDestination();
            if (Person.State != null)
                Person.State.Notify((int)StateStatus.FinishedMoving, null);
        }

        public override void Notify(int id, object parameters)
        {
            if ((Person.State != null) && (!goHome))
                Person.State.Notify(id, parameters);
        }

        public class GoToRoom : MainState
        {
            float delay;
            public GoToRoom(VIPPersonController parent, bool goHome, float delay = 0) : base(parent)
            {
                this.delay = delay;
                parent.goHome = goHome;

            }
            public override string SaveToString()
            {
                return "GTR!" + parent.goHome.ToString() + "!" + delay;
            }

            public override void OnEnter()
            {
                base.OnEnter();

                //this.parent.state = VIPPersonController.CharacterStatus.None;
                parent.GoTo(ResourcesHolder.GetHospital().VIPspots[1], PathType.GoHealingPath);

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);

                UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                //tutaj jeżeli cos w updacie się dzieje
            }

            /*public override void OnExit(){
                base.OnExit ();
                //tutaj co vip zrobi wychodząc ze stejdża
            }*/
            public override void Notify(int id, object parameters)
            {
                if (id == (int)StateNotifications.FinishedMoving)
                {
                    parent.anim.SetFloat("tile_Y", -1.0f);
                    if (!parent.pajamaOn)
                    {
                        try { 
                            parent.anim.SetTrigger("nude");
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }
                    else
                    {
                        try { 
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }
                    parent.walkingStateManager.State = null;
                    if (!parent.goHome)
                    {
                        Timing.RunCoroutine(GoBed());
                    }
                    else
                    {
                        parent.Person.State = new GoHome(parent, delay);
                    }
                }
            }
            IEnumerator<float> GoBed()
            {
                yield return Timing.WaitForSeconds(parent.pajamaOn ? 0 : 2);
                parent.Person.State = new InBed(parent.GetComponent<VIPPersonController>());
                parent.ChangeToPajama();
            }

        }

        public class InBed : MainState
        {
            public InBed(VIPPersonController parent) : base(parent)
            {
                HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().VIPinside = true;
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
                Hospital.HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().VIPInHeli = false;
                parent.abortPath();
                HospitalAreasMapController.HospitalMap.vipRoom.CoverVIPBed(0, parent);
                parent.transform.GetChild(1).gameObject.SetActive(false);
                parent.LayInBed();
                parent.ChangeToPajama();
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
                parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.vipbed);

                UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

                if (!HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    NotificationCenter.Instance.VIPReachedBed.Invoke(new BaseNotificationEventArgs());
                }
                if (UIController.getHospital.vIPPopUp.gameObject.activeSelf)
                {
                    UIController.getHospital.vIPPopUp.Exit();
                    HospitalAreasMapController.HospitalMap.vipRoom.OnClick();
                }
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                //tutaj jeżeli cos w updacie się dzieje
            }

            public override void OnExit()
            {
                parent.transform.GetChild(1).gameObject.SetActive(true);
                parent.transform.position = new Vector3(ResourcesHolder.GetHospital().VIPspots[1].x, parent.transform.position.y, ResourcesHolder.GetHospital().VIPspots[1].y);
                parent.position = ResourcesHolder.GetHospital().VIPspots[1];
            }

            public override void Notify(int id, object parameters)
            {
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
                    case StateNotifications.FinishedMoving:
                        try { 
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                            parent.anim.SetFloat("tile_Y", -1.0f);
                            parent.walkingStateManager.State = null;
                            parent.ChangeToPajama();
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    default:
                        break;
                }
            }

        }

        public class GoToElevator : MainState
        {

            public DiagnosticRoom room;

            public GoToElevator(VIPPersonController parent, DiagnosticRoom room) : base(parent)
            {
                this.room = room;

                Debug.Log(room == null ? "VIP Diag Room NULL" : "Vip Diag Room NOT NULL");

            }
            public override string SaveToString()
            {
                return "GTE!" + room.Tag;
            }

            public override void OnEnter()
            {
                base.OnEnter();
                parent.state = CharacterStatus.Diagnose;
                //parent.state = VIPPersonController.CharacterStatus.None;
                //Debug.LogError("Setting VIP Diagnose state 3");
                parent.ChangeToPajama();
                parent.GoTo(ResourcesHolder.GetHospital().VIPspots[2], PathType.GoHealingPath);
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                //UIController.get.PatientCard.RefreshViewOnBed(UIController.get.PatientCard.selectedBedId);//MEMEME
                //UIController.get.PatientCard.UpdateOtherPatients();//MEMEME
            }

            public int GetTimeToEndDiagnose()
            {
                if (room == null)
                {
                    return -1;
                }
                return room.DiagnosisTimeMastered - (int)parent.diagnoseTime;
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                //tutaj jeżeli cos w updacie się dzieje
            }

            /*public override void OnExit(){
			base.OnExit ();
			//tutaj co vip zrobi wychodząc ze stejdża
		}*/
            public override void Notify(int id, object parameters)
            {
                if (id == (int)StateNotifications.ForceTeleportToHospitalBed)
                {
                    parent.state = CharacterStatus.Healed;
                    UIController.getHospital.PatientCard.RefreshView(parent.GetComponent<HospitalCharacterInfo>());
                    UIController.getHospital.PatientCard.UpdateOtherPatients();
                    UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();

                    parent.Person.State = new InBed(parent);
                }
                if (id == (int)StateNotifications.FinishedMoving)
                {
                    try { 
                        parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        parent.anim.SetFloat("tile_X", 1.0f);
                        parent.anim.SetFloat("tile_Y", -1.0f);
                        parent.walkingStateManager.State = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    Timing.RunCoroutine(Teleport());
                }
            }

            IEnumerator<float> Teleport()
            {
                yield return Timing.WaitForSeconds(1);
                if (parent.state != CharacterStatus.Healed && parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis == true)
                {
                    parent.transform.position = new Vector3(ResourcesHolder.GetHospital().VIPspots[3].x, parent.transform.position.y, ResourcesHolder.GetHospital().VIPspots[3].y);
                    parent.position = ResourcesHolder.GetHospital().VIPspots[3];
                    parent.SetGoToDiagRoomState(room);
                    //parent.Person.State = new InBed (parent.GetComponent<VIPPersonController>());
                }
            }
        }

        private class GoToDiagRoom : MainState
        {
            private bool unAnchored = false;

            public DiagnosticRoom currentRoom;
            public GoToDiagRoom(VIPPersonController parent, DiagnosticRoom room, float time = 0.0f) : base(parent)
            {
                currentRoom = room;
                this.parent.diagnoseTime = time;
            }

            public override string SaveToString()
            {
                return "GTDR!" + currentRoom.Tag + "!" + parent.diagnoseTime.ToString();
            }

            public override void OnEnter()
            {
                this.parent.state = CharacterStatus.Diagnose;
                Debug.LogError("Setting VIP Diagnose state 2");
                parent.ChangeToPajama();
                UIController.getHospital.PatientCard.RefreshView(this.parent.GetComponent<HospitalCharacterInfo>());
                UIController.getHospital.PatientCard.UpdateOtherPatients();
                UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();

                parent.GoTo(currentRoom.GetMachinePosition(), PathType.GoHealingPath);

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);

                currentRoom.isTaken = true;

                //UIController.get.PatientCard.RefreshViewOnBed(UIController.get.PatientCard.selectedBedId);//MEMEME
                //UIController.get.PatientCard.UpdateOtherPatients();//MEMEME

            }
            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);
                if (id == (int)StateNotifications.ForceTeleportToHospitalBed)
                {
                    parent.diagnoseTime = ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered;
                    ((DiagnosticRoom)currentRoom).StopHealingAnimation();
                    parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
                    ((DiagnosticRoom)currentRoom).IsHealing = false;
                    ((DiagnosticRoom)currentRoom).SetIsHealingTag(false);
                    HospitalDataHolder.Instance.DecreaseQueueIDs(parent);

                    parent.state = CharacterStatus.Healed;
                    UIController.getHospital.PatientCard.RefreshView(parent.GetComponent<HospitalCharacterInfo>());
                    UIController.getHospital.PatientCard.UpdateOtherPatients();
                    UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();

                    parent.Person.State = new InBed(parent);
                }
                if (id == (int)StateNotifications.FinishedMoving)
                {
                    parent.Person.State = new Healing(parent, currentRoom);
                }

                if (id == (int)StateNotifications.OfficeUnAnchored)
                {
                    try { 
                        parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        parent.walkingStateManager.State = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    unAnchored = true;
                }
                if (id == (int)StateNotifications.OfficeAnchored)
                {
                    parent.GoTo(currentRoom.GetMachinePosition(), PathType.GoHealingPath);
                    unAnchored = false;
                }
            }

            public int GetTimeToEndDiagnose()
            {
                return ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered - (int)parent.diagnoseTime;
            }

            public override void OnUpdate()
            {
                if (parent.diagnoseTime < ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered && !unAnchored)
                {
                    parent.diagnoseTime += Time.deltaTime;
                }
                else if (!unAnchored)
                {
                    ((DiagnosticRoom)currentRoom).StopHealingAnimation();
                    parent.transform.position = new Vector3(currentRoom.GetMachinePosition().x, 0, currentRoom.GetMachinePosition().y);
                    parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
                    parent.state = CharacterStatus.Healed;
                    parent.Person.State = new ReturnToBed(parent, parent.goHome);
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
            public Healing(VIPPersonController parent, DiagnosticRoom room, float time = 0.0f) : base(parent)
            {
                currentRoom = room;
                this.parent.diagnoseTime = time;
            }

            public override string SaveToString()
            {
                return "H!" + parent.diagnoseTime.ToString() + "!" + currentRoom.Tag;
            }
            public override void OnEnter()
            {
                base.OnEnter();
                parent.StopMovement();
                parent.abortPath();
                Debug.LogError("Setting VIP Diagnose state 1");
                this.parent.state = CharacterStatus.Diagnose;
                currentRoom.isHealing = true;
                currentRoom.SetIsHealingTag(true);
                parent.ChangeToPajama();
                currentRoom.currentPatient = parent;
                parent.StartHealing(currentRoom);
                currentRoom.isTaken = true;

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
                parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.diagnosis);

                parent.AddHappyEffect(currentRoom);

                UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

            }

            public override void OnExit()
            {
                parent.StopHealing(currentRoom);
                parent.RemoveHappyEffect();
                //parent.MoveWithRoom = false;
            }

            public int GetTimeToEndDiagnose()
            {
                return ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered - (int)parent.diagnoseTime;
            }

            public override void OnUpdate()
            {
                if (parent.diagnoseTime < ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered && !unAnchored)
                {
                    parent.diagnoseTime += Time.deltaTime;
                }
                else if (!unAnchored)
                {
                    parent.diagnoseTime = ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered;
                    ((DiagnosticRoom)currentRoom).StopHealingAnimation();
                    parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
                    ((DiagnosticRoom)currentRoom).IsHealing = false;
                    ((DiagnosticRoom)currentRoom).SetIsHealingTag(false);
                    HospitalDataHolder.Instance.DecreaseQueueIDs(parent);
                    parent.state = CharacterStatus.Healed;
                    parent.transform.position = new Vector3(currentRoom.GetMachinePosition().x, 0, currentRoom.GetMachinePosition().y);
                    parent.Person.State = new ReturnToBed(parent, parent.goHome);
                }
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);
                if (id == (int)StateNotifications.ForceTeleportToHospitalBed)
                {
                    parent.diagnoseTime = ((DiagnosticRoom)currentRoom).DiagnosisTimeMastered;
                    ((DiagnosticRoom)currentRoom).StopHealingAnimation();
                    parent.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
                    ((DiagnosticRoom)currentRoom).IsHealing = false;
                    ((DiagnosticRoom)currentRoom).SetIsHealingTag(false);
                    HospitalDataHolder.Instance.DecreaseQueueIDs(parent);

                    parent.state = CharacterStatus.Healed;
                    UIController.getHospital.PatientCard.RefreshView(parent.GetComponent<HospitalCharacterInfo>());
                    UIController.getHospital.PatientCard.UpdateOtherPatients();
                    UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
                    parent.Person.State = new InBed(parent);
                }
                if (id == (int)StateNotifications.FinishedMoving)
                {
                    parent.Person.State = new InBed(parent);
                }
                if (id == (int)StateNotifications.OfficeUnAnchored)
                {
                    unAnchored = true;
                    parent.RemoveHappyEffect();
                }
                if (id == (int)StateNotifications.OfficeAnchored)
                {
                    unAnchored = false;
                    parent.AddHappyEffect(currentRoom);
                }
                if (id == (int)StateNotifications.OfficeMoved)
                {
                    parent.StartHealing(currentRoom);
                    ((DiagnosticRoom)currentRoom).StartHealingAnimation();
                }
            }

        }

        private class ReturnToBed : MainState
        {
            //int posID;
            float delay;
            public ReturnToBed(VIPPersonController parent, bool goHome, float delay = 0)
                : base(parent)
            {
                this.delay = delay;
                parent.goHome = goHome;
            }

            public override string SaveToString()
            {
                return "RTB!" + delay.ToString() + "!" + parent.goHome.ToString();
            }

            public override void OnEnter()
            {
                base.OnEnter();
                parent.ChangeToPajama();
                parent.GoTo(ResourcesHolder.GetHospital().VIPspots[3], PathType.GoHealingPath);

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);

                UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);
            }

            public override void Notify(int id, object parameters)
            {
                if (id == (int)StateNotifications.FinishedMoving)
                {
                    //GameManagerController.GetInstance().Patients.Add(parent.GetComponent<CharacterInfo>());
                    this.parent.state = CharacterStatus.Healed;
                    UIController.getHospital.PatientCard.RefreshView(parent.GetComponent<HospitalCharacterInfo>());
                    UIController.getHospital.PatientCard.UpdateOtherPatients();
                    UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();

                    parent.transform.position = new Vector3(ResourcesHolder.GetHospital().VIPspots[2].x, parent.transform.position.y, ResourcesHolder.GetHospital().VIPspots[2].y);
                    parent.position = ResourcesHolder.GetHospital().VIPspots[2];
                    //  if (parent.VIPdeparted)
                    // {
                    //    parent.Person.State = new GoHome(parent, delay);
                    //  }
                    //   else
                    //  {
                    parent.Person.State = new GoToRoom(parent, parent.goHome, delay);
                    //  }
                }

                if (id == (int)StateNotifications.OfficeUnAnchored)
                {
                    try { 
                        parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        parent.walkingStateManager.State = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                }
                if (id == (int)StateNotifications.OfficeAnchored)
                {
                    parent.GoTo(ResourcesHolder.GetHospital().VIPspots[3], PathType.GoHealingPath);
                }
            }
        }

        public class GoHome : MainState
        {
            float delay;
            public GoHome(VIPPersonController parent, float delay) : base(parent)
            {
                this.delay = delay;
            }
            public override string SaveToString()
            {
                return "GH!" + delay.ToString();
            }

            public override void OnEnter()
            {
                base.OnEnter();

                if (!HospitalAreasMapController.HospitalMap.VisitingMode && TutorialController.Instance.CurrentTutorialStepTag == StepTag.Emma_on_Sick_Leo)
                    NotificationCenter.Instance.VIPReachedBed.Invoke(new BaseNotificationEventArgs());

                parent.goHome = true;
                if (delay == 0)
                {
                    Timing.RunCoroutine(FreeBedDelay(0));
                    //parent.ShowNamePatientcured (parent.transform.GetChild (2), parent.GetComponent<HospitalCharacterInfo>()); // it is HospitalCharacterPrefab FloatingInfo

                    parent.SetRandomHurrayAnimation();
                    Timing.RunCoroutine(WalkToMedicopter(1.75f, true));
                    CreateHappyParticle();
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
                    parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.vipcured);
                    HospitalCharacterInfo hearts = parent.GetComponent<HospitalCharacterInfo>();
                    //hearts.SetHeartsActive();


                    //tututu

                }
                else
                {
                    Timing.RunCoroutine(DischargeBedDelay(0));
                    Timing.RunCoroutine(WalkToMedicopter(0, false));
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                }
                HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().VIPinside = false;

                parent.GetClothesOn();

                UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);
            }

            IEnumerator<float> FreeBedDelay(float delay)
            {
                yield return Timing.WaitForSeconds(delay);
                parent.FreeBed();
            }

            IEnumerator<float> DischargeBedDelay(float delay)
            {
                yield return Timing.WaitForSeconds(delay);
                parent.DischargeBed();
            }

            IEnumerator<float> WalkToMedicopter(float delay, bool happy)
            {
                yield return Timing.WaitForSeconds(delay);
                /*if (happy)
                {
                    Transform FanfareSpawnT = HospitalAreasMapController.Map.vipRoom.FanfareSpawnT;
                    GameObject FanfareParticles = Instantiate(ResourcesHolder.Get().ParticleBedPatientCured, FanfareSpawnT.position, Quaternion.Euler(45, 45, 0)) as GameObject;
                    Destroy(FanfareParticles, 2f);

                    GameObject floatingInfo = FanfareSpawnT.GetChild(0).gameObject;

                    if (!parent.GetComponent<HospitalCharacterInfo>().Name.Contains("_"))
                    {
                        floatingInfo.GetComponent<TextMeshPro>().text = parent.GetComponent<HospitalCharacterInfo>().Name + " " + parent.GetComponent<HospitalCharacterInfo>().Surname + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";
                    }
                    else
                    {
                        floatingInfo.GetComponent<TextMeshPro>().text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + parent.GetComponent<HospitalCharacterInfo>().Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + parent.GetComponent<HospitalCharacterInfo>().Surname) + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";
                    }
                    floatingInfo.gameObject.SetActive(false);
                    floatingInfo.gameObject.SetActive(true);

                    //UIController.get.vIPGiftBoxPopup.Open ();
                    //HospitalAreasMapController.Map.casesManager.GiftFromVIP = true;
                    //UIController.get.unboxingPopUp.OpenVIPCasePopup();
                }*/

                parent.GoTo(ResourcesHolder.GetHospital().VIPspots[0], PathType.GoHealingPath);
            }

            private void CreateHappyParticle()
            {
                Transform FanfareSpawnT = HospitalAreasMapController.HospitalMap.vipRoom.FanfareSpawnT;

                if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                {
                    GameObject FanfareParticles = Instantiate(ResourcesHolder.GetHospital().ParticleBedPatientCured, FanfareSpawnT.position, Quaternion.Euler(45, 45, 0)) as GameObject;
                    Destroy(FanfareParticles, 2f);
                }


                if (!HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    GameObject floatingInfo = FanfareSpawnT.GetChild(0).gameObject;

                    if (!parent.GetComponent<HospitalCharacterInfo>().Name.Contains("_"))
                    {
                        floatingInfo.GetComponent<TextMeshPro>().text = parent.GetComponent<HospitalCharacterInfo>().Name + " " + parent.GetComponent<HospitalCharacterInfo>().Surname + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";
                    }
                    else
                    {
                        floatingInfo.GetComponent<TextMeshPro>().text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + parent.GetComponent<HospitalCharacterInfo>().Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + parent.GetComponent<HospitalCharacterInfo>().Surname) + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";
                    }
                    floatingInfo.gameObject.SetActive(false);
                    floatingInfo.gameObject.SetActive(true);
                    floatingInfo.GetComponent<Animator>().SetTrigger("PlayFloat");
                }
            }

            public override void Notify(int id, object parameters)
            {
                if (id == (int)StateNotifications.FinishedMoving)
                {

                    parent.walkingStateManager.State = null;
                    parent.transform.parent.GetComponent<VIPSystemManager>().DepartVIP();

                }
            }
            /*public override void OnUpdate(){
                base.OnUpdate ();
                //tutaj jeżeli cos w updacie się dzieje
            }*/

            /*public override void OnExit(){
                base.OnExit ();
                //tutaj co vip zrobi wychodząc ze stejdża
            }*/
        }

        public class MainState : IState
        {
            protected VIPPersonController parent;

            public MainState(VIPPersonController parent)
            {
                this.parent = parent;
            }

            public virtual void Notify(int id, object parameters)
            {
            }

            public virtual void OnEnter()
            {
            }

            public virtual void OnExit()
            {
            }

            public virtual void OnUpdate()
            {
            }
            public virtual string SaveToString()
            {
                return "";
            }
        }

        public class TutorialTeaseGreetings : MainState
        {
            public TutorialTeaseGreetings(VIPPersonController parent) : base(parent)
            {

            }

            public override void OnEnter()
            {
                base.OnEnter();

                parent.state = CharacterStatus.None;
                parent.GoTo(ResourcesHolder.GetHospital().VIPspots[4], PathType.GoHealingPath);
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
            }

            public override void Notify(int id, object parameters)
            {
                if ((id == (int)StateNotifications.FinishedMoving) && !HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    try { 
                        parent.anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    NotificationCenter.Instance.CharacterReachedDestination.Invoke(new CharacterReachedDestinationArgs());
                }
            }

        }

        public class TutorialTeaseGoToHeli : MainState
        {
            public TutorialTeaseGoToHeli(VIPPersonController parent) : base(parent)
            {

            }
            public override void OnEnter()
            {
                parent.state = CharacterStatus.None;
                try { 
                    parent.anim.Play(AnimHash.Walk, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
                parent.GoTo(ResourcesHolder.GetHospital().VIPspots[0], PathType.GoHealingPath);
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }
            public override void OnUpdate()
            {
                base.OnUpdate();
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            public override void Notify(int id, object parameters)
            {
                if ((id == (int)StateNotifications.FinishedMoving) && !HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    try { 
                        parent.anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    //Hospital.HospitalAreasMapController.Map.vipRoom.GetComponent<VIPSystemManager>().DepartVIP(false);
                    if (!UIController.get.ActivePopUps.Exists(x => x as CasesPopUpController))
                    {
                        parent.transform.parent.GetComponent<VIPSystemManager>().DepartVIP();
                    }
                    else
                    {
                        parent.state = CharacterStatus.TutorialWaitForPopUpClose;
                    }
                }
            }
        }

        public HospitalCharacterInfo GetHospitalCharacterInfo()
        {
            return GetComponent<HospitalCharacterInfo>();
        }

        public bool IsGoHomeState()
        {
            return Person.State is VIPPersonController.GoHome;
        }

        public bool goHomeGet()
        {
            return goHome;
        }

        public bool SpeedUp(out float time, float timePassed, out bool isTaken)
        {
            time = 0;
            isTaken = false;
            if (timePassed <= 0)
            {
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
                else
                {
                    diagnoseTime = currentRoom.DiagnosisTimeMastered;
                    time = diff;
                    abortPath();
                    Person.State.Notify((int)StateNotifications.ForceTeleportToHospitalBed, null);
                    return true;
                }
            }
            if (Person.State is GoToDiagRoom || Person.State is GoToElevator || Person.State is InBed)
            {
                DiagnosticRoom currentRoom;

                if (Person.State is GoToDiagRoom)
                    currentRoom = ((GoToDiagRoom)Person.State).currentRoom;
                else if (Person.State is GoToElevator)
                    currentRoom = ((GoToElevator)Person.State).room;
                else
                    currentRoom = HospitalDataHolder.Instance.ReturnPatientDiagnosisRoom(this);

                if (timePassed >= currentRoom.DiagnosisTimeMastered)
                {
                    abortPath();
                    Person.State.Notify((int)StateNotifications.ForceTeleportToHospitalBed, null);
                    time = currentRoom.DiagnosisTimeMastered;
                    return true;
                }
                else
                {
                    time = timePassed;
                    diagnoseTime = timePassed;
                    isTaken = true;
                    abortPath();
                    Person.State = new Healing(this, currentRoom, diagnoseTime);
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// Cancel diagnose for VIP in set room.
        /// </summary>
        public void CancelDiagnose(DiagnosticRoom room)
        {
            if (room == null)
                return;
            room.StopHealingAnimation();

            transform.position = new Vector3(room.GetMachineSpot().x, 0, room.GetMachineSpot().y);
            GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
            if ((VIPPersonController)(room.currentPatient) == this)
            {
                room.IsHealing = false;
                room.SetIsHealingTag(false);
                room.currentPatient = null;
                room.isTaken = false;
            }
            //room.done = true;

            HospitalDataHolder.Instance.DecreaseQueueIDs(this);
        }


        private enum StateStatus
        {
            FinishedMoving,
            OfficeUnAnchored,
            OfficeAnchored
        }
    }
}
