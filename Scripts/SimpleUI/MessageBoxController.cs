using UnityEngine;
using System.Collections;
using System;

//public class EnumFlagsAttribute : PropertyAttribute
//{
//	public EnumFlagsAttribute()
//	{
//	}
//}


[ExecuteInEditMode]
public class MessageBoxController : MonoBehaviour
{
	[System.Flags]
	public enum Button : int
	{
		Cancel = (1 << 0),
		OK = (1 << 1),
		No = (1 << 2),
		Yes = (1 << 3),
	}
	public delegate void OnButton(Button but);

	public OnButton click;

	[EnumFlags, SerializeField]
	private Button buttons;
	public Button Buttons
	{
		get
		{
			return buttons;
		}

		set
		{
			if (buttons != value)
			{
				buttons = value;

				for (int i = 0; i < allButtons.Length; ++i)
				{
					if ((buttons & enumValues[i]) == enumValues[i])
						allButtons[i].gameObject.SetActive(true);
					else
						allButtons[i].gameObject.SetActive(false);
				}
			}
		}
	}

	private void ButtonClicked(Button button)
	{
		Result = button;
		click?.Invoke(Result);
	}

	private Button oldButtons;

	private Button[] enumValues;
	private RectTransform buttonsGroup;
	private RectTransform[] allButtons;

	public Button Result
	{
		get;
		private set;
	}
	
	public void OnEnable()
	{
		Debug.Log("Start");
		click = null;
		Result = 0;
		enumValues = (Button[])Enum.GetValues(typeof(Button));

		buttonsGroup = transform.Find("Buttons") as RectTransform;
		
		allButtons = new RectTransform[enumValues.Length];
		for (int i = 0; i < allButtons.Length; ++i)
		{
			allButtons[i] = buttonsGroup.Find(enumValues[i].ToString() + " button") as RectTransform;

			var button = allButtons[i].GetComponent<UnityEngine.UI.Button>();

			Button val = enumValues[i];
			button.onClick.AddListener(delegate() {ButtonClicked(val); });
		}
	}

#if UNITY_EDITOR
	public void Update()
	{
		if (buttons != oldButtons)
		{
			for (int i = 0; i < allButtons.Length; ++i)
			{
				if ((buttons & enumValues[i]) == enumValues[i])
					allButtons[i].gameObject.SetActive(true);
				else
					allButtons[i].gameObject.SetActive(false);
			}
		}

		if(Result != 0)
		{
			Debug.Log(Result);

		}

		oldButtons = buttons;
	}
#endif
}
