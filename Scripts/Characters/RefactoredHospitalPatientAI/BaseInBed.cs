using UnityEngine;
using System.Collections;

namespace Hospital
{
	public class BaseInBed : BasePatientState
	{
		public BaseInBed(RefactoredHospitalPatientAI refactoredHospitalPatientAI): base(refactoredHospitalPatientAI){
			
		}
		#region mainMethods
		public override void OnEnter(){
			base.OnEnter ();
			patient.LayInBed();
			HospitalCharacterInfo tmp = patient.GetComponent<HospitalCharacterInfo> ();
            if (!RefactoredHospitalPatientAI.Patients.Contains(tmp))
            {
                RefactoredHospitalPatientAI.Patients.Add(tmp);
            }

			UIController.getHospital.PatientCard.UpdateOtherPatients();
			UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
            
            patient.transform.GetChild(1).gameObject.SetActive(false);
			patient.ChangeToPajama();
		
		}
        
		public override void OnExit (){
			patient.transform.GetChild(1).gameObject.SetActive(true);
			patient.GetOutOfBed();
		}

		public override void Notify (int id, object parameters){
			base.Notify(id, parameters);
		}

		public override string SaveToString (){
			return "IB";
		}
		#endregion
		#region transitionMethods
		public override void ToInBedState (){
			Debug.Log ("Can't transition to same state");
		}
		#endregion
	}
}