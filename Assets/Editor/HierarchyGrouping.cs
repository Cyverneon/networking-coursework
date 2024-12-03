using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class Shortcuts
{
    // https://discussions.unity.com/t/grouping-objects-in-the-hierarchy/21358
    // script by idbrii based on script by bjennings76
    [MenuItem("GameObject/Group Selected %g")]
    static void GroupSelected()
    {
        if (!Selection.activeTransform)
            return;
        var go = new GameObject(Selection.activeTransform.name + " Group");
        if (Selection.activeTransform is RectTransform)
        {
            var rect = go.AddComponent<RectTransform>();
            Maximize(rect);
        }
        Undo.RegisterCreatedObjectUndo(go, "Group Selected");

        go.transform.SetParent(Selection.activeTransform.parent, false);

        foreach (var transform in GetSelectedTransformsInOrder())
        {
            Undo.SetTransformParent(transform, go.transform, "Group Selected");
        }
        Selection.activeGameObject = go;
    }
    [MenuItem("GameObject/Group Selected %g", true)]
    static bool Validate_GroupSelected()
    {
        // Grouping only makes sense in a scene.
        return Selection.activeTransform != null;
    }

    static void Maximize(RectTransform t)
    {
        t.anchorMin = Vector2.zero;
        t.anchorMax = Vector2.one;
        t.pivot = Vector2.one * 0.5f;
        t.sizeDelta = Vector2.zero;
    }

    static IEnumerable<Transform> GetSelectedTransformsInOrder()
    {
        // Selection order is arbitrary, so usually we sort by sibling
        // index to preserve ordering.
        return Selection.transforms
            .OrderBy(t => t.GetSiblingIndex());
    }

}
