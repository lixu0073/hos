public class RefactoredStateManager {
	
	private IHospitalPatientState state;
	public IHospitalPatientState State{
		get{ return state;}
		set{ 
			if (state == value)
				return;
			
			if(state != null){
				state.OnExit();
			}

			state = value;

			if (state != null) {
				state.OnEnter ();
			}
		}
	}


	public void Update () {
		if (state != null)
			state.OnUpdate();
	}
}
