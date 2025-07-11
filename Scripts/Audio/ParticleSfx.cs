using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using System;

/// <summary>
/// 粒子音效控制器，负责在粒子系统播放时同步触发相应的音效
/// </summary>
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
