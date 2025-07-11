using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimatorRandomizer : MonoBehaviour {


	private Animator anim;
	public float min = 0.9f;
	public float max = 1.1f;

	void Start () {

		anim = gameObject.GetComponent<Animator> ();
		anim.speed = Random.Range (min, max);
	}
	
	void Update () {
	
	}
}
