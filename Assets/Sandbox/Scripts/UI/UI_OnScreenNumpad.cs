//  
//  UI_OnScreenNumpad.cs
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

namespace ARSandbox
{
    public class UI_OnScreenNumpad : MonoBehaviour
    {

        public UI_KeyboardButton UI_KeyboardButtonPrefab;
        public UI_InputFieldFocused UI_InputField;
        public Text UI_InputTitle;

        public GameObject UI_NumberRow1;
        public GameObject UI_NumberRow2;
        public GameObject UI_NumberRow3;
        public GameObject UI_NumberRow4;

        private const string NUMBER_ROW_1 = "123";
        private const string NUMBER_ROW_2 = "456";
        private const string NUMBER_ROW_3 = "789";
        private const string NUMBER_ROW_4 = "0";

        private List<UI_KeyboardButton> numberRow1List;
        private List<UI_KeyboardButton> numberRow2List;
        private List<UI_KeyboardButton> numberRow3List;
        private List<UI_KeyboardButton> numberRow4List;

        private Action<int> Action_AcceptInput;
        private Action Action_CancelInput;

        void Awake()
        {
            numberRow1List = new List<UI_KeyboardButton>();
            numberRow2List = new List<UI_KeyboardButton>();
            numberRow3List = new List<UI_KeyboardButton>();
            numberRow4List = new List<UI_KeyboardButton>();

            CreateKeyboardRow(UI_NumberRow1, numberRow1List, NUMBER_ROW_1, 0);
            CreateKeyboardRow(UI_NumberRow2, numberRow2List, NUMBER_ROW_2, 0);
            CreateKeyboardRow(UI_NumberRow3, numberRow3List, NUMBER_ROW_3, 0);
            CreateKeyboardRow(UI_NumberRow4, numberRow4List, NUMBER_ROW_4, 1);

            UI_InputField.onValidateInput += InputValidation;
        }

        public void SetUpNumpad(string inputTitleString, string inputFieldString, Action<int> Action_AcceptInput, Action Action_CancelInput)
        {
            this.Action_AcceptInput = Action_AcceptInput;
            this.Action_CancelInput = Action_CancelInput;

            UI_InputTitle.text = inputTitleString;
            UI_InputField.text = inputFieldString;

            UI_InputField.caretPosition = inputFieldString.Length;
            UI_InputField.selectionFocusPosition = 0;

            UI_InputField.ActivateInputField();
            UI_InputField.Select();

            UI_InputField.ForceLabelUpdate();
        }
        private void CloseNumpad()
        {
            UI_InputField.DeactivateInputField();
        }
        private char InputValidation(string input, int charIndex, char addedChar)
        {

            if (addedChar == '-' && charIndex != 0) return '\0';

            char[] inputArr = input.ToCharArray();
            if (inputArr.Length > 0)
            {
                if (inputArr[0] == '-' && charIndex == 0)
                    return '\0';

                if (inputArr[0] == '-' && inputArr.Length >= 6)
                    return '\0';

                else if (inputArr[0] != '-' && inputArr.Length >= 5)
                    return '\0';
            }

            if (Char.IsDigit(addedChar) || addedChar == '-')
                return addedChar;

            return '\0';
        }
        private void CreateKeyboardRow(GameObject keyboardRow, List<UI_KeyboardButton> keyboardButtons, string keyboardValues, int siblingOffset)
        {
            for (int i = 0; i < keyboardValues.Length; i++)
            {
                string character = keyboardValues.Substring(i, 1);
                UI_KeyboardButton keyboardButton = Instantiate(UI_KeyboardButtonPrefab);
                keyboardButton.InitialiseItem(character);

                keyboardButton.gameObject.transform.SetParent(keyboardRow.transform);
                keyboardButton.gameObject.transform.SetSiblingIndex(i + siblingOffset);
                keyboardButton.gameObject.transform.localScale = Vector3.one;
                keyboardButton.SetKeyPressedFunction(Action_Keypressed);

                keyboardButtons.Add(keyboardButton);
            }
        }
        
        private void Action_Keypressed(string character)
        {
            // Deals with an edge case where the negative wouldn't be eliminated.
            if (UI_InputField.selectionAnchorPosition != UI_InputField.selectionFocusPosition)
            {
                UI_InputField.ProcessEvent(Event.KeyboardEvent("backspace"));
            }

            UI_InputField.ProcessEvent(Event.KeyboardEvent(character));
            UI_InputField.ForceLabelUpdate();
        }

        public void UI_Backspace()
        {
            UI_InputField.ProcessEvent(Event.KeyboardEvent("backspace"));
            UI_InputField.ForceLabelUpdate();
        }

        public void UI_Negative()
        {
            bool negativeEnabled = UI_InputField.text.Length > 0 && UI_InputField.text.ToCharArray()[0] == '-';

            if (negativeEnabled)
            {
                UI_InputField.text = UI_InputField.text.Substring(1);
            }
            else {
                UI_InputField.text = "-" + UI_InputField.text;
            }
            UI_InputField.ForceLabelUpdate();
        }

        public void UI_Accept()
        {
            Action_AcceptInput(int.Parse(UI_InputField.text));
            CloseNumpad();
        }

        public void UI_Cancel()
        {
            Action_CancelInput();
            CloseNumpad();
        }
    }
}
