using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class TurboFPS : MonoBehaviour
{
	private int count;
	private float time;
	public Text tekst;

	// Use this for initialization
	void Start()
	{
		count = 0;
		time = Time.time;

	}
	public void Toggle()
	{
		gameObject.SetActive(!gameObject.activeSelf);
       // MessageController.instance.ShowMessage("FPS ON!");
	}

	// Update is called once per frame
	void Update()
	{
		count++;
		if (Time.time - time > 1.0f)
		{
			if (tekst != null)
				tekst.text ="FPS: "+ count.ToString();
			count = 0;
            time = Time.time;
        }
    }
}
