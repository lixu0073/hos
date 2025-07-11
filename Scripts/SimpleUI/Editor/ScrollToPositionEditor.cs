using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SimpleUI
{
    [CustomEditor(typeof(ScrollToPosition))]
    public class ScrollToPositionEditor : Editor
    {
        RectTransform targetTransform = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ScrollToPosition mTarget = (ScrollToPosition)target;

            targetTransform = (RectTransform)EditorGUILayout.ObjectField(targetTransform, typeof(RectTransform), true);

            if (GUILayout.Button("ScrollHorizontal"))
            {
                mTarget.ScrollHorizontal(targetTransform);
            }
            if (GUILayout.Button("ScrollHorizontalInstant"))
            {
                mTarget.ScrollHorizontalInstant(targetTransform);
            }

            if (GUILayout.Button("ScrollVertical"))
            {
                mTarget.ScrollVertical(targetTransform);
            }

            if (GUILayout.Button("ScrollVerticalInstant"))
            {
                mTarget.ScrollVerticalInstant(targetTransform);
            }
        }
    }
}
