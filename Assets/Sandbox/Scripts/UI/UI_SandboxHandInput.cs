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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ARSandbox
{
    public class UI_SandboxHandInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public HandInput HandInput;
        public Sandbox Sandbox;

        private RawImage rawImage;
        private List<PointerEventData> activeEvents;
        private RectTransform imageRectTransform;
        private float rawImageAspectRatio;
        private SandboxDescriptor sandboxDescriptor;
        private const float normalAspectRatio = 16.0f / 9.0f;
        private float adjustedAspectRatio;
        private float widthAspect, heightAspect;
        private float xOffset, yOffset;
        // Use thiPointerEventData eventDatas for initialization
        void Start()
        {
            imageRectTransform = GetComponent<RectTransform>();
            activeEvents = new List<PointerEventData>();
            rawImageAspectRatio = imageRectTransform.rect.width / imageRectTransform.rect.height;
            rawImage = GetComponent<RawImage>();
            SetUpScreen();

            Sandbox.OnSandboxReady += SetUpScreen;
        }

        void OnEnable()
        {
            SetUpScreen();
        }

        void SetUpScreen()
        {
            rawImage = GetComponent<RawImage>();
            imageRectTransform = GetComponent<RectTransform>();
            rawImageAspectRatio = imageRectTransform.rect.width / imageRectTransform.rect.height;

            sandboxDescriptor = Sandbox.GetSandboxDescriptor();
            if (sandboxDescriptor != null)
            {
                float sandboxAspectRatio = (float)sandboxDescriptor.DataSize.x / (float)sandboxDescriptor.DataSize.y;
                float aspectDifference = (rawImageAspectRatio) / (normalAspectRatio);
                // Lock the height of the sandbox image.
                if (rawImageAspectRatio > sandboxAspectRatio)
                {
                    widthAspect = aspectDifference;
                    heightAspect = 1;
                }
                // Lock the width of the sandbox image (need to shrink down from the 16:9 camera)
                else {
                    float sandboxAdjustedAspect = (sandboxAspectRatio / normalAspectRatio);
                    widthAspect = sandboxAdjustedAspect;
                    heightAspect = sandboxAdjustedAspect / aspectDifference;
                }
                xOffset = (1 - widthAspect) / 2.0f;
                yOffset = (1 - heightAspect) / 2.0f;
                rawImage.uvRect = new Rect(xOffset, yOffset, widthAspect, heightAspect);
            }
        }

        // Update is called once per frame
        void Update()
        {
            foreach (PointerEventData eventData in activeEvents)
            {
                Vector2 normalisedPt = GetTouchViewportPosition(eventData);
                HandInput.OnUITouchMove(eventData.pointerId, normalisedPt);
            }
        }

        void OnDisable()
        {
            foreach (PointerEventData eventData in activeEvents)
            {
                HandInput.OnUITouchUp(eventData.pointerId);
            }
            activeEvents.Clear();

            Sandbox.OnSandboxReady -= SetUpScreen;
        }

        public Vector2 GetTouchViewportPosition(PointerEventData eventData)
        {
            Vector2 localPt;
            Vector2 normalisedPt = new Vector2();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRectTransform, eventData.position, null, out localPt))
            {
                Vector2 offsetPt = localPt + imageRectTransform.rect.size / 2;
                normalisedPt = new Vector2(offsetPt.x / imageRectTransform.rect.width * widthAspect + xOffset, offsetPt.y / imageRectTransform.rect.height * heightAspect + yOffset);
                normalisedPt.x = Mathf.Clamp(normalisedPt.x, 0, 0.995f);
                normalisedPt.y = Mathf.Clamp(normalisedPt.y, 0.005f, 0.995f);
            }

            return normalisedPt;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activeEvents.Add(eventData);
            Vector2 normalisedPt = GetTouchViewportPosition(eventData);
            HandInput.OnUITouchDown(eventData.pointerId, normalisedPt);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            activeEvents.Remove(eventData);
            HandInput.OnUITouchUp(eventData.pointerId);
        }
    }
}
