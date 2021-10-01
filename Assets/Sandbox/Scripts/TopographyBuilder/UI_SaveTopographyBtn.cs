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

namespace ARSandbox.TopographyBuilder
{
    public class UI_SaveTopographyBtn : MonoBehaviour
    {
        public UI_MenuManager UI_MenuManager;
        public TopographyBuilder TopographyBuilder;
        public UI_TopographyBuilderMenu UI_TopographyBuilderMenu;

        public string InputTitle = "Topography Name";
        public string InitialText = "Enter Name";

        public void OnClick()
        {
            UI_MenuManager.OpenOnScreenKeyboard(InputTitle, InitialText, Action_AcceptInput, Action_CancelInput);
        }

        private void Action_AcceptInput(string inputString)
        {
            TopographyBuilder.SaveCurrentTopography(inputString);
        }

        private void Action_CancelInput()
        {
            // Do something on cancel.
        }
    }
}
