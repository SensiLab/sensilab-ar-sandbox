//  
//  ModeSelector.cs
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
    public class ModeSelector : MonoBehaviour
    {

        public WaterSimulation.WaterSimulation WaterSimulation;
        public GeologySimulation.GeologySimulation GeologySimulation;
        public FireSimulation.FireSimulation FireSimulation;
        public TopographyBuilder.TopographyBuilder TopographyBuilder;
        public WindSimulation.WindSimulation WindSimulation;

        private GameObject CurrentMode;

        public void EnableWindSimulation()
        {
            WindSimulation.gameObject.SetActive(true);
            CurrentMode = WindSimulation.gameObject;
        }
        public void EnableTopographyBuilder()
        {
            TopographyBuilder.gameObject.SetActive(true);
            CurrentMode = TopographyBuilder.gameObject;
        }
        public void EnableGeologySimulation()
        {
            GeologySimulation.gameObject.SetActive(true);
            CurrentMode = GeologySimulation.gameObject;
        }
        public void EnableWaterSimulation()
        {
            WaterSimulation.gameObject.SetActive(true);
            CurrentMode = WaterSimulation.gameObject;
        }
        public void EnableFireSimulation()
        {
            FireSimulation.gameObject.SetActive(true);
            CurrentMode = FireSimulation.gameObject;
        }
        public void DisableCurrentMode()
        {
            if (CurrentMode != null) CurrentMode.gameObject.SetActive(false);
        }
        public void EnableCurrentMode()
        {
            if (CurrentMode != null) CurrentMode.gameObject.SetActive(true);
        }
    }
}
