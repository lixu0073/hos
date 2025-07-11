using UnityEngine;
using Hospital;
using IsoEngine;
using MovementEffects;
using System.Collections.Generic;


public class PlantationShed : MonoBehaviour
{

    Vector3 clickStartPosition;

#if UNITY_EDITOR || UNITY_STANDALONE
    private float sqrmargin = 100.0f;
#else
	private float sqrmargin = 400.0f;
#endif

    public Transform[] bouncingObject;
    int started_corountine = 0;

    void OnMouseDown()
    {
        clickStartPosition = Input.mousePosition;

        if (started_corountine == 0)// && !UIController.get.isAnyPopupActive())
        {
            BounceObject();
        }
    }

    void OnMouseUp()
    {
        if (!BaseCameraController.IsPointerOverInterface())
        {
            if ((clickStartPosition - Input.mousePosition).sqrMagnitude < sqrmargin)
            {
                OnClick();
            }
        }
    }

    void OnClick()
    {
        //IDEA we could introduce an info screen for the plantation.
        Debug.Log("Plantation Shed clicked");
        if (UIController.get.drawer.IsVisible)
        {
            UIController.get.drawer.SetVisible(false);
            return;
        }
        if (UIController.get.FriendsDrawer.IsVisible)
        {
            UIController.get.FriendsDrawer.SetVisible(false);
            return;
        }
        if (Game.Instance.gameState().GetHospitalLevel() < 15)
        {
            UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.Plantation);
        }

		SoundsController.Instance.PlayButtonClick2 ();

        if (HospitalAreasMapController.HospitalMap != null)
            HospitalAreasMapController.HospitalMap.HideTransformBorder();
    }


    void BounceObject()
    {
        if (bouncingObject != null && bouncingObject.Length > 0)
        {
            if (bouncingObject.Length == 1)
            {
                Timing.RunCoroutine(BounceCoroutine(bouncingObject[0]));
            }
            else
            {
                for (int i = 0; i < bouncingObject.Length; i++)
                {
                    if (i == 0)
                        Timing.RunCoroutine(BounceCoroutine(bouncingObject[i],false, i * 0.1f));
                    else
                        Timing.RunCoroutine(BounceCoroutine(bouncingObject[i],true, i * 0.1f));
                }
            }
        }
    }

    IEnumerator<float> BounceCoroutine(Transform objectToBounce, bool move = false, float delay = 0)
    {
        if (objectToBounce == null)
            yield return 0;

        yield return Timing.WaitForSeconds(delay);
        started_corountine++;
        //Debug.Log("BounceCoroutine");
        float bounceTime = .15f;
        float timer = 0f;

        Vector3 normalTransform, targetTransform;

        if (!move)
        {
            normalTransform = objectToBounce.localScale;
            targetTransform = normalTransform * 1.1f;
        }
        else
        {
            normalTransform = objectToBounce.position;
            targetTransform = new Vector3(objectToBounce.position.x, objectToBounce.position.y + 0.55f, objectToBounce.position.z); //new Vector3(1.1f, 1.1f, 1.1f);
        }

            while (timer < bounceTime)
            {
                timer += Time.deltaTime;

                if (!move)
                    objectToBounce.localScale = Vector3.Lerp(normalTransform, targetTransform, timer / bounceTime);
                else
                    objectToBounce.position = Vector3.Lerp(normalTransform, targetTransform, timer / bounceTime);

                yield return 0;
            }
            timer = 0f;
            //scale down
            while (timer < bounceTime)
            {
                timer += Time.deltaTime;

                if (!move)
                    objectToBounce.localScale = Vector3.Lerp(targetTransform, normalTransform, timer / bounceTime);
                else
                    objectToBounce.position = Vector3.Lerp(targetTransform, normalTransform, timer / bounceTime);

            yield return 0;
            }

            started_corountine--;
            yield return 0;

    }
}

