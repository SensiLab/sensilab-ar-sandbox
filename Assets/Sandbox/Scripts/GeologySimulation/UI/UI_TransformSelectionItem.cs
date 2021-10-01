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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ARSandbox.GeologySimulation.GeologicalTransforms;

namespace ARSandbox.GeologySimulation
{
    public class UI_TransformSelectionItem : MonoBehaviour
    {
        public RawImage UI_TransformImage;
        public Text UI_TransformTitle;

        private bool selected;
        private Action<GeologicalTransform.TransformType> Action_SelectItem;

        public GeologicalTransform.TransformType transformType
        {
            get; private set;
        }
        public void InitialiseItem(GeologicalTransform.TransformType transformType, bool selected)
        {
            this.transformType = transformType;
            this.selected = selected;

            UI_TransformImage.texture = GeologicalTransform.GetTransformIcon(transformType);
            UI_TransformTitle.text = GeologicalTransform.GetTransformTitle(transformType);

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

        public void SetSelectFunction(Action<GeologicalTransform.TransformType> Action_SelectItem)
        {
            this.Action_SelectItem = Action_SelectItem;
        }

        public void SelectItem()
        {
            Action_SelectItem(transformType);
        }
    }
}
