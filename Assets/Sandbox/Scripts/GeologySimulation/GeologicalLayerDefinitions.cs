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
    public static class GeologicalLayerDefinitions
    {
        public static GeologicalLayerDefinition Basalt = new GeologicalLayerDefinition("Basalt", new Color(89.0f / 255.0f, 89.0f / 255.0f, 89.0f / 255.0f));
        public static GeologicalLayerDefinition Gabbro = new GeologicalLayerDefinition("Gabbro", new Color(1, 0, 0));
        public static GeologicalLayerDefinition Granodiorite = new GeologicalLayerDefinition("Granodiorite", new Color(255.0f / 255.0f, 51.0f / 255.0f, 204.0f / 255.0f));
        public static GeologicalLayerDefinition Rhyolite = new GeologicalLayerDefinition("Rhyolite", new Color(255.0f / 255.0f, 153.0f / 255.0f, 255.0f / 255.0f));
        public static GeologicalLayerDefinition Sandstone = new GeologicalLayerDefinition("Sandstone", new Color(254.0f / 255.0f, 242.0f / 255.0f, 35.0f / 255.0f));
        public static GeologicalLayerDefinition Mudstone = new GeologicalLayerDefinition("Mudstone", new Color(255.0f / 255.0f, 196.0f / 255.0f, 119.0f / 255.0f));
        public static GeologicalLayerDefinition Conglomerate = new GeologicalLayerDefinition("Conglomerate", new Color(255.0f / 255.0f, 207.0f / 255.0f, 0f / 255.0f));
        public static GeologicalLayerDefinition Limestone = new GeologicalLayerDefinition("Limestone", new Color(51.0f / 255.0f, 204.0f / 255.0f, 255f / 255.0f));
        public static GeologicalLayerDefinition Coal = new GeologicalLayerDefinition("Coal", new Color(108.0f / 255.0f, 72.0f / 255.0f, 0.0f / 255.0f));
        public static GeologicalLayerDefinition Breccia = new GeologicalLayerDefinition("Breccia", new Color(0f / 255.0f, 128f / 255.0f, 0.0f / 255.0f));
        public static GeologicalLayerDefinition Slate = new GeologicalLayerDefinition("Slate", new Color(153f / 255.0f, 51f / 255.0f, 255.0f / 255.0f));
        public static GeologicalLayerDefinition Schist = new GeologicalLayerDefinition("Schist", new Color(204f / 255.0f, 153f / 255.0f, 255.0f / 255.0f));
        public static GeologicalLayerDefinition Gneiss = new GeologicalLayerDefinition("Gneiss", new Color(102f / 255.0f, 0f / 255.0f, 204.0f / 255.0f));
        public static GeologicalLayerDefinition Hornfels = new GeologicalLayerDefinition("Hornfels", new Color(86f / 255.0f, 67f / 255.0f, 40.0f / 255.0f));
        public static GeologicalLayerDefinition Marble = new GeologicalLayerDefinition("Marble", new Color(0f / 255.0f, 0f / 255.0f, 153.0f / 255.0f));
        public static GeologicalLayerDefinition Quartzite = new GeologicalLayerDefinition("Quartzite", new Color(255f / 255.0f, 255f / 255.0f, 153.0f / 255.0f));

        private static SortedDictionary<string, GeologicalLayerDefinition> layerDictionary;
        public static List<GeologicalLayerDefinition> sortedLayerList;

        static GeologicalLayerDefinitions()
        {
            layerDictionary = new SortedDictionary<string, GeologicalLayerDefinition>();

            layerDictionary.Add(Basalt.Name, Basalt);
            layerDictionary.Add(Gabbro.Name, Gabbro);
            layerDictionary.Add(Granodiorite.Name, Granodiorite);
            layerDictionary.Add(Rhyolite.Name, Rhyolite);
            layerDictionary.Add(Sandstone.Name, Sandstone);
            layerDictionary.Add(Mudstone.Name, Mudstone);
            layerDictionary.Add(Conglomerate.Name, Conglomerate);
            layerDictionary.Add(Limestone.Name, Limestone);
            layerDictionary.Add(Coal.Name, Coal);
            layerDictionary.Add(Breccia.Name, Breccia);
            layerDictionary.Add(Slate.Name, Slate);
            layerDictionary.Add(Schist.Name, Schist);
            layerDictionary.Add(Gneiss.Name, Gneiss);
            layerDictionary.Add(Hornfels.Name, Hornfels);
            layerDictionary.Add(Marble.Name, Marble);
            layerDictionary.Add(Quartzite.Name, Quartzite);

            sortedLayerList = new List<GeologicalLayerDefinition>(layerDictionary.Values);
        }

        public static GeologicalLayerDefinition GetLayerDefinitionByName(string name)
        {
            GeologicalLayerDefinition layerDefinition;

            if (layerDictionary.TryGetValue(name, out layerDefinition))
            {
                return layerDefinition;
            } else
            {
                Debug.Log(string.Format("Error! Cannot find layer: {0}", name));
                return Basalt;
            }
        }

        public static List<GeologicalLayerDefinition> GetLayerDefintions()
        {
            return sortedLayerList;
        }

        public static GeologicalLayerDefinition GetRandomDefinition()
        {
            float totalDefinitions = sortedLayerList.Count;
            int definitionIndex = (int)Mathf.Floor(UnityEngine.Random.value * totalDefinitions);
            return sortedLayerList[definitionIndex];
        }
    }

    public class GeologicalLayerDefinition : IComparable
    {
        public string Name
        {
            get; private set;
        }
        public Color Colour
        {
            get;
            private set;
        }

        public GeologicalLayerDefinition(string Name, Color Colour)
        {
            this.Name = Name;
            this.Colour = Colour;
        }

        public int CompareTo(System.Object obj)
        {
            GeologicalLayerDefinition geologicalLayer = (GeologicalLayerDefinition)obj;
            return string.Compare(Name, geologicalLayer.Name);
        }
    }

    public static class GeologicalLayerTextures
    {
        public enum Type
        {
            None,
            Dots,
            Plus
        }

        public const int TotalTypes = 3;

        static Texture2D DotsTexture = Resources.Load("DotPattern") as Texture2D;
        static Texture2D PlusTexture = Resources.Load("PlusPattern") as Texture2D;
        public readonly static Texture2DArray LayerPatternTexArray;

        static GeologicalLayerTextures()
        {
            LayerPatternTexArray = new Texture2DArray(256, 256, 3, TextureFormat.RGB24, false);
            Graphics.CopyTexture(DotsTexture, 0, 0, LayerPatternTexArray, 1, 0);
            Graphics.CopyTexture(PlusTexture, 0, 0, LayerPatternTexArray, 2, 0);
            LayerPatternTexArray.Apply();
        }

        public static Texture2D GetTexture(Type type)
        {
            switch(type)
            {
                case Type.None:
                    return null;
                case Type.Dots:
                    return DotsTexture;
                case Type.Plus:
                    return PlusTexture;
            }
            return null;
        }
    }
}
