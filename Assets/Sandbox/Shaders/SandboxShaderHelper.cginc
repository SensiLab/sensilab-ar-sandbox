//  
//  SandboxShaderHelper.cginc
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

#define POW_2_16	65535.0
#define MAJOR_CONTOUR_COLOUR fixed4(0, 0, 0, 1)
#define MINOR_CONTOUR_COLOUR fixed4(0.4, 0.4, 0.4, 1)
#define WHITE_COLOUR fixed4(1, 1, 1, 1)
#define BLACK_COLOUR fixed4(0, 0, 0, 1)

struct ContourMapFrag {
	float normalisedHeight;
	float discreteNormalisedHeight;
	int onMinorContourLine;
	int onMajorContourLine;
	int onText;
	int onTextMask;
	float textIntensity;
};

StructuredBuffer<float3> VertexBuffer;
StructuredBuffer<float2> UVBuffer;
float4x4 Mat_Object2World;
int MeshWidth;

sampler2D _HeightTex;
sampler2D _ColorScaleTex;
sampler2D _LabelMaskTex;

float4 _HeightTex_ST;
float4 _ColorScaleTex_ST;
float4 _LabelMaskTex_ST;

float _ContourStride;
float _ContourWidth;
int _MinorContours;
float _MinDepth;
float _MaxDepth;
int _DynamicLabelColouring;

float GetGrayscaleValue(fixed3 colour) {
	// 0.299 * colour.r + 0.587 * colour.g + 0.114 * colour.b;
	float grayscale = 0.3 * colour.r + 0.587 * colour.g + 0.114 * colour.b;
	return grayscale;
}

ContourMapFrag GetContourMap(v2f i) {
	float contourWidthConversion = 0.0001 * _ContourWidth;
	float2 contourWidth = float2(contourWidthConversion, contourWidthConversion);

	float height00, height10, height01, height11;
	height00 = (float)tex2D(_HeightTex, i.uv_HeightTex);
	height10 = (float)tex2D(_HeightTex, i.uv_HeightTex + float2(contourWidth.x, 0));
	height01 = (float)tex2D(_HeightTex, i.uv_HeightTex + float2(0, contourWidth.y));
	height11 = (float)tex2D(_HeightTex, i.uv_HeightTex + contourWidth*0.7071);
				
	float heightMin = min(min(height00, height11), min(height01, height10));
	float heightMax = max(max(height00, height11), max(height01, height10));
	float heightMinScaled = heightMin * POW_2_16;
	float heightMaxScaled = heightMax * POW_2_16;
	float height00Scaled = height00 * POW_2_16;

	float minorContourFactor = _MinorContours + 1.0;
				
	int minMinorHeightLevel = floor((heightMinScaled - _MaxDepth) * minorContourFactor / _ContourStride);
	int maxMinorHeightLevel = floor((heightMaxScaled - _MaxDepth) * minorContourFactor / _ContourStride);
	int minHeightLevel = floor((heightMinScaled - _MaxDepth) / _ContourStride);
	int maxHeightLevel = floor((heightMaxScaled - _MaxDepth) / _ContourStride);

	int onMajorContourLine = minHeightLevel != maxHeightLevel;
	int onMinorContourLine = !onMajorContourLine && minMinorHeightLevel != maxMinorHeightLevel;

	float depthRange = _MaxDepth - _MinDepth;
	float normalisedHeight = 1 - (height00Scaled - _MinDepth) / depthRange;	

	float contourIntervalDepth = floor((height00Scaled - _MaxDepth) / _ContourStride) * _ContourStride + _MaxDepth;
	float discreteNormalisedHeight = 1 - (contourIntervalDepth - _MinDepth) / depthRange;
	
	float2 maskSample = tex2D(_LabelMaskTex, i.uv_LabelMaskTex);

	ContourMapFrag contourMapFrag;
	contourMapFrag.discreteNormalisedHeight = discreteNormalisedHeight;
	contourMapFrag.normalisedHeight = normalisedHeight;
	contourMapFrag.onMajorContourLine = onMajorContourLine;
	contourMapFrag.onMinorContourLine = onMinorContourLine;
	contourMapFrag.onText = maskSample.g != 0;
	contourMapFrag.onTextMask = maskSample.r == 1;
	contourMapFrag.textIntensity = maskSample.g;

	return contourMapFrag;
}

uint GetVertexID(uint id) {
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
	}

	return vI;
}