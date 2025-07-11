using UnityEngine;
using System.Collections;

namespace SimpleUI
{
	public class PopupController : UIElement
	{
		[SerializeField]
		private bool allowMultiple = false;

		public bool AllowMultiple
		{
			get
			{
				return allowMultiple;
			}
		}
	}
}