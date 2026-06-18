using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule; // 追加
using UnityEngine.Rendering.Universal;

public class ScanlinePass : ScriptableRenderPass
{
    static readonly int LineCountID = Shader.PropertyToID("_LineCount");
    static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    static readonly int SpeedID     = Shader.PropertyToID("_Speed");

    readonly Material _material;
    readonly ScanlineFeature.Settings _settings;
    RTHandle _tempRT;

    // RenderGraph用：パスに渡すデータをまとめるクラス
    class PassData
    {
        public TextureHandle src;
        public TextureHandle tmp;
        public Material material;
    }

    public ScanlinePass(Material mat, ScanlineFeature.Settings settings)
    {
        _material = mat;
        _settings = settings;
        // RenderGraph対応パスであることを宣言
        requiresIntermediateTexture = true;
    }

    // ─── RenderGraph パス（URP 17+ / Unity 6） ───────────────────────────
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData   = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer) return; // バックバッファ直書き時はスキップ

        // シェーダーパラメータをここでセット
        _material.SetFloat(LineCountID, _settings.lineCount);
        _material.SetFloat(IntensityID, _settings.intensity);
        _material.SetFloat(SpeedID,     _settings.speed);

        // カメラカラーバッファのDescriptorを取得して一時テクスチャを作成
        var desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        TextureHandle src = resourceData.activeColorTexture;
        TextureHandle tmp = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, desc, "_ScanlineTemp", false);

        using (var builder = renderGraph.AddUnsafePass<PassData>("Scanline Pass", out var passData))
        {
            passData.src      = src;
            passData.tmp      = tmp;
            passData.material = _material;

            builder.UseTexture(src, AccessFlags.ReadWrite);
            builder.UseTexture(tmp, AccessFlags.ReadWrite);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
            {
                var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                // source → tempRT（エフェクト適用）→ source に戻す
                Blitter.BlitCameraTexture(cmd, data.src, data.tmp, data.material, 0);
                Blitter.BlitCameraTexture(cmd, data.tmp, data.src);
            });
        }
    }

    // ─── 旧パス（Compatibility Mode / RenderGraph 無効時） ──────────────
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData data)
    {
        var desc = data.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        RenderingUtils.ReAllocateIfNeeded(ref _tempRT, desc, name: "_ScanlineTemp");
    }

    public override void Execute(ScriptableRenderContext ctx, ref RenderingData data)
    {
        var cmd = CommandBufferPool.Get("Scanline");

        _material.SetFloat(LineCountID, _settings.lineCount);
        _material.SetFloat(IntensityID, _settings.intensity);
        _material.SetFloat(SpeedID,     _settings.speed);

        var source = data.cameraData.renderer.cameraColorTargetHandle;
        Blitter.BlitCameraTexture(cmd, source, _tempRT, _material, 0);
        Blitter.BlitCameraTexture(cmd, _tempRT, source);

        ctx.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }
}