using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using IsoEngine;
using SimpleUI;

namespace Hospital
{
	public class CaseClicker : MonoBehaviour, IPointerClickHandler {

		public void OnPointerClick(PointerEventData pointerEvent){
			if (!AreaMapController.Map.VisitingMode){
				NotificationCenter.Instance.PackageClicked.Invoke (new BaseNotificationEventArgs ());
                AreaMapController.Map.casesManager.OpenUnboxingPopUp ();
			}
        }
	}
}