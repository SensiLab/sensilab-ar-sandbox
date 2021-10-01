//  
//  UI_CalibrationMenuManager.cs
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
    public class UI_CalibrationMenuManager : MonoBehaviour
    {
        public GameObject CornerCalibrationMenu;
        public GameObject DepthCalibrationMenu;
        public GameObject LensShiftCalibrationMenu;
        public CalibrationManager CalibrationManager;

        private CalibrationMode calibrationMode;
        void OnEnable()
        {
            calibrationMode = CalibrationMode.CornerCalibration;

            CornerCalibrationMenu.SetActive(true);
            DepthCalibrationMenu.SetActive(false);
            LensShiftCalibrationMenu.SetActive(false);
        }
        // Update is called once per frame
        void Update()
        {
            CalibrationMode newMode = CalibrationManager.CalibrationMode;
            if (newMode != calibrationMode)
            {
                CornerCalibrationMenu.SetActive(false);
                DepthCalibrationMenu.SetActive(false);
                LensShiftCalibrationMenu.SetActive(false);
                switch (newMode)
                {
                    case CalibrationMode.CornerCalibration:
                        CornerCalibrationMenu.SetActive(true);
                        break;
                    case CalibrationMode.DepthCalibration:
                        DepthCalibrationMenu.SetActive(true);
                        break;
                    case CalibrationMode.LensShiftCalibration:
                        LensShiftCalibrationMenu.SetActive(true);
                        break;
                }

                calibrationMode = newMode;
            }
        }
    }
}
