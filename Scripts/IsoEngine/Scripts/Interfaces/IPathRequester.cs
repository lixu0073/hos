using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoEngine
{
	/// <summary>
	/// Implement this interface to be eligible to get path from PFMapController.
	/// To Make order use function MakeOrderForPath from PFMapController.
	/// </summary>
	public interface IPathRequester
	{
		/// <summary>
		/// What mapController should do while delivering path. Remember to not use UNITY API and to check if found path is null!
		/// </summary>
		/// <param name="Path"></param>
		 void SetPath(InterLevelPathInfo Path);
    }
}
