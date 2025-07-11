using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class setOrderInLayer : MonoBehaviour
{
	public int baseLayerOrder;

	public int bottomLayer;
	public int topLayer;
	//public float newLayer;
	int lastSortinglayer = 0;
	private SpriteRenderer sprite;
	public bool onTop = false;
	public bool onBottom = false;

	public bool forceUpdateOrderInLayer = false;


	// Use this for initialization
	void Awake()
	{
		lastSortinglayer = 0;
		//showInEditMode=false;

		if (bottomLayer == topLayer)
		{
			bottomLayer = 0;
			topLayer = 14;
		}
		sprite = gameObject.GetComponent<SpriteRenderer>();
		//baseLayerOrder = sprite.sortingOrder;

	}



	public void setNewOrder(int newSotrLayer)
	{


		lastSortinglayer = newSotrLayer;
		if (!sprite)
		{
			sprite = gameObject.GetComponent<SpriteRenderer>();
		}
		if (sprite)
		{
			int tempLayer = baseLayerOrder;


			if (onBottom)
			{
				tempLayer = bottomLayer;
			}

			if (onTop)
			{
				tempLayer = topLayer;
			}

			sprite.sortingOrder = newSotrLayer + tempLayer;
		}
		//baseLayerOrder = sprite.sortingOrder;
	}


	public void setBaseLayer(int value)
	{

		baseLayerOrder = value;
	}


	// Update is called once per frame
	void Update()
	{

		if (forceUpdateOrderInLayer)
        {
            Debug.LogError("forceUpdateOrderInLayer");
            if (!sprite)
			{
				sprite = gameObject.GetComponent<SpriteRenderer>();
			}

			if (sprite)
			{
				if (onBottom && sprite.sortingOrder != lastSortinglayer + bottomLayer)
				{
					sprite.sortingOrder = lastSortinglayer + bottomLayer;
				}

				if (onTop && sprite.sortingOrder != lastSortinglayer + topLayer)
				{
					sprite.sortingOrder = lastSortinglayer + topLayer;
				}

				if (!onTop && !onBottom & sprite.sortingOrder != lastSortinglayer + baseLayerOrder)
				{
					sprite.sortingOrder = lastSortinglayer + baseLayerOrder;
				}
			}
		}

	}

}
