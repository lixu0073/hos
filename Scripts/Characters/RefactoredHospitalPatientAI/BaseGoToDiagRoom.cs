using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{
	public class BaseGoToDiagRoom : BasePatientState
	{
		

		public BaseGoToDiagRoom(RefactoredHospitalPatientAI refactoredHospitalPatientAI): base(refactoredHospitalPatientAI){
		
		}
		#region mainMethods
		public override void OnEnter()
		{
			base.OnEnter ();
			patient.UnCoverBed ();
			patient.state = RefactoredHospitalPatientAI.CharacterStatus.Diagnose;
			UIController.getHospital.PatientCard.RefreshView(patient.GetComponent<HospitalCharacterInfo>());
            UIController.getHospital.PatientCard.UpdateOtherPatients();
            UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
            patient.GoTo (patient.currentRoom.GetMachinePosition(), PathType.Default);

		}
		/*public override void OnUpdate(){

		}

		public override void OnExit (){

		}*/

		public override void Notify (int id, object parameters){
			base.Notify(id, parameters);
			if (id == (int)StateNotifications.FinishedMoving)
			{
				ToHealingState ();
			}

			if (id == (int)StateNotifications.OfficeUnAnchored)
			{
                try { 
				    patient.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                    patient.walkingStateManager.State = null;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
        }
			if (id == (int)StateNotifications.OfficeAnchored)
			{
				patient.GoTo(patient.currentRoom.GetMachinePosition(), PathType.Default);
			}
		}

		public override string SaveToString (){
			return "GTDR";
		}
		#endregion
		#region transitionMethods
		public override void ToGoToDiagRoomState(){
			Debug.Log ("Can't transition to same state");
		}
		#endregion
	}
}