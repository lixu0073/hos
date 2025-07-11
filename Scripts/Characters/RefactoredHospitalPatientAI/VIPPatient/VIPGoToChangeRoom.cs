using UnityEngine;
using System.Collections;

using IsoEngine;

namespace Hospital{
	public class VIPGoToChangeRoom : BaseGoToChangeRoom {
		public VIPGoToChangeRoom(VIPPatientAI refactoredHospitalPatientAI, Vector2i changeRoomSpot): base(refactoredHospitalPatientAI, changeRoomSpot){
		}
	}


}
