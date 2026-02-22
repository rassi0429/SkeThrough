using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using UnityEngine;

namespace Kokoa.SkeThrough
{
    internal class TransparentPreviewFilter : IRenderFilter
    {
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            var allPreviews = context.GetComponentsByType<TransparentPreview>().ToList();
            var result = new List<RenderGroup>();

            foreach (var preview in allPreviews)
            {
                context.Observe(preview, p => p.alpha);

                var renderers = preview.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                    {
                        result.Add(RenderGroup.For(renderer));
                    }
                }
            }

            return result.ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(
            RenderGroup group,
            IEnumerable<(Renderer, Renderer)> proxyPairs,
            ComputeContext context)
        {
            var targetRenderer = group.Renderers.First();

            var preview = targetRenderer.GetComponentInParent<TransparentPreview>();

            context.Observe(preview, p => p.alpha);

            var proxyPair = proxyPairs.First();
            var proxyRenderer = proxyPair.Item2;

            var originalMaterials = proxyRenderer.sharedMaterials;
            var transparentMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                if (originalMaterials[i] != null)
                {
                    transparentMaterials[i] = MaterialTransparencyHelper.CreateTransparentCopy(
                        originalMaterials[i], preview.alpha);
                }
            }

            proxyRenderer.sharedMaterials = transparentMaterials;

            return Task.FromResult<IRenderFilterNode>(
                new TransparentPreviewNode(transparentMaterials));
        }

        internal class TransparentPreviewNode : IRenderFilterNode
        {
            public RenderAspects WhatChanged => RenderAspects.Material;

            private Material[] _materials;

            public TransparentPreviewNode(Material[] materials)
            {
                _materials = materials;
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (_materials != null)
                {
                    proxy.sharedMaterials = _materials;
                }
            }

            public void Dispose()
            {
                if (_materials != null)
                {
                    foreach (var mat in _materials)
                    {
                        if (mat != null)
                            Object.DestroyImmediate(mat);
                    }
                    _materials = null;
                }
            }
        }
    }
}
