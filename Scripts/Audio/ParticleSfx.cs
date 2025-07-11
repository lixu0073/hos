using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using System;

public class ParticleSfx : MonoBehaviour {

	public int soundIndex;
	private ParticleSystem par;
	public bool isPlayable;
	// Use this for initialization
	void Start () {
		par = GetComponent<ParticleSystem> ();
		//isPlayable = true;
	}
	
	// Update is called once per frame
	void Update () {

		if (par.IsAlive ()) {
			PlayParticleSfx ();
		} else {
			isPlayable = true;
		}
	}

	void PlayParticleSfx () {
		if (!SoundsController.Instance.Sounds [soundIndex].isPlaying && isPlayable) {
				
			SoundsController.Instance.PlayAnySound (soundIndex);
			isPlayable = false;
		} 
	}
}
