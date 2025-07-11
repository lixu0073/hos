using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SimpleUI
{
	public class HoverPanel : MonoBehaviour
	{

		public void AddElement(GameObject element, int width, int height)
		{
			//print("dodawanie");
			element.AddComponent<LayoutElement>();
			element.GetComponent<LayoutElement>().preferredWidth = width;
			element.GetComponent<LayoutElement>().preferredHeight = height;
			element.transform.SetParent(transform);
			element.transform.localScale = Vector3.one;
			//element.transform.localScale = Vector3.one;
		}
		public void RemoveAllElements()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Destroy(transform.GetChild(i).gameObject);
			}
		}
	}
}