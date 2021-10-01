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

namespace ARSandbox.GeologySimulation {
    public class UI_EditBedMenu : MonoBehaviour {
        public GeologySimulation GeologySimulation;
        public GameObject UI_BedSelectionArea;
        public ScrollRect UI_BedSelectionScrollRect;
        public GameObject UI_TextureSelectionArea;
        public Text UI_BedTitle;
        public RawImage UI_BedImage;
        public Slider UI_Slider;
        public UI_BedSelectionItem UI_BedSelectionItemPrefab;
        public UI_TextureSelectionItem UI_TextureSelectionItemPrefab;
        public UI_ColorTransformMenuHandler UI_ColorTransformMenuHandler;

        private bool addingNewLayer;
        private GeologicalLayerTextures.Type originalTextureType;
        private float originalHeight;
        private GeologicalLayerDefinition originalLayerDefinition;

        private GeologicalLayer geologicalLayer;
        private List<UI_BedSelectionItem> bedSelectionItems;
        private List<UI_TextureSelectionItem> textureSelectionItems;

        public void SetUpEditMenu(GeologicalLayer geologicalLayer, bool addingNewLayer)
        {
            this.geologicalLayer = geologicalLayer;
            this.addingNewLayer = addingNewLayer;

            originalTextureType = geologicalLayer.TextureType;
            originalHeight = geologicalLayer.Height;
            originalLayerDefinition = geologicalLayer.LayerDefinition;

            UI_BedTitle.text = geologicalLayer.LayerDefinition.Name;
            UI_BedImage.texture = GeologicalLayerTextures.GetTexture(geologicalLayer.TextureType);
            UI_BedImage.color = geologicalLayer.LayerDefinition.Colour;
            UI_Slider.value = originalHeight * 2;

            SetUpBedSelectionItems();
            SetUpTextureSelectionItems();
        }

        private void SetUpBedSelectionItems()
        {
            if (bedSelectionItems != null)
            {
                for (int i = bedSelectionItems.Count - 1; i >= 0; i--)
                {
                    Destroy(bedSelectionItems[i]);
                    Destroy(bedSelectionItems[i].gameObject);
                }
                bedSelectionItems.Clear();
            }

            bedSelectionItems = new List<UI_BedSelectionItem>();

            List<GeologicalLayerDefinition> layerDefinitions = GeologicalLayerDefinitions.GetLayerDefintions();

            int currentLayer = 0;
            int selectedLayer = 0;
            foreach (GeologicalLayerDefinition layer in layerDefinitions)
            {
                UI_BedSelectionItem selectionItem = Instantiate(UI_BedSelectionItemPrefab);
                selectionItem.InitialiseItem(layer, layer == geologicalLayer.LayerDefinition);

                if (layer == geologicalLayer.LayerDefinition)
                {
                    selectedLayer = currentLayer;
                }

                selectionItem.gameObject.transform.SetParent(UI_BedSelectionArea.transform);
                selectionItem.gameObject.transform.localScale = Vector3.one;
                selectionItem.SetSelectFunction(Action_SelectItem);

                bedSelectionItems.Add(selectionItem);

                currentLayer += 1;
            }

            // The 'Count - 2' is a from trial and error for feel.
            // The position of the scroll rect seems a bit random.
            // TODO: Find out why.
            UI_BedSelectionScrollRect.verticalNormalizedPosition = Mathf.Clamp(1 - (selectedLayer - 1.8f) / (float)(layerDefinitions.Count - 4.5f), 0, 1);
        }

        private void SetUpTextureSelectionItems()
        {
            if (textureSelectionItems != null)
            {
                for (int i = textureSelectionItems.Count - 1; i >= 0; i--)
                {
                    Destroy(textureSelectionItems[i]);
                    Destroy(textureSelectionItems[i].gameObject);
                }
                textureSelectionItems.Clear();
            }

            textureSelectionItems = new List<UI_TextureSelectionItem>();

            for (int i = 0; i < GeologicalLayerTextures.TotalTypes; i++)
            {
                UI_TextureSelectionItem selectionItem = Instantiate(UI_TextureSelectionItemPrefab);

                selectionItem.InitialiseItem((GeologicalLayerTextures.Type)i, geologicalLayer.LayerDefinition.Colour, i == (int)geologicalLayer.TextureType);

                selectionItem.gameObject.transform.SetParent(UI_TextureSelectionArea.transform);
                selectionItem.gameObject.transform.localScale = Vector3.one;
                selectionItem.SetSelectFunction(Action_SelectTexture);

                textureSelectionItems.Add(selectionItem);
            }
        }
        public void UI_DeleteBed()
        {
            GeologySimulation.GeologicalLayerHandler.RemoveGeologicalLayer(geologicalLayer);
            UI_ColorTransformMenuHandler.OpenBedMenu();
        }
        public void UI_SetBedHeight(float height)
        {
            geologicalLayer.Height = height / 2;
        }

        public void UI_AcceptChanges()
        {
            UI_ColorTransformMenuHandler.OpenBedMenu();
        }

        public void UI_CancelChanges() {
            if (addingNewLayer)
            {
                GeologySimulation.GeologicalLayerHandler.RemoveGeologicalLayer(geologicalLayer);
            }
            else {
                geologicalLayer.LayerDefinition = originalLayerDefinition;
                geologicalLayer.Height = originalHeight;
                geologicalLayer.TextureType = originalTextureType;
            }

            UI_ColorTransformMenuHandler.OpenBedMenu();
        }

        private void Action_SelectItem(UI_BedSelectionItem item)
        {
            geologicalLayer.LayerDefinition = item.geologicalLayerDefinition;

            foreach (UI_BedSelectionItem bedSelectionItem in bedSelectionItems)
            {
                bedSelectionItem.SetSelected(geologicalLayer.LayerDefinition == bedSelectionItem.geologicalLayerDefinition);
            }

            foreach (UI_TextureSelectionItem textureSelectionItem in textureSelectionItems)
            {
                textureSelectionItem.SetColour(geologicalLayer.LayerDefinition.Colour);
            }

            UI_BedTitle.text = geologicalLayer.LayerDefinition.Name;
            UI_BedImage.color = geologicalLayer.LayerDefinition.Colour;
        }

        private void Action_SelectTexture(GeologicalLayerTextures.Type textureType)
        {
            geologicalLayer.TextureType = textureType;
            UI_BedImage.texture = GeologicalLayerTextures.GetTexture(geologicalLayer.TextureType);

            foreach(UI_TextureSelectionItem item in textureSelectionItems)
            {
                item.SetSelected(textureType == item.textureType);
            }
        }
    }
}
