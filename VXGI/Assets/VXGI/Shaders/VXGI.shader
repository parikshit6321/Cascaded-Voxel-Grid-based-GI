Shader "Hidden/VXGI"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	// Structure representing an individual voxel element
	struct Voxel
	{
		int data;
	};

	// Structured buffer containing the data for the diffuse voxel grid's first cascade
	uniform StructuredBuffer<Voxel> _VoxelVolumeBufferDiffuse1;

	// Structured buffer containing the data for the diffuse voxel grid's second cascade
	uniform StructuredBuffer<Voxel> _VoxelVolumeBufferDiffuse2;

	// Structured buffer containing the data for the diffuse voxel grid's third cascade
	uniform StructuredBuffer<Voxel> _VoxelVolumeBufferDiffuse3;

	// Structured buffer containing the data for the diffuse voxel grid's fourth cascade
	uniform StructuredBuffer<Voxel> _VoxelVolumeBufferDiffuse4;

	// Structured buffer containing the data for the diffuse voxel grid's fifth cascade
	uniform StructuredBuffer<Voxel> _VoxelVolumeBufferDiffuse5;

	// Structured buffer containing the data for the diffuse voxel grid's sixth cascade
	uniform StructuredBuffer<Voxel> _VoxelVolumeBufferDiffuse6;

	// Structured buffer containing the data for the diffuse voxel grid's seventh cascade
	uniform StructuredBuffer<Voxel> _VoxelVolumeBufferDiffuse7;

	// Structured buffer containing the data for the diffuse voxel grid's eigth cascade
	uniform StructuredBuffer<Voxel> _VoxelVolumeBufferDiffuse8;

	// Structured buffer containing the data for the specular voxel grid
	uniform StructuredBuffer<Voxel> _VoxelVolumeBufferSpecular;

	// Main texture for the composite pass
	uniform sampler2D				_MainTex;

	// Indirect specular lighting texture for the composite pass
	uniform sampler2D				_IndirectSpecular;

	// Indirect diffuse lighting texture for the composite pass
	uniform sampler2D				_IndirectDiffuse;

	// Texture containing the current camera's world-space position texture
	uniform sampler2D				_PositionTexture;

	// Texture containing the current camera's color texture
	uniform sampler2D				_ColorTexture;

	// Texture containing the current camera's world-space normal texture
	uniform sampler2D				_NormalTexture;

	// Texel size used for calculating offset during blurring
	uniform float4					_MainTex_TexelSize;

	// Variable denoting the dimensions of the diffuse voxel grid's first cascade
	uniform int						_VoxelVolumeDimensionDiffuse1;

	// Variable denoting the dimensions of the diffuse voxel grid's second cascade
	uniform int						_VoxelVolumeDimensionDiffuse2;

	// Variable denoting the dimensions of the diffuse voxel grid's third cascade
	uniform int						_VoxelVolumeDimensionDiffuse3;

	// Variable denoting the dimensions of the diffuse voxel grid's fourth cascade
	uniform int						_VoxelVolumeDimensionDiffuse4;

	// Variable denoting the dimensions of the diffuse voxel grid's fifth cascade
	uniform int						_VoxelVolumeDimensionDiffuse5;

	// Variable denoting the dimensions of the diffuse voxel grid's sixth cascade
	uniform int						_VoxelVolumeDimensionDiffuse6;

	// Variable denoting the dimensions of the diffuse voxel grid's seventh cascade
	uniform int						_VoxelVolumeDimensionDiffuse7;

	// Variable denoting the dimensions of the diffuse voxel grid's eigth cascade
	uniform int						_VoxelVolumeDimensionDiffuse8;

	// Variable denoting the dimensions of the specular voxel grid
	uniform int						_VoxelVolumeDimensionSpecular;

	// Variable denoting the boundary of the world volume which has been voxelized
	uniform int						_WorldVolumeBoundary;

	// Strength of the direct lighting
	uniform float					_DirectStrength;

	// Strength of the ambient lighting
	uniform float					_AmbientLightingStrength;

	// Strength of the indirect specular lighting
	uniform float					_IndirectSpecularStrength;

	// Strength of the indirect diffuse lighting
	uniform float					_IndirectDiffuseStrength;

	// Maximum number of iterations in indirect specular cone tracing pass
	uniform float					_MaximumIterations;

	// Step mulitplier for the cone tracing step
	uniform float					_StepMultiplier;

	// Step value for indirect specular cone tracing pass
	uniform float					_ConeStep;

	// Angle for the cone tracing step in indirect diffuse lighting
	uniform float					_ConeAngle;

	// Offset value for indirect specular cone tracing pass
	uniform float					_ConeOffset;

	// Step value used for blurring
	uniform float					_BlurStep;

	// Current timestamp for voxel information
	uniform int						_CurrentTimestamp;

	// Structure representing the input to the vertex shader
	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	// Structure representing the input to the composite pass fragment shaders
	struct v2f_composite
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	// Structure representing the input to the indirect lighting fragment shaders
	struct v2f_render
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float4 worldPos : TEXCOORD1;
	};

	// Structure representing the input to the fragment shader of blur pass
	struct v2f_blur
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float2 offset1 : TEXCOORD1;
		float2 offset2 : TEXCOORD2;
		float2 offset3 : TEXCOORD3;
		float2 offset4 : TEXCOORD4;
	};

	// Vertex shader for the horizontal blurring pass
	v2f_blur vert_horizontal_blur(appdata v)
	{
		half unitX = _MainTex_TexelSize.x * _BlurStep;

		v2f_blur o;

		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.uv;

		o.offset1 = half2(-2.0 * unitX, 0.0);
		o.offset2 = half2(-unitX, 0.0);
		o.offset3 = half2(unitX, 0.0);
		o.offset4 = half2(2.0 * unitX, 0.0);

		return o;
	}

	// Vertex shader for the vertical blurring pass
	v2f_blur vert_vertical_blur(appdata v)
	{
		half unitY = _MainTex_TexelSize.y * _BlurStep;

		v2f_blur o;

		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.uv;

		o.offset1 = half2(0.0, 2.0 * unitY);
		o.offset2 = half2(0.0, unitY);
		o.offset3 = half2(0.0, -unitY);
		o.offset4 = half2(0.0, -2.0 * unitY);

		return o;
	}

	// Fragment shader for the blur pass
	float4 frag_blur(v2f_blur i) : SV_Target
	{
		float4 col = tex2D(_MainTex, i.uv);
		col += tex2D(_MainTex, i.uv + i.offset1);
		col += tex2D(_MainTex, i.uv + i.offset2);
		col += tex2D(_MainTex, i.uv + i.offset3);
		col += tex2D(_MainTex, i.uv + i.offset4);

		col *= 0.2;

		return col;
	}

	// Vertex shader for the indirect diffuse lighting pass
	v2f_render vert_indirect(appdata v)
	{
		v2f_render o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.uv;
		o.worldPos = mul(_Object2World, v.vertex);
		return o;
	}

	// Function used to unpack the data of the current voxel
	inline float4 UnpackData(int packedData)
	{
		int timestamp = packedData & 255;
		packedData = packedData >> 8;
		int colorB = packedData & 255;
		packedData = packedData >> 8;
		int colorG = packedData & 255;
		packedData = packedData >> 8;
		int colorR = packedData & 255;

		return float4(((float)colorR / 255.0), ((float)colorG / 255.0), ((float)colorB / 255.0), (float)timestamp);
	}

	// Returns the information (xyz - directLightingColor; w - timestamp) of the voxel at the given world position from the diffuse voxel grid's first cascade
	inline float4 GetVoxelInfoDiffuse1(float3 worldPosition)
	{
		// Default value
		float4 info = float4(0.0, 0.0, 0.0, 0.0);

		// Check if the given position is inside the voxelized volume
		if ((abs(worldPosition.x) < _WorldVolumeBoundary) && (abs(worldPosition.y) < _WorldVolumeBoundary) && (abs(worldPosition.z) < _WorldVolumeBoundary))
		{
			worldPosition += _WorldVolumeBoundary;
			worldPosition /= (2.0 * _WorldVolumeBoundary);

			float3 temp = worldPosition * _VoxelVolumeDimensionDiffuse1;

			int3 voxelPosition = (int3)(temp);

			int index = (voxelPosition.x * _VoxelVolumeDimensionDiffuse1 * _VoxelVolumeDimensionDiffuse1) + (voxelPosition.y * _VoxelVolumeDimensionDiffuse1) + (voxelPosition.z);

			info = UnpackData(_VoxelVolumeBufferDiffuse1[index].data);
		}

		return info;
	}

	// Returns the information (xyz - directLightingColor; w - timestamp) of the voxel at the given world position from the diffuse voxel grid's second cascade
	inline float4 GetVoxelInfoDiffuse2(float3 worldPosition)
	{
		// Default value
		float4 info = float4(0.0, 0.0, 0.0, 0.0);

		// Check if the given position is inside the voxelized volume
		if ((abs(worldPosition.x) < _WorldVolumeBoundary) && (abs(worldPosition.y) < _WorldVolumeBoundary) && (abs(worldPosition.z) < _WorldVolumeBoundary))
		{
			worldPosition += _WorldVolumeBoundary;
			worldPosition /= (2.0 * _WorldVolumeBoundary);

			float3 temp = worldPosition * _VoxelVolumeDimensionDiffuse2;

			int3 voxelPosition = (int3)(temp);

			int index = (voxelPosition.x * _VoxelVolumeDimensionDiffuse2 * _VoxelVolumeDimensionDiffuse2) + (voxelPosition.y * _VoxelVolumeDimensionDiffuse2) + (voxelPosition.z);

			info = UnpackData(_VoxelVolumeBufferDiffuse2[index].data);
		}

		return info;
	}

	// Returns the information (xyz - directLightingColor; w - timestamp) of the voxel at the given world position from the diffuse voxel grid's third cascade
	inline float4 GetVoxelInfoDiffuse3(float3 worldPosition)
	{
		// Default value
		float4 info = float4(0.0, 0.0, 0.0, 0.0);

		// Check if the given position is inside the voxelized volume
		if ((abs(worldPosition.x) < _WorldVolumeBoundary) && (abs(worldPosition.y) < _WorldVolumeBoundary) && (abs(worldPosition.z) < _WorldVolumeBoundary))
		{
			worldPosition += _WorldVolumeBoundary;
			worldPosition /= (2.0 * _WorldVolumeBoundary);

			float3 temp = worldPosition * _VoxelVolumeDimensionDiffuse3;

			int3 voxelPosition = (int3)(temp);

			int index = (voxelPosition.x * _VoxelVolumeDimensionDiffuse3 * _VoxelVolumeDimensionDiffuse3) + (voxelPosition.y * _VoxelVolumeDimensionDiffuse3) + (voxelPosition.z);

			info = UnpackData(_VoxelVolumeBufferDiffuse3[index].data);
		}

		return info;
	}

	// Returns the information (xyz - directLightingColor; w - timestamp) of the voxel at the given world position from the diffuse voxel grid's fourth cascade
	inline float4 GetVoxelInfoDiffuse4(float3 worldPosition)
	{
		// Default value
		float4 info = float4(0.0, 0.0, 0.0, 0.0);

		// Check if the given position is inside the voxelized volume
		if ((abs(worldPosition.x) < _WorldVolumeBoundary) && (abs(worldPosition.y) < _WorldVolumeBoundary) && (abs(worldPosition.z) < _WorldVolumeBoundary))
		{
			worldPosition += _WorldVolumeBoundary;
			worldPosition /= (2.0 * _WorldVolumeBoundary);

			float3 temp = worldPosition * _VoxelVolumeDimensionDiffuse4;

			int3 voxelPosition = (int3)(temp);

			int index = (voxelPosition.x * _VoxelVolumeDimensionDiffuse4 * _VoxelVolumeDimensionDiffuse4) + (voxelPosition.y * _VoxelVolumeDimensionDiffuse4) + (voxelPosition.z);

			info = UnpackData(_VoxelVolumeBufferDiffuse4[index].data);
		}

		return info;
	}

	// Returns the information (xyz - directLightingColor; w - timestamp) of the voxel at the given world position from the diffuse voxel grid's fifth cascade
	inline float4 GetVoxelInfoDiffuse5(float3 worldPosition)
	{
		// Default value
		float4 info = float4(0.0, 0.0, 0.0, 0.0);

		// Check if the given position is inside the voxelized volume
		if ((abs(worldPosition.x) < _WorldVolumeBoundary) && (abs(worldPosition.y) < _WorldVolumeBoundary) && (abs(worldPosition.z) < _WorldVolumeBoundary))
		{
			worldPosition += _WorldVolumeBoundary;
			worldPosition /= (2.0 * _WorldVolumeBoundary);

			float3 temp = worldPosition * _VoxelVolumeDimensionDiffuse5;

			int3 voxelPosition = (int3)(temp);

			int index = (voxelPosition.x * _VoxelVolumeDimensionDiffuse5 * _VoxelVolumeDimensionDiffuse5) + (voxelPosition.y * _VoxelVolumeDimensionDiffuse5) + (voxelPosition.z);

			info = UnpackData(_VoxelVolumeBufferDiffuse5[index].data);
		}

		return info;
	}

	// Returns the information (xyz - directLightingColor; w - timestamp) of the voxel at the given world position from the diffuse voxel grid's sixth cascade
	inline float4 GetVoxelInfoDiffuse6(float3 worldPosition)
	{
		// Default value
		float4 info = float4(0.0, 0.0, 0.0, 0.0);

		// Check if the given position is inside the voxelized volume
		if ((abs(worldPosition.x) < _WorldVolumeBoundary) && (abs(worldPosition.y) < _WorldVolumeBoundary) && (abs(worldPosition.z) < _WorldVolumeBoundary))
		{
			worldPosition += _WorldVolumeBoundary;
			worldPosition /= (2.0 * _WorldVolumeBoundary);

			float3 temp = worldPosition * _VoxelVolumeDimensionDiffuse6;

			int3 voxelPosition = (int3)(temp);

			int index = (voxelPosition.x * _VoxelVolumeDimensionDiffuse6 * _VoxelVolumeDimensionDiffuse6) + (voxelPosition.y * _VoxelVolumeDimensionDiffuse6) + (voxelPosition.z);

			info = UnpackData(_VoxelVolumeBufferDiffuse6[index].data);
		}

		return info;
	}

	// Returns the information (xyz - directLightingColor; w - timestamp) of the voxel at the given world position from the diffuse voxel grid's seventh cascade
	inline float4 GetVoxelInfoDiffuse7(float3 worldPosition)
	{
		// Default value
		float4 info = float4(0.0, 0.0, 0.0, 0.0);

		// Check if the given position is inside the voxelized volume
		if ((abs(worldPosition.x) < _WorldVolumeBoundary) && (abs(worldPosition.y) < _WorldVolumeBoundary) && (abs(worldPosition.z) < _WorldVolumeBoundary))
		{
			worldPosition += _WorldVolumeBoundary;
			worldPosition /= (2.0 * _WorldVolumeBoundary);

			float3 temp = worldPosition * _VoxelVolumeDimensionDiffuse7;

			int3 voxelPosition = (int3)(temp);

			int index = (voxelPosition.x * _VoxelVolumeDimensionDiffuse7 * _VoxelVolumeDimensionDiffuse7) + (voxelPosition.y * _VoxelVolumeDimensionDiffuse7) + (voxelPosition.z);

			info = UnpackData(_VoxelVolumeBufferDiffuse7[index].data);
		}

		return info;
	}

	// Returns the information (xyz - directLightingColor; w - timestamp) of the voxel at the given world position from the diffuse voxel grid's eigth cascade
	inline float4 GetVoxelInfoDiffuse8(float3 worldPosition)
	{
		// Default value
		float4 info = float4(0.0, 0.0, 0.0, 0.0);

		// Check if the given position is inside the voxelized volume
		if ((abs(worldPosition.x) < _WorldVolumeBoundary) && (abs(worldPosition.y) < _WorldVolumeBoundary) && (abs(worldPosition.z) < _WorldVolumeBoundary))
		{
			worldPosition += _WorldVolumeBoundary;
			worldPosition /= (2.0 * _WorldVolumeBoundary);

			float3 temp = worldPosition * _VoxelVolumeDimensionDiffuse8;

			int3 voxelPosition = (int3)(temp);

			int index = (voxelPosition.x * _VoxelVolumeDimensionDiffuse8 * _VoxelVolumeDimensionDiffuse8) + (voxelPosition.y * _VoxelVolumeDimensionDiffuse8) + (voxelPosition.z);

			info = UnpackData(_VoxelVolumeBufferDiffuse8[index].data);
		}

		return info;
	}

	// Returns the information (xyz - directLightingColor; w - timestamp) of the voxel at the given world position from the specular voxel grid
	inline float4 GetVoxelInfoSpecular(float3 worldPosition)
	{
		// Default value
		float4 info = float4(0.0, 0.0, 0.0, 0.0);

		// Check if the given position is inside the voxelized volume
		if ((abs(worldPosition.x) < _WorldVolumeBoundary) && (abs(worldPosition.y) < _WorldVolumeBoundary) && (abs(worldPosition.z) < _WorldVolumeBoundary))
		{
			worldPosition += _WorldVolumeBoundary;
			worldPosition /= (2.0 * _WorldVolumeBoundary);

			float3 temp = worldPosition * _VoxelVolumeDimensionSpecular;

			int3 voxelPosition = (int3)(temp);

			int index = (voxelPosition.x * _VoxelVolumeDimensionSpecular * _VoxelVolumeDimensionSpecular) + (voxelPosition.y * _VoxelVolumeDimensionSpecular) + (voxelPosition.z);

			info = UnpackData(_VoxelVolumeBufferSpecular[index].data);
		}

		return info;
	}

	// Uses approximate cone tracing to accumulate indirect diffuse illumination through the scene
	inline float4 CascadedConeTrace(float3 worldPosition, float3 reflectedRayDirection, float3 pixelColor)
	{
		// Color for storing all the samples
		float3 accumulatedColor = float3(0.0, 0.0, 0.0);
		float3 currentColor = float3(0.0, 0.0, 0.0);

		float3 currentPosition = worldPosition + (_ConeOffset * reflectedRayDirection);
		float4 currentVoxelInfo = float4(0.0, 0.0, 0.0, 0.0);

		float currentWeight = 1.0;
		float totalWeight = 1.0;
		float offset = 0.0;
		float numberOfCascadesHit = 0.0;

		// Random vector so that the cross product does not become zero
		float3 randomVector = float3(1.0, 2.0, 3.0);
		randomVector = normalize(randomVector);

		// Displacement vectors along which the sample points will be computed
		float3 displacementVector1 = normalize(cross(reflectedRayDirection, randomVector));
		float3 displacementVector2 = -displacementVector1;
		float3 displacementVector3 = normalize(cross(reflectedRayDirection, displacementVector1));
		float3 displacementVector4 = -displacementVector3;

		// Traces a cone through the scene
		for (float i = 1.0; i < _MaximumIterations; i += 1.0)
		{
			currentColor = float3(0.0, 0.0, 0.0);
			numberOfCascadesHit = 0.0;

			// Traverse the ray in the reflected direction
			currentPosition += (reflectedRayDirection * _ConeStep);
			_ConeStep *= _StepMultiplier;

			// First cascade
			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				currentColor += (currentWeight * (currentVoxelInfo.xyz * pixelColor));
				numberOfCascadesHit += 1.0;
			}

			// Second cascade
			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse2(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				currentColor += (currentWeight * (currentVoxelInfo.xyz * pixelColor));
				numberOfCascadesHit += 1.0;
			}

			// Third cascade
			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse3(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				currentColor += (currentWeight * (currentVoxelInfo.xyz * pixelColor));
				numberOfCascadesHit += 1.0;
			}

			// Fourth cascade
			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse4(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				currentColor += (currentWeight * (currentVoxelInfo.xyz * pixelColor));
				numberOfCascadesHit += 1.0;
			}

			// Fifth cascade
			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse5(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				currentColor += (currentWeight * (currentVoxelInfo.xyz * pixelColor));
				numberOfCascadesHit += 1.0;
			}

			// Sixth cascade
			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse6(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				currentColor += (currentWeight * (currentVoxelInfo.xyz * pixelColor));
				numberOfCascadesHit += 1.0;
			}

			// Seventh cascade
			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse7(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				currentColor += (currentWeight * (currentVoxelInfo.xyz * pixelColor));
				numberOfCascadesHit += 1.0;
			}

			// Eigth cascade
			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse8(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				currentColor += (currentWeight * (currentVoxelInfo.xyz * pixelColor));
				numberOfCascadesHit += 1.0;
			}

			currentColor /= max(numberOfCascadesHit, 1.0);
			totalWeight += min(numberOfCascadesHit, 1.0);
			accumulatedColor += currentColor;
		}

		// Average out the accumulated color
		accumulatedColor /= totalWeight;

		return float4(accumulatedColor, 1.0);
	}

	// Traces a cone starting from the current voxel in the reflected ray direction and accumulates color
	inline float4 ConeTrace(float3 worldPosition, float3 reflectedRayDirection, float3 pixelColor)
	{
		// Color for storing all the samples
		float3 accumulatedColor = float3(0.0, 0.0, 0.0);

		float3 currentPosition = worldPosition + (_ConeOffset * reflectedRayDirection);
		float4 currentVoxelInfo = float4(0.0, 0.0, 0.0, 0.0);

		float currentWeight = 1.0;
		float totalWeight = 1.0;
		float offset = 0.0;

		// Random vector so that the cross product does not become zero
		float3 randomVector = float3(1.0, 2.0, 3.0);
		randomVector = normalize(randomVector);

#if defined(LOW_SAMPLES)

		// Displacement vectors along which the sample points will be computed
		float3 displacementVector1 = normalize(cross(reflectedRayDirection, randomVector));
		float3 displacementVector2 = -displacementVector1;
		float3 displacementVector3 = normalize(cross(reflectedRayDirection, displacementVector1));
		float3 displacementVector4 = -displacementVector3;
		
		// Traces a cone through the scene
		for (float i = 1.0; i < _MaximumIterations; i += 1.0)
		{
			// Traverse the ray in the reflected direction
			currentPosition += (reflectedRayDirection * _ConeStep);
			_ConeStep *= _StepMultiplier;

			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
				totalWeight += currentWeight;
			}

			// Traverses in the directions perpendicular to the reflected ray
			for (float j = 1.0; j < i; j += 1.0)
			{
				offset = (j * _ConeAngle);

				// First sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector1));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Second sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector2));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Third sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector3));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Fourth sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector4));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}
			}
		}

#endif

#if defined(MEDIUM_SAMPLES)

		// Displacement vectors along which the sample points will be computed
		float3 displacementVector1 = normalize(cross(reflectedRayDirection, randomVector));
		float3 displacementVector2 = -displacementVector1;
		float3 displacementVector3 = normalize(cross(reflectedRayDirection, displacementVector1));
		float3 displacementVector4 = normalize(displacementVector1 + (2.0 * displacementVector3));
		float3 displacementVector5 = normalize(displacementVector1 - (2.0 * displacementVector3));
		float3 displacementVector6 = -displacementVector5;
		float3 displacementVector7 = -displacementVector6;

		// Traces a cone through the scene
		for (float i = 1.0; i < _MaximumIterations; i += 1.0)
		{
			// Traverse the ray in the reflected direction
			currentPosition += (reflectedRayDirection * _ConeStep);
			_ConeStep *= _StepMultiplier;

			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition);

			// At the currently traced sample
			if ((int)currentVoxelInfo.w == _CurrentTimestamp)
			{
				accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
				totalWeight += currentWeight;
			}

			// Traverses in the directions perpendicular to the reflected ray
			for (float j = 1.0; j < i; j += 1.0)
			{
				offset = (j * _ConeAngle);

				// First sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector1));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Second sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector2));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Third sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector4));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Fourth sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector5));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Fifth sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector6));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Sixth sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector7));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

			}
		}

#endif

#if defined(HIGH_SAMPLES)

		// Displacement vectors along which the sample points will be computed
		float3 displacementVector1 = normalize(cross(reflectedRayDirection, randomVector));
		float3 displacementVector2 = -displacementVector1;
		float3 displacementVector3 = normalize(cross(reflectedRayDirection, displacementVector1));
		float3 displacementVector4 = -displacementVector3;
		float3 displacementVector5 = normalize(displacementVector1 + displacementVector3);
		float3 displacementVector6 = normalize(displacementVector1 + displacementVector4);
		float3 displacementVector7 = -displacementVector5;
		float3 displacementVector8 = -displacementVector6;

		// Traces a cone through the scene
		for (float i = 1.0; i < _MaximumIterations; i += 1.0)
		{
			// Traverse the ray in the reflected direction
			currentPosition += (reflectedRayDirection * _ConeStep);
			_ConeStep *= _StepMultiplier;

			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition);

			// At the currently traced sample
			if ((int)currentVoxelInfo.w == _CurrentTimestamp)
			{
				accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
				totalWeight += currentWeight;
			}

			// Traverses in the directions perpendicular to the reflected ray
			for (float j = 1.0; j < i; j += 1.0)
			{
				offset = (j * _ConeAngle);

				// First sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector1));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Second sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector2));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Third sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector3));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Fourth sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector4));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Fifth sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector5));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Sixth sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector6));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Seventh sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector7));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}

				// Eigth sample
				currentVoxelInfo = GetVoxelInfoDiffuse1(currentPosition + (offset * displacementVector8));
				// At the currently traced sample
				if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
				{
					accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
					totalWeight += currentWeight;
				}
			}
		}

#endif

		// Average out the accumulated color
		accumulatedColor /= totalWeight;

		return float4(accumulatedColor, 1.0);
	}

	// Traces a ra starting from the current voxel in the reflected ray direction and accumulates color
	inline float4 RayTrace(float3 worldPosition, float3 reflectedRayDirection, float3 pixelColor)
	{
		// Color for storing all the samples
		float3 accumulatedColor = float3(0.0, 0.0, 0.0);

		float3 currentPosition = worldPosition + (_ConeOffset * reflectedRayDirection);
		float4 currentVoxelInfo = float4(0.0, 0.0, 0.0, 0.0);

		float currentWeight = 1.0;
		float totalWeight = 0.0;

		// Loop for tracing the ray through the scene
		for (float i = 0.0; i < _MaximumIterations; i += 1.0)
		{
			// Traverse the ray in the reflected direction
			currentPosition += (reflectedRayDirection * _ConeStep);
			_ConeStep *= _StepMultiplier;

			// Get the currently hit voxel's information
			currentVoxelInfo = GetVoxelInfoSpecular(currentPosition);

			// At the currently traced sample
			if (((int)currentVoxelInfo.w | 0) || ((int)currentVoxelInfo.w == _CurrentTimestamp))
			{
				accumulatedColor += (currentVoxelInfo.xyz * pixelColor);
				totalWeight += currentWeight;
			}
		}

		// Average out the accumulated color
		accumulatedColor /= totalWeight;

		return float4(accumulatedColor, 1.0);
	}

	// Traces hemisphere to collect one bounce indirect diffuse illumination
	inline float4 HemisphereTrace(float3 worldPosition, float3 normal, float3 pixelColor)
	{
		float4 accumulatedColor = float4(0.0, 0.0, 0.0, 1.0);

		float3 normalDirection = normalize(normal);

		// Random vector so that the cross product does not become zero
		float3 randomVector = float3(1.0, 2.0, 3.0);
		randomVector = normalize(randomVector);

		// Displacement vectors along which the sample points will be computed
		float3 displacementVector1 = normalize(cross(normalDirection, randomVector));
		float3 displacementVector2 = -displacementVector1;
		float3 displacementVector3 = normalize(cross(normalDirection, displacementVector1));
		float3 displacementVector4 = -displacementVector3;
		float3 displacementVector5 = normalize(displacementVector1 + displacementVector3);
		float3 displacementVector6 = normalize(displacementVector1 + displacementVector4);
		float3 displacementVector7 = -displacementVector5;
		float3 displacementVector8 = -displacementVector6;

#if defined(APPROXIMATE_CONE_TRACE)
		
		float3 direction1 = normalize(normalDirection);
		
		float3 direction2 = normalize(lerp(normalDirection, displacementVector1, 0.8));
		float3 direction3 = normalize(lerp(normalDirection, displacementVector2, 0.8));
		float3 direction4 = normalize(lerp(normalDirection, displacementVector3, 0.8));
		float3 direction5 = normalize(lerp(normalDirection, displacementVector4, 0.8));
		float3 direction6 = normalize(lerp(normalDirection, displacementVector5, 0.8));
		float3 direction7 = normalize(lerp(normalDirection, displacementVector6, 0.8));
		float3 direction8 = normalize(lerp(normalDirection, displacementVector7, 0.8));
		float3 direction9 = normalize(lerp(normalDirection, displacementVector8, 0.8));
		
		float3 direction10 = normalize(lerp(normalDirection, displacementVector1, 0.4));
		float3 direction11 = normalize(lerp(normalDirection, displacementVector2, 0.4));
		float3 direction12 = normalize(lerp(normalDirection, displacementVector3, 0.4));
		float3 direction13 = normalize(lerp(normalDirection, displacementVector4, 0.4));
		
		float3 temp1 = normalize(displacementVector5 + displacementVector3);
		float3 temp2 = normalize(displacementVector6 + displacementVector4);

		float3 direction14 = normalize(lerp(normalDirection, displacementVector1, 0.6));
		float3 direction15 = normalize(lerp(normalDirection, displacementVector2, 0.6));
		float3 direction16 = normalize(lerp(normalDirection, temp1, 0.6));
		float3 direction17 = normalize(lerp(normalDirection, -temp1, 0.6));
		float3 direction18 = normalize(lerp(normalDirection, temp2, 0.6));
		float3 direction19 = normalize(lerp(normalDirection, -temp2, 0.6));

		// Use 19 cones to approximate indirect diffuse illumination
		accumulatedColor += CascadedConeTrace(worldPosition, direction1, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction2, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction3, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction4, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction5, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction6, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction7, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction8, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction9, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction10, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction11, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction12, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction13, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction14, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction15, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction16, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction17, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction18, pixelColor);
		accumulatedColor += CascadedConeTrace(worldPosition, direction19, pixelColor);
		
		accumulatedColor /= 19.0;

#endif

#if defined(REALISTIC_CONE_TRACE)

		float3 direction1 = normalize(normalDirection);
		float3 direction2 = normalize(lerp(normalDirection, displacementVector1, 0.7));
		float3 direction3 = normalize(lerp(normalDirection, displacementVector2, 0.7));
		float3 direction4 = normalize(lerp(normalDirection, displacementVector3, 0.7));
		float3 direction5 = normalize(lerp(normalDirection, displacementVector4, 0.7));
		float3 direction6 = normalize(lerp(normalDirection, displacementVector5, 0.7));
		float3 direction7 = normalize(lerp(normalDirection, displacementVector6, 0.7));
		float3 direction8 = normalize(lerp(normalDirection, displacementVector7, 0.7));
		float3 direction9 = normalize(lerp(normalDirection, displacementVector8, 0.7));

		// Use 9 cones to approximate indirect diffuse illumination
		accumulatedColor += ConeTrace(worldPosition, direction1, pixelColor);
		accumulatedColor += ConeTrace(worldPosition, direction2, pixelColor);
		accumulatedColor += ConeTrace(worldPosition, direction3, pixelColor);
		accumulatedColor += ConeTrace(worldPosition, direction4, pixelColor);
		accumulatedColor += ConeTrace(worldPosition, direction5, pixelColor);
		accumulatedColor += ConeTrace(worldPosition, direction6, pixelColor);
		accumulatedColor += ConeTrace(worldPosition, direction7, pixelColor);
		accumulatedColor += ConeTrace(worldPosition, direction8, pixelColor);
		accumulatedColor += ConeTrace(worldPosition, direction9, pixelColor);

		accumulatedColor /= 9.0;

#endif
		
		return accumulatedColor;
	}

	// Fragment shader for indirect diffuse lighting pass
	float4 frag_indirect_diffuse(v2f_render i) : SV_Target
	{
		// This will store the indirect diffuse bounce
		float4 accumulatedColor = float4(0.0, 0.0, 0.0, 1.0);

		// Extract the current pixel's color
		float3 pixelColor = tex2D(_ColorTexture, i.uv).rgb;

		// Extracted world position is between 0...1
		float3 worldPosition = tex2D(_PositionTexture, i.uv);

		worldPosition *= (2.0 * _WorldVolumeBoundary);
		worldPosition -= _WorldVolumeBoundary;
		
		// Extract the information of the current pixel from the voxel grid
		float3 normal = tex2D(_NormalTexture, i.uv);

		normal -= 0.5;
		normal *= 2.0;

		// Trace the hemisphere to get the indirect diffuse illumination
		accumulatedColor = HemisphereTrace(worldPosition, normal, pixelColor);

		// Return the final color accumulated after tracing cones
		return accumulatedColor;
	}

	// Fragment shader for the indirect specular lighting pass
	float4 frag_indirect_specular(v2f_render i) : SV_Target
	{
		// Color which will be accumulated during the cone tracing pass
		float4 accumulatedColor = float4(0.0, 0.0, 0.0, 1.0);

		// Extract the current pixel's color
		float3 pixelColor = tex2D(_ColorTexture, i.uv).rgb;

		// Extracted world position is between 0...1
		float3 worldPosition = tex2D(_PositionTexture, i.uv);

		worldPosition *= (2.0 * _WorldVolumeBoundary);
		worldPosition -= _WorldVolumeBoundary;
		
		// Compute the current pixel to camera unit vector
		float3 pixelToCameraUnitVector = normalize(_WorldSpaceCameraPos - worldPosition);

		// Extract the information of the current pixel from the voxel grid
		float3 normal = tex2D(_NormalTexture, i.uv);

		normal -= 0.5;
		normal *= 2.0;

		// Compute the reflected ray direction
		float3 reflectedRayDirection = normalize(reflect(pixelToCameraUnitVector, normal));

		reflectedRayDirection *= -1.0;

		// Perform the cone tracing step
		accumulatedColor = RayTrace(worldPosition, reflectedRayDirection, pixelColor);

		// Return the final color accumulated after tracing rays
		return accumulatedColor;
	}

	// Vertex shader for the voxelization debug pass
	v2f_render vert_voxelization(appdata v)
	{
		v2f_render o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.uv;
		o.worldPos = mul(_Object2World, v.vertex);
		return o;
	}

	// Fragment shader for the voxelization debug pass
	float4 frag_voxelization(v2f_render i) : SV_Target
	{
		// Extracted world position is between 0...1
		float3 worldPosition = tex2D(_PositionTexture, i.uv);

		worldPosition *= (2.0 * _WorldVolumeBoundary);
		worldPosition -= _WorldVolumeBoundary;
		
		return float4(GetVoxelInfoDiffuse2(worldPosition).xyz, 1.0);
	}

	// Vertex shader for the composite pass
	v2f_composite vert_composite(appdata v)
	{
		v2f_composite o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.uv;
		return o;
	}

	// Fragment shader for the indirect diffuse composite pass
	float4 frag_composite_diffuse(v2f_composite i) : SV_Target
	{
		float4 directLighting = tex2D(_MainTex, i.uv) * _DirectStrength;
		float4 indirectDiffuseLighting = tex2D(_IndirectDiffuse, i.uv) * _IndirectDiffuseStrength;
		float4 ambientLighting = tex2D(_ColorTexture, i.uv) * _AmbientLightingStrength;

		float4 finalColor = directLighting + indirectDiffuseLighting + ambientLighting;

		return finalColor;
	}

	// Fragment shader for the indirect specular composite pass
	float4 frag_composite_specular(v2f_composite i) : SV_Target
	{
		float4 directLighting = tex2D(_MainTex, i.uv) * _DirectStrength;
		float4 indirectSpecularLighting = tex2D(_IndirectSpecular, i.uv) * _IndirectSpecularStrength;
		float4 ambientLighting = tex2D(_ColorTexture, i.uv) * _AmbientLightingStrength;

		float4 finalColor = directLighting + indirectSpecularLighting + ambientLighting;

		return finalColor;
	}

	// Fragment shader for the indirect diffuse and specular composite pass
	float4 frag_composite_diffuse_specular(v2f_composite i) : SV_Target
	{
		float4 directLighting = tex2D(_MainTex, i.uv) * _DirectStrength;
		float4 indirectDiffuseLighting = tex2D(_IndirectDiffuse, i.uv) * _IndirectDiffuseStrength;
		float4 indirectSpecularLighting = tex2D(_IndirectSpecular, i.uv) * _IndirectSpecularStrength;
		float4 ambientLighting = tex2D(_ColorTexture, i.uv) * _AmbientLightingStrength;

		float4 finalColor = directLighting + indirectDiffuseLighting + indirectSpecularLighting + ambientLighting;

		return finalColor;
	}

	ENDCG

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		// 0 : Indirect Diffuse Lighting Pass
		Pass
		{
			CGPROGRAM

			#pragma multi_compile LOW_SAMPLES MEDIUM_SAMPLES HIGH_SAMPLES
			#pragma multi_compile APPROXIMATE_CONE_TRACE REALISTIC_CONE_TRACE
			#pragma vertex vert_indirect
			#pragma fragment frag_indirect_diffuse

			ENDCG
		}

		// 1 : Composite Pass for indirect diffuse lighting
		Pass
		{
			CGPROGRAM

			#pragma vertex vert_composite
			#pragma fragment frag_composite_diffuse

			ENDCG
		}

		// 2 : Indirect Specular Lighting Pass
		Pass
		{
			CGPROGRAM

			#pragma vertex vert_indirect
			#pragma fragment frag_indirect_specular

			ENDCG
		}

		// 3 : Composite Pass for indirect specular lighting
		Pass
		{
			CGPROGRAM

			#pragma vertex vert_composite
			#pragma fragment frag_composite_specular

			ENDCG
		}

		// 4 : Composite Pass for indirect diffuse and specular lighting
		Pass
		{
			CGPROGRAM

			#pragma vertex vert_composite
			#pragma fragment frag_composite_diffuse_specular

			ENDCG
		}

		// 5 : Vertical Blurring
		Pass
		{
			CGPROGRAM

			#pragma vertex vert_vertical_blur
			#pragma fragment frag_blur

			ENDCG
		}

		// 6 : Horizontal Blurring
		Pass
		{
			CGPROGRAM

			#pragma vertex vert_horizontal_blur
			#pragma fragment frag_blur

			ENDCG
		}

		// 7 : Voxelization Debug Pass
		Pass
		{
			CGPROGRAM

			#pragma vertex vert_voxelization
			#pragma fragment frag_voxelization

			ENDCG
		}
	}
}