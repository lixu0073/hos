using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VipTimerDebugButton : MonoBehaviour
{
    Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SetVipToLeaveIn30Sec);
    }
    private void SetVipToLeaveIn30Sec()
    {
        try
        {
            if (Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip == null)
            {
                Debug.Log("Trying to set vip timer while they are not in hospital");
                return;
            }
            var vipPatient = Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<Hospital.VIPPersonController>();
            if (vipPatient != null && vipPatient.GetHospitalCharacterInfo() != null && vipPatient.pajamaOn)
            {
                vipPatient.GetHospitalCharacterInfo().VIPTime = 30f;
                Debug.Log("Set vip timer to 30 sec");
            }
            else
            {
                Debug.Log("Trying to set vip timer while they are coming/leaving");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Debug button error: \n" + e);
        }
    }
}
