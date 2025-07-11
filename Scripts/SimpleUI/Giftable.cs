using UnityEngine;
using System.Collections;


public class Giftable : IGiftable {

    public Animator anim;
    Vector2 position;
    

    void Start()
    {
        //moved to Start so the default position is cached and used instead of actual position
        //which might be outside the screen frustrum (when UI is moved out)
        position = transform.position;
        //Debug.LogError("Giftable name and position = " + name + transform.position);
    }

    public override Vector3 GetPosition() {
        //return transform.position;
        //Debug.LogError("Giftable GetPosition = " + name + transform.position);
        return position;
    }

    public override Vector3 GetTransformPosition()
    {
        //return transform.position;
        //Debug.LogError("Giftable GetPosition = " + name + transform.position);
        return transform.position;
    }

    public override void RunItemAddedAnimation() {
        if(anim != null)
            anim.SetTrigger("Giftable");
    }
}
