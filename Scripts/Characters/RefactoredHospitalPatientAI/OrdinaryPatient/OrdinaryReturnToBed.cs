using UnityEngine;
using System.Collections;

namespace Hospital{
	public class OrdinaryReturnToBed : BaseReturnToBed {
		public OrdinaryReturnToBed(OrdinaryPatientAI refactoredHospitalPatientAI): base(refactoredHospitalPatientAI){
			//patient.waitTime = time; -- trzeba to gdzieś w kodzie dożucić może przy wczytaniu czy coś
		}
}
}