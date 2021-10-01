//  
//  UI_LensShiftParameters.cs
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
using UnityEngine.UI;

namespace ARSandbox
{
    public class UI_LensShiftParameters : MonoBehaviour
    {
        public CalibrationManager CalibrationManager;
        public Text UI_LensShiftText;
        public Text UI_HorizontalTranslationText;
        public Text UI_VerticalTranslationText;
        public Text UI_HorizontalScaleText;
        public Text UI_VerticalScaleText;

        private bool AllowOnEnable = false;
        private float scaleScalingFactor = 10000.0f;

        private void Start()
        {
            AllowOnEnable = true;
            UpdateText();
        }

        private void OnEnable()
        {
            if (AllowOnEnable)
            {
                UpdateText();
            }
        }

        public void UpdateText()
        {
            CameraCalibrationParameters parameters = CalibrationManager.GetCameraCalibrationParameters();

            UI_LensShiftText.text = "Value: " + (parameters.LensShift / CalibrationManager.LENS_SHIFT_MAX * 100.0f).ToString("f1") + "%";
            UI_HorizontalTranslationText.text = "Value: " + parameters.ExtraCameraTranslation.x.ToString("f1");
            UI_VerticalTranslationText.text = "Value: " + parameters.ExtraCameraTranslation.y.ToString("f1");
            UI_HorizontalScaleText.text = "Value: " + (parameters.ExtraCameraScaling.x * scaleScalingFactor).ToString("f1");
            UI_VerticalScaleText.text = "Value: " + (parameters.ExtraCameraScaling.y * scaleScalingFactor).ToString("f1");
        }
    }
}
