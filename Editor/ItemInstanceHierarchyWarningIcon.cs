#if UNITY_EDITOR
using RoachRace.Interaction;
using UnityEditor;
using UnityEngine;

namespace RoachRace.Interaction.Editor
{
    [InitializeOnLoad]
    internal static class ItemInstanceHierarchyWarningIcon
    {
        private static readonly GUIContent WarnIcon = EditorGUIUtility.IconContent("console.warnicon.sml");
        private static readonly string Tooltip = "ItemInstance is missing ItemDefinition";

        static ItemInstanceHierarchyWarningIcon()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
        {
            if (Application.isPlaying) return;

            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is not GameObject go) return;

            if (!go.TryGetComponent<ItemInstance>(out var itemInstance)) return;
            if (itemInstance == null) return;
            if (itemInstance.Definition != null) return;

            // Draw a small warning icon on the right side of the hierarchy row.
            var rect = new Rect(selectionRect);
            rect.x = rect.xMax - 18f;
            rect.width = 18f;

            var content = new GUIContent(WarnIcon)
            {
                tooltip = Tooltip
            };

            GUI.Label(rect, content);
        }
    }
}
#endif
