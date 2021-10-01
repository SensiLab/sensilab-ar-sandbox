//  
//  UI_OpenSandboxSettingsBtn.cs
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
    public class UI_OpenSandboxSettingsBtn : MonoBehaviour
    {
        public UI_MenuManager UI_MenuManager;

        private RectTransform RectTransform;
        private Image BackgroundImage;
        private bool MenuOpen = false;

        void Update()
        {
            if (MenuOpen == true && !UI_MenuManager.SandboxSettingsOpen)
            {
                BackgroundImage.color = new Color(1, 1, 1, 100 / 255.0f);
                MenuOpen = false;
            }
        }
        void OnEnable()
        {
            RectTransform = GetComponent<RectTransform>();
            BackgroundImage = GetComponent<Image>();

            BackgroundImage.color = new Color(1, 1, 1, 100 / 255.0f);
        }
        public void OnPress()
        {
            if (UI_MenuManager.SandboxSettingsOpen)
            {
                UI_MenuManager.CloseSandboxSettings();
                BackgroundImage.color = new Color(1, 1, 1, 100 / 255.0f);
                MenuOpen = false;
            }
            else {
                UI_MenuManager.OpenSandboxSettings(RectTransform);
                BackgroundImage.color = new Color(0, 165/255.0f, 0/255.0f, 1);
                MenuOpen = true;
            }
        }
    }
}
