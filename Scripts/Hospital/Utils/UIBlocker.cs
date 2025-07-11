using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;


/// <summary>
/// UI Blocker to prevent every object in screen to react to tap event except for those ones included in excludedGO list (must have button component and
/// be in UI layer). Also the object containing it must have an Image component with Raycast Target set.
/// </summary>
public class UIBlocker : MonoBehaviour
{
    public GameObject[] excludedGO = null;  // List of objects that will react to tap event
    private List<RaycastResult> hitRes = new List<RaycastResult>();
    private PointerEventData pointerData = null;
    private Button buttonComp = null;

    public void Update()
    {
        RaycastWorldUI();
    }

    private void RaycastWorldUI()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            
            EventSystem.current.RaycastAll(pointerData, hitRes);

            if (hitRes.Count > 0 && excludedGO != null)
            {                
                foreach (GameObject go in excludedGO)
                {
                    foreach (RaycastResult hit in hitRes)
                    {
                        if (hit.gameObject == go && hit.gameObject.layer == LayerMask.NameToLayer("UI"))
                        {
                            hitRes.Clear();
                            buttonComp = go.GetComponent<Button>();
                            buttonComp?.OnPointerClick(pointerData);
                            break;
                        }
                    }
                }
            }
        }
    }

}
