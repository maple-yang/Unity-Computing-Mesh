using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Light))]
[RequireComponent(typeof(Camera))]
public class CustomDirectionalShadow : MonoBehaviour
{
    public struct AspectInfo
    {
        public Vector3 inPlanePoint;
        public Vector3 planeNormal;
        public float size;
    }
    public static CustomDirectionalShadow current;
    private static Light directionalLight;
    private static Camera shadowCam;
    private const string depthShader = "Hidden/ShadowDepth";
    private static Vector3[] frustumCorners = new Vector3[8];
    private static AspectInfo[] shadowFrustumPlanes = new AspectInfo[3];
    private static void SetFrustumCorners(Camera cam, float nearPlane, float farPlane)
    {
        //bottom left
        frustumCorners[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, nearPlane));
        // bottom right
        frustumCorners[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, nearPlane));
        // top left
        frustumCorners[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, nearPlane));
        // top right
        frustumCorners[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, nearPlane));
        //bottom left
        frustumCorners[4] = cam.ViewportToWorldPoint(new Vector3(0, 0, farPlane));
        // bottom right
        frustumCorners[5] = cam.ViewportToWorldPoint(new Vector3(1, 0, farPlane));
        // top left
        frustumCorners[6] = cam.ViewportToWorldPoint(new Vector3(0, 1, farPlane));
        // top right
        frustumCorners[7] = cam.ViewportToWorldPoint(new Vector3(1, 1, farPlane));
    }
    private static void GetPlanes()
    {
        shadowFrustumPlanes[0].planeNormal = shadowCam.transform.right;
        shadowFrustumPlanes[1].planeNormal = shadowCam.transform.up;
        shadowFrustumPlanes[2].planeNormal = shadowCam.transform.forward;
        for (int i = 0; i < 3; ++i)
        {
            AspectInfo info = shadowFrustumPlanes[i];
            float least = float.MaxValue;
            float maximum = float.MinValue;
            Vector3 lessPoint = Vector3.zero;
            Vector3 morePoint = Vector3.zero;
            for(int x = 0; x < 8; ++x)
            {
                float dotValue = Vector3.Dot(info.planeNormal, frustumCorners[x]);
                if(dotValue < least)
                {
                    least = dotValue;
                    lessPoint = frustumCorners[x];
                }
                if(dotValue > maximum)
                {
                    maximum = dotValue;
                    morePoint = frustumCorners[x];
                }
            }
            info.size = (maximum - least) / 2f;
            info.inPlanePoint = lessPoint + info.planeNormal * info.size;
            shadowFrustumPlanes[i] = info;
        }
        shadowFrustumPlanes[2].size = 75;
    }
    private static void SetPlanesToCamera()
    {
        Transform tr = shadowCam.transform;
        for (int i = 0; i < 3; ++i)
        {
            AspectInfo info = shadowFrustumPlanes[i];
            float dist = Vector3.Dot(info.inPlanePoint, info.planeNormal) - Vector3.Dot(tr.position, info.planeNormal);
            tr.position += dist * info.planeNormal;
        }
        shadowCam.orthographicSize = shadowFrustumPlanes[1].size;
        shadowCam.aspect = shadowFrustumPlanes[0].size / shadowFrustumPlanes[1].size;
        shadowCam.nearClipPlane = 0;
        shadowCam.farClipPlane = shadowFrustumPlanes[2].size * 2;
        tr.position -= shadowFrustumPlanes[2].size * shadowFrustumPlanes[2].planeNormal;
    }
    public Camera testCamera;
    private RenderTexture shadowMapTexture;
    private void Awake()
    {
        if (current)
        {
            Debug.LogError("There should only be one Directional light in this Scene!");
            Destroy(this);
            return;
        }
        current = this;
        directionalLight = GetComponent<Light>();
        shadowCam = GetComponent<Camera>();
        shadowCam.enabled = false;
        shadowCam.cullingMask = directionalLight.cullingMask;
        shadowCam.orthographic = true;
        shadowCam.renderingPath = RenderingPath.Forward;
        shadowCam.clearFlags = CameraClearFlags.Nothing;
        shadowMapTexture = new RenderTexture(4096, 4096, 16, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
        shadowMapTexture.filterMode = FilterMode.Point;
        shadowCam.targetTexture = shadowMapTexture;
        shadowCam.SetReplacementShader(Shader.Find(depthShader), "RenderType");
    }

    public void OnDrawShadow()
    {
        SetFrustumCorners(testCamera, testCamera.nearClipPlane, 10);
        GetPlanes();
        SetPlanesToCamera();
        Vector4 shadowcamDir = shadowCam.transform.forward;
        shadowcamDir.w = 0.5f;
        Shader.SetGlobalVector(ShaderIDs._ShadowCamDirection, shadowcamDir);
        Shader.SetGlobalFloat(ShaderIDs._ShadowCamFarClip, shadowCam.farClipPlane);
        Graphics.SetRenderTarget(shadowMapTexture);
        GL.Clear(true, true, Color.white);
        Shader.SetGlobalTexture(ShaderIDs._DirShadowMap, shadowMapTexture);
        Matrix4x4 vp = GL.GetGPUProjectionMatrix(shadowCam.projectionMatrix, false) * shadowCam.worldToCameraMatrix;
        Shader.SetGlobalMatrix(ShaderIDs._ShadowMapVP, vp);
        Shader.SetGlobalVector(ShaderIDs._ShadowCamPos, transform.position);
        var sd = QualitySettings.shadows;
        QualitySettings.shadows = ShadowQuality.Disable;
        shadowCam.Render();
        QualitySettings.shadows = sd;
    }

    private void OnDestroy()
    {
        current = null;
        shadowMapTexture.Release();
        Destroy(shadowMapTexture);
    }
}
