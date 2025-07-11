using UnityEngine;
using System.Collections;
namespace Hospital
{
    public class CoverController : RotatableObject
    {
        public bool IsClickable = false;
        
        void Start()
        {
            GetComponent<RotatableSimpleController>().draggingEnabled = false;

            //przy wylaczeniu tutoriala trzeba odkomentowac ponizsze
            //RemoveCover();
            //HospitalAreasMapController.Map.reception.IsClickable = true;
        }

        protected override void OnClickWorking()
        {
            if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            if (IsClickable)
            {
                Debug.Log("ODPIECIE INDYKATORA");
                TutorialUIController.Instance.SetIndicatorParent(TutorialUIController.Instance.gameObject.transform);

                Debug.Log("Spawning Cover Particles");
                var fp = (GameObject)Instantiate(ResourcesHolder.GetHospital().ParticleCover, transform.position, Quaternion.Euler(0, 0, 0));
                fp.SetActive(true);

                SetAnchored(false);
                RemoveFromMap();
                ((AreaMapController)(ReferenceHolder.Get().engine.Map)).RemoveRotatableObject(this);
                Destroy(GetActualGameObject());
                IsoDestroy();
                
                foreach (CoverSpawnInfo spawn in ((CoverInfo)info.infos).SpawnedObjects)
                {
                    RotatableObject g = CreateRotatableObject(spawn.tag, spawn.pos, spawn.rot, State.working ,spawn.settings);
                }

                NotificationCenter.Instance.SheetRemove.Invoke(new SheetRemoveEventArgs(this.Tag));
            }
        }
        public void RemoveCover()
        {
            TutorialUIController.Instance.SetIndicatorParent(TutorialUIController.Instance.gameObject.transform);

            NotificationCenter.Instance.SheetRemove.Invoke(new SheetRemoveEventArgs(this.Tag));
            SetAnchored(false);
            RemoveFromMap();

            ((AreaMapController)(ReferenceHolder.Get().engine.Map)).RemoveRotatableObject(this);
            Destroy(GetActualGameObject());
            IsoDestroy();

            foreach (CoverSpawnInfo spawn in ((CoverInfo)info.infos).SpawnedObjects)
            {
                RotatableObject g = CreateRotatableObject(spawn.tag, spawn.pos, spawn.rot, State.working, spawn.settings);
            }
        }
    }
}

