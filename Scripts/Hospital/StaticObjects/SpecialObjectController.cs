using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using IsoEngine;
using System;
using MovementEffects;


namespace Hospital
{
    public class SpecialObjectController : MonoBehaviour//,IPointerClickHandler
    {
        CapsuleCollider kolider;
        private Vector3 firstMousePos;
        private float touchTime;
        public ISpecialObject specialObj;

        public BouncingData[] bounceBeforeOpenObjectsList;
        public BouncingData[] bounceAfterOpenObjectsList;
        public float bounceDelay = 0.1f;

        public GameObject particleObject;
        public bool particleEmmisionOnStart = false;

        int started_corountine = 0;


        public void OnMouseUp()
        {
            // Debug.LogError("OnMouseUp " + BaseCameraController.IsPointerOverInterface() + " ,  " + Input.mousePosition);
            if (!BaseCameraController.IsPointerOverInterface()) {
                if ((Input.mousePosition - firstMousePos).magnitude < 10.0f)// && Time.time - touchTime < 0.5f)
                {
                    if (specialObj == null)
                    {
                        specialObj = GetComponent<ISpecialObject>();
                    }
                    try
                    {
                        specialObj.Clicked();
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("Something wrong with clicking in specialObjectController: " + e.Message + "\n" + e.StackTrace);
                    }
                    UpdateParticle();
                }

                //bounce moved from OnMouseDown
                if (started_corountine == 0)// && !UIController.get.isAnyPopupActive())
                {
                    if (gameObject.GetComponent<ExternalRoom>())
                    {
                        if (gameObject.GetComponent<ExternalRoom>().ExternalHouseState == ExternalRoom.EExternalHouseState.waitingForRenew || 
                            gameObject.GetComponent<ExternalRoom>().ExternalHouseState == ExternalRoom.EExternalHouseState.disabled)
                        {
                            BounceObject(false);
                        } 
                        else
                            BounceObject(true);
                    } else if (gameObject.GetComponent<Epidemy>())
                    {
                        if (!gameObject.GetComponent<Epidemy>().Unwrapped)
                        {
                            BounceObject(false);
                        }
                        else
                        BounceObject(true);
                    }
                    else
                        BounceObject(false);
                }
            }
            firstMousePos = Vector3.zero;
        }

        public void UpdateParticle()
        {
            if (particleObject != null && particleObject.GetComponent<ParticleSystem>())
            {
                //if (particleEmmisionOnStart)
                    particleObject.GetComponent<ParticleSystem>().Play();
               // else particleObject.GetComponent<ParticleSystem>().Stop();
            }
        }

        public void OnMouseDown()
        {
            touchTime = Time.time;
            firstMousePos = Input.mousePosition;

            if (AreaMapController.Map != null)
                AreaMapController.Map.HideTransformBorder();
        }

        // Use this for initialization
        void Start()
        {
            kolider = gameObject.AddComponent<CapsuleCollider>();
            gameObject.AddComponent<EventTrigger>();
            specialObj = gameObject.GetComponent<ISpecialObject>();
            //specialObj.SetController(this);
            float temp1, temp2;
            int dir;

            kolider.enabled = true;
            kolider.center = specialObj.GetColliderInfo(out temp1, out temp2, out dir);
            kolider.radius = temp1;
            kolider.direction = dir;
            kolider.height = temp2;
        }

        public void ChangeCollider(Vector2 center, float radius, float height, int direction = -1)
        {
            kolider.radius = radius;
            kolider.height = height;
            kolider.center = new Vector3(center.x, 0, center.y);
            if (direction >= 0)
                kolider.direction = direction;
        }

        void BounceObject(bool isOpened = true)
        {
            if (!isOpened)
            {
                if (bounceBeforeOpenObjectsList != null && bounceBeforeOpenObjectsList.Length > 0)
                {
                    if (bounceBeforeOpenObjectsList.Length == 1)
                    {
                        if (bounceBeforeOpenObjectsList[0].bounceObject != null && bounceBeforeOpenObjectsList[0].bounceObject.gameObject.activeSelf)
                            Timing.RunCoroutine(BounceCoroutine(bounceBeforeOpenObjectsList[0].bounceObject, bounceBeforeOpenObjectsList[0].isScale));
                    }
                    else
                    {
                        for (int i = 0; i < bounceBeforeOpenObjectsList.Length; i++)
                        {
                            if (bounceBeforeOpenObjectsList[i].bounceObject!=null && bounceBeforeOpenObjectsList[i].bounceObject.gameObject.activeSelf)
                                Timing.RunCoroutine(BounceCoroutine(bounceBeforeOpenObjectsList[i].bounceObject, bounceBeforeOpenObjectsList[i].isScale, i * bounceDelay));
                        }
                    }
                }
            }
            else if (bounceAfterOpenObjectsList != null && bounceAfterOpenObjectsList.Length > 0)
            {
                if (bounceAfterOpenObjectsList.Length == 1)
                {
                    if (bounceAfterOpenObjectsList[0].bounceObject != null && bounceAfterOpenObjectsList[0].bounceObject.gameObject.activeSelf)
                        Timing.RunCoroutine(BounceCoroutine(bounceAfterOpenObjectsList[0].bounceObject, bounceAfterOpenObjectsList[0].isScale, 0));
                }
                else
                {
                    for (int i = 0; i < bounceAfterOpenObjectsList.Length; i++)
                    {
                        if (bounceAfterOpenObjectsList[i].bounceObject != null && bounceAfterOpenObjectsList[i].bounceObject.gameObject.activeSelf)
                            Timing.RunCoroutine(BounceCoroutine(bounceAfterOpenObjectsList[i].bounceObject, bounceAfterOpenObjectsList[i].isScale, i * bounceDelay));
                    }
                }
            }

            SoundsController.Instance.PlayButtonClick2();
        }

        IEnumerator<float> BounceCoroutine(Transform objectToBounce, bool isScale = false, float delay = 0)
        {
            if (objectToBounce == null)
                yield return 0;

            started_corountine++;
            yield return Timing.WaitForSeconds(delay);

            //Debug.Log("BounceCoroutine");
            float bounceTime = .15f;
            float timer = 0f;

            Vector3 normalTransformation, targetTransformation;

            if (isScale)
            {
                normalTransformation = objectToBounce.localScale;
                targetTransformation = normalTransformation * 1.1f;
            }
            else
            {
                normalTransformation = objectToBounce.position;
                targetTransformation = new Vector3(objectToBounce.position.x, objectToBounce.position.y + 0.55f, objectToBounce.position.z);
            }

            //scale up
            if (normalTransformation != Vector3.zero && targetTransformation != Vector3.zero)
            {
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;

                    if (isScale)
                        objectToBounce.localScale = Vector3.Lerp(normalTransformation, targetTransformation, timer / bounceTime);
                    else
                        objectToBounce.position = Vector3.Lerp(normalTransformation, targetTransformation, timer / bounceTime);

                    yield return 0;
                }
                timer = 0f;
                //scale down
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;

                    if (isScale)
                        objectToBounce.localScale = Vector3.Lerp(targetTransformation, normalTransformation, timer / bounceTime);
                    else
                        objectToBounce.position = Vector3.Lerp(targetTransformation, normalTransformation, timer / bounceTime);

                    yield return 0;
                }

                started_corountine--;
                yield return 0;
            }
            else
            {
                started_corountine--;
                yield return 0;
            }
        }
    }

    public interface ISpecialObject
    {
        void Clicked();
        //void SetController(SpecialObjectController controller);
        Vector3 GetColliderInfo(out float radius, out float height, out int direction);
        void SetLevel(int lvl, bool showParticles = true);
    }

    public abstract class SuperObjectWithVisiting : SuperObject
    {
        public override void Clicked()
        {
			if (!visitingMode)
                base.Clicked();
			//SoundsController.Instance.PlayButtonClick2 ();
        }
    }

    public abstract class SuperObject : MonoBehaviour, ISpecialObject
    {
        //SpecialObjectController controller;
        public static bool visitingMode;
        public float ColliderRadius;
        public float ColliderHeight;
        public Vector3 ColliderCenter;
        public int ColliderDirection;
        public int actualLevel { get; protected set; }

        public Vector3 GetColliderInfo(out float radius, out float height, out int direction)
        {
            radius = ColliderRadius;
            height = ColliderHeight;
            direction = ColliderDirection;
            return ColliderCenter;
        }

        public virtual void Clicked()
        {
            OnClick();
			//SoundsController.Instance.PlayButtonClick2 ();
        }
        public abstract void OnClick();

        public virtual void SetLevel(int lvl, bool showParticles = true)
        {
            if (lvl > actualLevel)
                NotificationCenter.Instance.StaticObjectUpgraded.Invoke(new StaticObjectUpgradedEventArgs());
        }

        public abstract void IsoDestroy();
    }

    [Serializable]
    public class BouncingData
    {
        public Transform bounceObject;
        public bool isScale = false;
    }
}
