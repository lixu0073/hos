using UnityEngine;
using System.Collections;

namespace Hospital
{
    public interface IUpgradeUseCase
    {
        Save Upgrade(Save save,bool visitingPurpose);
    }
}
