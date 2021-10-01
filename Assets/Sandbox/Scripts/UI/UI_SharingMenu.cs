//  
//  UI_SharingMenu.cs
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
    public class UI_SharingMenu : MonoBehaviour
    {
        public SharingManager SharingManager;
        public UI_MenuManager UI_MenuManager;

        public GameObject UI_CaptureFrameMenu;
        public GameObject UI_SendFrameMenu;

        public Toggle UI_CSVToggle;
        public Toggle UI_BWImageToggle;
        public Image UI_EmailToggleBtn;
        public Image UI_DesktopToggleBtn;
        public Text UI_ImageSharingModeTitle;
        public RectTransform UI_ImageSharingModeTitleBG;
        public UI_TextInputButton UI_EmailInputButton;
        public Text UI_EmailInputText;
        public Text UI_SendButtonText;
        public RawImage UI_CapturedImage;
        public Button UI_UseFrameBtn;
        public Text UI_CaptureNameText;
        public Button UI_ShareButton;

        private string savedEmail;

        public void OpenSharingMenu()
        {
            UI_CaptureFrameMenu.SetActive(true);
            UI_SendFrameMenu.SetActive(false);

            if (!SharingManager.CapturedFrameReady)
            {
                UI_EmailInputButton.SetValidationFunction(ValidateEmail);
                savedEmail = UI_EmailInputText.text;
                UI_SelectEmailShare();
                UI_ShareButton.interactable = false;
            }
        }
        private bool ValidateEmail(string emailStr)
        {
            bool validationSuccess = SharingManager.ValidateEmail(emailStr);

            if (validationSuccess)
            {
                UI_ShareButton.interactable = true;
            }

            return validationSuccess;
        }
        public void UI_CaptureFrame()
        {
            SharingManager.CaptureData();

            Texture2D screenshotTex = SharingManager.CapturedFrame;
            UI_CapturedImage.texture = screenshotTex;

            Vector2 rawImageSize = UI_CapturedImage.rectTransform.sizeDelta;
            float rawImageAspect = rawImageSize.x / (float)rawImageSize.y;
            float screenshotAspect = screenshotTex.width / (float)screenshotTex.height;

            float aspectDifference = screenshotAspect / rawImageAspect;

            UI_CapturedImage.color = Color.white;
            UI_CapturedImage.uvRect = new Rect(0, (1 - aspectDifference) / 2.0f, 1, aspectDifference);

            UI_UseFrameBtn.interactable = true;
        }
        public void UI_SelectEmailShare()
        {
            UI_EmailToggleBtn.color = new Color(0.5f, 1, 0, 1);
            UI_DesktopToggleBtn.color = new Color(1, 1, 1, 100 / 255.0f);

            UI_ImageSharingModeTitle.text = "Email Address";
            UI_ImageSharingModeTitleBG.sizeDelta =
                        new Vector2(150, UI_ImageSharingModeTitleBG.sizeDelta.y);

            UI_EmailInputButton.GetComponent<Button>().interactable = true;

            UI_ShareButton.onClick.RemoveAllListeners();
            UI_ShareButton.onClick.AddListener(UI_OpenConfirmEmailPanel);
            UI_ShareButton.interactable = savedEmail != UI_EmailInputButton.InitialFauxText;

            UI_EmailInputText.text = savedEmail;
            UI_SendButtonText.text = "Send Email";
        }
        public void UI_SelectDesktopShare()
        {
            UI_EmailToggleBtn.color = new Color(1, 1, 1, 1);
            UI_DesktopToggleBtn.color = new Color(0.5f, 1, 0, 100 / 255.0f);

            UI_ImageSharingModeTitle.text = "Desktop Folder";
            UI_ImageSharingModeTitleBG.sizeDelta = 
                        new Vector2(160, UI_ImageSharingModeTitleBG.sizeDelta.y);

            savedEmail = UI_EmailInputText.text;
            UI_EmailInputButton.GetComponent<Button>().interactable = false;

            UI_ShareButton.onClick.RemoveAllListeners();
            UI_ShareButton.onClick.AddListener(UI_OpenConfirmDesktopSavePanel);
            UI_ShareButton.interactable = true;

            UI_EmailInputText.text = "Look in ARSandboxData";
            UI_SendButtonText.text = "Save";
        }
        public void UI_OpenConfirmEmailPanel()
        {
            SharingManager.SetUpShareCapture(UI_EmailInputText.text, UI_CaptureNameText.text, 
                                                    UI_CSVToggle.isOn, UI_BWImageToggle.isOn);
            UI_MenuManager.OpenConfirmEmailPanel(UI_EmailInputText.text, UI_CaptureNameText.text);
        }
        public void UI_OpenConfirmDesktopSavePanel()
        {
            SharingManager.SetUpShareCapture(UI_EmailInputText.text, UI_CaptureNameText.text,
                                                    UI_CSVToggle.isOn, UI_BWImageToggle.isOn);
            UI_MenuManager.OpenConfirmDesktopSavePanel(UI_CaptureNameText.text);
        }
        public void UI_UseFrame()
        {
            UI_CaptureFrameMenu.SetActive(false);
            UI_SendFrameMenu.SetActive(true);
        }
        public void UI_BackToCaptureFrameMenu()
        {
            UI_CaptureFrameMenu.SetActive(true);
            UI_SendFrameMenu.SetActive(false);
        }
    }
}
