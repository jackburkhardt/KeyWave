using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KawaseBlur : ScriptableRendererFeature
{
    [System.Serializable]
    public class KawaseBlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material blurMaterial = null;
        
        public bool copyToFramebuffer;
        public string targetName = "_blurTexture";
    }

  public KawaseBlurSettings settings = new KawaseBlurSettings();
   
   public KawaseBlurVolumeSettings blurSettings;

    class CustomRenderPass : ScriptableRenderPass
    {
        public Material blurMaterial;
        public int passes;
        public int downsample;
        public bool copyToFramebuffer;
        public string targetName;        
        string profilerTag;

        int tmpId1;
        int tmpId2;

        RenderTargetIdentifier tmpRT1;
        RenderTargetIdentifier tmpRT2;
        
        public KawaseBlurVolumeSettings blurSettings;
        
        private RenderTargetIdentifier source { get; set; }

        public bool Setup(ScriptableRenderer renderer, RenderTargetIdentifier src, out KawaseBlurVolumeSettings _blurSettings)
        {
            source = src;
            blurSettings = VolumeManager.instance.stack.GetComponent<KawaseBlurVolumeSettings>();
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            if (blurSettings != null && blurSettings.IsActive())
            {
                _blurSettings = blurSettings;
                blurMaterial = new Material(Shader.Find("Custom/RenderFeature/KawaseBlur"));
                return true;
            }

            _blurSettings = null;
            return false;
        }
        public CustomRenderPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (blurSettings == null || !blurSettings.IsActive())
            {
                return;
            }
            
            var width = cameraTextureDescriptor.width / blurSettings.downsample.value;
            var height = cameraTextureDescriptor.height / blurSettings.downsample.value;

            tmpId1 = Shader.PropertyToID("tmpBlurRT1");
            tmpId2 = Shader.PropertyToID("tmpBlurRT2");
            cmd.GetTemporaryRT(tmpId1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(tmpId2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            tmpRT1 = new RenderTargetIdentifier(tmpId1);
            tmpRT2 = new RenderTargetIdentifier(tmpId2);
            
            ConfigureTarget(tmpRT1);
            ConfigureTarget(tmpRT2);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (blurSettings == null || !blurSettings.IsActive())
            {
                return;
            }
            
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // first pass
            // cmd.GetTemporaryRT(tmpId1, opaqueDesc, FilterMode.Bilinear);
            cmd.SetGlobalFloat("_offset", 1.5f);
            cmd.Blit(source, tmpRT1, blurMaterial);

            for (var i=1; i<blurSettings.strength.value -1; i++) {
                cmd.SetGlobalFloat("_offset", 0.5f + i);
                cmd.Blit(tmpRT1, tmpRT2, blurMaterial);

                // pingpong
                var rttmp = tmpRT1;
                tmpRT1 = tmpRT2;
                tmpRT2 = rttmp;
            }

            // final pass
            cmd.SetGlobalFloat("_offset", 0.5f + blurSettings.strength.value - 1f);
            if (copyToFramebuffer) {
                cmd.Blit(tmpRT1, source, blurMaterial);
            } else {
                cmd.Blit(tmpRT1, tmpRT2, blurMaterial);
                cmd.SetGlobalTexture(targetName, tmpRT2);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass scriptablePass;
    
    

    public override void Create()
    {
        scriptablePass = new CustomRenderPass("KawaseBlur");
        name = "KawaseBlur";
        if (blurSettings == null)
        {
            blurSettings = VolumeManager.instance.stack.GetComponent<KawaseBlurVolumeSettings>();
        }
        scriptablePass.blurMaterial = settings.blurMaterial;
        scriptablePass.passes = blurSettings.strength.value;
        scriptablePass.downsample = blurSettings.downsample.value;
        scriptablePass.copyToFramebuffer = settings.copyToFramebuffer;
        scriptablePass.targetName = settings.targetName;

        scriptablePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        scriptablePass.Setup(renderer, src, out var _blurSettings);
        blurSettings = _blurSettings;
        renderer.EnqueuePass(scriptablePass);
    }
}


