using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public struct RainData
{
    public RainSettingData settingData;
    public RainShaderData shaderData;
}
[System.Serializable]
public struct RainSettingData
{
    public int maxSize;
    public int occlusionResolution;
    public ComputeShader randomDrawShader;
    public float fallDownVeolocity;
}

public struct RainShaderData
{
    public Vector3 volume;
    public Vector3 position;
    public Vector3 forward;
    public Vector3 right;
    public Vector3 up;
    public ComputeBuffer instancingBuffer;
    public RenderTexture shadowTexture;
}

public struct RainRender
{
    public Material rainMaterial;
    public Camera shadowCamera;
}

public struct RainPanel
{
    public Vector3 normal;
    public Vector3 binormal;
    public Vector3 position;
    public const int SIZE = 36;
}

public static partial class ShaderIDs
{
    public static int _Direction = Shader.PropertyToID("_Direction");
    public static int instancingBuffer = Shader.PropertyToID("instancingBuffer");
    public static int _InstancingBufferCount = Shader.PropertyToID("_InstancingBufferCount");
    public static int _RandomNumber = Shader.PropertyToID("_RandomNumber");
    public static int _ShadowTexture = Shader.PropertyToID("_ShadowTexture");
    public static int _WorldToShadowMatrix = Shader.PropertyToID("_WorldToShadowMatrix");
    public static int _CameraState = Shader.PropertyToID("_CameraState");
    public static int _FallDownSpeed = Shader.PropertyToID("_FallDownSpeed");
    public static int _ShadowTextureResolution = Shader.PropertyToID("_ShadowTextureResolution");
    public static int _LookPos = Shader.PropertyToID("_LookPos");
    public static int _Extent = Shader.PropertyToID("_Extent");
}