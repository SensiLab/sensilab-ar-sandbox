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

Shader "Unlit/SandboxShaderTopographyBuilder"
{
	Properties
	{
		_HeightTex("Height Texture", 2D) = "white" {}
		_LoadedHeightTex("Loaded Height Texture", 2D) = "white" {}
		_HeightColorScaleTex("Height Color Texture", 2D) = "white" {}
		_ContourStride("Contour Stride (mm)", float) = 20
		_ContourWidth("Contour Width", float) = 1
		_ValidHeightRange("Valid Height Range (mm)", float) = 12
		_LoadedHeightOffset("Loaded Height Offset (mm)", float) = 0
		_MinDepth("Min Depth (mm)", float) = 1000
		_MaxDepth("Max Depth (mm)", float) = 2000
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float2 uv_HeightTex : TEXCOORD0;
				float2 uv_LabelMaskTex : TEXCOORD1;
				float2 uv_LoadedHeightTex : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			#include "SandboxShaderHelper.cginc"

			float _HeightTextureSizeX;
			float _HeightTextureSizeY;
			sampler2D _LoadedHeightTex;
			sampler2D _HeightColorScaleTex;

			float4 _LoadedHeightTex_ST;

			float _LoadedHeightOffset;
			float _ValidHeightRange;

			float BellFunc(float x)
			{
				float f = (x / 2.0) * 1.5; // Converting -2 to +2 to -1.5 to +1.5
				if (f > -1.5 && f < -0.5)
				{
					return(0.5 * pow(f + 1.5, 2.0));
				}
				else if (f > -0.5 && f < 0.5)
				{
					return 3.0 / 4.0 - (f * f);
				}
				else if ((f > 0.5 && f < 1.5))
				{
					return(0.5 * pow(f - 1.5, 2.0));
				}
				return 0.0;
			}

			float BiCubicLoadedHeight(float2 uv)
			{
				float2 uvStep = float2(1.0 / _HeightTextureSizeX, 1.0 / _HeightTextureSizeY);
				float sum = 0.0;
				float denom = 0.0;
				float a = frac(uv.x * _HeightTextureSizeX);
				float b = frac(uv.y * _HeightTextureSizeY);

				float2 texUV = uv - float2(a / _HeightTextureSizeX, b / _HeightTextureSizeY);
				for (int m = -1; m <= 2; m++)
				{
					for (int n = -1; n <= 2; n++)
					{
						float2 uvOffset = float2(uvStep.x * float(m), uvStep.y * float(n));
						float heightVal = (float)tex2D(_LoadedHeightTex, texUV + uvOffset);

						float f = BellFunc(float(m) - a);
						float f1 = BellFunc(-(float(n) - b));

						sum = sum + (heightVal * f * f1);
						denom = denom + (f * f1);
					}
				}
				return sum / denom;
			}

			float BiCubicHeight(float2 uv)
			{
				float2 uvStep = float2(1.0 / _HeightTextureSizeX, 1.0 / _HeightTextureSizeY);
				float sum = 0.0;
				float denom = 0.0;
				float a = frac(uv.x * _HeightTextureSizeX);
				float b = frac(uv.y * _HeightTextureSizeY);

				float2 texUV = uv - float2(a / _HeightTextureSizeX, b / _HeightTextureSizeY);
				for (int m = -1; m <= 2; m++)
				{
					for (int n = -1; n <= 2; n++)
					{
						float2 uvOffset = float2(uvStep.x * float(m), uvStep.y * float(n));
						float heightVal = (float)tex2D(_HeightTex, texUV + uvOffset);

						float f = BellFunc(float(m) - a);
						float f1 = BellFunc(-(float(n) - b));

						sum = sum + (heightVal * f * f1);
						denom = denom + (f * f1);
					}
				}
				return sum / denom;
			}

			v2f vert(uint id : SV_VertexID)
			{
				v2f o;
				uint vIndex = GetVertexID(id);

				o.vertex = mul(UNITY_MATRIX_VP, mul(Mat_Object2World, float4(VertexBuffer[vIndex], 1.0f)));
				o.uv_HeightTex = TRANSFORM_TEX(UVBuffer[vIndex], _HeightTex);
				o.uv_LabelMaskTex = TRANSFORM_TEX(UVBuffer[vIndex], _LabelMaskTex);
				o.uv_LoadedHeightTex = TRANSFORM_TEX(UVBuffer[vIndex], _LoadedHeightTex);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				ContourMapFrag contourMapFrag = GetContourMap(i);

				int onText = contourMapFrag.onText == 1 || contourMapFrag.onTextMask == 1;
				int drawMajorContourLine = contourMapFrag.onMajorContourLine == 1 && onText == 0;
				int drawMinorContourLine = contourMapFrag.onMinorContourLine == 1 && onText == 0;

				float originalHeight = BiCubicHeight(i.uv_HeightTex) * POW_2_16;
				float loadedHeight = BiCubicLoadedHeight(i.uv_LoadedHeightTex) * POW_2_16 + _LoadedHeightOffset;

				float depthRange = _MaxDepth - _MinDepth;
				float differenceTexCoord = 0.5 + (originalHeight - loadedHeight) / depthRange / 2.0;

				fixed4 differenceColour = tex2D(_HeightColorScaleTex, differenceTexCoord);

				differenceColour = abs(originalHeight - loadedHeight) < _ValidHeightRange / 2.0 ? fixed4(36.0 / 255.0, 1, 36.0 / 255.0, 1) : differenceColour;

				fixed4 contrastColour = BLACK_COLOUR;
				fixed4 minorContourColour = MINOR_CONTOUR_COLOUR;

				fixed4 textColor = (1 - contourMapFrag.textIntensity) * differenceColour +
					contourMapFrag.textIntensity * contrastColour;

				fixed4 finalColor = drawMajorContourLine == 1 ? contrastColour : differenceColour;
				finalColor = drawMinorContourLine == 1 ? minorContourColour : finalColor;
				finalColor = contourMapFrag.onText == 1 ? textColor : finalColor;

				return finalColor;
			}
			ENDCG
		}
	}
}
