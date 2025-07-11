using UnityEngine;
using System.Collections;

public class CaseController : MonoBehaviour {
	private Vector3 firstMousePos;
	private float touchTime;

	void Clicked(){
		Debug.Log("CaseClicked");
		gameObject.SetActive(false);
	}

	void OnMouseDown()
	{
		touchTime = Time.time;
		firstMousePos = Input.mousePosition;
	}

	void OnMouseUp(){
		if (!IsoEngine.BaseCameraController.IsPointerOverInterface())
		if ((Input.mousePosition - firstMousePos).magnitude < 10.0f && Time.time - touchTime < 0.5f)
		{
			Clicked();
		}
	}
}
