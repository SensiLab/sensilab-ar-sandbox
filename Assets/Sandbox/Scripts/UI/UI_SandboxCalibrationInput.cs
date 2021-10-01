//  
//  UI_SandboxCalibrationInput.cs
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
using UnityEngine.EventSystems;

namespace ARSandbox
{
    public class UI_SandboxCalibrationInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        PointerEventData eventData;
        RectTransform imageRectTransform;
        public CalibrationManager calibrationManager;
        private bool ActiveInteraction = false;

        // Use thiPointerEventData eventDatas for initialization
        void Start()
        {
            imageRectTransform = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            if (eventData != null)
            {
                Vector2 localPt;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRectTransform, eventData.position, null, out localPt))
                {
                    Vector2 offsetPt = localPt + imageRectTransform.rect.size / 2;
                    Vector2 normalisedPt = new Vector2(offsetPt.x / imageRectTransform.rect.width, offsetPt.y / imageRectTransform.rect.height);
                    normalisedPt.x = Mathf.Clamp(normalisedPt.x, 0, 0.995f);
                    normalisedPt.y = Mathf.Clamp(normalisedPt.y, 0.005f, 0.995f);

                    if (calibrationManager.CalibrationMode == CalibrationMode.CornerCalibration)
                    {
                        calibrationManager.CalibrationPointInteractions(normalisedPt);
                    }
                    else if (calibrationManager.CalibrationMode == CalibrationMode.DepthCalibration)
                    {
                        calibrationManager.HandleDepthCalibration(normalisedPt, true);
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!ActiveInteraction)
            {
                this.eventData = eventData;
                ActiveInteraction = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            this.eventData = null;
            ActiveInteraction = false;
            if (calibrationManager.CalibrationMode == CalibrationMode.CornerCalibration)
            {
                calibrationManager.DropCalibartionPoint();
            }
        }
    }
}
