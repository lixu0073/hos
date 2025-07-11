using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using MovementEffects;

namespace Hospital
{
    public class ElixirTank : ProductionRotatable
    {
        private Vector3 normalScale = Vector3.zero;
        private Vector3 targetScale = Vector3.zero;
        private bool firstBuilded = true;

        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            base.Initialize(info, position, rotation, _state, shouldDisappear);
            RemoveFromMap();
            state = State.working;
            AddToMap();
        }

        public int GetMaxLVL()
        {
            return GetMaxLevel();
        }

        protected override int GetMaxLevel()
        {
            return 999;
        }

        public int GetActualLevel()
        {
            return actualLvl;
        }

        public override int GetAmountOnLevel(bool next = false)
        {
            //return base.GetAmountOnLevel(next);
            int level = actualLevel;
            if (next)
                level++;

            return 10 + (level * 20);
        }

        protected override void Init()
        {
            base.Init();
            if (!AreaMapController.Map.VisitingMode)
            {
                GameState.Get().ElixirTank = this;
            }
            SetIndicator();
        }

        public virtual void SetActualAmount(int amount)
        {
            actualAmount = amount;
            SetIndicator();
        }
        

        protected override void OnClickWorking()
        {
            if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            //ProductionMachineInfo prodInfo = (ProductionMachineInfo)info.infos;
            //NotificationCenter.Instance.SheetRemove.Invoke(new SheetRemoveEventArgs(prodInfo.BuildingName));
            base.OnClickWorking();
            BounceTankObject();
            UIController.getHospital.StoragePopUp.Open(true, false,true);
        }
        
        void BounceTankObject()
        {
            //Timing.KillCoroutine(BounceCoroutine());
            Timing.RunCoroutine(BounceCoroutine());
        }

        IEnumerator<float> BounceCoroutine()
        {
            //Debug.Log("BounceCoroutine");
            float bounceTime = .15f;
            float timer = 0f;
            Transform targetTransform = isoObj.GetGameObject().transform;
            if (firstBuilded)
            {
                firstBuilded = false;
                normalScale = Vector3.zero;
            }

            if (normalScale == Vector3.zero)
                normalScale = targetTransform.localScale;

            targetScale = normalScale * 1.1f; //new Vector3(1.1f, 1.1f, 1.1f);

            //scale up
            if (normalScale != Vector3.zero && normalScale != Vector3.zero)
            {
                //scale up
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