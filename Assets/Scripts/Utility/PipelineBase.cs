using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PipelineBase : MonoBehaviour {
    public static List<PipeLine> onPreRenderEvents = new List<PipeLine>(10);
    public static RenderTargetIdentifier[] gBufferIdentifier = new RenderTargetIdentifier[]
    {
                    BuiltinRenderTextureType.GBuffer0,
                    BuiltinRenderTextureType.GBuffer1,
                    BuiltinRenderTextureType.GBuffer2,
                    BuiltinRenderTextureType.GBuffer3
    };
    private Camera cam;
    private CommandBuffer geometryBuffer;
    private CommandBuffer motionVectorBuffer;
    private CommandBuffer beforeTransparentBuffer;
    private void Awake()
    {
        cam = GetComponent<Camera>();
        geometryBuffer = new CommandBuffer();
        motionVectorBuffer = new CommandBuffer();
        beforeTransparentBuffer = new CommandBuffer();
    }

    private void OnEnable()
    {
        cam.AddCommandBuffer(CameraEvent.AfterGBuffer, geometryBuffer);
        cam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, motionVectorBuffer);
        cam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, beforeTransparentBuffer);
        
    }

    private void OnDisable()
    {
        cam.RemoveCommandBuffer(CameraEvent.AfterGBuffer, geometryBuffer);
        cam.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, motionVectorBuffer);
        cam.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, beforeTransparentBuffer);
    }

    private void OnDestroy()
    {
        geometryBuffer.Dispose();
        motionVectorBuffer.Dispose();
        beforeTransparentBuffer.Dispose();
    }

    private void OnPreRender()
    {
        Matrix4x4 projMat = cam.projectionMatrix;
        PipeLine.projMatrix = GL.GetGPUProjectionMatrix(projMat, false);
        PipeLine.rtProjMatrix = GL.GetGPUProjectionMatrix(projMat, true);
        PipeLine.viewMatrix = Camera.current.worldToCameraMatrix;
        PipeLine.geometryBuffer = geometryBuffer;
        PipeLine.motionVectorBuffer = motionVectorBuffer;
        PipeLine.beforeTransparentBuffer = beforeTransparentBuffer;
        geometryBuffer.Clear();
        motionVectorBuffer.Clear();
        beforeTransparentBuffer.Clear();
        geometryBuffer.SetRenderTarget(gBufferIdentifier, BuiltinRenderTextureType.CameraTarget);
        motionVectorBuffer.SetRenderTarget(BuiltinRenderTextureType.MotionVectors, BuiltinRenderTextureType.CameraTarget);
        beforeTransparentBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        foreach (var i in onPreRenderEvents)
        {
            i.OnPreRenderEvent();
        }
    }
}
