using UnityEngine;
using System.Collections.Generic;
using MovementEffects;
using IsoEngine;
using System;

namespace Hospital
{
	public abstract class BaseGoToChangeRoom : BasePatientState
	{
#pragma warning disable 0649
        private readonly Vector2i changeRoomSpot;
#pragma warning restore 0649
        public BaseGoToChangeRoom(RefactoredHospitalPatientAI refactoredHospitalPatientAI, Vector2i changeRoomSpot): base(refactoredHospitalPatientAI) { }

		#region mainMethods
		public override void OnEnter()
        {
			base.OnEnter();
			patient.ChangeToOriginal();

			if (patient.cured)
            {
                try
                {
				    patient.anim.Play(AnimHash.Hurray, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }

                patient.ShowParticles ();
				Timing.RunCoroutine(GoHome(1.75f));
			} 
            else
				Timing.RunCoroutine(GoHome(0f));			
		}

		public override void Notify (int id, object parameters)
        {
			if (id == (int)StateNotifications.FinishedMoving)
			{
				if (patient.goingHome)
				{
					patient.ChangeToOriginal();
					ToGoHomeState ();
				}
				else
				{
					patient.ChangeToPajama();
					ToGoToRoomState ();
				}
			}
		}

		public override string SaveToString()
        {
			return "GTCR";
		}
		#endregion

		#region transitionMethods
		public override void ToGoToChangeRoomState()
        {
			Debug.Log("Can't transition to same state");
		}
		#endregion

		#region coroutines
		IEnumerator<float> GoHome(float delay)
		{
			yield return Timing.WaitForSeconds(delay);
			patient.GoTo(changeRoomSpot, PathType.GoHomePath);
		}
		#endregion
	}
}