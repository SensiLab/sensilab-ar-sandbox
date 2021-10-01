//  
//  UI_GeologyBedListItem.cs
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

namespace ARSandbox.GeologySimulation {
    public class UI_GeologyBedListItem : MonoBehaviour {
        public Text UI_BedTitle;
        public Text UI_ListIndexTitle;
        public RawImage UI_BedImage;

        private int listIndex;
        private GeologicalLayer geologicalLayer;
        private Action<UI_GeologyBedListItem> Action_DeleteItem;
        private Action<UI_GeologyBedListItem> Action_EditItem;

        public void InitialiseItem(GeologicalLayer geologicalLayer, int listIndex)
        {
            UI_ListIndexTitle.text = listIndex.ToString() + ".";
            UI_BedTitle.text = geologicalLayer.LayerDefinition.Name;
            UI_BedImage.texture = GeologicalLayerTextures.GetTexture(geologicalLayer.TextureType);
            UI_BedImage.color = geologicalLayer.LayerDefinition.Colour;
        }

        public void SetEditFunction(Action<UI_GeologyBedListItem> Action_EditItem)
        {
            this.Action_EditItem = Action_EditItem;
        }

        public void SetDeleteFunction(Action<UI_GeologyBedListItem> Action_DeleteItem)
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
