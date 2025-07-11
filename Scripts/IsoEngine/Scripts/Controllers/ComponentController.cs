using UnityEngine;

namespace IsoEngine
{
	/// <summary>
	/// Base for all controllers of Engine. Do not use Unity Start() in these classes.
	/// </summary>
	public abstract class ComponentController : MonoBehaviour
	{
		protected EngineController engineController
		{
			get;
			private set;
		}

		internal virtual void Initialize()
		{
			if (engineController == null)
			{
				engineController = transform.root.GetComponent<EngineController>();
			}
			else
			{
				throw new IsoException("Component is already initialized");
			}
		}

		public virtual void IsoDestroy()
		{

		}
	}
}