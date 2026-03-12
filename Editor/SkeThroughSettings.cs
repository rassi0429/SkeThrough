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
        [SerializeField] private int buttonOffsetFromRight = 20;
        [SerializeField] private float inactiveButtonAlpha = 0.3f;

        internal static DisplayMode CurrentMode
        {
            get => instance.displayMode;
            set
            {
                instance.displayMode = value;
                instance.Save(true);
            }
        }

        internal static int ButtonOffsetFromRight
        {
            get => instance.buttonOffsetFromRight;
            set
            {
                instance.buttonOffsetFromRight = value;
                instance.Save(true);
            }
        }

        internal static float InactiveButtonAlpha
        {
            get => instance.inactiveButtonAlpha;
            set
            {
                instance.inactiveButtonAlpha = value;
                instance.Save(true);
            }
        }
    }

    internal class SkeThroughSettingsWindow : EditorWindow
    {
        private const string Version = "1.0.0";

        private Texture2D _buttonImage;
        private Texture2D _contextImage;
        private Vector2 _scrollPosition;

        private static GUIStyle _titleStyle;
        private static GUIStyle _subtitleStyle;
        private static GUIStyle _sectionLabelStyle;
        private static GUIStyle _cardLabelStyle;
        private static GUIStyle _cardDescStyle;
        private static Texture2D _selectedBg;
        private static Texture2D _normalBg;
        private static Texture2D _hoverBg;

        [MenuItem("Tools/SkeThrough")]
        private static void Open()
        {
            var window = GetWindow<SkeThroughSettingsWindow>("SkeThrough");
            window.minSize = new Vector2(320, 420);
            window.Show();
        }

        private void OnEnable()
        {
            _buttonImage = Resources.Load<Texture2D>("description_button");
            _contextImage = Resources.Load<Texture2D>("description_context");
            _selectedBg = null;
            _normalBg = null;
        }

        private static void InitStyles()
        {
            _titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 0, 0)
            };

            _subtitleStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
            };

            _sectionLabelStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) },
                margin = new RectOffset(2, 0, 0, 4)
            };

            _cardLabelStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                margin = new RectOffset(0, 0, 0, 0)
            };

            _cardDescStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                wordWrap = true,
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) },
                margin = new RectOffset(0, 0, 0, 0)
            };

            if (_selectedBg == null)
            {
                _selectedBg = new Texture2D(1, 1);
                _selectedBg.SetPixel(0, 0, new Color(0.22f, 0.44f, 0.73f, 0.25f));
                _selectedBg.Apply();
                _selectedBg.hideFlags = HideFlags.HideAndDontSave;
            }

            if (_normalBg == null)
            {
                _normalBg = new Texture2D(1, 1);
                _normalBg.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.15f));
                _normalBg.Apply();
                _normalBg.hideFlags = HideFlags.HideAndDontSave;
            }

            if (_hoverBg == null)
            {
                _hoverBg = new Texture2D(1, 1);
                _hoverBg.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.28f));
                _hoverBg.Apply();
                _hoverBg.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void OnGUI()
        {
            InitStyles();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            float maxWidth = 300f;
            float pad = Mathf.Max((position.width - maxWidth) / 2f, 10f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(pad);
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth));

            // Header
            GUILayout.Space(16);
            GUILayout.Label("SkeThrough", _titleStyle);
            GUILayout.Space(2);
            GUILayout.Label($"v{Version}", _subtitleStyle);
            GUILayout.Space(20);

            // Section
            var mode = SkeThroughSettings.CurrentMode;
            GUILayout.Label("DISPLAY MODE", _sectionLabelStyle);
            GUILayout.Space(4);

            DrawCard("Button", "Hierarchy\u306b\u30c8\u30b0\u30eb\u30dc\u30bf\u30f3\u3092\u5e38\u6642\u8868\u793a",
                DisplayMode.AlwaysShow, _buttonImage, mode, true);

            GUILayout.Space(2);

            DrawCard("Context Menu", "Hierarchy\u306e\u53f3\u30af\u30ea\u30c3\u30af\u30e1\u30cb\u30e5\u30fc\u304b\u3089\u5207\u66ff",
                DisplayMode.ContextMenu, _contextImage, mode, false);

            // Button Settings (AlwaysShow モード時のみ)
            if (mode == DisplayMode.AlwaysShow)
            {
                GUILayout.Space(16);
                GUILayout.Label("BUTTON SETTINGS", _sectionLabelStyle);
                GUILayout.Space(4);

                EditorGUI.BeginChangeCheck();

                var offset = EditorGUILayout.IntSlider(
                    "右端からの距離", SkeThroughSettings.ButtonOffsetFromRight, 0, 200);

                var alpha = EditorGUILayout.Slider(
                    "非アクティブの透明度", SkeThroughSettings.InactiveButtonAlpha, 0.05f, 1f);

                if (EditorGUI.EndChangeCheck())
                {
                    SkeThroughSettings.ButtonOffsetFromRight = offset;
                    SkeThroughSettings.InactiveButtonAlpha = alpha;
                    EditorApplication.RepaintHierarchyWindow();
                }
            }

            GUILayout.Space(20);

            EditorGUILayout.EndVertical();
            GUILayout.Space(pad);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void DrawCard(string label, string description, DisplayMode cardMode,
            Texture2D image, DisplayMode current, bool isFirst)
        {
            bool isSelected = current == cardMode;

            // Hover detection (use a stable rect from previous frame)
            var preRect = GUILayoutUtility.GetRect(0, 0);
            bool isHover = false;

            // Card background - selected / hover / normal
            Texture2D bg;
            if (isSelected)
                bg = _selectedBg;
            else
            {
                // Check hover on repaint
                if (Event.current.type == EventType.Repaint)
                {
                    var checkRect = new Rect(preRect.x, preRect.y, preRect.width, 200);
                    isHover = checkRect.Contains(Event.current.mousePosition);
                }
                bg = isHover ? _hoverBg : _normalBg;
            }

            var cardStyle = new GUIStyle
            {
                normal = { background = bg },
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 0, 0)
            };

            EditorGUILayout.BeginVertical(cardStyle);

            // Title row with radio indicator
            EditorGUILayout.BeginHorizontal();

            var radioStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                fixedWidth = 20,
                normal = { textColor = isSelected
                    ? new Color(0.35f, 0.6f, 0.95f)
                    : new Color(0.5f, 0.5f, 0.5f, 0.6f) }
            };
            GUILayout.Label(isSelected ? "\u25c9" : "\u25cb", radioStyle, GUILayout.Width(18));

            EditorGUILayout.BeginVertical();
            GUILayout.Label(label, _cardLabelStyle);
            GUILayout.Label(description, _cardDescStyle);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // Image
            if (image != null)
            {
                GUILayout.Space(8);
                var rect = GUILayoutUtility.GetAspectRect((float)image.width / image.height);
                var frameStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(1, 1, 1, 1),
                    margin = new RectOffset(0, 0, 0, 0)
                };
                GUI.Box(rect, GUIContent.none, frameStyle);
                var imageRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
                GUI.DrawTexture(imageRect, image, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.EndVertical();

            // Full card click + hover cursor
            var cardRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(cardRect, MouseCursor.Link);

            // Repaint on hover for background change
            if (Event.current.type == EventType.MouseMove && cardRect.Contains(Event.current.mousePosition))
            {
                Repaint();
            }

            if (Event.current.type == EventType.MouseDown && cardRect.Contains(Event.current.mousePosition))
            {
                if (!isSelected)
                {
                    SkeThroughSettings.CurrentMode = cardMode;
                    EditorApplication.RepaintHierarchyWindow();
                    Repaint();
                }
                Event.current.Use();
            }

            // Border
            if (Event.current.type == EventType.Repaint)
            {
                var borderColor = isSelected
                    ? new Color(0.35f, 0.6f, 0.95f, 0.9f)
                    : new Color(0.5f, 0.5f, 0.5f, 0.25f);
                DrawBorder(cardRect, borderColor, isSelected ? 2f : 1f);
            }
        }

        private static void DrawBorder(Rect rect, Color color, float t)
        {
            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, t), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - t, rect.width, t), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, t, rect.height), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - t, rect.y, t, rect.height), EditorGUIUtility.whiteTexture);
            GUI.color = prev;
        }
    }
}
