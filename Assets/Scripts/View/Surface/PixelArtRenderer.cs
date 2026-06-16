using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class PixelArtRendererFeature : ScriptableRendererFeature
{
    // Material→接尾辞 Mat
    // Matrix, Texture→接尾辞なし
    public Material atmosphereMat; // 霧と埃
    public Material ditherMat; // Bayerディザリングとパレット量子化

    private PixelArtRendererPass _pass;

    public override void Create()
    {
        _pass = new PixelArtRendererPass(atmosphereMat, ditherMat)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (atmosphereMat == null || ditherMat == null)
        {
            Debug.LogWarning("[PixelArtRenderer] atmosphereMat, ditherMatが未設定です");
            return;
        }
        #if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return;
        }
        #endif
        renderer.EnqueuePass(_pass);
    }

    /// テクスチャーをパイプライン処理
    /// src→[atmosphereMat]→temp→[ditherMat]→dst
    public class PixelArtRendererPass : ScriptableRenderPass
    {
        private readonly Material atmosphereMat;
        private readonly Material ditherMat;

        private static readonly int InverseViewProjId = Shader.PropertyToID("_InverseViewProjMatrix");

        public PixelArtRendererPass(Material atmosphereMat, Material ditherMat)
        {
            this.atmosphereMat = atmosphereMat;
            this.ditherMat = ditherMat;
            // カメラカラーをサンプリングするため中間テクスチャを要求（バックバッファ直書きを防ぐ）
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // atmosphereMatへ逆ビュープロジェクション行列を渡す
            Camera camera = cameraData.camera;
            Matrix4x4 gpuProj = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
            Matrix4x4 inverseViewProj = (gpuProj * camera.worldToCameraMatrix).inverse;
            atmosphereMat.SetMatrix(InverseViewProjId, inverseViewProj);

            TextureHandle source = resourceData.activeColorTexture;

            // 中間テクスチャ（旧cmd.GetTemporaryRT相当）
            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            TextureHandle temp = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, descriptor, "_PixelArtTemp", false, FilterMode.Bilinear);

            // src→[atmosphereMat]→temp
            RenderGraphUtils.BlitMaterialParameters atmospherePass = new(source, temp, atmosphereMat, 0);
            renderGraph.AddBlitPass(atmospherePass, "PixelArt_Atmosphere");

            // temp→[ditherMat]→dst(=source)
            RenderGraphUtils.BlitMaterialParameters ditherPass = new(temp, source, ditherMat, 0);
            renderGraph.AddBlitPass(ditherPass, "PixelArt_Dither");
        }
    }
}
