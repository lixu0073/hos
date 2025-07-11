using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using IsoEngine;

namespace Hospital
{
	public abstract class RefactoredHospitalPatientAI : BasePatientAI
	{
		[HideInInspector] public static List<HospitalCharacterInfo> Patients;
		static RefactoredHospitalPatientAI(){
			Patients = new List<HospitalCharacterInfo> ();
		}

		[HideInInspector] public float waitTime;
		[HideInInspector] public CharacterStatus state = CharacterStatus.None;
		[HideInInspector] public enum CharacterStatus
		{
			Diagnose,
			InQueue,
			Healed,
			None
		}




		[HideInInspector] public RefactoredStateManager stateManager;
		 
		[HideInInspector] public IHospitalPatientState goToChangeRoom;
		[HideInInspector] public IHospitalPatientState inBed;
		[HideInInspector] public IHospitalPatientState goToDiagRoom;
		[HideInInspector] public IHospitalPatientState healing;
		[HideInInspector] public IHospitalPatientState returnToBed;
		[HideInInspector] public IHospitalPatientState goHome;
		[HideInInspector] public IHospitalPatientState goToRoom;

		[HideInInspector] public DiagnosticRoom currentRoom;
		[HideInInspector] public bool goingHome = false;
		[HideInInspector] public bool cured = false;

		public virtual void Awake (){
			
		}
		void Start(){
			stateManager.State = goToChangeRoom;
		}

		protected override void Update()
		{
			base.Update();
			stateManager.Update();
		}

		public virtual void Initialize(Vector2i pos, bool spawned){
			base.Initialize (pos);
			stateManager = new RefactoredStateManager ();
			/*{goToChangeRoom = new BaseGoToChangeRoom(this, HospitalDataHolder.GetInstance().Emergency.entrance);
			inBed = new BaseInBed(this);
			goToDiagRoom = new BaseGoToDiagRoom(this);
			healing = new BaseHealing(this);
			returnToBed = new BaseReturnToBed(this);
			goHome = new BaseGoHome(this, HospitalDataHolder.GetInstance().Emergency.entrance);
			goToRoom = new BaseGOToRoom(this);
		}*/
			if (!spawned) {
				stateManager.State = goToChangeRoom;
			} else {
				ToBed ();
			}
		}

		public override string SaveToString (){
			return base.SaveToString ();
		}

		public override void IsoDestroy()
		{
			if (this == null)
				return;
			var p = GetComponent<HospitalCharacterInfo>();
			Patients.Remove(p);
			UIController.getHospital.PatientCard.RemovePatient(p, true);
			HospitalDataHolder.Instance.RemovePatientFromQueues(this);
			base.IsoDestroy();
		}

		public virtual void Initialize (string info){
			
		}

		public override void Initialize (RotatableObject room, string info){
			base.Initialize();
		}

		public void TeleportToSpot(HospitalRoom hospRoom = null, DiagnosticRoom diagRoom = null, bool val = false)
		{
			if (!val)
			{
				if (hospRoom != null)
				{
					LayInBed();
				}
				if (diagRoom != null)
				{
					var p = diagRoom.GetMachineObject().transform.GetChild(1).transform.position;
					TeleportTo(p);
				}
			}
		}

		public bool isEscapeFromRoom()
		{
			if (stateManager.State is BaseGoToChangeRoom || stateManager.State is BaseGoToDiagRoom)
			{
				Debug.LogWarning(" Go home or go diag room now");
				return true;
			}
			return false;
		}

		public override void Initialize(Vector2i pos)
		{
			base.Initialize(pos);
			stateManager = new RefactoredStateManager();
		}

		public abstract void ChangeToPajama (); 

		public void ChangeToOriginal(){
			for (int i = 0; i < this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts.Count; ++i)
			{
				this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts[i].sprite = this.gameObject.GetComponent<HospitalCharacterInfo>().OriginalClothes[i];
			}
			return;
		}

		public virtual void GoChange(bool goingHome)
		{
			this.goingHome = goingHome;
			stateManager.State.ToGoToChangeRoomState ();
		}

		public void GoHomeSweetHome()
		{
			stateManager.State.ToGoHomeState();

		}

		public virtual void ToBed(){
			ChangeToPajama ();
			stateManager.State = inBed;
		}

		public virtual void LayInBed(){
			//GameState.Get().UpdateMedicinesNeededListWithAllPatients();
		}

		public abstract void GetOutOfBed ();

		public abstract void UnCoverBed ();

		public void StartHealing(RotatableObject room)
		{
			((DiagnosticRoom)room).StartHealingAnimation();
			((DiagnosticRoom)room).IsHealing = true;
			((DiagnosticRoom)room).SetIsHealingTag (true);

            ((DiagnosticRoom)room).UpdateNurseRotation();
			base.SetHealingAnimation(room);
		}

		public void StopHealing(RotatableObject room)
		{
			transform.position = new Vector3(((DiagnosticRoom)room).GetMachineSpot().x, 0, ((DiagnosticRoom)room).GetMachineSpot().y);
			position = ((DiagnosticRoom)room).GetMachinePosition();
			((DiagnosticRoom)room).IsHealing = false;
			((DiagnosticRoom)room).SetIsHealingTag (false);
            ((DiagnosticRoom)room).UpdateNurseRotation();
        }

		public void StopDiagnose(DiagnosticRoom room)
		{
            room.UpdateNurseRotation();
            room.StopHealingAnimation();
			transform.position = new Vector3(room.GetMachineSpot().x, 0, room.GetMachineSpot().y);
			GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
			stateManager.State.ToReturnToBedState ();
			room.IsHealing = false;
			room.SetIsHealingTag (false);
            ((DiagnosticRoom)room).UpdateNurseRotation();
            //Achievement
            AchievementNotificationCenter.Instance.PatientDiagnosed.Invoke(new AchievementProgressEventArgs(1));
		}

		public bool DoneHealing()
		{
			return (stateManager.State is BaseReturnToBed);
		}

		public bool GoingToDiag()
		{

			return (stateManager.State is BaseGoToDiagRoom || stateManager.State is BaseHealing);
		}

		public bool GoingToRoom()
		{
			return (stateManager.State is BaseGoToRoom || stateManager.State is BaseInBed || stateManager.State is BaseReturnToBed);
		}

		public int GetPersonState()
		{
			if (stateManager.State is BaseGoToRoom) return 0;
			else if (stateManager.State is BaseInBed) return 1;
			else if (stateManager.State is BaseReturnToBed) return 2;
			else if (stateManager.State is BaseGoToDiagRoom) return 3;
			else if (stateManager.State is BaseHealing) return 4;
			else return -1;
		}

		public void StateToDiagRoom(DiagnosticRoom room)
		{
			stateManager.State.ToGoToDiagRoomState();
		}

		protected override void ReachedDestination()
		{
			base.ReachedDestination();
			if (stateManager.State != null)
				stateManager.State.Notify((int)StateStatus.FinishedMoving, null);
		}

		public void ShowParticles(){
            if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                Instantiate(ResourcesHolder.GetHospital().ParticleCure, transform.position, Quaternion.identity);
		}

		private enum StateStatus
		{
			FinishedMoving,
			OfficeUnAnchored,
			OfficeAnchored
		}
	}
}