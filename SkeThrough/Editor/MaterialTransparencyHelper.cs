using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kokoa.SkeThrough
{
    internal static class MaterialTransparencyHelper
    {
        // 元テクスチャの InstanceID → alpha=1 コピーのキャッシュ
        private static readonly Dictionary<int, Texture2D> _opaqueAlphaCache = new();

        public static Material CreateTransparentCopy(Material source, float alpha)
        {
            var mat = new Material(source);
            mat.name = source.name + "_SkeThrough";

            // メインテクスチャの alpha をすべて 1.0 に強制（キャッシュから取得）
            ReplaceMainTexWithOpaqueAlpha(mat, "_MainTex");

            string shaderName = mat.shader.name;

            if (shaderName.Contains("lilToon"))
            {
                ApplyLilToon(mat, alpha);
            }
            else if (shaderName.Contains("Poiyomi") || shaderName.Contains(".poyi/"))
            {
                ApplyPoiyomi(mat, alpha);
            }
            else if (shaderName == "Standard")
            {
                ApplyStandard(mat, alpha);
            }
            else
            {
                ApplyFallback(mat, alpha);
            }

            return mat;
        }

        private static void ApplyLilToon(Material mat, float alpha)
        {
            string shaderName = mat.shader.name;
            bool isMulti = shaderName.Contains("Multi");

            if (isMulti)
            {
                // Multi シェーダー: プロパティで切り替え
                if (mat.HasProperty("_TransparentMode"))
                    mat.SetFloat("_TransparentMode", 2f);
            }
            else
            {
                // 非Multi: 透明版シェーダーに差し替え
                bool hasOutline = shaderName.Contains("Outline");
                bool isLite = shaderName.Contains("Lite");

                string transparentShaderName;
                if (isLite)
                    transparentShaderName = hasOutline
                        ? "Hidden/lilToonLiteTransparentOutline"
                        : "Hidden/lilToonLiteTransparent";
                else
                    transparentShaderName = hasOutline
                        ? "Hidden/lilToonTransparentOutline"
                        : "Hidden/lilToonTransparent";

                var transparentShader = Shader.Find(transparentShaderName);
                if (transparentShader != null)
                    mat.shader = transparentShader;
            }

            mat.SetOverrideTag("RenderType", "TransparentCutout");
            mat.renderQueue = 2460;

            // Premultiplied Alpha ブレンド (lilToon公式と同じ)
            if (mat.HasProperty("_SrcBlend"))
                mat.SetInt("_SrcBlend", (int)BlendMode.One);
            if (mat.HasProperty("_DstBlend"))
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_AlphaToMask"))
                mat.SetInt("_AlphaToMask", 0);

            // lilToon は透明モードでも ZWrite=1
            if (mat.HasProperty("_ZWrite"))
                mat.SetInt("_ZWrite", 1);
            if (mat.HasProperty("_ZTest"))
                mat.SetInt("_ZTest", 4); // LessEqual

            // アルファチャンネルブレンド
            if (mat.HasProperty("_SrcBlendAlpha"))
                mat.SetInt("_SrcBlendAlpha", (int)BlendMode.One);
            if (mat.HasProperty("_DstBlendAlpha"))
                mat.SetInt("_DstBlendAlpha", (int)BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_BlendOp"))
                mat.SetInt("_BlendOp", (int)BlendOp.Add);
            if (mat.HasProperty("_BlendOpAlpha"))
                mat.SetInt("_BlendOpAlpha", (int)BlendOp.Add);

            // Forward Add パス
            if (mat.HasProperty("_SrcBlendFA"))
                mat.SetInt("_SrcBlendFA", (int)BlendMode.One);
            if (mat.HasProperty("_DstBlendFA"))
                mat.SetInt("_DstBlendFA", (int)BlendMode.One);
            if (mat.HasProperty("_SrcBlendAlphaFA"))
                mat.SetInt("_SrcBlendAlphaFA", (int)BlendMode.Zero);
            if (mat.HasProperty("_DstBlendAlphaFA"))
                mat.SetInt("_DstBlendAlphaFA", (int)BlendMode.One);
            if (mat.HasProperty("_BlendOpFA"))
                mat.SetInt("_BlendOpFA", (int)BlendOp.Max);
            if (mat.HasProperty("_BlendOpAlphaFA"))
                mat.SetInt("_BlendOpAlphaFA", (int)BlendOp.Max);

            // アウトライン
            if (mat.HasProperty("_OutlineSrcBlend"))
            {
                mat.SetInt("_OutlineSrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_OutlineDstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_OutlineAlphaToMask", 0);
            }

            SetColorAlpha(mat, "_Color", alpha);
        }

        private static void ApplyPoiyomi(Material mat, float alpha)
        {
            mat.SetOverrideTag("RenderType", "Transparent");

            if (mat.HasProperty("_SrcBlend"))
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend"))
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite"))
                mat.SetInt("_ZWrite", 0);

            mat.renderQueue = 3000;

            SetColorAlpha(mat, "_Color", alpha);
        }

        private static void ApplyStandard(Material mat, float alpha)
        {
            // Standard shader: Transparent モード (mode=3)
            mat.SetFloat("_Mode", 3f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            SetColorAlpha(mat, "_Color", alpha);
        }

        private static void ApplyFallback(Material mat, float alpha)
        {
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;

            if (mat.HasProperty("_SrcBlend"))
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend"))
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite"))
                mat.SetInt("_ZWrite", 0);

            SetColorAlpha(mat, "_Color", alpha);
        }

        private static void SetColorAlpha(Material mat, string property, float alpha)
        {
            if (mat.HasProperty(property))
            {
                var color = mat.GetColor(property);
                color.a = alpha;
                mat.SetColor(property, color);
            }
        }

        private static void ReplaceMainTexWithOpaqueAlpha(Material mat, string property)
        {
            if (!mat.HasProperty(property)) return;
            var tex = mat.GetTexture(property);
            if (tex == null) return;

            int id = tex.GetInstanceID();
            if (!_opaqueAlphaCache.TryGetValue(id, out var cached) || cached == null)
            {
                cached = CreateOpaqueAlphaCopy(tex);
                _opaqueAlphaCache[id] = cached;
            }
            mat.SetTexture(property, cached);
        }

        private static Texture2D CreateOpaqueAlphaCopy(Texture tex)
        {
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(tex, rt);

            var prev = RenderTexture.active;
            RenderTexture.active = rt;

            var copy = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            var pixels = copy.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].a = 255;
            }
            copy.SetPixels32(pixels);
            copy.Apply();

            copy.name = tex.name + "_OpaqueAlpha";
            return copy;
        }
    }
}
