//  
//  SandboxContourCamera.cs
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
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace ARSandbox
{
    public class SandboxContourCamera : MonoBehaviour
    {
        public Sandbox Sandbox;
        public CalibrationManager CalibrationManager;

        private Camera contourCamera;
        private CommandBuffer drawSandboxBuffer;

        private void Start()
        {
            contourCamera = GetComponent<Camera>();
            Sandbox.OnSandboxReady += OnSandboxReady;

            drawSandboxBuffer = new CommandBuffer();
            drawSandboxBuffer.name = "Draw Sandbox Contour Buffer";

            contourCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, drawSandboxBuffer);
        }

        private void OnSandboxReady()
        {
            CalibrationManager.SetUpUICamera(contourCamera);
            // Hack to line up the contour labels properly.
            contourCamera.orthographicSize += 0.2f;
        }
       
        public void SetRenderTexture(RenderTexture rt)
        {
            contourCamera.targetTexture = rt;
        }
        private void OnPreRender()
        {
            if (Sandbox.SandboxReady)
            {
                Sandbox.SetRenderMaterial(SandboxRenderMaterial.ContourData);
                drawSandboxBuffer.Clear();

                Material material;
                int totalVerts;

                Sandbox.GetSandboxProceduralMaterials(out material, out totalVerts);
                drawSandboxBuffer.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, totalVerts);
            }
        }

        private void OnPostRender()
        {
            // CANNOT CS HERE.
        }
    }
}
