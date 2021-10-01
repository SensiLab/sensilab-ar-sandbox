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
using System;
using ARSandbox.GeologySimulation.GeologicalTransforms;

namespace ARSandbox.GeologySimulation
{
    public class UI_ColourSelectionItem : MonoBehaviour
    {
        public Image UI_ColourImage;
        public GameObject UI_NoneText;

        private int colourIndex;
        private bool selected;
        private Action<int> Action_SelectItem;

        public void InitialiseItem(int colourIndex, bool selected)
        {
            this.selected = selected;
            this.colourIndex = colourIndex;

            if (colourIndex == 0)
            {
                UI_ColourImage.color = Color.white;
            } else
            {
                UI_NoneText.SetActive(false);
                UI_ColourImage.color = TransformColours.ColourList[colourIndex];
            }

            SetBackgroundColor();
        }

        public void SetColour(Color colour)
        {
            UI_ColourImage.color = colour;
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;

            SetBackgroundColor();
        }

        private void SetBackgroundColor()
        {
            Color backgroundColor;
            if (selected)
            {
                backgroundColor = new Color(0, 1, 0, 100.0f / 255.0f);
            }
            else
            {
                backgroundColor = new Color(1, 1, 1, 100.0f / 255.0f);
            }
            GetComponent<Image>().color = backgroundColor;
        }

        public void SetSelectFunction(Action<int> Action_SelectItem)
        {
            this.Action_SelectItem = Action_SelectItem;
        }

        public void SelectItem()
        {
            Action_SelectItem(colourIndex);
        }
    }
}
