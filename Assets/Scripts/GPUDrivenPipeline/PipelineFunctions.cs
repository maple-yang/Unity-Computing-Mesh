using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Functional;
using UnityEngine.Rendering;

public static class PipelineFunctions
{
    public const int MATRIXSIZE = 64;
    private static int[] instanceCountArray = null;
    public static void GetCullingPlanes(ref Matrix4x4 vp, Vector4[] cullingPlanes)
    {
        Matrix4x4 invVp = vp.inverse;
        Vector3 nearLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 1));
        Vector3 nearLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 1));
        Vector3 nearRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 1));
        Vector3 nearRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 1));
        Vector3 farLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 0));
        Vector3 farLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 0));
        Vector3 farRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 0));
        Vector3 farRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 0));
        Plane plane;
        //Near
        plane = new Plane(nearRightTop, nearRightButtom, nearLeftButtom);
        cullingPlanes[0] = plane.normal;
        cullingPlanes[0].w = plane.distance;
        //Up
        plane = new Plane(farLeftTop, farRightTop, nearRightTop);
        cullingPlanes[1] = plane.normal;
        cullingPlanes[1].w = plane.distance;
        //Down
        plane = new Plane(nearRightButtom, farRightButtom, farLeftButtom);
        cullingPlanes[2] = plane.normal;
        cullingPlanes[2].w = plane.distance;
        //Left
        plane = new Plane(farLeftButtom, farLeftTop, nearLeftTop);
        cullingPlanes[3] = plane.normal;
        cullingPlanes[3].w = plane.distance;
        //Right
        plane = new Plane(farRightButtom, nearRightButtom, nearRightTop);
        cullingPlanes[4] = plane.normal;
        cullingPlanes[4].w = plane.distance;
        //Far
        plane = new Plane(farLeftButtom, farRightButtom, farRightTop);
        cullingPlanes[5] = plane.normal;
        cullingPlanes[5].w = plane.distance;
    }
    public static void Initialize(ref PipelineBaseBuffer baseBuffer, RenderObject[] allObjects, int maximumInstancing)
    {
        baseBuffer.Transforms = new ComputeBuffer(maximumInstancing, MATRIXSIZE);
        baseBuffer.instanceCountBuffer = new ComputeBuffer(allObjects.Length, 4);
        baseBuffer.allCountBuffer = new ComputeBuffer(1, 4);
        List<Point> allPoints = new List<Point>();
        List<Vector3> currentPoints = new List<Vector3>();
        List<Vector4> currentTangents = new List<Vector4>();
        List<Vector3> currentNormal = new List<Vector3>();
        List<Vector2> currentTexcoord = new List<Vector2>();
        instanceCountArray = new int[allObjects.Length];
        for (int i = 0; i < allObjects.Length; ++i)
        {
            Transform[] trs = allObjects[i].objPositions;
            allObjects[i].objBuffer = new ComputeBuffer(trs.Length, MATRIXSIZE);
            NativeArray<Matrix4x4> matrices = new NativeArray<Matrix4x4>(trs.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int a = 0; a < trs.Length; ++a)
            {
                matrices[a] = trs[a].localToWorldMatrix;
            }
            allObjects[i].objBuffer.SetData(matrices);
            allObjects[i].extent = allObjects[i].staticMesh.bounds.extents;
            matrices.Dispose();
            Mesh mesh = allObjects[i].staticMesh;
            int[] triangles = mesh.triangles;
            mesh.GetVertices(currentPoints);
            mesh.GetUVs(0, currentTexcoord);
            mesh.GetTangents(currentTangents);
            mesh.GetNormals(currentNormal);
            for (int j = 0; j < triangles.Length; ++j)
            {
                Point p;
                int index = triangles[j];
                p.vertex = currentPoints[index];
                p.normal = currentNormal[index];
                p.texcoord = currentTexcoord[index];
                p.tangent = currentTangents[index];
                allPoints.Add(p);
            }
            allObjects[i].vertexCount = triangles.Length;
        }
        for (int i = 1; i < allObjects.Length; ++i)
        {
            allObjects[i].vertexOffset = allObjects[i - 1].vertexOffset + allObjects[i - 1].vertexCount;
        }

        baseBuffer.vertexBuffer = new ComputeBuffer(allPoints.Count, Point.SIZE);
        baseBuffer.vertexBuffer.SetData(allPoints);
    }
    public static void Dispose(ref PipelineBaseBuffer baseBuffer, RenderObject[] allObjects)
    {
        baseBuffer.Transforms.Dispose();
        baseBuffer.allCountBuffer.Dispose();
        baseBuffer.instanceCountBuffer.Dispose();
        baseBuffer.vertexBuffer.Dispose();
        for (int i = 0; i < allObjects.Length; ++i)
        {
            allObjects[i].objBuffer.Dispose();
        }
    }
    public static void SetBaseBuffer(ref PipelineBaseBuffer baseBuffer, ComputeShader computeShader, Vector4[] cullingPlanes)
    {
        NativeArray<int> instanceArray = new NativeArray<int>(baseBuffer.instanceCountBuffer.count, Allocator.Temp, NativeArrayOptions.ClearMemory);
        baseBuffer.instanceCountBuffer.SetData(instanceArray);
        instanceArray.Dispose();
        instanceArray = new NativeArray<int>(1, Allocator.Temp, NativeArrayOptions.ClearMemory);
        baseBuffer.allCountBuffer.SetData(instanceArray);
        instanceArray.Dispose();
        computeShader.SetBuffer(0, ShaderIDs.instanceCountBuffer, baseBuffer.instanceCountBuffer);
        computeShader.SetBuffer(0, ShaderIDs.allCountBuffer, baseBuffer.allCountBuffer);
        computeShader.SetBuffer(0, ShaderIDs.Transforms, baseBuffer.Transforms);
        computeShader.SetVectorArray(ShaderIDs.planes, cullingPlanes);
    }
    private static void DispatchCulling(ref PipelineBaseBuffer baseBuffer, ComputeShader computeShader, ref RenderObject obj, int index)
    {
        computeShader.SetVector(ShaderIDs._Extent, obj.extent);
        computeShader.SetInt(ShaderIDs._IndirectIndex, index);
        computeShader.SetBuffer(0, ShaderIDs.objBuffer, obj.objBuffer);
        ComputeShaderUtility.Dispatch(computeShader, 0, obj.objBuffer.count, 128);
    }

    public static void DispatchCulling(ref PipelineBaseBuffer baseBuffer, ComputeShader computeShader, RenderObject[] obj, List<DrawIndirectCommand> indirectCommands)
    {
        indirectCommands.Clear();
        for (int i = 0; i < obj.Length; ++i)
        {
            DispatchCulling(ref baseBuffer, computeShader, ref obj[i], i);
        }
        baseBuffer.instanceCountBuffer.GetData(instanceCountArray);
        int instanceOffset = instanceCountArray[0];
        DrawIndirectCommand command;
        if(instanceCountArray[0] > 0)
        {
            command.instanceCount = instanceCountArray[0];
            command.instanceOffset = 0;
            command.vertexCount = obj[0].vertexCount;
            command.vertexOffset = obj[0].vertexOffset;
            command.mat = obj[0].mat;
            indirectCommands.Add(command);
        }
        for(int i = 1; i < obj.Length; ++i)
        {
            int insCount = instanceCountArray[i];
            if (insCount > 0)
            {
                command.instanceCount = insCount;
                command.instanceOffset = instanceOffset;
                command.vertexCount = obj[i].vertexCount;
                command.vertexOffset = obj[i].vertexOffset;
                command.mat = obj[i].mat;
                indirectCommands.Add(command);
                instanceOffset += insCount;
            }
        }
    }

    public static void SetShaderBuffer(CommandBuffer cb, ref PipelineBaseBuffer basebuffer)
    {
        cb.SetGlobalBuffer(ShaderIDs.VertexBuffer, basebuffer.vertexBuffer);
        cb.SetGlobalBuffer(ShaderIDs.Transforms, basebuffer.Transforms);
    }
    public static void RenderProceduralCommand(List<DrawIndirectCommand> indirectCommands, CommandBuffer command)
    {
        for (int i = 0; i < indirectCommands.Count; ++i)
        {
            DrawIndirectCommand drawcommand = indirectCommands[i];
            command.SetGlobalVector(ShaderIDs._ProceduralOffset, new Vector2(drawcommand.instanceOffset, drawcommand.vertexOffset));
            command.DrawProcedural(Matrix4x4.identity, drawcommand.mat, 0, MeshTopology.Triangles, drawcommand.vertexCount, drawcommand.instanceCount);
        }
    }
}
