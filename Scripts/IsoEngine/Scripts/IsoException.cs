using UnityEngine;
using System.Collections;

namespace IsoEngine
{
	/// <summary>
	/// Exception class used to pass error information across the entire engine.
	/// </summary>
	public class IsoException : System.Exception
	{
		internal IsoException(string message) : base(message)
		{
			
		}
	}
}