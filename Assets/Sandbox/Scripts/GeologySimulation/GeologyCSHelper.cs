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
using ARSandbox.GeologySimulation.GeologicalTransforms;

namespace ARSandbox.GeologySimulation
{
    public static class ComputeShaderHelper
    {
        public const string CS_COMPUTE_GEOLOGY_SURFACE = "CS_ComputeGeologySurface";
        public const string CS_FILL_RT = "CS_FillRT";
        public const string CS_COMPUTE_GEOLOGY_SLICE = "CS_ComputeGeologySlice";
        public const string CS_RASTERISE_GEOLOGY_SURFACE = "CS_RasteriseGeologySurface";

        public static readonly Point CS_COMPUTE_GEOLOGY_SURFACE_THREADS = new Point(16, 16);
        public static readonly Point CS_RASTERISE_GEOLOGY_SURFACE_THREADS = new Point(16, 16);
        public static readonly Point CS_FILL_RT_THREADS = new Point(16, 16);

        public static void Run_ComputeGeologySurface(ComputeShader geologyShader, Texture sandboxDepthsRT, RenderTexture geologySurfaceRT, Vector2 simulationDimensions,
                                                     GeologicalTransform_CSS[] transforms, GeologicalLayer_CSS[] layers, float layerStartDepth)
        {
            if (layers.Length > 0 && transforms.Length > 0)
            {
                int kernelHandle = geologyShader.FindKernel(CS_COMPUTE_GEOLOGY_SURFACE);
                geologyShader.SetTexture(kernelHandle, "SandboxDepthsRT", sandboxDepthsRT);
                geologyShader.SetTexture(kernelHandle, "GeologySurfaceRT", geologySurfaceRT);

                int inputTexSizeX = sandboxDepthsRT.width;
                int inputTexSizeY = sandboxDepthsRT.height;

                int outputTexSizeX = geologySurfaceRT.width;
                int outputTexSizeY = geologySurfaceRT.height;

                int[] bufferLengths = new int[2] { transforms.Length, layers.Length };
                geologyShader.SetInts("BufferLengths", bufferLengths);

                int[] textureSizes = new int[4] { inputTexSizeX, inputTexSizeY, outputTexSizeX, outputTexSizeY };
                geologyShader.SetInts("TextureSizes", textureSizes);

                float[] simulationSize = new float[2] { simulationDimensions.x, simulationDimensions.y };
                geologyShader.SetFloats("SimulationSize", simulationSize);

                geologyShader.SetFloat("LayerStartDepth", layerStartDepth);

                ComputeBuffer transformBuffer = new ComputeBuffer(transforms.Length, (int)StructSizes.GEOLOGICAL_TRANSFORM_SIZE);
                transformBuffer.SetData(transforms);
                geologyShader.SetBuffer(kernelHandle, "TransformBuffer", transformBuffer);

                ComputeBuffer layerBuffer = new ComputeBuffer(layers.Length, (int)StructSizes.GEOLOGICAL_LAYER_SIZE);
                layerBuffer.SetData(layers);
                geologyShader.SetBuffer(kernelHandle, "LayerBuffer", layerBuffer);

                Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(outputTexSizeX, outputTexSizeY), CS_COMPUTE_GEOLOGY_SURFACE_THREADS);
                geologyShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);

                transformBuffer.Release();
                transformBuffer.Dispose();
                layerBuffer.Release();
                layerBuffer.Dispose();
            }
        }

        public static void Run_RasteriseGeologySurface(ComputeShader geologyShader, RenderTexture geologySurfaceRT, RenderTexture colouredOutputRT, 
                                                        GeologicalLayer_CSS[] layers, Color[] faultColours, Color[] foldColours)
        {
            if (layers.Length > 0)
            {
                int kernelHandle = geologyShader.FindKernel(CS_RASTERISE_GEOLOGY_SURFACE);
                geologyShader.SetTexture(kernelHandle, "GeologySurfaceRT", geologySurfaceRT);
                geologyShader.SetTexture(kernelHandle, "ColouredOutputRT", colouredOutputRT);
                geologyShader.SetTexture(kernelHandle, "PatternsTex2DArr", GeologicalLayerTextures.LayerPatternTexArray);

                int outputTexSizeX = geologySurfaceRT.width;
                int outputTexSizeY = geologySurfaceRT.height;

                int[] bufferLengths = new int[2] { 0, layers.Length };
                geologyShader.SetInts("BufferLengths", bufferLengths);

                geologyShader.SetInt("TotalFaults", faultColours.Length);
                geologyShader.SetFloat("FaultLineWidth", 5);

                ComputeBuffer faultColourBuffer = new ComputeBuffer(faultColours.Length, (int)StructSizes.GEOLOGICAL_COLOUR_BUFFER_SIZE);
                faultColourBuffer.SetData(faultColours);
                geologyShader.SetBuffer(kernelHandle, "FaultColourBuffer", faultColourBuffer);

                geologyShader.SetInt("TotalFolds", foldColours.Length / 2);
                geologyShader.SetFloat("FoldLineWidth", 5);

                ComputeBuffer foldColourBuffer = new ComputeBuffer(foldColours.Length, (int)StructSizes.GEOLOGICAL_COLOUR_BUFFER_SIZE);
                foldColourBuffer.SetData(foldColours);
                geologyShader.SetBuffer(kernelHandle, "FoldColourBuffer", foldColourBuffer);

                ComputeBuffer layerBuffer = new ComputeBuffer(layers.Length, (int)StructSizes.GEOLOGICAL_LAYER_SIZE);
                layerBuffer.SetData(layers);
                geologyShader.SetBuffer(kernelHandle, "LayerBuffer", layerBuffer);

                Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(outputTexSizeX, outputTexSizeY), CS_RASTERISE_GEOLOGY_SURFACE_THREADS);
                geologyShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);

                faultColourBuffer.Release();
                faultColourBuffer.Dispose();
                foldColourBuffer.Release();
                foldColourBuffer.Dispose();
                layerBuffer.Release();
                layerBuffer.Dispose();
            }
        }

        public static void Run_ComputeGeologySlice(ComputeShader geologyShader, RenderTexture sandboxDepthsRT, RenderTexture geologySurfaceRT,
                                                     GeologicalTransform_CSS[] transforms, GeologicalLayer_CSS[] layers, float layerStartDepth,
                                                     Vector2 startPoint, Vector2 endPoint)
        {
            int kernelHandle = geologyShader.FindKernel(CS_COMPUTE_GEOLOGY_SLICE);
            geologyShader.SetTexture(kernelHandle, "SandboxDepthsRT", sandboxDepthsRT);
            geologyShader.SetTexture(kernelHandle, "GeologySurfaceRT", geologySurfaceRT);
            
            int inputTexSizeX = sandboxDepthsRT.width;
            int inputTexSizeY = sandboxDepthsRT.height;

            int outputTexSizeX = geologySurfaceRT.width;
            int outputTexSizeY = geologySurfaceRT.height;

            int[] bufferLengths = new int[2] { transforms.Length, layers.Length };
            geologyShader.SetInts("BufferLengths", bufferLengths);

            int[] textureSizes = new int[4] { inputTexSizeX, inputTexSizeY, outputTexSizeX, outputTexSizeY };
            geologyShader.SetInts("TextureSizes", textureSizes);

            float[] slicePoints = new float[4] { startPoint.x, startPoint.y, endPoint.x, endPoint.y };
            geologyShader.SetFloats("SlicePoints", slicePoints);

            geologyShader.SetFloat("LayerStartDepth", layerStartDepth);

            ComputeBuffer transformBuffer = new ComputeBuffer(transforms.Length, (int)StructSizes.GEOLOGICAL_TRANSFORM_SIZE);
            transformBuffer.SetData(transforms);
            geologyShader.SetBuffer(kernelHandle, "TransformBuffer", transformBuffer);

            ComputeBuffer layerBuffer = new ComputeBuffer(layers.Length, (int)StructSizes.GEOLOGICAL_LAYER_SIZE);
            layerBuffer.SetData(layers);
            geologyShader.SetBuffer(kernelHandle, "LayerBuffer", layerBuffer);

            
            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(outputTexSizeX, outputTexSizeY), CS_COMPUTE_GEOLOGY_SURFACE_THREADS);
            geologyShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);

            transformBuffer.Dispose();
            layerBuffer.Dispose();
        }

        public static void Run_FillRenderTexture(ComputeShader geologyShader, RenderTexture bufferRT, float fillValue)
        {
            int kernelHandle = geologyShader.FindKernel(CS_FILL_RT);
            geologyShader.SetTexture(kernelHandle, "BufferRT", bufferRT);

            int texSizeX = bufferRT.width;
            int texSizeY = bufferRT.height;

            geologyShader.SetFloat("FillValue", fillValue);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_FILL_RT_THREADS);
            geologyShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

    }
}