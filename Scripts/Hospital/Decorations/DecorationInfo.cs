using UnityEngine;
using System.Collections;

namespace Hospital
{

    public class DecorationInfo : ShopRoomInfo
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/RoomInfo/DecorationInfo")]
        public new static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<DecorationInfo>();
        }

#endif
        [HideInInspector]
        public bool isDecoration = true;
        public int goldIncreasePerOwnedItem
        {
            get
            {
                if (costInDiamonds!=0)
                {
                    return 0;
                }
                int valueToReturn = Mathf.CeilToInt(0.1f * cost);

                if (AreaMapController.Map.casesManager.decorationsToDraw.Contains(this))
                {
                    valueToReturn = Mathf.Clamp(valueToReturn, 5, 100);
                }
                else
                {
                    valueToReturn = Mathf.Clamp(valueToReturn, 10, 150);
                }

                return valueToReturn;
            }
            private set { }
        }


        public bool isSpot = false;
        public bool isAvailableFromAnyDirection = true;
        public bool isRandomizeColor = false;
        public DecorationInteractionType interactionType = DecorationInteractionType.Default;

        public Category category;



        public enum Category
        {
            Plants,
            Statues,
            Misc
        }
    }
}