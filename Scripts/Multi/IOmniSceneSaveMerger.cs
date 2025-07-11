using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete]
public interface IOmniSceneSaveMerger
{
    Save MergeSave(Save save);
    void Initialize(Save save);
    void Destroy();
}
