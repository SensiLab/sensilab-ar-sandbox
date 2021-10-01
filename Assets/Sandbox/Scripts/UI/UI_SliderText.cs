//  
//  UI_SliderText.cs
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

namespace ARSandbox
{
    public class UI_SliderText : MonoBehaviour
    {
        public Text UI_Text;
        public string TextPrefix = "Value:";
        public string TextSuffix = "";
        public float ValueScale = 1;
        public int DecimalsToShow = 2;
        public bool UseValueStrings;
        public string[] ValueStrings;
        
        public void UpdateText(float sliderValue)
        {
            if (UseValueStrings)
            {
                int valueIndex = (int)sliderValue;
                if (valueIndex < 0 || valueIndex >= ValueStrings.Length) valueIndex = 0;

                if (ValueStrings.Length != 0)
                {
                    UI_Text.text = TextPrefix + " " + ValueStrings[valueIndex];
                } else
                {
                    UI_Text.text = TextPrefix + " " + (sliderValue * ValueScale).ToString("F" + DecimalsToShow.ToString()) + " " + TextSuffix;
                }
            }
            else {
                UI_Text.text = TextPrefix + " " + (sliderValue * ValueScale).ToString("F" + DecimalsToShow.ToString()) + " " + TextSuffix;
            }
        }
    }
}