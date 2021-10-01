//  
//  GeologyFileManager.cs
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARSandbox.GeologySimulation
{
    public class GeologyFileManager
    {
        private const string GEOLOGY_FILE_NAME_LIST = "GeologyFilenameList.props";
        private const string SAVE_DIRECTORY_FOLDER = "GeologySimulation";
        private const string GEOLOGY_FILE_SUFFIX = ".geo";

        private static bool geologyFilesLoaded = false;
        private static List<string> savedGeologyFilenames;
        private static List<SerialisedGeologyFile> loadedGeologyFiles;

        public static List<SerialisedGeologyFile> GetLoadedGeologyFiles()
        {
            if (!geologyFilesLoaded)
            {
                LoadGeologyFilenameList();
                LoadGeologyFiles();

                geologyFilesLoaded = true;
            }

            SortLoadedGeologyFiles();
            return loadedGeologyFiles;
        }
        private static string GetGeologyDirectory()
        {
            string geologyDirectory = Path.Combine(Application.persistentDataPath, SAVE_DIRECTORY_FOLDER);
            if (!Directory.Exists(geologyDirectory))
            {
                Directory.CreateDirectory(geologyDirectory);
            }
            return geologyDirectory;
        }
        private static void SaveGeologyFilenameList()
        {
            string filePath = Path.Combine(GetGeologyDirectory(), GEOLOGY_FILE_NAME_LIST);
            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = File.Create(filePath))
            {
                bf.Serialize(fs, savedGeologyFilenames.ToArray());
            }
        }
        private static void LoadGeologyFilenameList()
        {
            string filePath = Path.Combine(GetGeologyDirectory(), GEOLOGY_FILE_NAME_LIST);
            BinaryFormatter bf = new BinaryFormatter();

            if (File.Exists(filePath))
            {
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        string[] topographyList = (string[])bf.Deserialize(fs);

                        savedGeologyFilenames = new List<string>(topographyList);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Error - File Exception: " + e.ToString());
                    Debug.Log("No geology files could be loaded.");
                    savedGeologyFilenames = new List<string>();
                }
            }
            else
            {
                try
                {
                    using (FileStream fs = File.Create(filePath))
                    {
                        string[] geologyFilenames = new string[0];
                        bf.Serialize(fs, geologyFilenames);

                        savedGeologyFilenames = new List<string>();
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Error - File Exception: " + e.ToString());

                    savedGeologyFilenames = new List<string>();
                }
            }
        }
        public static bool SaveGeologyFile(SerialisedGeologyFile geologyFile)
        {
            if (!geologyFilesLoaded)
            {
                GetLoadedGeologyFiles();
            }
            string filenameWithSuffix = geologyFile.Filename + GEOLOGY_FILE_SUFFIX;
            BinaryFormatter bf = new BinaryFormatter();
            string propsPath = Path.Combine(GetGeologyDirectory(), filenameWithSuffix);
            try
            {
                if (File.Exists(propsPath))
                {
                    Debug.Log("Warning File Already Exists: " + filenameWithSuffix);
                    return false;
                }
                using (FileStream fs = File.Create(propsPath))
                {
                    bf.Serialize(fs, geologyFile);
                }
                savedGeologyFilenames.Add(geologyFile.Filename + GEOLOGY_FILE_SUFFIX);
                SaveGeologyFilenameList();
                loadedGeologyFiles.Add(geologyFile);

                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Error - File Exception: " + e.ToString());
            }
            return false;
        }
        private static void LoadGeologyFiles()
        {
            loadedGeologyFiles = new List<SerialisedGeologyFile>();
            for (int i = savedGeologyFilenames.Count - 1; i >= 0; i--)
            {
                string filename = savedGeologyFilenames[i];
                string filePath = Path.Combine(GetGeologyDirectory(), filename);

                if (File.Exists(filePath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    try
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            SerialisedGeologyFile loadedGeologyFile = (SerialisedGeologyFile)bf.Deserialize(fs);
                            loadedGeologyFiles.Add(loadedGeologyFile);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error - File Exception: " + e.ToString());
                        savedGeologyFilenames.RemoveAt(i);
                    }
                }
                else
                {
                    Debug.Log("Cannot find file: " + filename + " Removing from list.");
                    savedGeologyFilenames.RemoveAt(i);
                }
            }
            SaveGeologyFilenameList();
        }
        public static bool RenameGeologyFile(string newName, SerialisedGeologyFile geologyFile)
        {
            string newNameWithSuffix = newName + GEOLOGY_FILE_SUFFIX;
            string newNamePath = Path.Combine(GetGeologyDirectory(), newNameWithSuffix);
            if (File.Exists(newNamePath))
            {
                // Cannot rename to a file that already exists.
                return false;
            }
            string oldNameWithSuffix = geologyFile.Filename + GEOLOGY_FILE_SUFFIX;
            savedGeologyFilenames.Remove(oldNameWithSuffix);
            loadedGeologyFiles.Remove(geologyFile);

            string oldNamePath = Path.Combine(GetGeologyDirectory(), oldNameWithSuffix);
            File.Delete(oldNamePath);

            // Save a new version of the file with the new file name.
            geologyFile.Filename = newName;
            SaveGeologyFile(geologyFile);

            return true;
        }
        public static bool DeleteGeologyFile(SerialisedGeologyFile geologyFile)
        {
            string filenameWithSuffix = geologyFile.Filename + GEOLOGY_FILE_SUFFIX;
            string filePath = Path.Combine(GetGeologyDirectory(), filenameWithSuffix);

            try
            {
                File.Delete(filePath);
                savedGeologyFilenames.Remove(filenameWithSuffix);
                loadedGeologyFiles.Remove(geologyFile);
                SaveGeologyFilenameList();
            }
            catch (Exception e)
            {
                Debug.Log("Error Deleting - File Exception: " + e.ToString());
                return false;
            }

            return true;
        }
        private static void SortLoadedGeologyFiles()
        {
            loadedGeologyFiles.Sort(delegate (SerialisedGeologyFile x, SerialisedGeologyFile y)
            {
                if (x.Filename == null && y.Filename == null) return 0;
                return x.Filename.CompareTo(y.Filename);
            });
        }
    }
}
