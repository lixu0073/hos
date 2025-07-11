using UnityEngine;
using System.Collections;

using IsoEngine;

namespace Hospital
{
	public class OrdinaryGoHome : BaseGoHome
	{
		public OrdinaryGoHome(OrdinaryPatientAI ordinaryPatientAI, Vector2i exitSpot):base(ordinaryPatientAI, exitSpot){
		
		}
	}
}