using UnityEditor;
using UnityEngine;

public class CalTransparentAreaByGPU
{
    private static int[] intZeroArray = { 0 };
    private static ComputeShader _computeShader;
    private static ComputeShader computeShader
    {
        get
        {
            if (_computeShader == null)
            {
                _computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Shaders/CalTransparentAreaByGPU.compute");
            }
            return _computeShader;
        }
    }

    [MenuItem("Tools/CalTransparentAreaByGPU")]
    public static void Test()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("Don't support Compute Shader!");
            return;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/test.png");
        float ratio = CalTransparencyAreaByGPU(texture);
        Debug.Log(ratio);
    }

    public static float CalTransparencyAreaByGPU(Texture2D texture)
    {
        int texWith = texture.width, texHeight = texture.height;
        ComputeBuffer resultBuffer = new ComputeBuffer(1, sizeof(int));
        resultBuffer.SetData(intZeroArray);                                     /* 用于清除buffer数据，非常重要 */
        ComputeShader shader = computeShader;
        int handle = shader.FindKernel("CalTransparentAreaByGPU");
        int[] result = new int[1];
        shader.SetTexture(handle, "inputTexture", texture);
        shader.SetBuffer(handle, "outputBuffer", resultBuffer);
        shader.SetInt("width", texWith);
        shader.SetInt("height", texHeight);
        int threadGroupSizeX = Mathf.CeilToInt((float)texWith / 8);
        int threadGroupSizeY = Mathf.CeilToInt((float)texHeight / 8);
        shader.Dispatch(handle, threadGroupSizeX, threadGroupSizeY, 1);
        resultBuffer.GetData(result);
        resultBuffer.Release();
        float ratio = (float)result[0] / (texWith * texHeight);
        return ratio;
    }
}