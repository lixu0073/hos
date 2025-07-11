using System.Collections;
using SimpleUI;

namespace Hospital
{
	public class EpidemyLockedPopUpController : UIElement
	{        
		public IEnumerator Open()
		{
			yield return base.Open();
		}

		public void Exit()
		{
			base.Exit();
		}
	}
}
