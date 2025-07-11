using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleUI
{
    public class StarsController : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] stars = null;

        public void SetStarsVisible(int number)
        {
            if (stars == null)
            {
                Debug.LogError("stars array is null");
                return;
            }

            number = Mathf.Clamp(number, 0, stars.Length);

            for (int i = 0; i < stars.Length; ++i)
            {
                stars[i].SetActive(false);
            }

            for (int i = 0; i < number; ++i)
            {
                stars[i].SetActive(true);
                Animator anim = stars[i].GetComponent<Animator>();
                anim.SetTrigger("Bump");
            }
        }
    }
}