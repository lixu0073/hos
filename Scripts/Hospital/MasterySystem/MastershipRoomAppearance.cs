using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public interface MastershipRoomAppearance
    {
        void SetAppearance(int masteryLevel, bool showAnimation = true);
    }
}
