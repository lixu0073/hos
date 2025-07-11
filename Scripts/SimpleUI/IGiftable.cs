using UnityEngine;
using System.Collections;

public abstract class IGiftable : MonoBehaviour
{
    public abstract Vector3 GetPosition();
    public abstract Vector3 GetTransformPosition();
    public abstract void RunItemAddedAnimation();
}
