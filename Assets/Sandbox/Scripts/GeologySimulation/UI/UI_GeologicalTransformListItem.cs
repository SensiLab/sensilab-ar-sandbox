//  
//  UI_GeologicalTransformListItem.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ARSandbox.GeologySimulation.GeologicalTransforms;

namespace ARSandbox.GeologySimulation
{
    public class UI_GeologicalTransformListItem : MonoBehaviour
    {
        public Text UI_TransformTitle;
        public Text UI_ListIndexTitle;
        public RawImage UI_TransformImage;

        private GeologicalTransform geologicalTransform;
        private Action<UI_GeologicalTransformListItem> Action_DeleteItem;
        private Action<UI_GeologicalTransformListItem> Action_EditItem;

        public void InitialiseItem(GeologicalTransform geologicalTransform, int listIndex)
        {
            UI_ListIndexTitle.text = listIndex.ToString() + ".";
            UI_TransformTitle.text = geologicalTransform.GetTransformName();
            UI_TransformImage.texture = geologicalTransform.GetIconTexture();
            UI_TransformImage.color = Color.white;
        }

        public void SetEditFunction(Action<UI_GeologicalTransformListItem> Action_EditItem)
        {
            this.Action_EditItem = Action_EditItem;
        }

        public void SetDeleteFunction(Action<UI_GeologicalTransformListItem> Action_DeleteItem)
        {
            this.Action_DeleteItem = Action_DeleteItem;
        }

        public void EditItem()
        {
            Action_EditItem(this);
        }

        public void DeleteItem()
        {
            Action_DeleteItem(this);
        }
    }
}
