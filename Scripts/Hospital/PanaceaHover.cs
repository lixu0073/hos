using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class PanaceaHover : MonoBehaviour
{
    public Image glow;
    public Vector3 axis;
	Vector3 pos;
	Transform tr;
	// Use this for initialization
	void Start()
	{
		pos = transform.localPosition;
		tr = transform;
	}

	// Update is called once per frame
	void Update()
	{
        //tr.localPosition = pos+axis * Mathf.Sin(Time.time);
        tr.localPosition = pos + axis * Mathf.SmoothStep(0, 1, Mathf.PingPong(Time.time, 1));

    }

    public void ShowGlow()
    {
        glow.enabled = true;
    }

    public void HideGlow()
    {
        glow.enabled = false;
    }

}
