using Maternity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsoEngine;
using Hospital;
using System;

public class BabyPatient : MonoBehaviour, IMotherRelatives
{
    Animator animator;
    MaternityPatientAI mother;
    private const string BABY_DATA_PLACEHOLDER = "null";
    private BabyCharacterInfo Info;

    public RelativesType relativesType
    {
        get
        {
            return RelativesType.Baby;
        }
    }

    public void Initialize(MaternityPatientAI mother)
    {
        try { 
            animator = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
            animator.Play(AnimHash.Baby_OnHands_Sleeping, 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
        this.mother = mother;
        mother.onStateChanged += UpdateBabyAnimation;
    }

    private void UpdateBabyAnimation()
    {
        switch (mother.Person.State.GetTag())
        {
            case Maternity.PatientStates.MaternityPatientStateTag.GTWR:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.WFSTD:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.GO:
                try { 
                animator.Play(AnimHash.Baby_OnHands_Idle, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.WFC:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.WFL:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.RFL:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.IDQ:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.ID:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.WFDR:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.IL:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.LF:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.RTWR:
                animator.SetBool(AnimHash.IsBabySleepingBool, true);
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.B:
                animator.SetBool(AnimHash.IsBabySleepingBool, false);
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.GTLR:
                break;
            case Maternity.PatientStates.MaternityPatientStateTag.WFCR:
                break;
            default:
                break;
        }
    }

    private void OnDestroy()
    {
        mother.onStateChanged -= UpdateBabyAnimation;
    }

    public string SaveToString()
    {
        return relativesType.ToString() + "/" + BABY_DATA_PLACEHOLDER + "|" + GetInfo().personalBIO;
    }

    public void InitializeFromString(MaternityPatientAI mother, string info)
    {
        Initialize(mother);
    }

    public BabyCharacterInfo GetInfo()
    {
        if(Info == null)
            Info = GetComponent<BabyCharacterInfo>();
        return Info;
    }
}
