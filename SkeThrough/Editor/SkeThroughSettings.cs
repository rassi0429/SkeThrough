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
        private const string Version = "0.1.0";
        private const string ImageDir = "Assets/kokoa/SkeThrough/Assets";

        private Texture2D _buttonImage;
        private Texture2D _contextImage;
        private Vector2 _scrollPosition;

        private static GUIStyle _headerStyle;
        private static GUIStyle _versionStyle;
        private static GUIStyle _boxStyle;

        [MenuItem("Tools/SkeThrough/Settings")]
        private static void Open()
        {
            var window = GetWindow<SkeThroughSettingsWindow>("SkeThrough Settings");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }

        private void OnEnable()
        {
            _buttonImage = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ImageDir}/description_button.png");
            _contextImage = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ImageDir}/description_context.png");
        }

        private static void InitStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    margin = new RectOffset(0, 0, 10, 5)
                };
            }

            if (_versionStyle == null)
            {
                _versionStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.gray }
                };
            }

            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle("HelpBox")
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(5, 5, 5, 5)
                };
            }
        }

        private void OnGUI()
        {
            InitStyles();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            float maxWidth = 300f;
            float padding = Mathf.Max((position.width - maxWidth) / 2f, 0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(padding);
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth));

            EditorGUILayout.Space(5);
            GUILayout.Label("SkeThrough", _headerStyle);
            GUILayout.Label($"v{Version} by kokoa", _versionStyle);
            EditorGUILayout.Space(10);

            var mode = SkeThroughSettings.CurrentMode;

            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("Display Mode", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawModeOption("Button", "Hierarchy にトグルボタンを常時表示します",
                DisplayMode.AlwaysShow, _buttonImage, mode);

            GUILayout.Space(8);

            DrawModeOption("Context Menu", "Hierarchy の右クリックメニューから切り替えます",
                DisplayMode.ContextMenu, _contextImage, mode);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.EndVertical();
            GUILayout.Space(padding);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void DrawModeOption(string label, string description, DisplayMode cardMode,
            Texture2D image, DisplayMode current)
        {
            bool isSelected = current == cardMode;

            EditorGUILayout.BeginHorizontal();

            var originalBg = GUI.backgroundColor;
            if (isSelected)
                GUI.backgroundColor = new Color(0.4f, 0.7f, 1f, 1f);

            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fixedHeight = 24
            };

            if (GUILayout.Button(isSelected ? $"\u2714 {label}" : label, buttonStyle))
            {
                if (!isSelected)
                {
                    SkeThroughSettings.CurrentMode = cardMode;
                    EditorApplication.RepaintHierarchyWindow();
                }
            }

            GUI.backgroundColor = originalBg;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(description, MessageType.Info);

            if (image != null)
            {
                GUILayout.Space(2);
                var rect = GUILayoutUtility.GetAspectRect((float)image.width / image.height);
                GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit);
            }
        }
    }
}
