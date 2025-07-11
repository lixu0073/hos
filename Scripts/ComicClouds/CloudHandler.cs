using UnityEngine;
using System.Collections;

public class CloudHandler : MonoBehaviour {
	private IPersonCloudController cloudController;

	void Awake(){
		cloudController = transform.parent.parent.GetComponent<IPersonCloudController> ();
	}

	public void CloudColapsed(){
		cloudController.CloudColapsed ();
	}
}
