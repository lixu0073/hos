using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;
using UnityEngine.UI;

public class InfoPopupMastershipTab : PopupTab
{
    [SerializeField]
    GameObject mastershipContent = null;
    [SerializeField]
    GameObject mastershipCardPrefab = null;
    [SerializeField]
    GameObject mastershipSeparatorPrefab = null;
    [SerializeField]
    ScrollRect scroll = null;
    [SerializeField]
    MastershipCardController[] cards = null;
    [SerializeField]
    GameObject[] separators = null;



    private Coroutine scrollCoroutine = null;

    [HideInInspector]
    public bool showScrollEffect = false;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    protected override void OnPopupOpen()
    {
        scroll.horizontalNormalizedPosition = 1;
        showScrollEffect = true;
    }

    protected override void PrepareContent()
    {
        MasterableProperties masterableobject = UIController.getHospital.HospitalInfoPopUp.GetCurrentMasterableObject();
        if (masterableobject != null)
        {
            PrepareMastershipContent(masterableobject);
        }
    }

    public void PrepareMastershipContent(MasterableProperties masterableObject)
    {
        if (mastershipContent == null)
        {
            Debug.LogError("MastershipContent is null");
            return;
        }
        if (mastershipCardPrefab == null)
        {
            Debug.LogError("MastershipCardPrefab is null");
            return;
        }
        if (masterableObject == null)
        {
            Debug.LogError("masterableObject is null");
            return;
        }

        if (cards.Length < masterableObject.MasterableConfigData.MasteryGoals.Length || separators.Length < masterableObject.MasterableConfigData.MasteryGoals.Length - 1)
        {
            Debug.LogError("need more cards or separators");
        }

        for (int i = masterableObject.MasterableConfigData.MasteryGoals.Length; i < cards.Length; ++i)
        {
            cards[i].gameObject.SetActive(false);
        }
        for (int i = masterableObject.MasterableConfigData.MasteryGoals.Length - 1; i < separators.Length; ++i)
        {
            separators[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < masterableObject.MasterableConfigData.MasteryGoals.Length; ++i)
        {
            cards[i].gameObject.SetActive(true);
            cards[i].SetCard(i, masterableObject);
            if (i < masterableObject.MasterableConfigData.MasteryGoals.Length - 1)
            {
                separators[i].gameObject.SetActive(true);
            }
        }
        RunScrollEffect(masterableObject);
    }

    private void AddCard(int cardLevel, MasterableProperties masterableObject)
    {
        GameObject card = Instantiate(mastershipCardPrefab, mastershipContent.transform);
        card.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        MastershipCardController controller = card.GetComponent<MastershipCardController>();
        if (controller == null)
        {
            Debug.LogError("controller is null");
            return;
        }
        controller.SetCard(cardLevel, masterableObject);
    }

    private void AddSeparator()
    {
        GameObject separator = Instantiate(mastershipSeparatorPrefab, mastershipContent.transform);
        separator.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
    }

    private void RunScrollEffect(MasterableProperties masterableObject)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (!showScrollEffect)
        {
            return;
        }
        showScrollEffect = false;
        try { 
            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        scrollCoroutine = StartCoroutine(ScrollEffect(masterableObject));
    }

    IEnumerator ScrollEffect(MasterableProperties masterableObject)
    {
        yield return new WaitForEndOfFrame();

        float selectPos = CalcSelectedScrollPosition(masterableObject);
        while (Mathf.Abs(scroll.horizontalNormalizedPosition - selectPos) > 0.01f)
        {
            //normPos += Time.deltaTime / 2;
            scroll.horizontalNormalizedPosition = Mathf.Lerp(scroll.horizontalNormalizedPosition, selectPos, .1f);
            yield return null;
        }
        scroll.horizontalNormalizedPosition = selectPos;
    }

    private float CalcSelectedScrollPosition(MasterableProperties masterableObject) //będzie hardkor
    {
        float scrollPosition = 0;
        int maxAmount = masterableObject.MasterableConfigData.MasteryGoals.Length - 1;
        scrollPosition = (Mathf.Clamp((masterableObject.MasteryLevel), 0, maxAmount) / ((float)maxAmount));

        return scrollPosition;
    }

    protected override void OnPopupClose()
    {

    }

    protected override void OnTabSwitchFromCurrent()
    {

    }
}
