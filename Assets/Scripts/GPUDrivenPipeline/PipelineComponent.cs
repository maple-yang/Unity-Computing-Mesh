using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public struct PipelineBaseBuffer
{
    public ComputeBuffer instanceCountBuffer;
    public ComputeBuffer allCountBuffer;
    public ComputeBuffer Transforms;
    public ComputeBuffer vertexBuffer;
}

[System.Serializable]
public struct RenderObject
{
    public Mesh staticMesh;
    public Material mat;
    public ComputeBuffer objBuffer;
    public Transform[] objPositions;
    [System.NonSerialized] public Vector3 extent;
    [System.NonSerialized] public int vertexOffset;
    [System.NonSerialized] public int vertexCount;
}

public struct DrawIndirectCommand
{
    public Material mat;
    public int vertexOffset;
    public int vertexCount;
    public int instanceCount;
    public int instanceOffset;
}

public struct Point
{
    public Vector3 vertex;
    public Vector4 tangent;
    public Vector3 normal;
    public Vector2 texcoord;
    public const int SIZE = 48;
};