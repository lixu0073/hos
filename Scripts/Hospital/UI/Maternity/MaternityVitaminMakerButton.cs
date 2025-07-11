using Hospital;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaternityVitaminMakerButton : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private Button button;
#pragma warning restore 0649

    private void Start()
    {
        button.gameObject.SetActive(false);
    }

    public void InitializeButton(List<string> laboratoryObjects)
    {
        if (IsVitaminMakerWorking(laboratoryObjects))
        {
            button.RemoveAllOnClickListeners();
            button.onClick.AddListener(delegate
            {
                Hospital.MaternityAreasMapController.MaternityMap.maternityVitaminMaker.OnClick();
            });
            button.gameObject.SetActive(true);
        }
    }

    private bool IsVitaminMakerWorking(List<string> laboratoryObjects)
    {
        for (int i = 0; i < laboratoryObjects.Count; i++)
        {
            if (laboratoryObjects[i].Contains("VitaminMaker$"))
            {
                string[] settings = laboratoryObjects[i].Split(';')[0].Split('/');
                if ((RotatableObject.State)Enum.Parse(typeof(RotatableObject.State), settings[2]) == RotatableObject.State.working)
                {
                    return true;
                }
                return false;
            }
        }
        return false;
    }
}
