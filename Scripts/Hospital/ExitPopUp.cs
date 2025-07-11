using UnityEngine;
using System.Collections;
using SimpleUI;

namespace Hospital
{
	public class ExitPopUp : UIElement
	{

		public IEnumerator Open(IMapArea areaData)
		{
			yield return base.Open();
	    }

        public void ButtonExit()
        {
            Exit();
        }

        public void ButtonConfirm()
        {
            SaveSynchronizer.Instance.InstantSave();
#if UNITY_ANDROID
            AndroidQuitter.Quit();
#elif !UNITY_EDITOR && !UNITY_ANDROID
            Application.Quit();
#else
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
	}
}