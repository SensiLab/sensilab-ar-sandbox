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
    public class UI_GeologyFileMenu : MonoBehaviour
    {
        public GeologySimulation GeologySimulation;
        public GeologyFileManager GeologyFileManager;
        public UI_MenuManager UI_MenuManager;
        public Text UI_CurrentFileText;
        public Button UI_RenameButton;
        public Button UI_DeleteButton;
        public Button UI_SaveReplaceButton;
        public GameObject UI_GeologyFileSelectionArea;
        public UI_GeologyFileSelectionItem UI_GeologyFileSelectionItemPrefab;

        private List<UI_GeologyFileSelectionItem> loadedGeologyFileListItems;

        private string saveInputTitle = "Geology File Name";
        private string saveInitialText = "Enter Name";

        private string noFileSelectedText = "No File Selected";
        private string renameInputTitle = "Rename Geology File";

        private SerialisedGeologyFile selectedGeologyFile;
        private UI_GeologyFileSelectionItem selectedGeologyFileListItem;
        private UI_GeologyFileSelectionItem possibleSelectionItem;
        private bool fileSelectedAfterOpeningMenu = false;

        public void SetUpFileMenu()
        {
            UI_CurrentFileText.text = noFileSelectedText;
            fileSelectedAfterOpeningMenu = false;

            PopulateGeologyFileListItems();
        }
        private void ClearGeologyFileListItems()
        {
            if (loadedGeologyFileListItems != null)
            {
                selectedGeologyFileListItem = null;
                for (int i = loadedGeologyFileListItems.Count - 1; i >= 0; i--)
                {
                    Destroy(loadedGeologyFileListItems[i].gameObject);
                    Destroy(loadedGeologyFileListItems[i]);
                    loadedGeologyFileListItems.RemoveAt(i);
                }
            } else
            {
                loadedGeologyFileListItems = new List<UI_GeologyFileSelectionItem>();
            }
        }
        private void PopulateGeologyFileListItems()
        {
            ClearGeologyFileListItems();
            List<SerialisedGeologyFile> loadedGeologyFiles = GeologyFileManager.GetLoadedGeologyFiles();

            if (selectedGeologyFile == null)
            {
                UI_CurrentFileText.text = noFileSelectedText;
                UI_RenameButton.interactable = false;
                UI_DeleteButton.interactable = false;
                UI_SaveReplaceButton.interactable = false;
            } else
            {
                UI_RenameButton.interactable = true;
                UI_DeleteButton.interactable = true;
                UI_SaveReplaceButton.interactable = true;
            }

            foreach (SerialisedGeologyFile geologyFile in loadedGeologyFiles)
            {
                UI_GeologyFileSelectionItem item = Instantiate(UI_GeologyFileSelectionItemPrefab);

                bool selected = false;
                if (selectedGeologyFile != null)
                {
                    if (selectedGeologyFile.Filename == geologyFile.Filename)
                    {
                        selected = true;
                        selectedGeologyFileListItem = item;
                        UI_CurrentFileText.text = geologyFile.Filename;

                        UI_RenameButton.interactable = true;
                        UI_DeleteButton.interactable = true;
                        UI_SaveReplaceButton.interactable = true;
                    }
                }
                
                item.InitialiseItem(geologyFile, selected);

                item.SetSelectFunction(Action_SelectGeologyFile);

                item.gameObject.transform.SetParent(UI_GeologyFileSelectionArea.transform);
                item.gameObject.transform.localScale = Vector3.one;

                loadedGeologyFileListItems.Add(item);
            }
        }
        public void UI_DeleteCurrentGeology()
        {
            UI_MenuManager.OpenGeologyFileDeletePanel(selectedGeologyFile.Filename);
        }
        public void Accept_DeleteCurrentGeology()
        {
            if (GeologyFileManager.DeleteGeologyFile(selectedGeologyFile))
            {
                selectedGeologyFile = null;
                PopulateGeologyFileListItems();
            }
        }
        public void UI_RenameCurrentGeology()
        {
            UI_MenuManager.OpenOnScreenKeyboard(renameInputTitle, selectedGeologyFile.Filename, Action_AcceptRenameGeology, Action_CancelInput);

        }
        public void UI_SaveCurrentGeology()
        {
            UI_MenuManager.OpenOnScreenKeyboard(saveInputTitle, saveInitialText, Action_AcceptSaveGeology, Action_CancelInput);
        }
        public void UI_SaveOverCurrentGeology()
        {
            UI_MenuManager.OpenGeologyFileReplacePanel(selectedGeologyFile.Filename);
        }
        public void Accept_SaveOverCurrentGeology()
        {
            SerialisedGeologyFile geologyFile = GeologySimulation.CreateSerialisedGeologyFile();
            geologyFile.Filename = selectedGeologyFile.Filename;
            GeologyFileManager.DeleteGeologyFile(selectedGeologyFile);

            if (GeologyFileManager.SaveGeologyFile(geologyFile))
            {
                selectedGeologyFile = geologyFile;
                PopulateGeologyFileListItems();
            }

            fileSelectedAfterOpeningMenu = true;
        }
        private void Action_AcceptRenameGeology(string outputString)
        {
            if (GeologyFileManager.RenameGeologyFile(outputString, selectedGeologyFile))
            {
                PopulateGeologyFileListItems();
            }
        }

        private void Action_AcceptSaveGeology(string outputString)
        {
            SerialisedGeologyFile geologyFile = GeologySimulation.CreateSerialisedGeologyFile();
            geologyFile.Filename = outputString;

            if (GeologyFileManager.SaveGeologyFile(geologyFile))
            {
                selectedGeologyFile = geologyFile;
                PopulateGeologyFileListItems();
                fileSelectedAfterOpeningMenu = true;
            }
        }

        private void Action_CancelInput()
        {
            // Do something on cancel.
        }
        public void Accept_SelectGeologyFile()
        {
            GeologySimulation.LoadSerialisedGeologyFile(possibleSelectionItem.GeologyFile);

            if (selectedGeologyFileListItem != null)
            {
                selectedGeologyFileListItem.SetSelected(false);
            }
            selectedGeologyFileListItem = possibleSelectionItem;
            selectedGeologyFileListItem.SetSelected(true);
            selectedGeologyFile = possibleSelectionItem.GeologyFile;

            UI_CurrentFileText.text = selectedGeologyFile.Filename;
            UI_RenameButton.interactable = true;
            UI_DeleteButton.interactable = true;
            UI_SaveReplaceButton.interactable = true;

            fileSelectedAfterOpeningMenu = true;
        }
        private void Action_SelectGeologyFile(UI_GeologyFileSelectionItem item)
        {
            possibleSelectionItem = item;
            if (fileSelectedAfterOpeningMenu)
            {
                Accept_SelectGeologyFile();
            } else {
                UI_MenuManager.OpenGeologyFileSelectPanel(item.GeologyFile.Filename);
            }
        }
    }
}
