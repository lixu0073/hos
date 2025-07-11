using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;
using Hospital;

public class DeliveryCarController : VehicleController
{
    [SerializeField] private float DeliveryWait = 0;

    [HideInInspector] private List<int> cargo = new List<int>();

    [HideInInspector] public bool casesDelivered = false;

    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = transform.GetChild(0).GetComponent<Animator>();
        casesDelivered = false;
    }

    public List<int> GetCargo()
    {
        return cargo;
    }

    public void AddCargo(int cargoNumber)
    {
        cargo.Add(cargoNumber);
    }

    public void EmptyCargo()
    {
        cargo.Clear();
    }

    public void Delivery()
    {
        for (int i = 0; i < cargo.Count; i++)
        {
            ((HospitalCasesManager)AreaMapController.Map.casesManager).DeployCase(cargo[i]);
        }
        cargo.Clear();
        casesDelivered = true;
        ((HospitalCasesManager)AreaMapController.Map.casesManager).casesDelivered = true;
        NotificationCenter.Instance.PackageArrived.Invoke(new BaseNotificationEventArgs());
    }

    public void StopNearDepot()
    {
        StopRun();
        animator.SetTrigger("Stop");
        Timing.RunCoroutine(WaitAndRun(DeliveryWait));

    }

    public void DestroyAtEnd()
    {
        StopRun();
        SoundsController.Instance.StopDeliverySound();
        ((HospitalCasesManager)AreaMapController.Map.casesManager).deliveryCarBusy = false;
        gameObject.SetActive(false);

    }

    IEnumerator<float> WaitAndRun(float waitTime)
    {
        //	ReferenceHolder.Get ().engine.MainCamera.StopFollowing ();
        yield return Timing.WaitForSeconds(waitTime / 2);
        Delivery();
        yield return Timing.WaitForSeconds(waitTime / 2);
        StartRunAt(currSegm, actionsPerformed);
        animator.SetTrigger("Go");
        SoundsController.Instance.PlayDeliveryCarOut();
    }
}
