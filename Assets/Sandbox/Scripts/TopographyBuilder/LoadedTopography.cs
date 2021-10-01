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
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ARSandbox.TopographyBuilder
{
    public class LoadedTopography
    {
        public string PropsPath { get { return topographyProperties.TopographyPropertiesPath; } }
        public string DisplayName { get { return topographyProperties.TopographyDisplayName; } }
        public bool DataLoaded { get; private set; }
        public bool ValidFileLoaded { get; private set; }
        public Point DataSize { get { return new Point(topographyProperties.Width, topographyProperties.Height); } }
        public Point DataStart { get { return new Point(topographyProperties.DataStartX, topographyProperties.DataStartY); } }
        public Point DataEnd { get { return new Point(topographyProperties.DataEndX, topographyProperties.DataEndY); } }
        public float MinDepth { get { return topographyProperties.MinDepth; } }
        public float MaxDepth { get { return topographyProperties.MaxDepth; } }
        public ushort[] DepthDataBuffer { get; private set; }

        private string TopographyPropertiesPath;
        private TopographyPropertiesSerialised topographyProperties;
        
        public LoadedTopography(string TopographyPropertiesPath)
        {
            this.TopographyPropertiesPath = TopographyPropertiesPath;
            ValidFileLoaded = LoadTopographyProperties();
            DataLoaded = false;
        }

        public void Delete(string topographyBuilderDirectory)
        {
            string propsPath = Path.Combine(topographyBuilderDirectory, topographyProperties.TopographyPropertiesPath);
            string dataPath = Path.Combine(topographyBuilderDirectory, topographyProperties.TopographyDataPath);

            File.Delete(propsPath);
            File.Delete(dataPath);
        }

        public bool Rename(string newName, string topographyBuilderDirectory)
        {
            string newNamePropsName = newName + ".props";
            string newNamePropsPath = Path.Combine(topographyBuilderDirectory, newNamePropsName);
            if (File.Exists(newNamePropsPath))
            {
                // Cannot rename to a file that already exists.
                return false;
            }

            // Should be fine to delete old props file, replace with new props.
            string oldNamePropsPath = Path.Combine(topographyBuilderDirectory, topographyProperties.TopographyPropertiesPath);
            string oldNameDataPath = Path.Combine(topographyBuilderDirectory, topographyProperties.TopographyDataPath);
            File.Delete(oldNamePropsPath);

            topographyProperties.Rename(newName);
            SaveTopographyProperties(topographyBuilderDirectory);

            string newNameDataPath = Path.Combine(topographyBuilderDirectory, topographyProperties.TopographyDataPath);
            File.Move(oldNameDataPath, newNameDataPath);

            return true;
        }

        private void SaveTopographyProperties(string topographyBuilderDirectory)
        {
            BinaryFormatter bf = new BinaryFormatter();
            string propsPath = Path.Combine(topographyBuilderDirectory, topographyProperties.TopographyPropertiesPath);
            try
            {
                if (File.Exists(propsPath))
                {
                    Debug.Log("Warning File Already Exists: " + propsPath);
                }
                using (FileStream fs = File.Create(propsPath))
                {
                    bf.Serialize(fs, topographyProperties);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error - File Exception: " + e.ToString());
            }
        }

        private bool LoadTopographyProperties()
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream fs = new FileStream(TopographyPropertiesPath, FileMode.Open, FileAccess.Read))
                {
                    TopographyPropertiesSerialised loadedTopography = (TopographyPropertiesSerialised)bf.Deserialize(fs);
                    topographyProperties = loadedTopography;
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error - File Exception: " + e.ToString());
                return false;
            }
        }

        public void LoadRawData(string TopographyDirectory)
        {
            BinaryFormatter bf = new BinaryFormatter();
            string filePath = Path.Combine(TopographyDirectory, topographyProperties.TopographyDataPath);
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    DepthDataBuffer = (ushort[])bf.Deserialize(fs);
                    DataLoaded = true;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error - File Exception: " + e.ToString());
                Debug.Log("Topography raw data couldn't be loaded");
            }
        }
    }
}
