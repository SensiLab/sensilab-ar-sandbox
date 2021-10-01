//  
//  This file is part of sensilab-ar-sandbox.
//
//  sensilab-ar-sandbox is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  sensilab-ar-sandbox is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with sensilab-ar-sandbox.  If not, see <https://www.gnu.org/licenses/>.
//

Shader "Unlit/SandboxShaderContourData"
{
	Properties
	{
		_HeightTex("Height Texture", 2D) = "white" {}
		_ColorScaleTex("Color Texture", 2D) = "white" {}
		_ContourStride("Contour Stride (mm)", float) = 20
		_ContourWidth("Contour Width", float) = 1
		_MinorContours("Minor Contours", float) = 0
		_MinDepth("Min Depth (mm)", float) = 1000
		_MaxDepth("Max Depth (mm)", float) = 2000
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#define POW_2_16	65535.0

			struct v2f
			{
				float2 uv_HeightTex : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			struct ContourMapFrag {
				uint sunkContourNumber;
				uint contourNumber;
			};

			StructuredBuffer<float3> VertexBuffer;
			StructuredBuffer<float2> UVBuffer;
			float4x4 Mat_Object2World;
			int MeshWidth;

			sampler2D _HeightTex;
			sampler2D _ColorScaleTex;

			float4 _HeightTex_ST;
			float4 _ColorScaleTex_ST;

			float _ContourStride;
			float _ContourWidth;
			int _MinorContours;
			float _MinDepth;
			float _MaxDepth;

			v2f vert (uint id : SV_VertexID)
			{
				v2f o;
				uint triIndex = id % 6;
				uint triNumber = floor(id / 6.0);
				uint x = triNumber % (MeshWidth - 1);
				uint y = floor(triNumber / (MeshWidth - 1));
				uint vertexIndex = x + y * (MeshWidth);

				uint vI;
				[branch] switch (triIndex) {
					case 0: vI = vertexIndex + MeshWidth; break;
					case 1: vI = vertexIndex + 1; break;
					case 2: vI = vertexIndex; break;

					case 3: vI = vertexIndex + MeshWidth + 1; break;
					case 4: vI = vertexIndex + 1; break;
					case 5: vI = vertexIndex + MeshWidth; break;
				};

				o.vertex = mul(UNITY_MATRIX_VP, mul(Mat_Object2World, float4(VertexBuffer[vI], 1.0f)));
				o.uv_HeightTex = TRANSFORM_TEX(UVBuffer[vI], _HeightTex);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float height = (float)tex2D(_HeightTex, i.uv_HeightTex);
				float heightScaled = height * POW_2_16;

				int heightLevel = floor((heightScaled - _MaxDepth) / _ContourStride);
				int sunkHeightLevel = floor((heightScaled - _MaxDepth + 5) / _ContourStride);

				return fixed4((uint)-heightLevel % 2, (float)-sunkHeightLevel / 255.0, 0, 1);
			}
			ENDCG
		}
	}
}
