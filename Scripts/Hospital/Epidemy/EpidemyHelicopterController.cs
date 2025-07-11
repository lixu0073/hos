using UnityEngine;
using Hospital;

public class EpidemyHelicopterController : VehicleController
{
    private static EpidemyHelicopterController instance;
    public static EpidemyHelicopterController Instance { get { return instance; } }

    private Animator animator;
    private Epidemy epidemy;
#pragma warning disable 0649
    [SerializeField] private GameObject package;
#pragma warning restore 0649

    protected override void Awake()
    {
        if (instance == null)
            instance = this;
        else if(instance != this)        
            Destroy(gameObject);
    }

    void Start()
    {
        epidemy = HospitalAreasMapController.HospitalMap.epidemy;
        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    public void PrepareToFly()
    {
        epidemy.ChangeEpidemyInputPermission();
        package.SetActive(epidemy.Outbreak);
    }

    public void InteractWithChest()
    {
        if (epidemy.Outbreak)
        {
            epidemy.ManageChestDeployment();
            DeployChest();
        }
        else
        {
			PickUpChest();
			epidemy.ManageChestPickUp();
        }

        epidemy.ChangeEpidemyInputPermission();
    }

    private void DeployChest()
    {
        package.SetActive(false);
    }

    private void PickUpChest()
    {
        package.SetActive(true);
    }

    public void ResetAfterReload()
    {
        epidemy.IsHelicopterInAction = false;
        transform.position = new Vector3(100f, 0, 100f);
        StopRun();
    }
}
