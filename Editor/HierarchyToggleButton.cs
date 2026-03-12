using UnityEditor;
using UnityEngine;

namespace Kokoa.SkeThrough
{
    [InitializeOnLoad]
    internal static class HierarchyToggleButton
    {
        private static GUIContent _activeIcon;
        private static GUIContent _inactiveIcon;

        static HierarchyToggleButton()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            _activeIcon = EditorGUIUtility.IconContent("Occlusion");
            _inactiveIcon = EditorGUIUtility.IconContent("Occlusion");
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (SkeThroughSettings.CurrentMode != DisplayMode.AlwaysShow) return;

            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;

            var preview = obj.GetComponent<TransparentPreview>();

            // 子孫に Renderer を持つか、既に TransparentPreview が付いているオブジェクトのみ表示
            if (preview == null && obj.GetComponentInChildren<Renderer>(true) == null) return;

            var offset = SkeThroughSettings.ButtonOffsetFromRight;
            var buttonRect = new Rect(selectionRect)
            {
                x = selectionRect.xMax - offset,
                width = 20
            };

            var originalColor = GUI.color;
            bool hasPreview = preview != null;

            var inactiveAlpha = SkeThroughSettings.InactiveButtonAlpha;
            var icon = hasPreview ? _activeIcon : _inactiveIcon;
            GUI.color = hasPreview
                ? new Color(0.5f, 0.8f, 1f, 1f)
                : new Color(0.5f, 0.5f, 0.5f, inactiveAlpha);

            var newState = GUI.Toggle(buttonRect, hasPreview, icon, GUI.skin.button);
            GUI.color = originalColor;

            if (newState != hasPreview)
            {
                if (newState)
                {
                    Undo.AddComponent<TransparentPreview>(obj);
                }
                else
                {
                    Undo.DestroyObjectImmediate(preview);
                }
            }
        }
    }
}
