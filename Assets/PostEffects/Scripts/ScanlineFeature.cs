using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScanlineFeature : ScriptableRendererFeature
{
    // Inspectorで調整できるパラメータ群
    [System.Serializable]
    public class Settings
    {
        public Shader shader;

        [Range(60, 480)]
        public float lineCount = 180f;   // 走査線の本数

        [Range(0f, 1f)]
        public float intensity = 0.25f;  // 暗線の強さ

        [Range(0f, 2f)]
        public float speed = 0f;         // 流れる速さ（0=静止）

        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public Settings settings = new();

    ScanlinePass _pass;
    Material _material;

    // Feature初期化（Unityが呼ぶ）
    public override void Create()
    {
        if (settings.shader == null)
        {
            Debug.LogWarning("ScanlineFeature: Shader が未アサインです");
            return;
        }
        _material = CoreUtils.CreateEngineMaterial(settings.shader);
        _pass = new ScanlinePass(_material, settings);
        _pass.renderPassEvent = settings.passEvent;
    }

    // 毎フレーム、パスをキューに追加
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        if (_material == null) return;
        renderer.EnqueuePass(_pass);
    }

    // 解放処理
    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_material);
    }
}