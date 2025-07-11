public interface IHospitalPatientState 
{
	void OnEnter();
	void OnUpdate();
	void OnExit ();
	void Notify (int id, object parameters);
	string SaveToString ();

	void ToGoToChangeRoomState ();
	void ToInBedState ();
	void ToGoToDiagRoomState();
	void ToHealingState();
	void ToReturnToBedState();
	void ToGoHomeState();
	void ToGoToRoomState();
}
