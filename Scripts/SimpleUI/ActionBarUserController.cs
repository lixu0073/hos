using UnityEngine;
using System.Collections.Generic;

namespace SimpleUI
{
	public class ActionBarUserController : MonoBehaviour
	{
		#region Debug tmp

		public List<string> names;
		public List<int> costs;

		public ActionBarController actionBar;

		private void Test()
		{
			ActionBarData = new ActionBarData(gameObject.name, names.Count);

			for (int i = 0; i < names.Count; ++i)
			{
				ActionBarData.Entries[i] = new ActionBarEntry(costs[i], names[i]);

				switch(i)
				{
					case 0:
					{
						ActionBarData.Entries[i].OnPress += () => { Debug.Log(gameObject.name + " says hello!"); };
					}
					break;

					case 1:
					{
						ActionBarData.Entries[i].OnPress += () => { Debug.Log(gameObject.name + " says bye!"); };
					}
					break;

					case 2:
					{
						ActionBarData.Entries[i].OnPress += () => { Debug.Log(gameObject.name + " says whatever!"); };
					}
					break;

					case 3:
					{
						ActionBarData.Entries[i].OnPress += () => { Debug.Log(gameObject.name + " says abracadabra!"); };
					}
					break;

					case 4:
					{
						ActionBarData.Entries[i].OnPress += () => { Debug.Log(gameObject.name + " says yo nigga!"); };
					}
					break;
				}

				
			}
		}

		private void OnMouseDown()
		{
			actionBar.Show(this);
		}



		#endregion






		public ActionBarData ActionBarData
		{
			get;
			private set;
		}

		void Start()
		{
			Test();
		}
	}
}