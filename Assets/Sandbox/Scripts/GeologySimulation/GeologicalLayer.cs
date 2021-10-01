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

namespace ARSandbox.GeologySimulation 
{
    public class GeologicalLayer : IComparable
    {
        public GeologicalLayerDefinition LayerDefinition;
        public float Height;
        public GeologicalLayerTextures.Type TextureType;

        public GeologicalLayer(GeologicalLayerDefinition LayerDefinition, float Height, GeologicalLayerTextures.Type TextureType)
        {
            this.LayerDefinition = LayerDefinition;
            this.Height = Height;
            this.TextureType = TextureType;
        }

        public int CompareTo(System.Object obj)
        {
            GeologicalLayer geologicalLayer = (GeologicalLayer)obj;
            if (Height < geologicalLayer.Height)
            {
                return -1;
            }
            if (Height > geologicalLayer.Height)
            {
                return 1;
            }
            return 0;
        }

        public GeologicalLayer_CSS GetGeologicalLayer_CSS()
        {
            GeologicalLayer_CSS layer;
            layer.Colour = new Vector3(LayerDefinition.Colour.r, LayerDefinition.Colour.g, LayerDefinition.Colour.b);
            layer.Height = Height;
            layer.TextureType = (int)TextureType;
            return layer;
        }

        public SerialisedGeologicalLayer GetSerialisedGeologicalLayer()
        {
            return new SerialisedGeologicalLayer(LayerDefinition.Name, Height, TextureType);
        }
    }

    [System.Serializable]
    public class SerialisedGeologicalLayer
    {
        public string LayerName;
        public float Height;
        public GeologicalLayerTextures.Type TextureType;

        public SerialisedGeologicalLayer (string LayerName, float Height, GeologicalLayerTextures.Type TextureType)
        {
            this.LayerName = LayerName;
            this.Height = Height;
            this.TextureType = TextureType;
        }
    }
}
