using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{
	public class BaseGoToRoom : BasePatientState
	{


		public BaseGoToRoom(RefactoredHospitalPatientAI refactoredHospitalPatientAI): base(refactoredHospitalPatientAI){

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

				ToInBedState ();
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
			return "GTR";
		}
		#endregion
		#region transitionMethods
		public override void ToGoToRoomState(){
			Debug.Log ("Can't transition to same state");
		}
		#endregion
	}
}