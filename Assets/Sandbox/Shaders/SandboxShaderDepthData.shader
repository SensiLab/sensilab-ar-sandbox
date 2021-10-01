//  
//  SandboxShaderDepthData.shader
//
//	Copyright 2021 SensiLab, Monash University <sensilab@monash.edu>
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

Shader "Unlit/SandboxShaderDepthData"
{
	Properties
	{
		_ZScaleFactor("_ZScaleFactor", float) = 1
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

			struct v2f
			{
				float2 vertexHeight : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			StructuredBuffer<float3> VertexBuffer;
			StructuredBuffer<float2> UVBuffer;
			float4x4 Mat_Object2World;
			int MeshWidth;

			v2f vert(uint id : SV_VertexID)
			{
				v2f o;
				uint triIndex = id % 6;
				uint triNumber = floor(id / 6);
				uint x = triNumber % (MeshWidth - 1);
				uint y = floor(triNumber / (MeshWidth - 1));
				uint vertexIndex = x + y * MeshWidth;

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
				o.vertexHeight = float2(VertexBuffer[vI].z, 0);

				return o;
			}
			
			float _ZScaleFactor;

			float frag (v2f i) : SV_Target
			{
				return i.vertexHeight.x * _ZScaleFactor / 65535.0f;
			}
			ENDCG
		}
	}
}
