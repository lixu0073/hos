using UnityEngine;
using UnityEngine.UI;

public class PopupBookmark : MonoBehaviour
{
    [SerializeField] private float shiftDistance = 0;
    [SerializeField] private Image background = null;
    [SerializeField] private Image icon = null;

    private Button button = null;

    [SerializeField] private Sprite selectedBackground = null;
    [SerializeField] private Sprite deselectedBackground = null;
    [SerializeField] private Sprite selectedIcon = null;
    [SerializeField] private Sprite deselectedIcon = null;
    [SerializeField] private Material grayscale = null;
#pragma warning disable 0649
    [SerializeField] private Animator tabAnimator;
#pragma warning restore 0649
    private bool isSelected = false;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetBookmarkSelected(bool selected)
    {
        if (selected && isSelected != selected)
        {
            tabAnimator.SetTrigger("Bounce");
        }

        isSelected = selected;

        if (isSelected)
        {
            transform.SetAsLastSibling();
            background.sprite = selectedBackground;
            icon.sprite = selectedIcon;
            ShiftBookmark(0);
        }
        else
        {
            transform.SetAsFirstSibling();
            background.sprite = deselectedBackground;
            icon.sprite = deselectedIcon;
            ShiftBookmark(shiftDistance);
        }
    }

    public void SetBookmarkInteractable(bool interactable)
    {
        /*if (button != null) {
            button.interactable = interactable;
        }*/
        if (interactable)
        {
            icon.material = null;
            background.material = null;
        }
        else
        {
            icon.material = grayscale;
            background.material = grayscale;
        }
    }

    private void ShiftBookmark(float distance)
    {
        Vector2 offsetMin = transform.GetComponent<RectTransform>().offsetMin;
        Vector2 offsetMax = transform.GetComponent<RectTransform>().offsetMax;
        offsetMin.x = distance;
        offsetMax.x = -distance;
        transform.GetComponent<RectTransform>().offsetMin = offsetMin;
        transform.GetComponent<RectTransform>().offsetMax = offsetMax;
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}
