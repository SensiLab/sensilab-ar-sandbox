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

using UnityEngine;
using System.Collections;
using System.IO;
using Windows.Kinect;

namespace ARSandbox
{
    public class KinectManager : MonoBehaviour
    {
        public bool UseSavedData;
        public TextAsset SavedData;

        public delegate void OnDataStarted_Delegate();
        public static event OnDataStarted_Delegate OnDataStarted;

        private FrameDescription kinectFrameDesc;
        private KinectSensor kinectSensor;
        private DepthFrameReader depthFrameReader;
        private ushort[] depthData;
        private bool dataReady = false;
        private bool newData = false;

        void Start()
        {
            if (GetFrameDescriptor())
            {
                if (UseSavedData)
                {
                    LoadDepthData();
                    StartCoroutine(Emulate30Hz());
                }
                else
                {
                    SetUpKinectBuffer();
                }
            }
        }

        void Update()
        {
            if (!UseSavedData)
            {
                if (depthFrameReader != null)
                {
                    DepthFrame frame = depthFrameReader.AcquireLatestFrame();
                    if (frame != null)
                    {
                        if (!dataReady)
                        {
                            dataReady = true;
                            if (OnDataStarted != null) OnDataStarted();
                        }
                        frame.CopyFrameDataToArray(depthData);
                        newData = true;
                        frame.Dispose();
                        frame = null;
                    }
                }

                if (Input.GetKeyUp(KeyCode.S))
                {
                    //SaveDepthData();
                }
            }
        }

        void OnApplicationQuit()
        {
            if (!UseSavedData)
            {
                if (depthFrameReader != null)
                {
                    depthFrameReader.Dispose();
                    depthFrameReader = null;
                }

                if (kinectSensor != null)
                {
                    if (kinectSensor.IsOpen)
                    {
                        kinectSensor.Close();
                    }

                    kinectSensor = null;
                }
            }
        }
        private IEnumerator Emulate30Hz()
        {
            while (true)
            {
                newData = true;
                yield return new WaitForSeconds(1 / 30.0f);

                if (!dataReady)
                {
                    dataReady = true;
                    if (OnDataStarted != null) OnDataStarted();
                }
            }
        }
        public FrameDescription GetKinectFrameDescriptor()
        {
            return kinectFrameDesc;
        }
        public Point GetKinectFrameSize()
        {
            return new Point(kinectFrameDesc.Width, kinectFrameDesc.Height);
        }
        public ushort[] GetCurrentData()
        {
            newData = false;
            return depthData;
        }

        public bool StreamStarted()
        {
            if (UseSavedData)
                return true;

            return dataReady;
        }

        public bool NewDataReady()
        {
            return newData;
        }

        private bool GetFrameDescriptor()
        {
            kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor != null)
            {
                kinectFrameDesc = kinectSensor.DepthFrameSource.FrameDescription;
                return true;
            }
            else
            {
                print("Error: KinectSensor not found. Make sure Kinect has been installed correctly");
                return false;
            }
        }

        private void SetUpKinectBuffer()
        {
            if (kinectSensor != null)
            {
                if (!kinectSensor.IsOpen)
                {
                    kinectSensor.Open();
                }

                depthFrameReader = kinectSensor.DepthFrameSource.OpenReader();
                depthData = new ushort[kinectSensor.DepthFrameSource.FrameDescription.LengthInPixels];
            }
        }

        private void LoadDepthData()
        {
            using (Stream s = new MemoryStream(SavedData.bytes))
            {
                using (BinaryReader br = new BinaryReader(s))
                {
                    int length = br.ReadInt32();
                    depthData = new ushort[length];
                    for (int i = 0; i < length; i++)
                    {
                        depthData[i] = br.ReadUInt16();
                    }
                }
            }
        }
        private void SaveDepthData()
        {
            using (FileStream fs = new FileStream(Application.dataPath + "/Depth.txt", FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(depthData.Length);
                    foreach (ushort value in depthData)
                    {
                        bw.Write(value);
                    }
                }
            }
        }
    }
}
