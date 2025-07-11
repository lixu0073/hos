using UnityEngine;

public class ParticleFromAnimator : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    ParticleSystem[] particleSystemArray;
#pragma warning restore 0649

    public void PlayParticleSystem(int index)
    {
        particleSystemArray[index].Play();
    }
}
