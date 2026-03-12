using UnityEditor;
using UnityEngine;

namespace Kokoa.SkeThrough
{
    internal static class SkeThroughContextMenu
    {
        private const string OffPath = "GameObject/SkeThrough";
        private const string OnPath = "GameObject/SkeThrough \u2714";

        [MenuItem(OffPath, false, 20)]
        private static void EnableSkeThrough()
        {
            foreach (var obj in Selection.gameObjects)
            {
                if (obj.GetComponent<TransparentPreview>() == null)
                    Undo.AddComponent<TransparentPreview>(obj);
            }
        }

        [MenuItem(OffPath, true)]
        private static bool ValidateOff()
        {
            if (SkeThroughSettings.CurrentMode != DisplayMode.ContextMenu) return false;
            if (Selection.gameObjects.Length == 0) return false;

            foreach (var obj in Selection.gameObjects)
            {
                if (obj.GetComponent<TransparentPreview>() != null) return false;
                if (obj.GetComponentInChildren<Renderer>(true) != null) return true;
            }

            return false;
        }

        [MenuItem(OnPath, false, 20)]
        private static void DisableSkeThrough()
        {
            foreach (var obj in Selection.gameObjects)
            {
                var preview = obj.GetComponent<TransparentPreview>();
                if (preview != null)
                    Undo.DestroyObjectImmediate(preview);
            }
        }

        [MenuItem(OnPath, true)]
        private static bool ValidateOn()
        {
            if (SkeThroughSettings.CurrentMode != DisplayMode.ContextMenu) return false;
            if (Selection.gameObjects.Length == 0) return false;

            foreach (var obj in Selection.gameObjects)
            {
                if (obj.GetComponent<TransparentPreview>() != null) return true;
            }

            return false;
        }
    }
}
