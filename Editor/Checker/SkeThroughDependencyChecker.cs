#if !SKETHROUGH_HAS_NDMF
using UnityEditor;
using UnityEngine;

namespace Kokoa.SkeThrough.Checker
{
    [InitializeOnLoad]
    internal static class SkeThroughDependencyChecker
    {
        static SkeThroughDependencyChecker()
        {
            Debug.LogError(
                "[SkeThrough] Modular Avatar が見つかりません。\n" +
                "SkeThrough を使用するには Modular Avatar をインストールしてください。\n" +
                "https://modular-avatar.nadena.dev/");

            EditorApplication.delayCall += ShowDialog;
        }

        private static void ShowDialog()
        {
            var install = EditorUtility.DisplayDialog(
                "SkeThrough",
                "SkeThrough を使用するには Modular Avatar が必要です。\n\n" +
                "VCC (VRChat Creator Companion) から Modular Avatar をインストールしてください。",
                "Modular Avatar のページを開く",
                "閉じる");

            if (install)
            {
                Application.OpenURL("https://modular-avatar.nadena.dev/");
            }
        }
    }
}
#endif
