//  
//  UI_TopographySelectionItem.cs
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
using System;

namespace ARSandbox.TopographyBuilder
{
    public class UI_TopographySelectionItem : MonoBehaviour
    {
        public Text UI_TopographyText;
        public LoadedTopography LoadedTopography { get; private set; }

        private bool selected;
        private Action<UI_TopographySelectionItem> Action_SelectItem;

        public void InitialiseItem(LoadedTopography LoadedTopography, bool selected)
        {
            this.selected = selected;
            this.LoadedTopography = LoadedTopography;

            UI_TopographyText.text = LoadedTopography.DisplayName;

            SetBackgroundColor();
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;

            SetBackgroundColor();
        }

        private void SetBackgroundColor()
        {
            Color backgroundColor;
            if (this.selected)
            {
                backgroundColor = new Color(1, 1, 0, 200.0f / 255.0f);
            }
            else
            {
                backgroundColor = new Color(1, 1, 1, 100.0f / 255.0f);
            }
            GetComponent<Image>().color = backgroundColor;
        }

        public void SetSelectFunction(Action<UI_TopographySelectionItem> Action_SelectItem)
        {
            this.Action_SelectItem = Action_SelectItem;
        }

        public void SelectItem()
        {
            Action_SelectItem(this);
        }
    }
}
