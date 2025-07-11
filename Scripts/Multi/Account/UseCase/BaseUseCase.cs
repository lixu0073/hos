namespace Hospital
{
    public abstract class BaseUseCase
    {
        protected OnFailure onFailure = null;

        public abstract void UnbindCallbacks();

    }
}
