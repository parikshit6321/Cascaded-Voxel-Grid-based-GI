using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WorldPositionSecondary : MonoBehaviour
{
    // Shader for writing the world position
    public Shader positionShader = null;
    
    // Render textures for storing the color with direct lighting
    public RenderTexture directTextureDiffuse1;
    public RenderTexture directTextureDiffuse2;
    public RenderTexture directTextureDiffuse3;
    public RenderTexture directTextureDiffuse4;
    public RenderTexture directTextureDiffuse5;
    public RenderTexture directTextureDiffuse6;
    public RenderTexture directTextureDiffuse7;
    public RenderTexture directTextureDiffuse8;
    public RenderTexture directTextureSpecular;

    // Render textures for storing the world position
    public RenderTexture positionTextureDiffuse1;
    public RenderTexture positionTextureDiffuse2;
    public RenderTexture positionTextureDiffuse3;
    public RenderTexture positionTextureDiffuse4;
    public RenderTexture positionTextureDiffuse5;
    public RenderTexture positionTextureDiffuse6;
    public RenderTexture positionTextureDiffuse7;
    public RenderTexture positionTextureDiffuse8;
    public RenderTexture positionTextureSpecular;

    // Array for storing all the light gameobjects
    private Light[] lights;

    // Boundaries for the world-space cascade volumes which will be voxelized
    private int worldVolumeBoundary = 1;

    // Dimensions of voxel grids of respective cascades
    private int voxelVolumeDimensionDiffuse1 = 0;
    private int voxelVolumeDimensionDiffuse2 = 0;
    private int voxelVolumeDimensionDiffuse3 = 0;
    private int voxelVolumeDimensionDiffuse4 = 0;
    private int voxelVolumeDimensionDiffuse5 = 0;
    private int voxelVolumeDimensionDiffuse6 = 0;
    private int voxelVolumeDimensionDiffuse7 = 0;
    private int voxelVolumeDimensionDiffuse8 = 0;
    private int voxelVolumeDimensionSpecular = 0;

    // Array for storing all the light intensities
    private float[] intensities;

    // Current diffuse voxel grid which is being voxelized
    private VXGI.VoxelGrid currentGrid = VXGI.VoxelGrid.DIFFUSE1;

    // Use this for initialization
    public void Initialize()
    {
        worldVolumeBoundary = GameObject.Find("Main Camera").GetComponent<VXGI>().worldVolumeBoundary;

        voxelVolumeDimensionDiffuse1 = GameObject.Find("Main Camera").GetComponent<VXGI>().voxelVolumeDimensionDiffuse1;
        voxelVolumeDimensionDiffuse2 = GameObject.Find("Main Camera").GetComponent<VXGI>().voxelVolumeDimensionDiffuse2;
        voxelVolumeDimensionDiffuse3 = GameObject.Find("Main Camera").GetComponent<VXGI>().voxelVolumeDimensionDiffuse3;
        voxelVolumeDimensionDiffuse4 = GameObject.Find("Main Camera").GetComponent<VXGI>().voxelVolumeDimensionDiffuse4;
        voxelVolumeDimensionDiffuse5 = GameObject.Find("Main Camera").GetComponent<VXGI>().voxelVolumeDimensionDiffuse5;
        voxelVolumeDimensionDiffuse6 = GameObject.Find("Main Camera").GetComponent<VXGI>().voxelVolumeDimensionDiffuse6;
        voxelVolumeDimensionDiffuse7 = GameObject.Find("Main Camera").GetComponent<VXGI>().voxelVolumeDimensionDiffuse7;
        voxelVolumeDimensionDiffuse8 = GameObject.Find("Main Camera").GetComponent<VXGI>().voxelVolumeDimensionDiffuse8;
        voxelVolumeDimensionSpecular = GameObject.Find("Main Camera").GetComponent<VXGI>().voxelVolumeDimensionSpecular;

        directTextureDiffuse1 = new RenderTexture(voxelVolumeDimensionDiffuse1, voxelVolumeDimensionDiffuse1, 32, RenderTextureFormat.ARGBFloat);
        directTextureDiffuse2 = new RenderTexture(voxelVolumeDimensionDiffuse2, voxelVolumeDimensionDiffuse2, 32, RenderTextureFormat.ARGBFloat);
        directTextureDiffuse3 = new RenderTexture(voxelVolumeDimensionDiffuse3, voxelVolumeDimensionDiffuse3, 32, RenderTextureFormat.ARGBFloat);
        directTextureDiffuse4 = new RenderTexture(voxelVolumeDimensionDiffuse4, voxelVolumeDimensionDiffuse4, 32, RenderTextureFormat.ARGBFloat);
        directTextureDiffuse5 = new RenderTexture(voxelVolumeDimensionDiffuse5, voxelVolumeDimensionDiffuse5, 32, RenderTextureFormat.ARGBFloat);
        directTextureDiffuse6 = new RenderTexture(voxelVolumeDimensionDiffuse6, voxelVolumeDimensionDiffuse6, 32, RenderTextureFormat.ARGBFloat);
        directTextureDiffuse7 = new RenderTexture(voxelVolumeDimensionDiffuse8, voxelVolumeDimensionDiffuse7, 32, RenderTextureFormat.ARGBFloat);
        directTextureDiffuse8 = new RenderTexture(voxelVolumeDimensionDiffuse7, voxelVolumeDimensionDiffuse8, 32, RenderTextureFormat.ARGBFloat);
        directTextureSpecular = new RenderTexture(voxelVolumeDimensionSpecular, voxelVolumeDimensionSpecular, 32, RenderTextureFormat.ARGBFloat);

        positionTextureDiffuse1 = new RenderTexture(voxelVolumeDimensionDiffuse1, voxelVolumeDimensionDiffuse1, 32, RenderTextureFormat.ARGBFloat);
        positionTextureDiffuse2 = new RenderTexture(voxelVolumeDimensionDiffuse2, voxelVolumeDimensionDiffuse2, 32, RenderTextureFormat.ARGBFloat);
        positionTextureDiffuse3 = new RenderTexture(voxelVolumeDimensionDiffuse3, voxelVolumeDimensionDiffuse3, 32, RenderTextureFormat.ARGBFloat);
        positionTextureDiffuse4 = new RenderTexture(voxelVolumeDimensionDiffuse4, voxelVolumeDimensionDiffuse4, 32, RenderTextureFormat.ARGBFloat);
        positionTextureDiffuse5 = new RenderTexture(voxelVolumeDimensionDiffuse5, voxelVolumeDimensionDiffuse5, 32, RenderTextureFormat.ARGBFloat);
        positionTextureDiffuse6 = new RenderTexture(voxelVolumeDimensionDiffuse6, voxelVolumeDimensionDiffuse6, 32, RenderTextureFormat.ARGBFloat);
        positionTextureDiffuse7 = new RenderTexture(voxelVolumeDimensionDiffuse7, voxelVolumeDimensionDiffuse7, 32, RenderTextureFormat.ARGBFloat);
        positionTextureDiffuse8 = new RenderTexture(voxelVolumeDimensionDiffuse8, voxelVolumeDimensionDiffuse8, 32, RenderTextureFormat.ARGBFloat);
        positionTextureSpecular = new RenderTexture(voxelVolumeDimensionSpecular, voxelVolumeDimensionSpecular, 32, RenderTextureFormat.ARGBFloat);

        lights = Resources.FindObjectsOfTypeAll<Light>();
        
        InitializeIntensities();

        TurnOnLights();
    }

    // Function to initialize light intensities
    void InitializeIntensities()
    {

        intensities = new float[lights.Length];

        for (int i = 0; i < lights.Length; ++i)
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
    
    // Function to release the dynamically allocated memory
    public void ReleaseMemory()
    {
        directTextureDiffuse1.Release();
        directTextureDiffuse2.Release();
        directTextureDiffuse3.Release();
        directTextureDiffuse4.Release();
        directTextureDiffuse5.Release();
        directTextureDiffuse6.Release();
        directTextureDiffuse7.Release();
        directTextureDiffuse8.Release();
        directTextureSpecular.Release();

        positionTextureDiffuse1.Release();
        positionTextureDiffuse2.Release();
        positionTextureDiffuse3.Release();
        positionTextureDiffuse4.Release();
        positionTextureDiffuse5.Release();
        positionTextureDiffuse6.Release();
        positionTextureDiffuse7.Release();
        positionTextureDiffuse8.Release();
        positionTextureSpecular.Release();
    }

    // Function to get the appropriate direct texture
    public RenderTexture GetDirectTexture(VXGI.VoxelGrid grid)
    {
        if(grid == VXGI.VoxelGrid.DIFFUSE1)
        {
            return directTextureDiffuse1;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE2)
        {
            return directTextureDiffuse2;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE3)
        {
            return directTextureDiffuse3;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE4)
        {
            return directTextureDiffuse4;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE5)
        {
            return directTextureDiffuse5;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE6)
        {
            return directTextureDiffuse6;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE7)
        {
            return directTextureDiffuse7;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE8)
        {
            return directTextureDiffuse8;
        }
        else
        {
            return directTextureSpecular;
        }
    }

    // Function to get the appropriate position texture
    public RenderTexture GetPositionTexture(VXGI.VoxelGrid grid)
    {
        if (grid == VXGI.VoxelGrid.DIFFUSE1)
        {
            return positionTextureDiffuse1;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE2)
        {
            return positionTextureDiffuse2;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE3)
        {
            return positionTextureDiffuse3;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE4)
        {
            return positionTextureDiffuse4;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE5)
        {
            return positionTextureDiffuse5;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE6)
        {
            return positionTextureDiffuse6;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE7)
        {
            return positionTextureDiffuse7;
        }
        else if (grid == VXGI.VoxelGrid.DIFFUSE8)
        {
            return positionTextureDiffuse8;
        }
        else
        {
            return positionTextureSpecular;
        }
    }

    // Function used to render the color, position and normal textures
    public void RenderTextures()
    {
        GetComponent<Camera>().orthographicSize = 10;

        currentGrid = GameObject.Find("Main Camera").GetComponent<VXGI>().currentVoxelGrid;

        // Update light intensities
        InitializeIntensities();
        
        // Render the color texture with direct lighting
        GetComponent<Camera>().targetTexture = directTextureSpecular;
        GetComponent<Camera>().Render();
        
        // Render the world position texture
        GetComponent<Camera>().targetTexture = positionTextureSpecular;
        Shader.SetGlobalInt("_WorldVolumeBoundary", worldVolumeBoundary);
        GetComponent<Camera>().RenderWithShader(positionShader, null);

        // Render to the low resolution render textures
        if(currentGrid == VXGI.VoxelGrid.DIFFUSE1)
        {
            Graphics.Blit(directTextureSpecular, directTextureDiffuse1);
            Graphics.Blit(positionTextureSpecular, positionTextureDiffuse1);
        }
        else if (currentGrid == VXGI.VoxelGrid.DIFFUSE2)
        {
            Graphics.Blit(directTextureSpecular, directTextureDiffuse2);
            Graphics.Blit(positionTextureSpecular, positionTextureDiffuse2);
        }
        else if (currentGrid == VXGI.VoxelGrid.DIFFUSE3)
        {
            Graphics.Blit(directTextureSpecular, directTextureDiffuse3);
            Graphics.Blit(positionTextureSpecular, positionTextureDiffuse3);
        }
        else if (currentGrid == VXGI.VoxelGrid.DIFFUSE4)
        {
            Graphics.Blit(directTextureSpecular, directTextureDiffuse4);
            Graphics.Blit(positionTextureSpecular, positionTextureDiffuse4);
        }
        else if (currentGrid == VXGI.VoxelGrid.DIFFUSE5)
        {
            Graphics.Blit(directTextureSpecular, directTextureDiffuse5);
            Graphics.Blit(positionTextureSpecular, positionTextureDiffuse5);
        }
        else if (currentGrid == VXGI.VoxelGrid.DIFFUSE6)
        {
            Graphics.Blit(directTextureSpecular, directTextureDiffuse6);
            Graphics.Blit(positionTextureSpecular, positionTextureDiffuse6);
        }
        else if (currentGrid == VXGI.VoxelGrid.DIFFUSE7)
        {
            Graphics.Blit(directTextureSpecular, directTextureDiffuse7);
            Graphics.Blit(positionTextureSpecular, positionTextureDiffuse7);
        }
        else
        {
            Graphics.Blit(directTextureSpecular, directTextureDiffuse8);
            Graphics.Blit(positionTextureSpecular, positionTextureDiffuse8);
        }
    }
}