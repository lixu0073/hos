using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using SimpleUI;
using Hospital;
using TMPro;
using MovementEffects;
using System;


public class EpidemyOffPopUpController : UIElement
{
#pragma warning disable 0649
    [SerializeField] private Image[] comingMedicines;
    [SerializeField] private PointerDownListener[] comingMedicinesListeners;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TextMeshProUGUI diamondsCost;
    [SerializeField] private Button getEpidemyNowButton;
#pragma warning restore 0649
    private Epidemy epidemyController;
    IEnumerator<float> updateCoroutine;

    public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
    {
        yield return base.Open();
        epidemyController = HospitalAreasMapController.HospitalMap.epidemy;

        if (updateCoroutine != null)
            Timing.KillCoroutine(updateCoroutine);
        updateCoroutine = Timing.RunCoroutine(UpdateTimerAndCost());

        whenDone?.Invoke();
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        base.Exit(hidePopupWithShowMainUI);
        if (updateCoroutine != null)
            Timing.KillCoroutine(updateCoroutine);
    }

    public void SetMedicinesImages(List<Sprite> comingMedicinesSprites)
    {
        for (int i = 0; i < comingMedicinesSprites.Count; i++)
        {
            comingMedicines[i].sprite = comingMedicinesSprites[i];
        }
    }

    public void SetMedicinesTooltips(List<MedicineRef> comingMedicines)
    {
        for (int i = 0; i < comingMedicines.Count; i++)
        {
            int index = i;
            if (comingMedicines.Count <= index || comingMedicines[index] == null)
                continue;
            if (comingMedicines[index].type == MedicineType.BasePlant)
            {
                comingMedicinesListeners[i].SetDelegate(() =>
                {
                    FloraTooltip.Open(comingMedicines[index]);
                });
            }
            else
            {
                comingMedicinesListeners[i].SetDelegate(() =>
                {
                    TextTooltip.Open(comingMedicines[index]);
                });
            }
        }
    }

    IEnumerator<float> UpdateTimerAndCost()
    {
        while (true)
        {
            //yield return Timing.WaitForSeconds(1f);
            if (!epidemyController.Outbreak)
            {
                timer.text = UIController.GetFormattedTime((int)ReferenceHolder.GetHospital().Epidemy.TimeTillOutbreak);
                diamondsCost.text = epidemyController.CalculateEpidemySpeedUpCost().ToString();
            }
            else
            {
                Exit();
            }
			yield return Timing.WaitForSeconds(1f);
        }
    }

    public void ButtonExit()
    {
        Exit();
    }

    public void ButtonGetNow()
    {
        epidemyController.SpeedUpEpidemyStartWithDiamonds(this);
    }
}
