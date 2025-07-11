using UnityEngine;
using System.Collections;

/// <summary>
/// 水粒子脚本，用于控制多个粒子系统的播放。
/// </summary>
public class water_particle_script : MonoBehaviour {
	// 水粒子系统1
	public ParticleSystem waterPartSystem1;
	// 水粒子系统2
	public ParticleSystem waterPartSystem2;
	// 水粒子系统3
	public ParticleSystem waterPartSystem3;
	// 水粒子系统4
	public ParticleSystem waterPartSystem4;
	// 水粒子系统5
	public ParticleSystem waterPartSystem5;
	// 水粒子系统6
	public ParticleSystem waterPartSystem6;

	/// <summary>
	/// 播放所有水粒子系统。
	/// </summary>
	public void playWater(){
		//waterPartSystem.GetComponent<ParticleSystem> ();
		waterPartSystem1.Play ();
		waterPartSystem2.Play ();
		waterPartSystem3.Play ();
		waterPartSystem4.Play ();
		waterPartSystem5.Play ();
		waterPartSystem6.Play ();
	}
}