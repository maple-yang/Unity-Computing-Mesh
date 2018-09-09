using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class ShaderIDs
{
    public static int _Count = Shader.PropertyToID("_Count");
    public static int planes = Shader.PropertyToID("planes");
    public static int objBuffer = Shader.PropertyToID("objBuffer");
    public static int instanceCountBuffer = Shader.PropertyToID("instanceCountBuffer");
    public static int allCountBuffer = Shader.PropertyToID("allCountBuffer");
    public static int Transforms = Shader.PropertyToID("Transforms");
    public static int LAST_VP_MATRIX = Shader.PropertyToID("LAST_VP_MATRIX");
    public static int VertexBuffer = Shader.PropertyToID("VertexBuffer");
    public static int lodGroupBuffer = Shader.PropertyToID("lodGroupBuffer");
    public static int changedBuffer = Shader.PropertyToID("changedBuffer");
    public static int newPositionBuffer = Shader.PropertyToID("newPositionBuffer");
    public static int _CameraPos = Shader.PropertyToID("_CameraPos");
    public static int lastFrameMatrices = Shader.PropertyToID("lastFrameMatrices");
    public static int _CurrentTime = Shader.PropertyToID("_CurrentTime");
    public static int _ShadowCamDirection = Shader.PropertyToID("_ShadowCamDirection");
    public static int _ShadowCamFarClip = Shader.PropertyToID("_ShadowCamFarClip");
    public static int _DirShadowMap = Shader.PropertyToID("_DirShadowMap");
    public static int _CustomShadowMap = Shader.PropertyToID("_CustomShadowMap");
    public static int _InvVP = Shader.PropertyToID("_InvVP");
    public static int _ShadowMapVP = Shader.PropertyToID("_ShadowMapVP");
    public static int _ShadowCamPos = Shader.PropertyToID("_ShadowCamPos");
    public static int _MVPMatrix = Shader.PropertyToID("_MVPMatrix");
    public static int _IndirectIndex = Shader.PropertyToID("_IndirectIndex");
    public static int _ProceduralOffset = Shader.PropertyToID("_ProceduralOffset");
}
