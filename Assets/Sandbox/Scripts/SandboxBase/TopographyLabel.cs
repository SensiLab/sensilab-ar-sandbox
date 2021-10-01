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
    public class TopographyLabel : MonoBehaviour
    {
        public GameObject MaskingPlane;
        public TextMesh TextMesh;
        public MeshRenderer MeshRenderer;

        public ContourLabelProps ContourLabelProps { get; private set; }
        private Sandbox sandbox;
        private SandboxDescriptor sandboxDescriptor;

        public void Initialise(ContourLabelProps contourLabelProps, Sandbox sandbox)
        {
            this.sandbox = sandbox;
            sandboxDescriptor = sandbox.GetSandboxDescriptor();

            UpdateProps(contourLabelProps);
        }
        public void UpdateDepthText(int depthOffset, float depthStep)
        {
            TextMesh.text = (depthOffset + ContourLabelProps.Depth * depthStep).ToString("f0");
            UpdateMaskSize();
        }
        public void UpdateProps(ContourLabelProps contourLabelProps)
        {
            ContourLabelProps = contourLabelProps;

            Vector2 meshSize = sandbox.GetAdjustedMeshSize();
            float xPos = contourLabelProps.NormalisedPosition.x * meshSize.x + sandboxDescriptor.MeshStart.x;
            float yPos = contourLabelProps.NormalisedPosition.y * meshSize.y + sandboxDescriptor.MeshStart.y;
            float zPos = sandbox.GetDepthFromWorldPos(new Vector3(xPos, yPos));
            float rotation = contourLabelProps.Rotation;

            Quaternion rotationQuaterion = new Quaternion();
            rotationQuaterion.eulerAngles = new Vector3(0, 0, rotation + 90);

            transform.position = new Vector3(xPos, yPos, zPos);
            transform.rotation = rotationQuaterion;
        }
        private void UpdateMaskSize()
        {
            // Set rotation to 0 so we can use the localScale to size the masking layer.
            Quaternion rotationQuaterion = transform.rotation;
            transform.rotation = Quaternion.identity;

            Bounds meshRendererBounds = MeshRenderer.bounds;
            MaskingPlane.transform.localScale = new Vector3(meshRendererBounds.extents.x * 2.4f, meshRendererBounds.extents.y * 1.65f, 1);

            transform.rotation = rotationQuaterion;
        }
    }
}
