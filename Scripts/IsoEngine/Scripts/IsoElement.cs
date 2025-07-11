using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoEngine
{
	/// <summary>
	/// Base class for every object in the engine.
	/// </summary>
	public abstract class IsoElement
	{
		protected EngineController engineController;

		protected IsoElement(EngineController engineController)
		{
			this.engineController = engineController;
		}
	}
}
