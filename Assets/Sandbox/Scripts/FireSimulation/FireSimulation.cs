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

namespace ARSandbox.FireSimulation
{
    public class FireSimulation : MonoBehaviour
    {
        public Sandbox Sandbox;
        public HandInput HandInput;
        public CalibrationManager CalibrationManager;
        public Shader FireVisualShader;
        public ComputeShader FireSimulationShader;

        private Point fireSimSize = new Point(1920, 1080);
        private RenderTexture fireRasterisedRT;
        private RenderTexture fireLandscapeRT_0;
        private RenderTexture fireLandscapeRT_1;
        CSS_FireCellMaterial[] fireCellMaterials;

        private SandboxDescriptor sandboxDescriptor;

        private bool swapBuffers;
        public float WindDirection { get; private set; }
        public float WindSpeed { get; private set; }
        public bool SimulationPaused { get; private set; }
        public float LandscapeZoom { get; private set; }
        private float TrueZoom;
        float randomSeed;
        private bool FirstRun = true;

        private IEnumerator RunSimulationCoroutine;
        private void InitialiseSimulation()
        {
            sandboxDescriptor = Sandbox.GetSandboxDescriptor();
            if (FirstRun)
            {
                CreateFireCellMaterials();
                randomSeed = Random.value * 10000.0f;
                LandscapeZoom = 1.0f;
                WindSpeed = 0;
                WindDirection = 0;

                FirstRun = false;
            }

            SimulationPaused = false;
            float sandboxAspect = (float)sandboxDescriptor.DataSize.x / (float)sandboxDescriptor.DataSize.y;
            fireSimSize = new Point(Mathf.FloorToInt(sandboxAspect * 1080.0f), 1080);

            DisposeRenderTextures();
            CreateRenderTextures();

            GenerateLandscape();

            Sandbox.SetSandboxShader(FireVisualShader);
            Sandbox.SetShaderTexture("_FireSurfaceTex", fireRasterisedRT);

            swapBuffers = false;
            StartCoroutine(RunSimulationCoroutine = RunSimulation());
        }
        private void GenerateLandscape()
        {
            TrueZoom = 2.4f * (1 / LandscapeZoom);
            FireSimulationCSHelper.Run_GenerateLandscape(FireSimulationShader, fireLandscapeRT_0, randomSeed, TrueZoom);
            FireSimulationCSHelper.Run_GenerateLandscape(FireSimulationShader, fireLandscapeRT_1, randomSeed, TrueZoom);
            FireSimulationCSHelper.Run_RasteriseFireSimulation(FireSimulationShader, fireLandscapeRT_0, fireRasterisedRT, fireCellMaterials);
        }
        private void OnEnable()
        {
            HandInput.OnGesturesReady += OnGesturesReady;
            CalibrationManager.OnCalibration += OnCalibration;

            InitialiseSimulation();
        }
        private void OnDisable()
        {
            HandInput.OnGesturesReady -= OnGesturesReady;
            CalibrationManager.OnCalibration -= OnCalibration;
            Sandbox.OnSandboxReady -= OnSandboxReady;

            DisposeRenderTextures();
            Sandbox.SetDefaultShader();

            StopCoroutine(RunSimulationCoroutine);
        }
        private void OnCalibration()
        {
            HandInput.OnGesturesReady -= OnGesturesReady;
            Sandbox.OnSandboxReady += OnSandboxReady;

            DisposeRenderTextures();
            StopCoroutine(RunSimulationCoroutine);
        }
        // Called only after a successful calibration.
        private void OnSandboxReady()
        {
            HandInput.OnGesturesReady += OnGesturesReady;

            InitialiseSimulation();
        }
        private void OnGesturesReady()
        {
            List<CSS_FireStartPoint> firePoints = new List<CSS_FireStartPoint>();
            foreach (HandInputGesture gesture in HandInput.GetCurrentGestures())
            {
                if (!gesture.OutOfBounds)
                {
                    CSS_FireStartPoint fireStartPoint;
                    Vector2 gestureNormPos = gesture.NormalisedPosition;
                    Point startPoint = new Point(Mathf.FloorToInt(gestureNormPos.x * fireSimSize.x), 
                                                    Mathf.FloorToInt(gestureNormPos.y * fireSimSize.y));
                    fireStartPoint.Position = startPoint;
                    fireStartPoint.Radius = 1;

                    firePoints.Add(fireStartPoint);
                }
            }
            if (firePoints.Count > 0)
            {
                CSS_FireStartPoint[] firePointArr = firePoints.ToArray();
                FireSimulationCSHelper.Run_StartFire(FireSimulationShader, fireLandscapeRT_0, firePointArr);
                FireSimulationCSHelper.Run_StartFire(FireSimulationShader, fireLandscapeRT_1, firePointArr);
                FireSimulationCSHelper.Run_RasteriseFireSimulation(FireSimulationShader, fireLandscapeRT_0, fireRasterisedRT, fireCellMaterials);
            }
        }
        private void CreateRenderTextures()
        {
            fireRasterisedRT = new RenderTexture(fireSimSize.x, fireSimSize.y, 0, RenderTextureFormat.ARGB32);
            fireRasterisedRT.enableRandomWrite = true;
            fireRasterisedRT.filterMode = FilterMode.Bilinear;
            fireRasterisedRT.Create();

            fireLandscapeRT_0 = new RenderTexture(fireSimSize.x, fireSimSize.y, 0, RenderTextureFormat.ARGBFloat);
            fireLandscapeRT_0.enableRandomWrite = true;
            fireLandscapeRT_0.filterMode = FilterMode.Point;
            fireLandscapeRT_0.Create();

            fireLandscapeRT_1 = new RenderTexture(fireSimSize.x, fireSimSize.y, 0, RenderTextureFormat.ARGBFloat);
            fireLandscapeRT_1.enableRandomWrite = true;
            fireLandscapeRT_1.filterMode = FilterMode.Point;
            fireLandscapeRT_1.Create();
        }
        private void DisposeRenderTextures()
        {
            if (fireRasterisedRT != null)
            {
                fireRasterisedRT.Release();
                fireRasterisedRT = null;
            }
            if (fireLandscapeRT_0 != null)
            {
                fireLandscapeRT_0.Release();
                fireLandscapeRT_0 = null;
            }
            if (fireLandscapeRT_1 != null)
            {
                fireLandscapeRT_1.Release();
                fireLandscapeRT_1 = null;
            }
        }
        private void CreateFireCellMaterials()
        {
            fireCellMaterials = new CSS_FireCellMaterial[4];

            CSS_FireCellMaterial fireMaterial0;
            fireMaterial0.BurnRate = 0.1f;
            fireMaterial0.BurnoutTime = 60;
            fireMaterial0.Colour = new Color(0, 0.2f, 0, 1);
            fireMaterial0.BurntColour = new Color(0.1f, 0.1f, 0.1f, 1);

            CSS_FireCellMaterial fireMaterial1;
            fireMaterial1.BurnRate = 0.2f;
            fireMaterial1.BurnoutTime = 40;
            fireMaterial1.Colour = new Color(0, 0.4f, 0, 1);
            fireMaterial1.BurntColour = new Color(0.2f, 0.2f, 0.2f, 1);

            CSS_FireCellMaterial fireMaterial2;
            fireMaterial2.BurnRate = 0.4f;
            fireMaterial2.BurnoutTime = 20;
            fireMaterial2.Colour = new Color(0, 0.6f, 0, 1);
            fireMaterial2.BurntColour = new Color(0.3f, 0.3f, 0.3f, 1);

            CSS_FireCellMaterial fireMaterial3;
            fireMaterial3.BurnRate = 0.6f;
            fireMaterial3.BurnoutTime = 15;
            fireMaterial3.Colour = new Color(0, 0.8f, 0, 1);
            fireMaterial3.BurntColour = new Color(0.4f, 0.4f, 0.4f, 1);

            fireCellMaterials[0] = fireMaterial0;
            fireCellMaterials[1] = fireMaterial1;
            fireCellMaterials[2] = fireMaterial2;
            fireCellMaterials[3] = fireMaterial3;
        }

        IEnumerator RunSimulation()
        {
            while (true)
            {
                if (!SimulationPaused)
                {
                    if (swapBuffers)
                    {
                        FireSimulationCSHelper.Run_StepFireSimulation(FireSimulationShader, sandboxDescriptor.ProcessedDepthsRT, fireLandscapeRT_1, fireLandscapeRT_0,
                                                                        fireCellMaterials, CalculateWindCoefficients(WindDirection, WindSpeed),
                                                                        TrueZoom);
                        FireSimulationCSHelper.Run_RasteriseFireSimulation(FireSimulationShader, fireLandscapeRT_0, fireRasterisedRT, fireCellMaterials);
                    }
                    else
                    {
                        FireSimulationCSHelper.Run_StepFireSimulation(FireSimulationShader, sandboxDescriptor.ProcessedDepthsRT, fireLandscapeRT_0, fireLandscapeRT_1,
                                                                        fireCellMaterials, CalculateWindCoefficients(WindDirection, WindSpeed),
                                                                        TrueZoom);
                        FireSimulationCSHelper.Run_RasteriseFireSimulation(FireSimulationShader, fireLandscapeRT_1, fireRasterisedRT, fireCellMaterials);
                    }
                    swapBuffers = !swapBuffers;
                }

                yield return new WaitForSeconds(1 / 60.0f);
            }
        }
        private WindCoefficients CalculateWindCoefficients(float angleInDegrees, float amplitude)
        {
            float amplitudeCutoff = 0.2f;
            float angleInRads = angleInDegrees * Mathf.Deg2Rad;
            WindCoefficients coeff;
            coeff.N = 1 + amplitude * Mathf.Sin(angleInRads);
            coeff.N = Mathf.Clamp(coeff.N, 0, 100);
            coeff.NE = 1 + amplitude * Mathf.Sin(angleInRads + Mathf.PI / 4.0f);
            coeff.NE = Mathf.Clamp(coeff.NE, 0, 100);
            coeff.E = 1 + amplitude * Mathf.Sin(angleInRads + Mathf.PI / 2.0f);
            coeff.E = Mathf.Clamp(coeff.E, 0, 100);
            coeff.SE = 1 + amplitude * Mathf.Sin(angleInRads + 3 * Mathf.PI / 4.0f);
            coeff.SE = Mathf.Clamp(coeff.SE, 0, 100);
            coeff.S = 1 + amplitude * Mathf.Sin(angleInRads + Mathf.PI);
            coeff.S = Mathf.Clamp(coeff.S, 0, 100);
            coeff.SW = 1 + amplitude * Mathf.Sin(angleInRads + 5 * Mathf.PI / 4.0f);
            coeff.SW = Mathf.Clamp(coeff.SW, 0, 100);
            coeff.W = 1 + amplitude * Mathf.Sin(angleInRads + 3 * Mathf.PI / 2.0f);
            coeff.W = Mathf.Clamp(coeff.W, 0, 100);
            coeff.NW = 1 + amplitude * Mathf.Sin(angleInRads + 7 * Mathf.PI / 4.0f);
            coeff.NW = Mathf.Clamp(coeff.NW, 0, 100);

            coeff.N = coeff.N < amplitudeCutoff ? 0 : coeff.N;
            coeff.NE = coeff.NE < amplitudeCutoff ? 0 : coeff.NE;
            coeff.E = coeff.E < amplitudeCutoff ? 0 : coeff.E;
            coeff.SE = coeff.SE < amplitudeCutoff ? 0 : coeff.SE;
            coeff.S = coeff.S < amplitudeCutoff ? 0 : coeff.S;
            coeff.SW = coeff.SW < amplitudeCutoff ? 0 : coeff.SW;
            coeff.W = coeff.W < amplitudeCutoff ? 0 : coeff.W;
            coeff.NW = coeff.NW < amplitudeCutoff ? 0 : coeff.NW;

            return coeff;
        }

        public void UI_ChangeWindDirection(float windDirection)
        {
            this.WindDirection = windDirection;
        }

        public void UI_ChangeWindSpeed(float windSpeed)
        {
            this.WindSpeed = windSpeed;
        }

        public void UI_ResetLandscape()
        {
            FireSimulationCSHelper.Run_ResetLandscape(FireSimulationShader, fireLandscapeRT_0);
            FireSimulationCSHelper.Run_ResetLandscape(FireSimulationShader, fireLandscapeRT_1);
            FireSimulationCSHelper.Run_RasteriseFireSimulation(FireSimulationShader, fireLandscapeRT_0, fireRasterisedRT, fireCellMaterials);
        }
        public void UI_RandomiseFlora()
        {
            randomSeed = Random.value * 100000.0f;
            GenerateLandscape();
        }

        public void UI_SetLandscapeZoom(float LandscapeZoom)
        {
            this.LandscapeZoom = LandscapeZoom;
            GenerateLandscape();
        }

        public bool TogglePauseSimulation()
        {
            SimulationPaused = !SimulationPaused;
            return SimulationPaused;
        }
    }
}