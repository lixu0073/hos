using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;

public class DepotCaseController : MonoBehaviour {
	private Animator animator;
	void Awake(){
		animator = GetComponent<Animator> ();
	}
	void OnEnable(){
		Timing.RunCoroutine (DelayedJumping(GameState.RandomFloat(0, 2f))); 
	}
	IEnumerator<float> DelayedJumping(float delay){
		yield return Timing.WaitForSeconds (delay);
		animator.SetTrigger ("CaseJumping");
	}
}
