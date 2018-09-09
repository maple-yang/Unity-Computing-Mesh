using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RainningSystem : PipeLine
{
    #region CONST
    private static Vector4[] directions = new Vector4[3];
    private const int THREADGROUPCOUNT = 128;
    #endregion
    #region STATIC_FUNCTION
    private static void Initialize(ref RainData data, ref RainRender rend, Camera cam, Transform box)
    {
        data.shaderData.shadowTexture = new RenderTexture(data.settingData.occlusionResolution, data.settingData.occlusionResolution, 16, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        data.shaderData.shadowTexture.enableRandomWrite = true;
        data.shaderData.instancingBuffer = new ComputeBuffer(data.settingData.maxSize, RainPanel.SIZE);
        data.shaderData.position = box.position;
        data.shaderData.volume = box.localScale;
        data.shaderData.forward = box.forward;
        data.shaderData.up = box.up;
        data.shaderData.right = box.right;
        data.shaderData.velocityBuffer = new ComputeBuffer(data.settingData.maxSize, 4);
        rend.shadowCamera = cam;
        rend.rainMaterial = new Material(Shader.Find("Unlit/RainningProcedural"));
        cam.enabled = false;
        cam.orthographic = true;
        cam.aspect = data.shaderData.volume.x / data.shaderData.volume.z;
        cam.targetTexture = data.shaderData.shadowTexture;
        cam.transform.position = data.shaderData.position + data.shaderData.volume.y / 2 * box.transform.up;
        cam.transform.forward = -box.transform.up;
        cam.transform.right = box.transform.right;
        cam.transform.up = box.transform.forward;
        cam.farClipPlane = data.shaderData.volume.y;
        cam.nearClipPlane = 0;
        cam.SetReplacementShader(Shader.Find("Hidden/OrthographiceDepth"), "");
        cam.orthographicSize = data.shaderData.volume.z / 2;
        cam.renderingPath = RenderingPath.Forward;
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.white;
        cam.useOcclusionCulling = false;
        cam.allowHDR = true;
        cam.allowMSAA = false;
        RainPanel[] panels = new RainPanel[data.settingData.maxSize];
        for (int i = 0; i < panels.Length; ++i)
        {
            panels[i].binormal = Vector3.right;
            panels[i].normal = Vector3.forward;
            Vector3 min = data.shaderData.position - data.shaderData.volume * 0.5f;
            Vector3 max = data.shaderData.position + data.shaderData.volume * 0.5f;
            panels[i].position = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
        }
        data.shaderData.instancingBuffer.SetData(panels);
        float[] fallDownSpeeds = new float[data.settingData.maxSize];
        for (int i = 0; i < fallDownSpeeds.Length; ++i)
        {
            fallDownSpeeds[i] = Random.Range(data.settingData.fallDownVeolocity.x, data.settingData.fallDownVeolocity.y);
        }
        data.shaderData.velocityBuffer.SetData(fallDownSpeeds);
    }
    private static void Dispose(ref RainData data, ref RainRender rend)
    {
        Destroy(rend.shadowCamera);
        Destroy(rend.rainMaterial);
        Destroy(data.shaderData.shadowTexture);
        data.shaderData.instancingBuffer.Dispose();
        data.shaderData.velocityBuffer.Dispose();
    }
    private static void SetShaderBuffer(ref RainData data, ref RainRender rend, CommandBuffer buffer)
    {
        ComputeShader sd = data.settingData.randomDrawShader;
        buffer.SetGlobalVector(ShaderIDs._CameraState, new Vector4(-data.shaderData.up.x, -data.shaderData.up.y, -data.shaderData.up.z, rend.shadowCamera.farClipPlane));
        sd.SetVector(ShaderIDs._CameraState, new Vector4(-data.shaderData.up.x, -data.shaderData.up.y, -data.shaderData.up.z, rend.shadowCamera.farClipPlane));
        buffer.SetGlobalVector(ShaderIDs._CameraPos, rend.shadowCamera.transform.position);
        buffer.SetGlobalBuffer(ShaderIDs.instancingBuffer, data.shaderData.instancingBuffer);
        buffer.SetGlobalTexture(ShaderIDs._EnvReflect, data.settingData.reflectionMap);
        sd.SetVector(ShaderIDs._CameraPos, rend.shadowCamera.transform.position);
        sd.SetBuffer(0, ShaderIDs.instancingBuffer, data.shaderData.instancingBuffer);
        directions[0] = data.shaderData.right;
        directions[1] = data.shaderData.forward;
        directions[2] = data.shaderData.volume;
        sd.SetVectorArray(ShaderIDs._Direction, directions);
        sd.SetFloat(ShaderIDs._RandomNumber, Random.Range(0f, 10f));
        sd.SetVector(ShaderIDs._ShadowTextureResolution, new Vector2(data.shaderData.shadowTexture.width, data.shaderData.shadowTexture.height));
        sd.SetTexture(0, ShaderIDs._ShadowTexture, data.shaderData.shadowTexture);
        sd.SetMatrix(ShaderIDs._WorldToShadowMatrix, GL.GetGPUProjectionMatrix(rend.shadowCamera.projectionMatrix, false) * rend.shadowCamera.worldToCameraMatrix);
        sd.SetFloat(ShaderIDs._FallDownSpeed, Time.deltaTime);
        sd.SetBuffer(0, ShaderIDs.velocityBuffer, data.shaderData.velocityBuffer);
        sd.SetVector(ShaderIDs._LookPos, Camera.current.transform.position);
        sd.SetVector(ShaderIDs._FallDownRange, data.settingData.fallDownVeolocity);
    }
    private static void DrawShadow(ref RainRender rend)
    {
        ShadowQuality currentQuality = QualitySettings.shadows;
        QualitySettings.shadows = ShadowQuality.Disable;
        rend.shadowCamera.Render();
        QualitySettings.shadows = currentQuality;
    }
    private static void Dispatch(ref RainData data)
    {
        ComputeShaderUtility.Dispatch(data.settingData.randomDrawShader, 0, data.shaderData.instancingBuffer.count, THREADGROUPCOUNT);
    }
    private static void Draw(ref RainData data, ref RainRender rend, CommandBuffer buffer)
    {
        buffer.DrawProcedural(Matrix4x4.identity, rend.rainMaterial, 0, MeshTopology.Quads, 4, data.shaderData.instancingBuffer.count);
    }
    #endregion
    public RainData data;
    public RainRender rainRend;
    public Transform box;
    private void Awake()
    {
        Initialize(ref data, ref rainRend, GetComponent<Camera>(), box);
    }

    public override void OnPreRenderEvent()
    {
        SetShaderBuffer(ref data, ref rainRend, beforeTransparentBuffer);
        DrawShadow(ref rainRend);
        Dispatch(ref data);
        Draw(ref data, ref rainRend, beforeTransparentBuffer);
    }

    private void OnDestroy()
    {
        Dispose(ref data, ref rainRend);

    }
}
