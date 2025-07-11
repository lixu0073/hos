using UnityEngine;
using System.Collections;
using System;

namespace SimpleUI
{

    public abstract class BaseHover : MonoBehaviour, IDiamondTransactionMaker
    {
        protected Guid ID;
        protected Guid EnlargeDiamondTransactionMakerID;
        protected Guid SpeedUpDiamondTransactionMakerID;
        protected static BaseHover actualHover;
        protected static BaseHover ActualHover
        {
            set
            {
                if (actualHover != null)
                    actualHover.Close();
                actualHover = value;
            }
        }
        protected HoverMode mode = HoverMode.StayAtPoint;
        protected Canvas canvas;
        protected Transform tr;
        protected RectTransform trans;
        protected Vector3 position;
        public RectTransform hoverFrame;
        private RectTransform parentRect;
        bool isAdjustedForBounds;
        private Coroutine smoothlyMove;
        private Coroutine smoothlyScale;

        private Vector3 startScale;

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all coroutines on this MonoBehaviour
        }

        protected virtual void Initialize()
        {
            InitializeID();
            //Debug.LogError("Initialize 1");
            if (gameObject.activeSelf)
                return;

            //Debug.LogError("Initialize 2");
            gameObject.SetActive(!UIController.get.IsHidingActiveHover);

            //if (UIController.get.drawer.IsVisible) {
            //	UIController.get.drawer.SetVisible (false);
            //}
            UIController.get.AddActiveHover(this);

            transform.SetParent(UIController.get.hoversParent);
            transform.SetAsLastSibling();

            if (!CampaignConfig.hintSystemEnabled && UIController.getHospital != null && UIController.getHospital.ObjectivesPanelUI.isSlidIn)
            {
                UIController.getHospital.ObjectivesPanelUI.SlideOut();
                UIController.getHospital.ObjectivesPanelUI.HideTemporary();
            }

            if (Hospital.AreaMapController.Map != null)
                Hospital.AreaMapController.Map.HideTransformBorder();
            smoothlyMove = null;
            smoothlyScale = null;
        }

        protected virtual void Init()
        {
            InitializeID();
            trans = GetComponent<RectTransform>();
            canvas = UIController.get.canvas;
            transform.SetParent(UIController.get.hoversParent);
            transform.SetAsLastSibling();

            if (UIController.getHospital != null && !CampaignConfig.hintSystemEnabled && UIController.getHospital.ObjectivesPanelUI.isSlidIn)
                UIController.getHospital.ObjectivesPanelUI.SlideOut();

            SetHoverScale();
            UIController.get.AddActiveHover(this);
            startScale = Vector3.one;

            Transform parent = transform.parent;
            if (parent != null)
            {
                parentRect = parent.GetComponent<RectTransform>();
            }
        }

        public virtual void SetHoverScale()
        {
            if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
                transform.localScale = Vector3.one * 0.8f;
            else
                transform.localScale = Vector3.one * 0.6f;
        }

        protected virtual void SetHoverFrame(int queueSize)
        {
            if (hoverFrame == null)
                return;
        }

        public virtual void Close()
        {
            EraseID();
            isAdjustedForBounds = false;
            /*
            if (Hospital.HintsController.Get().isHintArrowVisible)
            {
                if (TutorialUIController.Instance.IsAnyOfTutorialScreenClosedAndItsFreePlayStep())
                {
                    TutorialUIController.Instance.HideIndicator();
                    Hospital.HintsController.Get().isHintArrowVisible = false;
                    Hospital.HintsController.Get().currentHint = null;
                }
            }
            */

            if (smoothlyMove != null)
            {
                try
                {
                    StopCoroutine(smoothlyMove);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                smoothlyMove = null;
            }
            if (smoothlyScale != null)
            {
                try
                {
                    StopCoroutine(smoothlyScale);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                smoothlyScale = null;
            }
            //Set parent rect scale to normal size
            if (parentRect != null)
                parentRect.localScale = startScale;
            gameObject.SetActive(false);
            UIController.get.RemoveActiveHover(this);
            if (UIController.getHospital != null && !CampaignConfig.hintSystemEnabled && !UIController.getHospital.ObjectivesPanelUI.isSlidIn && UIController.getHospital.ObjectivesPanelUI.IsHiddenTemporary())
            {
                UIController.getHospital.ObjectivesPanelUI.SlideIn();
            }
        }

        protected virtual void Update()
        {
            //UpdateAccordingToMode();
        }

        protected virtual void LateUpdate()
        {
            if (smoothlyMove == null)
            {
                UpdateAccordingToMode();
            }
        }

        public virtual void UpdateAccordingToMode()
        {
            switch (mode)
            {
                case HoverMode.FollowGameObject:
                    SetWorldPosition(tr.position);
                    break;
                case HoverMode.StayAtPoint:
                    break;
                case HoverMode.StayAtWorldPoint:
                    SetWorldPosition(position);
                    break;
                default:
                    break;
            }
        }

        protected void SetPosition(Vector2 newPosition)
        {
            trans.anchoredPosition = newPosition;
        }

        protected void SetWorldPosition(Vector3 worldPosition)
        {
            Vector2 temp = ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(worldPosition);
            temp.x -= Screen.width / 2;
            temp.x /= canvas.transform.localScale.x;
            temp.y -= Screen.height / 2;
            temp.y /= canvas.transform.localScale.y;

            //SetPosition(ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(worldPosition));
            SetPosition(temp);
            AdjustForCameraBounds(temp);
        }

        void AdjustForCameraBounds(Vector2 currentPos)
        {
            ReferenceHolder.Get().engine.MainCamera.ForceCheckConstraints();

            //if hover frame is not in camera view and camera is already bound
            //then hover needs to be force set to position where it can be fully seen
            Vector3[] frameCorners = new Vector3[4];
            hoverFrame.GetWorldCorners(frameCorners);

#if UNITY_EDITOR && DEBUG
            DrawAreaLines(frameCorners);
#endif

            Vector3 minWorldPointSafeArea = Vector3.zero;
            Vector3 maxWorldPointSafeArea = Vector3.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(hoverFrame, Screen.safeArea.min, null, out minWorldPointSafeArea);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(hoverFrame, Screen.safeArea.max, null, out maxWorldPointSafeArea);

            bool isHoverOverflowingHorizontally = false;
            bool isHoverOverflowingVertically = false;
            float dist = 0f;
            Vector2 moveBy = Vector2.zero;
            bool scale = false;

            //check left
            dist = minWorldPointSafeArea.x - frameCorners[0].x;
            if (dist > 1)
            {
                isHoverOverflowingHorizontally = true;
                moveBy.x += dist;
            }
            //check right
            dist = maxWorldPointSafeArea.x - frameCorners[2].x;
            if (dist < -1)
            {
                isHoverOverflowingHorizontally = true;
                moveBy.x += dist;
            }
            //check bottom
            dist = minWorldPointSafeArea.y - frameCorners[0].y;
            if (dist > 1)
            {
                isHoverOverflowingVertically = true;
                moveBy.y += dist;
            }
            //check top
            dist = maxWorldPointSafeArea.y - frameCorners[2].y;
            if (dist < -1)
            {
                isHoverOverflowingVertically = true;
                moveBy.y += dist;
            }
            //If the hover is bigger than safe area, scale it smaller
            if (Mathf.Abs(minWorldPointSafeArea.y - maxWorldPointSafeArea.y) < Mathf.Abs(frameCorners[0].y - frameCorners[2].y)
                || Mathf.Abs(minWorldPointSafeArea.x - maxWorldPointSafeArea.x) < Mathf.Abs(frameCorners[0].x - frameCorners[2].x))
            {
                scale = true;
            }
            if (scale && smoothlyScale == null)
            {
                smoothlyScale = StartCoroutine(SetScale());
            }

            //if hover is outside camera bounds and cammera cannot move any further: adjust
            if (isHoverOverflowingHorizontally && (isAdjustedForBounds || ReferenceHolder.Get().engine.MainCamera.IsMoveRestrictedHorizontally) && smoothlyMove == null && !scale)
            {
                isAdjustedForBounds = true;
                smoothlyMove = StartCoroutine(SetPositionAfterEdit(new Vector2(0, currentPos.y)));
            }
            else if (isHoverOverflowingVertically && (isAdjustedForBounds || ReferenceHolder.Get().engine.MainCamera.IsMoveRestrictedVertically) && smoothlyMove == null && !scale)
            {
                isAdjustedForBounds = true;
                smoothlyMove = StartCoroutine(SetPositionAfterEdit(new Vector2(currentPos.x, 0)));
            }
        }

        private IEnumerator SetPositionAfterEdit(Vector2 newPosition)
        {
            trans.anchoredPosition = newPosition;
            yield return null;
        }
        private IEnumerator SetScale()
        {
            parentRect.localScale = Vector3.one * 0.8f;
            yield return null;
        }

#if UNITY_EDITOR && DEBUG
        private void DrawAreaLines(Vector3[] frameCorners)
        {
            Vector2 min = Screen.safeArea.min;
            Vector2 max = Screen.safeArea.max;
            LineRenderer lineRenderer = new GameObject("LineRendCorners").AddComponent(typeof(LineRenderer)) as LineRenderer;
            LineRenderer lineRendererSafeArea = new GameObject("LineRendSafeArea").AddComponent(typeof(LineRenderer)) as LineRenderer;
            lineRenderer.positionCount = 5;
            lineRendererSafeArea.positionCount = 5;
            lineRenderer.startWidth = 20;
            lineRendererSafeArea.startWidth = 20;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material.color = Color.red;
            lineRendererSafeArea.material = new Material(Shader.Find("Sprites/Default"));
            lineRendererSafeArea.material.color = Color.green;
            Destroy(lineRenderer.gameObject, 1f);
            Destroy(lineRendererSafeArea.gameObject, 1f);

            for (var i = 0; i < frameCorners.Length; i++)
            {
                lineRenderer.SetPosition(i, new Vector3(frameCorners[i].x, frameCorners[i].y, 0));
            }
            lineRenderer.SetPosition(4, new Vector3(frameCorners[0].x, frameCorners[0].y, 0));

            Vector3[] frameCornersIn2Ds = new Vector3[]
            {
                new Vector3(min.x, min.y),
                new Vector3(min.x, max.y),
                new Vector3(max.x, max.y),
                new Vector3(max.x, min.y),
                new Vector3(min.x, min.y)
            };

            // Set the positions of the LineRenderer's vertices to the corner points
            for (int i = 0; i < 5; i++)
            {
                Vector3 point = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(hoverFrame, frameCornersIn2Ds[i], null, out point);
                lineRendererSafeArea.SetPosition(i, new Vector3(point.x, point.y, 0));
            }
        }
#endif
        public void SetFollowingObject(GameObject obj)
        {
            print("setting following");
            tr = obj.transform;
            mode = HoverMode.FollowGameObject;
        }

        public void SetWorldPointHovering(Vector3 pos)
        {
            position = pos;
            mode = HoverMode.StayAtWorldPoint;
        }

        public void SetScreenPointHovering(Vector2 pos)
        {
            mode = HoverMode.StayAtPoint;

            Debug.LogError("SetScreenPointHovering " + pos);
            SetPosition(pos);
        }

        public void InitializeID()
        {
            ID = Guid.NewGuid();
        }

        public Guid GetID()
        {
            return ID;
        }

        public void EraseID()
        {
            ID = Guid.Empty;
        }
    }


    public enum HoverMode
    {
        FollowGameObject = 0,
        StayAtPoint = 1,
        StayAtWorldPoint = 2
    }
}
