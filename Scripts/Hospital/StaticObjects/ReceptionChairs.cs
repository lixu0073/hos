using UnityEngine;


namespace Hospital
{
	
	public class ReceptionChairs : SuperObject
	{
		public override void OnClick()
		{
			Debug.Log ("Open new sickness popup");
			StartCoroutine(UIController.getHospital.PatientZeroPopUp.Open());
		}

		public override void IsoDestroy()
		{
			Destroy(gameObject);
		}
	}
}
