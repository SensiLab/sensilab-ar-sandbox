//  
//  SharingManager.cs
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
using System.Net;
using System.Net.Mime;
using System.Net.Mail;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Text;

namespace ARSandbox
{
    public class MatlabCode
    {
        public const string MatlabString =
            "% ----- ARSandbox data and texture sample code -----\n"+
            "close all;\n" +
            "clc;\n" +
            "\n" +
            "% File name\n" +
            "filename = '{0}';\n" +
            "% Data title\n" +
            "dataTitle = '{1}';\n" +
            "% Depth file name\n" +
            "depthFilename = [filename, '.csv'];\n" +
            "% Texture file name\n" +
            "imageFilename = [filename, '.png'];\n" +
            "\n" +
            "% Load the depth data csv file\n" +
            "depthData = csvread(depthFilename);\n" +
            "% Flip and downsample the depth data.\n" +
            "downsampledDepthData = -depthData(1:2:end,1:2:end);\n" +
            "\n" +
            "% Load the texture image\n" +
            "topographyImage = imread(imageFilename);\n" +
            "% Flip the texture data, (its read in backwards by imread).\n" +
            "topographyImage = flip(topographyImage, 1);\n" +
            "\n" +
            "% Draw depth data with contours underneath.\n" +
            "figure('name', 'Depth Data (mm)');\n" +
            "surfc(downsampledDepthData,'linewidth',0.1);\n" +
            "title({{dataTitle, 'Depth Data (mm)'}});\n" +
            "% Fixing aspect ratio of depth data.\n" +
            "daspect([1 1 7])\n" +
            "set(gca,'BoxStyle','full','Box','on')\n" +
            "\n" +
            "% Draw depth data with contours underneath and texture\n" +
            "figure('name', 'Depth Data (mm) Texturised');\n" +
            "surfc(downsampledDepthData, 'CData', topographyImage,...\n" +
            "        'FaceColor','texturemap','edgecolor','none');\n" +
            "title({{dataTitle, 'Depth Data (mm) Texturised'}});\n" +
            "% Fixing aspect ratio of depth data.\n" +
            "daspect([1 1 7])\n" +
            "set(gca,'BoxStyle','full','Box','on')";
    }
    public class SharingManager : MonoBehaviour
    {
        // https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
        public const string EmailPattern =
        @"^\s*[\w\-\+_']+(\.[\w\-\+_']+)*\@[A-Za-z0-9]([\w\.-]*[A-Za-z0-9])?\.[A-Za-z][A-Za-z\.]*[A-Za-z]$";
        public const string DESKTOP_FOLDER = "ARSandboxData";
        private const int screenshotHeight = 1080;

        public Sandbox Sandbox;
        public CalibrationManager CalibrationManager;
        public SandboxCamera ScreenshotCamera;
        public TestPlane TestPlane;
        public Texture2D CapturedFrame { get; private set; }
        public Texture2D CapturedFrameBW { get; private set; }
        public bool CapturedFrameReady { get; private set; }

        private SandboxDescriptor sandboxDescriptor;
        private int[] processedDepthsArray;
        private string csvString;
        private bool csvReady;
        private static readonly MailAddress sandboxEmailAddress 
                                    = new MailAddress("monasharsandbox@gmail.com", "Monash AR Sandbox");
        private const string sandboxPassword = "MonashARSandbox321!";
        
        private string emailAddress;
        private string captureName;
        private SmtpClient smtpClient;
        private bool emailClientReady, readyToShare;
        private bool sendCSV, csvGenerated, sendBWImage;
        private Regex EmailRegex;

        private void Start()
        {
            EmailRegex = new Regex(EmailPattern, RegexOptions.IgnoreCase);
            ScreenshotCamera.gameObject.GetComponent<SandboxCamera>().InitialiseSandboxCamera();
            ScreenshotCamera.gameObject.SetActive(false);
        }
        public void CaptureData()
        {
            sandboxDescriptor = Sandbox.GetSandboxDescriptor();

            if (CapturedFrameReady)
            {
                Destroy(CapturedFrame);
                Destroy(CapturedFrameBW);
            } else
            {
                CapturedFrameReady = true;
            }

            CapturedFrame = TakeScreenshot(SandboxRenderMaterial.Normal);
            CapturedFrameBW = TakeScreenshot(SandboxRenderMaterial.BlackAndWhite);

            processedDepthsArray = Sandbox.GetProcessedDepthsArray();

            csvGenerated = false;
        }
        private void GenerateCSV()
        {
            if (!csvGenerated)
            {
                int dataWidth = sandboxDescriptor.DataSize.x;
                int dataHeight = sandboxDescriptor.DataSize.y;
                csvString = "";

                for (int y = 0; y < dataHeight; y++)
                {
                    string rowString = "";
                    for (int x = 0; x < dataWidth; x++)
                    {
                        int bufferIndex = x + y * dataWidth;
                        string dataStr = processedDepthsArray[bufferIndex].ToString();
                        if (x < dataWidth - 1)
                        {
                            rowString += dataStr + ",";
                        }
                        else
                        {
                            rowString += dataStr + "\n";
                        }
                    }
                    csvString += rowString;
                }
            }
            csvGenerated = true;
        }
        private Texture2D TakeScreenshot(SandboxRenderMaterial renderMaterial)
        {
            float aspectRatio = sandboxDescriptor.DataSize.x / (float)sandboxDescriptor.DataSize.y;
            int screenshotWidth = (int)(aspectRatio * screenshotHeight);
            RenderTexture screenshotRT = new RenderTexture(screenshotWidth, screenshotHeight, 24, 
                                                                        RenderTextureFormat.ARGB32);
            ScreenshotCamera.gameObject.SetActive(true);
            
            CalibrationManager.SetUpUICamera(ScreenshotCamera.Camera);
            ScreenshotCamera.Camera.targetTexture = screenshotRT;

            ScreenshotCamera.SetSandboxRenderMaterial(renderMaterial);
            ScreenshotCamera.Camera.Render();

            RenderTexture.active = screenshotRT;

            Texture2D frameTexture = new Texture2D(screenshotWidth, screenshotHeight, 
                                                                TextureFormat.ARGB32, false);
            frameTexture.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
            frameTexture.Apply();
            frameTexture.wrapMode = TextureWrapMode.Clamp;

            RenderTexture.active = null;

            ScreenshotCamera.Camera.targetTexture = null;
            ScreenshotCamera.gameObject.SetActive(false);
            Destroy(screenshotRT);

            return frameTexture;
        }

        public void SetUpShareCapture(string emailAddress, string captureName, bool sendCSV, bool sendBWImage)
        {
            emailClientReady = false;

            this.emailAddress = emailAddress;
            this.captureName = captureName;

            readyToShare = true;

            this.sendCSV = sendCSV;
            if (sendCSV)
                GenerateCSV();

            this.sendBWImage = sendBWImage;
        }
        public bool SaveToDesktop()
        {
            if (readyToShare && CapturedFrameReady)
            {
                // Create folder on users desktop.
                string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), DESKTOP_FOLDER);

                if (!Directory.Exists(desktopPath))
                {
                    try {
                        Directory.CreateDirectory(desktopPath);
                    } catch
                    {
                        print(string.Format("Error: Couldn't create directory: {0}", desktopPath));
                        return false;
                    }
                }

                // Can safely assume the desktop folder exists.
                string dataFolderName = captureName.Replace(" ", string.Empty);
                string dataPath = Path.Combine(desktopPath, dataFolderName);
                if (!Directory.Exists(dataPath))
                {
                    try
                    {
                        Directory.CreateDirectory(dataPath);
                    }
                    catch
                    {
                        print(string.Format("Error: Couldn't create directory: {0}", dataPath));
                        return false;
                    }
                }

                // Can now safely assume the data folder exists.
                // Create the image files.
                // Normal image, always included.
                string capturedFramePath = Path.Combine(dataPath, dataFolderName + ".png");
                try
                {
                    using (FileStream fs = new FileStream(capturedFramePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        byte[] capturedFrameBytes = CapturedFrame.EncodeToPNG();
                        MemoryStream capturedFrameMS = new MemoryStream(capturedFrameBytes);
                        capturedFrameMS.WriteTo(fs);
                    }
                } catch (Exception e)
                {
                    print(e.ToString());
                    print(string.Format("Error: Couldn't save file: {0}", capturedFramePath));
                    return false;
                }

                // Black and white image
                if (sendBWImage)
                {
                    string capturedFrameBWPath = Path.Combine(dataPath, dataFolderName + "BW.png");
                    try
                    {
                        using (FileStream fs = new FileStream(capturedFrameBWPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            byte[] capturedFrameBWBytes = CapturedFrameBW.EncodeToPNG();
                            MemoryStream capturedFrameBW_MS = new MemoryStream(capturedFrameBWBytes);
                            capturedFrameBW_MS.WriteTo(fs);
                        }
                    }
                    catch (Exception e)
                    {
                        print(e.ToString());
                        print(string.Format("Error: Couldn't save file: {0}", capturedFrameBWPath));
                        return false;
                    }
                }

                // Black and white image
                if (sendCSV)
                {
                    string capturedFrameCSVPath = Path.Combine(dataPath, dataFolderName + ".csv");
                    try
                    {
                        using (FileStream fs = new FileStream(capturedFrameCSVPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            byte[] csvBuffer = Encoding.ASCII.GetBytes(csvString);
                            MemoryStream capturedFrameCSV_MS = new MemoryStream(csvBuffer);
                            capturedFrameCSV_MS.WriteTo(fs);
                        }
                    }
                    catch (Exception e)
                    {
                        print(e.ToString());
                        print(string.Format("Error: Couldn't save file: {0}", capturedFrameCSVPath));
                        return false;
                    }

                    string capturedFrameMatlabPath = Path.Combine(dataPath, dataFolderName + ".m");
                    try
                    {
                        using (FileStream fs = new FileStream(capturedFrameMatlabPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            byte[] mBuffer = Encoding.ASCII.GetBytes(string.Format(MatlabCode.MatlabString, dataFolderName, captureName));
                            MemoryStream capturedFrameM_MS = new MemoryStream(mBuffer);
                            capturedFrameM_MS.WriteTo(fs);
                        }
                    }
                    catch (Exception e)
                    {
                        print(e.ToString());
                        print(string.Format("Error: Couldn't save file: {0}", capturedFrameMatlabPath));
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        public SmtpClient SetUpEmailClient()
        {
            if (emailClientReady) smtpClient = null;

            smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = (ICredentialsByHost)new NetworkCredential(sandboxEmailAddress.Address, sandboxPassword),
                Timeout = 20000
            };

            emailClientReady = true;
            return smtpClient;
        }
        public void SendEmail()
        {
            if (readyToShare && emailClientReady && CapturedFrameReady)
            {
                MailMessage mailMessage = new MailMessage();

                mailMessage.From = sandboxEmailAddress;
                mailMessage.To.Add(emailAddress);
                mailMessage.Subject = "AR Sandbox Captured Frame";

                AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(captureName, null, "text/plain");
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString("Captured frame \"" + captureName + "\" generated and sent by the Monash AR Sandbox.", null, "text/html");

                string filename = captureName.Replace(" ", string.Empty);

                byte[] capturedFrameBytes = CapturedFrame.EncodeToPNG();
                LinkedResource imageResource = new LinkedResource(new MemoryStream(capturedFrameBytes), "image/png");
                imageResource.ContentType.Name = filename + ".png";
                htmlView.LinkedResources.Add(imageResource);

                if (sendBWImage)
                {
                    byte[] capturedFrameBWBytes = CapturedFrameBW.EncodeToPNG();
                    LinkedResource imageResourceBW = new LinkedResource(new MemoryStream(capturedFrameBWBytes), "image/png");
                    imageResourceBW.ContentType.Name = filename + "BW" + ".png";
                    htmlView.LinkedResources.Add(imageResourceBW);
                }

                if (sendCSV)
                {
                    LinkedResource csvResource = LinkedResource.CreateLinkedResourceFromString(csvString);
                    csvResource.ContentType.Name = filename + ".csv";
                    csvResource.ContentType.MediaType = "text/csv";
                    htmlView.LinkedResources.Add(csvResource);

                    LinkedResource matlabResource = LinkedResource.CreateLinkedResourceFromString(
                                                string.Format(MatlabCode.MatlabString, filename, captureName));
                    matlabResource.ContentType.Name = filename + ".m";
                    matlabResource.ContentType.MediaType = "text/plain";
                    htmlView.LinkedResources.Add(matlabResource);
                }

                //Add two views to message.
                mailMessage.AlternateViews.Add(plainTextView);
                mailMessage.AlternateViews.Add(htmlView);

                //Send message
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                smtpClient.SendAsync(mailMessage, "CaptureEmail");

                emailClientReady = false;
            } else
            {
                if (!emailClientReady) print("Error: Cannot Send Email, SMTPClient not initialised");
                else if (!readyToShare) print("Error: Cannot Send Email, no email address to send to");
                else if (!CapturedFrameReady) print("Error: Cannot Send Email, no captured frame to send");
            }
        }
        public bool ValidateEmail(string emailAddress)
        {
            return EmailRegex.IsMatch(emailAddress);
        }
    }
}
