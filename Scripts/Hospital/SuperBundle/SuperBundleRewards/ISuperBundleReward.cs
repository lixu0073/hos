using UnityEngine;
using System.Collections;

public interface ISuperBundleReward
{
    void Collect(float delay = 0f);
    int GetAmount();
    string GetName();
    Sprite GetSprite();
    string ToString();

}
