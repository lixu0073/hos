using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;

public interface ISynchronizable
{
    void Load(Save save);
    void Save(Save save);
}
