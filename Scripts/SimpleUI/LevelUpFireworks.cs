using UnityEngine;
using System.Collections;

public class LevelUpFireworks : MonoBehaviour {

    public GameObject[] fireworks;

	/*  void Start()
    {
        SetScale();
    }

		void SetScale()
    {
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        if (aspectRatio > 1.75f)   //16:9 ratio is 1.7778
            transform.localScale = Vector3.one;
        else if (aspectRatio > 1.55f)   //16:10 ratio is 1.6 
            transform.localScale = Vector3.one * 0.9f;
        else  //4:3 or 3:2
            transform.localScale = Vector3.one * 0.8f;
    }*/

    public void Fire()
    {
        for (int i = 0; i < fireworks.Length; i++)
        {
            fireworks[i].SetActive(true);
        }

        Invoke("Stop", 5f);
    }

    void Stop()
    {
        for (int i = 0; i < fireworks.Length; i++)
        {
            fireworks[i].SetActive(false);
        }
    }

}
