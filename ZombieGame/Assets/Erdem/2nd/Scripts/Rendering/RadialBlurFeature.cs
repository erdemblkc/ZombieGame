using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class RadialBlurFeature : ScriptableRendererFeature
{
    private RadialBlurPass _pass;
    private Material       _material;

    public override void Create()
    {
        var shader = Shader.Find("Custom/RadialBlur");
        if (shader == null)
        {
            Debug.LogWarning("[RadialBlurFeature] 'Custom/RadialBlur' shader bulunamadı.");
            return;
        }
        _material = CoreUtils.CreateEngineMaterial(shader);
        _pass     = new RadialBlurPass(_material);
        _pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_pass == null || _material == null) return;
        if (renderingData.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_material);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

class RadialBlurPass : ScriptableRenderPass
{
    public static readonly int StrengthId = Shader.PropertyToID("_RadialBlurStrength");

    private readonly Material _mat;

    public RadialBlurPass(Material mat)
    {
        _mat = mat;
        requiresIntermediateTexture = true;
    }

    private class PassData
    {
        public TextureHandle src;
        public Material      mat;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (Shader.GetGlobalFloat(StrengthId) < 0.001f) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer) return;

        var source = resourceData.activeColorTexture;

        // Kaynak texture ile aynı formatta geçici texture oluştur
        var tempDesc             = renderGraph.GetTextureDesc(source);
        tempDesc.name            = "_RadialBlurTemp";
        tempDesc.depthBufferBits = DepthBits.None;
        tempDesc.clearBuffer     = false;
        var tempTex = renderGraph.CreateTexture(tempDesc);

        // Pass 1: Blur uygula — source → temp
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("RadialBlur_Blit", out var passData))
        {
            passData.src = source;
            passData.mat = _mat;

            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(tempTex, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1, 1, 0, 0), data.mat, 0);
            });
        }

        // Pass 2: Temp'i geri kopyala — temp → source (material yok, saf kopyalama)
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("RadialBlur_Copy", out var passData))
        {
            passData.src = tempTex;
            passData.mat = null;

            builder.UseTexture(tempTex, AccessFlags.Read);
            builder.SetRenderAttachment(source, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1, 1, 0, 0), 0, false);
            });
        }
    }

    // Compatibility Mode fallback (Render Graph kapalıysa kullanılır)
    private RTHandle _tempRT;

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc             = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        RenderingUtils.ReAllocateIfNeeded(ref _tempRT, desc, FilterMode.Bilinear, name: "_RadialBlurTemp");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (Shader.GetGlobalFloat(StrengthId) < 0.001f || _mat == null) return;

        var cmd    = CommandBufferPool.Get("RadialBlur");
        var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

        Blitter.BlitCameraTexture(cmd, source, _tempRT, _mat, 0);
        Blitter.BlitCameraTexture(cmd, _tempRT, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }

    public void Dispose() => _tempRT?.Release();
}
