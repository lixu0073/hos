using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Hospital;
using SimpleUI;

public class StorageTab : MonoBehaviour
{
        public Image background;
        public Image icon;
        public Sprite activeImageBg;
        public Sprite inactiveImageBg;
		public Animator tabAnimator;

        public void ChangeTabButton(bool isActive)
        {
            if (isActive)
            {
                icon.color = new Color(1f, 1f, 1f, 1f);
                background.sprite = activeImageBg;
			tabAnimator.SetTrigger ("Bounce");
            }
            else
            {
                icon.color = new Color(1f, 1f, 1f, 0.8f);
                background.sprite = inactiveImageBg;
            }

            SoundsController.Instance.PlayButtonClick(UIController.get.drawer.IsInitalizing());
        }

}
