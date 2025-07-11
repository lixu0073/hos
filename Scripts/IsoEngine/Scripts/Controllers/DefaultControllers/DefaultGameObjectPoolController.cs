using UnityEngine;
using System.Collections.Generic;

namespace IsoEngine
{
	/// <summary>
	/// Class used for storing temporarily unused object that are frequently created/changed.
	/// </summary>
	public sealed class DefaultGameObjectPoolController : ComponentController
	{
		// Fields
		public int poolSize = 50;

		public GameObject pooledObject;

		//private List<GameObject> freeObjects;
		//private List<GameObject> usedObjects;

		// Methods
		internal override void Initialize()
		{
			base.Initialize();

			//freeObjects = new List<GameObject>(poolSize);
			//usedObjects = new List<GameObject>(poolSize);

			//for (int i = 0; i < freeObjects.Capacity; ++i)
			//	AddObject(i);
		}


		//private void AddObject(int index)
		//{
		//	string name = "PooledGameObject_" + index.ToString();
		//	GameObject instance = Object.Instantiate(pooledObject);
		//	instance.SetActive(false);
		//	instance.name = name;
		//	instance.transform.SetParent(this.gameObject.transform, false);
		//	instance.GetComponent<Renderer>().sortingLayerName = "TileGround";
		//	freeObjects.Add(instance);
		//}

		/// <summary>
		/// Returns object from pool. In case of lack of objects creates new one.
		/// </summary>
		/// <returns></returns>
		//public GameObject GetObject()
		//{
		//	// Add new objects to pool
		//	if (freeObjects.Count == 0)
		//		AddObject(usedObjects.Count);

		//	GameObject ret = freeObjects[freeObjects.Count - 1];
		//	freeObjects.RemoveAt(freeObjects.Count - 1);
		//	usedObjects.Add(ret);

		//	return ret;
		//}

		/// <summary>
		/// Release used object and returns it to pool of unused.
		/// </summary>
		/// <param name="gameObject"></param>
		//public void ReleaseObject(GameObject gameObject)
		//{
		//	int index = usedObjects.IndexOf(gameObject);

		//	if (index == -1)
		//		throw new UnityException("Released object not found in pool");

		//	// Swap freed object with last
		//	usedObjects[index] = usedObjects[usedObjects.Count - 1];
		//	usedObjects[usedObjects.Count - 1] = gameObject;

		//	// Remove last
		//	usedObjects.RemoveAt(usedObjects.Count - 1);

		//	freeObjects.Add(gameObject);

		//	gameObject.transform.SetParent(this.gameObject.transform, false);
		//	gameObject.SetActive(false);
		//}
	}
}