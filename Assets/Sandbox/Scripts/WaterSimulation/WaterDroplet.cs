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
    public class WaterDroplet : MonoBehaviour
    {
        public GameObject WaterDropletPhysicsPrefab;
        public const float DROPLET_RADIUS = 2.5f;

        private GameObject waterDropletPhysics;
        private bool showMesh = false;

        void Start()
        {
            waterDropletPhysics = Instantiate(WaterDropletPhysicsPrefab, transform.position, Quaternion.identity);
            waterDropletPhysics.GetComponent<MeshRenderer>().enabled = showMesh;
        }

        void Update()
        {
            transform.position = waterDropletPhysics.transform.position;
        }

        void OnDestroy()
        {
            Destroy(waterDropletPhysics);
            Destroy(gameObject);
            Destroy(this);
        }

        public void SetShowMesh(bool showMesh)
        {
            this.showMesh = showMesh;
            if (waterDropletPhysics != null)
            {
                waterDropletPhysics.GetComponent<MeshRenderer>().enabled = showMesh;
            }
        }

        public void SetZPosition(float z)
        {
            Vector3 newPosition = transform.position;
            newPosition.z = z;
            transform.position = newPosition;
            waterDropletPhysics.transform.position = newPosition;
        }
    }
}
