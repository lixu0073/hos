using UnityEngine;
using System.Collections;
using SimpleUI;
using TMPro;
using System;

public class EventPopUp : UIElement
{
    public TextMeshProUGUI title;
    public Transform contentParent;

    BaseGameEventInfo currentEvent;
    public GameObject content;          //this can be null.
    Coroutine contentCoroutine;


    private void OnDisable()
    {
        if (contentCoroutine != null)
        {
            try
            { 
                StopCoroutine(contentCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
    {
        yield return base.Open();

        currentEvent = GameEventsController.Instance.currentEvent;
        SetTitle();
        SetContent();
		SoundsController.Instance.PlayEventPopUp ();

        AnalyticsController.instance.ReportEventOpened(currentEvent);

        whenDone?.Invoke();
    }

    void SetTitle()
    {
        title.text = I2.Loc.ScriptLocalization.Get(currentEvent.eventTitle);
    }

    void SetContent()
    {
        if (content == null && contentCoroutine == null)
            contentCoroutine = StartCoroutine(LoadContent());
    }

    IEnumerator LoadContent()
    {
        ResourceRequest rr = Resources.LoadAsync(currentEvent.popupContentPath);
        while (!rr.isDone)
            yield return 0;
        
        content = Instantiate(rr.asset) as GameObject;
        content.transform.SetParent(contentParent);
        content.transform.localScale = Vector3.one;
        RectTransform rt = content.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector3.zero;
    }

    public void SpawnDecorationGiftParticle()
    {
        if (content == null) return;
        LabourDayPopUpContent contentController = content.GetComponent<LabourDayPopUpContent>();
        if (contentController == null) return;
        contentController.SpawnDecorationGiftParticle();
    }

    public void ButtonExit()
    {
        Exit();
    }

    public void ButtonIAP()
    {
        IAPController.instance.BuyProductID("labourday_1");
    }
}