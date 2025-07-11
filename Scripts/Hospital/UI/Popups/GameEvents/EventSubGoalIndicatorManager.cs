using UnityEngine;

public class EventSubGoalIndicatorManager : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] GameObject eventSubGoalIndicatorPrefab;
#pragma warning restore 0649

    private EventSubGoalIndicator[] subGoals;

    public EventSubGoalIndicator[] GetSubGoals(int numberOf)
    {
        if(subGoals == null)
        {
            subGoals = new EventSubGoalIndicator[numberOf];
            GameObject current = null;

            for(int i = 0; i < numberOf; ++i)
            {
                current = Instantiate(eventSubGoalIndicatorPrefab, this.transform);
                subGoals[i] = current.GetComponent<EventSubGoalIndicator>();
            }
        }

        return subGoals;
    }
}
