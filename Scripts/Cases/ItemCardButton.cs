using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Hospital;

public class ItemCardButton : MonoBehaviour {

	void OnEnable(){
		GetComponent<Button> ().onClick.RemoveAllListeners();
		GetComponent<Button> ().onClick.AddListener (() => {CardClicked (); RemoveListenerOnClick();});
	}

	public void RemoveListenerOnClick(){
		GetComponent<Button> ().onClick.RemoveAllListeners();
	}

	private void CardClicked(){
		HospitalAreasMapController.HospitalMap.casesManager.CardClicked();
	}

}
