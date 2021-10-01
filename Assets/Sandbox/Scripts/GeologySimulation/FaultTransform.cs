//  
//  FaultTransform.cs
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

namespace ARSandbox.GeologySimulation.GeologicalTransforms
{
    public class FaultTransform : GeologicalTransform
    {
        public override TransformType Type
        {
            get { return TransformType.FaultTransform; }
        }
        public new const string TRANSFORM_TITLE = "Fault Transform";
        public static Texture2D IconTexture = Resources.Load("FaultTransformIcon") as Texture2D;

        private float strike;
        private float dip;
        private float plunge;
        private float amplitude;
        private float offset;
        private Vector3 rotationCentre;
        private int colourIndex;

        private Matrix4x4 translationMatrix;
        private Matrix4x4 rotationMatrix;
        private Matrix4x4 inverseTranslationMatrix;

        private Matrix4x4 tiltTransform;
        private Matrix4x4 inverseTiltTransform;

        public FaultTransform(float strike, float dip, float plunge, float amplitude, float offset, int colourIndex, Vector3 rotationCentre, bool anglesInDegrees = false)
        {
            if (anglesInDegrees)
            {
                this.strike = Mathf.Deg2Rad * strike;
                this.dip = Mathf.Deg2Rad * dip;
                this.plunge = Mathf.Deg2Rad * plunge;
            }
            else
            {
                this.strike = strike;
                this.dip = dip;
                this.plunge = plunge;
            }
            this.amplitude = amplitude;
            this.offset = offset;
            this.rotationCentre = rotationCentre;
            this.colourIndex = colourIndex;

            CalculateTranslationMatrix();
            CalculateRotationMatrix();
            CalculateTiltMatrix();
        }
        public FaultTransform(SerialisedGeologicalTransform loadedTransform)
        {
            if (loadedTransform.Type != Type)
            {
                Debug.Log(string.Format("Warning: Trying to initialise fault transform with: {0}", loadedTransform.Type));
            }

            strike = loadedTransform.Strike;
            dip = loadedTransform.Dip;
            plunge = loadedTransform.Plunge;
            amplitude = loadedTransform.Amplitude;
            offset = loadedTransform.Offset;
            colourIndex = loadedTransform.ColourIndex1;

            rotationCentre = Vector3.zero;

            CalculateTranslationMatrix();
            CalculateRotationMatrix();
            CalculateTiltMatrix();
        }
        public override Texture2D GetIconTexture()
        {
            return IconTexture;
        }
        public override string GetTransformName()
        {
            return TRANSFORM_TITLE;
        }
        public override void ChangeRotationCentre(Vector3 rotationCentre)
        {
            this.rotationCentre = rotationCentre;

            CalculateTranslationMatrix();
            CalculateTiltMatrix();
        }

        public void ChangeRotationParameters(float strike, float dip, float plunge, bool anglesInDegrees = false)
        {
            if (anglesInDegrees)
            {
                this.strike = Mathf.Deg2Rad * strike;
                this.dip = Mathf.Deg2Rad * dip;
                this.plunge = Mathf.Deg2Rad * plunge;
            }
            else
            {
                this.strike = strike;
                this.dip = dip;
                this.plunge = plunge;
            }

            CalculateRotationMatrix();
            CalculateTiltMatrix();
        }

        public void ChangeFaultParameters(float amplitude, float offset, int colourIndex)
        {
            this.amplitude = amplitude;
            this.offset = offset;
            this.colourIndex = colourIndex;
        }

        private void CalculateRotationMatrix()
        {
            float invertedStrike = -strike;
            Matrix4x4 rotationZ = new Matrix4x4(new Vector4(Mathf.Cos(invertedStrike), Mathf.Sin(invertedStrike), 0, 0),
                                                new Vector4(-Mathf.Sin(invertedStrike), Mathf.Cos(invertedStrike), 0, 0),
                                                new Vector4(0, 0, 1, 0),
                                                new Vector4(0, 0, 0, 1));
            Matrix4x4 rotationY = new Matrix4x4(new Vector4(Mathf.Cos(Mathf.PI / 2.0f - dip), 0, -Mathf.Sin(Mathf.PI / 2.0f - dip), 0),
                                                new Vector4(0, 1, 0, 0),
                                                new Vector4(Mathf.Sin(Mathf.PI / 2.0f - dip), 0,  Mathf.Cos(Mathf.PI / 2.0f - dip), 0),
                                                new Vector4(0, 0, 0, 1));
            Matrix4x4 rotationX = new Matrix4x4(new Vector4(1, 0, 0, 0),
                                                new Vector4(0, Mathf.Cos(plunge), Mathf.Sin(plunge), 0),
                                                new Vector4(0, -Mathf.Sin(plunge), Mathf.Cos(plunge), 0),
                                                new Vector4(0, 0, 0, 1));
            rotationMatrix = rotationZ * rotationY * rotationX;
        }

        private void CalculateTranslationMatrix()
        {
            translationMatrix = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0),
                                                new Vector4(-rotationCentre.x, -rotationCentre.y, -rotationCentre.z, 1));
            inverseTranslationMatrix = translationMatrix.inverse;
        }

        private void CalculateTiltMatrix()
        {
            tiltTransform = inverseTranslationMatrix * rotationMatrix * translationMatrix;
            inverseTiltTransform = tiltTransform.inverse;
        }
        public override GeologicalTransformParameters GetGeologicalTransformParameters(bool anglesInDegrees)
        {
            GeologicalTransformParameters parameters;
            parameters.Strike = anglesInDegrees ? strike * Mathf.Rad2Deg : strike;
            parameters.Dip = anglesInDegrees ? dip * Mathf.Rad2Deg : dip;
            parameters.Plunge = anglesInDegrees ? plunge * Mathf.Rad2Deg : plunge;
            parameters.Amplitude = amplitude;
            parameters.Offset = offset;
            parameters.Period = 0;
            parameters.ColourIndex1 = colourIndex;
            parameters.ColourIndex2 = 0;

            return parameters;
        }
        public int GetFaultColourIndex()
        {
            return colourIndex;
        }
        public override GeologicalTransform_CSS GetGeologicalTransform_CSS()
        {
            GeologicalTransform_CSS transform;
            transform.Type = (int)Type;
            transform.Transform = tiltTransform;
            transform.InverseTransform = inverseTiltTransform;
            transform.Amplitude = amplitude;
            transform.Period = 0;
            transform.Offset = rotationCentre.x + offset;

            return transform;
        }
        public override SerialisedGeologicalTransform GetSerialisedGeologicalTransform()
        {
            SerialisedGeologicalTransform serialisedTransform = new SerialisedGeologicalTransform();

            serialisedTransform.Type = TransformType.FaultTransform;
            serialisedTransform.Dip = dip;
            serialisedTransform.Strike = strike;
            serialisedTransform.Plunge = plunge;
            serialisedTransform.Amplitude = amplitude;
            serialisedTransform.Period = 0;
            serialisedTransform.Offset = offset;
            serialisedTransform.ColourIndex1 = colourIndex;
            serialisedTransform.ColourIndex2 = 0;

            return serialisedTransform;
        }
        public override GeologicalTransform Clone()
        {
            return new FaultTransform(strike, dip, plunge, amplitude, offset, colourIndex, rotationCentre, false);
        }
    }
}