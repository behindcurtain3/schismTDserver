using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Text;

namespace schismTD
{
    public class AStar
    {
        public static Path getPath(Cell start, List<Cell> targets)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Path empty = new Path();

            List<Cell> openList = new List<Cell>();
            List<Cell> closedList = new List<Cell>();

            Boolean foundTarget = false;
            Cell currentNode = start;

            Dictionary<Cell, Cell> parentCells = new Dictionary<Cell, Cell>();
            parentCells.Add(start, null);
            openList.Add(start);

            while (!foundTarget)
            {
                if (sw.ElapsedMilliseconds > 80)
                {
                    Console.WriteLine("Taking too long for: " + start.Index);
                    Console.WriteLine("Openlist: " + openList.Count);
                    Console.WriteLine("Closedlist: " + closedList.Count);
                    return empty;
                }

                // If the openlist has no cells return an empty path
                if (openList.Count == 0)
                    return empty;

                // sort the open list
                openList.RemoveAll(delegate(Cell c) { return c == null; });
                openList.Sort(delegate(Cell c1, Cell c2) { return c1.F.CompareTo(c2.F); });
                
                // grab the first cell w/ lowest f score
                currentNode = openList[0];

                if (targets.Contains(currentNode))
                {
                    // We found the target node
                    Path path = new Path();

                    foundTarget = true;
                    Cell pathNode = currentNode;
                    while(parentCells[pathNode] != null)
                    {
                        path.Push(pathNode);
                        pathNode = parentCells[pathNode];
                    }
                    return path;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                lock (currentNode.Neighbors)
                {
                    foreach (KeyValuePair<Cell, Boolean> neighbor in currentNode.Neighbors)
                    {
                        if (neighbor.Value && neighbor.Key.Passable)
                        {
                            if (!closedList.Contains(neighbor.Key) && !openList.Contains(neighbor.Key))
                            {
                                parentCells.Add(neighbor.Key, currentNode);
                                neighbor.Key.G = parentCells[neighbor.Key].G + getDistance(currentNode, neighbor.Key);
                                neighbor.Key.H = getClosestTarget(targets, neighbor.Key.Center);
                                neighbor.Key.F = neighbor.Key.G + neighbor.Key.H;
                                openList.Add(neighbor.Key);
                            }
                        }
                    }
                }
            }

            return empty;
        }

        public static float getDistance(Cell a, Cell b)
        {
            return Math.Abs(a.Position.X - b.Position.X) + Math.Abs(a.Position.Y - b.Position.Y);
        }

        public static float getClosestTarget(List<Cell> targets, PointF position)
        {
            float smallestDistance = 9999f;

            foreach (Cell c in targets)
            {
                float d = Math.Abs(c.Center.X - position.X) + Math.Abs(c.Center.Y - position.Y);
                if (d < smallestDistance)
                {
                    smallestDistance = d;
                }
            }

            return smallestDistance;
        }
    }
}
