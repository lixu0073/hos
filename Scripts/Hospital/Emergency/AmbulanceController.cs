using UnityEngine;
using System.Collections;

public class AmbulanceController : MonoBehaviour
{

	private static Vector3 resetPosition = new Vector3(19.6f, 13.61f, 0);

	public void SpawnAmbulance()
	{
		gameObject.transform.localPosition = resetPosition;
		gameObject.SetActive(true);
        //	GetComponent<Animator>().Play(AnimHash.AmbulanceDriveIn,0,0.0f);
    }

    public void ReleaseVIP()
	{
		ReferenceHolder.GetHospital().HospitalSpawner.SpawnAmbulanceVIP();
	}

	public void DriveOutAmbulance()
	{
        //	GetComponent<Animator>().Play(AnimHash.AmbulanceDriveOut,0,0.0f);
    }

    public void DespawnAmbulance()
	{
		gameObject.SetActive(false);
	}
}
