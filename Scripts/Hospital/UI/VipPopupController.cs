using UnityEngine;
using System.Collections;
using SimpleUI;
namespace Hospital
{
	public class VipPopupController : BaseHover
	{
		protected override void Initialize()
		{
			base.Initialize();
			SetScreenPointHovering(Vector2.zero);
		}
		private static VipPopupController hover;

		public static VipPopupController Open()
		{
			if (hover == null)
			{
				var p = GameObject.Instantiate(ResourcesHolder.GetHospital().VipPopupPrefab);
				hover = p.GetComponent<VipPopupController>();
				hover.Init();
				hover.transform.localScale = Vector2.one;
			}
			HospitalAreasMapController.HospitalMap.ChangeOnTouchType((x) =>
			{
				if (hover != null)
					hover.Close();
			});
            hover.Close();
			hover.Initialize();
			return hover;
		}

		public static VipPopupController GetActive()
		{
			return hover.gameObject.activeSelf ? hover : null;
		}
	}
}