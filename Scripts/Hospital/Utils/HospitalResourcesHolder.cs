using UnityEngine;
using System.Collections.Generic;
using IsoEngine;
using Hospital;

public class HospitalResourcesHolder : BaseResourcesHolder
{
    public override void PostStart()
    {
        HospitalAreasMapController.HospitalMap.epidemy.EpidemyPackageGenerator.StartUp();
    }

    #region prefabs
	public GameObject DoctorHoverButton;
	public GameObject ProductionHoverButton;
    public GameObject ParticleCover;
    public GameObject ParticlePanaceaStart;
    public GameObject ParticleReceptionOpen;
    public GameObject ParticleCure;
	public GameObject ParticleHearts;
    public GameObject ParticleBedPatientCured;
	public GameObject PanaceaCollectorDrop;
    public Sprite[] storageSprites;
	public Sprite baseProbeTableImage;
	public Sprite ProbeTableToolSprite;
	public Sprite PlantHarvestingSprite;
	public Sprite PatchCultivatorSprite;

    public string GetStoryForCure(MedicineRef med)
    {
        return I2.Loc.ScriptLocalization.Get(medicines.cures[(int)med.type].medicines[med.id].StoryKey);
    }

    public Sprite PatchHelpSignSprite;
    public Sprite ProbeSelectionEmpty;
    public Sprite ProbeSelectionFull;
    public Sprite defaultSign;
	public Sprite[] Beds;
	public Sprite[] VIPQuilt;
    public GameObject CureIndicator;
	public GameObject FloraTooltipPrefab;
    public GameObject BedStatusIndicator;
    public GameObject ZzzDocPrefab;
    public GameObject diagnosisIndicatorPrefab;
    public GameObject PatientHappyPrefab;
    public GameObject PatientDiagnosePrefab;
    public GameObject floorPainting;

    public Sprite BlueDoctorRoomElixir;
    public Sprite GreenDoctorRoomElixir;
    public Sprite PinkDoctorRoomElixir;
    public Sprite PurpleDoctorRoomElixir;
    public Sprite RedDoctorRoomElixir;
    public Sprite SkyBlueDoctorRoomElixir;
    public Sprite SunnyYellowDoctorRoomElixir;
    public Sprite WhiteDoctorRoomElixir;
    public Sprite YellowDoctorRoomElixir;
    #endregion

    [SerializeField]
    public BubbleBoyDatabase bubbleBoyDatabase;
    public CustomizableHospitalFlagDatabase flagsDatabase;
    public CustomizableHospitalSignDatabase signsDatabase;    

    public DailyTaskDataBase dailyTaskDatabase;

    public List<Vector2i> SpawnPoints;
	public Vector2i[] VIPspots; //0-spawn 1-bed 2-elevator 3- elevator hospital
	public Texture DoctorFloor;
    public Texture Roomx2Floor;
    public Texture Roomx3Floor;
    public Texture VipRoomFloor;
	public Texture DiagnosisFloor;
    public DiagnosisBadgeGfx diagnosisBadgeGfx = new DiagnosisBadgeGfx();

    #region prefabs
    public GameObject VipPopupPrefab;
	#endregion

    #region ScriptableObjects
    public List<RoomCharacterInfo> NursePaths;
    public List<ClinicDiseaseDatabaseEntry> ClinicDiseases;
    #endregion

    [Header("Game Events")]
    public Sprite GameEventDefaultBackground;
    public Sprite Biohazard;
    public Sprite ShovelIcon;
    public Sprite HelpIcon;
    public Sprite HeliIcon;
    public Sprite VIPIcon;
    public Sprite box;
    public Sprite BubbleBoyFace;
    public Sprite PanaceaCollectorImage;
    public Sprite TODOListImage;

    public GameObject MailBox;
    
    public ClinicDiseaseDatabaseEntry GetClinicDisease(int id)
    {
        for (int i = 0; i < ClinicDiseases.Count; i++)
        {
            if (ClinicDiseases[i].id == id)
                return ClinicDiseases[i];
        }

        Debug.LogError("ClinicDisease with this ID not found! Have some Headache.");
        return ClinicDiseases[0];
    }
    
    [System.Serializable]
    public class DiagnosisBadgeGfx
    {
#pragma warning disable 0649
        [SerializeField]
        private Sprite badgeLaser;
#pragma warning restore 0649
        public Sprite badgeLung;
        public Sprite badgeMRI;
        public Sprite badgeSound;
        public Sprite badgeXRay;

        public Sprite iconLaser;
        public Sprite iconLung;
        public Sprite iconMRI;
        public Sprite iconSound;
        public Sprite iconXRay;

        public Sprite GetDiagnosisBadge(HospitalDataHolder.DiagRoomType roomType)
        {
            switch (roomType)
            {
                case HospitalDataHolder.DiagRoomType.Laser:
                    return badgeLaser;
                case HospitalDataHolder.DiagRoomType.LungTesting:
                    return badgeLung;
                case HospitalDataHolder.DiagRoomType.MRI:
                    return badgeMRI;
                case HospitalDataHolder.DiagRoomType.UltraSound:
                    return badgeSound;
                case HospitalDataHolder.DiagRoomType.XRay:
                    return badgeXRay;
            }
            return null;
        }

        public Sprite GetDiagnosisCloudIcon(HospitalDataHolder.DiagRoomType roomType)
        {
            switch (roomType)
            {
                case HospitalDataHolder.DiagRoomType.Laser:
                    return iconLaser;
                case HospitalDataHolder.DiagRoomType.LungTesting:
                    return iconLung;
                case HospitalDataHolder.DiagRoomType.MRI:
                    return iconMRI;
                case HospitalDataHolder.DiagRoomType.UltraSound:
                    return iconSound;
                case HospitalDataHolder.DiagRoomType.XRay:
                    return iconXRay;
            }
            return null;
        }
    }
}