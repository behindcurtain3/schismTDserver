using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace schismTD
{
    public class AStar
    {
        public static Path getPath(Cell start, List<Cell> targets)
        {
            Path empty = new Path();

            List<Cell> openList = new List<Cell>();
            List<Cell> closedList = new List<Cell>();

            Boolean foundTarget = false;
            Cell currentNode = start;

            start.Parent = null;
            openList.Add(start);

            while (!foundTarget)
            {
                // If the openlist has no cells return an empty path
                if (openList.Count == 0)
                    return empty;

                int lowestFScore = 9999;

                foreach (Cell c in openList)
                {
                    if (c.F < lowestFScore)
                    {
                        lowestFScore = c.F;
                        currentNode = c;
                    }
                }

                if (targets.Contains(currentNode))
                {
                    // We found the target node
                    Path path = new Path();

                    foundTarget = true;
                    Cell pathNode = currentNode;
                    while (pathNode.Parent != null)
                    {
                        path.Push(pathNode);
                        pathNode = (Cell)pathNode.Parent;
                    }
                    return path;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (KeyValuePair<Cell, Boolean> neighbor in currentNode.Neighbors)
                {
                    if (neighbor.Value && neighbor.Key.Passable)
                    {
                        if (!closedList.Contains(neighbor.Key) && !openList.Contains(neighbor.Key))
                        {
                            neighbor.Key.Parent = currentNode;
                            neighbor.Key.G = neighbor.Key.Parent.G + getDistance(currentNode, neighbor.Key);
                            neighbor.Key.H = getClosestTarget(targets, neighbor.Key.Center);
                            neighbor.Key.F = neighbor.Key.G + neighbor.Key.H;
                            openList.Add(neighbor.Key);
                        }
                    }
                }
            }

            return empty;
        }

        public static int getDistance(Cell a, Cell b)
        {
            return Math.Abs(a.Position.X - b.Position.X) + Math.Abs(a.Position.Y - b.Position.Y);
        }

        public static int getClosestTarget(List<Cell> targets, Point position)
        {
            int smallestDistance = 9999;

            foreach (Cell c in targets)
            {
                int d = Math.Abs(c.Center.X - position.X) + Math.Abs(c.Center.Y - position.Y);
                if (d < smallestDistance)
                {
                    smallestDistance = d;
                }
            }

            return smallestDistance;
        }
    }
}
