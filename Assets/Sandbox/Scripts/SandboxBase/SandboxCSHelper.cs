/*  
 *  This file is part of sensilab-ar-sandbox.
 *
 *  sensilab-ar-sandbox is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 * 
 *  sensilab-ar-sandbox is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 * 
 *  You should have received a copy of the GNU General Public License
 *  along with sensilab-ar-sandbox.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARSandbox
{
    public static class SandboxCSHelper
    {
        public const string CS_LOW_PASS = "CS_LowPassData";
        public const string CS_INITIAL_LOW_PASS = "CS_SetInitialLowPassData";
        public const string CS_DOWNSAMPLE = "CS_DownsampleRT";
        public const string CS_GAUSSIAN_HORI = "CS_GaussianBlurHorizontal";
        public const string CS_GAUSSIAN_VERT = "CS_GaussianBlurVertical";
        public const string CS_GENERATE_MESH = "CS_GeneratePlaneMesh";
        public const string CS_GENERATE_MESH_NO_TRIS = "CS_GeneratePlaneMeshNoTris";
        public const string CS_CONTOUR_SOBEL_FILTER = "CS_ContourSobelFilter";
        public const string CS_CONTOUR_NON_MAXIMAL_SUPRESSION = "CS_ContourNonMaximalSupression";
        public const string CS_CONTOUR_FIND_PATHS = "CS_ContourFindPaths";
        public const string CS_EXTRACT_DEPTH_DATA = "CS_ExtractDepthData";

        public static readonly Point CS_SQUARE_LAYOUT_16 = new Point(16, 16);

        public static void Run_SetInitialLowPassData(ComputeShader sandboxCS, Texture rawDataRT, Point sandboxSize, Texture internalLowPassDataRT,
                                          Texture lowPassCounterRT, Texture lowPassDataRT, float minDepth, float maxDepth)
        {
            int kernelHandle = sandboxCS.FindKernel(CS_INITIAL_LOW_PASS);
            sandboxCS.SetTexture(kernelHandle, "RawDataRT", rawDataRT);
            sandboxCS.SetTexture(kernelHandle, "LowPassDataRT", lowPassDataRT);
            sandboxCS.SetTexture(kernelHandle, "InternalLowPassDataRT", internalLowPassDataRT);
            sandboxCS.SetTexture(kernelHandle, "LowPassCounterRT", lowPassCounterRT);

            int texSizeX = sandboxSize.x;
            int texSizeY = sandboxSize.y;

            float[] lowPassParams0 = new float[4] { texSizeX, texSizeY, 0, 0 };
            sandboxCS.SetFloats("LowPassParams0", lowPassParams0);

            float[] lowPassParams1 = new float[4] { minDepth, maxDepth, 0, 0 };
            sandboxCS.SetFloats("LowPassParams1", lowPassParams1);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_SQUARE_LAYOUT_16);
            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_ComputeLowPassRT(ComputeShader sandboxCS, Texture rawDataRT, Point sandboxSize, Texture internalLowPassDataRT,
                                          Texture lowPassCounterRT, Texture lowPassDataRT, float alpha1, float alpha2,
                                          float minDepth, float maxDepth, float noiseTolerance, float lowPassHoldTime)
        {
            int kernelHandle = sandboxCS.FindKernel(CS_LOW_PASS);
            sandboxCS.SetTexture(kernelHandle, "RawDataRT", rawDataRT);
            sandboxCS.SetTexture(kernelHandle, "InternalLowPassDataRT", internalLowPassDataRT);
            sandboxCS.SetTexture(kernelHandle, "LowPassCounterRT", lowPassCounterRT);
            sandboxCS.SetTexture(kernelHandle, "LowPassDataRT", lowPassDataRT);

            int texSizeX = sandboxSize.x;
            int texSizeY = sandboxSize.y;

            float[] lowPassParams0 = new float[4] { texSizeX, texSizeY, alpha1, alpha2 };
            sandboxCS.SetFloats("LowPassParams0", lowPassParams0);

            float[] lowPassParams1 = new float[4] { minDepth, maxDepth, noiseTolerance, lowPassHoldTime };
            sandboxCS.SetFloats("LowPassParams1", lowPassParams1);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_SQUARE_LAYOUT_16);
            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_BlurRT(ComputeShader sandboxCS, Texture unblurredDataRT, Texture tempRT, Texture blurredDataRT)
        {
            // Horizontal Pass
            int kernelHandle = sandboxCS.FindKernel(CS_GAUSSIAN_HORI);
            sandboxCS.SetTexture(kernelHandle, "UnblurredDataRT", unblurredDataRT);
            sandboxCS.SetTexture(kernelHandle, "BlurredDataRT", tempRT);

            int texSizeX = unblurredDataRT.width;
            int texSizeY = unblurredDataRT.height;
            sandboxCS.SetFloats("BlurTextureSize", new float[2] { texSizeX, texSizeY });

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_SQUARE_LAYOUT_16);
            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);

            // Vertical Pass
            kernelHandle = sandboxCS.FindKernel(CS_GAUSSIAN_VERT);
            sandboxCS.SetTexture(kernelHandle, "UnblurredDataRT", tempRT);
            sandboxCS.SetTexture(kernelHandle, "BlurredDataRT", blurredDataRT);
            sandboxCS.SetFloats("BlurTextureSize", new float[2] { texSizeX, texSizeY });

            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_DownsampleRT(ComputeShader sandboxCS, Texture originalRT, Texture downsampledRT)
        {
            int kernelHandle = sandboxCS.FindKernel(CS_DOWNSAMPLE);

            int texSizeOrig_X = originalRT.width;
            int texSizeOrig_Y = originalRT.height;

            int texSizeDS_X = downsampledRT.width;
            int texSizeDS_Y = downsampledRT.height;

            int[] downsampleParams = new int[4] { texSizeOrig_X, texSizeOrig_Y, texSizeDS_X, texSizeDS_Y };
            sandboxCS.SetInts("DownsampleParams", downsampleParams);

            sandboxCS.SetTexture(kernelHandle, "RawDataRT", originalRT);
            sandboxCS.SetTexture(kernelHandle, "DownsampledDataRT", downsampledRT);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeDS_X, texSizeDS_Y), CS_SQUARE_LAYOUT_16);
            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_ContourSobelFilter(ComputeShader sandboxCS, Texture contourRT, Texture sobelRT)
        {
            int kernelHandle = sandboxCS.FindKernel(CS_CONTOUR_SOBEL_FILTER);

            int texSizeX = contourRT.width;
            int texSizeY = contourRT.height;

            int[] texSize = new int[2] { texSizeX, texSizeY };
            sandboxCS.SetInts("ContourTextureSize", texSize);

            sandboxCS.SetTexture(kernelHandle, "ContourRT", contourRT);
            sandboxCS.SetTexture(kernelHandle, "SobelRT", sobelRT);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_SQUARE_LAYOUT_16);
            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_ContourNonMaximalSupression(ComputeShader sandboxCS, Texture sobelInputRT, Texture maximalContourRT)
        {
            int kernelHandle = sandboxCS.FindKernel(CS_CONTOUR_NON_MAXIMAL_SUPRESSION);

            int texSizeX = sobelInputRT.width;
            int texSizeY = sobelInputRT.height;

            int[] texSize = new int[2] { texSizeX, texSizeY };
            sandboxCS.SetInts("ContourTextureSize", texSize);

            sandboxCS.SetTexture(kernelHandle, "SobelInputRT", sobelInputRT);
            sandboxCS.SetTexture(kernelHandle, "MaximalContourRT", maximalContourRT);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_SQUARE_LAYOUT_16);
            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }
        public static void Run_ContourFindPaths(ComputeShader sandboxCS, Texture maximalContourInputRT, ComputeBuffer contourPathsBuffer)
        {
            int kernelHandle = sandboxCS.FindKernel(CS_CONTOUR_FIND_PATHS);

            int texSizeX = maximalContourInputRT.width;
            int texSizeY = maximalContourInputRT.height;

            int[] texSize = new int[2] { texSizeX, texSizeY };
            sandboxCS.SetInts("ContourTextureSize", texSize);

            sandboxCS.SetTexture(kernelHandle, "MaximalContourInputRT", maximalContourInputRT);
            sandboxCS.SetBuffer(kernelHandle, "ContourPathsBuffer", contourPathsBuffer);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_SQUARE_LAYOUT_16);
            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }
        public static void Run_GenerateSandboxMesh(ComputeShader sandboxCS, Texture dataToMeshRT, ComputeBuffer meshVertices_Buffer,
                                                    ComputeBuffer meshUV_Buffer, Vector3 meshOffset, Vector2 meshStride, float meshZScale)
        {
            int meshWidth = dataToMeshRT.width;
            int meshHeight = dataToMeshRT.height;

            int kernelHandle = sandboxCS.FindKernel(CS_GENERATE_MESH_NO_TRIS);
            sandboxCS.SetTexture(kernelHandle, "DataToMeshRT", dataToMeshRT);

            sandboxCS.SetBuffer(kernelHandle, "PlaneMeshVert", meshVertices_Buffer);
            sandboxCS.SetBuffer(kernelHandle, "PlaneMeshUV", meshUV_Buffer);

            sandboxCS.SetFloats("MeshWorldPos", new float[3] { meshOffset.x, meshOffset.y, meshOffset.z });
            sandboxCS.SetFloats("MeshParams", new float[3] { meshZScale, meshWidth, meshHeight });
            sandboxCS.SetFloats("MeshStride", new float[2] { meshStride.x, meshStride.y });

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(meshWidth, meshHeight), CS_SQUARE_LAYOUT_16);

            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_GenerateSandboxMesh(ComputeShader sandboxCS, Texture dataToMeshRT, ComputeBuffer meshVertices_Buffer,
                                                    ComputeBuffer meshUV_Buffer, ComputeBuffer meshTris_Buffer, Vector3 meshOffset,
                                                    Vector2 meshStride, float meshZScale)
        {
            int meshWidth = dataToMeshRT.width;
            int meshHeight = dataToMeshRT.height;

            int kernelHandle = sandboxCS.FindKernel(CS_GENERATE_MESH);
            sandboxCS.SetTexture(kernelHandle, "DataToMeshRT", dataToMeshRT);

            sandboxCS.SetBuffer(kernelHandle, "PlaneMeshVert", meshVertices_Buffer);
            sandboxCS.SetBuffer(kernelHandle, "PlaneMeshUV", meshUV_Buffer);
            sandboxCS.SetBuffer(kernelHandle, "PlaneMeshTriangles", meshTris_Buffer);

            sandboxCS.SetFloats("MeshWorldPos", new float[3] { meshOffset.x, meshOffset.y, meshOffset.z });
            sandboxCS.SetFloats("MeshParams", new float[3] { meshZScale, meshWidth, meshHeight });
            sandboxCS.SetFloats("MeshStride", new float[2] { meshStride.x, meshStride.y });

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(meshWidth, meshHeight), CS_SQUARE_LAYOUT_16);

            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static int[] Run_ExtractDepthData(ComputeShader sandboxCS, Texture depthDataRT)
        {
            int meshWidth = depthDataRT.width;
            int meshHeight = depthDataRT.height;
            int totalDataPoints = meshWidth * meshHeight;
            int[] extractedDepthData = new int[totalDataPoints];

            int kernelHandle = sandboxCS.FindKernel(CS_EXTRACT_DEPTH_DATA);
            sandboxCS.SetTexture(kernelHandle, "ExtractDataRT", depthDataRT);

            ComputeBuffer extractBuffer = new ComputeBuffer(totalDataPoints, 8);

            sandboxCS.SetBuffer(kernelHandle, "ExtractBuffer", extractBuffer);
            sandboxCS.SetInts("ExtractDataSize", new int[2] { meshWidth, meshHeight });

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(meshWidth, meshHeight), CS_SQUARE_LAYOUT_16);
            sandboxCS.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);

            extractBuffer.GetData(extractedDepthData);
            extractBuffer.Dispose();

            return extractedDepthData;
        }
    }
}
