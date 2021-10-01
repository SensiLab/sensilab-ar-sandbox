//  
//  UI_TransformMenu.cs
//
//	Copyright 2021 SensiLab, Monash University <sensilab@monash.edu>
//
//  This file is part of sensilab-ar-sandbox.
//
//  sensilab-ar-sandbox is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  sensilab-ar-sandbox is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with sensilab-ar-sandbox.  If not, see <https://www.gnu.org/licenses/>.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARSandbox.GeologySimulation.GeologicalTransforms;

namespace ARSandbox.GeologySimulation
{
    public class UI_TransformMenu : MonoBehaviour
    {
        public GeologySimulation GeologySimulation;
        public GameObject UI_TransformSelectionArea;
        public UI_ColorTransformMenuHandler UI_ColorTransformMenuHandler;
        public UI_AddNewTransformItem UI_AddNewTransformItemPrefab;
        public UI_GeologicalTransformListItem UI_GeologicalTransformListItemPrefab;

        private GeologicalTransformHandler geologicalTransformHandler;
        private List<GameObject> listItems;

        public void SetUpTransformMenu()
        {
            if (GeologySimulation.SimulationReady)
            {
                if (listItems == null) listItems = new List<GameObject>();
                InitialiseTransformList();
            }
        }

        private void InitialiseTransformList()
        {
            geologicalTransformHandler = GeologySimulation.GeologicalTransformHandler;
            List<GeologicalTransform> transforms = geologicalTransformHandler.GetGeologicalTransforms();

            for (int i = listItems.Count - 1; i >= 0; i--)
            {
                Destroy(listItems[i].gameObject);
                Destroy(listItems[i]);
                listItems.RemoveAt(i);
            }

            UI_AddNewTransformItem addTransformBtn = Instantiate(UI_AddNewTransformItemPrefab);
            addTransformBtn.SetAddFunction(Action_AddItem);

            addTransformBtn.gameObject.transform.SetParent(UI_TransformSelectionArea.transform);
            addTransformBtn.gameObject.transform.localScale = Vector3.one;
            listItems.Add(addTransformBtn.gameObject);

            for (int i = transforms.Count - 1; i >= 0; i--)
            {
                GeologicalTransform transform = transforms[i];

                UI_GeologicalTransformListItem item = Instantiate(UI_GeologicalTransformListItemPrefab);
                item.InitialiseItem(transform, i + 1);
                item.SetDeleteFunction(Action_DeleteItem);
                item.SetEditFunction(Action_EditItem);

                item.gameObject.transform.SetParent(UI_TransformSelectionArea.transform);
                item.gameObject.transform.localScale = Vector3.one;

                listItems.Add(item.gameObject);
            }
        }

        private void Action_EditItem(UI_GeologicalTransformListItem item)
        {
            List<GeologicalTransform> transforms = geologicalTransformHandler.GetGeologicalTransforms();
            int itemIndex = listItems.IndexOf(item.gameObject);
            int transformIndex = transforms.Count - itemIndex;

            UI_ColorTransformMenuHandler.OpenEditTransformMenu(transforms[transformIndex]);
        }

        private void Action_DeleteItem(UI_GeologicalTransformListItem item)
        {
            int itemIndex = listItems.IndexOf(item.gameObject);

            listItems.Remove(item.gameObject);
            Destroy(item.gameObject);
            Destroy(item);

            int transformIndex = geologicalTransformHandler.GetGeologicalTransforms().Count - itemIndex;
            geologicalTransformHandler.RemoveTransform(transformIndex);
        }

        private void Action_AddItem()
        {
            GeologicalTransform newTransform = geologicalTransformHandler.AddFlatTiltTransform();

            UI_ColorTransformMenuHandler.OpenAddTransformMenu(newTransform);
        }
    }
}
