#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

	[ExecuteInEditMode]
	public class TileController : MonoBehaviour
	{
		private new ObjectController gameObject;

		[HideInInspector]
		public CreatorController creatorController;

		[HideInInspector]
		public int x;

		[HideInInspector]
		public int z;

		private bool isPassable = true;


        public enum SpotDirection
		{
			None,
			NegativeX,
			PositiveX,
			NegativeZ,
			PositiveZ
		}

		[HideInInspector]
		public SpotDirection spotDirection = SpotDirection.None;
        public Hospital.SpotTypes spotID = Hospital.SpotTypes.EmptySpot;
		public GameObject spot;

		private Vector3 position;
		private Vector3 scale;
		private Quaternion rotation;

		private void Start()
		{
			position = transform.position;
			scale = transform.localScale;
			rotation = transform.rotation;
		}

		private void Update()
		{
			if (transform.position != position)
				transform.position = position;

			if (transform.localScale != scale)
				transform.localScale = scale;

			if (transform.rotation != rotation)
				transform.rotation = rotation;
		}

		public void SetSpot()
		{
			if (spotDirection == SpotDirection.None)
			{
				if (spot != null)
				{
					Object.DestroyImmediate(spot);
				}
			}
			else
			{
				if (spot == null)
				{
					spot = Object.Instantiate(creatorController.arrow);

					spot.transform.SetParent(transform);
					spot.transform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
					spot.SetActive(true);
				}

				switch (spotDirection)
				{
					case SpotDirection.NegativeX:
						{
							spot.transform.rotation = Quaternion.Euler(90, 180, 0);
							break;
						}

					case SpotDirection.PositiveX:
						{
							spot.transform.rotation = Quaternion.Euler(90, 0, 0);
							break;
						}

					case SpotDirection.NegativeZ:
						{
							spot.transform.rotation = Quaternion.Euler(90, 90, 0);
							break;
						}

					case SpotDirection.PositiveZ:
						{
							spot.transform.rotation = Quaternion.Euler(90, 270, 0);
							break;
						}
				}

            }
		}

    public void RemoveSpot()
    {
            if (spot != null)
            {
                Object.DestroyImmediate(spot);
            }
    }

    public ObjectController GameObject
		{
			get
			{
				return gameObject;
			}

			set
			{
				gameObject = value;

				SetColor();
			}
		}

		public bool IsPassable
		{
			get
			{
				return isPassable && gameObject == null;
			}

			set
			{
				isPassable = value;


				SetColor();
			}
		}

        private void SetColor()
		{
			SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

			if (IsPassable)
				spriteRenderer.color = Color.white;
			else
				spriteRenderer.color = Color.red;

        }
	}
#endif