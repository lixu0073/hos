using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{

    public class InGameFriend : BaseFollower
    {
        public InGameFriend(string SaveID)
        {
            saveID = SaveID;
        }

        public override Sprite GetFrame()
        {
            if (InGameFriendData.Accepted)
            {
                return ResourcesHolder.Get().frameData.inGameFriendFrame;
            }
            else
            {
                return ResourcesHolder.Get().frameData.basicFrame;
            }
        }
    }
}