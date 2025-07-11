using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseCharacterInfo : MonoBehaviour
{

	public Sprite AvatarBody;
	public Sprite AvatarHead;

	public string Name;
    public string Surname;
    public int Likes;        //ADD TO SAVE/LOAD!
    public int Dislikes;     //ADD TO SAVE/LOAD!
    public int Sex;
	public int Age;
    public BloodType BloodType; //ADD TO SAVE/LOAD
    public int Race;
	public int CoinsForCure;
	public int EXPForCure;
    public int PositiveEnergyForCure;

    public int Type;
	public bool IsVIP;
	public float VIPTime;
    public int VIPDescription;
    public bool RequiresDiagnosis;
    public bool HelpRequested = false;
    public bool HasBacteria;
    public int BacteriaGlobalTime;
    public int AWS_InfectionTime;

    public string personalBIO;

	public GameObject heartsCured;

    public virtual void Initialize()
	{

	}

    public virtual string GetLikesString()
    {
        if (IsVIP)
            return I2.Loc.ScriptLocalization.Get("VIP_BIOS/VIP_LIKES_" + Likes);
        else
            return I2.Loc.ScriptLocalization.Get("PATIENT_LIKES/LIKES_" + Likes);
    }

    public virtual string GetDislikesString()
    {
        if (IsVIP)
            return I2.Loc.ScriptLocalization.Get("VIP_BIOS/VIP_DISLIKES_" + Dislikes);
        else
            return I2.Loc.ScriptLocalization.Get("PATIENT_DISLIKES/DISLIKES_" + Dislikes);
    }

    public void CopyBaseCharacterInfo(BaseCharacterInfo BCI)
    {
        AvatarBody = BCI.AvatarBody;
        AvatarHead = BCI.AvatarHead;

        Name = BCI.Name;
        Surname = BCI.Surname;
        Likes = BCI.Likes;
        Dislikes = BCI.Dislikes;
        Sex = BCI.Sex;
        Age = BCI.Age;
        BloodType = BCI.BloodType;
        Race = BCI.Race;
        CoinsForCure = BCI.CoinsForCure;
        EXPForCure = BCI.EXPForCure;
        PositiveEnergyForCure = BCI.PositiveEnergyForCure;

        Type = BCI.Type;
        IsVIP = BCI.IsVIP;
        VIPTime = BCI.VIPTime;
        VIPDescription = BCI.VIPDescription;
        RequiresDiagnosis = BCI.RequiresDiagnosis;
        HelpRequested = BCI.HelpRequested;
        HasBacteria = BCI.HasBacteria;
        BacteriaGlobalTime = BCI.BacteriaGlobalTime;
        AWS_InfectionTime = BCI.AWS_InfectionTime;

        personalBIO = BCI.personalBIO;
    }
}

public enum BloodType
{
    Op,
    Om,
    Ap,
    Am,
    Bp,
    Bm,
    ABp,
    ABm
}