using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorToParticleSystemComunicator : MonoBehaviour
{
    public ParticleSystem particleSystem;

    public void Play()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }
}
