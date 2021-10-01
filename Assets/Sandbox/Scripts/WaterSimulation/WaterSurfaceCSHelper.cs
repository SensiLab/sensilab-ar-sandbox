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

namespace ARSandbox.WaterSimulation
{
    public class WaterSurfaceCSHelper
    {
        public const string CS_STEP_WATER_SIM = "CS_StepWaterSim";
        public const string CS_DISPLACE_WATER = "CS_DisplaceWater";
        public const string CS_ADD_NOISE = "CS_AddNoise";
        public const string CS_FILL_RT = "CS_FillRT";

        public static readonly Point CS_STEP_WATER_SIM_THREADS = new Point(16, 16);
        public static readonly Point CS_DISPLACE_WATER_THREADS = new Point(16, 16);
        public static readonly Point CS_ADD_NOISE_THREADS = new Point(16, 16);
        public static readonly Point CS_FILL_RT_THREADS = new Point(16, 16);

        public static void Run_StepWaterSim(ComputeShader waterSurfaceShader, RenderTexture waterBufferRT0, RenderTexture waterBufferRT1,
                                             float deltaT, float deltaX, float waveSpeed, float dampingConst, bool wrapAround)
        {
            int kernelHandle = waterSurfaceShader.FindKernel(CS_STEP_WATER_SIM);
            waterSurfaceShader.SetTexture(kernelHandle, "WaterBufferPrev", waterBufferRT0);
            waterSurfaceShader.SetTexture(kernelHandle, "WaterBufferCurr", waterBufferRT1);

            int texSizeX = waterBufferRT0.width;
            int texSizeY = waterBufferRT0.height;
            int wrapAroundToggle = wrapAround ? 1 : 0;

            int[] waterSimIntParams = new int[3] { texSizeX, texSizeY, wrapAroundToggle };
            waterSurfaceShader.SetInts("WaterIntParams", waterSimIntParams);

            float alpha = (waveSpeed * deltaT) / deltaX;
            alpha *= alpha;
            float[] waterSimParams = new float[2] { alpha, dampingConst };
            waterSurfaceShader.SetFloats("WaterSimParams", waterSimParams);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_STEP_WATER_SIM_THREADS);
            waterSurfaceShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_FillRenderTexture(ComputeShader waterSurfaceShader, RenderTexture bufferRT, float fillValue)
        {
            int kernelHandle = waterSurfaceShader.FindKernel(CS_FILL_RT);
            waterSurfaceShader.SetTexture(kernelHandle, "BufferRT", bufferRT);

            int texSizeX = bufferRT.width;
            int texSizeY = bufferRT.height;

            waterSurfaceShader.SetFloat("FillValue", fillValue);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_FILL_RT_THREADS);
            waterSurfaceShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_AddNoise(ComputeShader waterSurfaceShader, RenderTexture noiseRT0, RenderTexture noiseRT1)
        {
            int kernelHandle = waterSurfaceShader.FindKernel(CS_ADD_NOISE);
            waterSurfaceShader.SetTexture(kernelHandle, "NoiseRT0", noiseRT0);
            waterSurfaceShader.SetTexture(kernelHandle, "NoiseRT1", noiseRT1);
            waterSurfaceShader.SetFloat("Seed", Random.value);

            int texSizeX = noiseRT0.width;
            int texSizeY = noiseRT0.height;

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_ADD_NOISE_THREADS);
            waterSurfaceShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }

        public static void Run_DisplaceWater(ComputeShader waterSurfaceShader, RenderTexture waterBufferRT0, RenderTexture waterBufferRT1,
                                             Vector2[] displacementCentres, float[] displacementRadii, float[] displacementPowers, int totalDisplacements)
        {
            int kernelHandle = waterSurfaceShader.FindKernel(CS_DISPLACE_WATER);
            waterSurfaceShader.SetTexture(kernelHandle, "WaterBufferRT0", waterBufferRT0);
            waterSurfaceShader.SetTexture(kernelHandle, "WaterBufferRT1", waterBufferRT1);

            int texSizeX = waterBufferRT0.width;
            int texSizeY = waterBufferRT0.height;

            float[] waterDisplacementPoints0 = new float[4] { displacementCentres[0].x, displacementCentres[0].y,
                                                        displacementCentres[1].x, displacementCentres[1].y};
            float[] waterDisplacementPoints1 = new float[4] { displacementCentres[2].x, displacementCentres[2].y,
                                                        displacementCentres[3].x, displacementCentres[3].y};
            waterSurfaceShader.SetFloats("WaterDisplacementPoints0", waterDisplacementPoints0);
            waterSurfaceShader.SetFloats("WaterDisplacementPoints1", waterDisplacementPoints1);

            waterSurfaceShader.SetFloats("WaterDisplacementRadii", displacementRadii);
            waterSurfaceShader.SetFloats("WaterDisplacementPower", displacementPowers);

            waterSurfaceShader.SetInt("TotalDisplacements", totalDisplacements);

            Point threadsToRun = ComputeShaderHelpers.CalculateThreadsToRun(new Point(texSizeX, texSizeY), CS_DISPLACE_WATER_THREADS);
            waterSurfaceShader.Dispatch(kernelHandle, threadsToRun.x, threadsToRun.y, 1);
        }
    }
}