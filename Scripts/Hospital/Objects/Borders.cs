using UnityEngine;
using System.Collections;
using IsoEngine;
using System.Collections.Generic;
using System;

public class Borders : MonoBehaviour
{
    [SerializeField]
    private LineRenderer line1 = null;
    [SerializeField]
    private LineRenderer line2 = null;
    [SerializeField]
    private LineRenderer line3 = null;
    [SerializeField]
    private LineRenderer line4 = null;

    private int mode = 0;

    private Material mat = null;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void SetBorderSize(int x, int y)
    {
        line1.SetPositions(new Vector3[] { new Vector3(0, 0, 0), new Vector3(x, 0, 0) });
        line2.SetPositions(new Vector3[] { new Vector3(x, 0, -0.05f), new Vector3(x, 0, y + 0.05f) });
        line3.SetPositions(new Vector3[] { new Vector3(x, 0, y), new Vector3(0, 0, y) });
        line4.SetPositions(new Vector3[] { new Vector3(0, 0, y + 0.05f), new Vector3(0, 0, -0.05f) });
    }

    public void DestroyMaterial()
    {
        EngineController.DestroyMaterial(line1);
        EngineController.DestroyMaterial(line2);
        EngineController.DestroyMaterial(line3);
        EngineController.DestroyMaterial(line4);
    }

    Coroutine pulseAnimation;

    public void StartPulseAnimation(Vector3 fromScale, Vector3 toScale, float secondsPerAnimationCycle, AnimationCurve curve = null)
    {
        pulseAnimation = StartCoroutine(PulseAnimation(fromScale, toScale, secondsPerAnimationCycle, curve));
    }

    public void StartPulseAnimation()
    {
        pulseAnimation = StartCoroutine(PulseAnimation(Vector3.one, Vector3.one * 1.25f, 2f, null));
    }

    public void EndPulseAnimation()
    {
        try {
            if (pulseAnimation != null)
            {
                StopCoroutine(pulseAnimation);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }

    IEnumerator PulseAnimation(Vector3 fromScale, Vector3 toScale, float secondsPerAnimationCycle, AnimationCurve curve = null)
    {
        float step = 0f;
        float value = 0f;
        while (true)
        {
            if (step >= 1)
                step = 0f;

            if (curve != null)
                value = curve.Evaluate(step);
            else
                value = step;

            step += Time.deltaTime / secondsPerAnimationCycle;
            this.transform.localScale = Vector3.Lerp(fromScale, toScale, value);

            yield return null;
        }
    }

    Material[] materials = new Material[4];

    public void SetBorderColor(int i, Hospital.Rotation rot)
    {
        DestroyMaterial();
        switch (i)
        {
            case 3:
                if (materials[i] == null)
                    materials[i] = ResourcesHolder.Get().mutalPathBorder;
                mode = 3;
                break;
            case 2:
                if (materials[i] == null)
                    materials[i] = ResourcesHolder.Get().collisionBorder;
                mode = 2;
                break;
            case 1:
                if (materials[i] == null)
                    materials[i] = ResourcesHolder.Get().buildableBorder;
                mode = 1;
                break;
            default:
                if (materials[i] == null)
                    materials[i] = ResourcesHolder.Get().activeBorder;
                mode = 0;
                break;
        }
        mat = materials[i];
        line1.sharedMaterial = line2.sharedMaterial = line3.sharedMaterial = line4.sharedMaterial = mat;

        if (mat.HasProperty("_MainTex"))
        {
            if (rot == Hospital.Rotation.East || rot == Hospital.Rotation.West)
            {
                line1.sharedMaterial.mainTextureScale = new Vector2(9, 1);
                line3.sharedMaterial.mainTextureScale = new Vector2(9, 1);

                line2.sharedMaterial.mainTextureScale = new Vector2(3, 1);
                line4.sharedMaterial.mainTextureScale = new Vector2(3, 1);
            }
            else
            {
                line1.sharedMaterial.mainTextureScale = new Vector2(3, 1);
                line3.sharedMaterial.mainTextureScale = new Vector2(3, 1);

                line2.sharedMaterial.mainTextureScale = new Vector2(9, 1);
                line4.sharedMaterial.mainTextureScale = new Vector2(9, 1);
            }
        }
        //	print("setting color " + i);
    }

    private void OnDestroy() {
        DestroyMaterial();
    }

    public int GetMode()
    {
        return mode;
    }
}
