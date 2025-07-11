using UnityEngine;
using System.Collections;
using Hospital;
using UnityEngine.UI;

namespace SimpleUI
{

    public class RotateHover : BaseHover
    {

        public static RotateHover activeHover
        {
            get; private set;
        }

        [SerializeField] private GameObject bgSingle = null;
        [SerializeField] private GameObject bgDouble = null;

        RotatableObject rotatable;
        RotatableSimpleController controller;


        public static RotateHover Open(RotatableObject rotatable, RotatableSimpleController controller)
        {
            if (activeHover == null)
            {
                var p = Instantiate(UIController.get.rotateHoverPrefab);
                activeHover = p.GetComponent<RotateHover>();
                activeHover.Init();

                activeHover.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            activeHover.Initialize();
            activeHover.Init(rotatable, controller);
            BaseGameState.isHoverOn = true;

            return activeHover;
        }

        private void Init(RotatableObject rotatable, RotatableSimpleController controller)
        {
            this.rotatable = rotatable;
            this.controller = controller;
            if (rotatable.GetTypeOfInfos() == typeof(DecorationInfo))
            {
                Debug.Log(rotatable.GetTypeOfInfos().Name);
                bgSingle.SetActive(false);
                bgDouble.SetActive(true);
            }
            else
            {
                bgSingle.SetActive(true);
                bgDouble.SetActive(false);
            }
        }

        public override void Close()
        {
            base.Close();

            if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.arrange_text_before && rotatable.Tag == TutorialController.Instance.GetCurrentStepData().TargetMachineTag)
            {
                TutorialUIController.Instance.ShowIndictator(rotatable.gameObject, TutorialUIController.OnMapPointerTreatmentRoomOffset, false, false, TutorialUIController.TutorialPointerAnimationType.tap_hold_arrow);
            }

            BaseGameState.isHoverOn = false;
            if (rotatable != null)
                rotatable.SetBorderActive(false);

            if (TutorialController.Instance.GetCurrentStepData().NecessaryCondition == Condition.MoveRotateRoomEnd)
            {
                NotificationCenter.Instance.MoveRotateRoomEnd.Invoke(new MoveRotateRoomEndEventArgs());
            }
        }

        public void Rotate()
        {
            rotatable.RotateRight();

            if (rotatable is Decoration)
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.RotateDecorations));
        }

        public void StoreItem()
        {
            controller.StoreItem();
            Close();
        }
    }
}