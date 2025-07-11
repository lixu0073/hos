using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Hospital;

public class OpenBoxButton : MonoBehaviour {

	void OnEnable(){
		GetComponent<Button> ().onClick.RemoveAllListeners();
		GetComponent<Button> ().onClick.AddListener (() => {OpenBox (); RemoveListenerOnClick();});
	}

	public void RemoveListenerOnClick(){
		GetComponent<Button> ().onClick.RemoveAllListeners();
	}

	private void OpenBox(){
		HospitalAreasMapController.HospitalMap.casesManager.OpenBox ();
	}

}
