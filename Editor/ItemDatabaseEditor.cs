using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoachRace.Interaction.Editor
{
    [CustomEditor(typeof(ItemDatabase))]
    [CanEditMultipleObjects]
    public sealed class ItemDatabaseEditor : UnityEditor.Editor
    {
        private SerializedProperty _items;

        private void OnEnable()
        {
            _items = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            // Keep default editing experience.
            serializedObject.Update();

            DrawDuplicateIdWarnings();

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDuplicateIdWarnings()
        {
            // Only support single-target detailed reporting to avoid confusing mixed lists.
            if (targets == null || targets.Length != 1)
            {
                EditorGUILayout.HelpBox(
                    "Duplicate id validation is shown for a single selected ItemDatabase at a time.",
                    MessageType.Info);
                return;
            }

            var db = target as ItemDatabase;
            if (db == null) return;

            // Read from SerializedProperty so it matches inspector state.
            if (_items == null || !_items.isArray)
                return;

            var byId = new Dictionary<ushort, List<ItemDefinition>>();
            ushort maxId = 0;

            for (int i = 0; i < _items.arraySize; i++)
            {
                SerializedProperty element = _items.GetArrayElementAtIndex(i);
                var def = element.objectReferenceValue as ItemDefinition;
                if (def == null) continue;

                ushort id = def.id;
                if (id > maxId) maxId = id;

                if (!byId.TryGetValue(id, out List<ItemDefinition> list))
                {
                    list = new List<ItemDefinition>();
                    byId.Add(id, list);
                }

                // Avoid duplicates in case the same asset is listed multiple times.
                if (!list.Contains(def))
                    list.Add(def);
            }

            // Recommended id guidance.
            // Convention: id == 0 is reserved for empty slots / placeholders.
            if (maxId >= ushort.MaxValue)
            {
                EditorGUILayout.HelpBox(
                    "ItemDefinition ids are exhausted (max ushort reached). Please rework id allocation strategy.",
                    MessageType.Error);
            }
            else
            {
                ushort recommendedNextId = (ushort)Mathf.Max(1, maxId + 1);
                EditorGUILayout.HelpBox(
                    $"Recommended next ItemDefinition id: {recommendedNextId} (id 0 is reserved).",
                    MessageType.Info);
            }

            bool hasDuplicates = false;
            foreach (var kvp in byId)
            {
                if (kvp.Value.Count > 1)
                {
                    hasDuplicates = true;
                    break;
                }
            }

            if (!hasDuplicates) return;

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(
                "WARNING: Duplicate ItemDefinition ids detected in this ItemDatabase.\n" +
                "At runtime, the last occurrence in the list wins, which can cause unexpected icons/behavior.",
                MessageType.Warning);

            foreach (var kvp in byId)
            {
                ushort id = kvp.Key;
                List<ItemDefinition> defs = kvp.Value;
                if (defs.Count <= 1) continue;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"id = {id}", EditorStyles.boldLabel);

                    for (int i = 0; i < defs.Count; i++)
                    {
                        ItemDefinition def = defs[i];
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.ObjectField(def, typeof(ItemDefinition), allowSceneObjects: false);

                            if (GUILayout.Button("Ping", GUILayout.Width(44)))
                                EditorGUIUtility.PingObject(def);

                            if (GUILayout.Button("Select", GUILayout.Width(52)))
                                Selection.activeObject = def;
                        }
                    }
                }
            }

            EditorGUILayout.Space(6);
        }
    }
}
