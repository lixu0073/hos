using UnityEngine;
using System.Collections;

namespace SimpleUI
{
	public class ActionBarEntry
	{
		public delegate void PressEventHandler();

		public readonly int cash;
		public readonly string label;
		public readonly bool clickable;

		public UnityEngine.Events.UnityAction OnPress;

		public ActionBarEntry(int cash, string label)
		{
			this.cash = cash;
			this.label = label;
		}
	}
}