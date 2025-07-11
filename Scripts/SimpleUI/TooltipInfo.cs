using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI
{
//	public class TooltipInfo : MonoBehaviour
//	{
//		public string tooltipText;
//		public GameObject MovedObject;
//		public bool delayed;
//		private Coroutine rutyna;
//		public void ShowTooltip()
//		{
//#if (UNITY_ANDROID || UNITY_IOS)
//				rutyna = StartCoroutine(TooltipCoroutine());
//#else
//			Tooltip.ToolTip.MakeTooltip(this);
//#endif
//		}
//		public IEnumerator TooltipCoroutine()
//		{
//			yield return new WaitForSeconds(0.5f);
//			if (gameObject.GetComponent<Selectable>() != null)
//				gameObject.GetComponent<Selectable>().interactable = false;
//			Tooltip.ToolTip.MakeTooltip(this);
//		}
//		public void HideTooltip()
//		{
//			if (rutyna != null)
//			{
//				StopCoroutine(rutyna);
//				rutyna = null;
//			}
//#if (UNITY_ANDROID || UNITY_IOS)
//			if (gameObject.GetComponent<Selectable>() != null)
//			{
//				gameObject.GetComponent<Selectable>().interactable = true;
//			}
//#endif
//			Tooltip.ToolTip.HideTooltip();
//		}
//	}
}
