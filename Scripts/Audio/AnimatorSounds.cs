using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hospital;

public class AnimatorSounds : MonoBehaviour {
    public void PlaySound(int index) {
		SoundsController.Instance.PlayAnySound (index);
	}

	public void StopSound(int index) {
		SoundsController.Instance.StopAnySound (index);
	}
}
