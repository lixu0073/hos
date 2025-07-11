using UnityEngine;
using System.Collections;

namespace Hospital
{
	public class BaseHealing : BasePatientState
	{
		private bool unAnchored = false;

		public BaseHealing(RefactoredHospitalPatientAI refactoredHospitalPatientAI): base(refactoredHospitalPatientAI){
			//patient.waitTime = time; -- trzeba to gdzieś w kodzie dożucić może przy wczytaniu czy coś
		}
		#region mainMethods
		public override void OnEnter()
		{
			base.OnEnter ();
			patient.StartHealing (patient.currentRoom);
		}

		public override void OnUpdate(){
			if (patient.waitTime < ((DiagnosticRoom)(patient.currentRoom)).DiagnosisTimeMastered && !unAnchored)
			{
				patient.waitTime += Time.deltaTime;
			}
			else if (!unAnchored)
			{
				((DiagnosticRoom)(patient.currentRoom)).StopHealingAnimation();
				patient.transform.position = new Vector3(patient.currentRoom.GetMachinePosition().x, 0, patient.currentRoom.GetMachinePosition().y);
				patient.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis = false;
				ToReturnToBedState ();
				((DiagnosticRoom)(patient.currentRoom)).IsHealing = false;
				((DiagnosticRoom)(patient.currentRoom)).SetIsHealingTag (false);
			}
		}

		public override void OnExit (){
			patient.StopHealing(patient.currentRoom);
		}

		public override void Notify (int id, object parameters){
			base.Notify(id, parameters);
			if (id == (int)StateNotifications.FinishedMoving)
			{
				ToInBedState ();
			}

			if (id == (int)StateNotifications.OfficeUnAnchored)
			{
				unAnchored = true;
			}

			if (id == (int)StateNotifications.OfficeAnchored)
			{
				unAnchored = false;
			}

			if (id == (int)StateNotifications.OfficeMoved)
			{
				patient.StartHealing(patient.currentRoom);
				((DiagnosticRoom)(patient.currentRoom)).StartHealingAnimation();
			}
		}

		public override string SaveToString (){
			return "H!" + patient.waitTime.ToString () + "!" + patient.currentRoom.Tag;
		}
		#endregion
		#region transitionMethods
		public override void ToHealingState(){
			Debug.Log ("Can't transition to same state");
		}
		#endregion

	}
}