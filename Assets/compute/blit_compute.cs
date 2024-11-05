using UnityEngine;

public class BlitCompute : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material mat;
    private RenderTexture renderTexture;
    private ComputeBuffer AgentDataBuffer;
    private float time = 0.0f;

    void Start()
    {
        time = Time.time;
        InitializeRenderTexture();
        DispatchComputeShader();
        mat.mainTexture = renderTexture;

    }

    void Update()
    {
        DispatchComputeShader();
    }

    void InitializeRenderTexture()
    {
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
    }


    void DispatchComputeShader()
    {
        float dt = Time.time - time;
        time = Time.time;
        int kernelHandle = computeShader.FindKernel("CSMain");

        computeShader.SetFloat("_dt", dt);
        computeShader.SetTexture(kernelHandle, "Result", renderTexture);
        computeShader.SetInt("_width", renderTexture.width);
        computeShader.SetInt("_height", renderTexture.height);
        computeShader.SetFloat("_time", Time.time);

        int threadGroupsX = Mathf.CeilToInt(renderTexture.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(renderTexture.height / 8.0f);

        computeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);

    }

    void OnDestroy()
    {
        if (renderTexture != null)
            renderTexture.Release();
    }
}
