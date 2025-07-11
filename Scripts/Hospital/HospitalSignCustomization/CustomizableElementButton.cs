using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MovementEffects;

public class CustomizableElementButton : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
{
    [SerializeField] private Image icon = null;
    [SerializeField] private GameObject selection = null;
    [SerializeField] private GameObject premiumBg = null;
#pragma warning disable 0414
    [SerializeField] private Button button = null;
#pragma warning restore 0414
    [SerializeField] private Material grayscale = null;
    [SerializeField] private float selectionDelay = 0.05f;

    private bool isUnlocked = false;
    private bool isSelected = false;
    private bool isPremium = false;
    //private bool isNew = false;

    private GameObject container;
    private bool canScroll = true;
#pragma warning disable 0649
    [SerializeField] EventTrigger eventTrigger;
#pragma warning restore 0649
    IEnumerator<float> selectionCoroutine = null;
    private UnityAction buttonAction;

    public void SetElement(Sprite iconImage, UnityAction buttonAction, bool unlocked,  bool selected = false, bool premium = false)
    {
        SetIcon(iconImage);
        SetUnlocked(unlocked);
        SetPremiumBg(premium);
        SetSelected(selected);
        SetButtonAction(buttonAction);
    }

    private void SetIcon(Sprite iconImage)
    {
        if (iconImage != null)    
            icon.sprite = iconImage;
    }

    private void SetButtonAction(UnityAction buttonAction)
    {
        if (eventTrigger == null)    
            Debug.LogError("missing EventTrigger");
        else
        {
            eventTrigger.triggers[0].callback.RemoveAllListeners();

            eventTrigger.triggers[0].callback.AddListener((eventData) =>
            {
                canScroll = true;

                if (buttonAction != null)
                    this.buttonAction = buttonAction;
                else
                    this.buttonAction = null;

                if (selectionCoroutine != null)
                {
                    Timing.KillCoroutine(selectionCoroutine);
                    selectionCoroutine = null;
                }
                if (!isSelected)
                    selectionCoroutine = Timing.RunCoroutine(SelectionDelayCoroutine()); 
            }
            );
        }
        /*button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => UIController.get.hospitalSignPopup.SetCurrentButtonSelected(this));
        button.onClick.AddListener(buttonAction);*/
    }
    
    private void SetPremiumBg(bool premium = false)
    {
        isPremium = premium;
        premiumBg.SetActive(isPremium);
    }

    public void SetSelected(bool select)
    {
        isSelected = select;
        selection.SetActive(select);
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        if (unlocked)        
            icon.material = null;
        else
            icon.material = grayscale;
    }

    public void SetContainer(GameObject Container)
    {
        container = Container;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canScroll)
        {
            Timing.KillCoroutine(selectionCoroutine);
            selectionCoroutine = null;
            container.SendMessage("OnBeginDrag", eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canScroll)
        {
            Timing.KillCoroutine(selectionCoroutine);
            selectionCoroutine = null;
            container.SendMessage("OnEndDrag", eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canScroll)
        {
            Timing.KillCoroutine(selectionCoroutine);
            selectionCoroutine = null;
            container.SendMessage("OnDrag", eventData);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (canScroll)
        {
            Timing.KillCoroutine(selectionCoroutine);
            selectionCoroutine = null;
            container.SendMessage("OnScroll", eventData);
        }
    }

    IEnumerator<float> SelectionDelayCoroutine()
    {
        yield return Timing.WaitForSeconds(selectionDelay);
        //canScroll = false;
        UIController.getHospital.hospitalSignPopup.SetCurrentButtonSelected(this);
        buttonAction?.Invoke();
    }
}
