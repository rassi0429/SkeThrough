using UnityEngine;
using nadena.dev.ndmf;

namespace Kokoa.SkeThrough
{
    [AddComponentMenu("SkeThrough/Transparent Preview")]
    [DisallowMultipleComponent]
    public class TransparentPreview : MonoBehaviour, INDMFEditorOnly
    {
        [Range(0f, 1f)]
        public float alpha = 0.5f;

        [Tooltip("RenderQueue を上書きする (-1 で元マテリアルの値を維持)")]
        public int renderQueueOverride = -1;
    }
}
