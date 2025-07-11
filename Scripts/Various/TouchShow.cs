using UnityEngine;
using System.Collections;

public class TouchShow : MonoBehaviour
{

	public RectTransform thing;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			thing.gameObject.SetActive(true);
		}
		if (Input.GetMouseButtonUp(0))
		{

			thing.gameObject.SetActive(false);
			return;
		}
		thing.position = Input.mousePosition;

	}
}
