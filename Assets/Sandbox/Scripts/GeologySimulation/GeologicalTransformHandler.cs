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
using ARSandbox.GeologySimulation.GeologicalTransforms;

namespace ARSandbox.GeologySimulation
{
    public class GeologicalTransformHandler
    {
        private List<GeologicalTransform> geologicalTransforms;
        private Vector3 simulationCentre;

        public GeologicalTransformHandler()
        {
            geologicalTransforms = new List<GeologicalTransform>();
            simulationCentre = Vector3.zero;
        }

        public void CreateTransformsFromGeologyFile(SerialisedGeologyFile geologyFile)
        {
            geologicalTransforms.Clear();
            foreach (SerialisedGeologicalTransform loadedTransform in geologyFile.GeologicalTransforms)
            {
                GeologicalTransform newTransform;
                switch (loadedTransform.Type)
                {
                    case GeologicalTransform.TransformType.TiltTransform:
                        newTransform = new TiltTransform(loadedTransform);
                        break;
                    case GeologicalTransform.TransformType.FoldTransform:
                        newTransform = new FoldTransform(loadedTransform);
                        break;
                    case GeologicalTransform.TransformType.FaultTransform:
                        newTransform = new FaultTransform(loadedTransform);
                        break;
                    default:
                        newTransform = new TiltTransform(loadedTransform);
                        break;
                }
                newTransform.ChangeRotationCentre(simulationCentre);
                geologicalTransforms.Add(newTransform);
            }
        }

        public GeologicalTransform AddRandomTransform()
        {
            int transformType = Mathf.FloorToInt(Random.value * 3.0f);

            GeologicalTransform newTransform;
            switch (transformType) {
                case 0:
                    newTransform = AddTiltTransform(360.0f * Random.value, 90.0f * Random.value, 0, true);
                    break;
                case 1:
                    newTransform = AddFoldTransform(360.0f * Random.value, 90.0f * Random.value, 0, 25, 50, 0, 0, 0, true);
                    break;
                case 2:
                    newTransform = AddFaultTransform(360.0f * Random.value, 90.0f * Random.value, 0, 30, 0, GetTotalFaultTransforms(), true);
                    break;
                default:
                    Debug.Log("WARNING: Unknown transform created");
                    newTransform = AddTiltTransform(360.0f * Random.value, 90.0f * Random.value, 0, true);
                    break;
            }

            return newTransform;
        }

        public GeologicalTransform AddFlatTiltTransform()
        {
            TiltTransform tiltTransform = AddTiltTransform(0, 0, 0, true);
            return tiltTransform;
        }

        public TiltTransform AddTiltTransform(float strike, float dip, float plunge, bool anglesInDegrees)
        {
            TiltTransform tiltTransform = new TiltTransform(strike, dip, plunge, simulationCentre, anglesInDegrees);
            geologicalTransforms.Add(tiltTransform);
            return tiltTransform;
        }

        public FoldTransform AddFoldTransform(float strike, float dip, float plunge, float amplitude, float period, float offset, int colourIndex1, int colourIndex2, bool anglesInDegrees)
        {
            FoldTransform foldTransform = new FoldTransform(strike, dip, plunge, amplitude, period, offset, colourIndex1, colourIndex2, simulationCentre, anglesInDegrees);
            geologicalTransforms.Add(foldTransform);
            return foldTransform;
        }

        public FaultTransform AddFaultTransform(float strike, float dip, float plunge, float amplitude, float offset, int colourIndex, bool anglesInDegrees)
        {
            FaultTransform faultTransform = new FaultTransform(strike, dip, plunge, amplitude, offset, colourIndex, simulationCentre, anglesInDegrees);
            geologicalTransforms.Add(faultTransform);
            return faultTransform;
        }

        public void ReplaceTransform (GeologicalTransform newTransform, int index)
        {
            geologicalTransforms[index] = newTransform;
            newTransform.ChangeRotationCentre(simulationCentre);
        }

        public void ChangeSimulationCentre(Vector3 simulationCentre)
        {
            this.simulationCentre = simulationCentre;

            foreach(GeologicalTransform geologicalTransform in geologicalTransforms)
            {
                geologicalTransform.ChangeRotationCentre(simulationCentre);
            }
        }

        public void RemoveTransform(int index)
        {
            geologicalTransforms.RemoveAt(index);
        }

        public GeologicalTransform GetTransform(int index)
        {
            if (index >= 0 && index < geologicalTransforms.Count)
            {
                return geologicalTransforms[index];
            }
            return null;
        }

        public int GetTotalFaultTransforms()
        {
            int totalFaultTransforms = 0;
            foreach(GeologicalTransform transform in geologicalTransforms)
            {
                if (transform.Type == GeologicalTransform.TransformType.FaultTransform) totalFaultTransforms++;
            }
            return totalFaultTransforms;
        }

        public Color[] GetFaultColours()
        {
            List<Color> faultColours = new List<Color>();
            foreach (GeologicalTransform transform in geologicalTransforms)
            {
                if (transform.Type == GeologicalTransform.TransformType.FaultTransform)
                {
                    FaultTransform faultTransform = (FaultTransform)transform;
                    faultColours.Add(TransformColours.ColourList[faultTransform.GetFaultColourIndex()]);
                }
            }
            if (faultColours.Count > 0)
                return faultColours.ToArray();
            return new Color[1] { new Color(0, 0, 0, 0) };
        }
        public Color[] GetFoldColours()
        {
            List<Color> foldColours = new List<Color>();
            foreach (GeologicalTransform transform in geologicalTransforms)
            {
                if (transform.Type == GeologicalTransform.TransformType.FoldTransform)
                {
                    FoldTransform foldTransform = (FoldTransform)transform;
                    int[] colourIndices = foldTransform.GetFoldColourIndices();
                    foldColours.Add(TransformColours.ColourList[colourIndices[0]]);
                    foldColours.Add(TransformColours.ColourList[colourIndices[1]]);
                }
            }
            if (foldColours.Count > 0)
                return foldColours.ToArray();
            return new Color[1] { new Color(0, 0, 0, 0) };
        }
        public int GetTransformIndex(GeologicalTransform transform)
        {
            return geologicalTransforms.IndexOf(transform);
        }

        public List<GeologicalTransform> GetGeologicalTransforms()
        {
            return new List<GeologicalTransform>(geologicalTransforms);
        }

        public GeologicalTransform_CSS[] GetGeologicalTransformBuffer()
        {
            GeologicalTransform_CSS[] transforms = new GeologicalTransform_CSS[geologicalTransforms.Count];

            for (int i = 0; i < geologicalTransforms.Count; i++)
            {
                transforms[i] = geologicalTransforms[geologicalTransforms.Count - 1 - i].GetGeologicalTransform_CSS();
            }

            return transforms;
        }

        public SerialisedGeologicalTransform[] GetSerialisedGeologicalTransforms()
        {
            int totalLayers = geologicalTransforms.Count;
            SerialisedGeologicalTransform[] SerialisedGeologicalTransforms 
                                        = new SerialisedGeologicalTransform[totalLayers];

            for (int i = 0; i < geologicalTransforms.Count; i++)
            {
                SerialisedGeologicalTransforms[i] = geologicalTransforms[i].GetSerialisedGeologicalTransform();
            }

            return SerialisedGeologicalTransforms;
        }
    }
}
