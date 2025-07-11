using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SimpleUI
{
	public class Tooltip : MonoBehaviour
	{
		public RectTransform rt;
		public int XDistance = 10;
		public int YDistance = 10;
		//TooltipInfo actualTooltip;
		private Vector3 position;

		protected virtual void Initialize()
		{
			//tekst = GetComponentInChildren<Text>();
			rt = GetComponent<RectTransform>();
			XDistance = Mathf.Abs(XDistance);
			YDistance = Mathf.Abs(YDistance);
			position = new Vector3(XDistance, YDistance, 0);
		}
		protected virtual void Update()
		{
			if (!Input.GetMouseButton(0))
				Close();
			else
				SetPosition();
		}
		public virtual void Close()
		{
			gameObject.SetActive(false);
		}
		//public void MakeTooltip(TooltipInfo info)
		//{
		//	if (!TooltipEnabled)
		//		return;
		//	SetPosition();
		//	tekst.text = info.tooltipText;
		//	gameObject.SetActive(true);
		//}

		public void HideTooltip()
		{
			gameObject.SetActive(false);
		}
		protected void SetPosition()
		{
			var temp = Input.mousePosition;
			float x = rt.sizeDelta.x * rt.lossyScale.x;
			float y = rt.sizeDelta.y * rt.lossyScale.y;
			if (temp.x - x - XDistance < 0)
				position.x = XDistance;
			else
				position.x = -XDistance - x;


			if (temp.y + y + YDistance > Screen.height)
				position.y = -YDistance - y;
			else
				position.y = YDistance;
			transform.position = temp + position;
		}
	}
}
