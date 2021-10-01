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
    public class TestPlane : MonoBehaviour
    {
        public Sandbox Sandbox;
        public int texIndex = 0;

        //private bool sandboxReady = false;
        void Start()
        {
            Sandbox.OnSandboxReady += SetUpPlane;
        }

        public void SetTexture(Texture tex)
        {
            GetComponent<MeshRenderer>().material.SetTexture("_R16Tex", tex);
        }
        void Update()
        {
            /*if (sandboxReady)
            {
                SandboxDescriptor sandboxDescriptor = Sandbox.GetSandboxDescriptor();
                switch (texIndex)
                {
                    case 0:
                        GetComponent<MeshRenderer>().material.SetTexture("_R16Tex", sandboxDescriptor.RawDepthsTex);
                        break;
                    case 1:
                        GetComponent<MeshRenderer>().material.SetTexture("_R16Tex", sandboxDescriptor.RawDepthsRT_DS);
                        break;
                    case 2:
                        GetComponent<MeshRenderer>().material.SetTexture("_R16Tex", sandboxDescriptor.RawDepthsRT_DS2);
                        break;
                    case 3:
                        GetComponent<MeshRenderer>().material.SetTexture("_R16Tex", sandboxDescriptor.ProcessedDepthsRT);
                        break;
                    case 4:
                        GetComponent<MeshRenderer>().material.SetTexture("_R16Tex", sandboxDescriptor.ProcessedDepthsRT_DS);
                        break;
                    case 5:
                        GetComponent<MeshRenderer>().material.SetTexture("_R16Tex", sandboxDescriptor.ProcessedDepthsRT_DS2);
                        break;
                }
            }*/
        }
        void SetUpPlane()
        {
            /*SandboxDescriptor sandboxDescriptor = Sandbox.GetSandboxDescriptor();

            GetComponent<MeshRenderer>().material.SetTexture("_R16Tex", sandboxDescriptor.RawDepthsTex);

            sandboxReady = true;*/
        }
    }
}
