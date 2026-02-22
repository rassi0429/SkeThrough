using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(Kokoa.SkeThrough.TransparentPreviewPlugin))]

namespace Kokoa.SkeThrough
{
    public class TransparentPreviewPlugin : Plugin<TransparentPreviewPlugin>
    {
        public override string QualifiedName => "dev.kokoa.skethrough";
        public override string DisplayName => "SkeThrough";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .Run("SkeThrough Transparent Preview", ctx =>
                {
                    // プレビュー専用 - ビルド時処理なし
                    // TransparentPreview は IEditorOnly なのでビルド時自動除去
                })
                .PreviewingWith(new TransparentPreviewFilter());
        }
    }
}
