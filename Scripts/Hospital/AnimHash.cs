using UnityEngine;
using System.Collections;

public class AnimHash
{

    #region Person Animations
    public static readonly int Walk = Animator.StringToHash("Base Layer.WALK");
    public static readonly int Walk_Kid = Animator.StringToHash("Base Layer.WALK_KID");
    public static readonly int Stand_Unmoving = Animator.StringToHash("Base Layer.STAND_UNMOVING");
	public static readonly int Stand_Idle = Animator.StringToHash("Base Layer.STAND_IDLE");//("Base Layer.STAND_UNMOVING");//("Base Layer.STAND_IDLE");
    public static readonly int Stand_Sneeze = Animator.StringToHash("Base Layer.STAND_SNEEZE");
	public static readonly int Stand_Streatch = Animator.StringToHash("Base Layer.STAND_STREATCH");
	public static readonly int Stand_Scratch = Animator.StringToHash("Base Layer.STAND_SCRATCH");
    public static readonly int Stand_Read = Animator.StringToHash("Base Layer.STAND_PHONE");
    public static readonly int Stand_Phone = Animator.StringToHash("Base Layer.STAND_READ");
    public static readonly int Stand_Talk = Animator.StringToHash("Base Layer.STAND_TALK");//nie dodane do tablic jeszcze
    public static readonly int Stand_Drink = Animator.StringToHash("Base Layer.STAND_DRINK");
    public static readonly int Stand_Watch = Animator.StringToHash("Base Layer.STAND_WATCH");
    public static readonly int Hurray = Animator.StringToHash("Base Layer.HURRAY");
	public static readonly int Knock = Animator.StringToHash("Base Layer.KNOCK");

	public static readonly int Stand_To_Sit = Animator.StringToHash("Base Layer.STAND_TO_SIT");
	public static readonly int Sit_Idle = Animator.StringToHash("Base Layer.SIT_IDLE");
	public static readonly int Sit_Doctor = Animator.StringToHash("Base Layer.SIT_DOCTOR");
	public static readonly int Sit_Unmoving = Animator.StringToHash("Base Layer.SIT_UNMOVING");
	public static readonly int Sit_Talk = Animator.StringToHash("Base Layer.SIT_TALK");
	public static readonly int Sit_Listen = Animator.StringToHash("Base Layer.SIT_LISTEN");
	public static readonly int Sit_Machinetreatment = Animator.StringToHash("Base Layer.SIT_MACHINETREATMENT");
    public static readonly int Sit_Nurse = Animator.StringToHash("Base Layer.SIT_NURSE");
    public static readonly int Stand_Nurse = Animator.StringToHash("Base Layer.STAND_NURSE_DIAG");
    public static readonly int Stand_NurseWork = Animator.StringToHash("Base Layer.STAND_NURSE_DIAG_WORK");
    public static readonly int Sit_Relaxed = Animator.StringToHash("Base Layer.SIT_RELAXED");

    public static readonly int Bed_Idle = Animator.StringToHash("Base Layer.BED_IDLE");
	public static readonly int Bed_Handsmacking = Animator.StringToHash("Base Layer.BED_HANDSMACKING");
	public static readonly int Bed_Reading = Animator.StringToHash("Base Layer.BED_READING");
	public static readonly int Bed_Tablet = Animator.StringToHash("Base Layer.BED_TABLET");
	public static readonly int Bed_Hurray = Animator.StringToHash("Base Layer.BED_HURRAY");

    public static readonly int VIP_Bed_Idle = Animator.StringToHash("Base Layer.VIP_BED_IDLE");
    public static readonly int VIP_Bed_Handsmacking = Animator.StringToHash("Base Layer.VIP_BED_HANDSMACKING");
    public static readonly int VIP_Bed_Reading = Animator.StringToHash("Base Layer.VIP_BED_READING");

    public static readonly int Headache_1 = Animator.StringToHash("Base Layer.headache");
    public static readonly int Headache_2 = Animator.StringToHash("Base Layer.headache2");

    #endregion
    public static readonly int Waiting_Blue = Animator.StringToHash("Base Layer.waiting_headache_blueDoctor");
    public static readonly int Waiting_Yellow = Animator.StringToHash("Base Layer.waiting_stomach_yellowDoctor");
    public static readonly int Waiting_SunnyYellow = Animator.StringToHash("Base Layer.waiting_temp_sunnyYellowDoctor");
    public static readonly int Waiting_White = Animator.StringToHash("Base Layer.waitin_skin_whiteDoctor");


    public static readonly int Treatment_Head = Animator.StringToHash("Base Layer.Machine_Treatment_Head");
	public static readonly int Treatment_Green = Animator.StringToHash("Base Layer.Machine_Treatment_Green");
	public static readonly int Treatment_Purple = Animator.StringToHash("Base Layer.Machine_Treatment_Purple");

    #region Maternity Animations
    public static readonly int Mother_WOBaby_Walking = Animator.StringToHash("Base Layer.Walk_pregnant");
    public static readonly int Mother_WithBaby_Walking = Animator.StringToHash("Base Layer.Walk_wBaby");
    public static readonly int Mother_StandIdle_WO_Baby = Animator.StringToHash("Base Layer.standing_idle");
    public static readonly int Mother_StandIdle_W_Baby = Animator.StringToHash("Base Layer.standing_idle_wBaby");
    public static readonly int Mother_RestingWithBaby = Animator.StringToHash("Base Layer.Bed_Idle_wBaby");
    public static readonly int Mother_RestingWoBaby = Animator.StringToHash("Base Layer.Bed_Idle");
    public static readonly int Mother_ReadyForLabour = Animator.StringToHash("Base Layer.Bed_ready4labor");
    public static readonly int Mother_ExcerciseWithBall = Animator.StringToHash("Base Layer.Exercise with Ball");
    public static readonly int Mother_Yoga = Animator.StringToHash("Base Layer.Exercise_Yoga");
    public static readonly int Mother_Stretch = Animator.StringToHash("Base Layer.Exercise_Stretch");
    public static readonly int Baby_OnHands_Idle = Animator.StringToHash("Base Layer.Baby_with_Mom_go_home");
    public static readonly int Baby_OnHands_Bonding = Animator.StringToHash("Base Layer.Baby_Resting");
    public static readonly int Baby_OnHands_Sleeping = Animator.StringToHash("Base Layer.Baby_sleeping");
    public static readonly int IsBabySleepingBool = Animator.StringToHash("Sleeping");
    public static readonly int Nurse_Idle_Coach = Animator.StringToHash("Base Layer.Nurse_Idle");
    public static readonly int Nurse_Solitaire= Animator.StringToHash("Base Layer.Resting_room_solitaire");
    public static readonly int Nurse_Read_Magazine = Animator.StringToHash("Base Layer.Nurse_Resting_Magazine");
    public static readonly int Nurse_Sit_Talk_Back = Animator.StringToHash("Base Layer.Sit_Talk_Back_Left");
    public static readonly int Nurse_Dring_Coffe = Animator.StringToHash("Base Layer.Nurse_Resting_coffe");
    public static readonly int Scientist_Idle = Animator.StringToHash("Base Layer.Scientist_idle");
    public static readonly int Scientist_Back = Animator.StringToHash("Base Layer.Scientist_back");
    public static readonly int Scientist_wait_summary = Animator.StringToHash("Base Layer.Scientist_wait_summary");
    public static readonly int Scientist_Work = Animator.StringToHash("Base Layer.Scientist_work");
    public static readonly int BloodSampleAnimationUI = Animator.StringToHash("Base Layer.BloodSample");
    public static readonly int BloodResultDeliveredUI = Animator.StringToHash("Base Layer.BloodTest");
    public static readonly int StorkUI = Animator.StringToHash("Base Layer.Stork");

    #endregion

    #region Machine Animations
    public static readonly int Lungtesting_click = Animator.StringToHash("Base Layer.LungTesting_click");
	public static readonly int Lungtesting_work = Animator.StringToHash("Base Layer.LungTesting_work");

	public static readonly int Laser_click = Animator.StringToHash("Base Layer.Laser_click");
	public static readonly int Laser_work = Animator.StringToHash("Base Layer.Laser_work");

	public static readonly int MSI_click = Animator.StringToHash("Base Layer.MSI_click");
	public static readonly int MSI_work = Animator.StringToHash("Base Layer.MSI_work");

	public static readonly int Ultrasound_click = Animator.StringToHash("Base Layer.Ultrasound_click");
	public static readonly int Ultrasound_work = Animator.StringToHash("Base Layer.Ultrasound_work");

	public static readonly int Xray_click = Animator.StringToHash("Base Layer.Xray_click");
	public static readonly int Xray_work = Animator.StringToHash("Base Layer.Xray_work");
    
	public static readonly int MachineClick = Animator.StringToHash("Base Layer.Machine_Click");
	public static readonly int MachineWork = Animator.StringToHash("Base Layer.Machine_Work");
    public static readonly int MachineWorkKid = Animator.StringToHash("Base Layer.Machine_Work_Kid");

    //Production Machines
    public static readonly int Machine_Balsam_Click = Animator.StringToHash("Base Layer.Machine_Balsam_Click");
	public static readonly int Machine_Balsam_Work = Animator.StringToHash("Base Layer.Machine_Balsam_Work");

	public static readonly int Machine_DripLab_Click = Animator.StringToHash("Base Layer.Machine_DripLab_click");
	public static readonly int Machine_DripLab_Work = Animator.StringToHash("Base Layer.Machine_DripLab_Work");

	public static readonly int Machine_Capsule_Click = Animator.StringToHash("Base Layer.Machine_Capsule_click");
	public static readonly int Machine_Capsule_Work = Animator.StringToHash("Base Layer.Machine_Capsule_Anim");

	public static readonly int Elixir_Blender_Click = Animator.StringToHash("Base Layer.Elixir_Blender_click");
	public static readonly int Elixir_Blender_Work = Animator.StringToHash("Base Layer.Elixir_Blender_Work");

	public static readonly int Machine_EyeDrops_Click = Animator.StringToHash("Base Layer.EyeDrops_Work_click");
	public static readonly int Machine_EyeDrops_Work = Animator.StringToHash("Base Layer.EyeDrops_Work");

	public static readonly int Machine_Fizy_Click = Animator.StringToHash("Base Layer.Machine_Fizy_Tab_click");
	public static readonly int Machine_Fizy_Work = Animator.StringToHash("Base Layer.Machine_Fizy_Tab_Work");

	public static readonly int Machine_Inhale_Click = Animator.StringToHash("Base Layer.Machine_Inhale_click");
	public static readonly int Machine_Inhale_Work = Animator.StringToHash("Base Layer.Machine_Inhale_Work");

	public static readonly int Machine_Jelly_Click = Animator.StringToHash("Base Layer.Machine_Jelly_click");
	public static readonly int Machine_Jelly_Work = Animator.StringToHash("Base Layer.Machine_Jelly_Work");

	public static readonly int Machine_Nose_Click = Animator.StringToHash("Base Layer.Machine_Nose_click");
	public static readonly int Machine_Nose_Work = Animator.StringToHash("Base Layer.Machine_Nose_Work");

	public static readonly int Machine_Pils_Click = Animator.StringToHash("Base Layer.Machine_Pils_click");
	public static readonly int Machine_Pils_Work = Animator.StringToHash("Base Layer.Machine_Pils_Work");

	public static readonly int Machine_Shots_Click = Animator.StringToHash("Base Layer.Shots_click");
	public static readonly int Machine_Shots_Work = Animator.StringToHash("Base Layer.Shots_Work");

	public static readonly int Machine_Syrup_Click = Animator.StringToHash("Base Layer.Machine_Syrup_click");
	public static readonly int Machine_Syrup_Work = Animator.StringToHash("Base Layer.Machine_Syrup_Work");

	public static readonly int Machine_Extract_Click = Animator.StringToHash("Base Layer.Machine_Extract_click");
	public static readonly int Machine_Extract_Work = Animator.StringToHash("Base Layer.Machine_Extract_Work");

	public static readonly int Door_open = Animator.StringToHash("Base Layer.DoorAnimation");

	public static readonly int Playroom_slide_ballpit = Animator.StringToHash("Base Layer.Playroom_slide_ballpit");
	public static readonly int Playroom_slide = Animator.StringToHash("Base Layer.Playroom_slide");
	public static readonly int Playroom_ballpit_swim = Animator.StringToHash("Base Layer.Playroom_ballpit_swim");
	public static readonly int Playroom_beanbag = Animator.StringToHash("Base Layer.Playroom_beanbag");
	public static readonly int Playroom_horse = Animator.StringToHash("Base Layer.Playroom_horse");
	public static readonly int Playroom_bricks = Animator.StringToHash("Base Layer.Playroom_bricks");
	public static readonly int Playroom_bricks2 = Animator.StringToHash("Base Layer.Playroom_bricks2");


    public static readonly int Hurray2 = Animator.StringToHash("Base Layer.HURRAY2");
	public static readonly int Hurray3 = Animator.StringToHash("Base Layer.HURRAY3");
	public static readonly int Hurray4 = Animator.StringToHash("Base Layer.HURRAY4");
	public static readonly int Hurray5 = Animator.StringToHash("Base Layer.HURRAY5");


    #endregion

    #region Vehicles

    public static readonly int AmbulanceDriveIn1 = Animator.StringToHash("Base Layer.AmbulanceDriveIn");
    public static readonly int AmbulanceBreak = Animator.StringToHash("Base Layer.AmbulanceBreak");
    public static readonly int AmbulanceStop = Animator.StringToHash("Base Layer.AmbulanceStop");
    public static readonly int AmbulanceDriveOut1 = Animator.StringToHash("Base Layer.AmbulanceDriveOut1");
    public static readonly int AmbulanceDriveOut2 = Animator.StringToHash("Base Layer.AmbulanceDriveOut2");

    #endregion


    #region Plants

    public static readonly int PlantPatchIdle = Animator.StringToHash("Base Layer.PlantPatchIdle");
    public static readonly int ReadyAloeVera = Animator.StringToHash("Base Layer.ReadyAloeVera");
    public static readonly int ReadyBlueDishFungi = Animator.StringToHash("Base Layer.ReadyBlueDishFungi");
    public static readonly int ReadyDandelion = Animator.StringToHash("Base Layer.ReadyDandelion");
    public static readonly int ReadyGuaranaBerries = Animator.StringToHash("Base Layer.ReadyGuaranaBerries");
    public static readonly int ReadyHoneyMelon = Animator.StringToHash("Base Layer.ReadyHoneyMelon");
    public static readonly int ReadyPigTailFungi = Animator.StringToHash("Base Layer.ReadyPigTailFungi");
    public static readonly int HelpSign = Animator.StringToHash("Base Layer.HelpSign");

    #endregion


    #region Tutorial
    public static readonly int TutorialRefreshFullscreen = Animator.StringToHash("Refresh");
    public static readonly int TutorialRefreshInGame = Animator.StringToHash("NeedsRefresh");
    public static readonly int FullscreenCharacterVisible = Animator.StringToHash("FullscreenCharacterVisible");
    public static readonly int TutorialIsVisible = Animator.StringToHash("IsVisible");
    #endregion

    #region others
    public static readonly int BillboardActive = Animator.StringToHash("Billboard_Glow");
    public static readonly int BillboardInactive = Animator.StringToHash("Billboard_Idle");
    public static readonly int ToggleBadge = Animator.StringToHash("Base Layer.ToggleBadge");
    #endregion

}