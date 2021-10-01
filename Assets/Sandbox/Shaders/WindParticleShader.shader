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

Shader "Unlit/WindParticleShader"
{
	Properties
	{
		_MainTex("Particle Texture", 2D) = "white" {}
		_ColourTex("Speed Colour Map", 2D) = "white" {}
		_WindSpeedMultiplier("Wind Speed Multiplier", float) = 1
	}
		SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		Cull Back Lighting Off ZWrite Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma geometry geom

			#include "UnityCG.cginc"
			struct ParticleData {
				float2 Position;
				float2 Velocity;
				int Age;
				int StartingAge;
				int AlternateColour;
			};

			StructuredBuffer<ParticleData> ParticleBuffer;
			float4x4 Mat_Object2World;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 particleExtraInfo : TEXCOORD1;
				float2 particleAlternateColour : TEXCOORD2;
			};

			float _WindSpeedMultiplier;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _ColourTex;
			float4 _ColourTex_ST;

			v2f vert(uint id : SV_VertexID)
			{
				v2f o;
				uint vertexIndex = id / 3;
				uint vertexTri = id % 3;
				ParticleData particleData = ParticleBuffer[vertexIndex];

				float4 vertexPos = float4(particleData.Position.x, particleData.Position.y, 100, 1);
				float2 uv;
				float2 particleExtraInfo = float2(sqrt(particleData.Velocity.x * particleData.Velocity.x
										+ particleData.Velocity.y * particleData.Velocity.y) * 4.0 / _WindSpeedMultiplier, 0);
				
				[branch] switch (vertexTri) {
				case 0:
					uv = fixed2(0, 0);
					break;
				case 1:
					vertexPos = vertexPos + float4(0, 0.5, 0, 0);
					uv = fixed2(0, 1);
					break;
				case 2:
					vertexPos = vertexPos + float4(0.5, 0, 0, 0);
					uv = fixed2(1, 0);
					break;
				};
				vertexPos = mul(UNITY_MATRIX_VP, mul(Mat_Object2World, vertexPos));
				o.pos = vertexPos;
				o.uv = uv;

				float fadeIn = particleData.StartingAge - particleData.Age;
				float fadeUsed = particleData.Age < fadeIn ? particleData.Age : fadeIn;
				fadeUsed = clamp(fadeUsed, 0, 20);
				float fadeAmount = fadeUsed / 20.0;

				particleExtraInfo.y = fadeAmount;
				o.particleExtraInfo = particleExtraInfo;
				o.particleAlternateColour = float2(particleData.AlternateColour, 0);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 normalColour = fixed4(0, 0.2 + 0.8 * i.particleExtraInfo.x, 0, i.particleExtraInfo.y);
				fixed4 pollutantColour = fixed4(1, 0.6f, 0, i.particleExtraInfo.y);
				fixed4 finalColour = i.particleAlternateColour.x >= 1 ? pollutantColour : normalColour;

				return tex2D(_MainTex, i.uv)*finalColour;
			}
			ENDCG
		}
	}
}