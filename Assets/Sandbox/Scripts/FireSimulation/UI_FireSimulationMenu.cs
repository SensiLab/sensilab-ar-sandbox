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

namespace ARSandbox.FireSimulation
{
    public class UI_FireSimulationMenu : MonoBehaviour
    {
        public FireSimulation FireSimulation;
        public UI_Dial UI_WindDirectionDial;
        public Slider UI_WindSpeedSlider;
        public Text UI_PlayPauseBtnText;
        public Slider UI_ZoomSlider;

        public void OpenMenu()
        {
            UI_WindDirectionDial.SetDialRotation(FireSimulation.WindDirection, false);
            UI_WindSpeedSlider.value = FireSimulation.WindSpeed;
            UI_ZoomSlider.value = FireSimulation.LandscapeZoom;

            if (FireSimulation.SimulationPaused)
            {
                UI_PlayPauseBtnText.text = "Resume Simulation";
            }
            else
            {
                UI_PlayPauseBtnText.text = "Pause Simulation";
            }
        }

        public void UI_TogglePauseSimulation()
        {
            bool simulationPaused = FireSimulation.TogglePauseSimulation();
            if (simulationPaused)
            {
                UI_PlayPauseBtnText.text = "Resume Simulation";
            } else
            {
                UI_PlayPauseBtnText.text = "Pause Simulation";
            }
        }
    }
}
