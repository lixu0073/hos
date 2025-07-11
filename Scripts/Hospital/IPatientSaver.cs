using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPatientSaver
{
    List<string> SaveToStringList();
    void LoadFromStringList(List<string> saveList);
}
