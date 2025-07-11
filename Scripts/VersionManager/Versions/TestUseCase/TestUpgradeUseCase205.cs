using Hospital;
using System;
using System.Collections;
using UnityEngine;

public class TestUpgradeUseCase205 : MonoBehaviour
{
    private UpgradeUseCase_205 toTest;

#pragma warning disable 0649
    [SerializeField]
    private string testString;

    [SerializeField]
    private float testStartDelay;
#pragma warning restore 0649

    private void Awake()
    {
        toTest = new UpgradeUseCase_205();
    }

    private void Start()
    {
        StartCoroutine(TestCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    private IEnumerator TestCoroutine()
    {
        Debug.LogError(this.GetType().ToString() + ": Wait for Start (" + testStartDelay.ToString("n2") + ")");

        yield return new WaitForSecondsRealtime(testStartDelay);

        Debug.LogError(this.GetType().ToString() + ": Test Started");
        Debug.LogError(this.GetType().ToString() + ": before =" + testString);

        Save testSave = new Save();
        testSave.GlobalEvent = testString;

        try
        {
            toTest.Upgrade(testSave, false);

            Debug.LogError(this.GetType().ToString() + ": Upgrade done");

            var typeStr = testSave.GlobalEvent.Split(';');
            Type type = Type.GetType(typeStr[0]);
            System.Object obj = Activator.CreateInstance(type);
            (obj as GlobalEvent).LoadFromString(testSave.GlobalEvent);

            Debug.LogError(this.GetType().ToString() + ": TEST SUCCESFUL");
            Debug.LogError("Result = " + testSave.GlobalEvent);
        }
        catch(Exception e)
        {
            Debug.LogError(this.GetType().ToString() + ": ERROR");
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);            
        }
    }
}
