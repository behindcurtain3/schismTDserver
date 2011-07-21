using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace schismTD
{
    public class AStar
    {
        public static Path getPath(Cell start, Cell target)
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

                if (currentNode == target)
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

                foreach (Cell neighbor in currentNode.Neighbors)
                {
                    if (neighbor.Passable)
                    {
                        if (!closedList.Contains(neighbor) && !openList.Contains(neighbor))
                        {
                            neighbor.Parent = currentNode;
                            neighbor.G = neighbor.Parent.G + 25;
                            neighbor.H = Math.Abs(neighbor.Position.X - target.Position.X) + Math.Abs(neighbor.Position.Y - target.Position.Y);
                            neighbor.F = neighbor.G + neighbor.H;
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            return empty;
        }
        /*
        public static int getClosestTarget(List<Cell> targets, Point position)
        {
            int smallestDistance = 9999;

            foreach (Cell c in targets)
            {
                int d = Math.Abs(c.Position.X - position.X) + Math.Abs(c.Position.Y - position.Y);
                if (d < smallestDistance)
                {
                    smallestDistance = d;
                }
            }

            return smallestDistance;
        }*/
    }
}
