using UnityEngine;
using System.Collections;

namespace Hospital{
	public class VIPReturnToBed : BaseReturnToBed {
		public VIPReturnToBed(VIPPatientAI refactoredHospitalPatientAI): base(refactoredHospitalPatientAI){
			//patient.waitTime = time; -- trzeba to gdzieś w kodzie dożucić może przy wczytaniu czy coś
		}
}
}