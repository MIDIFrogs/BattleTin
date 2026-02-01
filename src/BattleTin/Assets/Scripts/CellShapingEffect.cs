using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CelShadingEffect : MonoBehaviour
{
    public Shader celShader;
    public Color edgeColor = Color.black;
    [Range(0, 0.1f)] public float edgeThreshold = 0.01f;
    [Range(1, 10)] public int colorBands = 3;
    [Range(0, 1)] public float depthSensitivity = 0.5f;
    [Range(0, 1)] public float normalSensitivity = 0.5f;

    private Material material;

    void OnEnable()
    {
        if (celShader == null)
        {
            celShader = Shader.Find("Custom/CelShadingPostProcess");
        }

        if (celShader != null && celShader.isSupported)
        {
            material = new Material(celShader);
        }
        else
        {
            enabled = false;
            Debug.LogError("Cel Shading shader is not supported");
        }
    }

    void OnDisable()
    {
        if (material != null)
        {
            DestroyImmediate(material);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetColor("_EdgeColor", edgeColor);
        material.SetFloat("_EdgeThreshold", edgeThreshold);
        material.SetFloat("_ColorBands", (float)colorBands);
        material.SetFloat("_DepthSensitivity", depthSensitivity);
        material.SetFloat("_NormalSensitivity", normalSensitivity);

        Graphics.Blit(source, destination, material);
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            // Обновление в редакторе
            Camera cam = GetComponent<Camera>();
            cam.depthTextureMode = DepthTextureMode.DepthNormals;
        }
    }

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.DepthNormals;
    }
}