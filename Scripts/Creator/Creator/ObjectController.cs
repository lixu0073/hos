#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

	[ExecuteInEditMode]
	public class ObjectController : MonoBehaviour
	{
		public int tilesX = 1;
		public int tilesZ = 1;

		Transform parent;

		Vector3 position;

		[HideInInspector]
		public int startX = 0;

		[HideInInspector]
		public int startZ = 0;

		[HideInInspector]
		public bool isValid = false;

		// Use this for initialization
		void Start()
		{
			parent = transform.parent;
			OnParentChange(null, parent);

			//position = transform.position;
			OnPositionChange(new Vector3(), position);
		}

		// Update is called once per frame
		void Update()
		{
			// Changed parent
			if (parent != transform.parent)
			{
				OnParentChange(parent, transform.parent);
				OnPositionChange(new Vector3(), transform.position);
				parent = transform.parent;
			}

			// Changed position
			if (position != transform.position)
			{
				OnPositionChange(position, transform.position);
				position = transform.position;
			}
		}

		private void OnParentChange(Transform oldParent, Transform newParent)
		{
			if (newParent != null)
			{
				Debug.Log("Parent changed to " + newParent.name);


				CreatorController creatorController = newParent.GetComponent<CreatorController>();

				// Parent changed to creator controller
				if (creatorController != null)
				{
					creatorController.AddObject(this);
				}
			}
			else
			{
				Debug.Log("Parent changed to none");
			}

			if (oldParent != null)
			{
				CreatorController creatorController = oldParent.GetComponent<CreatorController>();

				// Parent changed from creator controller
				if (creatorController != null)
				{
					creatorController.RemoveObject(this);
				}
			}
		}

		private void OnPositionChange(Vector3 oldPosition, Vector3 newPosition)
		{
			startX = (int)(newPosition.x - (tilesX - 2) * 0.5f);
			startZ = (int)(newPosition.z - (tilesZ - 2) * 0.5f);
		}
	}
#endif