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

namespace ARSandbox
{
    public class UI_NumpadInput : MonoBehaviour
    {
        public UI_MenuManager UI_MenuManager;
        public Text UI_Text;
        public Button UI_Button;
        public string InputTitle = "Rename Topography";
        public string suffix = "metres";

        private int InputNumber = 1000;
        private Func<int, bool> Action_ValidateOutput;

        public void SetInteractable(bool interactable)
        {
            UI_Button.interactable = interactable;
        }

        public void SetNumber(int number)
        {
            InputNumber = number;
            UI_Text.text = number.ToString() + " " + suffix;
        }

        public void SetAcceptAction(Func<int, bool> Action_ValidateOutput)
        {
            this.Action_ValidateOutput = Action_ValidateOutput;
        }

        public void OnClick()
        {
            UI_MenuManager.OpenOnScreenNumpad(InputTitle, InputNumber, Action_AcceptInput, Action_CancelInput);
        }

        private void Action_AcceptInput(int outputNumber)
        {
            if (Action_ValidateOutput(outputNumber))
            {
                InputNumber = outputNumber;
                UI_Text.text = outputNumber.ToString() + " " + suffix;
            }
        }

        private void Action_CancelInput()
        {
            // Do something on cancel.
        }
    }
}
