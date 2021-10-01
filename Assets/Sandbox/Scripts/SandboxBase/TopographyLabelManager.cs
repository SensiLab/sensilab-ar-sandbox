//  
//  TopographyLabelManager.cs
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

namespace ARSandbox
{
    public class TopographyLabelManager : MonoBehaviour
    {
        public enum ElevationSpacingType
        {
            ConstantSpacing,
            EndElevationSpacing
        }

        public Sandbox Sandbox;
        public CalibrationManager CalibrationManager;
        public TopographyLabel TopographyTextPrefab;
        public Camera TopographyTextMaskCamera;
        public ComputeShader SandboxCS;
        public SandboxContourCamera ContourCamera;
        public LayerMask LabelColliderMask;

        public delegate void OnNewContourLabels_Delegate();
        public static OnNewContourLabels_Delegate OnNewContourLabels;

        public delegate void OnContourLabelsToggled_Delegate(bool toggle);
        public static OnContourLabelsToggled_Delegate OnContourLabelsToggled;

        public bool ContourLabelsEnabled { get; private set; }
        public float LabelDensity { get; private set; }
        public int StartingElevation { get; private set; }
        public int EndingElevation { get; private set; }
        public int ElevationConstSpacing { get; private set; }
        public ElevationSpacingType ElevationSpacingMode { get; private set; }
        public int MaxElevationLevel { get; private set; }
        public int CurrentMaxElevationLevel { get; private set; }
        public int CurrentMinElevationLevel { get; private set; }
        public bool ElevationLabelsForced { get; private set; }

        private bool SavedContourLabelsEnabled;
        private int SavedStartingElevation;
        private int SavedEndingElevation;
        private int SavedElevationConstSpacing;
        private ElevationSpacingType SavedElevationSpacingMode;

        private int labelSpacing;
        private bool delayedRemoveLabels;
        private float calculatedElevationSpacing;

        private RenderTexture contourRT, sobelRT, maximalContourRT;
        private ComputeBuffer contourPathsBuffer;
        private int[] linePixels;
        private float texHeight = 256;
        private Point rT_Size;

        private SandboxDescriptor sandboxDescriptor;
        private List<TopographyLabel> ActiveTopographyTexts;
        private RenderTexture maskRT;
        
        void Start()
        {
            ActiveTopographyTexts = new List<TopographyLabel>();
            Sandbox.OnSandboxReady += OnSandboxReady;
            Sandbox.OnNewProcessedData += OnNewProcessedData;
            CalibrationManager.OnCalibration += OnCalibration;

            ContourLabelsEnabled = true;
            UI_ChangeContourLabelDensity(50);
            StartingElevation = 0;
            EndingElevation = 1000;
            ElevationConstSpacing = 50;
            MaxElevationLevel = 16;
            ElevationSpacingMode = ElevationSpacingType.ConstantSpacing;
        }
        void OnDestroy()
        {
            ReleaseBuffers();
        }
        void OnSandboxReady()
        {
            sandboxDescriptor = Sandbox.GetSandboxDescriptor();
            CalibrationManager.SetUpDataCamera(TopographyTextMaskCamera);

            UpdateMaxElevationLevel();

            CreateMaskRT();
            TopographyTextMaskCamera.targetTexture = maskRT;
            Sandbox.SetTopographyLabelMaskRT(maskRT);

            CreateContourRTs();
            ContourCamera.SetRenderTexture(contourRT);
        }
        void OnCalibration()
        {
            DestroyAllLabels();
        }

        private void CreateContourRTs()
        {
            ReleaseBuffers();

            float aspectRatio = (float)sandboxDescriptor.DataSize.x / (float)sandboxDescriptor.DataSize.y;
            rT_Size = new Point(Mathf.CeilToInt(texHeight * aspectRatio), Mathf.CeilToInt(texHeight));

            contourRT = InitialiseRT(rT_Size);
            sobelRT = InitialiseRT(rT_Size);
            maximalContourRT = InitialiseRT(rT_Size);

            contourPathsBuffer = new ComputeBuffer(rT_Size.x * rT_Size.y, 4);
            linePixels = new int[rT_Size.x * rT_Size.y];
        }
        private void ReleaseBuffers()
        {
            if (contourRT != null)
            {
                contourRT.Release();
                sobelRT.Release();
                maximalContourRT.Release();
                maskRT.Release();

                contourPathsBuffer.Dispose();
            }
        }
        private RenderTexture InitialiseRT(Point size)
        {
            RenderTexture renderTexture;

            renderTexture = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            return renderTexture;
        }
        void CreateMaskRT()
        {
            if (maskRT != null) maskRT.Release();

            float aspectRatio = (float)sandboxDescriptor.DataSize.x / (float)sandboxDescriptor.DataSize.y;
            Point maskRT_Size = new Point(Mathf.CeilToInt(1080 * aspectRatio), 1080);

            maskRT = new RenderTexture(maskRT_Size.x, maskRT_Size.y, 0, RenderTextureFormat.RG16, RenderTextureReadWrite.Linear);
            maskRT.filterMode = FilterMode.Bilinear;
            maskRT.Create();
        }
        /*private bool SavedContourLabelsEnabled;
        private int SavedStartingElevation;
        private int SavedEndingElevation;
        private int SavedElevationConstSpacing;
        private int SavedElevationSpacingMode;*/

        public void ForceElevationLabels(bool enabled, int startingElevation, int elevationConstSpacing, int endingElevation, ElevationSpacingType elevationSpacingMode)
        {
            if (!ElevationLabelsForced)
            {
                SavedContourLabelsEnabled = ContourLabelsEnabled;
                SavedStartingElevation = StartingElevation;
                SavedEndingElevation = EndingElevation;
                SavedElevationConstSpacing = ElevationConstSpacing;
                SavedElevationSpacingMode = ElevationSpacingMode;

                ElevationLabelsForced = true;
            }
            ContourLabelsEnabled = enabled;
            StartingElevation = startingElevation;
            ElevationConstSpacing = elevationConstSpacing;
            EndingElevation = endingElevation;
            ElevationSpacingMode = elevationSpacingMode;

            UpdateLabelText();
        }
        public void UnforceElevationLabels()
        {
            if (ElevationLabelsForced)
            {
                ContourLabelsEnabled = SavedContourLabelsEnabled;
                StartingElevation = SavedStartingElevation;
                EndingElevation = SavedEndingElevation;
                ElevationConstSpacing = SavedElevationConstSpacing;
                ElevationSpacingMode = SavedElevationSpacingMode;
                UpdateLabelText();

                ElevationLabelsForced = false;
            }
        }
        //startingElevation, endingElevation, constantElevationSpacing;
        public void SetStartingElevation(int startingElevation)
        {
            this.StartingElevation = startingElevation;
            UpdateLabelText();
        }
        public void SetEndingElevation(int endingElevation)
        {
            this.EndingElevation = endingElevation;
            UpdateLabelText();
        }
        public void SetElevationSpacing(int elevationSpacing)
        {
            this.ElevationConstSpacing = elevationSpacing;
            UpdateLabelText();
        }
        public void SetElevationSpacingMode(ElevationSpacingType elevationSpacingMode)
        {
            if (ElevationSpacingMode != elevationSpacingMode)
            {
                ElevationSpacingMode = elevationSpacingMode;
                UpdateLabelText();
            }
        }

        public void UI_ToggleContourLabels(bool labelToggle)
        {
            if (!labelToggle && labelToggle != ContourLabelsEnabled)
            {
                if (OnContourLabelsToggled != null) OnContourLabelsToggled(labelToggle);
                DestroyAllLabels();
            }
            ContourLabelsEnabled = labelToggle;
        }

        public void UI_ChangeContourLabelDensity(float labelDensity)
        {
            float scaledLabelDensity = labelDensity / 100.0f;
            if (scaledLabelDensity != LabelDensity)
            {
                LabelDensity = scaledLabelDensity;
                labelSpacing = 20 + Mathf.FloorToInt((1 - LabelDensity) * 100.0f);
                delayedRemoveLabels = true;
            }
        }

        public void UpdateMaxElevationLevel()
        {
            MaxElevationLevel = Mathf.CeilToInt((sandboxDescriptor.MaxDepth - sandboxDescriptor.MinDepth) / Sandbox.MajorContourSpacing);
            UpdateLabelText();
        }

        void DestroyAllLabels()
        {
            if (ActiveTopographyTexts != null)
            {
                int totalActiveTexts = ActiveTopographyTexts.Count;
                for (int i = totalActiveTexts - 1; i >= 0; i--)
                {
                    Destroy(ActiveTopographyTexts[i].gameObject);
                    Destroy(ActiveTopographyTexts[i]);
                    ActiveTopographyTexts.RemoveAt(i);
                }
            }
        }

        void UpdateLabelText()
        {
            if (ElevationSpacingMode == ElevationSpacingType.EndElevationSpacing)
                calculatedElevationSpacing = (EndingElevation - StartingElevation) / (float)MaxElevationLevel;
            foreach (TopographyLabel topographyText in ActiveTopographyTexts)
            {
                if (topographyText != null)
                {
                    if (ElevationSpacingMode == ElevationSpacingType.ConstantSpacing)
                    {
                        topographyText.UpdateDepthText(StartingElevation, ElevationConstSpacing);
                    }
                    else
                    {
                        topographyText.UpdateDepthText(StartingElevation, calculatedElevationSpacing);
                    }
                }
            }
        }

        void OnNewProcessedData()
        {
            if (ContourLabelsEnabled && !CalibrationManager.IsCalibrating)
            {
                SandboxCSHelper.Run_ContourSobelFilter(SandboxCS, contourRT, sobelRT);
                SandboxCSHelper.Run_ContourNonMaximalSupression(SandboxCS, sobelRT, maximalContourRT);
                SandboxCSHelper.Run_ContourFindPaths(SandboxCS, maximalContourRT, contourPathsBuffer);

                contourPathsBuffer.GetData(linePixels);

                ContourLineProcessor.ProcessContourPixels(linePixels, rT_Size);

                if (delayedRemoveLabels)
                {
                    delayedRemoveLabels = false;
                    DestroyAllLabels();
                }
                ProcessLabelPositions();
            }
        }

        void ProcessLabelPositions()
        {
            int totalActiveTexts = ActiveTopographyTexts.Count;
            for (int i = totalActiveTexts - 1; i >= 0; i--)
            {
                ContourLabelProps oldProps = ActiveTopographyTexts[i].ContourLabelProps;
                ContourLabelProps newProps;
                if (ContourLineProcessor.ValidateContourLabelProps(oldProps, out newProps))
                {
                    ActiveTopographyTexts[i].UpdateProps(newProps);
                }
                else
                {
                    Destroy(ActiveTopographyTexts[i].gameObject);
                    Destroy(ActiveTopographyTexts[i]);
                    ActiveTopographyTexts.RemoveAt(i);
                }
            }

            ContourLineProcessor.CalculatePotentialLabelPositions(labelSpacing);
            List<ContourLabelProps>[] possibleLabelPositions
                            = ContourLineProcessor.GetPotentialContourLabelPositions();

            CurrentMinElevationLevel = 0;
            if (possibleLabelPositions != null)
            {
                for (int i = 0; i < MaxElevationLevel; i++)
                {
                    if (possibleLabelPositions[i].Count > 0)
                    {
                        CurrentMinElevationLevel = CurrentMinElevationLevel > i ? i : CurrentMinElevationLevel;
                        CurrentMaxElevationLevel = i;
                    }

                    foreach (ContourLabelProps labelProps in possibleLabelPositions[i])
                    {
                        // Check if positioning is valid.
                        Vector2 meshSize = Sandbox.GetAdjustedMeshSize();
                        float xPos = labelProps.NormalisedPosition.x * meshSize.x + sandboxDescriptor.MeshStart.x;
                        float yPos = labelProps.NormalisedPosition.y * meshSize.y + sandboxDescriptor.MeshStart.y;

                        Collider2D overlapPoint = Physics2D.OverlapPoint(new Vector2(xPos, yPos), LabelColliderMask);

                        if (!overlapPoint)
                        {
                            TopographyLabel newTopographyLabel = Instantiate(TopographyTextPrefab);
                            newTopographyLabel.Initialise(labelProps, Sandbox);
                            newTopographyLabel.transform.parent = transform;

                            if (ElevationSpacingMode == ElevationSpacingType.ConstantSpacing)
                            {
                                newTopographyLabel.UpdateDepthText(StartingElevation, ElevationConstSpacing);
                            }
                            else
                            {
                                newTopographyLabel.UpdateDepthText(StartingElevation, calculatedElevationSpacing);
                            }

                            ActiveTopographyTexts.Add(newTopographyLabel);
                        }
                    }
                }

                if (OnNewContourLabels != null) OnNewContourLabels();
            }
        }
    }
}
