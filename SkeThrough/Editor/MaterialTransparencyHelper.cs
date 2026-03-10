using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kokoa.SkeThrough
{
    internal static class MaterialTransparencyHelper
    {
        // 元テクスチャの InstanceID → alpha=1 コピーのキャッシュ
        private static readonly Dictionary<int, Texture2D> _opaqueAlphaCache = new();

        public static Material CreateTransparentCopy(Material source, float alpha, int renderQueueOverride = -1)
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

            // コンポーネントで指定された renderQueue で上書き
            if (renderQueueOverride >= 0)
                mat.renderQueue = renderQueueOverride;

            return mat;
        }

        private static void ApplyLilToon(Material mat, float alpha)
        {
            string shaderName = mat.shader.name;
            bool isMulti = shaderName.Contains("Multi");

            if (isMulti)
            {
                // Multi シェーダー: プロパティ + キーワードで切り替え
                if (mat.HasProperty("_TransparentMode"))
                    mat.SetFloat("_TransparentMode", 2f);

                // Multi はキーワードで LIL_RENDER を決定する（これがないと描画が切り替わらない）
                mat.EnableKeyword("UNITY_UI_CLIP_RECT");    // LIL_RENDER 2 (Transparent)
                mat.DisableKeyword("UNITY_UI_ALPHACLIP");   // LIL_RENDER 1 (Cutout) を無効化
                mat.DisableKeyword("ETC1_EXTERNAL_ALPHA");  // Dither Cutout を無効化
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
            mat.renderQueue = 3000;

            // Cutoff を 0 にして discard を無効化（0.5 以下で消える問題の対策）
            if (mat.HasProperty("_Cutoff"))
                mat.SetFloat("_Cutoff", 0f);

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

            // アルファマスクを無効化（0=Off, 1=Replace, 2=Multiply, 3=Add, 4=Subtract）
            if (mat.HasProperty("_AlphaMaskMode"))
                mat.SetInt("_AlphaMaskMode", 0);

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
            // Rendering Preset: Fade (2)
            if (mat.HasProperty("_Mode"))
                mat.SetFloat("_Mode", 2);

            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;

            // メインパス: Blend SrcAlpha OneMinusSrcAlpha
            if (mat.HasProperty("_BlendOp"))
                mat.SetFloat("_BlendOp", 0);      // Add
            if (mat.HasProperty("_BlendOpAlpha"))
                mat.SetFloat("_BlendOpAlpha", 4);  // Max
            if (mat.HasProperty("_SrcBlend"))
                mat.SetFloat("_SrcBlend", 5);      // SrcAlpha
            if (mat.HasProperty("_DstBlend"))
                mat.SetFloat("_DstBlend", 10);     // OneMinusSrcAlpha
            if (mat.HasProperty("_SrcBlendAlpha"))
                mat.SetFloat("_SrcBlendAlpha", 1);  // One
            if (mat.HasProperty("_DstBlendAlpha"))
                mat.SetFloat("_DstBlendAlpha", 1);  // One

            // ForwardAdd パス
            if (mat.HasProperty("_AddSrcBlend"))
                mat.SetFloat("_AddSrcBlend", 5);    // SrcAlpha
            if (mat.HasProperty("_AddDstBlend"))
                mat.SetFloat("_AddDstBlend", 1);    // One
            if (mat.HasProperty("_AddSrcBlendAlpha"))
                mat.SetFloat("_AddSrcBlendAlpha", 0); // Zero
            if (mat.HasProperty("_AddDstBlendAlpha"))
                mat.SetFloat("_AddDstBlendAlpha", 1); // One

            // ZWrite=1 で深度書き込みを維持（メッシュ重なりの黒化を防止）
            if (mat.HasProperty("_ZWrite"))
                mat.SetFloat("_ZWrite", 1);
            if (mat.HasProperty("_ZTest"))
                mat.SetFloat("_ZTest", 4);          // LessEqual

            // Alpha 関連
            if (mat.HasProperty("_Cutoff"))
                mat.SetFloat("_Cutoff", 0);
            if (mat.HasProperty("_AlphaPremultiply"))
                mat.SetFloat("_AlphaPremultiply", 0);
            if (mat.HasProperty("_AlphaToCoverage"))
                mat.SetFloat("_AlphaToCoverage", 0);
            // これが 1 だとシェーダー内で alpha が強制的に 1 にされる
            if (mat.HasProperty("_AlphaForceOpaque"))
                mat.SetFloat("_AlphaForceOpaque", 0);
            // AlphaMask のブレンドを Off にして意図しない透明を防止
            if (mat.HasProperty("_MainAlphaMaskMode"))
                mat.SetFloat("_MainAlphaMaskMode", 0);
            // AlphaMod を 0 にリセット
            if (mat.HasProperty("_AlphaMod"))
                mat.SetFloat("_AlphaMod", 0);

            // Outline ブレンド
            if (mat.HasProperty("_OutlineSrcBlend"))
                mat.SetFloat("_OutlineSrcBlend", 5);
            if (mat.HasProperty("_OutlineDstBlend"))
                mat.SetFloat("_OutlineDstBlend", 10);
            if (mat.HasProperty("_OutlineSrcBlendAlpha"))
                mat.SetFloat("_OutlineSrcBlendAlpha", 1);
            if (mat.HasProperty("_OutlineDstBlendAlpha"))
                mat.SetFloat("_OutlineDstBlendAlpha", 1);
            if (mat.HasProperty("_OutlineBlendOp"))
                mat.SetFloat("_OutlineBlendOp", 0);
            if (mat.HasProperty("_OutlineBlendOpAlpha"))
                mat.SetFloat("_OutlineBlendOpAlpha", 4);

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
