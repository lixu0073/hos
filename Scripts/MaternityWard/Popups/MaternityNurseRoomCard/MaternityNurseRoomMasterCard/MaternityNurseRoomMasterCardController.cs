using Hospital;
using Maternity.PatientStates;
using Maternity.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity
{
    public class MaternityNurseRoomMasterCardController : MonoBehaviour
    {
        [HideInInspector]
        public MaternityPatientAI Ai;
        [HideInInspector]
        public MaternityCharacterInfo Info;
        public MaternityWaitingRoomBed Bed;
        private BaseAdapter adapter;
        private BaseAdapter Adapter
        {
            get
            {
                return adapter;
            }
            set
            {
                if (adapter != null)
                    adapter.OnDestroy();
                adapter = value;
            }
        }
#pragma warning disable 0649
        [SerializeField]
        private MaternityPatientPanelUI patientPanelUI;
#pragma warning restore 0649

        public void SetData(MaternityWaitingRoomBed bed)
        {
            if (adapter != null)
                adapter.OnDestroy();
            Bed = bed;
            if (Bed != null)
            {
                IMaternityFacilityPatient patient = Bed.GetPatient();
                if (patient != null)
                {
                    Ai = patient.GetPatientAI();
                    Info = patient.GetInfoPatient();
                    Ai.Person.State.MoveTo();
                }
                else
                {
                    Ai = null;
                    Info = null;
                    Bed.room.MoveCameraToThisRoom();
                }
            }
            SetUpAdapter();
            AddListeners();
        }

        private void AddListeners()
        {
            if (Ai == null)
                return;
            RemoveListeners();
            Ai.onStateChanged += Ai_onStateChanged;
        }

        private void Ai_onStateChanged()
        {
            Adapter = MaternityNurseRoomMasterCardAdapterFactory.Get(this, patientPanelUI);
        }

        private void RemoveListeners()
        {
            if (Ai != null)
                Ai.onStateChanged -= Ai_onStateChanged;
        }

        private void SetUpAdapter()
        {
            Ai_onStateChanged();
        }

        private void Update()
        {
            if (adapter != null)
                adapter.OnUpdate();
        }

        private void OnDestroy()
        {
            RemoveListeners();
            if (adapter != null)
                adapter = null;
        }

        #region Adapters

        public class UnsupportedStateAdapter : BaseAdapter
        {
            public UnsupportedStateAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetLaborEndedView("View not implemented yet", OpenPatientCard, "View not implemented yet");
            }
        }

        public class NoRequiredRoomsAdapter : BaseAdapter
        {
            public NoRequiredRoomsAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

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
                ui.SetLaborRoomRequiredView(OnBuildButtonClicked);
                /*ui.SetLaborRoomRequiredView(
                    string.Format(I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.roomRequiredKey).ToUpper(), laborRoomName),
                    OnBuildButtonClicked,
                    isOnMap ? I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonShowKey).ToUpper() : I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonBuildKey).ToUpper(),
                    null,
                    laborInfo.ShopImage
                );*/
            }

            private void SetUpWaitingRoomBuilding()
            {
                ShopRoomInfo waitingInfo = controller.Bed.room.GetRoomInfo();
                string waitingRoomName = I2.Loc.ScriptLocalization.Get(waitingInfo.ShopDescription);
                ui.SetLaborRoomRequiredView(OnBuildButtonClicked);
                /*ui.SetLaborRoomRequiredView(
                    string.Format(I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.roomRequiredKey).ToUpper(), waitingRoomName),
                    OnBuildButtonClicked,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonShowKey).ToUpper(),
                    null,
                    waitingInfo.ShopImage
                );*/
            }

            private void SetUpState()
            {
                MaternityLabourRoom laborRoom = controller.Bed.room.GetLabourRoom();
                if (laborRoom == null)
                {
                    state = State.LaborRoomReadyToBuild;
                    return;
                }
                if (laborRoom.state != Hospital.RotatableObject.State.working)
                {
                    state = State.LaborRoomBuilding;
                    return;
                }
                state = State.WaitingRoomBuilding;
            }

            private void OnBuildButtonClicked()
            {
                UIController.getMaternity.nurseRoomCardController.Exit();
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

        public class LaborFinishedAdapter : BaseAdapter
        {
            public LaborFinishedAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetLaborEndedView(
                    GetPatientName(),
                    OpenPatientCard,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.laborEndedKey).ToUpper()
                    );
            }
        }

        public class ReadyForLabourAdapter : BaseAdapter
        {
            public ReadyForLabourAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetReadyForLaborView(
                    GetPatientName(),
                    OpenPatientCard,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.readyForLabourKey).ToUpper()
                    );
            }
        }

        public class InLaborAdapater : BaseAdapter
        {
            public InLaborAdapater(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetLaborInProgressView(
                    GetPatientName(),
                    OpenPatientCard,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.inLaborKey).ToUpper()
                    );
                SetListeners();
                controller.Ai.Person.State.BroadcastData();
            }

            private void SetListeners()
            {
                controller.Ai.OnDataReceived_BLS += Ai_OnDataReceived_BLS;
            }

            private void Ai_OnDataReceived_BLS(PatientStates.MaternityPatientBaseLaborState.Data data)
            {
                int secondsLeft = (int)Math.Ceiling(data.timeLeft);
                ui.SetTreatmentTimer(UIController.GetFormattedTime(secondsLeft));
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
                if (controller.Ai != null)
                    controller.Ai.OnDataReceived_BLS -= Ai_OnDataReceived_BLS;
            }

        }

        public class WaitingForLaborAdapter : BaseAdapter
        {
            public WaitingForLaborAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetWaitingForLaborView(
                    GetPatientName(),
                    OpenPatientCard,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.waitingForLaborKey).ToUpper()
                    );
            }
        }

        public class WaitingForCuresAdapter : BaseAdapter
        {
            public WaitingForCuresAdapter(MaternityNurseRoomMasterCardController masterController, IMaternityPatientPanelUI masterUI) : base(masterController, masterUI) { }

            public override void SetUp()
            {
                base.SetUp();
                List<TreatmentPanelData> cures = new List<TreatmentPanelData>();
                foreach (KeyValuePair<MedicineDatabaseEntry, int> entry in controller.Info.RequiredCures)
                {
                    cures.Add(new TreatmentPanelData(entry.Key, entry.Value, 0));
                }
                ui.SetVitaminesView(
                    GetPatientName(),
                    controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Vitamines).ToString(),
                    OpenPatientCard,
                    cures
                    );
            }
        }

        public class InWaitingForDiagnoseResultsAdapter : BaseAdapter
        {
            public InWaitingForDiagnoseResultsAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetDiagnoseEndedView(
                    GetPatientName(),
                    controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Diagnose).ToString(),
                    OpenPatientCard,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.diagnoseEndedKey).ToUpper(),
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.diagnoseResultsKey).ToUpper()
                    );
            }
        }

        public class InDiagnoseAdapter : BaseAdapter
        {
            public InDiagnoseAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetDiagnoseInProgressView(
                    GetPatientName(),
                    controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Diagnose).ToString(),
                    OpenPatientCard,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.patientDiagnoseKey).ToUpper(),
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.analysingKey).ToUpper()
                    );
                SetListeners();
            }

            private void SetListeners()
            {
                controller.Ai.OnDataRecieved_ID += Ai_OnDataReceived_ID;
            }

            private void Ai_OnDataReceived_ID(MaternityPatientInDiagnoseState.Data data)
            {
                int timeLeft = (int)Math.Ceiling(data.timeLeft);
                ui.SetDiagnoseAndGiftTimer(UIController.GetFormattedTime(timeLeft));
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
                if (controller.Ai != null)
                    controller.Ai.OnDataRecieved_ID -= Ai_OnDataReceived_ID;
            }

        }

        public class InDiagnoseQueueAdapter : BaseAdapter
        {
            public InDiagnoseQueueAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetDiagnoseInQueueView(
                    GetPatientName(),
                    controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Diagnose).ToString(),
                    OpenPatientCard,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.bloodTestKey).ToUpper(),
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.bloodTestKey)
                    );
            }
        }

        public class WaitingForSendToDiagnoseAdapter : BaseAdapter
        {
            public WaitingForSendToDiagnoseAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetDiagnoseRequiredView(
                    GetPatientName(),
                    controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Diagnose).ToString(),
                    OpenPatientCard,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.bloodTestKey).ToUpper()
                );
            }
        }

        public class GoingOutAdapter : BaseAdapter
        {
            public GoingOutAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            private int costInDiamonds = 0;

            public override void SetUp()
            {
                base.SetUp();
                ui.SetWaitingForNextPatientView(
                    SpeedUpButton,
                    EMPTY);
                SetListeners();
                controller.Ai.Person.State.BroadcastData();
            }

            protected virtual void SpeedUpButton()
            {
                if (Game.Instance.gameState().GetDiamondAmount() >= costInDiamonds)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(costInDiamonds, delegate
                    {
                        Game.Instance.gameState().RemoveDiamonds(costInDiamonds, EconomySource.SpeedUpMotherSpawn);
                        controller.Ai.Person.State.Notify((int)StateNotifications.SpeedUpMotherSpawn, null);
                    }, this);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            }

            protected virtual void SetListeners()
            {
                controller.Ai.OnDataReceived_GO += Ai_OnDataReceived_GO;
            }

            private void Ai_OnDataReceived_GO(MaternityPatientGoingOutState.Data data)
            {
                int secondsLeft = (int)Math.Ceiling(data.timeLeft);
                ui.SetWaitingTimer(UIController.GetFormattedTime(secondsLeft));
                SetSpeedUpButton(secondsLeft, data.baseSpawnTime);
            }

            protected void SetSpeedUpButton(int timeLeft, float baseMaxTime)
            {
                costInDiamonds = DiamondCostCalculator.GetCostForAction(timeLeft, baseMaxTime);
                ui.SetTreatmentButton(
                    SpeedUpButton,
                    costInDiamonds.ToString(),
                    ResourcesHolder.GetMaternity().diamondSprite);
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
                if (controller.Ai != null && controller.Ai.Person != null && controller.Ai.Person.State != null)
                    controller.Ai.OnDataReceived_GO -= Ai_OnDataReceived_GO;
            }
        }

        public class WaitingForSpawnAdapter : GoingOutAdapter
        {
            public WaitingForSpawnAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            private int costInDiamonds = 0;

            public override void SetUp()
            {
                base.SetUp();
                ui.SetWaitingForNextPatientView(SpeedUpButton, EMPTY);
                SetListeners();
                controller.Bed.StateManager.State.BroadcastData();
            }

            protected override void SpeedUpButton()
            {
                if (Game.Instance.gameState().GetDiamondAmount() >= costInDiamonds)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(costInDiamonds, delegate
                    {
                        Game.Instance.gameState().RemoveDiamonds(costInDiamonds, EconomySource.SpeedUpMotherSpawn);
                        controller.Bed.StateManager.State.Notify((int)StateNotifications.SpeedUpMotherSpawn, null);
                    }, this);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            }

            protected override void SetListeners()
            {
                controller.Bed.OnDataReceived_WFP += Bed_OnDataReceived_WFP;
            }

            private void Bed_OnDataReceived_WFP(WaitingRoom.Bed.State.MaternityWaitingRoomBedWaitingForPatientState.Data data)
            {
                int secondsLeft = (int)Math.Ceiling(data.timeLeft);
                ui.SetWaitingTimer(UIController.GetFormattedTime(secondsLeft));
                SetSpeedUpButton(secondsLeft, data.baseSpawnTime);
            }

            public override void OnDestroy()
            {
                if (controller.Bed != null)
                    controller.Bed.OnDataReceived_WFP -= Bed_OnDataReceived_WFP;
            }

        }

        public class WaitingForPatientAdapter : BaseAdapter
        {
            public WaitingForPatientAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetNextPatientOnHisWayView();
            }
        }

        public class WaitForCollectLaborRewardAdapter : BaseAdapter
        {
            public WaitForCollectLaborRewardAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            public override void SetUp()
            {
                base.SetUp();
                ui.SetHealingAndBoundingGiftReadyView(
                    GetPatientName(),
                    OpenPatientCard,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.healingAndBondingKey.ToUpper()),
                    GetBabyGender() == PatientAvatarUI.PatientBackgroundType.boyBaby
                    );
            }
        }

        public class BondingAdapater : BaseAdapter
        {
            public BondingAdapater(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui) : base(controller, ui) { }

            private int speedUpCostInDiamonds = 0;
            private bool isFirstLoop = false;

            public override void SetUp()
            {
                base.SetUp();
                isFirstLoop = !Game.Instance.gameState().IsMaternityFirstLoopCompleted;
                ui.SetHealingAndBoundingGiftTimerView(
                    GetPatientName(),
                    OnSpeedUpGiftButtonClicked,
                    I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.healingAndBondingKey).ToUpper(),
                    EMPTY,
                    GetBabyGender() == PatientAvatarUI.PatientBackgroundType.boyBaby
                    );
                if (isFirstLoop)
                {
                    speedUpCostInDiamonds = 0;
                    ui.SetTreatmentButton(OnSpeedUpGiftButtonClicked, I2.Loc.ScriptLocalization.Get("FREE").ToUpper(), null, true);
                }
                SetListeners();
                controller.Ai.Person.State.BroadcastData();
            }

            private void OnSpeedUpGiftButtonClicked()
            {
                if (Game.Instance.gameState().GetDiamondAmount() >= speedUpCostInDiamonds)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(speedUpCostInDiamonds, delegate
                    {
                        Game.Instance.gameState().RemoveDiamonds(speedUpCostInDiamonds, EconomySource.SpeedUpBondingAndHealing, controller.Ai.Person.State.GetRoom().Tag);
                        controller.Ai.Notify((int)StateNotifications.SpeedUpRewardForLabor, null);
                    }, this);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            }

            private void SetListeners()
            {
                controller.Ai.OnDataReceived_BBS += Ai_OnDataReceived_BBS;
            }

            private void Ai_OnDataReceived_BBS(PatientStates.MaternityPatientBaseBondingState.Data data)
            {
                int secondsLeft = (int)Math.Ceiling(data.timeLeft);
                ui.SetDiagnoseAndGiftTimer(UIController.GetFormattedTime(secondsLeft));
                if (!isFirstLoop)
                    SetSpeedUpButton(secondsLeft);
            }

            private void SetSpeedUpButton(int timeLeft)
            {
                speedUpCostInDiamonds = DiamondCostCalculator.GetCostForAction(timeLeft, controller.Info.MaxHealingAndBondingTime);
                ui.SetTreatmentButton(OnSpeedUpGiftButtonClicked, speedUpCostInDiamonds.ToString(), ResourcesHolder.GetMaternity().diamondSprite);
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
                if (controller.Ai != null)
                    controller.Ai.OnDataReceived_BBS -= Ai_OnDataReceived_BBS;
            }
        }

        public abstract class BaseAdapter : IDiamondTransactionMaker
        {
            protected MaternityNurseRoomMasterCardController controller;
            protected IMaternityPatientPanelUI ui;
            protected Guid DiamondTransactionMakerID;

            public const string EMPTY = "";

            public BaseAdapter(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui)
            {
                this.controller = controller;
                this.ui = ui;
                SetUp();
            }

            protected string GetPatientName()
            {
                return I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + controller.Info.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + controller.Info.Surname);
            }

            protected string GetAboutMother()
            {
                // TODO
                return "About mother";
            }

            protected string GetBabyName()
            {
                return I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + controller.Ai.GetBabyInfo().Name);
            }

            protected string GetBabyInfo()
            {
                // TODO
                return "baby info";
            }

            protected PatientAvatarUI.PatientBackgroundType GetBabyGender()
            {
                BabyCharacterInfo info = controller.Ai.GetBabyInfo();
                if (info == null)
                    return PatientAvatarUI.PatientBackgroundType.unknownBaby;
                return info.Sex == 0 ? PatientAvatarUI.PatientBackgroundType.boyBaby : PatientAvatarUI.PatientBackgroundType.girlBaby;
            }

            protected Sprite GetBabyHead()
            {
                BabyCharacterInfo info = controller.Ai.GetBabyInfo();
                if (info == null)
                    return null;
                return info.AvatarHead;
            }

            protected Sprite GetBabyBody()
            {
                BabyCharacterInfo info = controller.Ai.GetBabyInfo();
                if (info == null)
                    return null;
                return info.AvatarBody;
            }

            protected void OpenPatientCard()
            {
                UIController.getMaternity.nurseRoomCardController.Exit();
                UIController.getMaternity.patientCardController.Open(controller.Bed);
            }

            protected void ClosePopupAndRedirectToBloodTestRoom(bool showHover = true)
            {
                UIController.getMaternity.patientCardController.Exit();
                MaternityBloodTestRoom bloodTestRoom = MaternityBloodTestRoomController.Instance.GetBloodTestRoom();
                bloodTestRoom.RedirectTo(showHover);
            }

            public virtual void SetUp()
            {
                InitializeID();
            }

            public virtual void OnUpdate() { }

            public virtual void OnDestroy() { }

            public void InitializeID()
            {
                DiamondTransactionMakerID = Guid.NewGuid();
            }

            public Guid GetID()
            {
                return DiamondTransactionMakerID;
            }

            public void EraseID()
            {
                DiamondTransactionMakerID = Guid.Empty;
            }
        }

        #endregion

    }

}
