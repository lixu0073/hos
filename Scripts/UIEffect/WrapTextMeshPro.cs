using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using I2.Loc;

public class WrapTextMeshPro : MonoBehaviour
{

    private TMP_Text m_TextComponent;
    private TextMeshProUGUI text;

    public AnimationCurve VertexCurveUP = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
    public AnimationCurve VertexCurveDown = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0f));
    public float CurveScale = 1.0f;
    public Material textMaterial;

    void Awake()
    {
        m_TextComponent = gameObject.GetComponent<TMP_Text>();
        LocalizationManager.OnLocalizeEvent += OnLocalize;
    }

    private void OnLocalize()
    {
        if (m_TextComponent != null)
        {
            m_TextComponent.fontMaterial = textMaterial;
            WrapTextToCurve();
        }
    }

    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        WrapTextToCurve();
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    private void OnDestroy()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    private void OnTextChanged(Object obj)
    {
        if (obj == m_TextComponent)
            WrapTextToCurve();
    }

    public void WrapTextToCurve()
    {
        VertexCurveUP.preWrapMode = WrapMode.Clamp;
        VertexCurveUP.postWrapMode = WrapMode.Clamp;

        VertexCurveDown.preWrapMode = WrapMode.Clamp;
        VertexCurveDown.postWrapMode = WrapMode.Clamp;

        Vector3[] vertices;

        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        int characterCount = textInfo.characterInfo.Length;


        if (characterCount == 0) return;


        float boundsMinX = m_TextComponent.bounds.min.x;  //textInfo.meshInfo[0].mesh.bounds.min.x;
        float boundsMaxX = m_TextComponent.bounds.max.x;  //textInfo.meshInfo[0].mesh.bounds.max.x;

        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            vertices = textInfo.meshInfo[materialIndex].vertices;

            Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);

            float x0 = (vertices[vertexIndex + 0].x - boundsMinX) / (boundsMaxX - boundsMinX);
            float x1 = (vertices[vertexIndex + 2].x - boundsMinX) / (boundsMaxX - boundsMinX);

            vertices[vertexIndex + 0].y += VertexCurveDown.Evaluate(x0) * CurveScale;
            vertices[vertexIndex + 1].y += VertexCurveUP.Evaluate(x0) * CurveScale;
            vertices[vertexIndex + 2].y += VertexCurveUP.Evaluate(x1) * CurveScale;
            vertices[vertexIndex + 3].y += VertexCurveDown.Evaluate(x1) * CurveScale;
        }
        // Upload the mesh with the revised information
        m_TextComponent.UpdateVertexData();
    }
}
