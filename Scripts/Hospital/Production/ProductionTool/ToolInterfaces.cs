using UnityEngine;
using System.Collections;
using IsoEngine;

namespace Hospital
{
	/// <summary>
	/// Interface used by ProbeTableTool in collecting mode. 
	/// </summary>
	public interface ICollectable
	{
		bool Collect(Vector2i position);
	}

	/// <summary>
	/// Interface used by ProbeTableTool in filling mode.
	/// </summary>
	public interface IFillable
	{
		bool Fill(Vector2i position, MedicineRef medicine);
	}
}