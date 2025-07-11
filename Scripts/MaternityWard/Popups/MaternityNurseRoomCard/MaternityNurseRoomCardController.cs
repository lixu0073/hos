using SimpleUI;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.UI
{

    public class MaternityNurseRoomCardController : MasterDetailElementUI<MaternityNurseRoomDetailCard, MaternityWaitingRoomBed>
    {
#pragma warning disable 0649
        [SerializeField]
        private MaternityPatientCardListUI patientCardListUI;
        [SerializeField]
        private MaternityNurseRoomMasterCardController masterCardController;
#pragma warning restore 0649

        public static void Refresh()
        {
            if (UIController.getMaternity.nurseRoomCardController.gameObject.activeSelf)
                UIController.getMaternity.nurseRoomCardController.RefreshCollection();
        }

        public override void UpdateMasterCard(MaternityWaitingRoomBed model, bool instant = true, Anim anim = Anim.NONE)
        {
            if (masterCardController != null)
                masterCardController.SetData(model);
        }

        protected override MaternityNurseRoomDetailCard GetDetailCard(MaternityWaitingRoomBed model, bool IsSelected)
        {
            return new MaternityNurseRoomDetailCard(model, patientCardListUI.AddBedPanel(), this, patientCardListUI, IsSelected);
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

        protected override void OnClearContent()
        {
            if(patientCardListUI != null)
                patientCardListUI.ClearList();
        }

        protected override void PostOpen()
        {
            UnbindListeners();
            MaternityWaitingRoom.MaternityWaitingRoomAddedToMap += MaternityWaitingRoom_MaternityWaitingRoomAddedToMap;
        }

        private void MaternityWaitingRoom_MaternityWaitingRoomAddedToMap(MaternityWaitingRoom room)
        {
            if(this != null && gameObject != null && gameObject.activeSelf)
                RefreshCollection();
        }

        protected override void PreOpen() {}

        protected override void SetMasterCardEmpty()
        {
            if (masterCardController != null)
                masterCardController.SetData(null);
        }

        public void Exit()
        {
            UnbindListeners();
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

    }
}
