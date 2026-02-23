using UnityEditor;
using UnityEngine;

namespace Kokoa.SkeThrough
{
    internal static class SkeThroughContextMenu
    {
        private const string MenuPath = "GameObject/SkeThrough";

        [MenuItem(MenuPath, false, 20)]
        private static void ToggleSkeThrough()
        {
            foreach (var obj in Selection.gameObjects)
            {
                var preview = obj.GetComponent<TransparentPreview>();
                if (preview != null)
                    Undo.DestroyObjectImmediate(preview);
                else
                    Undo.AddComponent<TransparentPreview>(obj);
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggle()
        {
            if (SkeThroughSettings.CurrentMode != DisplayMode.ContextMenu) return false;
            if (Selection.gameObjects.Length == 0) return false;

            bool show = false;
            bool isOn = false;

            foreach (var obj in Selection.gameObjects)
            {
                if (obj.GetComponent<TransparentPreview>() != null)
                {
                    show = true;
                    isOn = true;
                    break;
                }
                if (obj.GetComponentInChildren<Renderer>(true) != null)
                    show = true;
            }

            Menu.SetChecked(MenuPath, isOn);
            return show;
        }
    }
}
