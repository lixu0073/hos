using UnityEngine;
using System.Collections;


namespace Hospital
{
	public abstract class BasePatientState : IHospitalPatientState
	{

		//protected readonly RefactoredHospitalPatientAI patient;
		protected RefactoredHospitalPatientAI patient;

		public BasePatientState(RefactoredHospitalPatientAI refactoredHospitalPatientAI){
			patient = refactoredHospitalPatientAI;
		}


		#region mainMethods
		public virtual void OnEnter(){

		}

		public virtual void OnUpdate(){
			
		}

		public virtual void OnExit (){
			
		}

		public virtual void Notify (int id, object parameters){
			
		}

		public virtual string SaveToString (){
			return "";
		}
		#endregion

		#region transitionMethods
		public virtual void ToGoToChangeRoomState (){
			patient.stateManager.State = patient.goToChangeRoom;
		}

		public virtual void ToInBedState (){
			patient.stateManager.State = patient.inBed;
		}

		public virtual void ToGoToDiagRoomState(){
			patient.stateManager.State = patient.goToDiagRoom;
		}

		public virtual void ToHealingState(){
			patient.stateManager.State = patient.healing;
		}

		public virtual void ToReturnToBedState(){
			patient.stateManager.State = patient.returnToBed;
		}

		public virtual void ToGoHomeState(){
			patient.stateManager.State = patient.goHome;
		}

		public virtual void ToGoToRoomState(){
			patient.stateManager.State = patient.goToRoom;
		}
		#endregion



	}
}
