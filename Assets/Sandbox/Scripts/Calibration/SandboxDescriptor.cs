//  
//  SandboxDescriptor.cs
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
    public class SandboxDescriptor
    {
        public Vector3 MeshStart { get; private set; }
        public Vector3 MeshEnd { get; private set; }
        public float MeshXYStride { get; private set; }
        public float MeshWidth { get; private set; }
        public float MeshHeight { get; private set; }

        public MeshCollider MeshCollider { get; private set; }
        public Vector3[] MeshColliderVertices { get; private set; }

        public Texture2D RawDepthsTex { get; private set; }
        public RenderTexture RawDepthsRT_DS { get; private set; }
        public RenderTexture RawDepthsRT_DS2 { get; private set; }
        public RenderTexture ProcessedDepthsRT { get; private set; }
        public RenderTexture ProcessedDepthsRT_DS { get; private set; }
        public RenderTexture ProcessedDepthsRT_DS2 { get; private set; }
        public RenderTexture ProcessedDepthsRT_DS3 { get; private set; }

        public Point DataSize { get; private set; }
        public Point DataSize_DS { get; private set; }
        public Point DataSize_DS2 { get; private set; }
        public Point DataSize_DS3 { get; private set; }

        public float MinDepth { get; private set; }
        public float MaxDepth { get; private set; }
        public float DepthRange { get; private set; }

        public float DepthScale { get; private set; }
        public float MinDepthScaled { get; private set; }
        public float MaxDepthScaled { get; private set; }
        public float DepthRangeScaled { get; private set; }

        public SandboxDescriptor(Vector3 MeshStart, Vector3 MeshEnd, float MeshXYStride,
                                  MeshCollider MeshCollider, Vector3[] MeshColliderVertices,
                                  Texture2D RawDepthsTex, RenderTexture RawDepthsRT_DS, RenderTexture RawDepthsRT_DS2,
                                  RenderTexture ProcessedDepthsRT, RenderTexture ProcessedDepthsRT_DS,
                                  RenderTexture ProcessedDepthsRT_DS2, RenderTexture ProcessedDepthsRT_DS3,
                                  Point DataSize, Point DataSize_DS, Point DataSize_DS2, Point DataSize_DS3,
                                  float MinDepth, float MaxDepth, float DepthScale)
        {
            this.MeshStart = MeshStart;
            this.MeshEnd = MeshEnd;
            MeshWidth = MeshEnd.x - MeshStart.x;
            MeshHeight = MeshEnd.y - MeshStart.y;

            this.MeshXYStride = MeshXYStride;
            this.MeshCollider = MeshCollider;
            this.MeshColliderVertices = MeshColliderVertices;

            this.RawDepthsTex = RawDepthsTex;
            this.RawDepthsRT_DS = RawDepthsRT_DS;
            this.RawDepthsRT_DS2 = RawDepthsRT_DS2;

            this.ProcessedDepthsRT = ProcessedDepthsRT;
            this.ProcessedDepthsRT_DS = ProcessedDepthsRT_DS;
            this.ProcessedDepthsRT_DS2 = ProcessedDepthsRT_DS2;
            this.ProcessedDepthsRT_DS3 = ProcessedDepthsRT_DS3;

            this.DataSize = DataSize;
            this.DataSize_DS = DataSize_DS;
            this.DataSize_DS2 = DataSize_DS2;
            this.DataSize_DS3 = DataSize_DS3;

            this.MinDepth = MinDepth;
            this.MaxDepth = MaxDepth;
            DepthRange = MaxDepth - MinDepth;

            this.DepthScale = DepthScale;
            MinDepthScaled = MinDepth * DepthScale;
            MaxDepthScaled = MaxDepth * DepthScale;
            DepthRangeScaled = DepthRange * DepthScale;
        }
    }
}
