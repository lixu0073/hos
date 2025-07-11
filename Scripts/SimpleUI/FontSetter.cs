using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SimpleUI
{
	public class FontSetter : MonoBehaviour
	{

		public Font defaultFont;
		// Use this for initialization
		void Start()
		{
			SetFont(gameObject);
		}
		private void SetFont(GameObject temp)
		{
			foreach(var p in temp.GetComponents<Text>())
			{
				int size = p.fontSize;
				p.font = defaultFont;
				p.fontSize = size;
			}
			for (int i = 0; i < temp.transform.childCount; i++)
				SetFont(temp.transform.GetChild(i).gameObject);
		}
	}
}
