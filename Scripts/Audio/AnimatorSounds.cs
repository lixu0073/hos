using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hospital;

/// <summary>
/// 动画音效控制器，用于在动画播放过程中触发特定的音效播放和停止操作
/// </summary>
public class AnimatorSounds : MonoBehaviour {
    public void PlaySound(int index) {
		SoundsController.Instance.PlayAnySound (index);
	}

	public void StopSound(int index) {
		SoundsController.Instance.StopAnySound (index);
	}
}
