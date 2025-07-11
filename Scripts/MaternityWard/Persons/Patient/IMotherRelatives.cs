using Hospital;
using Maternity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMotherRelatives
{
    string SaveToString();
    void InitializeFromString(MaternityPatientAI mother, string info);
    RelativesType relativesType { get; }
    BabyCharacterInfo GetInfo();
}

public enum RelativesType
{
    Baby = 0,
}
