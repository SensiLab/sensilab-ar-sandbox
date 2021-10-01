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
using ARSandbox.GeologySimulation.GeologicalTransforms;

namespace ARSandbox.GeologySimulation
{
    public class GeologySimulation : MonoBehaviour
    {
        public Sandbox Sandbox;
        public CalibrationManager CalibrationManager;
        public ComputeShader GeologyComputeShader;
        public Shader GeologySurfaceShader;

        public GeologicalTransformHandler GeologicalTransformHandler
        {
            get; private set;
        }
        public GeologicalLayerHandler GeologicalLayerHandler
        {
            get; private set;
        }
        private bool _SimulationReady = false;
        public bool SimulationReady
        {
            get
            {
                return _SimulationReady;
            }
        }

        private bool FirstRun = true;
        private SandboxDescriptor sandboxDescriptor;
        private RenderTexture geologySurfaceRT;
        private RenderTexture colouredOutputRT;
        private Vector3 simulationCentre;
        private Vector2 simulationDimensions;

        private void SetUpGeologySimulation()
        {
            if (FirstRun) {
                GeologicalTransformHandler = new GeologicalTransformHandler();
                GeologicalTransformHandler.AddTiltTransform(0, 0, 0, true);

                GeologicalLayerHandler = new GeologicalLayerHandler(-256.1f);
                GeologicalLayerHandler.AddRandomGeologicalLayer();
                GeologicalLayerHandler.AddRandomGeologicalLayer();
                GeologicalLayerHandler.AddRandomGeologicalLayer();
                GeologicalLayerHandler.AddRandomGeologicalLayer();
                GeologicalLayerHandler.AddRandomGeologicalLayer();
                GeologicalLayerHandler.AddRandomGeologicalLayer();

                FirstRun = false;
            } else
            {
                DisposeRenderTextures();
            }
            CreateRenderTextures();

            Vector3 simulationCentre = CalculateSimulationCentre();
            GeologicalTransformHandler.ChangeSimulationCentre(simulationCentre);
            GeologicalLayerHandler.LayerStartingDepth = -sandboxDescriptor.MaxDepth;

            Sandbox.SetSandboxShader(GeologySurfaceShader);
            Sandbox.SetShaderTexture("_GeologySurfaceTex", colouredOutputRT);

            _SimulationReady = true;
        }
        private Vector3 CalculateSimulationCentre()
        {
            Point heightTexSize = sandboxDescriptor.DataSize;
            simulationDimensions = new Vector2(heightTexSize.x, heightTexSize.y);
            simulationCentre = new Vector3();
            simulationCentre.x = heightTexSize.x / 2.0f;
            simulationCentre.y = heightTexSize.y / 2.0f;
            simulationCentre.z = -(sandboxDescriptor.MaxDepth + sandboxDescriptor.MinDepth) / 2.0f;

            return simulationCentre;
        }
        private void CreateRenderTextures()
        {
            Point heightTexSize = sandboxDescriptor.DataSize;
            Point surfaceSize = new Point(1920, (int)(1920.0f * (float)heightTexSize.y / (float)heightTexSize.x));

            geologySurfaceRT = new RenderTexture(surfaceSize.x, surfaceSize.y, 0, RenderTextureFormat.ARGBFloat);
            geologySurfaceRT.enableRandomWrite = true;
            geologySurfaceRT.filterMode = FilterMode.Bilinear;
            geologySurfaceRT.Create();

            colouredOutputRT = new RenderTexture(surfaceSize.x, surfaceSize.y, 0, RenderTextureFormat.ARGB32);
            colouredOutputRT.enableRandomWrite = true;
            colouredOutputRT.filterMode = FilterMode.Bilinear;
            colouredOutputRT.Create();
        }
        private void DisposeRenderTextures()
        {
            if (geologySurfaceRT != null)
            {
                geologySurfaceRT.Release();
                geologySurfaceRT.DiscardContents();
                geologySurfaceRT = null;
            }
            if (colouredOutputRT != null)
            {
                colouredOutputRT.Release();
                colouredOutputRT.DiscardContents();
                colouredOutputRT = null;
            }
        }
        void OnEnable()
        {
            CalibrationManager.OnCalibration += OnCalibration;
            Sandbox.OnSandboxReady += OnSandboxReady;

            if (Sandbox.SandboxReady)
            {
                OnSandboxReady();
            }
        }
        void OnDisable()
        {
            CalibrationManager.OnCalibration -= OnCalibration;
            Sandbox.OnSandboxReady -= OnSandboxReady;
            Sandbox.OnNewProcessedData -= OnNewProcessedData;
            Sandbox.SetDefaultShader();
        }
        private void OnCalibration()
        {
            Sandbox.OnNewProcessedData -= OnNewProcessedData;
            Sandbox.SetDefaultShader();
        }

        private void OnSandboxReady()
        {
            sandboxDescriptor = Sandbox.GetSandboxDescriptor();
            SetUpGeologySimulation();
            Sandbox.OnNewProcessedData += OnNewProcessedData;
        }
        private void OnNewProcessedData()
        {
            ComputeShaderHelper.Run_ComputeGeologySurface(GeologyComputeShader, Sandbox.CurrentDepthTexture,
                                                            geologySurfaceRT, simulationDimensions, GeologicalTransformHandler.GetGeologicalTransformBuffer(),
                                                            GeologicalLayerHandler.GetGeologicalLayerBuffer(), GeologicalLayerHandler.GetStartingDepth());
            ComputeShaderHelper.Run_RasteriseGeologySurface(GeologyComputeShader, geologySurfaceRT, colouredOutputRT, 
                                                            GeologicalLayerHandler.GetGeologicalLayerBuffer(), GeologicalTransformHandler.GetFaultColours(),
                                                            GeologicalTransformHandler.GetFoldColours());
        }
        public void LoadSerialisedGeologyFile(SerialisedGeologyFile geologyFile)
        {
            GeologicalLayerHandler.CreateLayersFromGeologyFile(geologyFile);
            GeologicalLayerHandler.SetStartingDepthOffset(geologyFile.BedOffset);
            GeologicalTransformHandler.CreateTransformsFromGeologyFile(geologyFile);
        }
        public SerialisedGeologyFile CreateSerialisedGeologyFile()
        {
            SerialisedGeologyFile geologyFile = new SerialisedGeologyFile();
            geologyFile.GeologicalLayers = GeologicalLayerHandler.GetSerialisedGeologicalLayers();
            geologyFile.GeologicalTransforms = GeologicalTransformHandler.GetSerialisedGeologicalTransforms();
            geologyFile.BedOffset = GeologicalLayerHandler.LayerStartingDepthOffset;

            return geologyFile;
        }
        public void UI_SetStartingDepthOffset(float offset)
        {
            GeologicalLayerHandler.SetStartingDepthOffset(offset);
        }
    }
}
