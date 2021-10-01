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

namespace ARSandbox.GeologySimulation
{
    public class UI_BedMenu : MonoBehaviour
    {
        public GeologySimulation GeologySimulation;
        public GameObject UI_BedSelectionArea;
        public GameObject UI_BedOffsetSlider;
        public UI_ColorTransformMenuHandler UI_ColorTransformMenuHandler;
        public UI_AddNewBedItem UI_AddNewBedItemPrefab;
        public UI_GeologyBedListItem UI_GeologyBedListItemPrefab;
        
        private GeologicalLayerHandler geologicalLayerHandler;
        private List<GameObject> listItems;

        public void SetUpBedMenu()
        {
            if (GeologySimulation.SimulationReady)
            {
                if (listItems == null) listItems = new List<GameObject>();
                InitialiseBedList();
                UI_BedOffsetSlider.GetComponentInChildren<Slider>().value = geologicalLayerHandler.LayerStartingDepthOffset;
            }
        }

        private void InitialiseBedList()
        {
            geologicalLayerHandler = GeologySimulation.GeologicalLayerHandler;
            List<GeologicalLayer> layers = geologicalLayerHandler.GetGeologicalLayers();

            for (int i = listItems.Count - 1; i >= 0; i--)
            {
                Destroy(listItems[i].gameObject);
                Destroy(listItems[i]);
                listItems.RemoveAt(i);
            }

            UI_AddNewBedItem addBedBtn = Instantiate(UI_AddNewBedItemPrefab);
            addBedBtn.SetAddFunction(Action_AddItem);

            addBedBtn.gameObject.transform.SetParent(UI_BedSelectionArea.transform);
            addBedBtn.gameObject.transform.localScale = Vector3.one;
            listItems.Add(addBedBtn.gameObject);

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                GeologicalLayer layer = layers[i];

                UI_GeologyBedListItem item = Instantiate(UI_GeologyBedListItemPrefab);
                item.InitialiseItem(layer, i + 1);
                item.SetDeleteFunction(Action_DeleteItem);
                item.SetEditFunction(Action_EditItem);

                item.gameObject.transform.SetParent(UI_BedSelectionArea.transform);
                item.gameObject.transform.localScale = Vector3.one;

                listItems.Add(item.gameObject);
            }
        }

        private void Action_EditItem(UI_GeologyBedListItem item)
        {
            List<GeologicalLayer> layers = geologicalLayerHandler.GetGeologicalLayers();
            int itemIndex = listItems.IndexOf(item.gameObject);
            int layerIndex = layers.Count - itemIndex;

            UI_ColorTransformMenuHandler.OpenEditBedMenu(layers[layerIndex]);
        }

        private void Action_DeleteItem(UI_GeologyBedListItem item)
        {
            int itemIndex = listItems.IndexOf(item.gameObject);

            listItems.Remove(item.gameObject);
            Destroy(item.gameObject);
            Destroy(item);

            geologicalLayerHandler.RemoveGeologicalLayer(geologicalLayerHandler.GetGeologicalLayers().Count - itemIndex);
        }

        private void Action_AddItem()
        {
            GeologicalLayer newLayer = geologicalLayerHandler.AddRandomGeologicalLayer();
            UI_ColorTransformMenuHandler.OpenAddBedMenu(newLayer);

            /*UI_GeologyBedListItem item = Instantiate(UI_GeologyBedListItemPrefab);
            item.InitialiseItem(newLayer, geologicalLayerHandler.GetGeologicalLayers().Count);
            item.SetDeleteFunction(Action_DeleteItem);
            item.SetEditFunction(Action_EditItem);

            item.gameObject.transform.SetParent(UI_BedSelectionArea.transform);
            item.gameObject.transform.SetSiblingIndex(1);
            item.gameObject.transform.localScale = Vector3.one;

            listItems.Insert(1, item.gameObject);*/
        }
    }
}
