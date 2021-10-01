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

public class ComputeShaderHelpers
{
    public static Point CalculateThreadsToRun(Point textureSize, Point CS_Threads)
    {
        int xThreadsToRun = (int)Mathf.Ceil((float)(textureSize.x) / (float)(CS_Threads.x));
        int yThreadsToRun = (int)Mathf.Ceil((float)(textureSize.y) / (float)(CS_Threads.y));

        return new Point(xThreadsToRun, yThreadsToRun);
    }
}

public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public override string ToString()
    {
        return "x: " + x.ToString() + " y: " + y.ToString();
    }
    public static Point operator +(Point p1, Point p2)
    {
        return new Point(p1.x + p2.x, p1.y + p2.y);
    }
    public static Point operator -(Point p1, Point p2)
    {
        return new Point(p1.x - p2.x, p1.y - p2.y);
    }
}
