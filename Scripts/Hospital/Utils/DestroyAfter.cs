using UnityEngine;
using System.Collections;

namespace Hospital
{
		public class DestroyAfter : MonoBehaviour
		{
			public float time = 5;
			// Use this for initialization
			void Start()
			{
				Invoke("Destroy", time);
			}

			public void Destroy()
			{
				GameObject.Destroy(gameObject);
			}
		}
}