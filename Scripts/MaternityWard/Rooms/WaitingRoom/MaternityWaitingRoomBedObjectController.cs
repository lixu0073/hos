using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityWaitingRoomBedObjectController : MonoBehaviour
{
    public GameObject emptyBed;
    public GameObject occupiedBed;
    public GameObject occupiedWithoutPatientBed;
    public GameObject normalSheet;
    public GameObject bulgedSheet;
    public Animator bloodTestAnimator;

    private void ResetBedVisualization()
    {
        emptyBed.SetActive(false);
        occupiedBed.SetActive(false);
        occupiedWithoutPatientBed.SetActive(false);
        normalSheet.SetActive(false);
        bulgedSheet.SetActive(false);
    }

    public void RevealBloodTestBadge()
    {
        try { 
            if (bloodTestAnimator == null)
            {
                return;
            }
            bloodTestAnimator.gameObject.SetActive(true);
            bloodTestAnimator.Play("Reveal", 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
    }

    public void ShowBloodTestBadge()
    {
        try { 
            if (bloodTestAnimator == null)
            {
                return;
            }
            bloodTestAnimator.gameObject.SetActive(true);
            bloodTestAnimator.Play("Idle", 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
    }

    public void HideBloodTestBadge()
    {
        if (bloodTestAnimator.gameObject.activeInHierarchy)
        {
            try { 
                bloodTestAnimator.Play("Hide", 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            bloodTestAnimator.gameObject.SetActive(false);
        }
    }

    public void SetupBed(bool isPatientExist, bool isPatientAway = false, bool isPatientPregnant = false)
    {
        if (!isPatientExist)
        {
            MakeBed();
        }
        else if (isPatientAway)
        {
            UncoverBed();
        }
        else
        {
            MakeBedForPatient(isPatientPregnant);
        }
    }

    public void MakeBed()
    {
        ResetBedVisualization();
        emptyBed.SetActive(true);
    }

    private void MakeBedForPatient(bool isPregnant)
    {
        ResetBedVisualization();
        occupiedBed.SetActive(true);
        bulgedSheet.SetActive(isPregnant);
        normalSheet.SetActive(!isPregnant);
    }

    private void UncoverBed()
    {
        ResetBedVisualization();
        occupiedWithoutPatientBed.SetActive(true);
    }
}
