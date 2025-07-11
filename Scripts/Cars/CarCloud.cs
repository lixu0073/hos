using UnityEngine;
using System.Collections;

public class CarCloud : MonoBehaviour, ICarCloud {
	public void Show(){
	
	}

	public void Hide(){
		gameObject.SetActive (false);
	}
}
