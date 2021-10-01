//  
//  UI_ConfirmationPanel.cs
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

using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace ARSandbox
{
    public class UI_ConfirmationPanel : MonoBehaviour
    {
        public CalibrationManager CalibrationManager;
        public SharingManager SharingManager;
        public UI_SharingMenu UI_SharingMenu;
        public Image UI_InfoTextBG;
        public Button AcceptButton;
        public Button CancelButton;
        public Text AcceptButtonText;
        public Text CancelButtonText;
        public Text ConfirmationText;
        public TopographyBuilder.UI_TopographyBuilderMenu UI_TopographyBuilderMenu;
        public GeologySimulation.UI_GeologyFileMenu UI_GeologyFileMenu;

        public const string CANCEL_TEXT = "Are you sure you want to cancel this current calibration? \n (The previous calibration will be used)";
        public const string TOPOGRAPHY_DELETE_TEXT = "Are you sure you want to delete\n this saved topography?\n (This action cannot be undone)";
        public const string GEOLOGY_DELETE_TEXT = "Are you sure you want to delete the geology file: \"{0}\"?\n (This action cannot be undone)";
        public const string GEOLOGY_OPEN_TEXT = "Are you sure you want to open the geology file: \"{0}\"?\n (All unsaved progress will be lost)";
        public const string GEOLOGY_REPLACE_FILE = "Are you sure you want to save over the geology file: \"{0}\"?";
        public const string EMAIL_SEND_TEXT = "Confirm sending capture: \n\"{0}\"\nTo: {1}";
        public const string DESKTOP_SAVE_TEXT = "Confirm saving capture:\n\"{0}\"\nto desktop?\n(See folder \"/{1}\")";

        private SmtpClient smtpClient;
        private bool emailCancelled;

        public void SetUpCalibrationCancelPanel()
        {
            CancelButton.onClick.RemoveAllListeners();
            CancelButton.onClick.AddListener(ClosePanel);
            CancelButton.gameObject.SetActive(true);

            AcceptButton.GetComponent<Image>().color = new Color(255.0f / 255.0f, 70 / 255.0f, 0);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(CalibrationManager.UI_CancelCalibration);
            AcceptButton.interactable = true;
            AcceptButton.gameObject.SetActive(true);

            CancelButtonText.text = "Back To Calibration";
            AcceptButtonText.text = "Cancel Calibration";

            UI_InfoTextBG.color = new Color(1, 1, 1);
            ConfirmationText.text = CANCEL_TEXT;

            CalibrationManager.OnCalibration += OnCalibration;
            CalibrationManager.OnCalibrationComplete += OnCalibrationComplete;
        }

        public void SetUpTopographyDeletePanel()
        {
            CancelButton.onClick.RemoveAllListeners();
            CancelButton.onClick.AddListener(ClosePanel);
            CancelButton.gameObject.SetActive(true);

            AcceptButton.GetComponent<Image>().color = new Color(255.0f / 255.0f, 70 / 255.0f, 0);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(ClosePanel);
            AcceptButton.onClick.AddListener(UI_TopographyBuilderMenu.UI_DeleteTopography);
            AcceptButton.interactable = true;
            AcceptButton.gameObject.SetActive(true);

            CancelButtonText.text = "Cancel";
            AcceptButtonText.text = "Delete";

            UI_InfoTextBG.color = new Color(1, 1, 1);
            ConfirmationText.text = TOPOGRAPHY_DELETE_TEXT;
        }

        public void SetUpGeologyFileDeletePanel(string filename)
        {
            CancelButton.onClick.RemoveAllListeners();
            CancelButton.onClick.AddListener(ClosePanel);
            CancelButton.gameObject.SetActive(true);

            AcceptButton.GetComponent<Image>().color = new Color(255.0f / 255.0f, 70 / 255.0f, 0);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(ClosePanel);
            AcceptButton.onClick.AddListener(UI_GeologyFileMenu.Accept_DeleteCurrentGeology);
            AcceptButton.interactable = true;
            AcceptButton.gameObject.SetActive(true);

            CancelButtonText.text = "Cancel";
            AcceptButtonText.text = "Delete";

            UI_InfoTextBG.color = new Color(1, 1, 1);
            ConfirmationText.text = string.Format(GEOLOGY_DELETE_TEXT, filename);
        }

        public void SetUpGeologyFileSelectPanel(string filename)
        {
            CancelButton.onClick.RemoveAllListeners();
            CancelButton.onClick.AddListener(ClosePanel);
            CancelButton.gameObject.SetActive(true);

            AcceptButton.GetComponent<Image>().color = new Color(0, 1, 0);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(ClosePanel);
            AcceptButton.onClick.AddListener(UI_GeologyFileMenu.Accept_SelectGeologyFile);
            AcceptButton.interactable = true;
            AcceptButton.gameObject.SetActive(true);

            CancelButtonText.text = "Cancel";
            AcceptButtonText.text = "Open";

            UI_InfoTextBG.color = new Color(1, 1, 1);
            ConfirmationText.text = string.Format(GEOLOGY_OPEN_TEXT, filename);
        }

        public void SetUpGeologyFileReplacePanel(string filename)
        {
            CancelButton.onClick.RemoveAllListeners();
            CancelButton.onClick.AddListener(ClosePanel);
            CancelButton.gameObject.SetActive(true);

            AcceptButton.GetComponent<Image>().color = new Color(0, 1, 0);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(ClosePanel);
            AcceptButton.onClick.AddListener(UI_GeologyFileMenu.Accept_SaveOverCurrentGeology);
            AcceptButton.interactable = true;
            AcceptButton.gameObject.SetActive(true);

            CancelButtonText.text = "Cancel";
            AcceptButtonText.text = "Save";

            UI_InfoTextBG.color = new Color(1, 1, 1);
            ConfirmationText.text = string.Format(GEOLOGY_REPLACE_FILE, filename);
        }

        public void SetUpConfirmDesktopSavePanel(string captureName)
        {
            CancelButton.onClick.RemoveAllListeners();
            CancelButton.onClick.AddListener(ClosePanel);
            CancelButton.gameObject.SetActive(true);

            AcceptButton.GetComponent<Image>().color = new Color(0 / 255.0f, 255 / 255.0f, 0);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(SaveToDesktop);
            AcceptButton.interactable = true;
            AcceptButton.gameObject.SetActive(true);

            CancelButtonText.text = "Cancel";
            AcceptButtonText.text = "Save";

            UI_InfoTextBG.color = new Color(1, 1, 1);
            ConfirmationText.text = string.Format(DESKTOP_SAVE_TEXT, captureName, SharingManager.DESKTOP_FOLDER);
        }

        private void SaveToDesktop()
        {
            bool saveOutcome = SharingManager.SaveToDesktop();

            CancelButton.gameObject.SetActive(false);

            AcceptButton.GetComponent<Image>().color = new Color(1, 1, 1);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(ClosePanel);
            AcceptButton.gameObject.SetActive(true);
            AcceptButtonText.text = "Close";

            if (saveOutcome)
            {
                UI_InfoTextBG.color = new Color(0, 1, 0);
                ConfirmationText.text = "\nCapture saved!\n";
            } else
            {
                UI_InfoTextBG.color = new Color(1, 100.0f / 255.0f, 100.0f / 255.0f);
                ConfirmationText.text = "Capture couldn't be saved.\nTry starting software in administrator mode.";
            }
        }

        public void SetUpConfirmEmailPanel(string emailAddress, string captureName)
        {
            CancelButton.onClick.RemoveAllListeners();
            CancelButton.onClick.AddListener(ClosePanel);
            CancelButton.gameObject.SetActive(true);

            AcceptButton.GetComponent<Image>().color = new Color(0 / 255.0f, 255 / 255.0f, 0);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(SetUpEmailSendingPanel);
            AcceptButton.interactable = true;
            AcceptButton.gameObject.SetActive(true);

            CancelButtonText.text = "Cancel";
            AcceptButtonText.text = "Send";

            UI_InfoTextBG.color = new Color(1, 1, 1);
            ConfirmationText.text = string.Format(EMAIL_SEND_TEXT, captureName, emailAddress);
        }

        private void SetUpEmailSendingPanel()
        {
            CancelButton.gameObject.SetActive(false);

            AcceptButton.GetComponent<Image>().color = new Color(255.0f / 255.0f, 70 / 255.0f, 0);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(CancelEmail);
            AcceptButton.gameObject.SetActive(true);
            AcceptButtonText.text = "Cancel Sending Email";

            UI_InfoTextBG.color = new Color(1, 1, 1);
            ConfirmationText.text = "\nSending Email   \n";

            smtpClient = SharingManager.SetUpEmailClient();
            smtpClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            SharingManager.SendEmail();

            emailCancelled = false;
            StartCoroutine(TextAnimation());
        }

        private void CancelEmail()
        {
            smtpClient.SendAsyncCancel();
            ConfirmationText.text = "\nSending email cancelled.\n";

            UI_InfoTextBG.color = new Color(1, 1, 1);
            AcceptButton.GetComponent<Image>().color = new Color(1, 1, 1);
            AcceptButton.onClick.RemoveAllListeners();
            AcceptButton.onClick.AddListener(ClosePanel);
            AcceptButtonText.text = "Close";

            StopAllCoroutines();
            emailCancelled = true;
        }
        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Cancelled doesn't work for some reason.
            /*if (e.Cancelled)
            {
                print("Cancelled");
            }*/
            if (!emailCancelled)
            {
                AcceptButton.onClick.RemoveAllListeners();
                if (e.Error != null)
                {
                    UI_InfoTextBG.color = new Color(1, 100.0f / 255.0f, 100.0f / 255.0f);
                    ConfirmationText.text = "Email could not be sent!\nPlease check your internet connection\nand try again.";
                }
                else
                {
                    UI_InfoTextBG.color = new Color(0, 1, 0);
                    ConfirmationText.text = "\nEmail sent!\n";
                    AcceptButton.onClick.AddListener(UI_SharingMenu.UI_BackToCaptureFrameMenu);
                }

                AcceptButton.GetComponent<Image>().color = new Color(1, 1, 1);
                AcceptButton.onClick.AddListener(ClosePanel);
                AcceptButtonText.text = "Close";

                StopAllCoroutines();
            }
        }

        private IEnumerator TextAnimation()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                ConfirmationText.text = "\nSending Email.  \n";
                yield return new WaitForSeconds(0.5f);
                ConfirmationText.text = "\nSending Email.. \n";
                yield return new WaitForSeconds(0.5f);
                ConfirmationText.text = "\nSending Email...\n";
                yield return new WaitForSeconds(0.5f);
                ConfirmationText.text = "\nSending Email   \n";
            }
        }

        private void ClosePanel()
        {
            this.gameObject.SetActive(false);
        }

        private void OnCalibration()
        {
            this.gameObject.SetActive(false);
        }

        private void OnCalibrationComplete()
        {
            this.gameObject.SetActive(false);
        }
    }
}
