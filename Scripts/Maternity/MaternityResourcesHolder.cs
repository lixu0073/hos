using Hospital;
using Maternity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaternityResourcesHolder : BaseResourcesHolder
{
    public Texture WaitingRoomFloor;
    public Texture LabourRoomFloor;
    public Texture BloodDiagnosisRoomFloor;
    public Sprite diagnosisBadgeGfx;
    public MaternityWaitingRoomIndicatorsPresenter WaitingRoomIndicators;

    [Header("LaborStageBadges")]
    public Sprite diagnosisStageBadge;
    public Sprite vitaminesBadge;
    public Sprite waitingStageBadge;
    public Sprite inLaborStageBadge;
    public Sprite bondingStageBadge;
    public Sprite newPatientBadge;

    [Header("BabyBackgrounds")]
    public Sprite unknownBabyAvatarBg;
    public Sprite boyBabyBabyAvatarBg;
    public Sprite girlBabyAvatarBg;
    public Sprite popupBoyBg;
    public Sprite popupGirlBg;

    [Header("BabyBallons")]
    public Sprite boyBabyBallon;
    public Sprite girlBabyBallon;

    [Header("MaternityLockit")]
    public MaternityTreatmentPanelLocKeys maternityLocKeys;

    public GameObject BloodHoverButton;

    public override void PostStart(){}


    public override List<Rotations> GetMachinesForLevel(int level)
    {
        return HospitalAreasMapController.Map.rotationsIDs
            .Where(p =>
            {
                return ((ShopRoomInfo)p.infos).unlockLVL == level && 
                p.infos.DrawerArea == HospitalAreaInDrawer.MaternityClinic;
            }).ToList();
    }
}
