using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IsoEngine
{
	class IsoSpriteLayer : IsoLayer
	{
		private int sortingHeight;

		public IsoSpriteLayer(EngineController engineController, IsoTile isoTile, IsoLayerData tileData, bool isObjectPart = false)
			: base(engineController, isoTile, tileData, isObjectPart)
		{
			this.sortingHeight = 0;
		}

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
					//this.layerObject.GetComponent<SpriteRenderer>().sortingOrder = this.sortingHeight;
				}
			}
		}

		public override void Load()
		{
			if (IsLoaded)
				throw new IsoException("Object is already loaded");

			//sprite
			//layerObject = engineController.SpritePool.GetObject();

			//layerObject.transform.position = new Vector3(isoTile.X, Spacing, isoTile.Y);
			//layerObject.transform.localScale = new Vector3(1, 1, 1);
			//layerObject.transform.rotation = Quaternion.Euler(90, 0, 0);

			//SpriteRenderer spriteRenderer = layerObject.GetComponent<SpriteRenderer>();

			//spriteRenderer.sprite = engineController.sprites[SpriteID];
			//spriteRenderer.sortingLayerName = "TileGround";
			//spriteRenderer.sortingOrder = sortingHeight;

			//layerObject.transform.SetParent(engineController.Map.GetLevel(isoTile.LevelID).transform, false);

			//layerObject.SetActive(true);

			IsLoaded = true;
		}

		public override void Unload()
		{
			if (!IsLoaded)
				throw new IsoException("Object is already unloaded");

			//engineController.SpritePool.ReleaseObject(layerObject);

			IsLoaded = false;
		}

		public override void IsoDestroy()
		{
			base.IsoDestroy();

			if(IsLoaded)
			{
				Unload();
			}
		}
	}
}
