//  
//  SecondDisplayHandler.cs
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

namespace ARSandbox
{
    public class SecondDisplayHandler : MonoBehaviour
    {
        public Camera MainCamera;
        public Camera UICamera;
        public Canvas UICanvas;

        public bool MockUsingSecondScreen = true;
        public static bool USING_ONLY_PROJECTOR { get; private set; }
        public static bool USING_SECOND_SCREEN { get; private set; }

        private void Start()
        {
            if (Display.displays.Length > 1)
            {
                Display.displays[1].Activate();
                USING_ONLY_PROJECTOR = false;
                USING_SECOND_SCREEN = true;

            }
            else if (MockUsingSecondScreen)
            {
                USING_ONLY_PROJECTOR = false;
                USING_SECOND_SCREEN = true;

            }
            else
            {
                MainCamera.targetDisplay = 0;

                // Hide the UI if no second screen found.
                UICamera.targetDisplay = 1;
                UICanvas.targetDisplay = 1;

                USING_ONLY_PROJECTOR = true;
                USING_SECOND_SCREEN = false;
            }
        }
    }
}
