using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class CustomShadowCamera : MonoBehaviour {
    private const string shadowMaskShader = "Hidden/ShadowMask";
    private Material shadowMaskMat;
    private CommandBuffer beforeLightingBuffer;
    private Camera cam;
    private RenderTexture shadowMask;
    private void Awake()
    {
        
        beforeLightingBuffer = new CommandBuffer();
        cam = GetComponent<Camera>();
        cam.AddCommandBuffer(CameraEvent.BeforeLighting, beforeLightingBuffer);
        shadowMaskMat = new Material(Shader.Find(shadowMaskShader));
        shadowMask = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
    }

    private void OnPreRender()
    {
        if(shadowMask.width != cam.pixelWidth || shadowMask.height != cam.pixelHeight)
        {
            shadowMask.Release();
            shadowMask.width = cam.pixelWidth;
            shadowMask.height = cam.pixelHeight;
            shadowMask.Create();
        }
        CustomDirectionalShadow.current.OnDrawShadow();
        beforeLightingBuffer.Clear();
        beforeLightingBuffer.SetGlobalTexture(ShaderIDs._CustomShadowMap, shadowMask);
        beforeLightingBuffer.SetGlobalMatrix(ShaderIDs._InvVP, (GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix).inverse);
        beforeLightingBuffer.Blit(null, shadowMask, shadowMaskMat, 0);
    }

    private void OnDestroy()
    {
        Destroy(shadowMaskMat);
        beforeLightingBuffer.Dispose();
    }
}
