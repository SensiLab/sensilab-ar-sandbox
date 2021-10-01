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
using UnityEngine;
using UnityEngine.UI;

namespace ARSandbox
{
    public class UI_TextInputButton : MonoBehaviour
    {
        public Text UI_Text;
        public UI_MenuManager UI_MenuManager;
        public string InputTitle;
        public string InitialFauxText = "";
        public string InitialText;
        public bool EmailInput;
        public int CharacterLimit = 32;
        private Func<string, bool> validationFunction;

        public string Text { get; private set; }

        public void Awake()
        {
            Text = InitialText;

            if (InitialFauxText != "")
            {
                UI_Text.text = InitialFauxText;
            } else
            {
                UI_Text.text = InitialText;
            }
        }

        public void SetText(string text)
        {
            Text = text;
            UI_Text.text = Text;
        }
        public void OnClick()
        {
            UI_MenuManager.OpenOnScreenKeyboard(InputTitle, Text, Action_AcceptInput, Action_CancelInput,
                                                EmailInput ? UI_OnScreenKeyboard.KeyboardType.Email : UI_OnScreenKeyboard.KeyboardType.Normal,
                                                CharacterLimit);
        }
        public void SetValidationFunction(Func<string, bool> validationFunction)
        {
            this.validationFunction = validationFunction;
        }
        private void Action_AcceptInput(string inputString)
        {
            if (validationFunction == null || validationFunction(inputString))
            {
                Text = inputString;
                UI_Text.text = Text;
            }
        }

        private void Action_CancelInput()
        {
            // Do something on cancel.
        }
    }
}
