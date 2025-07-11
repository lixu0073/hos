using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class UIElementController : MonoBehaviour
{
	private bool visible = false;

	public void ToggleVisible()
	{
		visible = !visible;
		if (visible)
			gameObject.SetActive(true);

		if(!visible)
			GetComponent<Animator>().SetTrigger("VisibleTrigger");
	}

	public void ToggleActive()
	{
		if(!visible)
		{
			gameObject.SetActive(false);
		}
	}
}
