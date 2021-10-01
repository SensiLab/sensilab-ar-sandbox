//  
//  CalibrationDescriptor.cs
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
    public class CalibrationDescriptor
    {
        public Point DataStart { get; private set; }
        public Point DataEnd { get; private set; }
        public Point DataSize { get; private set; }
        public Point DataSize_DS { get; private set; }
        public Point DataSize_DS2 { get; private set; }
        public Point DataSize_DS3 { get; private set; }

        public int TotalDataPoints { get; private set; }
        public int TotalDataPoints_DS { get; private set; }
        public int TotalDataPoints_DS2 { get; private set; }
        public int TotalDataPoints_DS3 { get; private set; }

        public float MinDepth { get; private set; }
        public float MaxDepth { get; private set; }
        public float DepthRange { get; private set; }

        public CalibrationDescriptor(Point DataStart, Point DataEnd, float MinDepth, float MaxDepth)
        {
            this.DataStart = DataStart;
            this.DataEnd = DataEnd;

            DataSize = new Point(DataEnd.x - DataStart.x, DataEnd.y - DataStart.y);
            TotalDataPoints = DataSize.x * DataSize.y;

            DataSize_DS = new Point((int)Mathf.Ceil(DataSize.x / 2.0f) + 1,
                                        (int)Mathf.Ceil(DataSize.y / 2.0f) + 1);
            TotalDataPoints_DS = DataSize_DS.x * DataSize_DS.y;

            DataSize_DS2 = new Point((int)Mathf.Ceil(DataSize.x / 4.0f) + 1,
                                        (int)Mathf.Ceil(DataSize.y / 4.0f) + 1);
            TotalDataPoints_DS2 = DataSize_DS2.x * DataSize_DS2.y;

            DataSize_DS3 = new Point((int)Mathf.Ceil(DataSize.x / 8.0f) + 1,
                                        (int)Mathf.Ceil(DataSize.y / 8.0f) + 1);
            TotalDataPoints_DS3 = DataSize_DS3.x * DataSize_DS3.y;

            this.MinDepth = MinDepth;
            this.MaxDepth = MaxDepth;
            this.DepthRange = MaxDepth - MinDepth;
        }
    }
}
