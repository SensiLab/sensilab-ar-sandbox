//  
//  UI_WindSimulationMenu.cs
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

namespace ARSandbox.WindSimulation
{
    public class UI_WindSimulationMenu : MonoBehaviour
    {
        public Image UI_SouthernHemisphereBG;
        public Image UI_NorthernHemisphereBG;

        public void UI_SetSouthernHemisphere()
        {
            UI_SouthernHemisphereBG.color = new Color(0.5f, 1, 0);
            UI_NorthernHemisphereBG.color = new Color(1, 1, 1);
        }

        public void UI_SetNorthernHemisphere()
        {
            UI_SouthernHemisphereBG.color = new Color(1, 1, 1);
            UI_NorthernHemisphereBG.color = new Color(0.5f, 1, 0); 
        }
    }
}
