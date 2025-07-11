using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace SimpleUI{
[RequireComponent (typeof(RectTransform))]
	public class CustomBarController : MonoBehaviour{
	[SerializeField] Transform currentBar = null;
	[SerializeField] Transform additionalBar = null;

	private float f_barWidth;

	public void SetBar (float f_current, float f_additional)
	{
		f_barWidth = transform.GetComponent <RectTransform>().sizeDelta.x;
		if (currentBar.GetComponent<LayoutElement> () != null)
			currentBar.GetComponent<LayoutElement> ().preferredWidth = f_current * f_barWidth;
		
		if (additionalBar.GetComponent<LayoutElement> () != null)
			additionalBar.GetComponent<LayoutElement> ().preferredWidth = f_additional * f_barWidth;
	}

}
}