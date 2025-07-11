
using Maternity;

public class StateManager
{
    public IState State
    {
        get
        {
            return state;
        }

        set
        {
            if (state == value)
                return;

            if (state != null)
                state.OnExit();

            state = value;

            if (state != null)
                state.OnEnter();
        }
    }

    private IState state;
    private float lastUpdateTime = 0f;
    private float updateFreq = 2.0f;

    public void Update()
    {
        if (TutorialController.Instance.ram)
        {
            float time = UnityEngine.Time.time;
            if (time > lastUpdateTime + updateFreq)
            {
                if (state != null)
                    state.OnUpdate();
                lastUpdateTime = time;
            }
        }
        else
        {
            if (state != null)
                state.OnUpdate();
        }
    }
}

public class MaternityStateManager
{

    MaternityPatientAI parent;

    public MaternityIState State
    {
        get
        {
            return state;
        }

        set
        {
            if (state == value)
                return;

            if (state != null)
                state.OnExit();

            state = value;

            if (state != null)
                state.OnEnter();

            if (parent != null)
                parent.NotifyStateChanged();
        }
    }

    public MaternityStateManager(MaternityPatientAI parent)
    {
        this.parent = parent;
    }

    private MaternityIState state;

    public void Update()
    {
        if (state != null)
            state.OnUpdate();
    }
}

public class SubStateManager<T> where T : MaternityIState
{
    public ISubState<T> State
    {
        get
        {
            return state;
        }

        set
        {
            if (state == value)
                return;

            if (state != null)
                state.OnExit();

            state = value;

            if (state != null)
                state.OnEnter();
        }
    }
    private ISubState<T> state;

    public void Update()
    {
        if (state != null)
            state.OnUpdate();
    }
}