using UnityEngine;
using System.Collections;

using IsoEngine;

namespace Hospital{
	public class OrdinaryGoToChangeRoom : BaseGoToChangeRoom {
		public OrdinaryGoToChangeRoom(OrdinaryPatientAI ordinaryHospitalPatientAI, Vector2i changeRoomSpot): base(ordinaryHospitalPatientAI, changeRoomSpot){
			
		}
	

	#region mainMethods
	public override void OnEnter(){
			if (patient.goingHome) {
			//	patient.

			}

			base.OnEnter ();

	}
	#endregion
	}
}
