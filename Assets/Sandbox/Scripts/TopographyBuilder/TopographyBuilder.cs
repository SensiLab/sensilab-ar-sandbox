//  
//  TopographyBuilder.cs
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
using Windows.Kinect;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ARSandbox.TopographyBuilder
{
    public class TopographyBuilder : MonoBehaviour
    {
        public Sandbox Sandbox;
        public CalibrationManager CalibrationManager;
        public KinectManager KinectManager;
        public ComputeShader SandboxProcessingShader;
        public Shader SandboxTopographyBuilderShader;
        public Texture2D HeightDifferenceColour;

        public delegate void OnNewListLoaded_Delegate();
        public static OnNewListLoaded_Delegate OnNewListLoaded;

        private const string TOPOGRAPHY_FILE_NAME_LIST = "SavedTopographyNames.props";
        private const string SAVE_DIRECTORY_FOLDER = "TopographyBuilder";
        private FrameDescription kinectFrameDesc;
        public float ValidHeightRange { get; private set; }
        public float HeightOffset { get; private set; }

        private byte[] rawDepthData;
        private Texture2D rawDepthsTex;
        private RenderTexture internalLowPassDataRT;
        private RenderTexture lowPassCounterRT;
        private RenderTexture lowPassDataRT;
        private RenderTexture blurredDataTempRT;
        private RenderTexture processedDepthsRT;

        private SandboxDescriptor sandboxDescriptor;
        public LoadedTopography SelectedTopography { get; private set; }
        private bool ValidTopographyProps, TopographyInitialised;

        private List<string> savedTopographyNames;
        private bool setInitialData;
        public List<LoadedTopography> loadedTopographies { get; private set; }

        private void OnEnable()
        {
            CalibrationManager.OnCalibration += OnCalibration;
            Sandbox.OnNewProcessedData += OnNewProcessedData;

            kinectFrameDesc = KinectManager.GetKinectFrameDescriptor();
            InitialiseBuilder();
        }
        private void OnDisable()
        {
            CalibrationManager.OnCalibration -= OnCalibration;
            Sandbox.OnSandboxReady -= OnSandboxReady;

            DisposeRenderTextures();
            Sandbox.SetDefaultShader();
        }
        private void OnCalibration()
        {
            Sandbox.OnSandboxReady += OnSandboxReady;

            DisposeRenderTextures();
            Sandbox.SetDefaultShader();
        }
        // Called only after a successful calibration.
        private void OnSandboxReady()
        {
            InitialiseBuilder();
        }
        private void OnNewProcessedData()
        {
            if (TopographyInitialised)
            {
                GenerateProcessedTexture();
            }
        }
        private void InitialiseBuilder()
        {
            sandboxDescriptor = Sandbox.GetSandboxDescriptor();
            TopographyInitialised = false;
            ValidHeightRange = 25;

            Sandbox.SetSandboxShader(SandboxTopographyBuilderShader);
            Sandbox.SetShaderTexture("_HeightColorScaleTex", HeightDifferenceColour);
            Sandbox.SetShaderFloat("_ValidHeightRange", ValidHeightRange);

            LoadTopographyList();
            LoadTopographyProps();
            SortTopographyProps();

            if (!UI_SelectTopography(0))
            {
                Sandbox.SetShaderTexture("_LoadedHeightTex", Sandbox.CurrentDepthTexture);
            }
        }
        private void SetUpLoadedTopography()
        {
            if (ValidTopographyProps)
            {
                SelectedTopography.LoadRawData(GetTopographyBuilderDirectory());
                if (SelectedTopography.DataLoaded)
                {
                    DisposeRenderTextures();
                    InitialiseBuffers();
                    UpdateRawTexture();
                    Sandbox.SetShaderTexture("_LoadedHeightTex", processedDepthsRT);
                    Sandbox.SetForcedHeightTexture(rawDepthsTex);
                    Sandbox.SetForcedHeightEnabled(true);
                    Sandbox.SetShaderFloat("_HeightTextureSizeX", SelectedTopography.DataSize.x);
                    Sandbox.SetShaderFloat("_HeightTextureSizeY", SelectedTopography.DataSize.y);
                    setInitialData = true;

                    TopographyInitialised = true;
                } else
                {
                    ValidTopographyProps = false;
                    print("Warning: Trying to initialise null topography");
                }
            } else
            {
                print("Warning: Trying to initialise null topography properties");
            }
        }
        private void InitialiseBuffers()
        {
            Point dataSize = SelectedTopography.DataSize;
            int width = dataSize.x;
            int height = dataSize.y;
            int totalValues = dataSize.x * dataSize.y;

            rawDepthData = new byte[totalValues * 2];
            rawDepthsTex = new Texture2D(width, height, TextureFormat.R16, false);
            rawDepthsTex.filterMode = FilterMode.Bilinear;

            lowPassCounterRT = InitialiseDepthRT(sandboxDescriptor.DataSize);
            internalLowPassDataRT = InitialiseDepthRT(sandboxDescriptor.DataSize);
            lowPassDataRT = InitialiseDepthRT(sandboxDescriptor.DataSize);

            blurredDataTempRT = InitialiseDepthRT(sandboxDescriptor.DataSize);
            processedDepthsRT = InitialiseDepthRT(sandboxDescriptor.DataSize);
        }
        private RenderTexture InitialiseDepthRT(Point size)
        {
            return InitialiseDepthRT(size.x, size.y);
        }
        private RenderTexture InitialiseDepthRT(int width, int height)
        {
            RenderTexture renderTexture = new RenderTexture(width, height, 0,
                                                    RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            return renderTexture;
        }
        private void UpdateRawTexture()
        {
            Point dataSize = SelectedTopography.DataSize;
            Point dataStart = SelectedTopography.DataStart;
            Point dataEnd = SelectedTopography.DataEnd;
            ushort[] depthDataBuffer = SelectedTopography.DepthDataBuffer;

            int x = dataStart.x;
            int y = dataStart.y;
            int dataStartX = dataStart.x;
            int dataEndX = dataEnd.x;
            int totalDataPoints = dataSize.x * dataSize.y;

            int width = kinectFrameDesc.Width;
            for (int i = 0; i < totalDataPoints; i++)
            {
                int index = y * width + x;

                rawDepthData[2 * i] = (byte)depthDataBuffer[index];
                rawDepthData[2 * i + 1] = (byte)(depthDataBuffer[index] >> 8);

                x += 1;
                if (x >= dataEndX)
                {
                    x = dataStartX;
                    y += 1;
                }
            }

            rawDepthsTex.LoadRawTextureData(rawDepthData);
            rawDepthsTex.Apply();
        }
        private void GenerateProcessedTexture()
        {
            if (setInitialData)
            {
                SandboxCSHelper.Run_SetInitialLowPassData(SandboxProcessingShader, sandboxDescriptor.RawDepthsTex, sandboxDescriptor.DataSize, internalLowPassDataRT, lowPassCounterRT,
                                                              lowPassDataRT, SelectedTopography.MinDepth, SelectedTopography.MaxDepth);
                setInitialData = false;
            }
            SandboxCSHelper.Run_ComputeLowPassRT(SandboxProcessingShader, sandboxDescriptor.RawDepthsTex, sandboxDescriptor.DataSize, internalLowPassDataRT, lowPassCounterRT, lowPassDataRT,
                                                     Sandbox.ALPHA_1, Sandbox.ALPHA_2, SelectedTopography.MinDepth, SelectedTopography.MaxDepth,
                                                     Sandbox.NoiseTolerance, Sandbox.LowPassHoldTime);
            SandboxCSHelper.Run_BlurRT(SandboxProcessingShader, lowPassDataRT, blurredDataTempRT, processedDepthsRT);
        }
        private void DisposeRenderTextures()
        {
            if (ValidTopographyProps && TopographyInitialised)
            {
                internalLowPassDataRT.Release();
                lowPassCounterRT.Release();
                lowPassDataRT.Release();
                blurredDataTempRT.Release();
                processedDepthsRT.Release();
            }
        }
        private string GetTopographyBuilderDirectory()
        {
            string topographyDirectory = Path.Combine(Application.persistentDataPath, SAVE_DIRECTORY_FOLDER);
            if (!Directory.Exists(topographyDirectory))
            {
                Directory.CreateDirectory(topographyDirectory);
            }
            return topographyDirectory;
        }
        private void SaveTopographyList()
        {
            string filePath = Path.Combine(GetTopographyBuilderDirectory(), TOPOGRAPHY_FILE_NAME_LIST);
            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = File.Create(filePath))
            {
                bf.Serialize(fs, savedTopographyNames.ToArray());
            }
        }
        private void LoadTopographyList()
        {
            string filePath = Path.Combine(GetTopographyBuilderDirectory(), TOPOGRAPHY_FILE_NAME_LIST);
            BinaryFormatter bf = new BinaryFormatter();

            if (File.Exists(filePath))
            {
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        string[] topographyList = (string[])bf.Deserialize(fs);

                        savedTopographyNames = new List<string>(topographyList);
                    }
                } catch (Exception e)
                {
                    print("Error - File Exception: " + e.ToString());
                    print("No topographys could be loaded.");
                    savedTopographyNames = new List<string>();
                }
            } else
            {
                try
                {
                    using (FileStream fs = File.Create(filePath))
                    {
                        string[] topographyList = new string[0];
                        bf.Serialize(fs, topographyList);

                        savedTopographyNames = new List<string>();
                    }
                }
                catch (Exception e)
                {
                    print("Error - File Exception: " + e.ToString());

                    savedTopographyNames = new List<string>();
                }
            }
        }
        private void LoadTopographyProps()
        {
            loadedTopographies = new List<LoadedTopography>();
            for (int i = savedTopographyNames.Count - 1; i >= 0; i--)
            {
                string propsName = savedTopographyNames[i];
                string propsPath = Path.Combine(GetTopographyBuilderDirectory(), propsName);

                if (File.Exists(propsPath))
                {
                    LoadedTopography loadedTopography = new LoadedTopography(propsPath);
                    if (loadedTopography.ValidFileLoaded)
                    {
                        loadedTopographies.Add(loadedTopography);
                    } else
                    {
                        print("Invalid topography loaded. Removing from list.");
                        savedTopographyNames.RemoveAt(i);
                    }
                } else
                {
                    print("Cannot find file: " + propsName + ". Removing from list.");
                    savedTopographyNames.RemoveAt(i);
                }
            }
            SaveTopographyList();
            if (OnNewListLoaded != null) OnNewListLoaded();
        }
        private void AddLoadedTopography(LoadedTopography loadedTopography)
        {
            loadedTopographies.Add(loadedTopography);
            SortTopographyProps();
            UI_SelectTopography(loadedTopography);

            if (OnNewListLoaded != null) OnNewListLoaded();
        }
        private void SortTopographyProps()
        {
            loadedTopographies.Sort(delegate (LoadedTopography x, LoadedTopography y)
            {
                if (x.DisplayName == null && y.DisplayName == null) return 0;
                return x.DisplayName.CompareTo(y.DisplayName);
            });
        }
        public void SaveCurrentTopography(string topographyName)
        {
            TopographyPropertiesSerialised topographySerialised 
                = new TopographyPropertiesSerialised(sandboxDescriptor, CalibrationManager.GetCalibrationDescriptor(), false, topographyName);

            BinaryFormatter bf = new BinaryFormatter();
            string propsPath = Path.Combine(GetTopographyBuilderDirectory(), topographySerialised.TopographyPropertiesPath);
            string dataPath = Path.Combine(GetTopographyBuilderDirectory(), topographySerialised.TopographyDataPath);

            bool uniqueName = true;
            bool propsSaved = true;
            bool dataSaved = true;

            try
            {
                if (File.Exists(propsPath))
                {
                    uniqueName = false;
                    print("Warning File Already Exists: " + topographySerialised.TopographyPropertiesPath);
                }
                using (FileStream fs = File.Create(propsPath))
                {
                    bf.Serialize(fs, topographySerialised);
                }
            } catch (Exception e)
            {
                print("Error - File Exception: " + e.ToString());
                propsSaved = false;
            }

            try
            {
                if (File.Exists(dataPath))
                {
                    print("Warning File Already Exists: " + topographySerialised.TopographyDataPath);
                }
                using (FileStream fs = File.Create(dataPath))
                {
                    bf.Serialize(fs, Sandbox.depthDataBuffer);
                }
            }
            catch (Exception e)
            {
                print("Error - File Exception: " + e.ToString());
                dataSaved = false;
            }

            if (propsSaved && dataSaved && uniqueName)
            {
                savedTopographyNames.Add(topographySerialised.TopographyPropertiesPath);
                SaveTopographyList();
                AddLoadedTopography(new LoadedTopography(propsPath));
            }
        }
        public void UI_DeleteTopography(LoadedTopography loadedTopography)
        {
            savedTopographyNames.Remove(loadedTopography.PropsPath);
            SaveTopographyList();

            int topographyIndex = loadedTopographies.IndexOf(loadedTopography);
            topographyIndex = topographyIndex == 0 ? 0 : topographyIndex - 1;
            loadedTopographies.Remove(loadedTopography);

            loadedTopography.Delete(GetTopographyBuilderDirectory());

            UI_SelectTopography(topographyIndex);
            if (OnNewListLoaded != null) OnNewListLoaded();
        }
        public bool UI_SelectTopography(int index)
        {
            if (index < loadedTopographies.Count && index >= 0)
            {
                SelectedTopography = loadedTopographies[index];
                ValidTopographyProps = true;

                HeightOffset = 0;
                SetUpLoadedTopography();
                return true;
            }

            SelectedTopography = null;
            return false;
        }
        public bool UI_SelectTopography(LoadedTopography loadedTopography)
        {
            if (loadedTopographies.Contains(loadedTopography))
            {
                SelectedTopography = loadedTopography;
                ValidTopographyProps = true;

                HeightOffset = 0;
                SetUpLoadedTopography();
                return true;
            }
            return false;
        }
        public bool UI_RenameTopography(string newName)
        {
            if (SelectedTopography != null)
            {
                string originalPropsPath = SelectedTopography.PropsPath;
                if (SelectedTopography.Rename(newName, GetTopographyBuilderDirectory()))
                {
                    savedTopographyNames.Remove(originalPropsPath);
                    savedTopographyNames.Add(SelectedTopography.PropsPath);
                    SaveTopographyList();
                    SortTopographyProps();

                    if (OnNewListLoaded != null) OnNewListLoaded();
                    return true;
                }
            }
            return false;
        }

        public void UI_UpdateValidHeightRange(float validHeightRange)
        {
            this.ValidHeightRange = validHeightRange;
            Sandbox.SetShaderFloat("_ValidHeightRange", validHeightRange);
        }
        public void UI_UpdateHeightOffset(float heightOffset)
        {
            this.HeightOffset = heightOffset;
            Sandbox.SetShaderFloat("_LoadedHeightOffset", heightOffset);
        }
    }
}
