//  
//  SandboxDataCamera.cs
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

namespace ARSandbox
{
    public class SandboxDataCamera : MonoBehaviour
    {
        public Sandbox Sandbox;
        public CalibrationManager CalibrationManager;
        public RenderTexture floatRT
        {
            get; private set;
        }

        private SandboxResolution sandboxResolution;
        private Camera dataCamera;
        private RenderTexture tempRT;
        private float texHeight = 32;
        private CommandBuffer drawSandboxBuffer;
        private Vector3 cameraOriginalPosition;
        private CalibrationDescriptor calibrationDescriptor;

        private void Start()
        {
            dataCamera = GetComponent<Camera>();
            Sandbox.OnSandboxReady += OnSandboxReady;

            drawSandboxBuffer = new CommandBuffer();
            drawSandboxBuffer.name = "Draw Sandbox Data Buffer";

            dataCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, drawSandboxBuffer);
        }

        private void OnSandboxReady()
        {
            calibrationDescriptor = CalibrationManager.GetCalibrationDescriptor();
            CalibrationManager.SetUpDataCamera(dataCamera);

            UpdateSandboxResolution(Sandbox.SandboxResolution);
        }

        public void UpdateSandboxResolution(SandboxResolution sandboxResolution)
        {
            if (Sandbox.SandboxReady)
            {
                this.sandboxResolution = sandboxResolution;
                switch (sandboxResolution)
                {
                    case SandboxResolution.Downsampled_1x:
                        texHeight = calibrationDescriptor.DataSize_DS.y * 0.6f;
                        break;
                    case SandboxResolution.Downsampled_2x:
                        texHeight = calibrationDescriptor.DataSize_DS2.y * 0.6f;
                        break;
                    case SandboxResolution.Downsampled_3x:
                        texHeight = calibrationDescriptor.DataSize_DS3.y * 0.6f;
                        break;
                    default:
                        texHeight = calibrationDescriptor.DataSize.y * 0.6f;
                        break;
                }
                CreateFloatRT();
            }
        }
        public void UpdateCalibrationDescriptor(CalibrationDescriptor calibrationDescriptor)
        {
            this.calibrationDescriptor = calibrationDescriptor;
            UpdateSandboxResolution(sandboxResolution);
        }
        private void CreateFloatRT()
        {
            float aspectRatio = (float)calibrationDescriptor.DataSize.x / (float)calibrationDescriptor.DataSize.y;
            if (floatRT != null)
            {
                floatRT.DiscardContents();
                floatRT.Release();
            }

            floatRT = new RenderTexture(Mathf.CeilToInt(texHeight * aspectRatio), Mathf.CeilToInt(texHeight), 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            floatRT.filterMode = FilterMode.Bilinear;

            dataCamera.targetTexture = floatRT;
        }

        private void OnPreRender()
        {
            if (Sandbox.SandboxReady)
            {
                Sandbox.SetRenderMaterial(SandboxRenderMaterial.DepthData);

                drawSandboxBuffer.Clear();

                Material material;
                int totalVerts;

                Sandbox.GetSandboxProceduralMaterials(out material, out totalVerts);
                drawSandboxBuffer.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, totalVerts);
            }
        }

        private void OnPostRender()
        {
            if (Sandbox.SandboxReady)
            {
                Sandbox.SetHeightTexture(floatRT);
            }
        }
    }
}
