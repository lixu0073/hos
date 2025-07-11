
using Hospital;
using Maternity;
using Maternity.PatientStates;
using Maternity.UI;

public interface IState
{
	void OnEnter();
	void OnExit();
	void OnUpdate();
	void Notify(int id, object parameters);
	string SaveToString();
}

public interface BedIState : IState
{
    MaternityWaitingRoomBed.State GetTag();
    void EmulateTime(TimePassedObject timePassed);
    void BroadcastData();
    void OnEmulationEnded();
    MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator();
}

public interface MaternityIState : IState
{
    MaternityIState GetNextStateOnLoad();
    MaternityPatientStateTag GetTag();
    void EmulateTime(TimePassedObject timePassed);
    void BroadcastData();
    MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator();
    void MoveTo();
    RotatableObject GetRoom();
}

public interface BloodTestIState : IState
{
    BloodTestRoomState GetTag();
    void EmulateTime(TimePassedObject timePassed);
    void BroadcastData();
}

public interface ISubState<T> where T: MaternityIState
{
    void OnEnter();
    void OnExit();
    void OnUpdate();
    void Notify(int id, object parameters);
}
