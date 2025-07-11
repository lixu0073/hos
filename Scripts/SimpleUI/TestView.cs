using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using MovementEffects;
using Facebook;
using Facebook.Unity;


public class TestView : MonoBehaviour {


	public TextMeshProUGUI timeFromSave;
	public TextMeshProUGUI currentTime;
	public TextMeshProUGUI lastSaveTime;
	public TextMeshProUGUI lastPauseTime;
	public TextMeshProUGUI CoinAmount;
	public TextMeshProUGUI DiamondAmount;
	public TextMeshProUGUI PatientsCured;
    public TextMeshProUGUI email;
    public TextMeshProUGUI zoom;
    public TextMeshProUGUI versionText;

    public string Version;

    public void Start(){
		Timing.RunCoroutine (UpdateCurrentTime());
        versionText.text = Version;
	}

    #region saveStats

    void Update()
    {
        zoom.text = "Zoom: " + ReferenceHolder.Get().engine.MainCamera.GetCamera().orthographicSize;
    }
    
    public void GetUserEmail()
    {
        string query = "/me?fields=email";
        FB.API(query, HttpMethod.GET, (result) =>
        {
            if (result.Error == null)
            {
                email.text = result.ResultDictionary["email"].ToString();
            }
            else
            {
                email.text = "Failed to get email";
            }
        });
    }

    public void SetTimeFromSaveTime(TimePassedObject timeObject){
		timeFromSave.SetText ("Time from hospital save : " + UIController.GetFormattedShortTime(timeObject.GetTimePassed()));
	}

	public void SetCurrentTime(){
		currentTime.SetText ("Current time: " + DateTime.UtcNow.ToString());
	}

	public void SetLastSaveTime(){
		lastSaveTime.SetText ("Last Save Time: " + DateTime.UtcNow.ToString());
	}

	public void SetLastPauseTime(){
		lastPauseTime.SetText ("Last Pause Time: " + DateTime.UtcNow.ToString());
	}

	public void SetSaveStatus(bool saveSuccesfull){
		if (saveSuccesfull) {
			lastSaveTime.color = Color.white;
		} else {
			lastSaveTime.color = Color.red;
		}
	}

	#endregion

	#region resources

	public void SetCoinAmount(){
		CoinAmount.SetText ("C: " + Game.Instance.gameState().GetCoinAmount().ToString());
	}

	public void SetDiamondAmount(){
		DiamondAmount.SetText ("D: " + Game.Instance.gameState().GetDiamondAmount().ToString());
	}

	public void SetCounterStatus(ResourceType type){
		//switch (type) {
		//case ResourceType.Coin:
		//	CoinAmount.color = CheckCounterColor (Game.Instance.gameState().GetCoinAmount(), UIController.get.coinCounter.GetValue());
		//	break;
		//case ResourceType.Diamonds:
		//	DiamondAmount.color = CheckCounterColor (Game.Instance.gameState().GetDiamondAmount(), UIController.get.diamondCounter.GetValue());
		//	break;
		//}
	}

	private Color CheckCounterColor(int GSAmount, int UIAmount){
		Color color = Color.white;
		if(GSAmount < UIAmount){
			color = Color.red;
		} else if(GSAmount > UIAmount){
			color = Color.blue;
		}
		return color;
	}

    #endregion

    #region tempCounters
    public void SetPatientsCuredCount()
    {
        //PatientsCured.SetText("PatientsCured: " + Game.Instance.gameState().GetPatientsCount().PatientsCuredCount.ToString());
    }
    #endregion

    IEnumerator<float> UpdateCurrentTime(){
		for(;;){
			yield return Timing.WaitForSeconds (1f);
			SetCurrentTime ();
		}
	}
}
