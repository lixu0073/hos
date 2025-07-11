using UnityEngine;
using System.Collections;

using IsoEngine;

namespace Hospital
{
	public class BaseGoHome : BasePatientState
	{
		private readonly Vector2i exitSpot;

		public BaseGoHome(RefactoredHospitalPatientAI refactoredHospitalPatientAI, Vector2i exitSpot): base(refactoredHospitalPatientAI){
			this.exitSpot = exitSpot;
		}
		#region mainMethods
		public override void OnEnter()
		{
			base.OnEnter ();
            if (patient.goingHome)
			{
                patient.GoTo(exitSpot, PathType.Default);
				patient.goingHome = false;
			}
			else
			{
				patient.IsoDestroy();
			}

		}
		/*public override void OnUpdate(){

		}

		public override void OnExit (){

		}*/

		public override void Notify (int id, object parameters){
			base.Notify(id, parameters);
			if (id == (int)StateNotifications.FinishedMoving)
			{
				patient.IsoDestroy();
			}
		}

		public override string SaveToString (){
			return "GH";
		}
		#endregion
		#region transitionMethods
		public override void ToGoHomeState(){
			Debug.Log ("Can't transition to same state");
		}
		#endregion
	}
}
