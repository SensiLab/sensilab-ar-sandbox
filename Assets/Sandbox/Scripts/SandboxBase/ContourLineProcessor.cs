//  
//  ContourLineProcessor.cs
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
using System;

namespace ARSandbox
{
    public class ContourLineProcessor
    {
        // Contour Pixel Makeup
        // 00000000 | 00000000 | 00000000  | 00PCEDIR
        // Nothing  |  Angle   | ContourNo | P = processed, 
        //                                   C = onContour,
        //                                   E = direction error,
        //                                   DIR = 0 - 7 directions

        private static ContourNodeStorage contourNodeStorage;
        private static ContourNode[] contourNodeGrid;
        private static Point imageSize;
        private static bool negateLoopDetector;
        private static int MaxDepthLevel = 64;
        private static List<ContourLabelProps>[] potentialLabelPositions;
        private static List<ContourCentreLabelProps>[] potentialCentreLabelPositions;
        private static int[] contourPixels;

        private const int DIR_ERROR_OPERATOR = 1 << 3;
        private const int ON_CONTOUR_OPERATOR = 1 << 4;
        private const int PROCESSED_OPERATOR = 1 << 5;
        private const int HAS_PARENT_OPERATOR = 1 << 6;
        private const int STARTING_PIXEL_OPERATOR = 1 << 7;
        private const int CONTOUR_NUMBER_OPERATOR = 255 << 8;
        private const int ANGLE_OPERATOR = 255 << 16;

        private class ContourNode {
            public ContourNode Child;
            public Point GridPosition;
            public Vector2 NormalisedPosition;
            public int ContourDepth;
            public float ContourAngle;
            public bool LoopDetector;
            public bool ContourLabelPlaced;

            public ContourNode(int ContourDepth, int ContourAngle, Point GridPosition,
                                    Vector2 NormalisedPosition, ContourNode Child)
            {
                this.ContourDepth = ContourDepth;
                this.ContourAngle = ContourAngle / 255.0f * 360.0f;
                this.GridPosition = GridPosition;
                this.NormalisedPosition = NormalisedPosition;
                this.Child = Child;
                this.ContourLabelPlaced = false;
            }
        }

        private class ContourNodeStorage
        {
            public List<ContourNode>[] contourRoots;
            

            public ContourNodeStorage()
            {
                contourRoots = new List<ContourNode>[MaxDepthLevel];

                for (int i = 0; i < MaxDepthLevel; i++)
                {
                    contourRoots[i] = new List<ContourNode>();
                }
            }

            public void ClearNodeStorage()
            {
                for (int i = 0; i < MaxDepthLevel; i++)
                {
                    contourRoots[i].Clear();
                }
            }

            public void AddNode(int contourDepth, ContourNode contourNode)
            {
                if (contourDepth >= 0 && contourDepth < MaxDepthLevel)
                {
                    if (!contourRoots[contourDepth].Contains(contourNode))
                    {
                        contourRoots[contourDepth].Add(contourNode);
                    } else
                    {
                        //Debug.Log("Cannot Add: Contour node already added!");
                    }
                } else
                {
                    //Debug.Log("Cannot Add: Contour depth out of range!");
                    //Debug.Log(contourDepth);
                }
            }

            public void RemoveNode(int contourDepth, ContourNode contourNode)
            {
                if (contourDepth >= 0 && contourDepth < MaxDepthLevel)
                {
                    if (contourRoots[contourDepth].Contains(contourNode))
                    {
                        contourRoots[contourDepth].Remove(contourNode);
                        //Debug.Log("Remove Success");
                    }
                    else
                    {
                        //Debug.Log("Cannot Remove: Contour node doesn't exist!");
                    }
                }
                else
                {
                    Debug.Log("Cannot Remove: Contour depth out of range!");
                    Debug.Log(contourDepth);
                }
            }
        }

        //TODO: Clean this up, separate into separate functions.
        public static void ProcessContourPixels(int[] contourPixels, Point imageSize)
        {
            ContourLineProcessor.contourPixels = contourPixels;
            ContourLineProcessor.imageSize = imageSize;
            int totalElements = imageSize.x * imageSize.y;

            if (contourNodeGrid == null || contourNodeGrid.Length != totalElements)
            {
                contourNodeGrid = new ContourNode[totalElements];
            }
            Array.Clear(contourNodeGrid, 0, totalElements);

            if (contourNodeStorage == null)
            {
                contourNodeStorage = new ContourNodeStorage();
            }
            contourNodeStorage.ClearNodeStorage();
            negateLoopDetector = false;

            for (int i = 0; i < contourPixels.Length; i++)
            {
                int contourPixel = contourPixels[i];
                int xPos = i % imageSize.x;
                int yPos = i / imageSize.x;

                bool pixelProcessed = (contourPixel & PROCESSED_OPERATOR) != 0;
                bool onContour = (contourPixel & ON_CONTOUR_OPERATOR) != 0;
                bool validDirection = (contourPixel & DIR_ERROR_OPERATOR) == 0;
                int j;

                if (onContour && validDirection && !pixelProcessed)
                {
                    int rootContourDepth, currContourDepth;
                    rootContourDepth = (contourPixel & CONTOUR_NUMBER_OPERATOR) >> 8;
                    int currAngle = (contourPixel & ANGLE_OPERATOR) >> 16;

                    if (rootContourDepth > MaxDepthLevel)
                    {
                        contourPixels[i] |= PROCESSED_OPERATOR;
                        continue;
                    }

                    ContourNode currContourNode, prevContourNode, rootContourNode;
                    rootContourNode = new ContourNode(rootContourDepth, currAngle, new Point(xPos, yPos),
                        new Vector2(xPos / (float)imageSize.x, yPos / (float)imageSize.y), null);

                    contourNodeStorage.AddNode(rootContourNode.ContourDepth, rootContourNode);

                    prevContourNode = rootContourNode;
                    contourNodeGrid[i] = prevContourNode;

                    contourPixels[i] |= STARTING_PIXEL_OPERATOR;

                    int direction = contourPixel % 8;

                    int currIndex = GetNextPixelPos(i, direction, imageSize);
                    int prevDirection = direction;

                    j = 0;
                    bool hasParent = false;

                    while (currIndex != -1)
                    {
                        j += 1;
                        int currPixel = contourPixels[currIndex];
                        bool currOnContour = (currPixel & ON_CONTOUR_OPERATOR) != 0;
                        bool currValidDirection = (currPixel & DIR_ERROR_OPERATOR) == 0;
                        bool currHasParent = (currPixel & HAS_PARENT_OPERATOR) != 0;
                        bool currStartingPixel = (currPixel & STARTING_PIXEL_OPERATOR) != 0;
                        currContourDepth = (currPixel & CONTOUR_NUMBER_OPERATOR) >> 8;

                        int currXPos = currIndex % imageSize.x;
                        int currYPos = currIndex / imageSize.x;

                        if (currOnContour && currValidDirection && currContourDepth == rootContourDepth)
                        {
                            if (currStartingPixel)
                            {
                                if (currIndex != i)
                                {
                                    if (!currHasParent)
                                    {
                                        contourNodeStorage.RemoveNode(currContourDepth, contourNodeGrid[currIndex]);
                                        prevContourNode.Child = contourNodeGrid[currIndex];

                                        contourPixels[currIndex] |= HAS_PARENT_OPERATOR;
                                        currIndex = -1;
                                    } else
                                    {
                                        // Special case. If the direction is heading down and left we could be cutting off
                                        // a previously left pixel directly below. Check for this.
                                        if (prevDirection == 5 && currXPos < imageSize.x - 1)
                                        {
                                            prevDirection = 6;
                                            currIndex += 1;
                                        }
                                        else
                                        {
                                            hasParent = true;
                                            currIndex = -1;
                                        }
                                    }
                                } else
                                {
                                    if (!currHasParent)
                                    {
                                        prevContourNode.Child = contourNodeGrid[currIndex];
                                        contourPixels[currIndex] |= HAS_PARENT_OPERATOR;
                                    }
                                    currIndex = -1;
                                }
                            }
                            else if (currHasParent)
                            {
                                currIndex = -1;
                                hasParent = true;
                            } else
                            {
                                currAngle = (currPixel & ANGLE_OPERATOR) >> 16;

                                currContourNode = new ContourNode(currContourDepth, currAngle, new Point(currXPos, currYPos),
                                    new Vector2(currXPos / (float)imageSize.x, currYPos / (float)imageSize.y), null);
                                prevContourNode.Child = currContourNode;
                                contourNodeGrid[currIndex] = currContourNode;

                                int currDirection = currPixel % 8;

                                contourPixels[currIndex] |= PROCESSED_OPERATOR;
                                contourPixels[currIndex] |= HAS_PARENT_OPERATOR;

                                prevDirection = currDirection;
                                currIndex = GetNextPixelPos(currIndex, currDirection, imageSize);
                                prevContourNode = currContourNode;
                            }
                        } else
                        {
                            currIndex = -1;
                        }
                    }
                    if (hasParent && j < 2)
                    {
                        contourNodeStorage.RemoveNode(rootContourNode.ContourDepth, rootContourNode);
                    }
                }
                contourPixels[i] |= PROCESSED_OPERATOR;
            }
        }

        public static int GetDepthLevel(Point gridPoint)
        {
            int gridIndex = gridPoint.x + gridPoint.y * imageSize.x;
            int contourPixel = contourPixels[gridIndex];

            return (contourPixel & CONTOUR_NUMBER_OPERATOR) >> 8;
        }

        public static bool ValidateContourLabelProps(ContourLabelProps labelProps, 
                                                        out ContourLabelProps alteredProps)
        {
            int squareHalfSize = 1;

            int minX = labelProps.GridPosition.x - squareHalfSize;
            minX = minX < 0 ? 0 : minX;
            int maxX = labelProps.GridPosition.x + squareHalfSize;
            maxX = maxX >= imageSize.x ? imageSize.x - 1 : maxX;

            int minY = labelProps.GridPosition.y - squareHalfSize;
            minY = minY < 0 ? 0 : minY;
            int maxY = labelProps.GridPosition.y + squareHalfSize;
            maxY = maxY >= imageSize.y ? imageSize.y - 1 : maxY;

            bool validPosition = false;
            Vector2 newPosition = new Vector2();
            int totalValidPositions = 0;

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    int index = y * imageSize.x + x;
                    ContourNode node = contourNodeGrid[index];

                    if (node != null)
                    {
                        if (labelProps.Depth == node.ContourDepth)
                        {
                            totalValidPositions += 1;
                            node.ContourLabelPlaced = true;
                            newPosition += node.NormalisedPosition;

                            validPosition = true;
                        }
                    }
                }
            }

            if (validPosition)
            {
                newPosition /= (float)totalValidPositions;

                alteredProps = new ContourLabelProps();
                alteredProps.Depth = labelProps.Depth;
                alteredProps.NormalisedPosition = newPosition;
                alteredProps.GridPosition = labelProps.GridPosition;
                alteredProps.Rotation = labelProps.Rotation;
            } else
            {
                alteredProps = null;
            }
            return validPosition;
        }

        public static List<ContourLabelProps>[] GetPotentialContourLabelPositions()
        {
            return potentialLabelPositions;
        }
        public static List<ContourCentreLabelProps>[] GetPotentialContourCentreLabelPositions()
        {
            return potentialCentreLabelPositions;
        }

        public static void CalculatePotentialLabelPositions(int spacing)
        {
            if (contourNodeStorage != null)
            {
                potentialLabelPositions = new List<ContourLabelProps>[MaxDepthLevel];
                potentialCentreLabelPositions = new List<ContourCentreLabelProps>[MaxDepthLevel];

                for (int i = 0; i < MaxDepthLevel; i++)
                {
                    potentialLabelPositions[i] = new List<ContourLabelProps>();
                    potentialCentreLabelPositions[i] = new List<ContourCentreLabelProps>();
                    int numContourLines = contourNodeStorage.contourRoots[i].Count;

                    for (int j = 0; j < numContourLines; j++)
                    {
                        ContourNode root = contourNodeStorage.contourRoots[i][j];
                        ContourNode curr = root;

                        int creationCount = 0;
                        bool tempPropsCreated = false;
                        ContourLabelProps tempProps = new ContourLabelProps();

                        ContourCentreLabelProps centreLabelProps = new ContourCentreLabelProps();
                        centreLabelProps.Depth = i;
                        centreLabelProps.Rotation = 0;
                        centreLabelProps.Circular = false;

                        while (curr != null)
                        {
                            if (curr.LoopDetector == !negateLoopDetector)
                            {
                                centreLabelProps.Circular = true;
                                break;
                            }

                            // Calculating the centre of the blob.
                            centreLabelProps.NormalisedPosition += curr.NormalisedPosition;
                            centreLabelProps.ContourLength += 1;
                            centreLabelProps.GridPosition += curr.GridPosition;

                            // Even spacing around the contours.
                            if (curr.ContourLabelPlaced) creationCount = -spacing;
                            if (creationCount == 0)
                            {
                                tempPropsCreated = true;

                                tempProps = new ContourLabelProps();
                                tempProps.Depth = i;
                                tempProps.NormalisedPosition = curr.NormalisedPosition;
                                tempProps.Rotation = curr.ContourAngle;
                            } else if (creationCount > 0 && creationCount <= 4)
                            {
                                tempProps.NormalisedPosition += curr.NormalisedPosition;
                                if (creationCount == 2) tempProps.Rotation = curr.ContourAngle;
                                if (creationCount == 2) tempProps.GridPosition = curr.GridPosition;
                            }
                            if (creationCount == spacing)
                            {
                                creationCount = -1;
                                if (tempPropsCreated)
                                {
                                    tempProps.NormalisedPosition /= 5.0f;
                                    potentialLabelPositions[i].Add(tempProps);
                                }
                                tempPropsCreated = false;
                            }
                            curr.LoopDetector = !negateLoopDetector;
                            curr = curr.Child;

                            creationCount++;
                        }

                        centreLabelProps.NormalisedPosition /= (float)centreLabelProps.ContourLength;
                        centreLabelProps.GridPosition = new Point((int)(centreLabelProps.GridPosition.x / (float)centreLabelProps.ContourLength),
                                                                    (int)(centreLabelProps.GridPosition.y / (float)centreLabelProps.ContourLength));
                        potentialCentreLabelPositions[i].Add(centreLabelProps);
                    }
                }
                negateLoopDetector = !negateLoopDetector;
            }
        }

        private static Color GetContourColour(int direction)
        {
            switch (direction)
            {
                case 0:
                    return new Color(1, 0, 0);
                case 1:
                    return new Color(1, 1, 0);
                case 2:
                    return new Color(0, 1, 0);
                case 3:
                    return new Color(0, 1, 1);
                case 4:
                    return new Color(0, 0, 1);
                case 5:
                    return new Color(1, 0, 1);
                case 6:
                    return new Color(1, 0.5f, 0);
                case 7:
                    return new Color(0, 0.5f, 0.5f);
            }
            return Color.white;
        }

        private static int GetNextPixelPos(int index, int direction, Point imageSize)
        {
            int xPos = index % imageSize.x;

            if (xPos == 0)
            {
                if (direction >= 3 && direction <= 5) return -1;
            }
            if (xPos == imageSize.x - 1)
            {
                if (direction == 0 || direction == 1 || direction == 7) return -1;
            }

            int directionStep = GetDirectionStep(direction, imageSize);
            int newPos = index + directionStep;

            if (newPos < 0) return -1;
            if (newPos >= imageSize.x * imageSize.y) return -1;

            return newPos;
        }
        private static int GetDirectionStep(int direction, Point imageSize)
        {
            switch (direction)
            {
                case 0:
                    return 1;
                case 1:
                    return imageSize.x + 1;
                case 2:
                    return imageSize.x;
                case 3:
                    return imageSize.x - 1;
                case 4:
                    return -1;
                case 5:
                    return -imageSize.x - 1;
                case 6:
                    return -imageSize.x;
                case 7:
                    return -imageSize.x + 1;
            }
            return 0;
        }
    }

    public class ContourLabelProps {
        public Vector2 NormalisedPosition;
        public float Rotation;
        public Point GridPosition;
        public int Depth;
    }

    public class ContourCentreLabelProps : ContourLabelProps
    {
        public int ContourLength;
        public bool Circular;
    }
}