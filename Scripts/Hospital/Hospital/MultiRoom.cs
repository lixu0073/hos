using UnityEngine;
using System.Collections;
using IsoEngine;
using System.Collections.Generic;
using Hospital;
using SimpleUI;
using System.Linq;
public class MultiRoom : RotatableObject
{


	public override void SetAnchored(bool value)
	{
		base.SetAnchored(value);
	}

	//public override void Initialize(IsoRotatableData data)
	//{
	//    base.Initialize(data);

	//}
	public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State state = State.fresh, bool shouldDisappear = true)
	{
		base.Initialize(info, position, rotation, state,shouldDisappear);
	}

	protected override void AddToMap()
	{
		base.AddToMap();
	}
	public Vector2i ReacquireTakenSpot(int id)
	{
		return position;
	}

	private void SetSpots()
	{

	}
	public Vector2i GetEntrancePosition()
	{
		return position;
	}
	private void SpawnPerson()
	{

	}

	public void ReturnTakenSpot(int id)
	{

	}
	protected override void OnClickWorking()
	{
		base.OnClickWorking();
	}
	public int GetBedSpot()
	{
		return -1;
	}



	public override void IsoUpdate()
	{
		base.IsoUpdate();
	}
	public enum SpotTypes
	{
		Door,
		CorridorChair,
		InteriorChair,
		DoctorChair,
		Machine,
		FileCase,
		DiagnosticConsole,
		HospitalBed
	}
}
