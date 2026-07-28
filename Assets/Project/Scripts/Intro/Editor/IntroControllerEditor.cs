using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IntroController))]
public class IntroControllerEditor : Editor
{
    // 런타임 스크립트의 cuts 배열을 편집하기 위한 SerializedProperty입니다.
    private SerializedProperty cutsProperty;
    // Scene 뷰 핸들로 편집할 컷과 손전등 경로점 번호입니다.
    private int selectedCut;
    private int selectedStep;

    // Inspector가 활성화될 때 직렬화된 컷 배열을 찾습니다.
    private void OnEnable()
    {
        cutsProperty = serializedObject.FindProperty("cuts");
    }

    // 기본 Inspector 아래에 예상 재생시간과 Scene 핸들 선택 메뉴를 추가합니다.
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        IntroController controller = (IntroController)target;
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            $"예상 총 재생시간: {controller.EstimatedTotalDuration:0.00}초\n"
            + "손전등 경로의 각 원소에서 이동시간과 머무르는 시간을 설정하세요.\n"
            + "Scene 뷰의 청록색 핸들은 선택한 경로점, 노란색 핸들은 줌 중심입니다.",
            MessageType.Info);

        if (cutsProperty != null && cutsProperty.arraySize > 0)
        {
            selectedCut = EditorGUILayout.IntSlider(
                "Scene 핸들로 조정할 컷",
                Mathf.Clamp(selectedCut, 0, cutsProperty.arraySize - 1),
                0,
                cutsProperty.arraySize - 1);

            SerializedProperty selectedCutProperty = cutsProperty.GetArrayElementAtIndex(selectedCut);
            SerializedProperty pathProperty = selectedCutProperty.FindPropertyRelative("spotlightPath");
            if (pathProperty != null && pathProperty.arraySize > 0)
            {
                selectedStep = EditorGUILayout.IntSlider(
                    "Scene 핸들로 조정할 경로점",
                    Mathf.Clamp(selectedStep, 0, pathProperty.arraySize - 1),
                    0,
                    pathProperty.arraySize - 1);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // 선택한 손전등 위치/크기와 줌 중심을 Scene 뷰 핸들로 표시합니다.
    private void OnSceneGUI()
    {
        serializedObject.Update();
        if (cutsProperty == null || cutsProperty.arraySize == 0)
            return;

        selectedCut = Mathf.Clamp(selectedCut, 0, cutsProperty.arraySize - 1);
        SerializedProperty cut = cutsProperty.GetArrayElementAtIndex(selectedCut);
        SerializedProperty path = cut.FindPropertyRelative("spotlightPath");
        SerializedProperty spotlightPosition;
        SerializedProperty spotlightSize;
        SerializedProperty zoomFocusPosition;
        bool zoomFollowsSpotlight = false;

        // 경로점이 있으면 선택한 경로점을 편집하고, 없으면 기존 단일 설정을 편집합니다.
        if (path != null && path.arraySize > 0)
        {
            selectedStep = Mathf.Clamp(selectedStep, 0, path.arraySize - 1);
            SerializedProperty step = path.GetArrayElementAtIndex(selectedStep);
            spotlightPosition = step.FindPropertyRelative("position");
            spotlightSize = step.FindPropertyRelative("size");
            zoomFollowsSpotlight = step.FindPropertyRelative("zoomFollowsSpotlight").boolValue;
            zoomFocusPosition = zoomFollowsSpotlight
                ? spotlightPosition
                : step.FindPropertyRelative("zoomFocusPosition");
        }
        else
        {
            spotlightPosition = cut.FindPropertyRelative("spotlightPosition");
            spotlightSize = cut.FindPropertyRelative("spotlightSize");
            zoomFocusPosition = cut.FindPropertyRelative("zoomFocusPosition");
        }

        IntroController controller = (IntroController)target;
        RectTransform canvasRect = controller.PreviewRect;
        if (canvasRect == null)
            return;

        string spotlightLabel = path != null && path.arraySize > 0
            ? $"손전등 경로점 {selectedStep + 1}"
            : "손전등";
        DrawPositionHandle(canvasRect, spotlightPosition, Color.cyan, spotlightLabel);
        if (!zoomFollowsSpotlight)
            DrawPositionHandle(canvasRect, zoomFocusPosition, Color.yellow, "줌 중심");
        DrawSizeHandles(canvasRect, spotlightPosition, spotlightSize);

        serializedObject.ApplyModifiedProperties();
    }

    // 정규화 위치를 Scene 뷰의 드래그 가능한 원형 핸들로 표시합니다.
    private static void DrawPositionHandle(
        RectTransform canvasRect,
        SerializedProperty property,
        Color color,
        string label)
    {
        Vector3 worldPosition = NormalizedToWorld(canvasRect, property.vector2Value);
        float handleSize = HandleUtility.GetHandleSize(worldPosition) * 0.08f;

        Handles.color = color;
        EditorGUI.BeginChangeCheck();
        Vector3 movedPosition = Handles.FreeMoveHandle(
            worldPosition,
            handleSize,
            Vector3.zero,
            Handles.CircleHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(property.serializedObject.targetObject, $"{label} 위치 변경");
            property.vector2Value = WorldToNormalized(canvasRect, movedPosition);
        }

        Handles.Label(worldPosition + Vector3.up * handleSize, label);
    }

    // 손전등 중심에서 가로/세로 크기를 조절하는 핸들을 표시합니다.
    private static void DrawSizeHandles(
        RectTransform canvasRect,
        SerializedProperty positionProperty,
        SerializedProperty sizeProperty)
    {
        Vector2 position = positionProperty.vector2Value;
        Vector2 size = sizeProperty.vector2Value;
        Vector3 center = NormalizedToWorld(canvasRect, position);
        Vector3 right = NormalizedToWorld(canvasRect, position + new Vector2(size.x * 0.5f, 0f));
        Vector3 up = NormalizedToWorld(canvasRect, position + new Vector2(0f, size.y * 0.5f));
        float handleSize = HandleUtility.GetHandleSize(center) * 0.06f;

        Handles.color = new Color(0f, 1f, 1f, 0.75f);
        Handles.DrawDottedLine(center, right, 4f);
        Handles.DrawDottedLine(center, up, 4f);

        EditorGUI.BeginChangeCheck();
        Vector3 movedRight = Handles.FreeMoveHandle(right, handleSize, Vector3.zero, Handles.DotHandleCap);
        Vector3 movedUp = Handles.FreeMoveHandle(up, handleSize, Vector3.zero, Handles.DotHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(sizeProperty.serializedObject.targetObject, "손전등 크기 변경");
            Vector2 normalizedRight = WorldToNormalized(canvasRect, movedRight);
            Vector2 normalizedUp = WorldToNormalized(canvasRect, movedUp);
            sizeProperty.vector2Value = new Vector2(
                Mathf.Max(0.01f, Mathf.Abs(normalizedRight.x - position.x) * 2f),
                Mathf.Max(0.01f, Mathf.Abs(normalizedUp.y - position.y) * 2f));
        }
    }

    // 0~1 정규화 좌표를 Canvas의 월드 좌표로 변환합니다.
    private static Vector3 NormalizedToWorld(RectTransform rect, Vector2 normalized)
    {
        Rect localRect = rect.rect;
        Vector3 local = new(
            Mathf.Lerp(localRect.xMin, localRect.xMax, normalized.x),
            Mathf.Lerp(localRect.yMin, localRect.yMax, normalized.y),
            0f);
        return rect.TransformPoint(local);
    }

    // Scene 핸들의 월드 좌표를 해상도 독립적인 0~1 좌표로 되돌립니다.
    private static Vector2 WorldToNormalized(RectTransform rect, Vector3 world)
    {
        Vector3 local = rect.InverseTransformPoint(world);
        Rect localRect = rect.rect;
        return new Vector2(
            Mathf.Clamp01(Mathf.InverseLerp(localRect.xMin, localRect.xMax, local.x)),
            Mathf.Clamp01(Mathf.InverseLerp(localRect.yMin, localRect.yMax, local.y)));
    }
}
