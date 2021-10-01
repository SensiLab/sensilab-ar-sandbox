//  
//  UI_KeyboardButton.cs
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
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ARSandbox
{
    public class UI_KeyboardButton : MonoBehaviour
    {

        public Text UI_KeyboardCharacter;

        private string character;
        private Action<string> Action_KeyPressed;

        public void InitialiseItem(string character)
        {
            this.character = character;
            UI_KeyboardCharacter.text = character;
        }
        public void SetFontSize(int fontSize)
        {
            UI_KeyboardCharacter.fontSize = fontSize;
        }
        public void SetCase(bool upperCase)
        {
            if (upperCase)
            {
                character = character.ToUpper();
            }
            else {
                character = character.ToLower();
            }

            UI_KeyboardCharacter.text = character;
        }

        public void SetKeyPressedFunction(Action<string> Action_KeyPressed)
        {
            this.Action_KeyPressed = Action_KeyPressed;
        }

        public void KeyPressed()
        {
            Action_KeyPressed(character);
        }
    }
}
