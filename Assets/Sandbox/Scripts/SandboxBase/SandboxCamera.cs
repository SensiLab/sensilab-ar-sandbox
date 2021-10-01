//  
//  SandboxCamera.cs
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
    public class SandboxCamera : MonoBehaviour
    {
        public Sandbox Sandbox;
        public Camera Camera { get; private set; }

        private CommandBuffer drawSandboxBuffer;
        private bool cameraInitialised = false;
        private SandboxRenderMaterial sandboxRenderMaterial = SandboxRenderMaterial.Normal;

        private void Start()
        {
            InitialiseSandboxCamera();
        }
        public void InitialiseSandboxCamera()
        {
            if (!cameraInitialised)
            {
                drawSandboxBuffer = new CommandBuffer();
                drawSandboxBuffer.name = "Draw Sandbox";

                Camera = GetComponent<Camera>();
                Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, drawSandboxBuffer);

                cameraInitialised = true;
            }
        }
        public void SetSandboxRenderMaterial(SandboxRenderMaterial sandboxRenderMaterial)
        {
            this.sandboxRenderMaterial = sandboxRenderMaterial;
        }
        private void OnPreRender()
        {
            if (Sandbox.SandboxReady)
            {
                Sandbox.SetRenderMaterial(sandboxRenderMaterial);

                drawSandboxBuffer.Clear();

                Material material;
                int totalVerts;

                Sandbox.GetSandboxProceduralMaterials(out material, out totalVerts);
                drawSandboxBuffer.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, totalVerts);
            }
        }
    }
}
