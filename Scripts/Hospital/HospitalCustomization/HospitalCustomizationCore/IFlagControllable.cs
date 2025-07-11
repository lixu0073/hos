using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFlagControllable
{
    void UnloadFlagAssetFromBundle(string flagName);
    void AddNewFlagToPlayerCollection(string flagName);
    bool IsFlagBought(string flagName);
    List<string> GetOwnedFlags();
    string GetCurrentFlagName();
    string GetPreviousFlagName();
    void SetCurrentFlagName(string flagName);
    string GetSelectedFlagName();
    void SetSelectedFlagName(string flagName);
    void ApplySelectedFlagName();
    void AddFlagCustomization();
    void UpdateFlagSprite(Sprite sprite, bool fromsave = false);
    void SetFlagActive(bool setActive);
}
