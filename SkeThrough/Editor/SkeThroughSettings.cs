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

        [MenuItem("Tools/SkeThrough")]
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

            var cardStyle = new GUIStyle("HelpBox")
            {
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(0, 0, 0, 0)
            };

            EditorGUILayout.BeginVertical(cardStyle);

            var newValue = EditorGUILayout.ToggleLeft(label, isSelected, EditorStyles.boldLabel);
            if (newValue != isSelected && newValue)
            {
                SkeThroughSettings.CurrentMode = cardMode;
                EditorApplication.RepaintHierarchyWindow();
            }

            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);

            if (image != null)
            {
                GUILayout.Space(4);
                var rect = GUILayoutUtility.GetAspectRect((float)image.width / image.height);
                GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.EndVertical();

            // カード全体をクリック可能にする
            var totalRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && totalRect.Contains(Event.current.mousePosition))
            {
                if (!isSelected)
                {
                    SkeThroughSettings.CurrentMode = cardMode;
                    EditorApplication.RepaintHierarchyWindow();
                }
                Event.current.Use();
            }

            // 選択中の枠線
            if (isSelected)
            {
                var borderColor = new Color(0.4f, 0.7f, 1f, 0.8f);
                DrawBorder(totalRect, borderColor, 2f);
            }
        }

        private static void DrawBorder(Rect rect, Color color, float thickness)
        {
            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), EditorGUIUtility.whiteTexture);
            GUI.color = prev;
        }
    }
}
