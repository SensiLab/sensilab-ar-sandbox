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
    public class HandInput : MonoBehaviour
    {
        public Sandbox Sandbox;
        public CalibrationManager CalibrationManager;
        public Camera SandboxUICamera;
        public LayerMask SandboxLayerMask;
        public TestObject TestObject;

        private List<HandInputGesture> CurrentGestures;
        //private SandboxDescriptor sandboxDescriptor;

        public delegate void OnGesturesReady_Delegate();
        public static OnGesturesReady_Delegate OnGesturesReady;

        private bool IsCalibrating;
        void Start()
        {
            IsCalibrating = true;

            CalibrationManager.OnCalibration += OnCalibration;
            Sandbox.OnSandboxReady += OnSandboxReady;
            Sandbox.OnNewProcessedData += OnNewProcessedData;

            CurrentGestures = new List<HandInputGesture>();
        }
        private void OnCalibration()
        {
            IsCalibrating = true;
        }

        private void OnSandboxReady()
        {
            IsCalibrating = false;
            //sandboxDescriptor = Sandbox.GetSandboxDescriptor();
        }

        private void OnNewProcessedData()
        {
            if (!IsCalibrating)
            {
                if (OnGesturesReady != null) OnGesturesReady();
            }
        }

        public void OnUITouchDown(int touchID, Vector2 screenSpacePoint)
        {
            if (!CurrentGestures.Exists((gesture) => gesture.GestureID == touchID))
            {
                Vector3 worldPosition = SandboxUICamera.ViewportToWorldPoint(new Vector3(screenSpacePoint.x, screenSpacePoint.y));
                bool outOfBounds = false;
                float depth = -1;
                Ray ray = SandboxUICamera.ViewportPointToRay(screenSpacePoint);
                RaycastHit hitInfo;
                bool meshHit = Physics.Raycast(ray, out hitInfo, 1000, SandboxLayerMask);
                if (meshHit)
                {
                    depth = hitInfo.point.z / Sandbox.MESH_Z_SCALE;
                }
                else
                {
                    outOfBounds = true;
                }
                worldPosition.z = depth * Sandbox.MESH_Z_SCALE - 5;
                Point dataPosition = Sandbox.WorldPosToDataPos(worldPosition);
                Vector2 normalisedPosition = Sandbox.WorldPosToNormalisedPos(worldPosition);

                HandInputGesture newGesture = new HandInputGesture(touchID, worldPosition, normalisedPosition, depth, dataPosition, outOfBounds, true);

                CurrentGestures.Add(newGesture);
            }
            else
            {
                print("ERROR: Gesture with id: " + touchID.ToString() + " already exists!");
            }
        }
        public void OnUITouchMove(int touchID, Vector2 screenSpacePoint)
        {
            HandInputGesture UIGesture = CurrentGestures.Find((gesture) => gesture.GestureID == touchID);
            if (UIGesture != null)
            {
                Vector3 worldPosition = SandboxUICamera.ViewportToWorldPoint(new Vector3(screenSpacePoint.x, screenSpacePoint.y));
                bool outOfBounds = false;
                float depth = -1;
                Ray ray = SandboxUICamera.ViewportPointToRay(screenSpacePoint);
                RaycastHit hitInfo;
                bool meshHit = Physics.Raycast(ray, out hitInfo, 1000, SandboxLayerMask);
                if (meshHit)
                {
                    depth = hitInfo.point.z / Sandbox.MESH_Z_SCALE - 5;
                }
                else
                {
                    outOfBounds = true;
                }
                worldPosition.z = depth * Sandbox.MESH_Z_SCALE;
                Point dataPosition = Sandbox.WorldPosToDataPos(worldPosition);
                Vector2 normalisedPosition = Sandbox.WorldPosToNormalisedPos(worldPosition);

                UIGesture.UpdatePosition(worldPosition, normalisedPosition, depth, dataPosition, outOfBounds);
            }
            else
            {
                print("ERROR: Gesture with id: " + touchID.ToString() + " is missing!");
            }
        }
        public void OnUITouchUp(int touchID)
        {
            HandInputGesture UIGesture = CurrentGestures.Find((gesture) => gesture.GestureID == touchID);
            if (UIGesture != null)
            {
                CurrentGestures.Remove(UIGesture);
            }
            else
            {
                print("ERROR: Gesture with id: " + touchID.ToString() + " is missing!");
            }
        }

        // Returns a shallow copy of gestures. List is safe to manipulate.
        public List<HandInputGesture> GetCurrentGestures()
        {
            return CurrentGestures.GetRange(0, CurrentGestures.Count);
        }
    }

    public class HandInputGesture
    {
        public int GestureID { get; private set; }
        public bool IsUIGesture { get; private set; }
        public bool OutOfBounds { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public Vector2 NormalisedPosition { get; private set; }
        public Point DataPosition { get; private set; }
        public Point DataPosition_DS { get; private set; }
        public Point DataPosition_DS2 { get; private set; }
        public float SandboxDepth { get; private set; }
        // Age of the gesture in processed kinect frames. (30 = 1 second)
        public int Age { get; private set; }

        public HandInputGesture(int GestureID, Vector3 WorldPosition, Vector2 NormalisedPosition, float SandboxDepth, Point DataPosition)
        {
            Initialise(GestureID, WorldPosition, NormalisedPosition, SandboxDepth, DataPosition);
        }
        public HandInputGesture(int GestureID, Vector3 WorldPosition, Vector2 NormalisedPosition, float SandboxDepth, Point DataPosition, bool OutOfBounds, bool IsUIGesture)
        {
            Initialise(GestureID, WorldPosition, NormalisedPosition, SandboxDepth, DataPosition);
            this.IsUIGesture = IsUIGesture;
            this.OutOfBounds = OutOfBounds;
        }

        private void Initialise(int GestureID, Vector3 WorldPosition, Vector2 NormalisedPosition, float SandboxDepth, Point DataPosition)
        {
            this.GestureID = GestureID;
            this.WorldPosition = WorldPosition;
            this.SandboxDepth = SandboxDepth;
            this.DataPosition = DataPosition;
            this.NormalisedPosition = NormalisedPosition;
            Age = 1;
            IsUIGesture = false;
            OutOfBounds = false;
            DataPosition_DS = new Point(DataPosition.x / 2, DataPosition.y / 2);
            DataPosition_DS2 = new Point(DataPosition_DS.x / 2, DataPosition_DS.y / 2);
        }

        public void UpdatePosition(Vector3 WorldPosition, Vector2 NormalisedPosition, float SandboxDepth, Point DataPosition, bool OutOfBounds)
        {
            this.WorldPosition = WorldPosition;
            this.SandboxDepth = SandboxDepth;
            this.DataPosition = DataPosition;
            this.OutOfBounds = OutOfBounds;
            this.NormalisedPosition = NormalisedPosition;

            DataPosition_DS = new Point(DataPosition.x / 2, DataPosition.y / 2);
            DataPosition_DS2 = new Point(DataPosition_DS.x / 2, DataPosition_DS.y / 2);

            Age += 1;
        }
    }
}
