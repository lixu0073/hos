using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoEngine
{
	/// <summary>
	/// Interface for all the object with Load, Unload and Destroy functionality
	/// </summary>
	public interface ILoadable
	{
		/// <summary>
		/// Tells whether the object is loaded into memoory or not.
		/// When set to false the object is not displayed and updated.
		/// </summary>
		bool IsLoaded
		{
			get;
		}

		/// <summary>
		/// Loads the object to the memory. It makes it visible on the map. The object will start getting updates.
		/// Throws IsoException when invoked on already loaded object.
		/// </summary>
		void Load();

		/// <summary>
		/// Unloads the object from the memory. It makes it invisible on the map. The object will not get any updates.
		/// Throws IsoException when invoked on already unloaded object.
		/// </summary>
		void Unload();

		bool IsCreated
		{
			get;
		}

		/// <summary>
		/// Destroys object and unloads it from memory. It is forbidden to use the destroyed object as it's data might be inconsistent.
		/// </summary>
		void IsoDestroy();
	}
}
