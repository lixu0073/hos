using UnityEngine;
using System.Collections;

namespace IsoEngine
{
#pragma warning disable 649
	/// <summary>
	/// Controller used to store reference to the global prefab data.
	/// </summary>
	internal class IsoObjectPrefabController : MonoBehaviour
	{
		/// <summary>
		/// Reference to the object storing common prefab data.
		/// </summary>
		[SerializeField]
		internal IsoObjectPrefabData prefabData;
	}
#pragma warning restore 649
}