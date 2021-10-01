//  
//  CalibrationFileManager.cs
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ARSandbox
{
    public static class CalibrationFileManager
    {
        public static void Save(StoredCalibration newCalibration)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/SandboxCalibration.cal");
            bf.Serialize(file, newCalibration);
            file.Close();
        }

        public static bool Load(out StoredCalibration loadedCalibration)
        {
            if (File.Exists(Application.persistentDataPath + "/SandboxCalibration.cal"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/SandboxCalibration.cal", FileMode.Open);
                loadedCalibration = (StoredCalibration)bf.Deserialize(file);
                file.Close();
                return true;
            }
            loadedCalibration = null;
            return false;
        }
    }
}
