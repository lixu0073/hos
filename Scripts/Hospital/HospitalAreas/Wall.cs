using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using IsoEngine;

namespace Hospital
{
	internal class Wall
	{
		private static GameObject temp;
		private GameObject obj;
		public readonly int priority;
		public bool isDoor;
		public WallType wallID;

        public void SetDoors(bool val)
		{

		}
		public Wall(int Priority, GameObject Obj, WallType wallID,bool IsDoor=false)
		{
			obj = Obj;
			priority = Priority;
			isDoor = IsDoor;
			this.wallID = wallID;
        }
		public void SetActive(bool value)
		{
			//Debug.Log("setting wall active");
			if (obj != null)
			{
				//Debug.Log("Set");
				obj.SetActive(value);
			}
			//else
				//Debug.Log("not set");
		}
		public void Destroy()
		{
			//GameObject.Destroy(obj);
			if (obj != null)
			{
       
                if (isDoor && obj.GetComponent<Doors>()) // if wall is a door the delete it from map
                {
                    if (ReferenceHolder.Get().engine.Map.GetLevel<HospitalAreasLevelController>(0))
                    {
                        ReferenceHolder.Get().engine.Map.GetLevel<HospitalAreasLevelController>(0).map.RemoveDoorFromMap(obj.GetComponent<Doors>());
                    }
                }

				temp = obj;
                obj = null;
				GameObject.Destroy(temp);
			}
		}
	}
}
