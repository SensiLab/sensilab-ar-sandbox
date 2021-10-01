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

namespace ARSandbox.GeologySimulation
{
    public enum StructSizes
    {
        GEOLOGICAL_TRANSFORM_SIZE = 144,
        GEOLOGICAL_LAYER_SIZE = 20,
        GEOLOGICAL_COLOUR_BUFFER_SIZE = 16
    };

    public struct GeologicalLayer_CSS
    {
        public Vector3 Colour;
        public float Height;
        public int TextureType;
    }

    namespace GeologicalTransforms {
        // CSS - Compute Shader Struct
        public struct GeologicalTransform_CSS
        {
            public int Type;
            public Matrix4x4 Transform;
            public Matrix4x4 InverseTransform;
            public float Amplitude;
            public float Period;
            public float Offset;
        }
        public struct GeologicalTransformParameters
        {
            public float Strike;
            public float Dip;
            public float Plunge;
            public float Amplitude;
            public float Period;
            public float Offset;
            public int ColourIndex1;
            public int ColourIndex2;
        }
        public static class TransformColours
        {
            public static Color Empty = new Color(0, 0, 0, 0);
            public static Color Red = new Color(1, 0, 0, 1);
            public static Color Orange = new Color(1, 0.5f, 0, 1);
            public static Color Yellow = new Color(1, 1, 0, 1);
            public static Color LightGreen = new Color(0.5f, 1, 0, 1);
            public static Color Green = new Color(0, 0.5f, 0, 1);
            public static Color BlueGreen = new Color(0, 0.5f, 0.5f, 1);
            public static Color Cyan = new Color(0, 1, 1, 1);
            public static Color LightBlue = new Color(0, 0.5f, 1, 1);
            public static Color Blue = new Color(0, 0, 1, 1);
            public static Color Purple = new Color(0.5f, 0, 1, 1);
            public static Color Pink = new Color(1, 0, 1, 1);
            public static Color Magenta = new Color(1, 0, 0.5f, 1);

            public static Color[] ColourList = new Color[13] { Empty, Red, Orange, Yellow, LightGreen, Green, BlueGreen, Cyan,
                                                               LightBlue, Blue, Purple, Pink, Magenta };
        }
        public abstract class GeologicalTransform
        {
            public const string TRANSFORM_TITLE = "Unknown Transform";
            public enum TransformType
            {
                TiltTransform = 0,
                FoldTransform = 1,
                FaultTransform = 2
            };
            public const int TOTAL_TRANSFORM_TYPES = 3;
            public abstract TransformType Type
            {
                get;
            }
            public abstract GeologicalTransform_CSS GetGeologicalTransform_CSS();
            public abstract void ChangeRotationCentre(Vector3 rotationCentre);
            public abstract Texture2D GetIconTexture();
            public abstract string GetTransformName();
            public abstract GeologicalTransformParameters GetGeologicalTransformParameters(bool anglesInDegrees);
            public abstract SerialisedGeologicalTransform GetSerialisedGeologicalTransform();
            public abstract GeologicalTransform Clone();
            public static Texture2D GetTransformIcon(TransformType type)
            {
                switch(type)
                {
                    case TransformType.TiltTransform:
                        return TiltTransform.IconTexture;
                    case TransformType.FoldTransform:
                        return FoldTransform.IconTexture;
                    case TransformType.FaultTransform:
                        return FaultTransform.IconTexture;
                }
                return null;
            }
            public static string GetTransformTitle(TransformType type)
            {
                switch (type)
                {
                    case TransformType.TiltTransform:
                        return TiltTransform.TRANSFORM_TITLE;
                    case TransformType.FoldTransform:
                        return FoldTransform.TRANSFORM_TITLE;
                    case TransformType.FaultTransform:
                        return FaultTransform.TRANSFORM_TITLE;
                }
                return  TRANSFORM_TITLE;
            }
        }

        [System.Serializable]
        public class SerialisedGeologicalTransform
        {
            public GeologicalTransform.TransformType Type = GeologicalTransform.TransformType.TiltTransform;
            public float Strike = 0;
            public float Dip = 0;
            public float Plunge = 0;
            public float Amplitude = 0;
            public float Period = 0;
            public float Offset = 0;
            public int ColourIndex1 = 0;
            public int ColourIndex2 = 0;

        }
    }
}