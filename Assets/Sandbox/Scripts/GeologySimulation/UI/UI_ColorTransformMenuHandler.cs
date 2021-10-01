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
using ARSandbox.GeologySimulation.GeologicalTransforms;

namespace ARSandbox.GeologySimulation
{
    public class UI_ColorTransformMenuHandler : MonoBehaviour
    {
        public UI_BedMenu UI_BedMenu;
        public UI_EditBedMenu UI_EditBedMenu;
        public UI_TransformMenu UI_TransformMenu;
        public UI_EditTransformMenu UI_EditTransformMenu;
        public UI_GeologyFileMenu UI_GeologyFileMenu;
         
        public GameObject UI_BedMenuBtn;
        public GameObject UI_TransformMenuBtn;
        public GameObject UI_GeologyFileMenuBtn;
        public GameObject UI_MenuTitle;
        public Image UI_MenuBG;

        public static readonly Color TransformColour = new Color(245.0f / 255.0f, 178.0f / 255.0f, 0, 1);
        public static readonly Color BedColour = new Color(56.0f / 255.0f, 205.0f / 255.0f, 1, 1);
        public static readonly Color GeologyFileColour = new Color(27.0f / 255.0f, 169.0f / 255.0f, 0, 1);
        public static readonly Color SelectedColour = new Color(128.0f / 255.0f, 1, 0, 1);

        public enum GeologyMenu
        {
            BedMenu,
            EditBedMenu,
            TransformMenu,
            EditTransformMenu,
            GeologyFileMenu,
        }

        public const string BED_MENU_TITLE = "Beds";
        public const string ADD_BED_MENU_TITLE = "Add Bed Menu";
        public const string EDIT_BED_MENU_TITLE = "Edit Bed Menu";
        public const string TRANSFORM_MENU_TITLE = "Transforms";
        public const string ADD_TRANSFORM_MENU_TITLE = "Add Transform Menu";
        public const string EDIT_TRANSFORM_MENU_TITLE = "Edit Transform Menu";

        public GeologyMenu CurrentGeologyMenu { get; private set; }

        private void CloseMenus()
        {
            UI_BedMenu.gameObject.SetActive(false);
            UI_EditBedMenu.gameObject.SetActive(false);
            UI_TransformMenu.gameObject.SetActive(false);
            UI_EditTransformMenu.gameObject.SetActive(false);
            UI_GeologyFileMenu.gameObject.SetActive(false);
        }
        private void ToggleMenuButtons(bool enabled)
        {
            UI_MenuTitle.SetActive(!enabled);
            UI_BedMenuBtn.SetActive(enabled);
            UI_TransformMenuBtn.SetActive(enabled);
            UI_GeologyFileMenuBtn.SetActive(enabled);
        }
        private void SetTitleColourAndText(Color color, string title)
        {
            UI_MenuTitle.GetComponent<Image>().color = SelectedColour;
            UI_MenuTitle.transform.GetChild(0).GetComponent<Image>().color = color;
            UI_MenuTitle.GetComponentInChildren<Text>().text = title;
        }
        public void UI_OpenGeologyFileMenu()
        {
            CurrentGeologyMenu = GeologyMenu.GeologyFileMenu;
            
            CloseMenus();
            UI_GeologyFileMenu.gameObject.SetActive(true);
            UI_GeologyFileMenu.SetUpFileMenu();

            ToggleMenuButtons(true);
            UI_BedMenuBtn.GetComponent<Image>().color = BedColour;
            UI_TransformMenuBtn.GetComponent<Image>().color = TransformColour;
            UI_GeologyFileMenuBtn.GetComponent<Image>().color = SelectedColour;
            UI_MenuBG.color = GeologyFileColour;
        }
        public void OpenBedMenu()
        {
            CurrentGeologyMenu = GeologyMenu.BedMenu;

            CloseMenus();
            UI_BedMenu.gameObject.SetActive(true);
            UI_BedMenu.SetUpBedMenu();

            ToggleMenuButtons(true);
            UI_BedMenuBtn.GetComponent<Image>().color = SelectedColour;
            UI_TransformMenuBtn.GetComponent<Image>().color = TransformColour;
            UI_GeologyFileMenuBtn.GetComponent<Image>().color = GeologyFileColour;
            UI_MenuBG.color = BedColour;
        }

        public void OpenEditBedMenu(GeologicalLayer layer)
        {
            CurrentGeologyMenu = GeologyMenu.EditBedMenu;

            CloseMenus();
            UI_EditBedMenu.gameObject.SetActive(true);
            UI_EditBedMenu.SetUpEditMenu(layer, false);

            ToggleMenuButtons(false);
            SetTitleColourAndText(BedColour, EDIT_BED_MENU_TITLE);

            UI_MenuBG.color = BedColour;
        }
        public void OpenAddBedMenu(GeologicalLayer layer)
        {
            CurrentGeologyMenu = GeologyMenu.EditBedMenu;

            CloseMenus();
            UI_EditBedMenu.gameObject.SetActive(true);
            UI_EditBedMenu.SetUpEditMenu(layer, true);

            ToggleMenuButtons(false);
            SetTitleColourAndText(BedColour, ADD_BED_MENU_TITLE);

            UI_MenuBG.color = BedColour;
        }
        public void OpenTransformMenu()
        {
            CurrentGeologyMenu = GeologyMenu.TransformMenu;

            CloseMenus();
            UI_TransformMenu.gameObject.SetActive(true);
            UI_TransformMenu.SetUpTransformMenu();

            ToggleMenuButtons(true);
            UI_BedMenuBtn.GetComponent<Image>().color = BedColour;
            UI_TransformMenuBtn.GetComponent<Image>().color = SelectedColour;
            UI_GeologyFileMenuBtn.GetComponent<Image>().color = GeologyFileColour;
            UI_MenuBG.color = TransformColour;
        }

        public void OpenEditTransformMenu(GeologicalTransform transform)
        {
            CurrentGeologyMenu = GeologyMenu.EditTransformMenu;

            CloseMenus();
            UI_EditTransformMenu.gameObject.SetActive(true);
            UI_EditTransformMenu.SetUpEditMenu(transform, false);

            ToggleMenuButtons(false);
            SetTitleColourAndText(TransformColour, EDIT_TRANSFORM_MENU_TITLE);
            UI_MenuBG.color = TransformColour;
        }

        public void OpenAddTransformMenu(GeologicalTransform transform)
        {
            CurrentGeologyMenu = GeologyMenu.EditTransformMenu;

            CloseMenus();
            UI_EditTransformMenu.gameObject.SetActive(true);
            UI_EditTransformMenu.SetUpEditMenu(transform, true);

            ToggleMenuButtons(false);
            SetTitleColourAndText(TransformColour, ADD_TRANSFORM_MENU_TITLE);
            UI_MenuBG.color = TransformColour;
        }
    }
}
