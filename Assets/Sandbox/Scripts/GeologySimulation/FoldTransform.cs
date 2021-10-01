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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARSandbox.GeologySimulation.GeologicalTransforms
{
    public class FoldTransform : GeologicalTransform
    {
        public override TransformType Type
        {
            get { return TransformType.FoldTransform; }
        }
        public new const string TRANSFORM_TITLE = "Fold Transform";
        public static Texture2D IconTexture = Resources.Load("FoldTransformIcon") as Texture2D;

        private float strike;
        private float dip;
        private float plunge;
        private float amplitude;
        private float period;
        private float offset;
        private Vector3 rotationCentre;
        private int colourIndex1;
        private int colourIndex2;

        private Matrix4x4 translationMatrix;
        private Matrix4x4 foldRotationMatrix;
        private Matrix4x4 strikeRotationMatrix, plungeDipRotationMatrix, invStrikeRotationMatrix;
        private Matrix4x4 inverseTranslationMatrix;

        private Matrix4x4 tiltTransform;
        private Matrix4x4 inverseTiltTransform;

        public FoldTransform(float strike, float dip, float plunge, float amplitude, float period, float offset, int colourIndex1, int colourIndex2, Vector3 rotationCentre, bool anglesInDegrees = false)
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
            this.period = period;
            this.offset = offset;
            this.rotationCentre = rotationCentre;
            this.colourIndex1 = colourIndex1;
            this.colourIndex2 = colourIndex2;

            CalculateTranslationMatrix();
            CalculateFoldRotationMatrix();
            CalculateTiltRotationMatrices();
            CalculateFinalMatrices();
        }
        public FoldTransform(SerialisedGeologicalTransform loadedTransform)
        {
            if (loadedTransform.Type != Type)
            {
                Debug.Log(string.Format("Warning: Trying to initialise fold transform with: {0}", loadedTransform.Type));
            }

            strike = loadedTransform.Strike;
            dip = loadedTransform.Dip;
            plunge = loadedTransform.Plunge;
            amplitude = loadedTransform.Amplitude;
            period = loadedTransform.Period;
            offset = loadedTransform.Offset;
            colourIndex1 = loadedTransform.ColourIndex1;
            colourIndex2 = loadedTransform.ColourIndex2;

            rotationCentre = Vector3.zero;

            CalculateTranslationMatrix();
            CalculateFoldRotationMatrix();
            CalculateTiltRotationMatrices();
            CalculateFinalMatrices();
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
            CalculateFinalMatrices();
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

            CalculateFoldRotationMatrix();
            CalculateTiltRotationMatrices();
            CalculateFinalMatrices();
        }

        public void ChangeFoldParameters(float amplitude, float period, float offset, int colourIndex1, int colourIndex2)
        {
            this.amplitude = amplitude;
            this.period = period;
            this.offset = offset;
            this.colourIndex1 = colourIndex1;
            this.colourIndex2 = colourIndex2;
        }

        private void CalculateFoldRotationMatrix()
        {
            float invertedStrike = strike;
            Matrix4x4 rotationZ = new Matrix4x4(new Vector4(Mathf.Cos(invertedStrike), Mathf.Sin(invertedStrike), 0, 0),
                                                new Vector4(-Mathf.Sin(invertedStrike), Mathf.Cos(invertedStrike), 0, 0),
                                                new Vector4(0, 0, 1, 0),
                                                new Vector4(0, 0, 0, 1));
            foldRotationMatrix = rotationZ;
        }

        private void CalculateTiltRotationMatrices()
        {
            float invertedStrike = -strike;
            Matrix4x4 rotationZ = new Matrix4x4(new Vector4(Mathf.Cos(invertedStrike), Mathf.Sin(invertedStrike), 0, 0),
                                                new Vector4(-Mathf.Sin(invertedStrike), Mathf.Cos(invertedStrike), 0, 0),
                                                new Vector4(0, 0, 1, 0),
                                                new Vector4(0, 0, 0, 1));
            Matrix4x4 rotationY = new Matrix4x4(new Vector4(Mathf.Cos(Mathf.PI / 2.0f - dip), 0, -Mathf.Sin(Mathf.PI / 2.0f - dip), 0),
                                                new Vector4(0, 1, 0, 0),
                                                new Vector4(Mathf.Sin(Mathf.PI / 2.0f - dip), 0, Mathf.Cos(Mathf.PI / 2.0f - dip), 0),
                                                new Vector4(0, 0, 0, 1));
            Matrix4x4 rotationX = new Matrix4x4(new Vector4(1, 0, 0, 0),
                                                new Vector4(0, Mathf.Cos(plunge), Mathf.Sin(plunge), 0),
                                                new Vector4(0, -Mathf.Sin(plunge), Mathf.Cos(plunge), 0),
                                                new Vector4(0, 0, 0, 1));
            strikeRotationMatrix = rotationZ;
            plungeDipRotationMatrix = rotationY * rotationX;
            invStrikeRotationMatrix = rotationZ.inverse;
        }

        private void CalculateTranslationMatrix()
        {
            translationMatrix = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0),
                                                new Vector4(-rotationCentre.x, -rotationCentre.y, -rotationCentre.z, 1));
            inverseTranslationMatrix = translationMatrix.inverse;
        }

        private void CalculateFinalMatrices()
        {
            Matrix4x4 foldTransform = inverseTranslationMatrix * foldRotationMatrix * translationMatrix;

            // Could be optimised, but I'd rather not break things for now.
            Matrix4x4 strikeMat = (inverseTranslationMatrix * strikeRotationMatrix * translationMatrix).inverse;
            Matrix4x4 plungeDipMat = (inverseTranslationMatrix * plungeDipRotationMatrix * translationMatrix).inverse;
            Matrix4x4 invStrikeMat = (inverseTranslationMatrix * invStrikeRotationMatrix * translationMatrix).inverse;

            tiltTransform = foldTransform * strikeMat * plungeDipMat * invStrikeMat;
            inverseTiltTransform = foldTransform.inverse;
        }
        public override GeologicalTransformParameters GetGeologicalTransformParameters(bool anglesInDegrees)
        {
            GeologicalTransformParameters parameters;
            parameters.Strike = anglesInDegrees ? strike * Mathf.Rad2Deg : strike;
            parameters.Dip = anglesInDegrees ? dip * Mathf.Rad2Deg : dip;
            parameters.Plunge = anglesInDegrees ? plunge * Mathf.Rad2Deg : plunge;
            parameters.Amplitude = amplitude;
            parameters.Offset = offset;
            parameters.Period = period;
            parameters.ColourIndex1 = colourIndex1;
            parameters.ColourIndex2 = colourIndex2;

            return parameters;
        }
        public int[] GetFoldColourIndices()
        {
            return new int[2] { colourIndex1, colourIndex2 };
        }
        public override GeologicalTransform_CSS GetGeologicalTransform_CSS()
        {
            GeologicalTransform_CSS transform;
            transform.Type = (int)Type;
            transform.Transform = tiltTransform;
            transform.InverseTransform = inverseTiltTransform;
            transform.Amplitude = amplitude;
            transform.Period = period;
            transform.Offset = offset;

            return transform;
        }
        public override SerialisedGeologicalTransform GetSerialisedGeologicalTransform()
        {
            SerialisedGeologicalTransform serialisedTransform = new SerialisedGeologicalTransform();

            serialisedTransform.Type = TransformType.FoldTransform;
            serialisedTransform.Dip = dip;
            serialisedTransform.Strike = strike;
            serialisedTransform.Plunge = plunge;
            serialisedTransform.Amplitude = amplitude;
            serialisedTransform.Period = period;
            serialisedTransform.Offset = offset;
            serialisedTransform.ColourIndex1 = colourIndex1;
            serialisedTransform.ColourIndex2 = colourIndex2;

            return serialisedTransform;
        }
        public override GeologicalTransform Clone()
        {
            return new FoldTransform(strike, dip, plunge, amplitude, period, offset, colourIndex1, colourIndex2, rotationCentre, false);
        }
    }
}