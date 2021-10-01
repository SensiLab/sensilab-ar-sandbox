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

using System;
using UnityEngine;
using Windows.Kinect;

namespace ARSandbox
{
    public enum SandboxResolution
    {
        RawData,
        Downsampled_3x,
        Downsampled_2x,
        Downsampled_1x,
        Original,
    }
    public enum SandboxRenderMaterial
    {
        Normal,
        DepthData,
        ContourData,
        BlackAndWhite
    }
    public class Sandbox : MonoBehaviour
    {
        public const float MESH_Z_SCALE = 1 / 8000.0f * 2000.0f;
        public const int COLL_MESH_DELAY = 15; // Amount of new frames needed to update the collider mesh.
        public const float ALPHA_1 = 0.3f;
        public const float ALPHA_2 = 0.05f;
        public readonly static Vector2 MESH_XY_STRIDE = new Vector2(0.5f, 0.5f);
        public readonly static Vector3 MESH_POSITION = new Vector3(0, 0, 0);

        public KinectManager KinectManager;
        public CalibrationManager CalibrationManager;
        public ComputeShader SandboxProcessingShader;
        public Shader DefaultSandboxShader;
        public Texture2D ColourScaleTexture;
        public SandboxDataCamera SandboxDataCamera;
        public TopographyLabelManager TopographyTextHandler;

        public Vector2 MESH_XY_STRIDE_DS1 { get; private set; }
        public Vector2 MESH_XY_STRIDE_DS2 { get; private set; }
        public Vector2 MESH_XY_STRIDE_DS3 { get; private set; }

        [Range(0, 50)]
        public float NoiseTolerance = 5.0f;

        [Range(0, 60)]
        public float LowPassHoldTime = 30.0f;

        public float MajorContourSpacing { get; private set; }
        public int MinorContours { get; private set; }
        public float ContourThickness { get; private set; }
        public bool SandboxReady{ get; private set; }
        public bool UsingCustomShader { get; private set; }
        public SandboxResolution SandboxResolution { get; private set; }
        public Texture CurrentDepthTexture { get; private set; }
        public bool DynamicLabelColouring { get; private set; }

        private SandboxRenderMaterial sandboxRenderMaterial;
        
        private bool initialCalibrationComplete;

        public delegate void OnSandboxReady_Delegate();
        public static OnSandboxReady_Delegate OnSandboxReady;

        public delegate void OnNewProcessedData_Delegate();
        public static OnNewProcessedData_Delegate OnNewProcessedData;

        // Attached components
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private Mesh colliderMesh;
        private FrameDescription kinectFrameDesc;

        private BoxCollider LeftCollider, TopCollider, RightCollider, BottomCollider;

        private SandboxDescriptor sandboxDescriptor;
        private CalibrationDescriptor calibrationDescriptor;
        private Material NormalMaterial, DepthDataMaterial, ContourDataMaterial, BlackAndWhiteMaterial;

        private Vector3 meshStart = Vector3.zero;
        private bool setInitialLowPassData = true;

        private Texture2D rawDepthsTex;
        private RenderTexture rawDepthsRT_DS, rawDepthsRT_DS2, rawDepthsRT_DS3;
        private RenderTexture processedDepthsRT, processedDepthsRT_DS, 
                                  processedDepthsRT_DS2, processedDepthsRT_DS3;
        private RenderTexture internalLowPassDataRT, lowPassCounterRT, lowPassDataRT;
        private RenderTexture blurredDataTempRT, blurredDataDSTempRT, blurredDataDS2TempRT;
        private ComputeBuffer proceduralVertices_Buffer, proceduralUV_Buffer;
        private ComputeBuffer proceduralVertices_DS_Buffer, proceduralUV_DS_Buffer;
        private ComputeBuffer proceduralVertices_DS2_Buffer, proceduralUV_DS2_Buffer;
        private ComputeBuffer proceduralVertices_DS3_Buffer, proceduralUV_DS3_Buffer;
        private ComputeBuffer collMeshVertices_Buffer, collMeshUV_Buffer, collMeshTris_Buffer;

        private Texture TopographyLabelMaskTex;

        private Vector3[] collMeshVertices;
        private int[] collMeshTris;
        private int colliderDelay = 0;

        private byte[] rawDepthData;
        public ushort[] depthDataBuffer { get; private set; }

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();

            initialCalibrationComplete = false;
            UsingCustomShader = false;
            MajorContourSpacing = 15f;
            MinorContours = 1;
            ContourThickness = 15f;
            DynamicLabelColouring = true;

            colliderMesh = new Mesh();

            CalibrationManager.OnCalibrationComplete += OnCalibrationComplete;
            CalibrationManager.OnCalibration += OnCalibration;

            NormalMaterial = meshRenderer.materials[0];
            DepthDataMaterial = meshRenderer.materials[1];
            ContourDataMaterial = meshRenderer.materials[2];
            BlackAndWhiteMaterial = meshRenderer.materials[3];
        }

        private void Update()
        {
            if (initialCalibrationComplete)
            {
                LoadData();

                if (Input.GetKeyDown("="))
                {
                    CalibrationManager.StartCalibration();
                }
            }
        }
        private void OnDestroy()
        {
            ReleaseBuffers();
        }
        public void GetSandboxProceduralMaterials(out Material material, out int totalVerts)
        {
            Material sandboxMaterial;
            switch (sandboxRenderMaterial)
            {
                case SandboxRenderMaterial.Normal:
                    sandboxMaterial = NormalMaterial;
                    SetShaderProperties(NormalMaterial);
                    break;
                case SandboxRenderMaterial.DepthData:
                    sandboxMaterial = DepthDataMaterial;
                    SetShaderProperties(DepthDataMaterial);
                    break;
                case SandboxRenderMaterial.ContourData:
                    sandboxMaterial = ContourDataMaterial;
                    SetShaderProperties(ContourDataMaterial);
                    break;
                case SandboxRenderMaterial.BlackAndWhite:
                    sandboxMaterial = BlackAndWhiteMaterial;
                    SetShaderProperties(BlackAndWhiteMaterial);
                    break;
                default:
                    sandboxMaterial = NormalMaterial;
                    break;
            }

            ComputeBuffer verticesBuffer, uvBuffer;
            int meshWidth;
            switch (SandboxResolution)
            {
                case SandboxResolution.Downsampled_1x:
                    verticesBuffer = proceduralVertices_DS_Buffer;
                    uvBuffer = proceduralUV_DS_Buffer;
                    totalVerts = (calibrationDescriptor.DataSize_DS.x - 1) * (calibrationDescriptor.DataSize_DS.y - 1) * 6;
                    meshWidth = calibrationDescriptor.DataSize_DS.x;
                    break;
                case SandboxResolution.Downsampled_2x:
                    verticesBuffer = proceduralVertices_DS2_Buffer;
                    uvBuffer = proceduralUV_DS2_Buffer;
                    totalVerts = (calibrationDescriptor.DataSize_DS2.x - 1) * (calibrationDescriptor.DataSize_DS2.y - 1) * 6;
                    meshWidth = calibrationDescriptor.DataSize_DS2.x;
                    break;
                case SandboxResolution.Downsampled_3x:
                    verticesBuffer = proceduralVertices_DS3_Buffer;
                    uvBuffer = proceduralUV_DS3_Buffer;
                    totalVerts = (calibrationDescriptor.DataSize_DS3.x - 1) * (calibrationDescriptor.DataSize_DS3.y - 1) * 6;
                    meshWidth = calibrationDescriptor.DataSize_DS3.x;
                    break;
                default:
                    verticesBuffer = proceduralVertices_Buffer;
                    uvBuffer = proceduralUV_Buffer;
                    totalVerts = (calibrationDescriptor.DataSize.x - 1) * (calibrationDescriptor.DataSize.y - 1) * 6;
                    meshWidth = calibrationDescriptor.DataSize.x;
                    break;
            }

            sandboxMaterial.SetPass(0);
            sandboxMaterial.SetMatrix("Mat_Object2World", transform.localToWorldMatrix);
            sandboxMaterial.SetBuffer("VertexBuffer", verticesBuffer);
            sandboxMaterial.SetBuffer("UVBuffer", uvBuffer);
            sandboxMaterial.SetInt("MeshWidth", meshWidth);

            material = sandboxMaterial;
        }
        public int[] GetProcessedDepthsArray()
        {
            return SandboxCSHelper.Run_ExtractDepthData(SandboxProcessingShader, processedDepthsRT);
        }
        public void SetHeightTexture(Texture heightTexture)
        {
            CurrentDepthTexture = heightTexture;
            NormalMaterial.SetTexture("_HeightTex", CurrentDepthTexture);
        }
        public void SetRenderMaterial(SandboxRenderMaterial sandboxRenderMaterial)
        {
            this.sandboxRenderMaterial = sandboxRenderMaterial;
        }

        public void SetSandboxShader(Shader sandboxShader)
        {
            NormalMaterial.shader = sandboxShader;
            UsingCustomShader = true;
            SetShaderProperties(NormalMaterial);
        }
        public void SetShaderTexture(string textureName, Texture texture)
        {
            NormalMaterial.SetTexture(textureName, texture);
        }
        public void SetShaderFloat(string floatName, float value)
        {
            if (SandboxReady)
            {
                NormalMaterial.SetFloat(floatName, value);
            }
        }
        public void SetShaderInt(string floatName, int value)
        {
            if (SandboxReady)
            {
                NormalMaterial.SetInt(floatName, value);
            }
        }
        public void SetShaderFloatArray(string floatName, float[] values)
        {
            if (SandboxReady)
            {
                NormalMaterial.SetFloatArray(floatName, values);
            }
        }
        public void SetTextureProperties(string textureName, Vector2 offset, Vector2 scaling)
        {
            NormalMaterial.SetTextureOffset(textureName, offset);
            NormalMaterial.SetTextureScale(textureName, scaling);
        }
        public void SetTopographyLabelMaskRT(Texture labelMaskRT)
        {
            TopographyLabelMaskTex = labelMaskRT;
        }
        public void SetDefaultShader()
        {
            if (meshRenderer != null)
            {
                NormalMaterial.shader = DefaultSandboxShader;
                UsingCustomShader = false;
                SetShaderProperties(NormalMaterial);
                if (forcedTextureEnabled)
                {
                    setInitialLowPassData = true;
                    forcedTextureEnabled = false;
                }
            }
        }
        private void SetShaderProperties(Material material)
        {
            material.SetFloat("_ContourStride", MajorContourSpacing);
            material.SetInt("_MinorContours", MinorContours);
            material.SetFloat("_ContourWidth", ContourThickness);
            material.SetFloat("_MaxDepth", calibrationDescriptor.MaxDepth);
            material.SetFloat("_MinDepth", calibrationDescriptor.MinDepth);
            material.SetTexture("_HeightTex", CurrentDepthTexture);
            material.SetTexture("_ColorScaleTex", ColourScaleTexture);
            material.SetTexture("_LabelMaskTex", TopographyLabelMaskTex);
            material.SetInt("_DynamicLabelColouring", DynamicLabelColouring ? 1 : 0);
        }
        public void UI_ChangeDynamicLabelColouring(bool dynamicLabelColouring)
        {
            this.DynamicLabelColouring = dynamicLabelColouring;
        }
        public void UI_ChangeResolution(float resolution)
        {
            int resolution_int = (int)resolution;

            SandboxResolution = (SandboxResolution)resolution_int;
            SandboxDataCamera.UpdateSandboxResolution(SandboxResolution);
        }
        public void UI_ChangeMajorContourSpacing(float spacing)
        {
            // Spacing is doubled on the slider.
            MajorContourSpacing = spacing / 2.0f;

            NormalMaterial.SetFloat("_ContourStride", MajorContourSpacing);
        }
        public void UI_ChangeMinorContourAmount(float amount)
        {
            MinorContours = (int)amount;

            NormalMaterial.SetInt("_MinorContours", MinorContours);
        }
        public void UI_ChangeContourThickness(float thickness)
        {
            ContourThickness = thickness * 30;

            NormalMaterial.SetFloat("_ContourWidth", ContourThickness);
        }
        public void UpdateCalibrationDescriptor(CalibrationDescriptor calibrationDescriptor, bool depthsOnly)
        {
            this.calibrationDescriptor = calibrationDescriptor;

            NormalMaterial.SetFloat("_MaxDepth", calibrationDescriptor.MaxDepth);
            NormalMaterial.SetFloat("_MinDepth", calibrationDescriptor.MinDepth);

            SandboxDataCamera.UpdateCalibrationDescriptor(calibrationDescriptor);
            if (!depthsOnly)
            {
                ReleaseBuffers();
                InitialiseBuffers();

                meshStart = MESH_POSITION + new Vector3(calibrationDescriptor.DataStart.x * MESH_XY_STRIDE.x,
                                                        calibrationDescriptor.DataStart.y * MESH_XY_STRIDE.y, 0);
            }
        }
        public SandboxDescriptor GetSandboxDescriptor()
        {
            return sandboxDescriptor;
        }
        public void RenderRawData(bool renderRawData)
        {
            if(renderRawData)
            {
                SandboxResolution = SandboxResolution.RawData;
            } else
            {
                SandboxResolution = SandboxResolution.Original;
                setInitialLowPassData = true;
            }
            SandboxDataCamera.UpdateSandboxResolution(SandboxResolution);
        }
        // Really need to dig and find out why this is needed.
        // Multiply by a normalised value (width and height) to get proper position.
        public Vector2 GetAdjustedMeshSize()
        {
            float meshWidth, meshHeight;
            switch (SandboxResolution)
            {
                case SandboxResolution.Downsampled_1x:
                    meshWidth = sandboxDescriptor.MeshWidth - sandboxDescriptor.MeshWidth / (float)(calibrationDescriptor.DataSize_DS.x);
                    meshHeight = sandboxDescriptor.MeshHeight - sandboxDescriptor.MeshHeight / (float)(calibrationDescriptor.DataSize_DS.y);
                    break;
                case SandboxResolution.Downsampled_2x:
                    meshWidth = sandboxDescriptor.MeshWidth - sandboxDescriptor.MeshWidth / (float)(calibrationDescriptor.DataSize_DS2.x);
                    meshHeight = sandboxDescriptor.MeshHeight - sandboxDescriptor.MeshHeight / (float)(calibrationDescriptor.DataSize_DS2.y);
                    break;
                case SandboxResolution.Downsampled_3x:
                    meshWidth = sandboxDescriptor.MeshWidth - sandboxDescriptor.MeshWidth / (float)(calibrationDescriptor.DataSize_DS3.x);
                    meshHeight = sandboxDescriptor.MeshHeight - sandboxDescriptor.MeshHeight / (float)(calibrationDescriptor.DataSize_DS3.y);
                    break;
                default:
                    meshWidth = sandboxDescriptor.MeshWidth;
                    meshHeight = sandboxDescriptor.MeshHeight;
                    break;
            }

            return new Vector2(meshWidth, meshHeight);
        }
        public Vector2 WorldPosToNormalisedPos(Vector3 worldPosition)
        {
            Vector2 normalisedPosition = new Vector2();
            normalisedPosition.x = (worldPosition.x - sandboxDescriptor.MeshStart.x) / sandboxDescriptor.MeshWidth;
            normalisedPosition.y = (worldPosition.y - sandboxDescriptor.MeshStart.y) / sandboxDescriptor.MeshHeight;

            return normalisedPosition;
        }
        public Point WorldPosToDataPos(Vector3 worldPosition)
        {
            Point dataPosition = new Point();
            dataPosition.x = (int)((worldPosition.x - sandboxDescriptor.MeshStart.x + MESH_XY_STRIDE_DS3.x) / MESH_XY_STRIDE.x);
            dataPosition.y = (int)((worldPosition.y - sandboxDescriptor.MeshStart.y + MESH_XY_STRIDE_DS3.y) / MESH_XY_STRIDE.y);

            return dataPosition;
        }
        public Vector3 DataPosToWorldPos(Point dataPosition)
        {
            Vector3 worldPosition = new Vector3();
            worldPosition.x = dataPosition.x * MESH_XY_STRIDE.x + sandboxDescriptor.MeshStart.x;
            worldPosition.y = dataPosition.y * MESH_XY_STRIDE.y + sandboxDescriptor.MeshStart.y;

            return worldPosition;
        }
        public float GetDepthFromWorldPos(Vector3 worldPosition)
        {
            Point dataPos = WorldPosToDataPos(worldPosition);
            Point dataPos_DS3 = new Point(dataPos.x / 8, dataPos.y / 8);

            return GetDepthFromDataPos_DS3(dataPos_DS3);
        }
        public float GetDepthFromDataPos_DS3(Point dataPosition_DS3)
        {
            int index = dataPosition_DS3.y * sandboxDescriptor.DataSize_DS3.x + dataPosition_DS3.x;
            if (index < 0 || index > collMeshVertices.Length)
            {
                return -1;
            }
            return collMeshVertices[index].z;
        }
        private void OnCalibration()
        {
            SandboxResolution = SandboxResolution.RawData;
            SandboxDataCamera.UpdateSandboxResolution(SandboxResolution);
            meshStart = Vector3.zero;
        }
        private void OnCalibrationComplete()
        {
            if (!initialCalibrationComplete)
            {
                kinectFrameDesc = KinectManager.GetKinectFrameDescriptor();
                CreateBoxColliders();
                initialCalibrationComplete = true;
            }
            else
            {
                ReleaseBuffers();
                sandboxDescriptor = null;
            }

            calibrationDescriptor = CalibrationManager.GetCalibrationDescriptor();
            NormalMaterial.SetFloat("_MaxDepth", calibrationDescriptor.MaxDepth);
            NormalMaterial.SetFloat("_MinDepth", calibrationDescriptor.MinDepth);

            setInitialLowPassData = true;
            SandboxResolution = SandboxResolution.Downsampled_1x;
            
            InitialiseBuffers();
            
            // Set up the sandbox descriptor.
            meshStart = MESH_POSITION + new Vector3(calibrationDescriptor.DataStart.x * MESH_XY_STRIDE.x,
                                                        calibrationDescriptor.DataStart.y * MESH_XY_STRIDE.y, 0);
            Vector3 meshEnd = MESH_POSITION + new Vector3((calibrationDescriptor.DataEnd.x - 1) * MESH_XY_STRIDE.x,
                                                               (calibrationDescriptor.DataEnd.y - 1) * MESH_XY_STRIDE.y, 0);

            sandboxDescriptor = new SandboxDescriptor(meshStart, meshEnd, MESH_XY_STRIDE.x,
                                                      meshCollider, collMeshVertices, rawDepthsTex, rawDepthsRT_DS, rawDepthsRT_DS2,
                                                      processedDepthsRT, processedDepthsRT_DS, processedDepthsRT_DS2, processedDepthsRT_DS3,
                                                      calibrationDescriptor.DataSize, calibrationDescriptor.DataSize_DS, calibrationDescriptor.DataSize_DS2,
                                                      calibrationDescriptor.DataSize_DS3, calibrationDescriptor.MinDepth, calibrationDescriptor.MaxDepth, MESH_Z_SCALE);

            float meshWidth = sandboxDescriptor.MeshWidth;
            float meshHeight = sandboxDescriptor.MeshHeight;
            MESH_XY_STRIDE_DS1 = new Vector2(meshWidth / (float)(calibrationDescriptor.DataSize_DS.x - 1), meshHeight / (float)(calibrationDescriptor.DataSize_DS.y - 1));
            MESH_XY_STRIDE_DS2 = new Vector2(meshWidth / (float)(calibrationDescriptor.DataSize_DS2.x - 1), meshHeight / (float)(calibrationDescriptor.DataSize_DS2.y - 1));
            MESH_XY_STRIDE_DS3 = new Vector2(meshWidth / (float)(calibrationDescriptor.DataSize_DS3.x - 1), meshHeight / (float)(calibrationDescriptor.DataSize_DS3.y - 1));

            ResizeBoxColliders();

            SandboxReady = true;
            if (OnSandboxReady != null) OnSandboxReady();
        }
        private void InitialiseBuffers()
        {
            int width = calibrationDescriptor.DataSize.x;
            int height = calibrationDescriptor.DataSize.y;
            int totalValues = calibrationDescriptor.TotalDataPoints;

            rawDepthData = new byte[totalValues * 2];
            rawDepthsTex = new Texture2D(width, height, TextureFormat.R16, false);
            rawDepthsTex.filterMode = FilterMode.Bilinear;

            lowPassCounterRT = InitialiseDepthRT(calibrationDescriptor.DataSize);
            internalLowPassDataRT = InitialiseDepthRT(calibrationDescriptor.DataSize);
            lowPassDataRT = InitialiseDepthRT(calibrationDescriptor.DataSize);

            blurredDataTempRT = InitialiseDepthRT(calibrationDescriptor.DataSize);
            blurredDataDSTempRT = InitialiseDepthRT(calibrationDescriptor.DataSize_DS);
            blurredDataDS2TempRT = InitialiseDepthRT(calibrationDescriptor.DataSize_DS2);

            processedDepthsRT = InitialiseDepthRT(calibrationDescriptor.DataSize);
            processedDepthsRT_DS = InitialiseDepthRT(calibrationDescriptor.DataSize_DS);
            processedDepthsRT_DS2 = InitialiseDepthRT(calibrationDescriptor.DataSize_DS2);
            processedDepthsRT_DS3 = InitialiseDepthRT(calibrationDescriptor.DataSize_DS3);

            rawDepthsRT_DS = InitialiseDepthRT(calibrationDescriptor.DataSize_DS);
            rawDepthsRT_DS2 = InitialiseDepthRT(calibrationDescriptor.DataSize_DS2);
            rawDepthsRT_DS3 = InitialiseDepthRT(calibrationDescriptor.DataSize_DS3);

            int num_procedural_verts = calibrationDescriptor.TotalDataPoints;
            proceduralVertices_Buffer = new ComputeBuffer(num_procedural_verts, sizeof(float) * 3, ComputeBufferType.Default);
            proceduralUV_Buffer = new ComputeBuffer(num_procedural_verts, sizeof(float) * 2, ComputeBufferType.Default);

            int num_procedural_DS_verts = calibrationDescriptor.TotalDataPoints_DS;
            proceduralVertices_DS_Buffer = new ComputeBuffer(num_procedural_DS_verts, sizeof(float) * 3, ComputeBufferType.Default);
            proceduralUV_DS_Buffer = new ComputeBuffer(num_procedural_DS_verts, sizeof(float) * 2, ComputeBufferType.Default);

            int num_procedural_DS2_verts = calibrationDescriptor.TotalDataPoints_DS2;
            proceduralVertices_DS2_Buffer = new ComputeBuffer(num_procedural_DS2_verts, sizeof(float) * 3, ComputeBufferType.Default);
            proceduralUV_DS2_Buffer = new ComputeBuffer(num_procedural_DS2_verts, sizeof(float) * 2, ComputeBufferType.Default);

            int num_coll_verts = calibrationDescriptor.TotalDataPoints_DS3;
            int num_coll_tris = 6 * (calibrationDescriptor.DataSize_DS3.x - 1)
                                        * (calibrationDescriptor.DataSize_DS3.y - 1);

            proceduralVertices_DS3_Buffer = new ComputeBuffer(num_coll_verts, sizeof(float) * 3, ComputeBufferType.Default);
            proceduralUV_DS3_Buffer = new ComputeBuffer(num_coll_verts, sizeof(float) * 2, ComputeBufferType.Default);

            collMeshVertices_Buffer = new ComputeBuffer(num_coll_verts, sizeof(float) * 3, ComputeBufferType.Default);
            collMeshUV_Buffer = new ComputeBuffer(num_coll_verts, sizeof(float) * 2, ComputeBufferType.Default);
            collMeshTris_Buffer = new ComputeBuffer(num_coll_tris, sizeof(int), ComputeBufferType.Default);

            collMeshVertices = new Vector3[num_coll_verts];
            collMeshTris = new int[num_coll_tris];
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
        private void ReleaseBuffers()
        {
            if (initialCalibrationComplete)
            {
                internalLowPassDataRT.Release();
                lowPassCounterRT.Release();
                lowPassDataRT.Release();

                blurredDataTempRT.Release();
                blurredDataDSTempRT.Release();
                blurredDataDS2TempRT.Release();

                rawDepthsRT_DS.Release();
                rawDepthsRT_DS2.Release();
                processedDepthsRT.Release();
                processedDepthsRT_DS.Release();
                processedDepthsRT_DS2.Release();

                proceduralVertices_Buffer.Release();
                proceduralUV_Buffer.Release();
                proceduralVertices_DS_Buffer.Release();
                proceduralUV_DS_Buffer.Release();
                proceduralVertices_DS2_Buffer.Release();
                proceduralUV_DS2_Buffer.Release();
                proceduralVertices_DS3_Buffer.Release();
                proceduralUV_DS3_Buffer.Release();

                collMeshVertices_Buffer.Release();
                collMeshUV_Buffer.Release();
                collMeshTris_Buffer.Release();

                colliderMesh.Clear();
            }
        }
        private void CreateBoxColliders()
        {
            LeftCollider = gameObject.AddComponent<BoxCollider>();
            RightCollider = gameObject.AddComponent<BoxCollider>();
            BottomCollider = gameObject.AddComponent<BoxCollider>();
            TopCollider = gameObject.AddComponent<BoxCollider>();
        }
        private void ResizeBoxColliders()
        {
            float colliderHeight = 600;
            float colliderThickness = 5;
            float colliderOffset = colliderThickness / 2.0f;

            LeftCollider.center = sandboxDescriptor.MeshStart + new Vector3(-colliderOffset, sandboxDescriptor.MeshHeight / 2.0f, colliderHeight / 2.0f);
            LeftCollider.size = new Vector3(colliderThickness, sandboxDescriptor.MeshHeight, colliderHeight);

            RightCollider.center = sandboxDescriptor.MeshStart + new Vector3(sandboxDescriptor.MeshWidth + colliderOffset, 
                                                                    sandboxDescriptor.MeshHeight / 2.0f, colliderHeight / 2.0f);
            RightCollider.size = new Vector3(colliderThickness, sandboxDescriptor.MeshHeight, colliderHeight);

            BottomCollider.center = sandboxDescriptor.MeshStart + new Vector3(sandboxDescriptor.MeshWidth / 2.0f, -colliderOffset, colliderHeight / 2.0f);
            BottomCollider.size = new Vector3(sandboxDescriptor.MeshWidth, colliderThickness, colliderHeight);

            TopCollider.center = sandboxDescriptor.MeshStart + new Vector3(sandboxDescriptor.MeshWidth / 2.0f,
                                                                    sandboxDescriptor.MeshHeight + colliderOffset, colliderHeight / 2.0f);
            TopCollider.size = new Vector3(sandboxDescriptor.MeshWidth, colliderThickness, colliderHeight);
        }
        private void UpdateRawTexture()
        {
            // If you're accessing data from the sandboxDescriptor in iterations
            // Make sure you store it separately. Stops unnecessary Gets(), very slow.
            int x = calibrationDescriptor.DataStart.x;
            int y = calibrationDescriptor.DataStart.y;
            int dataStartX = calibrationDescriptor.DataStart.x;
            int dataEndX = calibrationDescriptor.DataEnd.x;
            int totalDataPoints = calibrationDescriptor.TotalDataPoints;

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
        private Texture forcedTexture;
        private bool forcedTextureEnabled;
        public void SetForcedHeightTexture(Texture forcedTexture)
        {
            this.forcedTexture = forcedTexture;
        }
        public void SetForcedHeightEnabled(bool enabled)
        {
            forcedTextureEnabled = enabled;
            setInitialLowPassData = true;
        }
        private void LoadData()
        {
            if (KinectManager.NewDataReady())
            {
                depthDataBuffer = KinectManager.GetCurrentData();
                UpdateRawTexture();
                ProcessSandboxData();

                if (OnNewProcessedData != null) OnNewProcessedData();
            }
        }
        private void ProcessSandboxData()
        {
            if (SandboxResolution == SandboxResolution.RawData)
            {
                // Create downsampled raw data
                SandboxCSHelper.Run_DownsampleRT(SandboxProcessingShader, rawDepthsTex, rawDepthsRT_DS);
                SandboxCSHelper.Run_DownsampleRT(SandboxProcessingShader, rawDepthsRT_DS, rawDepthsRT_DS2);
                SandboxCSHelper.Run_DownsampleRT(SandboxProcessingShader, rawDepthsRT_DS2, rawDepthsRT_DS3);

                SandboxCSHelper.Run_GenerateSandboxMesh(SandboxProcessingShader, rawDepthsTex, proceduralVertices_Buffer, proceduralUV_Buffer,
                                                            meshStart, MESH_XY_STRIDE, MESH_Z_SCALE);
                SandboxCSHelper.Run_GenerateSandboxMesh(SandboxProcessingShader, rawDepthsRT_DS3, collMeshVertices_Buffer, collMeshUV_Buffer, collMeshTris_Buffer,
                                                        meshStart, MESH_XY_STRIDE_DS3, MESH_Z_SCALE);
            }
            else {
                // Create processed and downsampled depth data
                Texture initialData = forcedTextureEnabled ? forcedTexture : rawDepthsTex;
                if (setInitialLowPassData)
                {
                    setInitialLowPassData = false;
                    SandboxCSHelper.Run_SetInitialLowPassData(SandboxProcessingShader, initialData, calibrationDescriptor.DataSize, internalLowPassDataRT, lowPassCounterRT,
                                                              lowPassDataRT, calibrationDescriptor.MinDepth, calibrationDescriptor.MaxDepth);
                }
                SandboxCSHelper.Run_ComputeLowPassRT(SandboxProcessingShader, initialData, calibrationDescriptor.DataSize, internalLowPassDataRT, lowPassCounterRT, lowPassDataRT,
                                                     ALPHA_1, ALPHA_2, calibrationDescriptor.MinDepth, calibrationDescriptor.MaxDepth,
                                                     NoiseTolerance, LowPassHoldTime);

                SandboxCSHelper.Run_BlurRT(SandboxProcessingShader, lowPassDataRT, blurredDataTempRT, processedDepthsRT);

                SandboxCSHelper.Run_DownsampleRT(SandboxProcessingShader, processedDepthsRT, processedDepthsRT_DS);
                SandboxCSHelper.Run_DownsampleRT(SandboxProcessingShader, processedDepthsRT_DS, processedDepthsRT_DS2);
                SandboxCSHelper.Run_DownsampleRT(SandboxProcessingShader, processedDepthsRT_DS2, processedDepthsRT_DS3);

                SandboxCSHelper.Run_BlurRT(SandboxProcessingShader, processedDepthsRT_DS, blurredDataDSTempRT, processedDepthsRT_DS);
                SandboxCSHelper.Run_BlurRT(SandboxProcessingShader, processedDepthsRT_DS2, blurredDataDS2TempRT, processedDepthsRT_DS2);

                switch(SandboxResolution)
                {
                    case SandboxResolution.Original:
                        SandboxCSHelper.Run_GenerateSandboxMesh(SandboxProcessingShader, processedDepthsRT, proceduralVertices_Buffer, proceduralUV_Buffer,
                                                            meshStart, MESH_XY_STRIDE, MESH_Z_SCALE);
                        break;
                    case SandboxResolution.Downsampled_1x:
                        SandboxCSHelper.Run_GenerateSandboxMesh(SandboxProcessingShader, processedDepthsRT_DS, proceduralVertices_DS_Buffer, proceduralUV_DS_Buffer,
                                                            meshStart, MESH_XY_STRIDE_DS1, MESH_Z_SCALE);
                        break;
                    case SandboxResolution.Downsampled_2x:
                        SandboxCSHelper.Run_GenerateSandboxMesh(SandboxProcessingShader, processedDepthsRT_DS2, proceduralVertices_DS2_Buffer, proceduralUV_DS2_Buffer,
                                                            meshStart, MESH_XY_STRIDE_DS2, MESH_Z_SCALE);
                        break;
                    case SandboxResolution.Downsampled_3x:
                        SandboxCSHelper.Run_GenerateSandboxMesh(SandboxProcessingShader, processedDepthsRT_DS3, proceduralVertices_DS3_Buffer, proceduralUV_DS3_Buffer,
                                                            meshStart, MESH_XY_STRIDE_DS3, MESH_Z_SCALE);
                        break;
                    default:
                        SandboxCSHelper.Run_GenerateSandboxMesh(SandboxProcessingShader, processedDepthsRT, proceduralVertices_Buffer, proceduralUV_Buffer,
                                                            meshStart, MESH_XY_STRIDE, MESH_Z_SCALE);
                        break;
                }
                
                SandboxCSHelper.Run_GenerateSandboxMesh(SandboxProcessingShader, processedDepthsRT_DS3, collMeshVertices_Buffer, collMeshUV_Buffer, collMeshTris_Buffer,
                                                        meshStart, MESH_XY_STRIDE_DS3, MESH_Z_SCALE);
            }

            if (colliderDelay == COLL_MESH_DELAY)
            {
                collMeshVertices_Buffer.GetData(collMeshVertices);
                collMeshTris_Buffer.GetData(collMeshTris);

                colliderMesh.vertices = collMeshVertices;
                colliderMesh.triangles = collMeshTris;

                meshCollider.sharedMesh = colliderMesh;
                colliderDelay = 0;
            }
            else
            {
                colliderDelay++;
            }
        }
    }
}
