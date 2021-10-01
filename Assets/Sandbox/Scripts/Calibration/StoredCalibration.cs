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

namespace ARSandbox {

    [System.Serializable]
    public class StoredCalibration
    {
        public float[,] KinectPoints;
        public float[,] SandboxPoints;
        public float MinDepth;
        public float MaxDepth;
        public float LensShift;
        public float CameraShiftY;
        public float CameraShiftX;
        public float ScaleX;
        public float ScaleY;

        public StoredCalibration()
        {
            KinectPoints = new float[4, 2];
            SandboxPoints = new float[4, 2];
            MinDepth = 1100;
            MaxDepth = 1400;
            LensShift = 0;
            CameraShiftY = 0;
            CameraShiftX = 0;
            ScaleX = 0;
            ScaleY = 0;
        }
    }
}
