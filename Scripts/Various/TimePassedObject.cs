public abstract class TimePassedObject
{

    protected long hospitalSaveTime;
    protected long maternitySaveTime;

    protected long hospitalTimePassed;
    protected long maternityTimePassed;

    public TimePassedObject(long hospitalSaveTime, long maternitySaveTime)
    {
        this.hospitalSaveTime = hospitalSaveTime;
        this.maternitySaveTime = maternitySaveTime;
        hospitalTimePassed = ((long)ServerTime.getTime() - hospitalSaveTime);
        maternityTimePassed = ((long)ServerTime.getTime() - maternitySaveTime);
#if UNITY_EDITOR
        if (DeveloperParametersController.Instance().parameters.hospitalTimePassed!=-1)
        {
            hospitalTimePassed = DeveloperParametersController.Instance().parameters.hospitalTimePassed;
        }
        if (DeveloperParametersController.Instance().parameters.maternityTimePassed !=-1)
        {
            maternityTimePassed = DeveloperParametersController.Instance().parameters.maternityTimePassed;
        }
#endif
    }

    public long GetSmallestTimePassedFromAllTimes()
    {
        return System.Math.Min(hospitalTimePassed, maternityTimePassed);
    }

    public abstract long GetSaveTime();
    public abstract long GetTimePassed();
}
