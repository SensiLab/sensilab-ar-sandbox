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
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace ARSandbox
{
    public class UI_Dial : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public GameObject UI_DialImage;
        public Text UI_DialText;

        public bool ConstrainDial = true;
        public bool FlipConstraint = false;
        public float MinValue = 0;
        public float MaxValue = 360;
        public bool ShowNegatives = false;

        public DialChangeEvent OnDialChange;

        private RectTransform rectTransform;
        private bool dialInUse = false;

        // Only support one touch at a time. -1 is a mouse, id >= 0 is a touch.
        private int currentTouchID;
        private Vector2 originalTouchPt;
        private float originalTouchRotation, originalRotation;

        private float internalRotation;
        private float currentRotation;

        // Use this for initialization
        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetDialRotation(float rotation, bool invokeOnChange)
        {
            internalRotation = 360 - rotation;
            ConstrainRotation();

            if (invokeOnChange)
                OnDialChange.Invoke(currentRotation);

            UpdateVisuals();
        }

        private void ConstrainRotation()
        {
            while (internalRotation > 360.01f)
            {
                internalRotation -= 360.0f;
            }
            while (internalRotation < 0)
            {
                internalRotation += 360.0f;
            }

            currentRotation = 360 - internalRotation;
            if (ConstrainDial)
            {
                if (!FlipConstraint)
                {
                    if (currentRotation < MinValue || currentRotation > MaxValue)
                    {
                        CalculateClosestAngle();
                    }
                }
                else {
                    if (currentRotation > MinValue && currentRotation < MaxValue)
                    {
                        CalculateClosestAngle();
                    }
                }
            }
            if (ShowNegatives)
            {
                if (currentRotation > 180) currentRotation -= 360;
            }
            internalRotation = 360 - currentRotation;
        }

        private void CalculateClosestAngle()
        {
            float minDist = Mathf.Min(Mathf.Abs(currentRotation - MinValue), Mathf.Abs(currentRotation - MinValue - 360));
            float maxDist = Mathf.Min(Mathf.Abs(currentRotation - MaxValue), Mathf.Abs(currentRotation - MaxValue - 360));
            if (minDist < maxDist)
            {
                currentRotation = MinValue;
            }
            else
            {
                currentRotation = MaxValue;
            }
        }

        private void UpdateVisuals()
        {
            Vector3 eulerAngles = new Vector3(0, 0, internalRotation);
            Quaternion imageRotation = new Quaternion();
            imageRotation.eulerAngles = eulerAngles;

            UI_DialImage.transform.rotation = imageRotation;

            UI_DialText.text = Mathf.Round(currentRotation).ToString() + "°";
        }

        // Get touch between 0 - 1 on x and y. Centre of dial is at (0.5, 0.5)
        private Vector2 GetTouchNormalisedPosition(PointerEventData pointerData)
        {
            Vector2 localPt;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointerData.position, null, out localPt);
            return localPt;
        }

        public void OnPointerDown(PointerEventData pointerData)
        {
            if (!dialInUse)
            {
                originalRotation = internalRotation;

                originalTouchPt = GetTouchNormalisedPosition(pointerData);
                originalTouchRotation = Mathf.Atan2(-originalTouchPt.y, -originalTouchPt.x) * Mathf.Rad2Deg;

                currentTouchID = pointerData.pointerId;
                dialInUse = true;
            }
        }

        public void OnDrag(PointerEventData pointerData)
        {
            if (currentTouchID == pointerData.pointerId)
            {
                Vector2 newTouchPt = GetTouchNormalisedPosition(pointerData);
                float newRotation = Mathf.Atan2(-newTouchPt.y, -newTouchPt.x) * Mathf.Rad2Deg;
                float rotationDelta = newRotation - originalTouchRotation;

                internalRotation = originalRotation + rotationDelta;
                ConstrainRotation();
                OnDialChange.Invoke(currentRotation);

                UpdateVisuals();
            }
        }

        public void OnPointerUp(PointerEventData pointerData)
        {
            if (currentTouchID == pointerData.pointerId)
            {
                Vector2 newTouchPt = GetTouchNormalisedPosition(pointerData);
                float newRotation = Mathf.Atan2(-newTouchPt.y, -newTouchPt.x) * Mathf.Rad2Deg;
                float rotationDelta = newRotation - originalTouchRotation;

                internalRotation = originalRotation + rotationDelta;
                ConstrainRotation();

                dialInUse = false;
                currentTouchID = -1;
                OnDialChange.Invoke(currentRotation);

                UpdateVisuals();
            }
        }
    }

    [System.Serializable]
    public class DialChangeEvent : UnityEvent<float>
    {

    }
}
