using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColourHanger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (GameState.isHoverOn) {
			GetComponent<Image> ().color = Color.cyan;
		} else {
			GetComponent<Image> ().color = Color.magenta;
		}
	}
}
