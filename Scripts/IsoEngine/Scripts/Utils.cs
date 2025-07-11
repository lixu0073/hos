using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

namespace IsoEngine
{

    public class Utils : MonoBehaviour
    {
        private static Utils _instance = null;
        public static Utils Instance
        {
            get
            {
                if (_instance == null)
                {
                    var g = new GameObject("UtilsObject");
                    DontDestroyOnLoad(g);
                    _instance = g.AddComponent<Utils>();
                }
                return _instance;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public static bool IsPointerOverInterface()
        {
            return (EventSystem.current.IsPointerOverGameObject() || (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)));
        }

        public static Vector2 ScreenToCanvasPosition(Vector2 screenPos)
        {
            var canvas = UIController.get.canvas;
            return new Vector2((screenPos.x - Screen.width / 2) / canvas.transform.localScale.x, (screenPos.y - Screen.height / 2) / canvas.transform.localScale.y);
        }
        public static Vector2 ScreenToCanvasPosition(Vector2i screenPos)
        {
            var canvas = UIController.get.canvas;
            return new Vector2((screenPos.x - Screen.width / 2) / canvas.transform.localScale.x, (screenPos.y - Screen.height / 2) / canvas.transform.localScale.y);
        }
        public static Vector2 GetScreenPosition(Vector3 worldPos)
        {
            var canvas = UIController.get.canvas;
            Vector2 temp = ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(worldPos);
            temp.x -= Screen.width / 2;
            temp.x /= canvas.transform.localScale.x;
            temp.y -= Screen.height / 2;
            temp.y /= canvas.transform.localScale.y;
            return temp;
        }

        public static bool DoesAnimatorParamExists(Animator anim, string valueName) {
            bool yupItsThere = false;
            try
            {
                foreach (AnimatorControllerParameter x in anim.parameters)
                {
                    if (x.name == valueName)
                    {
                        yupItsThere = true;
                    }
                }
                if (yupItsThere)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e) {
                Debug.Log("Parameter fidning failed: " + e.Message);
            }
            return false;
        }

    }
}
