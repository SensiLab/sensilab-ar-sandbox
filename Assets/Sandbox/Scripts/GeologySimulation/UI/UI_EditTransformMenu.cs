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
    public class UI_EditTransformMenu : MonoBehaviour
    {
        public GeologySimulation GeologySimulation;
        public Text UI_TransformTitle;
        public Text UI_PlungeRakeTitle;
        public RawImage UI_TransformImage;
        public GameObject UI_TransformSelectionArea;
        public ScrollRect UI_TransformScrollRect;
        public UI_ColorTransformMenuHandler UI_ColorTransformMenuHandler;
        public UI_TransformSelectionItem UI_TransformSelectionItemPrefab;
        public UI_ColourSelectionItem UI_ColourSelectionItemPrefab;
        public UI_Dial UI_StrikeDial;
        public UI_Dial UI_DipDial;
        public UI_Dial UI_PlungeDial;

        public GameObject UI_AmplitudeTitle;
        public GameObject UI_AmplitudeSlider;
        public GameObject UI_OffsetTitle;
        public GameObject UI_OffsetSlider;
        public GameObject UI_PeriodTitle;
        public GameObject UI_PeriodSlider;
        public GameObject UI_ColourSelectionTitle1;
        public GameObject UI_ColourSelectionTitle2;
        public GameObject UI_Colour1SelectionArea;
        public GameObject UI_Colour2SelectionArea;
        public ScrollRect UI_Colour1ScrollRect;
        public ScrollRect UI_Colour2ScrollRect;

        private bool addingNewTransform;
        private GeologicalTransformHandler geologicalTransformHandler;
        private GeologicalTransform geologicalTransform;
        private GeologicalTransform originalTransform;
        private int transformIndex;

        private float strike, dip, plunge, amplitude, period, offset;

        private List<UI_TransformSelectionItem> transformSelectionItems;
        private List<UI_ColourSelectionItem> colour1SelectionItems;
        private List<UI_ColourSelectionItem> colour2SelectionItems;

        private int selectedColourIndex1;
        private int selectedColourIndex2;

        public void SetUpEditMenu(GeologicalTransform geologicalTransform, bool addingNewTransform)
        {

            this.addingNewTransform = addingNewTransform;
            this.geologicalTransform = geologicalTransform;
            originalTransform = geologicalTransform.Clone();

            geologicalTransformHandler = GeologySimulation.GeologicalTransformHandler;
            transformIndex = geologicalTransformHandler.GetTransformIndex(geologicalTransform);

            GeologicalTransformParameters parameters = geologicalTransform.GetGeologicalTransformParameters(true);
            strike = parameters.Strike;
            dip = parameters.Dip;
            plunge = parameters.Plunge;

            amplitude = parameters.Amplitude;
            period = parameters.Period;
            offset = parameters.Offset;

            selectedColourIndex1 = parameters.ColourIndex1;
            selectedColourIndex2 = parameters.ColourIndex2;

            UI_TransformTitle.text = geologicalTransform.GetTransformName();
            UI_TransformImage.texture = geologicalTransform.GetIconTexture();

            UI_StrikeDial.SetDialRotation(strike, false);
            UI_DipDial.SetDialRotation(dip, false);
            UI_PlungeDial.SetDialRotation(plunge, false);

            SetUpTransformSelectionItems();
            SetUpColour1SelectionItems();
            SetUpColour2SelectionItems();
            OpenExtraOptions();
        }

        private void SetUpTransformSelectionItems()
        {
            if (transformSelectionItems != null)
            {
                for (int i = transformSelectionItems.Count - 1; i >= 0; i--)
                {
                    Destroy(transformSelectionItems[i]);
                    Destroy(transformSelectionItems[i].gameObject);
                }
                transformSelectionItems.Clear();
            }

            transformSelectionItems = new List<UI_TransformSelectionItem>();

            for (int i = 0; i < GeologicalTransform.TOTAL_TRANSFORM_TYPES; i++)
            {
                UI_TransformSelectionItem selectionItem = Instantiate(UI_TransformSelectionItemPrefab);

                selectionItem.InitialiseItem((GeologicalTransform.TransformType)i, (int)geologicalTransform.Type == i);

                selectionItem.gameObject.transform.SetParent(UI_TransformSelectionArea.transform);
                selectionItem.gameObject.transform.localScale = Vector3.one;
                selectionItem.SetSelectFunction(Action_SelectTransform);

                transformSelectionItems.Add(selectionItem);
            }

            UI_TransformScrollRect.horizontalNormalizedPosition = (float)geologicalTransform.Type / GeologicalTransform.TOTAL_TRANSFORM_TYPES;
        }

        private void SetUpColour1SelectionItems()
        {
            if (colour1SelectionItems != null)
            {
                for (int i = colour1SelectionItems.Count - 1; i >= 0; i--)
                {
                    Destroy(colour1SelectionItems[i]);
                    Destroy(colour1SelectionItems[i].gameObject);
                }
                colour1SelectionItems.Clear();
            }

            colour1SelectionItems = new List<UI_ColourSelectionItem>();

            for (int i = 0; i < TransformColours.ColourList.Length; i++)
            {
                UI_ColourSelectionItem selectionItem = Instantiate(UI_ColourSelectionItemPrefab);

                selectionItem.InitialiseItem(i, selectedColourIndex1 == i);

                selectionItem.gameObject.transform.SetParent(UI_Colour1SelectionArea.transform);
                selectionItem.gameObject.transform.localScale = Vector3.one;
                selectionItem.SetSelectFunction(Action_SelectColour1);

                colour1SelectionItems.Add(selectionItem);
            }
            // -4.5f is the amount of visible blocks. That is, the amount we need to take off from scrolling at the end.
            UI_Colour1ScrollRect.horizontalNormalizedPosition = Mathf.Clamp((float)(selectedColourIndex1 - 1.8f) / (TransformColours.ColourList.Length - 4.5f), 0, 1);
        }

        private void SetUpColour2SelectionItems()
        {
            if (colour2SelectionItems != null)
            {
                for (int i = colour2SelectionItems.Count - 1; i >= 0; i--)
                {
                    Destroy(colour2SelectionItems[i]);
                    Destroy(colour2SelectionItems[i].gameObject);
                }
                colour2SelectionItems.Clear();
            }

            colour2SelectionItems = new List<UI_ColourSelectionItem>();

            for (int i = 0; i < TransformColours.ColourList.Length; i++)
            {
                UI_ColourSelectionItem selectionItem = Instantiate(UI_ColourSelectionItemPrefab);

                selectionItem.InitialiseItem(i, selectedColourIndex2 == i);

                selectionItem.gameObject.transform.SetParent(UI_Colour2SelectionArea.transform);
                selectionItem.gameObject.transform.localScale = Vector3.one;
                selectionItem.SetSelectFunction(Action_SelectColour2);

                colour2SelectionItems.Add(selectionItem);
            }
            // -4.5f is the amount of visible blocks. That is, the amount we need to take off from scrolling at the end.
            UI_Colour2ScrollRect.horizontalNormalizedPosition = Mathf.Clamp((float)(selectedColourIndex2 - 1.8f) / (TransformColours.ColourList.Length - 4.5f), 0, 1);
        }

        private GeologicalTransform CreateNewTransform(GeologicalTransform.TransformType type)
        {
            switch (type)
            {
                case GeologicalTransform.TransformType.TiltTransform:
                    dip = 0;
                    UI_DipDial.SetDialRotation(dip, false);

                    return new TiltTransform(strike, dip, plunge, Vector3.zero, true);
                    
                case GeologicalTransform.TransformType.FoldTransform:
                    amplitude = 25;
                    period = 50;
                    offset = 0;

                    dip = 90;
                    UI_DipDial.SetDialRotation(dip, false);

                    colour1SelectionItems[selectedColourIndex1].SetSelected(false);
                    selectedColourIndex1 = 0;
                    colour1SelectionItems[selectedColourIndex1].SetSelected(true);

                    colour2SelectionItems[selectedColourIndex2].SetSelected(false);
                    selectedColourIndex2 = 0;
                    colour2SelectionItems[selectedColourIndex2].SetSelected(true);

                    UI_Colour1ScrollRect.horizontalNormalizedPosition = Mathf.Clamp((float)(selectedColourIndex1 - 1.8f) / (TransformColours.ColourList.Length - 4.5f), 0, 1);
                    UI_Colour2ScrollRect.horizontalNormalizedPosition = Mathf.Clamp((float)(selectedColourIndex2 - 1.8f) / (TransformColours.ColourList.Length - 4.5f), 0, 1);

                    return new FoldTransform(strike, dip, plunge, amplitude, period, offset, selectedColourIndex1, selectedColourIndex2, Vector3.zero, true);

                case GeologicalTransform.TransformType.FaultTransform:
                    amplitude = 25;
                    offset = 0;

                    dip = 90;
                    UI_DipDial.SetDialRotation(dip, false);

                    colour1SelectionItems[selectedColourIndex1].SetSelected(false);
                    selectedColourIndex1 = geologicalTransformHandler.GetTotalFaultTransforms() + 1;
                    colour1SelectionItems[selectedColourIndex1].SetSelected(true);

                    UI_Colour1ScrollRect.horizontalNormalizedPosition = Mathf.Clamp((float)(selectedColourIndex1 - 1.8f) / (TransformColours.ColourList.Length - 4.5f), 0, 1);

                    return new FaultTransform(strike, dip, plunge, amplitude, offset, selectedColourIndex1, Vector3.zero, true);
            }
            print("Warning: Unknown transform!");
            return null;
        }

        private void UpdateTransform()
        {
            switch (geologicalTransform.Type)
            {
                case GeologicalTransform.TransformType.TiltTransform:
                    TiltTransform tiltTransform = (TiltTransform)geologicalTransform;
                    tiltTransform.ChangeRotationParamaters(strike, dip, plunge, true);
                    break;

                case GeologicalTransform.TransformType.FoldTransform:
                    FoldTransform foldTransform = (FoldTransform)geologicalTransform;
                    foldTransform.ChangeRotationParameters(strike, dip, plunge, true);
                    foldTransform.ChangeFoldParameters(amplitude, period, offset, selectedColourIndex1, selectedColourIndex2);
                    break;

                case GeologicalTransform.TransformType.FaultTransform:
                    FaultTransform faultTransform = (FaultTransform)geologicalTransform;
                    faultTransform.ChangeRotationParameters(strike, dip, plunge, true);
                    faultTransform.ChangeFaultParameters(amplitude, offset, selectedColourIndex1);
                    break;
            }
        }

        private void OpenExtraOptions()
        {
            switch (geologicalTransform.Type)
            {
                case GeologicalTransform.TransformType.TiltTransform:
                    UI_AmplitudeTitle.SetActive(false);
                    UI_AmplitudeSlider.SetActive(false);
                    UI_PeriodTitle.SetActive(false);
                    UI_PeriodSlider.SetActive(false);
                    UI_OffsetTitle.SetActive(false);
                    UI_OffsetSlider.SetActive(false);
                    UI_ColourSelectionTitle1.SetActive(false);
                    UI_Colour1ScrollRect.gameObject.SetActive(false);
                    UI_ColourSelectionTitle2.SetActive(false);
                    UI_Colour2ScrollRect.gameObject.SetActive(false);

                    UI_PlungeRakeTitle.text = "Plunge";
                    break;

                case GeologicalTransform.TransformType.FoldTransform:
                    UI_AmplitudeTitle.SetActive(true);
                    UI_AmplitudeTitle.GetComponentInChildren<Text>().text = "Amplitude";
                    UI_AmplitudeTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(108.5f, 25);
                    UI_AmplitudeSlider.SetActive(true);
                    UI_PeriodTitle.SetActive(true);
                    UI_PeriodSlider.SetActive(true);
                    UI_OffsetTitle.SetActive(true);
                    UI_OffsetTitle.GetComponentInChildren<Text>().text = "Offset";
                    UI_OffsetTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(79.21f, 25);
                    UI_OffsetSlider.SetActive(true);
                    UI_ColourSelectionTitle1.SetActive(true);
                    UI_ColourSelectionTitle1.GetComponent<RectTransform>().sizeDelta = new Vector2(215.5f, 25);
                    UI_ColourSelectionTitle1.GetComponentInChildren<Text>().text = "Syncline Hinge Colour";
                    UI_Colour1ScrollRect.gameObject.SetActive(true);
                    UI_ColourSelectionTitle2.SetActive(true);
                    UI_ColourSelectionTitle2.GetComponentInChildren<Text>().text = "Anticline Hinge Colour";
                    UI_Colour2ScrollRect.gameObject.SetActive(true);

                    UI_AmplitudeSlider.GetComponentInChildren<Slider>().value = amplitude * 2.0f;
                    UI_PeriodSlider.GetComponentInChildren<Slider>().value = period * 2.0f;
                    UI_OffsetSlider.GetComponentInChildren<Slider>().value = offset * 2.0f;

                    UI_PlungeRakeTitle.text = "Plunge";
                    break;

                case GeologicalTransform.TransformType.FaultTransform:
                    UI_AmplitudeTitle.SetActive(true);
                    UI_AmplitudeTitle.GetComponentInChildren<Text>().text = "Slip";
                    UI_AmplitudeTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(53, 25);
                    UI_AmplitudeSlider.SetActive(true);
                    UI_PeriodTitle.SetActive(false);
                    UI_PeriodSlider.SetActive(false);
                    UI_OffsetTitle.SetActive(true);
                    UI_OffsetTitle.GetComponentInChildren<Text>().text = "Position";
                    UI_OffsetTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 25);
                    UI_OffsetSlider.SetActive(true);
                    UI_ColourSelectionTitle1.SetActive(true);
                    UI_ColourSelectionTitle1.GetComponent<RectTransform>().sizeDelta = new Vector2(141f, 25);
                    UI_ColourSelectionTitle1.GetComponentInChildren<Text>().text = "Fault Colour";
                    UI_Colour1ScrollRect.gameObject.SetActive(true);
                    UI_ColourSelectionTitle2.SetActive(false);
                    UI_Colour2ScrollRect.gameObject.SetActive(false);

                    UI_AmplitudeSlider.GetComponentInChildren<Slider>().value = amplitude * 2.0f;
                    UI_OffsetSlider.GetComponentInChildren<Slider>().value = offset * 2.0f;

                    UI_PlungeRakeTitle.text = "Rake";
                    break;
            }
        }

        private void Action_SelectTransform(GeologicalTransform.TransformType type)
        {
            if (type != geologicalTransform.Type) {
                GeologicalTransform newTransform = CreateNewTransform(type);
                geologicalTransformHandler.ReplaceTransform(newTransform, transformIndex);
                geologicalTransform = newTransform;

                UI_TransformTitle.text = geologicalTransform.GetTransformName();
                UI_TransformImage.texture = geologicalTransform.GetIconTexture();

                for (int i = 0; i < transformSelectionItems.Count; i++)
                {
                    transformSelectionItems[i].SetSelected(i == (int)type);
                }

                OpenExtraOptions();
            }
        }

        private void Action_SelectColour1(int colourIndex)
        {
            colour1SelectionItems[selectedColourIndex1].SetSelected(false);
            selectedColourIndex1 = colourIndex;
            colour1SelectionItems[selectedColourIndex1].SetSelected(true);
            UpdateTransform();
        }

        private void Action_SelectColour2(int colourIndex)
        {
            colour2SelectionItems[selectedColourIndex2].SetSelected(false);
            selectedColourIndex2 = colourIndex;
            colour2SelectionItems[selectedColourIndex2].SetSelected(true);
            UpdateTransform();
        }

        public void UI_ChangeStrike(float strike)
        {
            this.strike = strike;
            UpdateTransform();
        }
        public void UI_ChangeDip(float dip)
        {
            this.dip = dip;
            UpdateTransform();
        }
        public void UI_ChangePlunge(float plunge)
        {
            this.plunge = plunge;
            UpdateTransform();
        }
        public void UI_ChangeAmplitude(float amplitude)
        {
            this.amplitude = amplitude / 2.0f;
            UpdateTransform();
        }
        public void UI_ChangePeriod(float period)
        {
            this.period = period / 2.0f;
            UpdateTransform();
        }
        public void UI_ChangeOffset(float offset)
        {
            this.offset = offset / 2.0f;
            UpdateTransform();
        }
        public void UI_DeleteTransform()
        {
            geologicalTransformHandler.RemoveTransform(transformIndex);
            UI_ColorTransformMenuHandler.OpenTransformMenu();
        }

        public void UI_AcceptEdit()
        {
            UI_ColorTransformMenuHandler.OpenTransformMenu();
        }

        public void UI_CancelEdit()
        {
            if (addingNewTransform)
            {
                geologicalTransformHandler.RemoveTransform(transformIndex);
            }
            else {
                geologicalTransformHandler.ReplaceTransform(originalTransform, transformIndex);
            }
            UI_ColorTransformMenuHandler.OpenTransformMenu();
        }
    }
}
