//  
//  UI_TopographyRename.cs
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

namespace ARSandbox.TopographyBuilder
{
    public class UI_TopographyRename : MonoBehaviour
    {
        public TopographyBuilder TopographyBuilder;
        public UI_MenuManager UI_MenuManager;
        public Text UI_TopographyTitle;

        public string InputTitle = "Rename Topography";
        private string InputText = "No Topography Selected";

        public void SetText(string text)
        {
            InputText = text;
            UI_TopographyTitle.text = text;
        }

        public void OnClick()
        {
            UI_MenuManager.OpenOnScreenKeyboard(InputTitle, InputText, Action_AcceptInput, Action_CancelInput);
        }

        private void Action_AcceptInput(string inputString)
        {
            if (TopographyBuilder.UI_RenameTopography(inputString))
            {
                InputText = inputString;
                UI_TopographyTitle.text = inputString;
            }
        }

        private void Action_CancelInput()
        {
            // Do something on cancel.
        }
    }
}
