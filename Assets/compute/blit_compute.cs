using UnityEngine;

public class BlitCompute : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material mat;
    private RenderTexture renderTexture;
    private ComputeBuffer AgentDataBuffer;
    private int numAgents = 1024;

    private float time = 0.0f;

    void Start()
    {
        time = Time.time;
        InitializeRenderTexture();
        InitializeComputeBuffer();
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

    void InitializeComputeBuffer()
    {
        AgentDataBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
        Vector4[] AgentData = new Vector4[numAgents];

        for (int i = 0; i < numAgents; i++)
        {   
            float posX = Random.Range(5.0f, Screen.width-5.0f);
            float posY = Random.Range(0, Screen.height-1.0f);
            float speedX = Random.Range(-1f, 1f);
            float speedY = Random.Range(-1f, 1f);

            AgentData[i] = new Vector4(posX, posY, speedX, speedY);
        }

        AgentDataBuffer.SetData(AgentData);
    }

    void DispatchComputeShader()
    {
        float dt = Time.time - time;
        time = Time.time;
        int kernelHandle = computeShader.FindKernel("CSMain");
        int postProcessKernel = computeShader.FindKernel("ImageProcess");
        
        computeShader.SetFloat("_dt", dt);
        computeShader.SetTexture(kernelHandle, "Result", renderTexture);
        computeShader.SetBuffer(kernelHandle, "AgentData", AgentDataBuffer);
        computeShader.SetInt("_width", renderTexture.width);
        computeShader.SetInt("_height", renderTexture.height);


        int threadGroupsX = Mathf.CeilToInt(numAgents / 1024.0f);

        computeShader.Dispatch(kernelHandle, threadGroupsX, 1, 1);


        int threadGroupsX2 = Mathf.CeilToInt(renderTexture.width / 8.0f);
        int threadGroupsY2 = Mathf.CeilToInt(renderTexture.height / 8.0f);

        computeShader.Dispatch(postProcessKernel, threadGroupsX2, threadGroupsY2, 1);

    }

    void OnDestroy()
    {
        if (renderTexture != null)
            renderTexture.Release();
        if (AgentDataBuffer != null)
            AgentDataBuffer.Release();
    }
}
