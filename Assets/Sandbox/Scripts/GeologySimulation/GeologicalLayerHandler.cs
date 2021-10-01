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
    public class GeologicalLayerHandler
    {
        public float LayerStartingDepth;
        public float LayerStartingDepthOffset;
        private List<GeologicalLayer> geologicalLayers;

        public GeologicalLayerHandler(float layerStartingDepth)
        {
            LayerStartingDepth = layerStartingDepth;

            geologicalLayers = new List<GeologicalLayer>();
        }

        public void CreateLayersFromGeologyFile(SerialisedGeologyFile geologyFile)
        {
            geologicalLayers.Clear();

            foreach (SerialisedGeologicalLayer loadedLayer in geologyFile.GeologicalLayers)
            {
                GeologicalLayerDefinition layerDefinition = GeologicalLayerDefinitions.GetLayerDefinitionByName(loadedLayer.LayerName);
                AddGeologicalLayer(layerDefinition, loadedLayer.Height, loadedLayer.TextureType);
            }
        }

        public void SetStartingDepthOffset(float offset)
        {
            LayerStartingDepthOffset = offset;
        }

        public float GetStartingDepth()
        {
            return LayerStartingDepth + LayerStartingDepthOffset;
        }

        public GeologicalLayer AddGeologicalLayer(GeologicalLayerDefinition layerDefinition, float height, GeologicalLayerTextures.Type textureType)
        {
            GeologicalLayer newLayer = new GeologicalLayer(layerDefinition, height, textureType);
            geologicalLayers.Add(newLayer);

            return newLayer;
        }

        public GeologicalLayer AddRandomGeologicalLayer()
        {
            GeologicalLayerDefinition randomDefinition = GeologicalLayerDefinitions.GetRandomDefinition();

            return AddGeologicalLayer(randomDefinition, 25, GeologicalLayerTextures.Type.None);
        }

        public void RemoveGeologicalLayer(int index)
        {
            geologicalLayers.RemoveAt(index);
        }
        public void RemoveGeologicalLayer(GeologicalLayer layer)
        {
            geologicalLayers.Remove(layer);
        }
        public List<GeologicalLayer> GetGeologicalLayers()
        {
            return new List<GeologicalLayer>(geologicalLayers);
        }

        public GeologicalLayer_CSS[] GetGeologicalLayerBuffer()
        {
            float currentDepth = LayerStartingDepth + LayerStartingDepthOffset;
            int totalLayers = geologicalLayers.Count;
            GeologicalLayer_CSS[] geologicalLayers_CSS = new GeologicalLayer_CSS[totalLayers];

            for(int i = 0; i < geologicalLayers.Count; i++)
            {
                GeologicalLayer_CSS layer = geologicalLayers[i].GetGeologicalLayer_CSS();
                layer.Height = currentDepth + layer.Height;
                currentDepth = layer.Height;

                geologicalLayers_CSS[i] = layer;
            }

            return geologicalLayers_CSS;
        }

        public SerialisedGeologicalLayer[] GetSerialisedGeologicalLayers()
        {
            int totalLayers = geologicalLayers.Count;
            SerialisedGeologicalLayer[] SerialisedGeologicalLayers = new SerialisedGeologicalLayer[totalLayers];

            for (int i = 0; i < geologicalLayers.Count; i++)
            {
                SerialisedGeologicalLayers[i] = geologicalLayers[i].GetSerialisedGeologicalLayer();
            }

            return SerialisedGeologicalLayers;
        }
    }
}
