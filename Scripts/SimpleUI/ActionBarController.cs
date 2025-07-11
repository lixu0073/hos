using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SimpleUI
{
	public class ActionBarController : UIElement
	{
		private ActionBarUserController current;

		private PopupController.FinishedAnimatingEventHandler currentAnimation = null;

		public void Show(ActionBarUserController owner)
		{
			if (IsVisible)
			{
				if (currentAnimation != null)
					return;

				if(current != owner)
				{
					OnFinishedAnimating += currentAnimation = delegate()
					{
						OnFinishedAnimating -= currentAnimation;

						SetData(owner);

						OnFinishedAnimating += currentAnimation = delegate()
						{
							OnFinishedAnimating -= currentAnimation;
							currentAnimation = null;
						};
						SetVisible(true);
					};
				}
				else
				{
					OnFinishedAnimating += currentAnimation = delegate()
					{
						OnFinishedAnimating -= currentAnimation;
						currentAnimation = null;
					};
				}
				SetVisible(false);
				
				current = null;
			}
			else
			{
				// It is hiding
				if(currentAnimation != null)
				{
					OnFinishedAnimating -= currentAnimation;
					OnFinishedAnimating += currentAnimation = delegate()
					{
						OnFinishedAnimating -= currentAnimation;
						SetData(owner);

						OnFinishedAnimating += currentAnimation = delegate()
						{
							OnFinishedAnimating -= currentAnimation;
							currentAnimation = null;
						};
						SetVisible(true);
					};
				}
				else
				{
					SetData(owner);

					OnFinishedAnimating += currentAnimation = delegate()
					{
						OnFinishedAnimating -= currentAnimation;
						currentAnimation = null;
					};
					SetVisible(true);
				}

				
			}
		}

		private void SetData(ActionBarUserController owner)
		{
			this.current = owner;

			ActionBarData actionBarData = current.ActionBarData;

			transform.GetChild(0).GetComponent<Text>().text = actionBarData.Name;

			for (int i = 0; i < actionBarData.Entries.Length; ++i)
			{
				GameObject child = transform.GetChild(i + 1).gameObject;

				child.SetActive(true);

				Transform buttonTransform = child.transform.GetChild(0);
				buttonTransform.GetChild(0).GetComponent<Text>().text = actionBarData.Entries[i].label;

				Button button = buttonTransform.GetComponent<Button>();
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(actionBarData.Entries[i].OnPress);
			}

			for (int i = actionBarData.Entries.Length; i < transform.childCount - 1; ++i)
			{
				transform.GetChild(i + 1).gameObject.SetActive(false);
			}
		}
	}
}