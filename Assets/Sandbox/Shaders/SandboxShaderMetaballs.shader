//  
//  SandboxShaderMetaballs.shader
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

Shader "Unlit/SandboxShaderMetaballs"
{
	Properties
	{
		_HeightTex("Height Texture", 2D) = "white" {}
		_ColorScaleTex("Color Texture", 2D) = "white" {}
		_WaterColorTex("Water Color Texture", 2D) = "white" {}
		_LabelMaskTex("Label Mask Texture", 2D) = "white" {}
		_MetaballTex("Metaball Texture", 2D) = "white" {}
		_WaterSurfaceTex("Water Surface Texture", 2D) = "white" {}
		_ContourStride("Contour Stride (mm)", float) = 20
		_ContourWidth("Contour Width", float) = 1
		_MinorContours("Minor Contours", float) = 0
		_MinDepth("Min Depth (mm)", float) = 1000
		_MaxDepth("Max Depth (mm)", float) = 2000
		_DynamicLabelColouring("Dynamic Label Colouring", int) = 1
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
				float2 uv_MetaballTex : TEXCOORD2;
				float2 uv_WaterSurfaceTex : TEXCOORD3;
				float4 vertex : SV_POSITION;
			};

			#include "SandboxShaderHelper.cginc"

			sampler2D _MetaballTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _WaterColorTex;

			float4 _MetaballTex_ST;
			float4 _WaterSurfaceTex_ST;
			float4 _WaterColorTex_ST;

			v2f vert(uint id : SV_VertexID)
			{
				v2f o;
				uint vIndex = GetVertexID(id);

				o.vertex = mul(UNITY_MATRIX_VP, mul(Mat_Object2World, float4(VertexBuffer[vIndex], 1.0f)));
				o.uv_HeightTex = TRANSFORM_TEX(UVBuffer[vIndex], _HeightTex);
				o.uv_LabelMaskTex = TRANSFORM_TEX(UVBuffer[vIndex], _LabelMaskTex);
				o.uv_MetaballTex = TRANSFORM_TEX(UVBuffer[vIndex], _MetaballTex);
				o.uv_WaterSurfaceTex = TRANSFORM_TEX(UVBuffer[vIndex], _WaterSurfaceTex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				ContourMapFrag contourMapFrag = GetContourMap(i);
				
				int onText = contourMapFrag.onText == 1 || contourMapFrag.onTextMask == 1;
				int drawMajorContourLine = contourMapFrag.onMajorContourLine == 1 && onText == 0;
				int drawMinorContourLine = contourMapFrag.onMinorContourLine == 1 && onText == 0;

				fixed4 colorScaleSample = tex2D(_ColorScaleTex, contourMapFrag.discreteNormalisedHeight);

				fixed4 textColor_alt = (1 - contourMapFrag.textIntensity) * colorScaleSample +
					contourMapFrag.textIntensity * BLACK_COLOUR;

				fixed4 finalColor_alt = drawMajorContourLine == 1 ? MAJOR_CONTOUR_COLOUR : colorScaleSample;
				finalColor_alt = drawMinorContourLine == 1 ? MINOR_CONTOUR_COLOUR : finalColor_alt;
				finalColor_alt = contourMapFrag.onText == 1 ? textColor_alt : finalColor_alt;

				fixed4 colourSample_alt = _DynamicLabelColouring ? colorScaleSample : finalColor_alt;

				float metaballValue = tex2D(_MetaballTex, i.uv_MetaballTex).r;
				float waterSurfaceHeight = (float)tex2D(_WaterSurfaceTex, i.uv_WaterSurfaceTex * 5);
				float waterSurfaceHeightLarge = (float)tex2D(_WaterSurfaceTex, i.uv_WaterSurfaceTex);

				float clampHeight = clamp(waterSurfaceHeight, 0.35, 0.6);
				float finalMetaballValue = metaballValue - 0.1 + (clampHeight - 0.35) * 2;
				float clampHeightLarge = clamp(waterSurfaceHeightLarge, 0.3, 0.7);

				fixed4 waterColor = tex2D(_WaterColorTex, float2((waterSurfaceHeightLarge - 0.26) * 6.5f, 0));

				fixed4 mixedColor = colourSample_alt * 0.2 + waterColor * 0.8;
				int inWater = finalMetaballValue > 0.3;

				fixed4 metaballColour = inWater == 1 ? mixedColor : colourSample_alt;

				float greyscale = GetGrayscaleValue(metaballColour);

				fixed4 contrastColour = inWater ? WHITE_COLOUR : BLACK_COLOUR;
				fixed4 minorContourColour = inWater ? WHITE_COLOUR - MINOR_CONTOUR_COLOUR : MINOR_CONTOUR_COLOUR;

				fixed4 textColor = (1 - contourMapFrag.textIntensity) * metaballColour +
					contourMapFrag.textIntensity * contrastColour;

				fixed4 finalColor = drawMajorContourLine == 1 ? contrastColour : metaballColour;
				finalColor = drawMinorContourLine == 1 ? minorContourColour : finalColor;
				finalColor = contourMapFrag.onText == 1 ? textColor : finalColor;

				return _DynamicLabelColouring ? finalColor : metaballColour;
			}
			ENDCG
		}
	}
}
