//  
//  CalibrationManager.cs
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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARSandbox
{
    public enum CalibrationMode
    {
        CornerCalibration,
        DepthCalibration,
        LensShiftCalibration,
    }

    public class CalibrationManager : MonoBehaviour
    {
        public KinectManager KinectManager;
        public Sandbox Sandbox;
        public Camera[] SandboxProjectionCameras;
        public Camera[] SandboxUICameras;
        public Camera[] SandboxDataCameras;

        public LayerMask SandboxLayerMask;
        public GameObject CalibrationPointPrefab;
        public GameObject LineRendererPrefab;

        // Keeps the camera relatively still when lens shifting.
        // TODO: Make this dependent on the depth min / max
        public const float LensShiftOffsetFactor = 180.0f;
        public const float LENS_SHIFT_MIN = -0.02f;
        public const float LENS_SHIFT_MAX = 0.02f;
        private const float LENS_SHIFT_DELTA = 0.000008f;
        private const float CAMERA_TRANSLATION_DELTA = 0.1f;
        private const float CAMERA_SCALE_DELTA = 0.00001f;

        public bool IsCalibrating { get; private set; }

        private static CalibrationDescriptorInternal calibrationDescriptorInternal;
        private static StoredCalibration storedCalibration;

        public delegate void OnCalibration_Delegate();
        public static OnCalibration_Delegate OnCalibration;

        public delegate void OnCalibrationComplete_Delegate();
        public static OnCalibrationComplete_Delegate OnCalibrationComplete;

        private Point frameSize;
        public CalibrationMode CalibrationMode { get; private set; }

        // Corner calibration
        private List<GameObject> kinectCalibrationPoints;
        private LineRenderer[] kinectLines;

        private List<GameObject> sandboxCalibrationPoints;
        private LineRenderer[] sandboxLines;

        private Vector3 grabOffset;
        private GameObject grabbedPoint;
        private bool isKinectCaliPoint;

        private Vector2[] kinectPoints;
        private Vector2[] sandboxPoints;

        // Depth calibration
        public int CurrentDepthCheck { get; private set; }

        // Lens shift calibration
        void Start()
        {
            KinectManager.OnDataStarted += InitialiseCalibration;

            calibrationDescriptorInternal = new CalibrationDescriptorInternal();
        }
        private void Update()
        {
            if (IsCalibrating)
            {
                if (CalibrationMode == CalibrationMode.CornerCalibration)
                {
                    if (SecondDisplayHandler.USING_ONLY_PROJECTOR)
                    {
                        CalibrationPointInteractions();
                    }
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        UI_CompleteBoxCalibration();
                    }
                }
                else if (CalibrationMode == CalibrationMode.DepthCalibration)
                {
                    if (SecondDisplayHandler.USING_ONLY_PROJECTOR)
                    {
                        HandleDepthCalibration(Input.mousePosition, false);
                    }
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        CalibrationMode = CalibrationMode.LensShiftCalibration;
                        Sandbox.RenderRawData(false);
                    }
                }
                else if (CalibrationMode == CalibrationMode.LensShiftCalibration)
                {
                    HandleLensShiftCalibration();
                }
            }
        }
        private void InitialiseCalibration()
        {
            frameSize = KinectManager.GetKinectFrameSize();
            if (!CalibrationFileManager.Load(out storedCalibration))
            {
                CreateInitialCalibration();
            }
            ApplyCurrentCalibration();
            if (OnCalibrationComplete != null) OnCalibrationComplete();
        }
        public CalibrationDescriptor GetCalibrationDescriptor()
        {
            return calibrationDescriptorInternal.GetCalibrationDescriptor();
        }
        public void StartCalibration()
        {
            if (!IsCalibrating)
            {
                if (OnCalibration != null) OnCalibration();

                IsCalibrating = true;
                CalibrationMode = CalibrationMode.CornerCalibration;

                calibrationDescriptorInternal.DataStart = new Point(0, 0);
                calibrationDescriptorInternal.DataEnd = new Point(512, 424);

                Sandbox.UpdateCalibrationDescriptor(calibrationDescriptorInternal.GetCalibrationDescriptor(), false);

                ResetCameras();
                CreateCalibrationBoxes();
            }
        }

        public void UI_SaveCalibration()
        {
            storedCalibration.KinectPoints = ConvertVec2ToFloat(kinectPoints);
            storedCalibration.SandboxPoints = ConvertVec2ToFloat(sandboxPoints);

            storedCalibration.MaxDepth = calibrationDescriptorInternal.MaxDepth - calibrationDescriptorInternal.MaxDepthOffset;
            storedCalibration.MinDepth = calibrationDescriptorInternal.MinDepth - calibrationDescriptorInternal.MinDepthOffset;

            storedCalibration.LensShift = calibrationDescriptorInternal.LensShift;
            storedCalibration.CameraShiftX = calibrationDescriptorInternal.ExtraCameraTranslation.x;
            storedCalibration.CameraShiftY = calibrationDescriptorInternal.ExtraCameraTranslation.y;
            storedCalibration.ScaleX = calibrationDescriptorInternal.ExtraCameraScaling.x;
            storedCalibration.ScaleY = calibrationDescriptorInternal.ExtraCameraScaling.y;

            CalibrationFileManager.Save(storedCalibration);
            IsCalibrating = false;

            SetUpCameras();

            if (OnCalibrationComplete != null) OnCalibrationComplete();
        }

        public void UI_CancelCalibration()
        {
            if (CalibrationMode == CalibrationMode.CornerCalibration)
            {
                RemoveCalibrationBoxes();
            }

            calibrationDescriptorInternal.MaxDepthOffset = 0;
            calibrationDescriptorInternal.MinDepthOffset = 0;

            CalibrationFileManager.Load(out storedCalibration);
            ApplyCurrentCalibration();
            IsCalibrating = false;

            if (OnCalibrationComplete != null) OnCalibrationComplete();
        }
        public void ApplyCurrentCalibration()
        {
            ResetCameras();
            kinectPoints = ConvertFloatToVec2(storedCalibration.KinectPoints);
            sandboxPoints = ConvertFloatToVec2(storedCalibration.SandboxPoints);

            UpdateFromCalibrationPoints();
            ApplyInitialCameraCalibration();

            calibrationDescriptorInternal.LensShift = storedCalibration.LensShift;
            calibrationDescriptorInternal.ExtraCameraScaling = new Vector2(storedCalibration.ScaleX, storedCalibration.ScaleY);
            calibrationDescriptorInternal.ExtraCameraTranslation = new Vector2(storedCalibration.CameraShiftX, storedCalibration.CameraShiftY);

            calibrationDescriptorInternal.MaxDepth = storedCalibration.MaxDepth;
            calibrationDescriptorInternal.MinDepth = storedCalibration.MinDepth;

            SetUpCameras();
        }
        private void ResetCameras()
        {
            Quaternion defaultCameraRotation = new Quaternion();
            defaultCameraRotation.eulerAngles = new Vector3(0, 0, 180);
            Vector3 defaultPosition = new Vector3(frameSize.x / 2.0f * Sandbox.MESH_XY_STRIDE.x,
                                                    frameSize.y / 2.0f * Sandbox.MESH_XY_STRIDE.y, -10);
            float defaultOrthoSize = frameSize.y / 2.0f * Sandbox.MESH_XY_STRIDE.y;
            for (int i = 0; i < SandboxProjectionCameras.Length; i++)
            {
                SandboxProjectionCameras[i].ResetProjectionMatrix();
                SandboxProjectionCameras[i].transform.position = defaultPosition;
                SandboxProjectionCameras[i].orthographicSize = defaultOrthoSize * 1.1f;
                SandboxProjectionCameras[i].transform.rotation = defaultCameraRotation;
            }
            for (int i = 0; i < SandboxUICameras.Length; i++)
            {
                SandboxUICameras[i].transform.position = defaultPosition;
                SandboxUICameras[i].orthographicSize = defaultOrthoSize * 1.1f;
                SandboxUICameras[i].transform.rotation = defaultCameraRotation;
            }
            for (int i = 0; i < SandboxDataCameras.Length; i++)
            {
                SandboxDataCameras[i].transform.position = defaultPosition;
                SandboxDataCameras[i].orthographicSize = defaultOrthoSize;
                SandboxDataCameras[i].aspect = frameSize.x / (float)frameSize.y;
                SandboxDataCameras[i].transform.rotation = Quaternion.identity;
            }
        }
        private void ApplyInitialCameraCalibration()
        {
            for (int i = 0; i < SandboxProjectionCameras.Length; i++)
            {
                SandboxProjectionCameras[i].ResetProjectionMatrix();
                SandboxProjectionCameras[i].transform.position = calibrationDescriptorInternal.ProjectionCameraPos_Orig;
                SandboxProjectionCameras[i].orthographicSize = calibrationDescriptorInternal.ProjectionCameraSize;
                SandboxProjectionCameras[i].transform.rotation = calibrationDescriptorInternal.ProjectionCameraQuaternion;
            }
            calibrationDescriptorInternal.ProjectionCameraMatrix_Orig = SandboxProjectionCameras[0].projectionMatrix;

            for (int i = 0; i < SandboxUICameras.Length; i++)
            {
                SandboxUICameras[i].transform.position = calibrationDescriptorInternal.UICameraPos;
                SandboxUICameras[i].orthographicSize = calibrationDescriptorInternal.UICameraSize;
            }

            for (int i = 0; i < SandboxDataCameras.Length; i++)
            {
                SandboxDataCameras[i].transform.position = calibrationDescriptorInternal.DataCameraPos;
                SandboxDataCameras[i].orthographicSize = calibrationDescriptorInternal.DataCameraSize;
                SandboxDataCameras[i].aspect = calibrationDescriptorInternal.DataCameraAspect;
            }
        }
        private void SetUpCameras()
        {
            ApplyInitialCameraCalibration();
            ApplyLensShift();
        }
        public void SetUpUICamera(Camera UICamera)
        {
            UICamera.ResetProjectionMatrix();
            UICamera.transform.position = calibrationDescriptorInternal.UICameraPos;
            UICamera.orthographicSize = calibrationDescriptorInternal.UICameraSize;
            UICamera.transform.rotation = Quaternion.identity;
        }
        public void SetUpDataCamera(Camera UICamera)
        {
            UICamera.ResetProjectionMatrix();
            UICamera.transform.position = calibrationDescriptorInternal.DataCameraPos;
            UICamera.orthographicSize = calibrationDescriptorInternal.DataCameraSize;
            UICamera.aspect = calibrationDescriptorInternal.DataCameraAspect;
            UICamera.transform.rotation = Quaternion.identity;
        }
        private Vector2[] ConvertFloatToVec2(float[,] floatArray)
        {
            Vector2[] newVec2List = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                newVec2List[i] = new Vector2(floatArray[i, 0], floatArray[i, 1]);
            }
            return newVec2List;
        }
        private float[,] ConvertVec2ToFloat(Vector2[] vec2Array)
        {
            float[,] newFloatList = new float[4, 2];
            for (int i = 0; i < 4; i++)
            {
                newFloatList[i, 0] = vec2Array[i].x;
                newFloatList[i, 1] = vec2Array[i].y;
            }
            return newFloatList;
        }

        private void CreateInitialCalibration()
        {
            storedCalibration = new StoredCalibration();
            float[,] KinectPoints = new float[4, 2];
            // BL
            KinectPoints[0, 0] = frameSize.x * Sandbox.MESH_XY_STRIDE.x * 0.1f;
            KinectPoints[0, 1] = frameSize.y * Sandbox.MESH_XY_STRIDE.y * 0.1f; ;

            // TL
            KinectPoints[1, 0] = frameSize.x * Sandbox.MESH_XY_STRIDE.x * 0.1f;
            KinectPoints[1, 1] = frameSize.y * Sandbox.MESH_XY_STRIDE.y * 0.9f;

            // TR
            KinectPoints[2, 0] = frameSize.x * Sandbox.MESH_XY_STRIDE.x * 0.9f;
            KinectPoints[2, 1] = frameSize.y * Sandbox.MESH_XY_STRIDE.y * 0.9f;

            // BR
            KinectPoints[3, 0] = frameSize.x * Sandbox.MESH_XY_STRIDE.x * 0.9f;
            KinectPoints[3, 1] = frameSize.y * Sandbox.MESH_XY_STRIDE.y * 0.1f; ;

            float[,] SandboxPoints = new float[4, 2];
            // BL
            SandboxPoints[0, 0] = -frameSize.x * Sandbox.MESH_XY_STRIDE.x * 0.01f;
            SandboxPoints[0, 1] = -frameSize.y * Sandbox.MESH_XY_STRIDE.y * 0.01f; ;

            // TL
            SandboxPoints[1, 0] = -frameSize.x * Sandbox.MESH_XY_STRIDE.x * 0.01f;
            SandboxPoints[1, 1] = frameSize.y * Sandbox.MESH_XY_STRIDE.y * 1.01f;

            // TR
            SandboxPoints[2, 0] = frameSize.x * Sandbox.MESH_XY_STRIDE.x * 1.01f;
            SandboxPoints[2, 1] = frameSize.y * Sandbox.MESH_XY_STRIDE.y * 1.01f;

            // BR
            SandboxPoints[3, 0] = frameSize.x * Sandbox.MESH_XY_STRIDE.x * 1.01f;
            SandboxPoints[3, 1] = -frameSize.y * Sandbox.MESH_XY_STRIDE.y * 0.01f; ;

            storedCalibration.KinectPoints = KinectPoints;
            storedCalibration.SandboxPoints = SandboxPoints;

            CalibrationFileManager.Save(storedCalibration);
        }

        // ------ Used by UI for Calibration ------
        public void UI_CompleteBoxCalibration()
        {
            ApplyCalibrationBoxes();
            Sandbox.UpdateCalibrationDescriptor(calibrationDescriptorInternal.GetCalibrationDescriptor(), false);
            ApplyInitialCameraCalibration();

            storedCalibration.KinectPoints = ConvertVec2ToFloat(kinectPoints);
            storedCalibration.SandboxPoints = ConvertVec2ToFloat(sandboxPoints);

            RemoveCalibrationBoxes();
            CalibrationMode = CalibrationMode.DepthCalibration;
            CurrentDepthCheck = 0;
        }
        public void UI_UndoDepthCalibration()
        {
            CreateCalibrationBoxes();

            calibrationDescriptorInternal.DataStart = new Point(0, 0);
            calibrationDescriptorInternal.DataEnd = new Point(frameSize.x, frameSize.y);
            Sandbox.UpdateCalibrationDescriptor(calibrationDescriptorInternal.GetCalibrationDescriptor(), false);
            ResetCameras();

            CalibrationMode = CalibrationMode.CornerCalibration;
        }
        public void UI_UndoLensCalibration()
        {
            for (int i = 0; i < SandboxProjectionCameras.Length; i++)
            {
                SandboxProjectionCameras[i].transform.position = calibrationDescriptorInternal.ProjectionCameraPos_Orig;
                SandboxProjectionCameras[i].transform.rotation = calibrationDescriptorInternal.ProjectionCameraQuaternion;
                SandboxProjectionCameras[i].ResetProjectionMatrix();
            }

            Sandbox.RenderRawData(true);
            CalibrationMode = CalibrationMode.DepthCalibration;
        }
        public void UI_CompleteDepthCalibration()
        {
            Sandbox.RenderRawData(false);
            CalibrationMode = CalibrationMode.LensShiftCalibration;
        }
        public void UI_SetMinDepthOffset(float offset)
        {
            calibrationDescriptorInternal.MinDepthOffset = offset;

            Sandbox.UpdateCalibrationDescriptor(calibrationDescriptorInternal.GetCalibrationDescriptor(), true);
        }
        public void UI_SetMaxDepthOffset(float offset)
        {
            calibrationDescriptorInternal.MaxDepthOffset = offset;

            Sandbox.UpdateCalibrationDescriptor(calibrationDescriptorInternal.GetCalibrationDescriptor(), true);
        }
        public void UI_ChangeLensShift(bool negative)
        {
            if (negative)
            {
                calibrationDescriptorInternal.LensShift -= LENS_SHIFT_DELTA;
                if (calibrationDescriptorInternal.LensShift < LENS_SHIFT_MIN)
                {
                    calibrationDescriptorInternal.LensShift = LENS_SHIFT_MIN;
                }
            }
            else
            {
                calibrationDescriptorInternal.LensShift += LENS_SHIFT_DELTA;
                if (calibrationDescriptorInternal.LensShift > LENS_SHIFT_MAX)
                {
                    calibrationDescriptorInternal.LensShift = LENS_SHIFT_MAX;
                }
            }
        }
        public void UI_ChangeCameraTranslationY(bool negative)
        {
            if (negative)
            {
                calibrationDescriptorInternal.ExtraCameraTranslation.y -= CAMERA_TRANSLATION_DELTA;
            }
            else
            {
                calibrationDescriptorInternal.ExtraCameraTranslation.y += CAMERA_TRANSLATION_DELTA;
            }
        }
        public void UI_ChangeCameraTranslationX(bool negative)
        {
            if (negative)
            {
                calibrationDescriptorInternal.ExtraCameraTranslation.x -= CAMERA_TRANSLATION_DELTA;
            }
            else
            {
                calibrationDescriptorInternal.ExtraCameraTranslation.x += CAMERA_TRANSLATION_DELTA;
            }
        }
        public void UI_ChangeCameraScaleX(bool negative)
        {
            if (negative)
            {
                calibrationDescriptorInternal.ExtraCameraScaling.x -= CAMERA_SCALE_DELTA;
            }
            else
            {
                calibrationDescriptorInternal.ExtraCameraScaling.x += CAMERA_SCALE_DELTA;
            }
        }
        public void UI_ChangeCameraScaleY(bool negative)
        {
            if (negative)
            {
                calibrationDescriptorInternal.ExtraCameraScaling.y -= CAMERA_SCALE_DELTA;
            }
            else
            {
                calibrationDescriptorInternal.ExtraCameraScaling.y += CAMERA_SCALE_DELTA;
            }
        }
        public void UI_ResetLensShiftCalibration()
        {
            calibrationDescriptorInternal.ExtraCameraTranslation = new Vector2(0, 0);
            calibrationDescriptorInternal.ExtraCameraScaling = new Vector2(0, 0);
            calibrationDescriptorInternal.LensShift = 0;
        }

        public CameraCalibrationParameters GetCameraCalibrationParameters()
        {
            CameraCalibrationParameters parameters;
            parameters.LensShift = calibrationDescriptorInternal.LensShift;
            parameters.ExtraCameraScaling = calibrationDescriptorInternal.ExtraCameraScaling;
            parameters.ExtraCameraTranslation = calibrationDescriptorInternal.ExtraCameraTranslation;

            return parameters;
        }

        // --------- Corner Calibration ----------
        private void CreateCalibrationBoxes()
        {
            kinectCalibrationPoints = new List<GameObject>(4);
            sandboxCalibrationPoints = new List<GameObject>(4);
            kinectLines = new LineRenderer[4];
            sandboxLines = new LineRenderer[4];

            for (int i = 0; i < 4; i++)
            {
                Vector3 pointPosition = new Vector3(storedCalibration.KinectPoints[i, 0], storedCalibration.KinectPoints[i, 1], -1);
                GameObject calibrationPoint = Instantiate(CalibrationPointPrefab, pointPosition, Quaternion.identity);
                calibrationPoint.GetComponent<SpriteRenderer>().color = Color.cyan;

                kinectCalibrationPoints.Add(calibrationPoint);
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject newLineRenderer = (GameObject)Instantiate(LineRendererPrefab, Vector3.zero, Quaternion.identity);
                newLineRenderer.SetActive(true);
                kinectLines[i] = newLineRenderer.GetComponent<LineRenderer>();
                kinectLines[i].material.color = Color.green;
                kinectLines[i].positionCount = 2;
                kinectLines[i].SetPosition(0, kinectCalibrationPoints[i].transform.position);
                if (i < 3)
                    kinectLines[i].SetPosition(1, kinectCalibrationPoints[i + 1].transform.position);
                else
                    kinectLines[i].SetPosition(1, kinectCalibrationPoints[0].transform.position);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector3 pointPosition = new Vector3(storedCalibration.SandboxPoints[i, 0], storedCalibration.SandboxPoints[i, 1], -1);
                GameObject calibrationPoint = Instantiate(CalibrationPointPrefab, pointPosition, Quaternion.identity);
                calibrationPoint.GetComponent<SpriteRenderer>().color = Color.red;

                sandboxCalibrationPoints.Add(calibrationPoint);
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject newLineRenderer = (GameObject)Instantiate(LineRendererPrefab, Vector3.zero, Quaternion.identity);
                newLineRenderer.SetActive(true);
                sandboxLines[i] = newLineRenderer.GetComponent<LineRenderer>();
                sandboxLines[i].material.color = Color.magenta;
                sandboxLines[i].positionCount = 2;
                sandboxLines[i].SetPosition(0, sandboxCalibrationPoints[i].transform.position);
                if (i < 3)
                    sandboxLines[i].SetPosition(1, sandboxCalibrationPoints[i + 1].transform.position);
                else
                    sandboxLines[i].SetPosition(1, sandboxCalibrationPoints[0].transform.position);
            }
        }
        private void RemoveCalibrationBoxes()
        {
            for (int i = 0; i < 4; i++)
            {
                Destroy(kinectCalibrationPoints[i]);
                Destroy(kinectLines[i].gameObject);
                Destroy(sandboxCalibrationPoints[i]);
                Destroy(sandboxLines[i].gameObject);
            }
            kinectCalibrationPoints = null;
            kinectLines = null;
            sandboxCalibrationPoints = null;
            sandboxLines = null;
        }
        // Projection cameras at this point are zoomed out.
        private void CalibrationPointInteractions()
        {
            Vector2 mousePos = Input.mousePosition;
            Vector3 worldPos = SandboxUICameras[0].ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
            CalibrationPointInteractions(worldPos);
        }
        public void CalibrationPointInteractions(Vector2 viewportPos)
        {
            Vector2 mousePos = Input.mousePosition;
            Vector3 worldPos = SandboxUICameras[0].ViewportToWorldPoint(viewportPos);
            CalibrationPointInteractions(worldPos);
        }
        public void DropCalibartionPoint()
        {
            if (grabbedPoint != null)
            {
                grabbedPoint.transform.position = new Vector3(grabbedPoint.transform.position.x,
                                                                grabbedPoint.transform.position.y, -1.0f);
                grabbedPoint = null;
            }
        }
        private void CalibrationPointInteractions(Vector3 worldPos)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D collider2D = Physics2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));

                if (collider2D != null)
                {
                    grabbedPoint = collider2D.gameObject;
                    grabOffset = new Vector3(grabbedPoint.transform.position.x - worldPos.x,
                                                grabbedPoint.transform.position.y - worldPos.y, 0);
                    isKinectCaliPoint = kinectCalibrationPoints.Contains(grabbedPoint);
                }
            }
            else if (grabbedPoint != null)
            {
                if (Input.GetMouseButton(0))
                {
                    grabbedPoint.transform.position = new Vector3(worldPos.x, worldPos.y, -1.01f) + grabOffset;

                    if (isKinectCaliPoint)
                    {
                        Vector3 ptPos = grabbedPoint.transform.position;
                        if (ptPos.x < 0) ptPos.x = 0;
                        if (ptPos.y < 0) ptPos.y = 0;
                        if (ptPos.x > frameSize.x * Sandbox.MESH_XY_STRIDE.x) ptPos.x = frameSize.x * Sandbox.MESH_XY_STRIDE.x;
                        if (ptPos.y > frameSize.y * Sandbox.MESH_XY_STRIDE.y) ptPos.y = frameSize.y * Sandbox.MESH_XY_STRIDE.y;
                        grabbedPoint.transform.position = ptPos;

                        UpdateKinectBoundLines();
                    }
                    else
                    {
                        UpdateSandboxBoundLines();
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    DropCalibartionPoint();
                }
            }
        }
        private void UpdateKinectBoundLines()
        {
            Vector3 offset = new Vector3(0, 0, 0.01f);
            for (int i = 0; i < 4; i++)
            {
                kinectLines[i].SetPosition(0, kinectCalibrationPoints[i].transform.position + offset);
                if (i < 3)
                    kinectLines[i].SetPosition(1, kinectCalibrationPoints[i + 1].transform.position + offset);
                else
                    kinectLines[i].SetPosition(1, kinectCalibrationPoints[0].transform.position + offset);
            }
        }
        private void UpdateSandboxBoundLines()
        {
            Vector3 offset = new Vector3(0, 0, 0.01f);
            for (int i = 0; i < 4; i++)
            {
                sandboxLines[i].SetPosition(0, sandboxCalibrationPoints[i].transform.position + offset);
                if (i < 3)
                    sandboxLines[i].SetPosition(1, sandboxCalibrationPoints[i + 1].transform.position + offset);
                else
                    sandboxLines[i].SetPosition(1, sandboxCalibrationPoints[0].transform.position + offset);
            }
        }
        private void ApplyCalibrationBoxes()
        {
            //BL TL TR BR
            for (int i = 0; i < 4; i++)
            {
                kinectPoints[i].x = kinectCalibrationPoints[i].transform.position.x;
                kinectPoints[i].y = kinectCalibrationPoints[i].transform.position.y;

                sandboxPoints[i].x = sandboxCalibrationPoints[i].transform.position.x;
                sandboxPoints[i].y = sandboxCalibrationPoints[i].transform.position.y;
            }

            UpdateFromCalibrationPoints();
        }
        private void UpdateFromCalibrationPoints()
        {
            //BL TL TR BR
            Point[] depthIndex = new Point[4];

            for (int i = 0; i < 4; i++)
            {
                depthIndex[i] = new Point((int)Mathf.Round(kinectPoints[i].x / Sandbox.MESH_XY_STRIDE.x),
                                            (int)Mathf.Round(kinectPoints[i].y / Sandbox.MESH_XY_STRIDE.y));
            }

            int minX = Mathf.Min(Mathf.Min(depthIndex[0].x, depthIndex[1].x),
                                    Mathf.Min(depthIndex[2].x, depthIndex[3].x));

            int maxX = Mathf.Max(Mathf.Max(depthIndex[0].x, depthIndex[1].x),
                                    Mathf.Max(depthIndex[2].x, depthIndex[3].x));

            int minY = Mathf.Min(Mathf.Min(depthIndex[0].y, depthIndex[1].y),
                                    Mathf.Min(depthIndex[2].y, depthIndex[3].y));

            int maxY = Mathf.Max(Mathf.Max(depthIndex[0].y, depthIndex[1].y),
                                    Mathf.Max(depthIndex[2].y, depthIndex[3].y));

            Point dataStart = new Point(minX, minY);
            calibrationDescriptorInternal.DataStart = dataStart;

            Point dataEnd = new Point(maxX, maxY);
            calibrationDescriptorInternal.DataEnd = dataEnd;

            // Calculate new rotation
            float kinectRotation = Mathf.Atan2((kinectPoints[3] - kinectPoints[0]).y, (kinectPoints[3] - kinectPoints[0]).x) * 180 / Mathf.PI;
            float sandboxRotation = Mathf.Atan2((sandboxPoints[3] - sandboxPoints[0]).y, (sandboxPoints[3] - sandboxPoints[0]).x) * 180 / Mathf.PI;

            Vector3 cameraEulerAngles = SandboxProjectionCameras[0].transform.rotation.eulerAngles;
            cameraEulerAngles.z += kinectRotation - sandboxRotation;

            Quaternion cameraQuaternion = new Quaternion();
            cameraQuaternion.eulerAngles = cameraEulerAngles;

            // Calculate new camera scaling
            float scalingFactor = (sandboxPoints[2] - sandboxPoints[0]).magnitude / (kinectPoints[2] - kinectPoints[0]).magnitude;
            float cameraSize = SandboxProjectionCameras[0].orthographicSize / scalingFactor;

            // Calculate the new camera position
            Vector2 kinectCentrePos = (kinectPoints[0] + kinectPoints[2]) / 2.0f;
            Vector2 sandboxCentrePos = (sandboxPoints[0] + sandboxPoints[2]) / 2.0f;
            Vector2 sandboxOffset = (sandboxCentrePos - kinectCentrePos) / scalingFactor;

            calibrationDescriptorInternal.ProjectionCameraPos_Orig = kinectCentrePos - sandboxOffset;
            calibrationDescriptorInternal.ProjectionCameraSize = cameraSize;
            calibrationDescriptorInternal.ProjectionCameraQuaternion = cameraQuaternion;
            calibrationDescriptorInternal.ProjectionCameraRotation = kinectRotation - sandboxRotation;
            calibrationDescriptorInternal.CalculateShiftDirections();

            calibrationDescriptorInternal.UICameraPos = (dataEnd.ToVector() + dataStart.ToVector() + new Vector2(-1, -1)) / 2.0f * Sandbox.MESH_XY_STRIDE.x;
            calibrationDescriptorInternal.UICameraSize = (dataEnd.y - dataStart.y - 1) / 2 * Sandbox.MESH_XY_STRIDE.x + 0.4f;

            calibrationDescriptorInternal.DataCameraPos = (dataEnd.ToVector() + dataStart.ToVector() + new Vector2(-1, -1)) / 2.0f * Sandbox.MESH_XY_STRIDE.x;
            calibrationDescriptorInternal.DataCameraSize = (dataEnd.y - dataStart.y - 1) / 2 * Sandbox.MESH_XY_STRIDE.x;
            calibrationDescriptorInternal.DataCameraAspect = (dataEnd.x - dataStart.x - 1) / (float)(dataEnd.y - dataStart.y - 1);
        }

        // --------- Depth Calibration ----------
        public void HandleDepthCalibration(Vector2 position, bool usingViewportPoint)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray;
                if (usingViewportPoint)
                {
                    ray = SandboxUICameras[0].ViewportPointToRay(position);
                }
                else
                {
                    ray = SandboxUICameras[0].ScreenPointToRay(position);
                }
                RaycastHit hitInfo;
                bool meshHit = Physics.Raycast(ray, out hitInfo, 1000, SandboxLayerMask);
                if (meshHit)
                {
                    float depth = hitInfo.point.z / Sandbox.MESH_Z_SCALE;
                    if (CurrentDepthCheck == 0)
                    {
                        calibrationDescriptorInternal.MaxDepth = depth;
                        CurrentDepthCheck = 1;
                    }
                    else
                    {
                        calibrationDescriptorInternal.MinDepth = depth;
                        CurrentDepthCheck = 0;
                    }
                    Sandbox.UpdateCalibrationDescriptor(calibrationDescriptorInternal.GetCalibrationDescriptor(), true);
                }
            }
        }

        // --------- Lens Shift Calibration ----------
        private void HandleLensShiftCalibration()
        {
            LensShiftKeyboardInput();
            ApplyLensShift();
        }
        private void LensShiftKeyboardInput()
        {
            if (Input.GetKey("q"))
            {
                UI_ChangeLensShift(true);
            }
            if (Input.GetKey("e"))
            {
                UI_ChangeLensShift(false);
            }
            if (Input.GetKey("w"))
            {
                UI_ChangeCameraTranslationY(false);
            }
            if (Input.GetKey("s"))
            {
                UI_ChangeCameraTranslationY(true);
            }
            if (Input.GetKey("a"))
            {
                UI_ChangeCameraTranslationX(false);
            }
            if (Input.GetKey("d"))
            {
                UI_ChangeCameraTranslationX(true);
            }
            if (Input.GetKey("r"))
            {
                UI_ResetLensShiftCalibration();
            }
            if (Input.GetKey("z"))
            {
                UI_ChangeCameraScaleX(true);
            }
            if (Input.GetKey("x"))
            {
                UI_ChangeCameraScaleX(false);
            }
            if (Input.GetKey("c"))
            {
                UI_ChangeCameraScaleY(true);
            }
            if (Input.GetKey("v"))
            {
                UI_ChangeCameraScaleY(false);
            }
        }
        private void ApplyLensShift()
        {
            CalibrationDescriptorInternal cDI = calibrationDescriptorInternal;

            Matrix4x4 p = cDI.ProjectionCameraMatrix_Orig;
            p.m02 += cDI.LensShift * cDI.VerticalShiftDirection.x;
            p.m12 += cDI.LensShift * cDI.VerticalShiftDirection.y;
            p.m00 += cDI.ExtraCameraScaling.x;
            p.m11 += cDI.ExtraCameraScaling.y;
            cDI.ProjectionCameraMatrix_Adjusted = p;

            cDI.ProjectionCameraPos_Adjusted.x = cDI.ProjectionCameraPos_Orig.x
                                            + cDI.LensShift * cDI.VerticalShiftDirection.x * LensShiftOffsetFactor * cDI.ProjectionCameraSize
                                            + cDI.ExtraCameraTranslation.y * cDI.VerticalShiftDirection.x
                                            + cDI.ExtraCameraTranslation.x * cDI.HorizontalShiftDirection.x;
            cDI.ProjectionCameraPos_Adjusted.y = cDI.ProjectionCameraPos_Orig.y
                                            + cDI.LensShift * cDI.VerticalShiftDirection.y * LensShiftOffsetFactor * cDI.ProjectionCameraSize
                                            + cDI.ExtraCameraTranslation.y * cDI.VerticalShiftDirection.y
                                            + cDI.ExtraCameraTranslation.x * cDI.HorizontalShiftDirection.y;

            cDI.ProjectionCameraPos_Adjusted.z = cDI.ProjectionCameraPos_Orig.z;

            for (int i = 0; i < SandboxProjectionCameras.Length; i++)
            {
                SandboxProjectionCameras[i].projectionMatrix = p;
                SandboxProjectionCameras[i].transform.position = cDI.ProjectionCameraPos_Adjusted;
            }
        }
        private class CalibrationDescriptorInternal
        {
            public Point DataStart;
            public Point DataEnd;

            public float MinDepth;
            public float MaxDepth;

            public float MinDepthOffset;
            public float MaxDepthOffset;

            // Projected Camera Positioning
            public Vector3 ProjectionCameraPos_Orig;
            public Vector3 ProjectionCameraPos_Adjusted;
            public float ProjectionCameraSize;
            public float ProjectionCameraRotation;
            public Quaternion ProjectionCameraQuaternion;

            // UI Camera Positioning
            public Vector3 UICameraPos;
            public float UICameraSize;

            // Data Camera Positioning
            public Vector3 DataCameraPos;
            public float DataCameraSize;
            public float DataCameraAspect;

            public Matrix4x4 ProjectionCameraMatrix_Orig;
            public Matrix4x4 ProjectionCameraMatrix_Adjusted;

            public float LensShift;
            public Vector2 ExtraCameraScaling;
            public Vector2 ExtraCameraTranslation;

            public Vector2 VerticalShiftDirection;
            public Vector2 HorizontalShiftDirection;

            public CalibrationDescriptorInternal()
            {
                DataStart = new Point(0, 0);
                DataEnd = new Point(512, 424);
                MinDepth = 1100;
                MaxDepth = 1400;
            }

            public CalibrationDescriptorInternal(Point DataStart, Point DataEnd, float MinDepth, float MaxDepth)
            {
                this.DataStart = DataStart;
                this.DataEnd = DataEnd;
                this.MinDepth = MinDepth;
                this.MaxDepth = MaxDepth;
            }

            public void CalculateShiftDirections()
            {
                VerticalShiftDirection = new Vector2();
                VerticalShiftDirection.x = Mathf.Sin(ProjectionCameraRotation / 180.0f * Mathf.PI);
                VerticalShiftDirection.y = Mathf.Cos(ProjectionCameraRotation / 180.0f * Mathf.PI);

                HorizontalShiftDirection = new Vector2();
                HorizontalShiftDirection.x = -Mathf.Sin(ProjectionCameraRotation / 180.0f * Mathf.PI - Mathf.PI / 2);
                HorizontalShiftDirection.y = Mathf.Cos(ProjectionCameraRotation / 180.0f * Mathf.PI - Mathf.PI / 2);
            }
            public CalibrationDescriptor GetCalibrationDescriptor()
            {
                return new CalibrationDescriptor(DataStart, DataEnd, MinDepth - MinDepthOffset, MaxDepth - MaxDepthOffset);
            }
        }
    }

    public struct CameraCalibrationParameters
    {
        public float LensShift;
        public Vector2 ExtraCameraScaling;
        public Vector2 ExtraCameraTranslation;
    }
}