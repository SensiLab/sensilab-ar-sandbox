//  
//  TopographyPropertiesSerialised.cs
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

namespace ARSandbox.TopographyBuilder
{
    [System.Serializable]
    public class TopographyPropertiesSerialised
    {
        public int Width, Height;
        public float MinDepth, MaxDepth;
        public int DataStartX, DataStartY;
        public int DataEndX, DataEndY;
        public string TopographyDisplayName;
        public string TopographyDataPath;
        public string TopographyPropertiesPath;
        public bool UsedWater;

        public TopographyPropertiesSerialised (int Width, int Height, float MinDepth, float MaxDepth,
                                               int DataStartX, int DataStartY, int DataEndX, int DataEndY,
                                                string TopographyName, bool UsedWater)
        {
            this.Width = Width;
            this.Height = Height;
            this.MinDepth = MinDepth;
            this.MaxDepth = MaxDepth;
            this.DataStartX = DataStartX;
            this.DataStartY = DataStartY;
            this.DataEndX = DataEndX;
            this.DataEndY = DataEndY;
            this.UsedWater = UsedWater;
            TopographyDisplayName = TopographyName;
            TopographyDataPath = TopographyName + ".data";
            TopographyPropertiesPath = TopographyName + ".props";
        }

        public TopographyPropertiesSerialised(SandboxDescriptor sandboxDescriptor, CalibrationDescriptor calibrationDescriptor, 
                                              bool UsedWater, string TopographyName)
        {
            Width = sandboxDescriptor.DataSize.x;
            Height = sandboxDescriptor.DataSize.y;
            MinDepth = sandboxDescriptor.MinDepth;
            MaxDepth = sandboxDescriptor.MaxDepth;
            DataStartX = calibrationDescriptor.DataStart.x;
            DataStartY = calibrationDescriptor.DataStart.y;
            DataEndX = calibrationDescriptor.DataEnd.x;
            DataEndY = calibrationDescriptor.DataEnd.y;
            this.UsedWater = UsedWater;
            TopographyDisplayName = TopographyName;
            TopographyDataPath = TopographyName + ".data";
            TopographyPropertiesPath = TopographyName + ".props";
        }

        public void Rename(string newName)
        {
            TopographyDisplayName = newName;
            TopographyDataPath = newName + ".data";
            TopographyPropertiesPath = newName + ".props";
        }
    }
}
