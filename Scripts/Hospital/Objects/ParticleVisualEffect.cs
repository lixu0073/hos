using UnityEngine;

public class ParticleVisualEffect : MonoBehaviour, VisualEffect
{
#pragma warning disable 0649
    [SerializeField] ParticleSystem particleSystem;
#pragma warning restore 0649

    public bool HasEnded()
    {
        return !particleSystem.IsAlive(false);
    }

    public void RunVisualEffect()
    {
        particleSystem.Play();
    }
}
