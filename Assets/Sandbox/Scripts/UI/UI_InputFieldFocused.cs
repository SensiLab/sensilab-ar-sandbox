//  
//  UI_InputFieldFocused.cs
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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ARSandbox
{
    public class UI_InputFieldFocused : InputField
    {
        public override void OnSelect(BaseEventData eventData)
        {
            //base.OnSelect(eventData);
            //ActivateInputField();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            //DeactivateInputField();
            //base.OnDeselect(eventData);
        }
    }
}