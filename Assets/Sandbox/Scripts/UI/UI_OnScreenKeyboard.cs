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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ARSandbox
{
    public class UI_OnScreenKeyboard : MonoBehaviour
    {
        public enum KeyboardType
        {
            Normal,
            Email
        }
        public UI_KeyboardButton UI_KeyboardButtonPrefab;
        public UI_InputFieldFocused UI_InputField;
        public GameObject UI_SpaceButton;
        public Button UI_ShiftButton;
        public Text UI_InputTitle;
        public KeyboardType KeyboardMode { get; private set; }

        public GameObject UI_NumberRow;
        public GameObject UI_LetterRow1;
        public GameObject UI_LetterRow2;
        public GameObject UI_LetterRow3;
        public GameObject UI_BottomRow;

        private const string NUMBER_ROW = "1234567890";
        private const string LETTER_ROW_1 = "qwertyuiop";
        private const string LETTER_ROW_2 = "asdfghjkl";
        private const string LETTER_ROW_3 = "zxcvbnm";
        private const string EMAIL_ROW = "@_-.";
        private const string MONASH_EDU = "monash\n.edu";
        private const string GMAIL_COM = "gmail\n.com";

        private List<UI_KeyboardButton> numberRowList;
        private List<UI_KeyboardButton> letterRow1List;
        private List<UI_KeyboardButton> letterRow2List;
        private List<UI_KeyboardButton> letterRow3List;
        private List<UI_KeyboardButton> emailRowList;

        private Action<string> Action_AcceptInput;
        private Action Action_CancelInput;

        private bool keyboardCaseUpper;
        private string finalString;
        private bool keyboardInitialised;

        void InitialiseKeyboard()
        {
            numberRowList = new List<UI_KeyboardButton>();
            letterRow1List = new List<UI_KeyboardButton>();
            letterRow2List = new List<UI_KeyboardButton>();
            letterRow3List = new List<UI_KeyboardButton>();
            emailRowList = new List<UI_KeyboardButton>();

            CreateKeyboardRow(UI_NumberRow, numberRowList, NUMBER_ROW, 0);
            CreateKeyboardRow(UI_LetterRow1, letterRow1List, LETTER_ROW_1, 0);
            CreateKeyboardRow(UI_LetterRow2, letterRow2List, LETTER_ROW_2, 0);
            CreateKeyboardRow(UI_LetterRow3, letterRow3List, LETTER_ROW_3, 1);
            CreateEmailRow();

            UI_InputField.onValidateInput += InputValidation;

            keyboardInitialised = true;
        }
        public void SetUpKeyboard(string inputTitleString, string inputFieldString, Action<string> Action_AcceptInput, 
                                    Action Action_CancelInput, KeyboardType keyboardMode = KeyboardType.Normal, int characterLimit = 32)
        {
            if (!keyboardInitialised) InitialiseKeyboard();

            this.Action_AcceptInput = Action_AcceptInput;
            this.Action_CancelInput = Action_CancelInput;
            this.KeyboardMode = keyboardMode;

            UI_InputTitle.text = inputTitleString;
            UI_InputField.text = inputFieldString;
            UI_InputField.characterLimit = characterLimit;

            keyboardCaseUpper = false;

            UI_InputField.caretPosition = inputFieldString.Length;
            UI_InputField.selectionFocusPosition = 0;

            UI_InputField.ActivateInputField();
            UI_InputField.Select();

            UI_InputField.ForceLabelUpdate();

            if (KeyboardMode == KeyboardType.Normal)
            {
                EnableNormalKeyboard();
            } else if (KeyboardMode == KeyboardType.Email)
            {
                EnableEmailKeyboard();
            } else
            {
                EnableNormalKeyboard();
            }
        }
        private void EnableNormalKeyboard()
        {
            // Hide email specific keys.
            foreach(UI_KeyboardButton key in emailRowList)
            {
                key.gameObject.SetActive(false);
            }
            UI_SpaceButton.SetActive(true);
            UI_ShiftButton.interactable = true;
        }
        private void EnableEmailKeyboard()
        {
            // Hide email specific keys.
            foreach (UI_KeyboardButton key in emailRowList)
            {
                key.gameObject.SetActive(true);
            }
            UI_SpaceButton.SetActive(false);
            UI_ShiftButton.interactable = false;
        }
        private void CreateEmailRow()
        {
            for (int i = 0; i < EMAIL_ROW.Length; i++)
            {
                string character = EMAIL_ROW.Substring(i, 1);
                UI_KeyboardButton keyboardButton = Instantiate(UI_KeyboardButtonPrefab);
                keyboardButton.InitialiseItem(character);

                keyboardButton.gameObject.transform.SetParent(UI_BottomRow.transform);
                keyboardButton.gameObject.transform.SetSiblingIndex(i + 1);
                keyboardButton.gameObject.transform.localScale = Vector3.one;
                keyboardButton.gameObject.SetActive(false);
                keyboardButton.SetKeyPressedFunction(Action_Keypressed);

                emailRowList.Add(keyboardButton);
            }

            // Add the extra keys.
            UI_KeyboardButton monashButton = Instantiate(UI_KeyboardButtonPrefab);
            monashButton.InitialiseItem(MONASH_EDU);
            monashButton.SetFontSize(22);
            monashButton.gameObject.transform.SetParent(UI_BottomRow.transform);
            monashButton.gameObject.transform.SetSiblingIndex(2);
            monashButton.gameObject.transform.localScale = Vector3.one;
            monashButton.gameObject.SetActive(false);
            monashButton.SetKeyPressedFunction(Action_MonashEduInput);

            emailRowList.Add(monashButton);

            UI_KeyboardButton gmailButton = Instantiate(UI_KeyboardButtonPrefab);
            gmailButton.InitialiseItem(GMAIL_COM);
            gmailButton.SetFontSize(22);
            gmailButton.gameObject.transform.SetParent(UI_BottomRow.transform);
            gmailButton.gameObject.transform.SetSiblingIndex(3);
            gmailButton.gameObject.transform.localScale = Vector3.one;
            gmailButton.gameObject.SetActive(false);
            gmailButton.SetKeyPressedFunction(Action_GmailComInput);

            emailRowList.Add(gmailButton);
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
        private void CloseKeyboard()
        {
            UI_InputField.DeactivateInputField();
        }
        private bool ValidateFinalString()
        {
            finalString = UI_InputField.text;
            finalString.Trim();

            return finalString.Length > 0;
        }
        private char InputValidation(string input, int charIndex, char addedChar)
        {
            if (KeyboardMode == KeyboardType.Normal) return ValidateNormalKeyboard(input, charIndex, addedChar);
            if (KeyboardMode == KeyboardType.Email) return ValidateEmailKeyboard(input, charIndex, addedChar);
            return ValidateNormalKeyboard(input, charIndex, addedChar);
        }
        private char ValidateEmailKeyboard(string input, int charIndex, char addedChar)
        {
            if (addedChar == '@')
            {
                char[] charList = input.ToCharArray();
                foreach (char c in charList)
                {
                    if (c == '@') return '\0';
                }
            }
            if (Char.IsLetterOrDigit(addedChar) || addedChar == '.' || addedChar == '_' || addedChar == '-' || addedChar == '@')
                return addedChar;

            return '\0';
        }
        private char ValidateNormalKeyboard(string input, int charIndex, char addedChar)
        {
            bool newWord = false;
            if (charIndex == 0 && addedChar == ' ')
            {
                return '\0';
            }
            if (charIndex > 0)
            {
                if (input.Substring(charIndex - 1, 1) == " ")
                {
                    if (addedChar == ' ')
                    {
                        return '\0';
                    }
                    newWord = true;
                }
            }
            if (addedChar == ' ' || Char.IsLetterOrDigit(addedChar))
            {
                if (keyboardCaseUpper || charIndex == 0 || newWord)
                {
                    addedChar = addedChar.ToString().ToUpper().ToCharArray()[0];
                }
                return addedChar;
            }
            return '\0';
        }
        private void ToggleCapsOnList(List<UI_KeyboardButton> keyboardButtons)
        {
            foreach (UI_KeyboardButton keyboardBtn in keyboardButtons)
            {
                keyboardBtn.SetCase(keyboardCaseUpper);
            }
        }
        private void ToggleCaps()
        {
            keyboardCaseUpper = !keyboardCaseUpper;
            ToggleCapsOnList(letterRow1List);
            ToggleCapsOnList(letterRow2List);
            ToggleCapsOnList(letterRow3List);
        }

        private void Action_Keypressed(string character)
        {
            UI_InputField.ProcessEvent(Event.KeyboardEvent(character));
            UI_InputField.ForceLabelUpdate();

            if (keyboardCaseUpper) ToggleCaps();
        }
        private void Action_MonashEduInput(string unused)
        {
            char[] charList = MONASH_EDU.ToCharArray();
            foreach (char c in charList)
            {
                UI_InputField.ProcessEvent(Event.KeyboardEvent(c.ToString()));
            }
            
            UI_InputField.ForceLabelUpdate();
        }
        private void Action_GmailComInput(string unused)
        {
            char[] charList = GMAIL_COM.ToCharArray();
            foreach (char c in charList)
            {
                UI_InputField.ProcessEvent(Event.KeyboardEvent(c.ToString()));
            }

            UI_InputField.ForceLabelUpdate();
        }
        public void UI_Backspace()
        {
            UI_InputField.ProcessEvent(Event.KeyboardEvent("backspace"));
            UI_InputField.ForceLabelUpdate();
        }

        public void UI_Space()
        {
            UI_InputField.ProcessEvent(Event.KeyboardEvent("space"));
            UI_InputField.ForceLabelUpdate();
        }

        public void UI_Shift()
        {
            ToggleCaps();
        }

        public void UI_Accept()
        {
            if (ValidateFinalString())
            {
                Action_AcceptInput(finalString);
                CloseKeyboard();
            }
            else
            {
                UI_Cancel();
            }
        }

        public void UI_Cancel()
        {
            Action_CancelInput();
            CloseKeyboard();
        }
    }
}
