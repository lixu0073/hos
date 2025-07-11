using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IsoEngine
{
	class IsoQuadBackground : IsoElement, ILoadable
	{
		public readonly int x;
		public readonly int y;
		public readonly int width;
		public readonly int height;
		public readonly int materialID;
		public readonly int levelID;
		
		private GameObject gameObject;

		public bool IsCreated
		{
			get;
			private set;
		}

		public bool IsLoaded
		{
			get;
			private set;
		}

		public IsoQuadBackground(EngineController engineController, int levelID, IsoBackgroundData backgroundData)
			: base(engineController)
		{
			this.levelID = levelID;

			this.x = backgroundData.x;
			this.y = backgroundData.y;
			this.width = backgroundData.width;
			this.height = backgroundData.height;
			
			this.materialID = backgroundData.materialID;

			this.IsLoaded = false;

			this.IsCreated = true;
		}

		public void IsoDestroy()
		{
			if (!IsCreated)
				throw new IsoException("Background is already destroyed");

			UnityEngine.Object.Destroy(gameObject);
		}

		public void Load()
		{
			if (IsLoaded)
				throw new IsoException("Background is already loaded");

			if(gameObject == null)
			{
				gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);

				Component.Destroy(gameObject.GetComponent<MeshCollider>());
				gameObject.transform.position = new Vector3(x + (float)(width - 1) / 2.0f, 0, y + (float)(height - 1) / 2.0f);
				gameObject.transform.localScale = new Vector3(width, height, 1);
				gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
				gameObject.GetComponent<Renderer>().material = engineController.materials[materialID];

				gameObject.transform.SetParent(engineController.Map.GetLevel(levelID).transform, false);
				gameObject.name = "Background lvl" + levelID.ToString();
				gameObject.GetComponent<Renderer>().sortingLayerName = "Ground";
			}

			gameObject.SetActive(true);
			IsLoaded = true;
		}

		public void Unload()
		{
			if (!IsLoaded)
				throw new IsoException("Background is already unloaded");

			gameObject.SetActive(false);
			IsLoaded = false;
		}

		public IsoBackgroundData GetData()
		{
			IsoBackgroundData data = new IsoBackgroundData(x, y, width, height, materialID);

			return data;
		}
	}
}
