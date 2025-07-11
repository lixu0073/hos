
public abstract class BaseState<TParent> : IState
{
	protected TParent parent;

	public BaseState(TParent parent)
	{
		this.parent = parent;
	}

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnUpdate()
	{ 
	}
	
	public virtual void Notify(int id, object parameters)
	{
	}
	public virtual string SaveToString()
	{
		return "";
	}
}

