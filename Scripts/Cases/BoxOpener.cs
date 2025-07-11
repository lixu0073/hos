using UnityEngine;
using System.Collections;
using Hospital;

public class BoxOpener : MonoBehaviour {

	public void ShowCards(){
        HospitalAreasMapController.HospitalMap.casesManager.ChooseCardType ();
	}

	public void PlaySound() {
		SoundsController.Instance.PlayReward ();
	}
}
