using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using System;
using MovementEffects;

namespace Hospital
{
    public class RemovableDecoration : RotatableObject
    {

        public bool shouldWork
        {
            get;
            private set;
        }

        protected override void AddToMap()
        {
            if (state != State.working)
                state = State.working;
            base.AddToMap();

            GetComponent<RotatableSimpleController>().draggingEnabled = false;
        }

        public override void SetAnchored(bool value)
        {
            base.SetAnchored(value);
        }

        protected override void RemoveFromMap()
        {
            base.RemoveFromMap();
        }

        public override void IsoDestroy()
        {
            base.IsoDestroy();
        }

        protected override void OnClickWorking() {

            if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            base.OnClickWorking();

            if (Game.Instance.gameState().GetHospitalLevel()<4)
            {
                MessageController.instance.ShowMessage(38);
                return;
            }

            if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.patio_tidy_5))
                return;

            TutorialUIController.Instance.SetIndicatorParent(TutorialUIController.Instance.gameObject.transform);

            int expReward = 1;
            if (Game.Instance.gameState().GetHospitalLevel() > 4)
                expReward = 1;
            else
            {
                expReward = (int)(((Game.Instance.gameState().GetExpForLevel(4) + 5) - Game.Instance.gameState().GetExperienceAmount()) / 6f);
                expReward = Mathf.Clamp(expReward, 1, 5);
            }
            //Debug.LogError("User will get xp for removable decoration: " + xp);

            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.Tutorial, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(SimpleUI.GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () => {
                Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
            });

            var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleRemovableDecoration, transform.position, Quaternion.Euler(0, 0, 0));
            fp.transform.localScale = Vector3.one;
            fp.transform.position = this.transform.position + new Vector3(info.infos.NorthPrefab.GetComponent<IsoObjectPrefabController>().prefabData.tilesX / 2, 0, info.infos.NorthPrefab.GetComponent<IsoObjectPrefabController>().prefabData.tilesY / 2);
            fp.SetActive(true);

            base.IsoDestroy();
            SpawnNewDecorationOnMapAfterRemove();

            gameObject.SetActive(false);
            NotificationCenter.Instance.PatioElementCleared.Invoke(new BaseNotificationEventArgs());
            Destroy(gameObject);
			SoundsController.Instance.PlayPoof ();
        }

        protected void SpawnNewDecorationOnMapAfterRemove()
        {
            var newDec = info.infos as RemovableDecorationInfo;
            
            if (newDec != null)
            {
                if (newDec.spawneObjectWhenRemoved != null)
                {
                    var tmpPrefabRotation = HospitalAreasMapController.HospitalMap.GetPrefabInfoWithName(newDec.spawneObjectWhenRemoved.name);
                    CreateRotatableObject(tmpPrefabRotation.infos.Tag, this.position, Rotation.North, State.working);
                }
            }

        }
    }
}
