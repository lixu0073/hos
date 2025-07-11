using UnityEngine;
using System.Collections.Generic;
using MovementEffects;
using Hospital;

public class Floor : MonoBehaviour
{
    public HospitalArea hospitalArea;

    private Vector3 last_color;
#pragma warning disable 0649
    [SerializeField] private bool isFloorWithChildren;
    [SerializeField] private float shaderDelay;
    [SerializeField] Vector3 floorPaintOffset;
#pragma warning restore 0649

    IEnumerator<float> updateFloorColor = null;


    public void RefreshFloorColor(Vector4 color, Vector3 startPos, bool fromSave = false)
    {
        if (!fromSave)
        {
            if (updateFloorColor != null)
            {
                Timing.KillCoroutine(updateFloorColor);
                UpdateCurrentFloorColor(last_color);

                updateFloorColor = null;
            }
            updateFloorColor = Timing.RunCoroutine(UpdateFloorColor(color, startPos));
            last_color = color;
        }
        else UpdateCurrentFloorColor(color);
    }

    private IEnumerator<float> UpdateFloorColor(Vector4 color, Vector3 startPos)
    {
        float paintingsTime = 0f;
        float timeMultiplier = 1f;

        yield return Timing.WaitForSeconds(shaderDelay);

        while (paintingsTime < 20)
        {
            paintingsTime += 0.12f;
            timeMultiplier = 1 + Mathf.Clamp(paintingsTime / 10f, 0, 2);
            UpdateNewFloorColor(color, startPos + floorPaintOffset, paintingsTime, timeMultiplier);
            yield return Timing.WaitForSeconds(0.01f);
        }

        UpdateCurrentFloorColor(color);
        updateFloorColor = null;
        yield return 0;
    }

    private void UpdateNewFloorColor(Vector4 color, Vector3 startPos, float paintingsTime = 0f, float paintingTimeMultiplier = 1f)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        if (!isFloorWithChildren)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.GetPropertyBlock(block);
            block.SetVector("_HSVAAdjust_NEW", new Vector4(color.x, color.y, color.z));
            block.SetFloat("_PaintingTime", paintingsTime);
            block.SetVector("_StartPosition", startPos + new Vector3(5, 0, 5));
            block.SetFloat("_PaintingTimeMultiplier", paintingTimeMultiplier);
            sr.SetPropertyBlock(block);
        }
        else
        {
            foreach (Transform t in gameObject.transform)
            {
                SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
                sr.GetPropertyBlock(block);
                block.SetVector("_HSVAAdjust_NEW", color);
                block.SetFloat("_PaintingTime", paintingsTime);
                block.SetVector("_StartPosition", startPos + new Vector3(5, 0, 5));
                block.SetFloat("_PaintingTimeMultiplier", paintingTimeMultiplier);
                sr.SetPropertyBlock(block);
            }
        }
    }

    private void UpdateCurrentFloorColor(Vector4 color)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        if (!isFloorWithChildren)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.GetPropertyBlock(block);
            block.SetVector("_HSVAAdjust", color);
            block.SetFloat("_PaintingTime", 0f);
            block.SetFloat("_PaintingTimeMultiplier", 1f);
            sr.SetPropertyBlock(block);
        }
        else
        {
            foreach (Transform t in gameObject.transform)
            {
                SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
                sr.GetPropertyBlock(block);
                block.SetVector("_HSVAAdjust", color);
                block.SetFloat("_PaintingTime", 0f);
                block.SetFloat("_PaintingTimeMultiplier", 1f);
                sr.SetPropertyBlock(block);
            }
        }
    }

    public void Reset()
    {
        if (updateFloorColor != null)
        {
            Timing.KillCoroutine(updateFloorColor);
            updateFloorColor = null;
        }
    }
}
