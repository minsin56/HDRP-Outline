using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Serialization;

namespace Modules.Rendering.Outline
{
    [Serializable,VolumeComponentMenu("Post-processing/Custom/OutlinePass")]
    public class OutlinePass: CustomPass
    {
        public LayerMask OutlineLayer = 0;
        [ColorUsage(false, true)] public Color OutlineColor = Color.black;
        public float Threshold = 1;
        public float Thickness = 1;
        public float OutlineIntensity = 1;

        public static List<Renderer> OutlineRenderers = new List<Renderer>();

        // To make sure the shader will ends up in the build, we keep it's reference in the custom pass
        [SerializeField, HideInInspector] private Shader OutlineShader;

        Material FullscreenOutline;
        RTHandle OutlineBuffer;

        protected override void Setup(ScriptableRenderContext RenderContext, CommandBuffer CMD)
        {
            OutlineShader = Shader.Find("Hidden/Shader/OutlinePass");
            FullscreenOutline = CoreUtils.CreateEngineMaterial(OutlineShader);

            OutlineBuffer = RTHandles.Alloc
            (
                Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
                useDynamicScale: true, name: "Outline Buffer"
            );
        }

        protected override void Execute(CustomPassContext CTX)
        {
            CoreUtils.SetRenderTarget(CTX.cmd, OutlineBuffer, ClearFlag.Color);

            OutlineRenderers.ForEach((Renderer =>
            {
                for (int i = 0; i < Renderer.sharedMaterials.Length; i++)
                {
                    CTX.cmd.DrawRenderer(Renderer, Renderer.sharedMaterials[i], i);
                }
            }));
            
            CustomPassUtils.DrawRenderers(CTX, OutlineLayer);


            CTX.propertyBlock.SetColor("OutlineColor", OutlineColor);
            CTX.propertyBlock.SetTexture("OutlineBuffer", OutlineBuffer);
            CTX.propertyBlock.SetFloat("Threshold", Mathf.Max(0.000001f, Threshold * 0.01f));
            CTX.propertyBlock.SetFloat("Thickness", Thickness);
            CTX.propertyBlock.SetFloat("OutlineIntensity", OutlineIntensity);
            CTX.propertyBlock.SetVector("TexelSize", OutlineBuffer.rt.texelSize);
            
            
            CoreUtils.DrawFullScreen(CTX.cmd, FullscreenOutline, CTX.cameraColorBuffer, shaderPassId: 0,
                properties: CTX.propertyBlock);
        }

        protected override void Cleanup()
        {
            CoreUtils.Destroy(FullscreenOutline);
            OutlineBuffer.Release();
        }
        
    }
}