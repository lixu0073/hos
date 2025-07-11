using Hospital;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

public abstract class MultiSceneInformationController : MonoBehaviour
{
    private List<MultiSceneInformation> informationData;
    private Coroutine updateInformations;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    protected void AddInfoToInformationData(MultiSceneInformation info)
    {
        if (info!=null)
        {
            informationData.Add(info);
        }
    }

    public void InitializeFromSave(Save saveData)
    {
        informationData = new List<MultiSceneInformation>();
        FillList(saveData);
        StartUpdatingInformers();
    }

    private void StartUpdatingInformers()
    {
        StopUpdatingInformers();
        if (informationData.Count > 0)
        {
            updateInformations = StartCoroutine(UpdateInfo());
        }
    }

    private void StopUpdatingInformers()
    {
        if (updateInformations != null)
        {
            try { 
                StopCoroutine(updateInformations);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            updateInformations = null;
        }
    }

    private IEnumerator UpdateInfo()
    {
        while (informationData.Count > 0)
        {
            for (int i = 0; i < informationData.Count; i++)
            {
                informationData[i].CheckMultiSceneInformatin();
            }
            yield return new WaitForSeconds(2);
        }
        StopUpdatingInformers();
    }

    protected abstract void FillList(Save saveData);
}