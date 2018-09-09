using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GpuDrivenPipeline : PipeLine
{
    public ComputeShader gpuFrustumShader;
    public RenderObject[] renderObjects;
    public PipelineBaseBuffer baseBuffer;
    public static Vector4[] frustumCullingPlanes = new Vector4[6];
    private List<DrawIndirectCommand> drawingCommands;

    private void Awake()
    {
        drawingCommands = new List<DrawIndirectCommand>(renderObjects.Length);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        int count = 0;
        foreach(var i in renderObjects)
        {
            count += i.objPositions.Length;
        }
        PipelineFunctions.Initialize(ref baseBuffer, renderObjects, count);
        System.GC.Collect();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        PipelineFunctions.Dispose(ref baseBuffer, renderObjects);
        System.GC.Collect();
    }

    public override void OnPreRenderEvent()
    {
        Matrix4x4 vp = projMatrix * viewMatrix;
        PipelineFunctions.GetCullingPlanes(ref vp, frustumCullingPlanes);
        PipelineFunctions.SetBaseBuffer(ref baseBuffer, gpuFrustumShader, frustumCullingPlanes);
        PipelineFunctions.DispatchCulling(ref baseBuffer, gpuFrustumShader, renderObjects, drawingCommands);
        PipelineFunctions.SetShaderBuffer(geometryBuffer, ref baseBuffer);
        PipelineFunctions.RenderProceduralCommand(drawingCommands, geometryBuffer);
    }
}
