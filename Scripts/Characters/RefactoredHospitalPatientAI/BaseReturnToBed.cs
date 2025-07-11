using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{
	public class BaseReturnToBed : BasePatientState
	{


		public BaseReturnToBed(RefactoredHospitalPatientAI refactoredHospitalPatientAI): base(refactoredHospitalPatientAI){
			//patient.waitTime = time; -- trzeba to gdzieś w kodzie dożucić może przy wczytaniu czy coś
		}
		#region mainMethods
		public override void OnEnter()
		{
			base.OnEnter ();

		}
		/*public override void OnUpdate(){

		}

		public override void OnExit (){

		}*/

		public override void Notify (int id, object parameters){
			if (id == (int)StateNotifications.FinishedMoving)
			{
				patient.state = RefactoredHospitalPatientAI.CharacterStatus.Healed;
				UIController.getHospital.PatientCard.RefreshView(patient.GetComponent<HospitalCharacterInfo>());
				UIController.getHospital.PatientCard.UpdateOtherPatients();
                UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();

                ToInBedState();
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

		}

		public override string SaveToString (){
			return "RTB";
		}
		#endregion
		#region transitionMethods
		public override void ToReturnToBedState(){
			Debug.Log ("Can't transition to same state");
		}
		#endregion

	}
}
