using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GlobalEventController))]
public class GlobalEventControllerEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GlobalEventController myTarget = (GlobalEventController)target;

        if (GUILayout.Button("AddFakeReward"))
        {
            myTarget.FakeAddGlobalEventReward();
        }

        if (GUILayout.Button("GenTestEvents"))
        {
            myTarget.TestEventAWSGenerator();
        }

        if (GUILayout.Button("IncrementContribution"))
        {
            myTarget.TestIncrementPersonalGoal();
        }
    }
}
