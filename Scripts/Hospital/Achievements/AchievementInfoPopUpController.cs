using UnityEngine;
using System.Collections;
using TMPro;

public class AchievementInfoPopUpController : MonoBehaviour {

	[SerializeField] private TextMeshProUGUI achievementInfo = null;

	public void Open(string info){
        if (Game.Instance.gameState().GetHospitalLevel() < 3) {
            return;
        }
        transform.SetAsLastSibling();
		gameObject.SetActive (true);
		achievementInfo.SetText (info);
        transform.SetAsLastSibling();
	}

	public void HideIt(){   //called from animation?
        transform.SetAsFirstSibling();
		gameObject.SetActive (false);
	}
}
