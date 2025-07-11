using UnityEngine.UI;
using SimpleUI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MovementEffects;

namespace Hospital
{
    public class DrawerDraggableItem : MonoBehaviour
    {
        IDrawer drawer;
        Image image;
        ShopRoomInfo info;
        Vector3 mousePos;
        Rotations rot;
        private OnEvent onEnd;
        private OnEvent onFast;

        private DraggableItemState itemState;
        private DraggableItemType itemType;

        private Material defaultMat;
        private Material colorMat;
        private Vector4 color;
        private Vector2 size;

        Coroutine _destroyDelay;

        private bool isMovementActive = true;
#pragma warning disable 0649
        [SerializeField] private GameObject defaultObject;
        [SerializeField] private GameObject interactiveObject;
        [SerializeField] private ParticleSystem interactiveParticle;
        [SerializeField] private ParticleSystem interactiveParticle2;
#pragma warning restore 0649

        private Vector3 dragingPos;
        private Vector3 lastMousePos;
        private Vector3 screenPos;

        //[SerializeField]
        //private ParticleSystem interactiveParticleSubEmitter;
#pragma warning disable 0649
        [SerializeField] private float interactiveDestroyDelay;
        [SerializeField] private float interactiveRotateSpeed;
        [SerializeField] private float interactiveRotateAngle;
        [SerializeField] private Vector2 interactiveOffset;
#pragma warning restore 0649
        private Vector3 offset = Vector3.zero;

        private IEnumerator<float> rorateCoroutine = null;

        private void OnDisable()
        {
            if (_destroyDelay != null)
            {
                try
                {
                    StopCoroutine(_destroyDelay);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
            }
        }

        public void Initialize(ShopRoomInfo info, IDrawer drawer, Rotations rot, OnEvent oneEnd = null, OnEvent onFastEnd = null)
        {
            Canvas canvas = UIController.get.canvas;

            transform.localScale = Vector3.one * 1.5f * UIController.get.canvas.transform.localScale.y;

            float zoomScaleValue = (1f - Mathf.Clamp(ReferenceHolder.Get().engine.MainCamera.GetZoom() / ReferenceHolder.Get().engine.MainCamera.GetMaxZoom(), 0.1f, 0.6f)) * 2;

            interactiveOffset = new Vector2(interactiveOffset.x * UIController.get.canvas.transform.localScale.x * zoomScaleValue, interactiveOffset.y * UIController.get.canvas.transform.localScale.y * zoomScaleValue);

            transform.position = Input.mousePosition;
            this.size = GetComponent<RectTransform>().sizeDelta;
            this.image = null;
            this.isMovementActive = true;

            if (info.DynamicShopImage != null)
            {
                itemState = DraggableItemState.Interactive;
                defaultObject.SetActive(false);
                interactiveObject.SetActive(true);
                this.image = interactiveObject.transform.GetChild(0).GetComponent<Image>();

                if (info.GetType() == typeof(CanInfo))
                {
                    itemType = DraggableItemType.Can;
                    color = (info as CanInfo).canColor;
                }

                this.image.material.SetVector("_HSVAAdjust", new Vector4(color.x, color.y, color.z, color.w));
            }
            else
            {
                itemState = DraggableItemState.Default;
                itemType = DraggableItemType.Default;
                defaultObject.SetActive(true);
                interactiveObject.SetActive(false);
                this.image = defaultObject.GetComponent<Image>();
                this.image.material.SetVector("_HSVAAdjust", new Vector4(0, 0, 0, 0));
            }

            this.image.sprite = info.ShopImage;
            defaultMat = Instantiate(ResourcesHolder.Get().drawerDragableDefaultColorMaterial);
            colorMat = this.image.material;

            this.info = info;

            this.drawer = drawer;
            this.rot = rot;
            mousePos = Input.mousePosition;
            this.onEnd = oneEnd;
            onFast = onFastEnd;
        }

        // Update is called once per frame
        void Update()
        {
            if (this.isMovementActive)
            {
                lastMousePos = Input.mousePosition;

                if (Input.GetMouseButtonUp(0))
                {
                    this.isMovementActive = false;

                    dragingPos = drawer.DragingObjectPosition();
                    screenPos = (ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(dragingPos) - lastMousePos);

                    if (itemState == DraggableItemState.Changed && !drawer.IsDragingObjectDummy())
                    {
                        if (itemType == DraggableItemType.Can)
                        {
                            if (ReferenceHolder.Get().floorControllable.GetCurrentFloorColorName(drawer.DragingObjectArea()) != info.Tag)
                            {
                                rorateCoroutine = Timing.RunCoroutine(Rotate(interactiveRotateAngle, interactiveRotateSpeed));
                                interactiveParticle2.Play();
                                interactiveParticle.Stop();
                                _destroyDelay = StartCoroutine(DestroyDelay(ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(lastMousePos)));
                            }
                            else Destroy(gameObject);
                        }
                        else Destroy(gameObject);
                    }
                    else
                        Destroy(gameObject);

                    onFast?.Invoke();
                }
                transform.position = Input.mousePosition + offset;

                //Debug.LogError("MATERNITY DRAWER HACK");
                if ((Input.mousePosition - mousePos).x > 50 || !((MonoBehaviour)drawer).isActiveAndEnabled)
                {
                    drawer.GenerateSimpleRotatableItem(rot);

                    switch (itemState)
                    {
                        case DraggableItemState.Interactive:

                            this.image.sprite = info.DynamicShopImage;
                            interactiveObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                            if (itemType == DraggableItemType.Can) // Can object can be set from shader
                            {
                                this.image.material.SetVector("_HSVAAdjust", new Vector4(color.x, color.y, color.z, color.w));
                                interactiveParticle.GetComponent<ParticleSystemRenderer>().sharedMaterial.SetVector("_HSVAAdjust", new Vector4(color.x, color.y, color.z, color.w));
                                //interactiveParticleSubEmitter.GetComponent<ParticleSystemRenderer>().sharedMaterial.SetVector("_HSVAAdjust", new Vector4(color.x, color.y, color.z, color.w));
                            }

                            itemState = DraggableItemState.Changed;
                            offset = new Vector3(interactiveOffset.x, interactiveOffset.y, 0);
                            interactiveParticle.Play();
                            break;
                        case DraggableItemState.Changed:

                            if (itemType == DraggableItemType.Can) // Can object can be set from shader
                            {
                                if (drawer.IsDragingObjectDummy()) // if is dummy then delete shader and set color
                                {
                                    interactiveParticle.Stop();

                                    this.image.material = defaultMat;
                                    this.image.material.color = new Color(249f / 255f, 47f / 255f, 51f / 255f, 1f);
                                }
                                else
                                {
                                    if (!interactiveParticle.isPlaying)
                                        interactiveParticle.Play();

                                    this.image.material = colorMat;
                                    this.image.material.SetVector("_HSVAAdjust", new Vector4(color.x, color.y, color.z, color.w));
                                }
                            }
                            break;
                        default:
                            Destroy(gameObject);
                            onEnd?.Invoke();
                            break;
                    }

                }
                else if (itemState == DraggableItemState.Interactive && itemType == DraggableItemType.Can)
                    this.image.material.SetVector("_HSVAAdjust", new Vector4(color.x, color.y, color.z, color.w));
            }
            else
            {
                transform.position = Input.mousePosition + offset;

                if (gameObject != null)
                {
                    if (dragingPos != Vector3.zero)
                        transform.position = ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(dragingPos) - screenPos + offset;
                }
            }
        }

        IEnumerator<float> Rotate(float val, float speed)
        {
            if (gameObject != null)
            {
                float rotZ = this.transform.localRotation.z;
                while (rotZ > val)
                {
                    this.transform.rotation = Quaternion.Euler(0, 0, rotZ);
                    rotZ -= speed;
                    yield return Timing.WaitForSeconds(Time.deltaTime);
                }
                yield return Timing.WaitForSeconds(0.3f);

                while (rotZ <= 40)
                {
                    rotZ += 8f;
                    this.transform.rotation = Quaternion.Euler(0, 0, rotZ);
                    yield return Timing.WaitForSeconds(Time.deltaTime);
                }
            }

            yield return 0;
        }

        IEnumerator DestroyDelay(Vector3 position)
        {
            yield return new WaitForEndOfFrame();

            bool wasPopupOpen = false;

            while (UIController.get.BuyResourcesPopUp.IsVisible)
            {
                this.image.gameObject.SetActive(false);
                wasPopupOpen = true;

                if (rorateCoroutine != null)
                {
                   Timing.KillCoroutine(rorateCoroutine);
                    rorateCoroutine = null;
                    transform.position = ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(dragingPos) - screenPos + offset;
                    this.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                yield return new WaitForSeconds(0.5f);
            }

            if (wasPopupOpen)
            {
                rorateCoroutine = Timing.RunCoroutine(Rotate(interactiveRotateAngle, interactiveRotateSpeed));
                interactiveParticle2.Play();
                interactiveParticle.Stop();
                wasPopupOpen = false;
                this.image.gameObject.SetActive(true);
            }

            if (ReferenceHolder.Get().floorControllable.CanAnimationRefresh)
            {
                GameObject painting = GameObject.Instantiate(ResourcesHolder.GetHospital().floorPainting);

                float zoomScaleValue = 1 + ReferenceHolder.Get().engine.MainCamera.GetZoomScale();

                painting.GetComponent<FloorPainting>().InitializeWithColor(color, zoomScaleValue);
                painting.transform.position = position;

                this.gameObject.transform.SetAsFirstSibling();
                yield return new WaitForSeconds(interactiveDestroyDelay);
            }

            if (rorateCoroutine != null)
            {
                Timing.KillCoroutine(rorateCoroutine);
                rorateCoroutine = null;
            }
            Destroy(gameObject);
            yield return null;
        }

        public enum DraggableItemState
        {
            Default,
            Interactive,
            Changed,
        }

        public enum DraggableItemType
        {
            Default,
            Can,
            Other,
        }
    }
}