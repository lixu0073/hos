using IsoEngine;
using UnityEngine;
using Hospital;

namespace Maternity
{
    public class MaternityBasePersonAI : BasePatientAI
    {
        public string ID;

        public override string SaveToString()
        {
            return "";
        }

        public override void Initialize(Vector2i pos)
        {
            walkingStateManager = new StateManager();
            transform.position = new Vector3(pos.x, 0, pos.y);
            position = pos;

            sprites = GetComponent<BaseCharacterInfo>();
            
            // TODO add patients to list
            patients.Add(this);

            anim = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
            ListeningForPath = false;
        }

        public override void Initialize(RotatableObject room, string info) {}
        public override void Initialize(string info, int timeFromSave) {}

        public override void IsoDestroy()
        {
            ListeningForPath = false;
            // TODO add patients to list
            //patients.Remove(this);
            //RemoveHappyEffect();
            if (this != null && gameObject != null)
                GameObject.Destroy(gameObject);
        }

    }
}
