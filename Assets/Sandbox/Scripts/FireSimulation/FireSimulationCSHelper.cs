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

namespace ARSandbox.FireSimulation
{
    public struct CSS_FireStartPoint
    {
        public Point Position;
        public int Radius;
    }

    public struct CSS_FireCellMaterial
    {
        public float BurnRate;
        public float BurnoutTime;
        public Color Colour;
        public Vector4 BurntColour;
    };

    public struct WindCoefficients
    {
        public float N;
        public float E;
        public float S;
        public float W;
        public float NE;
        public float SE;
        public float SW;
        public float NW;
    };

    public enum StructSizes
    {
        FIRE_START_POINT_SIZE = 12,
        FIRE_CELL_MATERIAL_SIZE = 40
    }

    public static class FireSimulationCSHelper
    {
        public const string CS_GENERATE_LANDSCAPE = "CS_GenerateLandscape";
        public const string CS_START_FIRE = "CS_StartFire";
        public const string CS_STEP_FIRE_SIMULATION = "CS_StepFireSimulation";
        public const string CS_RASTERISE_FIRE_SIMULATION = "CS_RasteriseFireSimulation";
        public const string CS_RESET_LANDSCAPE = "CS_ResetLandscape";

        public static readonly Point CS_GENERATE_LANDSCAPE_THREADS = new Point(16, 16);
        public static readonly Point CS_START_FIRE_THREADS = new Point(16, 16);
        public static readonly Point CS_STEP_FIRE_SIMULATION_THREADS = new Point(16, 16);
        public static readonly Point CS_RASTERISE_FIRE_SIMULATION_THREADS = new Point(16, 16);
        public static readonly Point CS_RESET_LANDSCAPE_THREADS = new Point(16, 16);

        public static void Run_RasteriseFireSimulation(ComputeShader fireSimulationShader, RenderTexture fireLandscapeRT,
                                                        RenderTexture fireRasterisedRT, CSS_FireCellMaterial[] fireCellMaterials)
        {
            int kernelHandle = fireSimulationShader.FindKernel(CS_RASTERISE_FIRE_SIMULATION);
            fireSimulationShader.SetTexture(kernelHandle, "FireLandscapeRT", fireLandscapeRT);
            fireSimulationShader.SetTexture(kernelHandle, "FireRasterisedRT", fireRasterisedRT);

            int texSizeX = fireLandscapeRT.width;
            int texSizeY = fireLandscapeRT.height;
            int[] textureSize = new int[2] { texSizeX, texSizeY };
            fireSimulationShader.SetInts("FireLandscapeSize", textureSize);

            ComputeBuffer fireCellMaterialsBuffer = new ComputeBuffer(fireCellMaterials.Length, (int)StructSizes.FIRE_CELL_MATERIAL_SIZE);
            fireCellMaterialsBuffer.SetData(fireCellMaterials);
            fireSimulationShader.SetBuffer(kernelHandle, "FireCellMaterialsBuffer", fireCellMaterialsBuffer);
            fireSimulationShader.SetInt("TotalFireCellMaterials", fireCellMaterials.Length);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_RASTERISE_FIRE_SIMULATION_THREADS);
            fireSimulationShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);

            fireCellMaterialsBuffer.Dispose();
        }

        public static void Run_StepFireSimulation(ComputeShader fireSimulationShader, RenderTexture sandboxDepthsRT, RenderTexture prevStepRT,
                                                    RenderTexture nextStepRT, CSS_FireCellMaterial[] fireCellMaterials,
                                                    WindCoefficients windCoefficients, float sizeFactor)
        {
            int kernelHandle = fireSimulationShader.FindKernel(CS_STEP_FIRE_SIMULATION);
            fireSimulationShader.SetTexture(kernelHandle, "SandboxDepthsRT", sandboxDepthsRT);
            fireSimulationShader.SetTexture(kernelHandle, "PrevStepRT", prevStepRT);
            fireSimulationShader.SetTexture(kernelHandle, "FireLandscapeRT", nextStepRT);

            int texSizeX = nextStepRT.width;
            int texSizeY = nextStepRT.height;
            int[] textureSize = new int[2] { texSizeX, texSizeY };
            fireSimulationShader.SetInts("FireLandscapeSize", textureSize);

            ComputeBuffer fireCellMaterialsBuffer = new ComputeBuffer(fireCellMaterials.Length, (int)StructSizes.FIRE_CELL_MATERIAL_SIZE);
            fireCellMaterialsBuffer.SetData(fireCellMaterials);
            fireSimulationShader.SetBuffer(kernelHandle, "FireCellMaterialsBuffer", fireCellMaterialsBuffer);
            fireSimulationShader.SetInt("TotalFireCellMaterials", fireCellMaterials.Length);

            float[] windCoefficients0 = new float[] { windCoefficients.N, windCoefficients.E,
                                                    windCoefficients.S, windCoefficients.W };
            float[] windCoefficients1 = new float[] { windCoefficients.NE, windCoefficients.SE,
                                                    windCoefficients.SW, windCoefficients.SW };
            fireSimulationShader.SetFloats("WindCoefficients0", windCoefficients0);
            fireSimulationShader.SetFloats("WindCoefficients1", windCoefficients1);
            fireSimulationShader.SetFloat("SizeFactor", 1.0f / sizeFactor);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_STEP_FIRE_SIMULATION_THREADS);
            fireSimulationShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);

            fireCellMaterialsBuffer.Dispose();
        }

        public static void Run_StartFire(ComputeShader fireSimulationShader, RenderTexture fireLandscapeRT,
                                            CSS_FireStartPoint[] fireStartPoints)
        {
            int kernelHandle = fireSimulationShader.FindKernel(CS_START_FIRE);
            fireSimulationShader.SetTexture(kernelHandle, "FireLandscapeRT", fireLandscapeRT);

            int texSizeX = fireLandscapeRT.width;
            int texSizeY = fireLandscapeRT.height;
            int[] textureSize = new int[2] { texSizeX, texSizeY };
            fireSimulationShader.SetInts("FireLandscapeSize", textureSize);

            ComputeBuffer fireStartPointBuffer = new ComputeBuffer(fireStartPoints.Length, (int)StructSizes.FIRE_START_POINT_SIZE);
            fireStartPointBuffer.SetData(fireStartPoints);
            fireSimulationShader.SetBuffer(kernelHandle, "FireStartPointBuffer", fireStartPointBuffer);
            fireSimulationShader.SetInt("TotalFireStartPoints", fireStartPoints.Length);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_START_FIRE_THREADS);
            fireSimulationShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);

            fireStartPointBuffer.Dispose();
        }

        public static void Run_GenerateLandscape(ComputeShader fireSimulationShader, RenderTexture fireLandscapeRT, float randomSeedOffset, float landscapeZoom)
        {
            int kernelHandle = fireSimulationShader.FindKernel(CS_GENERATE_LANDSCAPE);
            fireSimulationShader.SetTexture(kernelHandle, "FireLandscapeRT", fireLandscapeRT);

            int texSizeX = fireLandscapeRT.width;
            int texSizeY = fireLandscapeRT.height;
            int[] textureSize = new int[2] { texSizeX, texSizeY };
            fireSimulationShader.SetInts("FireLandscapeSize", textureSize);

            fireSimulationShader.SetFloat("RandomSeedOffset", randomSeedOffset);
            fireSimulationShader.SetFloat("LandscapeZoom", landscapeZoom);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_GENERATE_LANDSCAPE_THREADS);
            fireSimulationShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_ResetLandscape(ComputeShader fireSimulationShader, RenderTexture fireLandscapeRT)
        {
            int kernelHandle = fireSimulationShader.FindKernel(CS_RESET_LANDSCAPE);
            fireSimulationShader.SetTexture(kernelHandle, "FireLandscapeRT", fireLandscapeRT);

            int texSizeX = fireLandscapeRT.width;
            int texSizeY = fireLandscapeRT.height;
            int[] textureSize = new int[2] { texSizeX, texSizeY };
            fireSimulationShader.SetInts("FireLandscapeSize", textureSize);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_RESET_LANDSCAPE_THREADS);
            fireSimulationShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }
    }
}
