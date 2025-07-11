using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSceneLoader : MonoBehaviour
{

	IEnumerator Start ()
    {
        yield return new WaitForSeconds(2.0f);
        Loaded();
    }

    protected abstract void Loaded();
	
}
