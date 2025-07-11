using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IsoEngine
{
	public class IsoQuadLayer : IsoLayer
	{
		public IsoQuadLayer(EngineController engineController, IsoTile isoTile, IsoLayerData tileData, bool isObjectPart = false)
			: base(engineController, isoTile, tileData, isObjectPart)
		{
			sortingHeight = 0;
		}

		private int sortingHeight;

		internal override int SortingHeight
		{
			get
			{
				return sortingHeight;
			}

			set
			{
				this.sortingHeight = value;

				if (IsLoaded)
				{
					layerObject.transform.localPosition = new Vector3(layerObject.transform.localPosition.x, Spacing * (sortingHeight + 1), layerObject.transform.localPosition.z);
				}
			}
		}

		public override void Load()
		{
			if (IsLoaded)
				throw new IsoException("Object is already loaded");

			if (layerObject == null)
			{
				layerObject = UnityEngine.Object.Instantiate(engineController.Quad);

				layerObject.transform.SetParent(engineController.Map.GetLevel(isoTile.LevelID).transform);

				MeshRenderer meshRenderer = layerObject.GetComponent<MeshRenderer>();

				meshRenderer.material = engineController.materials[SpriteID];
				meshRenderer.sortingLayerName = "TileGround";
				meshRenderer.sortingOrder = sortingHeight;

				layerObject.transform.localPosition = new Vector3(isoTile.X, Spacing, isoTile.Y);
				layerObject.transform.localScale = new Vector3(1, 1, 1);
				layerObject.transform.rotation = Quaternion.Euler(90, 0, 0);
			}

			layerObject.SetActive(true);

			IsLoaded = true;
		}

		public override void Unload()
		{
			if (!IsLoaded)
				throw new IsoException("Object is already unloaded");

			layerObject.SetActive(false);

			IsLoaded = false;
		}

		public override void IsoDestroy()
		{
			base.IsoDestroy();

			if (layerObject != null)
			{
				GameObject.Destroy(layerObject);

				layerObject = null;
			}
		}
	}
}
