using UnityEngine;
using System.Collections;

namespace SimpleUI
{
	public class ActionBarData
	{
		public readonly string Name;
		public readonly ActionBarEntry[] Entries;

		public ActionBarData(string name, int entries)
		{
			this.Name = name;
			this.Entries = new ActionBarEntry[entries];
		}
	}
}