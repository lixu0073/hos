using UnityEngine;
using System.Collections.Generic;
using IsoEngine;
using SimpleUI;
using System;

namespace Hospital
{
	public class PlantField : RotatableWithoutBuilding/*, ISeedable, ICollectable, IHoverable*/
	{
		protected override void OnClickWorking()
		{
			Debug.Log("PlantField OnClickWorking()");
			/*if (selection != null)
				selection.SetActive(true);
			ReferenceHolder.Get().engine.MainCamera.SmoothZoom(ReferenceHolder.Get().engine.MainCamera.MinZoom + 0.5f, 0.5f, false);
			SoundsController.Instance.PlayProbeTableSelect();
			//ShopRoomInfo shopInfo = (ShopRoomInfo)info.infos;
			//NotificationCenter.Instance.SheetRemove.Invoke(new SheetRemoveEventArgs(shopInfo.ShopTitle));
			base.OnClickWorking();
			//	Debug.Log(GetActualGameObject().name);
			GetActualGameObject().GetComponent<Animator>().SetTrigger("Click");
			ShowHover();*/
		}
	}
}
