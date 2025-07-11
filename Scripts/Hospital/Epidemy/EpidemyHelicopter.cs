using UnityEngine;
using Hospital;
using MovementEffects;
using System.Collections.Generic;

public class EpidemyHelicopter : MonoBehaviour
{
    private static EpidemyHelicopter instance;
    public static EpidemyHelicopter Instance { get { return instance; } }

    private Animator animator;
    private Epidemy epidemy;

    public int HeliWaitingDurationInSeconds;
    private IEnumerator<float> heliWaiting;

#pragma warning disable 0649
    [SerializeField] private GameObject package;
#pragma warning restore 0649

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        epidemy = HospitalAreasMapController.HospitalMap.epidemy;
        animator = transform.GetComponent<Animator>();
    }

    public void StartRunAt()
    {
        Debug.LogError("start run at");
    }

    public void HeliLandedCargo()
    {
        HeliLanded(true);
    }

    public void HeliLandedNoCargo()
    {
        HeliLanded(false);
    }

    public void HeliLanded(bool withCargo)
    {
        animator.SetTrigger("Wait");

        if (heliWaiting != null)
            Timing.KillCoroutine(heliWaiting);

        heliWaiting = Timing.RunCoroutine(HeliWaiting(withCargo));
    }

    IEnumerator<float> HeliWaiting(bool withCargo)
    {		
        epidemy.ChangeEpidemyInputPermission();
        if (withCargo)        
            InteractWithChest();

		if (((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromEpidemy)
        {
			yield break;
		}

        yield return Timing.WaitForSeconds(HeliWaitingDurationInSeconds);

        if (!withCargo)        
            InteractWithChest();

        animator.SetTrigger(withCargo ? "Fly_Out_NoCargo" : "Fly_Out_Cargo");
    }


	public void FlyOutCargo()
    {
		InteractWithChest();
		animator.SetTrigger("Fly_Out_Cargo");
	}

    public void FlyInCargo()
    {
        epidemy.IsHelicopterInAction = true;
        animator.SetTrigger("Fly_In_Cargo");
    }

    public void FlyInNoCargo()
    {
        epidemy.IsHelicopterInAction = true;
        animator.SetTrigger("Fly_In_NoCargo");
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
    }

    private void DeployChest()
    {
        package.SetActive(false);
    }

    private void PickUpChest()
    {
        package.SetActive(true);
    }

    public void Deactivate()
    {
        ResetAfterReload();
    }

    public void ResetAfterReload()
    {
        epidemy.IsHelicopterInAction = false;
        animator.SetTrigger("Fly");
    }

	public void PlayFlyInSfx()
    {
		SoundsController.Instance.PlayChooperIn ();
	}

	public void PlayFlyOutSfx()
    {
		SoundsController.Instance.StopHeliWait ();
		SoundsController.Instance.PlayFlyOutEpidemy ();
	}

	public void PlayWaitSfx()
    {
		SoundsController.Instance.PlayChooperWait ();
	}
}
