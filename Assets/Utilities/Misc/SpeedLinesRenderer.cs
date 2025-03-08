using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volpi.Entertainment.SDK.Utilities
{
    public class SpeedLinesRenderer : ScriptableRendererFeature
    {
        private SpeedLinesPass _pass;
        
        private class SpeedLinesPass : ScriptableRenderPass
        {
            private readonly Material _material;
            private RenderTargetIdentifier _source;
            private readonly RTHandle _tempTexture;
            private SpeedLinesEffect _settings;
            
            private static readonly int _strength = Shader.PropertyToID("_Strength");
            private static readonly int _speed = Shader.PropertyToID("_Speed");
            private static readonly int _lineCount = Shader.PropertyToID("_LineCount");
            private static readonly int _lineWidth = Shader.PropertyToID("_LineWidth");
            private static readonly int _fadeDistance = Shader.PropertyToID("_FadeDistance");
            private static readonly int _centerOffset = Shader.PropertyToID("_CenterOffset");

            public SpeedLinesPass()
            {
                Shader shader = Shader.Find("Hidden/Custom/AnimeSpeedLinesPostProcess");
                
                if (shader != null)
                {
                    _material = new Material(shader);
                }
                else
                {
                    Debug.LogError(
                        "Could not find Anime Speed Lines shader. Make sure the shader is included in your project.");
                }

                _tempTexture = RTHandles.Alloc("_ShaderProperty", name: "_ShaderProperty");
            }

            public void Setup(SpeedLinesEffect settings)
            {
                _settings = settings;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                _source = renderingData.cameraData.renderer.cameraColorTargetHandle;
                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

                cmd.GetTemporaryRT(Shader.PropertyToID(_tempTexture.name), descriptor, FilterMode.Bilinear);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (_material == null || _settings == null)
                {
                    return;
                }

                CommandBuffer cmd = CommandBufferPool.Get("AnimeSpeedLinesEffect");

                _material.SetFloat(_strength, _settings.Strength.value);
                _material.SetFloat(_speed, _settings.Speed.value);
                _material.SetFloat(_lineCount, _settings.LineCount.value);
                _material.SetFloat(_lineWidth, _settings.LineWidth.value);
                _material.SetFloat(_fadeDistance, _settings.FadeDistance.value);
                _material.SetVector(_centerOffset, _settings.CenterOffset.value);

                Blit(cmd, _source, _tempTexture, _material);
                Blit(cmd, _tempTexture, _source);
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(Shader.PropertyToID(_tempTexture.name));
            }
        }

        public override void Create()
        {
            _pass = new SpeedLinesPass
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            VolumeStack stack = VolumeManager.instance.stack;
            SpeedLinesEffect settings = stack.GetComponent<SpeedLinesEffect>();

            if (settings != null && settings.IsActive())
            {
                _pass.Setup(settings);
                renderer.EnqueuePass(_pass);
            }
        }
    }
}