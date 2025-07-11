using UnityEngine;
using System.Collections;

using IsoEngine;

namespace Hospital
{
	public class VIPGoHome : BaseGoHome
	{
		public VIPGoHome(VIPPatientAI ordinaryPatientAI, Vector2i exitSpot):base(ordinaryPatientAI, exitSpot){
		
		}
	}
}