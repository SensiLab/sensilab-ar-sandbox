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

namespace ARSandbox.TopographyBuilder {
    public class UI_TopographyBuilderMenu : MonoBehaviour
    {
        public TopographyBuilder TopographyBuilder;
        public UI_TopographySelectionItem UI_TopographySelectionItemPrefab;
        public UI_TopographyRename UI_TopographySelectionTitle;
        public Button UI_DeleteButton;
        public GameObject UI_TopographySelectionArea;
        public Slider UI_ValidHeightRangeSlider;
        public Slider UI_HeightOffsetSlider;

        private List<GameObject> topographyListItems;
        private UI_TopographySelectionItem selectedTopographyItem;

        void OnEnable()
        {
            SetUpTopographyMenu();
            TopographyBuilder.OnNewListLoaded += RepopulateTopographyList;
        }
        void OnDisable()
        {
            TopographyBuilder.OnNewListLoaded -= RepopulateTopographyList;
        }
        public void SetUpTopographyMenu()
        {
            if (topographyListItems == null) topographyListItems = new List<GameObject>();
            RepopulateTopographyList();

            UI_ValidHeightRangeSlider.value = TopographyBuilder.ValidHeightRange;
            UI_HeightOffsetSlider.value = TopographyBuilder.HeightOffset;
        }
        public void RepopulateTopographyList()
        {
            List<LoadedTopography> topographies = TopographyBuilder.loadedTopographies;
           
            if (topographies != null)
            {
                for (int i = topographyListItems.Count - 1; i >= 0; i--)
                {
                    Destroy(topographyListItems[i].gameObject);
                    Destroy(topographyListItems[i]);
                    topographyListItems.RemoveAt(i);
                }
                selectedTopographyItem = null;

                foreach (LoadedTopography loadedTopography in topographies)
                {
                    UI_TopographySelectionItem item = Instantiate(UI_TopographySelectionItemPrefab);

                    item.InitialiseItem(loadedTopography, loadedTopography == TopographyBuilder.SelectedTopography);
                    if (loadedTopography == TopographyBuilder.SelectedTopography)
                    {
                        selectedTopographyItem = item;
                        UI_TopographySelectionTitle.SetText(selectedTopographyItem.LoadedTopography.DisplayName);
                    }

                    item.SetSelectFunction(Action_SelectItem);

                    item.gameObject.transform.SetParent(UI_TopographySelectionArea.transform);
                    item.gameObject.transform.localScale = Vector3.one;

                    topographyListItems.Add(item.gameObject);
                }

                if (selectedTopographyItem == null)
                {
                    UI_TopographySelectionTitle.SetText("No Topography Selected");
                    UI_TopographySelectionTitle.GetComponent<Button>().interactable = false;
                    UI_DeleteButton.interactable = false;
                } else
                {
                    UI_TopographySelectionTitle.GetComponent<Button>().interactable = true;
                    UI_DeleteButton.interactable = true;
                }
            }

            UI_ValidHeightRangeSlider.value = TopographyBuilder.ValidHeightRange;
            UI_HeightOffsetSlider.value = TopographyBuilder.HeightOffset;
        }

        private void Action_SelectItem(UI_TopographySelectionItem selectionItem)
        {
            if (selectedTopographyItem != null)
            {
                selectedTopographyItem.SetSelected(false);
            }
            selectionItem.SetSelected(true);
            selectedTopographyItem = selectionItem;
            UI_TopographySelectionTitle.SetText(selectedTopographyItem.LoadedTopography.DisplayName);
            UI_TopographySelectionTitle.GetComponent<Button>().interactable = true;
            UI_DeleteButton.interactable = true;

            TopographyBuilder.UI_SelectTopography(selectionItem.LoadedTopography);
        }

        public void UI_DeleteTopography()
        {
            if (selectedTopographyItem != null)
            {
                TopographyBuilder.UI_DeleteTopography(selectedTopographyItem.LoadedTopography);
            }
        }
    }
}
