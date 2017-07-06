using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class VXGI : MonoBehaviour {

    // Enum representing the computation to be performed
    public enum Computation
    {
        DIFFUSE,
        SPECULAR,
        DIFFUSE_SPECULAR,
        VOXELIZATION
    }
    
    // Enum representing the method used to perform cone tracing
    public enum ConeTracingMethod
    {
        REALISTIC,
        APPROXIMATE
    }

    // Enum representing the number of samples in the cone tracing step
    public enum NumberOfSamples
    {
        LOW,
        MEDIUM,
        HIGH
    }

    // Enum representing the voxel grid which is currently in use
    public enum VoxelGrid
    {
        DIFFUSE1,
        DIFFUSE2,
        DIFFUSE3,
        DIFFUSE4,
        DIFFUSE5,
        DIFFUSE6,
        DIFFUSE7,
        DIFFUSE8,
        SPECULAR
    }

    // Structure representing an individual voxel element
    public struct Voxel
    {
        public int data;
    }

    [Header("General")]
    // Computation to be performed
    public Computation computation = Computation.DIFFUSE;

    // Method used for the cone tracing pass
    public ConeTracingMethod coneTracingMethod = ConeTracingMethod.REALISTIC;
    
    // Shader used to render the final scene using cone tracing
    public Shader lightingShader = null;
    
    // Compute shader used to filter the unoccupied voxels
    public ComputeShader filteredVoxelizationShader = null;
    
    // Boundary of world volume which will be voxelized in the respective cascades
    public int worldVolumeBoundary = 1;
    
    // Number of iterations for initial voxelization computation for static objects
    public int initialVoxelizationIterations = 10;
    
    // Strength of the direct lighting
    public float directLightingStrength = 0.5f;

    // Strength of the ambient lighting
    public float ambientLightingStrength = 0.1f;

    [Header("Indirect Diffuse Lighting")]
    // Number of samples used in the cone tracing step
    public NumberOfSamples numberOfSamples = NumberOfSamples.HIGH;

    // Dimension of the voxel grid for the diffuse voxel grid
    public int voxelVolumeDimensionDiffuse = 1;

    // Number of cone tracing iterations used in the indirect diffuse lighting step
    [Range(0.0f, 500.0f)]
    public float maximumIterationsDiffuse = 10.0f;

    // Step value for the cone tracing process in the indirect diffuse lighting step
    public float coneStepDiffuse = 0.5f;

    // Step multiplier for the cone tracing step in indirect diffuse illumination
    public float stepMultiplierDiffuse = 1.0f;

    // Angle of the cone in the indirect diffuse lighting step
    [Range(0.0f, 1.0f)]
    public float coneAngleDiffuse = 0.3f;

    // Offset value for the cone tracing process in the indirect diffuse lighting step
    public float coneOffsetDiffuse = 0.1f;

    // Strength of the computed indirect diffuse lighting
    public float indirectDiffuseLightingStrength = 1.0f;
    
    // Downsampling for the indirect diffuse lighting
    public int downsampleDiffuse = 1;

    // Number of blur iterations for the indirect diffuse lighting
    public int blurIterationsDiffuse = 0;

    // Step value for blurring of indirect diffuse lighting
    public float blurStepDiffuse = 1.0f;

    [Header("Indirect Specular Lighting")]
    // Dimension of the voxel grid for the specular voxel grid
    public int voxelVolumeDimensionSpecular = 1;

    // Number of cone tracing iterations used in the indirect specular lighting step
    [Range(0.0f, 500.0f)]
    public float maximumIterationsSpecular = 10;

    // Step value for the cone tracing process in the indirect specular lighting step
    public float coneStepSpecular = 0.5f;

    // Step multiplier for the cone tracing step in indirect specular illumination
    public float stepMultiplierSpecular = 1.0f;
    
    // Offset value for the cone tracing process in the indirect specular lighting step
    public float coneOffsetSpecular = 0.1f;

    // Strength of the computed indirect specular lighting
    public float indirectSpecularLightingStrength = 1.0f;
    
    // Downsampling for the indirect specular lighting
    public int downsampleSpecular = 1;

    // Number of blur iterations for the indirect specular lighting
    public int blurIterationsSpecular = 0;

    // Step value for blurring of indirect specular lighting
    public float blurStepSpecular = 1.0f;

    [HideInInspector]
    // Array of gameobjects used for per object voxelization
    public GameObject[] gameObjects = null;

    // Number of game objects involved in the voxelization process
    private int numberOfGameObjects = 0;

    // Voxel data representation of diffuse voxel grid's first cascade
    private Voxel[] voxelDataDiffuse1;

    // Voxel data representation of diffuse voxel grid's second cascade
    private Voxel[] voxelDataDiffuse2;

    // Voxel data representation of diffuse voxel grid's third cascade
    private Voxel[] voxelDataDiffuse3;

    // Voxel data representation of diffuse voxel grid's fourth cascade
    private Voxel[] voxelDataDiffuse4;

    // Voxel data representation of diffuse voxel grid's fifth cascade
    private Voxel[] voxelDataDiffuse5;

    // Voxel data representation of diffuse voxel grid's sixth cascade
    private Voxel[] voxelDataDiffuse6;

    // Voxel data representation of diffuse voxel grid's seventh cascade
    private Voxel[] voxelDataDiffuse7;

    // Voxel data representation of diffuse voxel grid's eigth cascade
    private Voxel[] voxelDataDiffuse8;

    // Voxel data representation of specular voxel grid
    private Voxel[] voxelDataSpecular;
    
    // Structured buffer to hold the data in the diffuse voxel grid's first cascade
    private ComputeBuffer voxelVolumeBufferDiffuse1 = null;

    // Structured buffer to hold the data in the diffuse voxel grid's second cascade
    private ComputeBuffer voxelVolumeBufferDiffuse2 = null;

    // Structured buffer to hold the data in the diffuse voxel grid's third cascade
    private ComputeBuffer voxelVolumeBufferDiffuse3 = null;

    // Structured buffer to hold the data in the diffuse voxel grid's fourth cascade
    private ComputeBuffer voxelVolumeBufferDiffuse4 = null;

    // Structured buffer to hold the data in the diffuse voxel grid's fifth cascade
    private ComputeBuffer voxelVolumeBufferDiffuse5 = null;

    // Structured buffer to hold the data in the diffuse voxel grid's sixth cascade
    private ComputeBuffer voxelVolumeBufferDiffuse6 = null;

    // Structured buffer to hold the data in the diffuse voxel grid's seventh cascade
    private ComputeBuffer voxelVolumeBufferDiffuse7 = null;

    // Structured buffer to hold the data in the diffuse voxel grid's eigth cascade
    private ComputeBuffer voxelVolumeBufferDiffuse8 = null;

    // Structured buffer to hold the data in the specular voxel grid
    private ComputeBuffer voxelVolumeBufferSpecular = null;
    
    // Material for the given shader
    private Material material = null;

    // Current timestamp for the voxel information
    private int currentTimestamp = 0;
    
    // Camera references
    private Camera[] cameras = null;
    private Camera frontCamera = null;
    private Camera backCamera = null;
    private Camera leftCamera = null;
    private Camera rightCamera = null;
    private Camera topCamera = null;
    private Camera bottomCamera = null;
    private Camera childCamera = null;

    // Voxel volume dimensions of the diffuse voxel grid cascades
    [HideInInspector]
    public int voxelVolumeDimensionDiffuse1 = 1;
    [HideInInspector]
    public int voxelVolumeDimensionDiffuse2 = 1;
    [HideInInspector]
    public int voxelVolumeDimensionDiffuse3 = 1;
    [HideInInspector]
    public int voxelVolumeDimensionDiffuse4 = 1;
    [HideInInspector]
    public int voxelVolumeDimensionDiffuse5 = 1;
    [HideInInspector]
    public int voxelVolumeDimensionDiffuse6 = 1;
    [HideInInspector]
    public int voxelVolumeDimensionDiffuse7 = 1;
    [HideInInspector]
    public int voxelVolumeDimensionDiffuse8 = 1;

    [HideInInspector]
    // The voxel grid which is currently in use
    public VoxelGrid currentVoxelGrid = VoxelGrid.DIFFUSE1;

    // Use this for initialization
    void Awake () {
        
        // Create a material for the given shader
        if (lightingShader != null)
        {
            material = new Material(lightingShader);
        }

        // Initialize the voxel grid data
        InitializeVoxelGrid();

        // Initialize the camera references
        InitializeCameras();
        
        // Initialize the game objects array
        InitializeGameObjectsArray();

        // Voxelize the scene using high quality voxelization
        for (int i = 0; i < initialVoxelizationIterations; ++i)
        {
            currentTimestamp = 255;
            VoxelizePerObjectInitialization();
        }
    }
	
    // Function to initialize the voxel grid data
    void InitializeVoxelGrid() {

        // Ensure that the voxel volume dimension of specular grid is greater than or equal to the diffuse grid
        if (voxelVolumeDimensionDiffuse > voxelVolumeDimensionSpecular)
            voxelVolumeDimensionDiffuse = voxelVolumeDimensionSpecular;

        // Initialize the cascade dimensions
        voxelVolumeDimensionDiffuse1 = voxelVolumeDimensionDiffuse;
        voxelVolumeDimensionDiffuse2 = voxelVolumeDimensionDiffuse / 2;
        voxelVolumeDimensionDiffuse3 = voxelVolumeDimensionDiffuse / 4;
        voxelVolumeDimensionDiffuse4 = voxelVolumeDimensionDiffuse / 8;
        voxelVolumeDimensionDiffuse5 = voxelVolumeDimensionDiffuse / 16;
        voxelVolumeDimensionDiffuse6 = voxelVolumeDimensionDiffuse / 32;
        voxelVolumeDimensionDiffuse7 = voxelVolumeDimensionDiffuse / 64;
        voxelVolumeDimensionDiffuse8 = voxelVolumeDimensionDiffuse / 128;

        // Diffuse voxel grid cascades
        // First cascade
        voxelDataDiffuse1 = new Voxel[voxelVolumeDimensionDiffuse1 * voxelVolumeDimensionDiffuse1 * voxelVolumeDimensionDiffuse1];

        for (int i = 0; i < voxelVolumeDimensionDiffuse1 * voxelVolumeDimensionDiffuse1 * voxelVolumeDimensionDiffuse1; ++i)
        {
            voxelDataDiffuse1[i].data = 0;
        }

        voxelVolumeBufferDiffuse1 = new ComputeBuffer(voxelDataDiffuse1.Length, 4);
        voxelVolumeBufferDiffuse1.SetData(voxelDataDiffuse1);

        // Second cascade
        voxelDataDiffuse2 = new Voxel[voxelVolumeDimensionDiffuse2 * voxelVolumeDimensionDiffuse2 * voxelVolumeDimensionDiffuse2];

        for (int i = 0; i < voxelVolumeDimensionDiffuse2 * voxelVolumeDimensionDiffuse2 * voxelVolumeDimensionDiffuse2; ++i)
        {
            voxelDataDiffuse2[i].data = 0;
        }

        voxelVolumeBufferDiffuse2 = new ComputeBuffer(voxelDataDiffuse2.Length, 4);
        voxelVolumeBufferDiffuse2.SetData(voxelDataDiffuse2);

        // Third cascade
        voxelDataDiffuse3 = new Voxel[voxelVolumeDimensionDiffuse3 * voxelVolumeDimensionDiffuse3 * voxelVolumeDimensionDiffuse3];

        for (int i = 0; i < voxelVolumeDimensionDiffuse3 * voxelVolumeDimensionDiffuse3 * voxelVolumeDimensionDiffuse3; ++i)
        {
            voxelDataDiffuse3[i].data = 0;
        }

        voxelVolumeBufferDiffuse3 = new ComputeBuffer(voxelDataDiffuse3.Length, 4);
        voxelVolumeBufferDiffuse3.SetData(voxelDataDiffuse3);

        // Fourth cascade
        voxelDataDiffuse4 = new Voxel[voxelVolumeDimensionDiffuse4 * voxelVolumeDimensionDiffuse4 * voxelVolumeDimensionDiffuse4];

        for (int i = 0; i < voxelVolumeDimensionDiffuse4 * voxelVolumeDimensionDiffuse4 * voxelVolumeDimensionDiffuse4; ++i)
        {
            voxelDataDiffuse4[i].data = 0;
        }

        voxelVolumeBufferDiffuse4 = new ComputeBuffer(voxelDataDiffuse4.Length, 4);
        voxelVolumeBufferDiffuse4.SetData(voxelDataDiffuse4);

        // Fifth cascade
        voxelDataDiffuse5 = new Voxel[voxelVolumeDimensionDiffuse5 * voxelVolumeDimensionDiffuse5 * voxelVolumeDimensionDiffuse5];

        for (int i = 0; i < voxelVolumeDimensionDiffuse5 * voxelVolumeDimensionDiffuse5 * voxelVolumeDimensionDiffuse5; ++i)
        {
            voxelDataDiffuse5[i].data = 0;
        }

        voxelVolumeBufferDiffuse5 = new ComputeBuffer(voxelDataDiffuse5.Length, 4);
        voxelVolumeBufferDiffuse5.SetData(voxelDataDiffuse5);

        // Sixth cascade
        voxelDataDiffuse6 = new Voxel[voxelVolumeDimensionDiffuse6 * voxelVolumeDimensionDiffuse6 * voxelVolumeDimensionDiffuse6];

        for (int i = 0; i < voxelVolumeDimensionDiffuse6 * voxelVolumeDimensionDiffuse6 * voxelVolumeDimensionDiffuse6; ++i)
        {
            voxelDataDiffuse6[i].data = 0;
        }

        voxelVolumeBufferDiffuse6 = new ComputeBuffer(voxelDataDiffuse6.Length, 4);
        voxelVolumeBufferDiffuse6.SetData(voxelDataDiffuse6);

        // Seventh cascade
        voxelDataDiffuse7 = new Voxel[voxelVolumeDimensionDiffuse7 * voxelVolumeDimensionDiffuse7 * voxelVolumeDimensionDiffuse7];

        for (int i = 0; i < voxelVolumeDimensionDiffuse7 * voxelVolumeDimensionDiffuse7 * voxelVolumeDimensionDiffuse7; ++i)
        {
            voxelDataDiffuse7[i].data = 0;
        }

        voxelVolumeBufferDiffuse7 = new ComputeBuffer(voxelDataDiffuse7.Length, 4);
        voxelVolumeBufferDiffuse7.SetData(voxelDataDiffuse7);

        // Eigth cascade
        voxelDataDiffuse8 = new Voxel[voxelVolumeDimensionDiffuse8 * voxelVolumeDimensionDiffuse8 * voxelVolumeDimensionDiffuse8];

        for (int i = 0; i < voxelVolumeDimensionDiffuse8 * voxelVolumeDimensionDiffuse8 * voxelVolumeDimensionDiffuse8; ++i)
        {
            voxelDataDiffuse8[i].data = 0;
        }

        voxelVolumeBufferDiffuse8 = new ComputeBuffer(voxelDataDiffuse8.Length, 4);
        voxelVolumeBufferDiffuse8.SetData(voxelDataDiffuse8);

        // Specular voxel grid
        voxelDataSpecular = new Voxel[voxelVolumeDimensionSpecular * voxelVolumeDimensionSpecular * voxelVolumeDimensionSpecular];

        for (int i = 0; i < voxelVolumeDimensionSpecular * voxelVolumeDimensionSpecular * voxelVolumeDimensionSpecular; ++i)
        {
            voxelDataSpecular[i].data = 0;
        }

        voxelVolumeBufferSpecular = new ComputeBuffer(voxelDataSpecular.Length, 4);
        voxelVolumeBufferSpecular.SetData(voxelDataSpecular);
        
    }

    // Function to initialize the camera references
    void InitializeCameras() {

        cameras = Resources.FindObjectsOfTypeAll<Camera>();

        for (int i = 0; i < cameras.Length; ++i)
        {
            if (cameras[i].name.Equals("Front Camera"))
            {
                frontCamera = cameras[i];
                frontCamera.GetComponent<WorldPositionSecondary>().Initialize();
            }
            else if (cameras[i].name.Equals("Back Camera"))
            {
                backCamera = cameras[i];
                backCamera.GetComponent<WorldPositionSecondary>().Initialize();
            }
            else if (cameras[i].name.Equals("Left Camera"))
            {
                leftCamera = cameras[i];
                leftCamera.GetComponent<WorldPositionSecondary>().Initialize();
            }
            else if (cameras[i].name.Equals("Right Camera"))
            {
                rightCamera = cameras[i];
                rightCamera.GetComponent<WorldPositionSecondary>().Initialize();
            }
            else if (cameras[i].name.Equals("Top Camera"))
            {
                topCamera = cameras[i];
                topCamera.GetComponent<WorldPositionSecondary>().Initialize();
            }
            else if (cameras[i].name.Equals("Bottom Camera"))
            {
                bottomCamera = cameras[i];
                bottomCamera.GetComponent<WorldPositionSecondary>().Initialize();
            }
            else if (cameras[i].name.Equals("Child Camera"))
            {
                childCamera = cameras[i];
                childCamera.GetComponent<WorldPositionChild>().Initialize();
            }
        }
    }
    
    // Function to initialize the game objects array
    void InitializeGameObjectsArray() {

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        gameObjects = new GameObject[objects.Length];
        
        for(int i = 0; i < gameObjects.Length; ++i)
        {
            // If the game object is neither a camera nor a light
            if(objects[i].GetComponent<Camera>() == null && objects[i].GetComponent<Light>() == null && objects[i].tag.Equals("Voxelize"))
            {
                gameObjects[numberOfGameObjects++] = objects[i];
            }
        }

    }

    // Function which is called when the gameobject is destroyed
    void OnDestroy() {

        //// Release the memory from the depth script of the secondary cameras
        //frontCamera.GetComponent<WorldPositionSecondary>().ReleaseMemory();
        //backCamera.GetComponent<WorldPositionSecondary>().ReleaseMemory();
        //leftCamera.GetComponent<WorldPositionSecondary>().ReleaseMemory();
        //rightCamera.GetComponent<WorldPositionSecondary>().ReleaseMemory();
        //topCamera.GetComponent<WorldPositionSecondary>().ReleaseMemory();
        //bottomCamera.GetComponent<WorldPositionSecondary>().ReleaseMemory();

        //// Release the memory from the position script of the child camera
        //childCamera.GetComponent<WorldPositionChild>().ReleaseMemory();
    }
    
    // Function which voxelizes the scene and stores the grid in the voxel buffer
    void Voxelize(VoxelGrid grid) {
        
        // Kernel index for the entry point in compute shader
        int kernelHandle = filteredVoxelizationShader.FindKernel("FilteredVoxelizationMain");

        int currentVoxelVolumeDimension = 1;

        if(grid == VoxelGrid.DIFFUSE1)
        {
            filteredVoxelizationShader.SetBuffer(kernelHandle, "_VoxelVolumeBuffer", voxelVolumeBufferDiffuse1);
            filteredVoxelizationShader.SetInt("_VoxelVolumeDimension", voxelVolumeDimensionDiffuse1);
            currentVoxelVolumeDimension = voxelVolumeDimensionDiffuse1;
        }
        else if (grid == VoxelGrid.DIFFUSE2)
        {
            filteredVoxelizationShader.SetBuffer(kernelHandle, "_VoxelVolumeBuffer", voxelVolumeBufferDiffuse2);
            filteredVoxelizationShader.SetInt("_VoxelVolumeDimension", voxelVolumeDimensionDiffuse2);
            currentVoxelVolumeDimension = voxelVolumeDimensionDiffuse2;
        }
        else if (grid == VoxelGrid.DIFFUSE3)
        {
            filteredVoxelizationShader.SetBuffer(kernelHandle, "_VoxelVolumeBuffer", voxelVolumeBufferDiffuse3);
            filteredVoxelizationShader.SetInt("_VoxelVolumeDimension", voxelVolumeDimensionDiffuse3);
            currentVoxelVolumeDimension = voxelVolumeDimensionDiffuse3;
        }
        else if (grid == VoxelGrid.DIFFUSE4)
        {
            filteredVoxelizationShader.SetBuffer(kernelHandle, "_VoxelVolumeBuffer", voxelVolumeBufferDiffuse4);
            filteredVoxelizationShader.SetInt("_VoxelVolumeDimension", voxelVolumeDimensionDiffuse4);
            currentVoxelVolumeDimension = voxelVolumeDimensionDiffuse4;
        }
        else if (grid == VoxelGrid.DIFFUSE5)
        {
            filteredVoxelizationShader.SetBuffer(kernelHandle, "_VoxelVolumeBuffer", voxelVolumeBufferDiffuse5);
            filteredVoxelizationShader.SetInt("_VoxelVolumeDimension", voxelVolumeDimensionDiffuse5);
            currentVoxelVolumeDimension = voxelVolumeDimensionDiffuse5;
        }
        else if (grid == VoxelGrid.DIFFUSE6)
        {
            filteredVoxelizationShader.SetBuffer(kernelHandle, "_VoxelVolumeBuffer", voxelVolumeBufferDiffuse6);
            filteredVoxelizationShader.SetInt("_VoxelVolumeDimension", voxelVolumeDimensionDiffuse6);
            currentVoxelVolumeDimension = voxelVolumeDimensionDiffuse6;
        }
        else if (grid == VoxelGrid.DIFFUSE7)
        {
            filteredVoxelizationShader.SetBuffer(kernelHandle, "_VoxelVolumeBuffer", voxelVolumeBufferDiffuse7);
            filteredVoxelizationShader.SetInt("_VoxelVolumeDimension", voxelVolumeDimensionDiffuse8);
            currentVoxelVolumeDimension = voxelVolumeDimensionDiffuse7;
        }
        else if (grid == VoxelGrid.DIFFUSE8)
        {
            filteredVoxelizationShader.SetBuffer(kernelHandle, "_VoxelVolumeBuffer", voxelVolumeBufferDiffuse8);
            filteredVoxelizationShader.SetInt("_VoxelVolumeDimension", voxelVolumeDimensionDiffuse8);
            currentVoxelVolumeDimension = voxelVolumeDimensionDiffuse8;
        }
        else
        {
            filteredVoxelizationShader.SetBuffer(kernelHandle, "_VoxelVolumeBuffer", voxelVolumeBufferSpecular);
            filteredVoxelizationShader.SetInt("_VoxelVolumeDimension", voxelVolumeDimensionSpecular);
            currentVoxelVolumeDimension = voxelVolumeDimensionSpecular;
        }

        filteredVoxelizationShader.SetInt("_CurrentTimestamp", currentTimestamp);
        
        // 1st pass
        // Render the color and position textures for the camera
        frontCamera.GetComponent<WorldPositionSecondary>().RenderTextures();
        
        filteredVoxelizationShader.SetTexture(kernelHandle, "_DirectLightingColorTexture", frontCamera.GetComponent<WorldPositionSecondary>().GetDirectTexture(grid));
        filteredVoxelizationShader.SetTexture(kernelHandle, "_PositionTexture", frontCamera.GetComponent<WorldPositionSecondary>().GetPositionTexture(grid));

        filteredVoxelizationShader.Dispatch(kernelHandle, currentVoxelVolumeDimension, currentVoxelVolumeDimension, 1);

        // 2nd pass
        // Render the color and position textures for the camera
        backCamera.GetComponent<WorldPositionSecondary>().RenderTextures();

        filteredVoxelizationShader.SetTexture(kernelHandle, "_DirectLightingColorTexture", backCamera.GetComponent<WorldPositionSecondary>().GetDirectTexture(grid));
        filteredVoxelizationShader.SetTexture(kernelHandle, "_PositionTexture", backCamera.GetComponent<WorldPositionSecondary>().GetPositionTexture(grid));

        filteredVoxelizationShader.Dispatch(kernelHandle, currentVoxelVolumeDimension, currentVoxelVolumeDimension, 1);

        // 3rd pass
        // Render the color and position textures for the camera
        leftCamera.GetComponent<WorldPositionSecondary>().RenderTextures();

        filteredVoxelizationShader.SetTexture(kernelHandle, "_DirectLightingColorTexture", leftCamera.GetComponent<WorldPositionSecondary>().GetDirectTexture(grid));
        filteredVoxelizationShader.SetTexture(kernelHandle, "_PositionTexture", leftCamera.GetComponent<WorldPositionSecondary>().GetPositionTexture(grid));

        filteredVoxelizationShader.Dispatch(kernelHandle, currentVoxelVolumeDimension, currentVoxelVolumeDimension, 1);

        // 4th pass
        // Render the color and position textures for the camera
        rightCamera.GetComponent<WorldPositionSecondary>().RenderTextures();

        filteredVoxelizationShader.SetTexture(kernelHandle, "_DirectLightingColorTexture", rightCamera.GetComponent<WorldPositionSecondary>().GetDirectTexture(grid));
        filteredVoxelizationShader.SetTexture(kernelHandle, "_PositionTexture", rightCamera.GetComponent<WorldPositionSecondary>().GetPositionTexture(grid));

        filteredVoxelizationShader.Dispatch(kernelHandle, currentVoxelVolumeDimension, currentVoxelVolumeDimension, 1);

        // 5th pass
        // Render the color and position textures for the camera
        topCamera.GetComponent<WorldPositionSecondary>().RenderTextures();

        filteredVoxelizationShader.SetTexture(kernelHandle, "_DirectLightingColorTexture", topCamera.GetComponent<WorldPositionSecondary>().GetDirectTexture(grid));
        filteredVoxelizationShader.SetTexture(kernelHandle, "_PositionTexture", topCamera.GetComponent<WorldPositionSecondary>().GetPositionTexture(grid));

        filteredVoxelizationShader.Dispatch(kernelHandle, currentVoxelVolumeDimension, currentVoxelVolumeDimension, 1);

        // 6th pass
        // Render the color and position textures for the camera
        bottomCamera.GetComponent<WorldPositionSecondary>().RenderTextures();

        filteredVoxelizationShader.SetTexture(kernelHandle, "_DirectLightingColorTexture", bottomCamera.GetComponent<WorldPositionSecondary>().GetDirectTexture(grid));
        filteredVoxelizationShader.SetTexture(kernelHandle, "_PositionTexture", bottomCamera.GetComponent<WorldPositionSecondary>().GetPositionTexture(grid));

        filteredVoxelizationShader.Dispatch(kernelHandle, currentVoxelVolumeDimension, currentVoxelVolumeDimension, 1);
        
    }

    // Function used for per object voxelization during voxel grid initialization
    void VoxelizePerObjectInitialization()
    {

        // Deactivate all the gameobjects
        for (int i = 0; i < numberOfGameObjects; ++i)
        {
            gameObjects[i].SetActive(false);
        }

        // Activate the gameobjects one by one and voxelize them
        for (int i = 0; i < numberOfGameObjects; ++i)
        {
            gameObjects[i].SetActive(true);

            Voxelize(VoxelGrid.DIFFUSE1);
            Voxelize(VoxelGrid.DIFFUSE2);
            Voxelize(VoxelGrid.DIFFUSE3);
            Voxelize(VoxelGrid.DIFFUSE4);
            Voxelize(VoxelGrid.DIFFUSE5);
            Voxelize(VoxelGrid.DIFFUSE6);
            Voxelize(VoxelGrid.DIFFUSE7);
            Voxelize(VoxelGrid.DIFFUSE8);
            Voxelize(VoxelGrid.SPECULAR);

            gameObjects[i].SetActive(false);
        }

        // Activate all the gameobjects
        for (int i = 0; i < numberOfGameObjects; ++i)
        {
            gameObjects[i].SetActive(true);
        }
    }

    // Function used for per object voxelization
    void VoxelizePerObject() {

        // Deactivate all the gameobjects
        for (int i = 0; i < numberOfGameObjects; ++i)
        {
            gameObjects[i].SetActive(false);
        }

        // Activate the gameobjects one by one and voxelize them
        for (int i = 0; i < numberOfGameObjects; ++i)
        {
            // Only voxelize the game objects which are not static
            if(!gameObjects[i].isStatic)
            {
                gameObjects[i].SetActive(true);

                Voxelize(NextCascade());
                Voxelize(VoxelGrid.SPECULAR);

                gameObjects[i].SetActive(false);
            }
        }

        // Activate all the gameobjects
        for(int i = 0; i < numberOfGameObjects; ++i)
        {
            gameObjects[i].SetActive(true);
        }
    }

    // Function to return the next cascade which will be voxelized
    VoxelGrid NextCascade() {

        if (currentVoxelGrid == VoxelGrid.DIFFUSE1)
            currentVoxelGrid = VoxelGrid.DIFFUSE2;
        else if (currentVoxelGrid == VoxelGrid.DIFFUSE2)
            currentVoxelGrid = VoxelGrid.DIFFUSE3;
        else if (currentVoxelGrid == VoxelGrid.DIFFUSE3)
            currentVoxelGrid = VoxelGrid.DIFFUSE4;
        else if (currentVoxelGrid == VoxelGrid.DIFFUSE4)
            currentVoxelGrid = VoxelGrid.DIFFUSE5;
        else if (currentVoxelGrid == VoxelGrid.DIFFUSE5)
            currentVoxelGrid = VoxelGrid.DIFFUSE6;
        else if (currentVoxelGrid == VoxelGrid.DIFFUSE6)
            currentVoxelGrid = VoxelGrid.DIFFUSE7;
        else if (currentVoxelGrid == VoxelGrid.DIFFUSE7)
            currentVoxelGrid = VoxelGrid.DIFFUSE8;
        else
        {
            currentVoxelGrid = VoxelGrid.DIFFUSE1;

            // Increment the timestamp
            ++currentTimestamp;
            currentTimestamp = currentTimestamp % 100;
        }

        return currentVoxelGrid;
    }

	// Function called after rendering the current image
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
        
        
        
        // Filtered per object voxelization
        VoxelizePerObject();
        
        // Render the current camera's world position texture
        childCamera.GetComponent<WorldPositionChild>().RenderTextures();

        // Set the general shader properties
        material.SetBuffer("_VoxelVolumeBufferDiffuse1", voxelVolumeBufferDiffuse1);
        material.SetBuffer("_VoxelVolumeBufferDiffuse2", voxelVolumeBufferDiffuse2);
        material.SetBuffer("_VoxelVolumeBufferDiffuse3", voxelVolumeBufferDiffuse3);
        material.SetBuffer("_VoxelVolumeBufferDiffuse4", voxelVolumeBufferDiffuse4);
        material.SetBuffer("_VoxelVolumeBufferDiffuse5", voxelVolumeBufferDiffuse5);
        material.SetBuffer("_VoxelVolumeBufferDiffuse6", voxelVolumeBufferDiffuse6);
        material.SetBuffer("_VoxelVolumeBufferDiffuse7", voxelVolumeBufferDiffuse7);
        material.SetBuffer("_VoxelVolumeBufferDiffuse8", voxelVolumeBufferDiffuse8);
        material.SetBuffer("_VoxelVolumeBufferSpecular", voxelVolumeBufferSpecular);
        material.SetInt("_VoxelVolumeDimensionDiffuse1", voxelVolumeDimensionDiffuse1);
        material.SetInt("_VoxelVolumeDimensionDiffuse2", voxelVolumeDimensionDiffuse2);
        material.SetInt("_VoxelVolumeDimensionDiffuse3", voxelVolumeDimensionDiffuse3);
        material.SetInt("_VoxelVolumeDimensionDiffuse4", voxelVolumeDimensionDiffuse4);
        material.SetInt("_VoxelVolumeDimensionDiffuse5", voxelVolumeDimensionDiffuse5);
        material.SetInt("_VoxelVolumeDimensionDiffuse6", voxelVolumeDimensionDiffuse6);
        material.SetInt("_VoxelVolumeDimensionDiffuse7", voxelVolumeDimensionDiffuse7);
        material.SetInt("_VoxelVolumeDimensionDiffuse8", voxelVolumeDimensionDiffuse8);
        material.SetInt("_VoxelVolumeDimensionSpecular", voxelVolumeDimensionSpecular);
        material.SetInt("_WorldVolumeBoundary", worldVolumeBoundary);
        material.SetTexture("_PositionTexture", childCamera.GetComponent<WorldPositionChild>().positionTexture);
        material.SetTexture("_ColorTexture", childCamera.GetComponent<WorldPositionChild>().colorTexture);
        material.SetTexture("_NormalTexture", childCamera.GetComponent<WorldPositionChild>().normalTexture);
        material.SetFloat("_DirectStrength", directLightingStrength);
        material.SetFloat("_AmbientLightingStrength", ambientLightingStrength);
        material.SetInt("_CurrentTimestamp", currentTimestamp);
        
        // Indirect diffuse lighting
        if(computation == Computation.DIFFUSE)
        {
            if(numberOfSamples == NumberOfSamples.LOW)
            {
                material.EnableKeyword("LOW_SAMPLES");
                material.DisableKeyword("MEDIUM_SAMPLES");
                material.DisableKeyword("HIGH_SAMPLES");
            }
            else if (numberOfSamples == NumberOfSamples.MEDIUM)
            {
                material.DisableKeyword("LOW_SAMPLES");
                material.EnableKeyword("MEDIUM_SAMPLES");
                material.DisableKeyword("HIGH_SAMPLES");
            }
            else if (numberOfSamples == NumberOfSamples.HIGH)
            {
                material.DisableKeyword("LOW_SAMPLES");
                material.DisableKeyword("MEDIUM_SAMPLES");
                material.EnableKeyword("HIGH_SAMPLES");
            }

            if(coneTracingMethod == ConeTracingMethod.APPROXIMATE)
            {
                material.EnableKeyword("APPROXIMATE_CONE_TRACE");
                material.DisableKeyword("REALISTIC_CONE_TRACE");
            }
            else
            {
                material.DisableKeyword("APPROXIMATE_CONE_TRACE");
                material.EnableKeyword("REALISTIC_CONE_TRACE");
            }

            RenderTexture indirectDiffuse = RenderTexture.GetTemporary(source.width / downsampleDiffuse, source.height / downsampleDiffuse);
            RenderTexture indirectDiffuseTemp = RenderTexture.GetTemporary(source.width / downsampleDiffuse, source.height / downsampleDiffuse);

            material.SetFloat("_MaximumIterations", maximumIterationsDiffuse);
            material.SetFloat("_ConeStep", coneStepDiffuse);
            material.SetFloat("_StepMultiplier", stepMultiplierDiffuse);
            material.SetFloat("_ConeAngle", coneAngleDiffuse);
            material.SetFloat("_ConeOffset", coneOffsetDiffuse);

            Graphics.Blit(source, indirectDiffuse, material, 0);
            
            material.SetFloat("_BlurStep", blurStepDiffuse);

            for (int i = 0; i < blurIterationsDiffuse; ++i)
            {
                Graphics.Blit(indirectDiffuse, indirectDiffuseTemp, material, 5);
                Graphics.Blit(indirectDiffuseTemp, indirectDiffuse, material, 6);
            }

            material.SetFloat("_IndirectDiffuseStrength", indirectDiffuseLightingStrength);
            material.SetTexture("_IndirectDiffuse", indirectDiffuse);

            Graphics.Blit(source, destination, material, 1);

            RenderTexture.ReleaseTemporary(indirectDiffuse);
            RenderTexture.ReleaseTemporary(indirectDiffuseTemp);
        }
        // Indirect specular lighting
        else if(computation == Computation.SPECULAR)
        {
            RenderTexture indirectSpecular = RenderTexture.GetTemporary(source.width / downsampleSpecular, source.height / downsampleSpecular);
            RenderTexture indirectSpecularTemp = RenderTexture.GetTemporary(source.width / downsampleSpecular, source.height / downsampleSpecular);

            material.SetFloat("_MaximumIterations", maximumIterationsSpecular);
            material.SetFloat("_ConeStep", coneStepSpecular);
            material.SetFloat("_StepMultiplier", stepMultiplierSpecular);
            material.SetFloat("_ConeOffset", coneOffsetSpecular);

            Graphics.Blit(source, indirectSpecular, material, 2);
            
            material.SetFloat("_BlurStep", blurStepSpecular);

            for(int i = 0; i < blurIterationsSpecular; ++i)
            {
                Graphics.Blit(indirectSpecular, indirectSpecularTemp, material, 5);
                Graphics.Blit(indirectSpecularTemp, indirectSpecular, material, 6);
            }

            material.SetFloat("_IndirectSpecularStrength", indirectSpecularLightingStrength);
            material.SetTexture("_IndirectSpecular", indirectSpecular);

            Graphics.Blit(source, destination, material, 3);

            RenderTexture.ReleaseTemporary(indirectSpecular);
            RenderTexture.ReleaseTemporary(indirectSpecularTemp);
        }
        // Complete indirect illumination
        else if(computation == Computation.DIFFUSE_SPECULAR)
        {
            if (numberOfSamples == NumberOfSamples.LOW)
            {
                material.EnableKeyword("LOW_SAMPLES");
                material.DisableKeyword("MEDIUM_SAMPLES");
                material.DisableKeyword("HIGH_SAMPLES");
            }
            else if (numberOfSamples == NumberOfSamples.MEDIUM)
            {
                material.DisableKeyword("LOW_SAMPLES");
                material.EnableKeyword("MEDIUM_SAMPLES");
                material.DisableKeyword("HIGH_SAMPLES");
            }
            else if (numberOfSamples == NumberOfSamples.HIGH)
            {
                material.DisableKeyword("LOW_SAMPLES");
                material.DisableKeyword("MEDIUM_SAMPLES");
                material.EnableKeyword("HIGH_SAMPLES");
            }

            if (coneTracingMethod == ConeTracingMethod.APPROXIMATE)
            {
                material.EnableKeyword("APPROXIMATE_CONE_TRACE");
                material.DisableKeyword("REALISTIC_CONE_TRACE");
            }
            else
            {
                material.DisableKeyword("APPROXIMATE_CONE_TRACE");
                material.EnableKeyword("REALISTIC_CONE_TRACE");
            }

            RenderTexture indirectDiffuse = RenderTexture.GetTemporary(source.width / downsampleDiffuse, source.height / downsampleDiffuse);
            RenderTexture indirectDiffuseTemp = RenderTexture.GetTemporary(source.width / downsampleDiffuse, source.height / downsampleDiffuse);

            RenderTexture indirectSpecular = RenderTexture.GetTemporary(source.width / downsampleSpecular, source.height / downsampleSpecular);
            RenderTexture indirectSpecularTemp = RenderTexture.GetTemporary(source.width / downsampleSpecular, source.height / downsampleSpecular);

            material.SetFloat("_MaximumIterations", maximumIterationsDiffuse);
            material.SetFloat("_ConeStep", coneStepDiffuse);
            material.SetFloat("_StepMultiplier", stepMultiplierDiffuse);
            material.SetFloat("_ConeAngle", coneAngleDiffuse);
            material.SetFloat("_ConeOffset", coneOffsetDiffuse);

            Graphics.Blit(source, indirectDiffuse, material, 0);
            
            material.SetFloat("_BlurStep", blurStepDiffuse);

            for (int i = 0; i < blurIterationsDiffuse; ++i)
            {
                Graphics.Blit(indirectDiffuse, indirectDiffuseTemp, material, 5);
                Graphics.Blit(indirectDiffuseTemp, indirectDiffuse, material, 6);
            }

            material.SetFloat("_MaximumIterations", maximumIterationsSpecular);
            material.SetFloat("_ConeStep", coneStepSpecular);
            material.SetFloat("_StepMultiplier", stepMultiplierSpecular);
            material.SetFloat("_ConeOffset", coneOffsetSpecular);

            Graphics.Blit(source, indirectSpecular, material, 2);
            
            material.SetFloat("_BlurStep", blurStepSpecular);

            for (int i = 0; i < blurIterationsSpecular; ++i)
            {
                Graphics.Blit(indirectSpecular, indirectSpecularTemp, material, 5);
                Graphics.Blit(indirectSpecularTemp, indirectSpecular, material, 6);
            }

            material.SetFloat("_IndirectDiffuseStrength", indirectDiffuseLightingStrength);
            material.SetTexture("_IndirectDiffuse", indirectDiffuse);

            material.SetFloat("_IndirectSpecularStrength", indirectSpecularLightingStrength);
            material.SetTexture("_IndirectSpecular", indirectSpecular);

            Graphics.Blit(source, destination, material, 4);

            RenderTexture.ReleaseTemporary(indirectDiffuse);
            RenderTexture.ReleaseTemporary(indirectDiffuseTemp);

            RenderTexture.ReleaseTemporary(indirectSpecular);
            RenderTexture.ReleaseTemporary(indirectSpecularTemp);

        }
        // Voxelization debug pass
        else if(computation == Computation.VOXELIZATION)
        {
            Graphics.Blit(source, destination, material, 7);
        }
    }
}