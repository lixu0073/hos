using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using System.Linq;
using MovementEffects;
using SimpleUI;

namespace Hospital
{
    public class Decoration : RotatableObject
    {
        public bool isPatientUsing;
        public Rotation spotRotation;
        public DecorationInteractionType interactionType;

        public bool isSpot;
        private bool hasColorRandomization;
        private float randomizedColor;

        private Vector2i size = new Vector2i(1, 1);
        private bool isAvailableFromAnyDirection = false;

        private Vector3[] spostsPositions;
        private bool[] spotsFull;

        private Vector3 normalScale = Vector3.zero;
        private Vector3 targetScale = Vector3.zero;

        public bool shouldWork
        {
            get;
            private set;
        }

        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            base.Initialize(info, position, rotation, _state, shouldDisappear);
            UpdateColor();
        }

        public override void StartBuilding()
        {
            StartBuildingDeco();

            if (!TutorialController.Instance.IsNonLinearStepCompleted(StepTag.NL_newspaper_patio_decos) && !VisitingController.Instance.IsVisiting)
            {
                List<Decoration> decos = FindObjectsOfType<Decoration>().ToList();
                if (decos.Count >= 10)
                {
                    decos = decos.Where((x) => x.info.infos.Area == HospitalArea.Patio).ToList();
                    if (decos.Count >= 10)
                        NotificationCenter.Instance.TenPatioDecorations.Invoke(new BaseNotificationEventArgs());
                }
            }
        }

        private void StartBuildingDeco()
        {
            if (state != State.fresh)
                return;

            var val = Anchored;
            SetAnchored(false);
            RemoveFromMap();
            state = State.working;
            AddToMap();
            SetAnchored(val);
            UpdateColor();
        }

        protected override void AddToMap()
        {
            base.AddToMap();
            isPatientUsing = false;

            this.isSpot = ((DecorationInfo)info.infos).isSpot;
            this.hasColorRandomization = ((DecorationInfo)info.infos).isRandomizeColor;

            this.isAvailableFromAnyDirection = ((DecorationInfo)info.infos).isAvailableFromAnyDirection;
            this.interactionType = ((DecorationInfo)info.infos).interactionType;
            this.spotRotation = actualRotation;

            switch ((Rotation)spotRotation)
            {
                case Rotation.North:
                    this.size = GetPrefabSize(((DecorationInfo)info.infos).NorthPrefab);
                    break;
                case Rotation.South:
                    this.size = GetPrefabSize(((DecorationInfo)info.infos).SouthPrefab);
                    break;
                case Rotation.East:
                    this.size = GetPrefabSize(((DecorationInfo)info.infos).EastPrefab);
                    break;
                case Rotation.West:
                    this.size = GetPrefabSize(((DecorationInfo)info.infos).WestPrefab);
                    break;
                default:
                    break;
            }


            var prefabOnScene = AreaMapController.Map.GetObject(position);

            if ((prefabOnScene != null) && (prefabOnScene.transform.childCount > 0) && (prefabOnScene.transform.GetChild(0).gameObject.transform.childCount > 1) && this.isSpot)
            {
                spostsPositions = new Vector3[prefabOnScene.transform.GetChild(0).gameObject.transform.childCount - 1];
                spotsFull = new bool[prefabOnScene.transform.GetChild(0).gameObject.transform.childCount - 1];
                int id = 0;

                foreach (Transform t in prefabOnScene.transform.GetChild(0).gameObject.transform)
                {
                    if (id != 0)
                    {
                        spostsPositions[id - 1] = t.position;
                        spotsFull[id - 1] = false;
                    }
                    id++;
                }
            }

            if (AreaMapController.Map.AddDecorationToMap(this) && info.infos.Area != HospitalArea.Laboratory)
            {
                UpdateColor();
            }

        }

        public Vector2i GetPrefabSize(GameObject tilePRefab)
        {
            return new Vector2i(tilePRefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesX, tilePRefab.gameObject.GetComponent<IsoObjectPrefabController>().prefabData.tilesY);
        }

        public Vector2i GetDecorationSpot(out Rotation rot)
        {

            if (this.isAvailableFromAnyDirection)
            {
                // UnityEngine.Random.seed = DateTime.Now.Second;
                spotRotation = (Rotation)(BaseGameState.RandomNumber(0, 4));
            }

            switch ((Rotation)spotRotation)
            {
                case Rotation.North:
                    if (!AreaMapController.Map.isAnyObjectExistOnPosition(position.x + size.x, position.y))
                    {
                        rot = spotRotation;
                        return new Vector2i(position.x + size.x, position.y);
                    }
                    break;
                case Rotation.South:
                    if (!AreaMapController.Map.isAnyObjectExistOnPosition(position.x - 1, position.y))
                    {
                        rot = spotRotation;
                        return new Vector2i(position.x - 1, position.y);
                    }
                    break;
                case Rotation.East:
                    if (!AreaMapController.Map.isAnyObjectExistOnPosition(position.x, position.y - 1))
                    {
                        rot = spotRotation;
                        return new Vector2i(position.x, position.y - 1);
                    }
                    break;
                case Rotation.West:
                    if (!AreaMapController.Map.isAnyObjectExistOnPosition(position.x, position.y + size.y))
                    {
                        rot = spotRotation;
                        return new Vector2i(position.x, position.y + size.y);
                    }
                    break;
                default:
                    break;
            }

            rot = spotRotation;
            return Vector2i.zero;
        }

        public int GetSpot(out Vector3 spotPos)
        {
            if (spotsFull != null && spotsFull.Length > 0)
            {
                for (int id = 0; id < spotsFull.Length; id++)
                {
                    if (spotsFull[id] == false)
                    {
                        spotsFull[id] = true;
                        spotPos = spostsPositions[id];
                        return id;
                    }
                }
            }
            spotPos = transform.position;
            return -1;
        }

        public void FreeSpot(int id)
        {
            if (spotsFull != null && spotsFull.Length > 0)
            {
                spotsFull[id] = false;
            }
        }

        public bool isAnySpotIsEmpty()
        {
            if (spotsFull != null && spotsFull.Length > 0)
            {
                for (int id = 0; id < spotsFull.Length; id++)
                {
                    if (spotsFull[id] == false)
                        return true;
                }
            }

            return false;
        }

        public override void SetAnchored(bool value)
        {
            base.SetAnchored(value);

            if (((DecorationInfo)info.infos).Area == HospitalArea.Laboratory)
                return;

            foreach (var p in BasePatientAI.patients)
            {
                if (p != null)
                {
                    p.Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
                }
            }

            shouldWork = obj == null && Anchored;

            if (value)
                NotificationCenter.Instance.PatioDecorationsAdded.Invoke(new BaseNotificationEventArgs());
        }

        public override void IsoDestroy()
        {
            AreaMapController.Map.RemoveDecorationFromMap(this);
            base.IsoDestroy();
        }


        protected override void OnClickWorking()
        {
            if (UIController.get.drawer.IsVisible || (UIController.get.FriendsDrawer != null && UIController.get.FriendsDrawer.IsVisible))
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            base.OnClickWorking();


            AreaMapController.Map.ChangeOnTouchType((x) =>
            {
                // empty needed to unclick other Hovers
                AreaMapController.Map.ResetAllInputActions();
            });

            if (AreaMapController.Map != null)
                AreaMapController.Map.HideTransformBorder();

            BounceObject();
            if (onClickVisualEffects != null)
            {
                onClickVisualEffects.RunVisualEffects(10);
            }
        }

        public override void MoveTo(int x, int y)
        {
            base.MoveTo(x, y);
            UpdateColor(false);
        }

        public override void MoveTo(int x, int y, Rotation beforeRotation)
        {
            base.MoveTo(x, y, beforeRotation);
            UpdateColor(false);
        }

        public override void RotateRight()
        {
            base.RotateRight();
            UpdateColor();
        }

        /* //FOR TESTING - EDITING SPOTS IN REAL TIME
        public void Update()
        {
            var prefabOnScene = HospitalAreasMapController.Map.GetObject(position);

            if ((prefabOnScene != null) && (prefabOnScene.transform.childCount > 0) && (prefabOnScene.transform.GetChild(0).gameObject.transform.childCount > 1) && this.isSpot)
            {
                spostsPositions = new Vector3[prefabOnScene.transform.GetChild(0).gameObject.transform.childCount - 1];
                spotsFull = new bool[prefabOnScene.transform.GetChild(0).gameObject.transform.childCount - 1];
                int id = 0;

                foreach (Transform t in prefabOnScene.transform.GetChild(0).gameObject.transform)
                {
                    if (id != 0)
                    {
                        spostsPositions[id - 1] = t.position;
                        spotsFull[id - 1] = false;
                    }
                    id++;
                }
            }
        }
        */

        void BounceObject()
        {
            Timing.RunCoroutine(BounceCoroutine());
        }

        MaterialPropertyBlock propBlock = null;
        void UpdateColor(bool randomizeColor = true)
        {
            if (!hasColorRandomization || IsDummy)
                return;

            if (randomizeColor == true)
                randomizedColor = BaseGameState.RandomFloat(0f, 255f);

            bool firstEl = true;
            foreach (Transform t in isoObj.GetGameObject().transform.GetChild(0))
            {
                if (firstEl != true)
                {
                    if (propBlock == null)
                        propBlock = new MaterialPropertyBlock();
                    var renderer = t.GetComponent<SpriteRenderer>();
                    renderer.GetPropertyBlock(propBlock);
                    propBlock.SetVector("_HSVAAdjust", new Vector4(randomizedColor, 0, 0, 0));
                    renderer.SetPropertyBlock(propBlock);
                }
                else firstEl = false;
            }
        }

        IEnumerator<float> BounceCoroutine()
        {
            //Debug.Log("BounceCoroutine");
            float bounceTime = .1f;
            float timer = 0f;
            Transform targetTransform;
            targetTransform = isoObj.GetGameObject().transform.GetChild(0);

            if (normalScale == Vector3.zero)
                normalScale = targetTransform.localScale;

            if (targetScale == Vector3.zero)
                targetScale = targetTransform.localScale * 1.1f; //new Vector3(1.1f, 1.1f, 1.1f);

            //scale up
            if (normalScale != Vector3.zero && normalScale != Vector3.zero)
            {
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;
                    targetTransform.localScale = Vector3.Lerp(normalScale, targetScale, timer / bounceTime);
                    yield return 0;
                }
                timer = 0f;
                //scale down
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;
                    targetTransform.localScale = Vector3.Lerp(targetScale, normalScale, timer / bounceTime);
                    yield return 0;
                }
            }
            else yield return 0;
        }
    }
}
