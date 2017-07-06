using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WorldPositionChild : MonoBehaviour
{
    // Shader for writing the world-space position
    public Shader positionShader = null;

    // Shader for writing the color
    public Shader colorShader = null;

    // Shader for writing the normal
    public Shader normalShader = null;

    // Render texture for storing the world-space position
    public RenderTexture positionTexture = null;

    // Render texture for storing the color without lighting
    public RenderTexture colorTexture = null;

    // Render texture for storing the world-space normal
    public RenderTexture normalTexture = null;

    // Downsampling for all the textures
    public int downsample = 1;
    
    // Array for storing all the light gameobjects
    private Light[] lights;

    // Array for storing all the light intensities
    private float[] intensities;

    // Boundary for the world volume which will be voxelized (last cascade)
    private int worldVolumeBoundary = 1;
    
    // Use this for initialization
    public void Initialize()
    {
        worldVolumeBoundary = GameObject.Find("Main Camera").GetComponent<VXGI>().worldVolumeBoundary;

        positionTexture = new RenderTexture(Screen.width / downsample, Screen.height / downsample, 32, RenderTextureFormat.ARGBFloat);
        colorTexture = new RenderTexture(Screen.width / downsample, Screen.height / downsample, 32, RenderTextureFormat.ARGBFloat);
        normalTexture = new RenderTexture(Screen.width / downsample, Screen.height / downsample, 32, RenderTextureFormat.ARGBFloat);
        
        lights = Resources.FindObjectsOfTypeAll<Light>();

        InitializeIntensities();
    }

    // Function to initialize light intensities
    void InitializeIntensities() {

        intensities = new float[lights.Length];

        for(int i = 0; i < lights.Length; ++i)
        {
            intensities[i] = lights[i].intensity;
        }

    }

    // Function to turn the lights on
    void TurnOnLights()
    {
        for (int i = 0; i < lights.Length; ++i)
            lights[i].intensity = intensities[i];

        RenderSettings.ambientIntensity = 0.0f;
    }

    // Function to turn the lights off
    void TurnOffLights()
    {
        for (int i = 0; i < lights.Length; ++i)
            lights[i].intensity = 0.0f;

        RenderSettings.ambientIntensity = 1.0f;
    }

    // Function to release the dynamically allocated memory
    public void ReleaseMemory()
    {
        positionTexture.Release();
        colorTexture.Release();
        normalTexture.Release();
    }

    // Function to render the world position texture
    public void RenderTextures()
    {
        // Update light intensities
        InitializeIntensities();

        // Render position texture
        GetComponent<Camera>().targetTexture = positionTexture;
        Shader.SetGlobalInt("_WorldVolumeBoundary", worldVolumeBoundary);
        GetComponent<Camera>().RenderWithShader(positionShader, null);

        // Render normal texture
        GetComponent<Camera>().targetTexture = normalTexture;
        GetComponent<Camera>().RenderWithShader(normalShader, null);

        // Turn off direct lighting and turn on ambient lighting to get the true colors of the objects on screen
        TurnOffLights();

        // Render color texture
        GetComponent<Camera>().targetTexture = colorTexture;
        GetComponent<Camera>().Render();

        // Toggle lights back to original state
        TurnOnLights();
    }
}