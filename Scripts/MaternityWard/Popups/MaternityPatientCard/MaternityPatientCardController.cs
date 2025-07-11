using MovementEffects;
using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Maternity.UI
{
    public class MaternityPatientCardController : MasterDetailElementUI<MaternityPatientDetailCard, MaternityWaitingRoomBed>
    {
#pragma warning disable 0649
        [SerializeField]
        private MaternityPatientCardListUI patientCardListUI;
        [SerializeField]
        private MaternityPatientMasterCardController masterCardController;
        [SerializeField]
        private Animator SwitchCardAnimator;
        [SerializeField]
        private ScrollRect patientsScroll;
#pragma warning restore 0649
        private Coroutine scrollCoroutine;

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public static void Refresh()
        {
            if (UIController.getMaternity.patientCardController.gameObject.activeSelf)
                UIController.getMaternity.patientCardController.RefreshCollection();
        }

        protected override void PreOpen() {}

        protected override void PostOpen()
        {
            MaternityWaitingRoom.MaternityWaitingRoomAddedToMap -= MaternityWaitingRoom_MaternityWaitingRoomAddedToMap;
            MaternityWaitingRoom.MaternityWaitingRoomAddedToMap += MaternityWaitingRoom_MaternityWaitingRoomAddedToMap;
        }

        private void MaternityWaitingRoom_MaternityWaitingRoomAddedToMap(MaternityWaitingRoom obj)
        {
            if(this != null && gameObject != null && gameObject.activeSelf)
                RefreshCollection();
        }

        public void Exit()
        {
            UnbindListeners();
            currentModel = null;
            base.Exit(false);
        }

        void OnDestroy()
        {
            UnbindListeners();
        }

        void UnbindListeners()
        {
            MaternityWaitingRoom.MaternityWaitingRoomAddedToMap -= MaternityWaitingRoom_MaternityWaitingRoomAddedToMap;
        }

        protected override List<MaternityWaitingRoomBed> GetModels()
        {
            List<MaternityWaitingRoomBed> beds = new List<MaternityWaitingRoomBed>();
            foreach (MaternityWaitingRoom room in MaternityWaitingRoomController.Instance.Rooms())
            {
                MaternityWaitingRoomBed bed = room.bed;
                if (bed != null)
                    beds.Add(bed);
            }
            return beds;
        }

        protected override MaternityPatientDetailCard GetDetailCard(MaternityWaitingRoomBed model, bool IsSelected)
        {
            return new MaternityPatientDetailCard(model, patientCardListUI.AddBedPanel(), this, patientCardListUI, IsSelected);
        }

        public override void UpdateMasterCard(MaternityWaitingRoomBed model, bool instant = true, Anim anim = Anim.NONE)
        {
            NotificationCenter.Instance.WaitingRoomWorkingClickedNotif.Invoke(new SetCurrentlyPointedMachineEventArgs(model.room));
            SetCurrent(model);
            Timing.RunCoroutine(UpdateMasterCardCoroutine(model, instant, anim));
            AdjustBottomPanel(model);
        }

        private void AdjustBottomPanel(MaternityWaitingRoomBed model)
        {
            List<MaternityWaitingRoomBed> beds = GetModels();
            int index = beds.IndexOf(model);
            try
            { 
                if (scrollCoroutine != null)                
                    StopCoroutine(scrollCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            if (beds.Count > 3 && gameObject.activeSelf)            
                scrollCoroutine = StartCoroutine(UpdatePanelScrollPosition((float)index / (float)(beds.Count - 1)));
        }

        IEnumerator UpdatePanelScrollPosition(float pos)
        {
            float timeToAdjust = .4f;
            float timer = 0f;
            float startPos = patientsScroll.horizontalNormalizedPosition;
            while (timer < timeToAdjust)
            {
                timer += Time.deltaTime;
                patientsScroll.horizontalNormalizedPosition = Mathf.Lerp(startPos, pos, timer / timeToAdjust);
                yield return null;
            }
            scrollCoroutine = null;
        }

        protected override IEnumerator<float> UpdateMasterCardCoroutine(MaternityWaitingRoomBed model, bool instant, Anim anim = Anim.NONE)
        {
            if (!instant)
            {
                switch(anim)
                {
                    case Anim.RIGHT:
                        SwitchCardAnimator.ResetTrigger("SwitchRight");
                        SwitchCardAnimator.SetTrigger("SwitchLeft");
                        break;
                    case Anim.LEFT:
                        SwitchCardAnimator.ResetTrigger("SwitchLeft");
                        SwitchCardAnimator.SetTrigger("SwitchRight");
                        break;
                }
                yield return Timing.WaitForSeconds(1f / 3f);
            }
            if (masterCardController != null)
                masterCardController.SetData(model);
        }

        protected override void SetMasterCardEmpty()
        {
            if (masterCardController != null)
                masterCardController.SetData(null);
        }

        protected override void OnClearContent()
        {
            if(patientCardListUI != null)
                patientCardListUI.ClearList();
        }
    }
}
