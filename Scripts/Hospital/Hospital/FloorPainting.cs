using UnityEngine;
using System.Collections.Generic;
using MovementEffects;

public class FloorPainting : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private float delay;
    [SerializeField] private GameObject paticle;
#pragma warning restore 0649
    public ParticleSystem.MinMaxCurve particle1Size;
    public ParticleSystem.MinMaxCurve particle2Size;
#pragma warning disable 0649
    [SerializeField] private GameObject paticleSub;
    [SerializeField] private Vector2 paticleSize;
#pragma warning restore 0649

    void Awake()
    {
        Timing.RunCoroutine(DestroyDelay());
    }

    public void InitializeWithColor(Vector4 color, float particleScaleValue)
    {
        var mainParticles = paticle.GetComponent<ParticleSystem>().main;
        mainParticles.startSize = new ParticleSystem.MinMaxCurve((mainParticles.startSize.constantMin * particleScaleValue) / 1.7f, (mainParticles.startSize.constantMax * particleScaleValue) / 1.7f);

        var subParticles = paticleSub.GetComponent<ParticleSystem>().main;
        subParticles.startSize = new ParticleSystem.MinMaxCurve((subParticles.startSize.constantMin * particleScaleValue) / 1.7f, (subParticles.startSize.constantMax * particleScaleValue) / 1.7f);

        paticle.GetComponent<ParticleSystemRenderer>().sharedMaterial.SetVector("_HSVAAdjust", color);
        paticleSub.GetComponent<ParticleSystemRenderer>().sharedMaterial.SetVector("_HSVAAdjust", color);

        this.gameObject.SetActive(true);
    }

    IEnumerator<float> DestroyDelay()
    {
        yield return Timing.WaitForSeconds(delay);
        Destroy(gameObject);
    }
}

