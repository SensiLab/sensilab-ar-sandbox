//  
//  WaterSimulation.cs
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARSandbox.WaterSimulation
{
    public class WaterSimulation : MonoBehaviour
    {
        public Sandbox Sandbox;
        public HandInput HandInput;
        public CalibrationManager CalibrationManager;
        public WaterDroplet WaterDroplet;
        public Camera MetaballCamera;
        public Shader MetaballShader;
        public ComputeShader WaterSurfaceComputeShader;
        public Texture2D WaterColorTexture;

        private SandboxDescriptor sandboxDescriptor;
        private List<WaterDroplet> waterDroplets;
        private RenderTexture metaballRT;
        private int currSubsection;
        private bool showParticles;

        private RenderTexture waterBufferRT0;
        private RenderTexture waterBufferRT1;
        private bool swapBuffers;

        private IEnumerator RunSimulationCoroutine;
        private bool initialised;

        private const int MaxMetaballs = 2000;

        void InitialiseSimulation()
        {
            if (!initialised)
            {
                waterDroplets = new List<WaterDroplet>();
                currSubsection = 0;
                showParticles = false;

                CreateWaterSurfaceRenderTextures();
                swapBuffers = false;
            }
            initialised = true;
        }

        IEnumerator RunSimulation()
        {
            while (true)
            {
                CullStrayMetaballs();

                KeepMetaballsAboveSandbox();

                StepWaterSurfaceSimulation();
                if (Random.value < 2 / 60.0f) DisturbWaterSurfaceSimulation();

                yield return new WaitForSeconds(1 / 60.0f);
            }
        }

        void OnEnable()
        {
            InitialiseSimulation();

            HandInput.OnGesturesReady += OnGesturesReady;
            CalibrationManager.OnCalibration += OnCalibration;
            Sandbox.OnSandboxReady += OnSandboxReady;
            sandboxDescriptor = Sandbox.GetSandboxDescriptor();

            SetUpMetaballCamera();
            MetaballCamera.gameObject.SetActive(true);

            StartCoroutine(RunSimulationCoroutine = RunSimulation());
        }

        void OnDisable()
        {
            HandInput.OnGesturesReady -= OnGesturesReady;
            CalibrationManager.OnCalibration -= OnCalibration;
            Sandbox.OnSandboxReady -= OnSandboxReady;
            Sandbox.SetDefaultShader();
            MetaballCamera.gameObject.SetActive(false);

            DestroyWaterDroplets();

            StopCoroutine(RunSimulationCoroutine);
        }

        private void OnCalibration()
        {
            HandInput.OnGesturesReady -= OnGesturesReady;
            MetaballCamera.gameObject.SetActive(false);
            DestroyWaterDroplets();
            Sandbox.SetDefaultShader();

            StopCoroutine(RunSimulationCoroutine);
        }

        private void OnSandboxReady()
        {
            InitialiseSimulation();

            HandInput.OnGesturesReady += OnGesturesReady;
            SetUpMetaballCamera();
            sandboxDescriptor = Sandbox.GetSandboxDescriptor();

            StartCoroutine(RunSimulationCoroutine = RunSimulation());
        }

        private void CreateWaterSurfaceRenderTextures()
        {
            waterBufferRT0 = new RenderTexture(256, 256, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            waterBufferRT0.filterMode = FilterMode.Bilinear;
            waterBufferRT0.wrapMode = TextureWrapMode.Repeat;
            waterBufferRT0.enableRandomWrite = true;
            waterBufferRT0.Create();

            WaterSurfaceCSHelper.Run_FillRenderTexture(WaterSurfaceComputeShader, waterBufferRT0, 0.5f);

            waterBufferRT1 = new RenderTexture(256, 256, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            waterBufferRT1.filterMode = FilterMode.Bilinear;
            waterBufferRT1.wrapMode = TextureWrapMode.Repeat;
            waterBufferRT1.enableRandomWrite = true;
            waterBufferRT1.Create();

            WaterSurfaceCSHelper.Run_FillRenderTexture(WaterSurfaceComputeShader, waterBufferRT1, 0.5f);
        }

        private void StepWaterSurfaceSimulation()
        {
            if (swapBuffers)
            {
                WaterSurfaceCSHelper.Run_StepWaterSim(WaterSurfaceComputeShader, waterBufferRT0, waterBufferRT1, 1 / 60f, 1, 20, 0.999f, true);
                Sandbox.SetShaderTexture("_WaterSurfaceTex", waterBufferRT0);
            }
            else
            {
                WaterSurfaceCSHelper.Run_StepWaterSim(WaterSurfaceComputeShader, waterBufferRT1, waterBufferRT0, 1 / 60f, 1, 20, 0.999f, true);
                Sandbox.SetShaderTexture("_WaterSurfaceTex", waterBufferRT1);
            }
            swapBuffers = !swapBuffers;
        }

        private void DisturbWaterSurfaceSimulation()
        {
            Vector2 point0 = new Vector2(32 + Random.value * 224.0f, 32 + Random.value * 224.0f);
            Vector2 point1 = new Vector2(32 + Random.value * 224.0f, 32 + Random.value * 224.0f);
            Vector2 point2 = new Vector2(32 + Random.value * 224.0f, 32 + Random.value * 224.0f);
            Vector2 point3 = new Vector2(32 + Random.value * 224.0f, 32 + Random.value * 224.0f);

            Vector2[] centres = new Vector2[4] { point0, point1, point2, point3 };
            float[] radii = new float[4] { 5, 5, 10, 10 };
            float[] powers = new float[4] { 0.1f * Random.value, -0.1f * Random.value, 0.25f, -0.25f };

            WaterSurfaceCSHelper.Run_DisplaceWater(WaterSurfaceComputeShader, waterBufferRT0, waterBufferRT1, centres, radii, powers, 2);
        }

        private void KeepMetaballsAboveSandbox()
        {
            if (currSubsection == Sandbox.COLL_MESH_DELAY - 1)
            {
                if (waterDroplets.Count < 200)
                {
                    foreach (WaterDroplet droplet in waterDroplets)
                    {
                        float sandboxDepth = Sandbox.GetDepthFromWorldPos(droplet.transform.position);
                        if (droplet.transform.position.z > sandboxDepth)
                        {
                            droplet.SetZPosition(sandboxDepth - WaterDroplet.DROPLET_RADIUS * 1.25f);
                        }
                    }
                }

                currSubsection = 0;
            }
            else
            {
                if (waterDroplets.Count >= 200)
                {
                    int dropletStep = waterDroplets.Count / Sandbox.COLL_MESH_DELAY;
                    for (int i = currSubsection * dropletStep; i < (currSubsection + 1) * dropletStep; i++)
                    {
                        if (i < waterDroplets.Count)
                        {
                            WaterDroplet droplet = waterDroplets[i];
                            float sandboxDepth = Sandbox.GetDepthFromWorldPos(droplet.transform.position);
                            // Constant of 10 is a small buffer to allow stacked water to rest.
                            if (droplet.transform.position.z > sandboxDepth - WaterDroplet.DROPLET_RADIUS + 10)
                            {
                                droplet.SetZPosition(sandboxDepth - WaterDroplet.DROPLET_RADIUS);
                            }
                        }
                    }
                }
                currSubsection += 1;
            }
        }

        private void CullStrayMetaballs()
        {
            float minX = sandboxDescriptor.MeshStart.x - 5;
            float minY = sandboxDescriptor.MeshStart.y - 5;
            float maxX = sandboxDescriptor.MeshEnd.x + 5;
            float maxY = sandboxDescriptor.MeshEnd.y + 5;
            int metaballCount = 0;

            for (int i = waterDroplets.Count - 1; i >= 0; i--)
            {
                WaterDroplet droplet = waterDroplets[i];
                Vector3 position = droplet.transform.position;
                if (position.x < minX || position.y < minY || position.x > maxX || position.y > maxY)
                {
                    Destroy(droplet.gameObject);
                    Destroy(droplet);
                    waterDroplets.RemoveAt(i);
                } else
                {
                    metaballCount++;
                    if (metaballCount > MaxMetaballs)
                    {
                        Destroy(droplet.gameObject);
                        Destroy(droplet);
                        waterDroplets.RemoveAt(i);
                    }
                }
            }
        }
        private void SetUpMetaballCamera()
        {
            CalibrationManager.SetUpDataCamera(MetaballCamera);
            CreateMetaballRT();
            MetaballCamera.targetTexture = metaballRT;
            Sandbox.SetSandboxShader(MetaballShader);
            Sandbox.SetShaderTexture("_MetaballTex", metaballRT);
            Sandbox.SetShaderTexture("_WaterColorTex", WaterColorTexture);

            Vector2 waterSurfaceTexScaling = new Vector2((float)sandboxDescriptor.DataSize.x / (float)sandboxDescriptor.DataSize.y * 1f, 1f);
            Sandbox.SetTextureProperties("_WaterSurfaceTex", Vector2.zero, waterSurfaceTexScaling);
        }

        private void CreateMetaballRT()
        {
            float aspectRatio = (float)sandboxDescriptor.DataSize.x / (float)sandboxDescriptor.DataSize.y;
            if (metaballRT != null)
            {
                metaballRT.DiscardContents();
                metaballRT.Release();
            }

            metaballRT = new RenderTexture((int)(256.0f * aspectRatio), 256, 0);
        }

        private void OnGesturesReady()
        {
            foreach (HandInputGesture gesture in HandInput.GetCurrentGestures())
            {
                if (!gesture.OutOfBounds)
                {
                    if (!Physics.CheckSphere(gesture.WorldPosition + new Vector3(0, 0, -5), 1.0f))
                    {
                        WaterDroplet waterDroplet = Instantiate(WaterDroplet, gesture.WorldPosition, Quaternion.identity);
                        waterDroplet.SetShowMesh(showParticles);
                        waterDroplets.Add(waterDroplet);
                    }
                }
            }
        }

        private void DestroyWaterDroplets()
        {
            foreach (WaterDroplet droplet in waterDroplets)
            {
                Destroy(droplet);
            }
            waterDroplets.Clear();
        }

        public void UI_DestroyWaterDroplets()
        {
            DestroyWaterDroplets();
        }

        public void UI_ToggleShowParticles(bool showParticles)
        {
            this.showParticles = showParticles;
            foreach (WaterDroplet droplet in waterDroplets)
            {
                droplet.SetShowMesh(showParticles);
            }
        }
    }
}
