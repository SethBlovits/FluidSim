using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogRenderFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {

        private Material m_fogMaterial;
        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fog Pass");
        RTHandle m_CameraColorTarget;
        float m_Intensity;
        float m_Density;
        float m_Offset;
        Color m_FogColor;
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public CustomRenderPass(Material fogMaterial){
            m_fogMaterial = fogMaterial;
        }
        public void SetTarget(RTHandle colorHandle ,float intensity,float density,float offset,Color color){
            m_CameraColorTarget = colorHandle;
            m_Intensity = intensity;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(m_CameraColorTarget);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if(cameraData.camera.cameraType != CameraType.Game){
                return;
            }
            if(m_fogMaterial == null){
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get();
            using(new ProfilingScope(cmd,m_ProfilingSampler)){
                m_fogMaterial.SetFloat("_Intensity", m_Intensity);
                m_fogMaterial.SetFloat("_FogDensity",m_Density);
                m_fogMaterial.SetFloat("_FogOffset",m_Offset);
                //m_fogMaterial.SetColor("_FogColor",m_FogColor);

                Blitter.BlitCameraTexture(cmd,m_CameraColorTarget,m_CameraColorTarget,m_fogMaterial,0);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass=null;
    [SerializeField]
    private Shader FogShader;
    public float m_Intensity;
    public float m_Density;
    public float m_Offset;
    public Color m_FogColor;

    Material m_fogMaterial;
    /// <inheritdoc/>
    public override void Create()
    {
        m_fogMaterial = CoreUtils.CreateEngineMaterial(FogShader);
        m_ScriptablePass = new CustomRenderPass(m_fogMaterial);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer,in RenderingData renderingData){
        if(renderingData.cameraData.cameraType == CameraType.Game){
            m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color);
            m_ScriptablePass.SetTarget(renderer.cameraColorTargetHandle,m_Intensity,m_Density,m_Offset,m_FogColor);
        }
    }
    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(renderingData.cameraData.cameraType == CameraType.Game){
            renderer.EnqueuePass(m_ScriptablePass);
        }
        
    }
    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_fogMaterial);
    }
}


