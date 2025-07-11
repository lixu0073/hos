public class MaternityWaitingRoomBedStateManager<Parent> where Parent : IStateChangable 
{
    Parent parent;
    public BedIState State
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
    private BedIState state;

    public MaternityWaitingRoomBedStateManager(Parent parent)
    {
        this.parent = parent;
    }


    public void Update()
    {
        if (state != null)
            state.OnUpdate();
    }
}

public interface IStateChangable
{
    void NotifyStateChanged();
}