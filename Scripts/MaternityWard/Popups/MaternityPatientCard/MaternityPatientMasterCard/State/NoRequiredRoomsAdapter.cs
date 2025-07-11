using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;

namespace Maternity.Adapter
{
    public class NoRequiredRoomsAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public NoRequiredRoomsAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) {}

        private enum State
        {
            LaborRoomReadyToBuild,
            LaborRoomBuilding,
            WaitingRoomBuilding
        }

        private State state = State.LaborRoomReadyToBuild;

        public override void SetUp()
        {
            base.SetUp();
            SetUpState();
            switch (state)
            {
                case State.LaborRoomReadyToBuild:
                    SetUpLaborRoomRequired(false);
                    break;
                case State.LaborRoomBuilding:
                    SetUpLaborRoomRequired(true);
                    break;
                case State.WaitingRoomBuilding:
                    SetUpWaitingRoomBuilding();
                    break;
            }
        }

        private void SetUpLaborRoomRequired(bool isOnMap)
        {
            ShopRoomInfo laborInfo = controller.Bed.room.GetLabourRoomInfo();
            string laborRoomName = I2.Loc.ScriptLocalization.Get(laborInfo.ShopDescription);
            ui.SetLaborRoomRequiredView(
                string.Format(I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.roomRequiredKey).ToUpper(), laborRoomName),
                OnBuildButtonClicked,
                isOnMap ? I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.checkKey).ToUpper() : I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonBuildKey).ToUpper(),
                null,
                laborInfo.ShopImage
            );
        }
        
        private void SetUpWaitingRoomBuilding()
        {
            ShopRoomInfo waitingInfo = controller.Bed.room.GetRoomInfo();
            string waitingRoomName = I2.Loc.ScriptLocalization.Get(waitingInfo.ShopDescription);
            ui.SetLaborRoomRequiredView(
                string.Format(I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.roomRequiredKey).ToUpper(), waitingRoomName),
                OnBuildButtonClicked,
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonBuildKey).ToUpper(),
                null,
                waitingInfo.ShopImage
            );
        }

        private void SetUpState()
        {
            MaternityLabourRoom laborRoom = controller.Bed.room.GetLabourRoom();
            if (laborRoom == null)
            {
                state = State.LaborRoomReadyToBuild;
                return;
            }
            if(laborRoom.state != Hospital.RotatableObject.State.working)
            {
                state = State.LaborRoomBuilding;
                return;
            }
            state = State.WaitingRoomBuilding;
        }

        private void OnBuildButtonClicked()
        {
            UIController.getMaternity.patientCardController.Exit();
            switch (state)
            {
                case State.LaborRoomReadyToBuild:
                    OpenDrawerOnLabourRoom();
                    break;
                case State.LaborRoomBuilding:
                    controller.Bed.room.GetLabourRoom().OnClick(true);
                    break;
                case State.WaitingRoomBuilding:
                    controller.Bed.room.OnClick(true);
                    break;
            }
        }

        private void OpenDrawerOnLabourRoom()
        {
            //RefactoredDrawerController drawer = UIController.get.drawer;
            IDrawer drawer = UIController.get.drawer;
            ShopRoomInfo laborInfo = controller.Bed.room.GetLabourRoomInfo();

            foreach (UIDrawerController.DrawerRotationItem item in drawer.AllDrawerItems())
            {
                if (item.drawerItem.GetInfo.infos.Tag == laborInfo.Tag)
                {
                    if (!UIController.get.drawer.IsVisible)
                    {
                        drawer.ToggleVisible();
                    }
                    drawer.SwitchToTab(1);
                    drawer.CenterInMaternityTabToItem(item.drawerItem.GetComponent<RectTransform>());
                    break;
                }
            }
        }

    }
}
