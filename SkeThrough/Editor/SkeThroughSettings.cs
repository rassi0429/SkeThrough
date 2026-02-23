using UnityEditor;
using UnityEngine;

namespace Kokoa.SkeThrough
{
    internal enum DisplayMode
    {
        AlwaysShow,
        ContextMenu
    }

    [FilePath("ProjectSettings/SkeThroughSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class SkeThroughSettings : ScriptableSingleton<SkeThroughSettings>
    {
        [SerializeField] private DisplayMode displayMode = DisplayMode.AlwaysShow;

        internal static DisplayMode CurrentMode
        {
            get => instance.displayMode;
            set
            {
                instance.displayMode = value;
                instance.Save(true);
            }
        }
    }

    internal class SkeThroughSettingsWindow : EditorWindow
    {
        [MenuItem("Tools/SkeThrough/Settings")]
        private static void Open()
        {
            var window = GetWindow<SkeThroughSettingsWindow>("SkeThrough Settings");
            window.minSize = new Vector2(280, 100);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("SkeThrough Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            var mode = SkeThroughSettings.CurrentMode;
            var newMode = (DisplayMode)EditorGUILayout.EnumPopup("Display Mode", mode);

            if (newMode != mode)
            {
                SkeThroughSettings.CurrentMode = newMode;
                EditorApplication.RepaintHierarchyWindow();
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                newMode == DisplayMode.AlwaysShow
                    ? "Hierarchy にトグルボタンを常時表示します。"
                    : "Hierarchy の右クリックメニューから切り替えます。",
                MessageType.Info);
        }
    }
}
