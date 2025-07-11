using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleUI;
using IsoEngine;

namespace SimpleUI
{
    public class Collectable : MonoBehaviour {

        public Animator anim;
        public Image glow;
        public Image icon;
        public MedicineRef medicine;
        public int amount;
        public bool isPositiveEnergy;
        public string patientID;


        public void Initialize(Sprite image, MedicineRef medicine, int amount, bool isPositiveEnergy, string patientID) {
            this.medicine = medicine;
            this.amount = amount;
            this.isPositiveEnergy = isPositiveEnergy;
            icon.sprite = image;
            this.patientID = patientID;
        }

        public void ShowGlow() {
            glow.enabled = true;
        }

        public void HideGlow() {
            glow.enabled = false;
        }

        public void StartBumping() {
            if (anim!=null)
                anim.SetBool("IsBumping", true);
        }
        public void StopBumping() {
            if (anim != null)
                anim.SetBool("IsBumping", false);
        }
    }
}