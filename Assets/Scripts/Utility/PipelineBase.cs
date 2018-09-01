using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GPUPipeline.Culling;

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
    private void Awake()
    {
        cam = GetComponent<Camera>();
        geometryBuffer = new CommandBuffer();
        motionVectorBuffer = new CommandBuffer();
    }

    private void OnEnable()
    {
        cam.AddCommandBuffer(CameraEvent.AfterGBuffer, geometryBuffer);
        cam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, motionVectorBuffer);
    }

    private void OnDisable()
    {
        cam.RemoveCommandBuffer(CameraEvent.AfterGBuffer, geometryBuffer);
        cam.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, motionVectorBuffer);
    }

    private void OnDestroy()
    {
        geometryBuffer.Dispose();
        motionVectorBuffer.Dispose();
    }

    private void OnPreRender()
    {
        Matrix4x4 projMat = cam.projectionMatrix;
        PipeLine.projMatrix = GL.GetGPUProjectionMatrix(projMat, false);
        PipeLine.rtProjMatrix = GL.GetGPUProjectionMatrix(projMat, true);
        PipeLine.lastVPMatrix = PipeLine.rtProjMatrix * PipeLine.viewMatrix;
        PipeLine.viewMatrix = Camera.current.worldToCameraMatrix;
        PipeLine.geometryCommandBuffer = geometryBuffer;
        PipeLine.beforeImageOpaqueBuffer = motionVectorBuffer;
        PipeLine.geometryCommandBuffer.Clear();
        PipeLine.beforeImageOpaqueBuffer.Clear();
        PipeLine.geometryCommandBuffer.SetRenderTarget(gBufferIdentifier, BuiltinRenderTextureType.CameraTarget);
        PipeLine.beforeImageOpaqueBuffer.SetRenderTarget(BuiltinRenderTextureType.MotionVectors, BuiltinRenderTextureType.CameraTarget);
        foreach (var i in onPreRenderEvents)
        {
            i.OnPreRenderEvent();
        }
    }
}
