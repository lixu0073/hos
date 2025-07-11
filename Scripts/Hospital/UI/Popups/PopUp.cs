using UnityEngine;
using System.Collections;
using SimpleUI;

namespace Hospital
{


	public class PopUp : BaseHover
	{
		protected override void Initialize()
		{
			SetScreenPointHovering(Vector2.zero);
			HospitalAreasMapController.HospitalMap.ChangeOnTouchType((x) =>
			{
				Close();
			});
			base.Initialize();
			trans.localScale = Vector2.one;
		}

	}
}
