using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PassFriendCodeView : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button applyCodeButton;
    [SerializeField] private Sprite buttonActiveSprite;
    [SerializeField] private Sprite buttonInactiveSprite;

    public UnityAction Exit
    {
        set
        {
            exitButton.RemoveAllOnClickListeners();
            exitButton.onClick.AddListener(value);
        }
    }

    public TMP_InputField CodeInputField
    {
        get { return codeInputField; }
    }

    public UnityAction<string> OnInputFieldValueChange
    {
        set
        {
            codeInputField.onValueChanged.RemoveAllListeners();
            codeInputField.onValueChanged.AddListener(value);
        }
    }

    public UnityAction ApplyCode
    {
        set
        {
            applyCodeButton.RemoveAllOnClickListeners();
            applyCodeButton.onClick.AddListener(value);
        }
    }

    public bool IsClickable { get; private set; }

    public void SetButtonClickable(bool value)
    {
        applyCodeButton.image.sprite = value ? buttonActiveSprite : buttonInactiveSprite;
        IsClickable = value;
        applyCodeButton.interactable = value;
    }
}
