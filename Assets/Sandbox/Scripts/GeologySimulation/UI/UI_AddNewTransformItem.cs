//  
//  UI_AddNewTransformItem.cs
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
using System;

namespace ARSandbox.GeologySimulation
{
    public class UI_AddNewTransformItem : MonoBehaviour
    {
        private Action Action_AddItem;

        public void SetAddFunction(Action Action_AddItem)
        {
            this.Action_AddItem = Action_AddItem;
        }

        public void AddItem()
        {
            Action_AddItem();
        }
    }
}
