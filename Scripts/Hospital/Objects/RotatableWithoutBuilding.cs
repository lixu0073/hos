using UnityEngine;
using System.Collections;

namespace Hospital
{
	public class RotatableWithoutBuilding : RotatableObject
	{

		protected override void AddToMap()
		{
			if (state != State.working)
				state = State.working;
			base.AddToMap();
		}
		public override void StartBuilding()
		{
		}
	}
}