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

namespace ARSandbox.WindSimulation
{
    public static class ComputeShaderHelper
    {
        public const string CS_INITIALISE_PARTICLES = "CS_InitialiseParticles";
        public const string CS_STEP_PARTICLES = "CS_StepParticles";
        public const string CS_ADD_POLLUTANT = "CS_AddPollutant";

        public static readonly Point CS_SQUARE_LAYOUT_16 = new Point(16, 16);
        public static readonly Point CS_LENGTH_LAYOUT_64 = new Point(64, 1);

        public static void Run_InitialiseParticles(ComputeShader particleShader, ComputeBuffer particleData_Buffer,
                                                        float seed, Vector2 BoundsStart, Vector2 BoundsEnd, int TotalParticles)
        {
            int kernelHandle = particleShader.FindKernel(CS_INITIALISE_PARTICLES);
            particleShader.SetBuffer(kernelHandle, "ParticleBuffer", particleData_Buffer);

            particleShader.SetFloat("RandomSeed", seed);
            particleShader.SetFloats("SimulationBounds", new float[4] { BoundsStart.x, BoundsStart.y, BoundsEnd.x, BoundsEnd.y });

            particleShader.Dispatch(kernelHandle, Mathf.CeilToInt(TotalParticles / (float)CS_LENGTH_LAYOUT_64.x), 1, 1);
        }

        public static void Run_AddPollutant(ComputeShader particleShader, ComputeBuffer particleData_Buffer,
                                                        float seed, Vector2 touchPoint, float radius, int TotalParticles)
        {
            int kernelHandle = particleShader.FindKernel(CS_ADD_POLLUTANT);
            particleShader.SetBuffer(kernelHandle, "ParticleBuffer", particleData_Buffer);

            particleShader.SetFloat("RandomSeed", seed);
            particleShader.SetFloats("PollutantPoint", new float[3] { touchPoint.x, touchPoint.y, radius });

            particleShader.Dispatch(kernelHandle, Mathf.CeilToInt(TotalParticles / (float)CS_LENGTH_LAYOUT_64.x), 1, 1);
        }

        public static void Run_StepParticles(ComputeShader particleShader, ComputeBuffer particleData_Buffer, Texture sandboxDepthsRT, 
                                                bool northernHemisphere, float windSpeedMultiplier, bool coriolisEffectEnabled,
                                                  float seed, Vector2 BoundsStart, Vector2 BoundsEnd, int TotalParticles)
        {
            int kernelHandle = particleShader.FindKernel(CS_STEP_PARTICLES);
            particleShader.SetBuffer(kernelHandle, "ParticleBuffer", particleData_Buffer);

            particleShader.SetTexture(kernelHandle, "SandboxDepthsRT", sandboxDepthsRT);
            particleShader.SetFloat("RandomSeed", seed);
            particleShader.SetFloat("WindSpeedMultiplier", windSpeedMultiplier);
            particleShader.SetInt("NorthernHemisphere", northernHemisphere ? 1 : 0);
            particleShader.SetInt("CoriolisEffectEnabled", coriolisEffectEnabled ? 1 : 0);
            particleShader.SetFloats("SimulationBounds", new float[4] { BoundsStart.x, BoundsStart.y, BoundsEnd.x, BoundsEnd.y });

            particleShader.Dispatch(kernelHandle, Mathf.CeilToInt(TotalParticles / (float)CS_LENGTH_LAYOUT_64.x), 1, 1);
        }
    }
}
