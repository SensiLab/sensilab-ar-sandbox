//  
//  WindSimulation.cs
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
using UnityEngine.Rendering;

namespace ARSandbox.WindSimulation {
    public class WindSimulation : MonoBehaviour
    {
        public CalibrationManager CalibrationManager;
        public Sandbox Sandbox;
        public TopographyLabelManager TopographyLabelManager;
        public HandInput HandInput;
        public Shader WindVisualisationShader;
        public Texture WindColourMap;
        public WindSimulationLabel WindLabelPrefab;
        public Material WindParticleMaterial;
        public ComputeShader WindComputeShader;
        public Camera WindCamera;
        public LayerMask LabelColliderMask;

        public bool WindEnabled { get; private set; }
        public float WindSpeedMultiplier { get; private set; }
        public bool NorthernHemisphere { get; private set; }
        public bool CoriolisEffectEnabled { get; private set; }

        private bool firstRun = true;
        private RenderTexture windParticlesRT;
        private CommandBuffer drawParticlesBuffer;
        private bool commandBufferInitialised;
        private int totalParticles = 300000;
        private ComputeBuffer particleData_Buffer;
        private SandboxDescriptor sandboxDescriptor;
        private IEnumerator RunSimulationCoroutine;
        private List<WindSimulationLabel> ActiveWindLabels;
        private void InitialiseSimulation()
        {
            if (firstRun)
            {
                WindEnabled = true;
                WindSpeedMultiplier = 1;
                NorthernHemisphere = false;
                CoriolisEffectEnabled = true;

                firstRun = false;
            }
            sandboxDescriptor = Sandbox.GetSandboxDescriptor();

            ActiveWindLabels = new List<WindSimulationLabel>();
            TopographyLabelManager.ForceElevationLabels(true, 980, 4, 1000, TopographyLabelManager.ElevationSpacingType.ConstantSpacing);

            CreateRenderTextures();
            Sandbox.SetSandboxShader(WindVisualisationShader);
            Sandbox.SetShaderTexture("_WindColourMap", WindColourMap);
            Sandbox.SetShaderTexture("_WindParticleTex", windParticlesRT);
            Sandbox.SetShaderInt("_WindEnabled", WindEnabled ? 1 : 0);
            WindParticleMaterial.SetFloat("_WindSpeedMultiplier", WindSpeedMultiplier);

            ComputeShaderHelper.Run_InitialiseParticles(WindComputeShader, particleData_Buffer,
                                            100, sandboxDescriptor.MeshStart, sandboxDescriptor.MeshEnd, totalParticles);
            InitialiseWindParticleCamera();

            TopographyLabelManager.OnNewContourLabels += OnNewContourLabels;
            TopographyLabelManager.OnContourLabelsToggled += OnContourLabelsToggled;
            HandInput.OnGesturesReady += OnGesturesReady;
            StartCoroutine(RunSimulationCoroutine = RunSimulation());
        }
        public void InitialiseWindParticleCamera()
        {
            if (!commandBufferInitialised)
            {
                drawParticlesBuffer = new CommandBuffer();
                drawParticlesBuffer.name = "Draw Particles";

                WindCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, drawParticlesBuffer);

                commandBufferInitialised = true;
            }
            CalibrationManager.SetUpDataCamera(WindCamera);
            WindCamera.targetTexture = windParticlesRT;

            drawParticlesBuffer.Clear();

            WindParticleMaterial.SetMatrix("Mat_Object2World", transform.localToWorldMatrix);
            WindParticleMaterial.SetBuffer("ParticleBuffer", particleData_Buffer);
            drawParticlesBuffer.DrawProcedural(Matrix4x4.identity, WindParticleMaterial, 0, MeshTopology.Triangles, totalParticles * 3);
        }
        private void CloseSimulation()
        {
            Sandbox.SetDefaultShader();
            TopographyLabelManager.UnforceElevationLabels();
            DisposeRenderTextures();
            StopCoroutine(RunSimulationCoroutine);
            DestroyLabels();
            TopographyLabelManager.OnNewContourLabels -= OnNewContourLabels;
            TopographyLabelManager.OnContourLabelsToggled -= OnContourLabelsToggled;
            HandInput.OnGesturesReady -= OnGesturesReady;
        }
        private void OnEnable()
        {
            CalibrationManager.OnCalibration += OnCalibration;

            InitialiseSimulation();
        }
        private void OnDisable()
        {
            CalibrationManager.OnCalibration -= OnCalibration;
            Sandbox.OnSandboxReady -= OnSandboxReady;

            CloseSimulation();
        }
        private void OnCalibration()
        {
            Sandbox.OnSandboxReady += OnSandboxReady;

            CloseSimulation();
        }
        // Called only after a successful calibration.
        private void OnSandboxReady()
        {
            InitialiseSimulation();
        }

        private void OnGesturesReady()
        {
            foreach (HandInputGesture gesture in HandInput.GetCurrentGestures())
            {
                if (!gesture.OutOfBounds)
                {
                    ComputeShaderHelper.Run_AddPollutant(WindComputeShader, particleData_Buffer,
                                            Random.value * 1000.0f, gesture.WorldPosition, 3.0f, totalParticles);
                }
            }
        }

        private void CreateRenderTextures()
        {
            Point heightTexSize = sandboxDescriptor.DataSize;
            Point surfaceSize = new Point(1920, (int)(1920.0f * (float)heightTexSize.y / (float)heightTexSize.x));

            windParticlesRT = new RenderTexture(surfaceSize.x, surfaceSize.y, 0, RenderTextureFormat.ARGB32);
            windParticlesRT.enableRandomWrite = true;
            windParticlesRT.filterMode = FilterMode.Bilinear;
            windParticlesRT.Create();

            particleData_Buffer = new ComputeBuffer(totalParticles, 28);

        }
        private void DisposeRenderTextures()
        {
            if (particleData_Buffer != null) particleData_Buffer.Release();

        }
        public void UI_ToggleWindEnabled(bool enabled)
        {
            WindEnabled = enabled;
            Sandbox.SetShaderInt("_WindEnabled", WindEnabled ? 1 : 0);
        }
        public void UI_SetWindSpeedMultiplier(float windSpeedMultiplier)
        {
            WindSpeedMultiplier = windSpeedMultiplier;
        }
        public void UI_SetNorthernHemisphere(bool northernHemisphere)
        {
            NorthernHemisphere = northernHemisphere;
        }
        public void UI_ToggleCoriolisEffect(bool enabled)
        {
            CoriolisEffectEnabled = enabled;
        }
        IEnumerator RunSimulation()
        {
            while (true)
            {
                ComputeShaderHelper.Run_StepParticles(WindComputeShader, particleData_Buffer, Sandbox.CurrentDepthTexture, NorthernHemisphere, WindSpeedMultiplier, CoriolisEffectEnabled,
                                           Random.value * 10000.0f, sandboxDescriptor.MeshStart, sandboxDescriptor.MeshEnd, totalParticles);
                // The speed multiplier is only set here as the scene will render faster than the simulation.
                // If the speed is changed quickly can get weird flickering.
                WindParticleMaterial.SetFloat("_WindSpeedMultiplier", WindSpeedMultiplier);
                yield return new WaitForSeconds(1 / 60.0f);
            }
        }
        private float calculateDepthLevel(float worldDepth)
        {
            float shiftedDepth = sandboxDescriptor.MaxDepth - worldDepth / Sandbox.MESH_Z_SCALE;
            return shiftedDepth / Sandbox.MajorContourSpacing;
        }
        private void DestroyLabels()
        {
            int totalActiveTexts = ActiveWindLabels.Count;
            for (int i = totalActiveTexts - 1; i >= 0; i--)
            {
                Destroy(ActiveWindLabels[i].gameObject);
                Destroy(ActiveWindLabels[i]);
                ActiveWindLabels.RemoveAt(i);
            }
        }
        private void OnContourLabelsToggled(bool toggled)
        {
            if (toggled == false)
            {
                DestroyLabels();
            }
        }
        private void OnNewContourLabels()
        {
            // Need to add something smarter here.
            int totalActiveTexts = ActiveWindLabels.Count;
            for (int i = totalActiveTexts - 1; i >= 0; i--)
            {
                if (ActiveWindLabels[i].AgeLabel())
                {
                    Destroy(ActiveWindLabels[i].gameObject);
                    Destroy(ActiveWindLabels[i]);
                    ActiveWindLabels.RemoveAt(i);
                }
            }

            List<ContourCentreLabelProps>[] possibleLabelPositions
                            = ContourLineProcessor.GetPotentialContourCentreLabelPositions();
            if (possibleLabelPositions != null)
            {
                for (int i = 0; i < TopographyLabelManager.MaxElevationLevel; i++)
                {
                    foreach (ContourCentreLabelProps labelProps in possibleLabelPositions[i])
                    {
                        int maxElevation = TopographyLabelManager.CurrentMaxElevationLevel;
                        int minElevation = TopographyLabelManager.CurrentMinElevationLevel;
                        int validRange = Mathf.CeilToInt((maxElevation - minElevation) * 0.2f);

                        if (labelProps.ContourLength > 20 && labelProps.ContourLength < 600 && labelProps.Circular &&
                            (labelProps.Depth <= (minElevation + validRange) || labelProps.Depth > (maxElevation - 1 - validRange)))
                        {
                            // Check if positioning is valid.
                            Vector2 meshSize = Sandbox.GetAdjustedMeshSize();

                            float xPos = labelProps.NormalisedPosition.x * meshSize.x + sandboxDescriptor.MeshStart.x;
                            float yPos = labelProps.NormalisedPosition.y * meshSize.y + sandboxDescriptor.MeshStart.y;

                            float depthLevel = ContourLineProcessor.GetDepthLevel(labelProps.GridPosition);

                            if (Mathf.Abs(depthLevel - labelProps.Depth) < 2f)
                            {
                                Collider2D overlapPoint = Physics2D.OverlapPoint(new Vector2(xPos, yPos), LabelColliderMask);
                                if (!overlapPoint)
                                {
                                    WindSimulationLabel newWindLabel = Instantiate(WindLabelPrefab);
                                    newWindLabel.Initialise(labelProps, Sandbox);
                                    newWindLabel.transform.parent = transform;

                                    newWindLabel.UpdateLabelText("L");
                                    if (labelProps.Depth > (maxElevation - 1 - validRange))
                                        newWindLabel.UpdateLabelText("H");

                                    ActiveWindLabels.Add(newWindLabel);
                                }
                                else
                                {
                                    WindSimulationLabel otherLabel = overlapPoint.gameObject.GetComponent<WindSimulationLabel>();
                                    if (otherLabel != null)
                                    {
                                        otherLabel.ResetTimeToLive();
                                        otherLabel.UpdateProps(labelProps);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
