using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class AStar
    {
        public static List<Cell> getPath(Cell start, Cell target)
        {
            List<Cell> path = new List<Cell>();

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
                    return new List<Cell>();

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
                    foundTarget = true;
                    Cell pathNode = currentNode;
                    while (pathNode.Parent != null)
                    {
                        path.Add(pathNode);
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
                            neighbor.H = Math.Abs(neighbor.Position.X - target.Position.X) + Math.Abs(neighbor.Position.Y + target.Position.Y);
                            neighbor.F = neighbor.G + neighbor.H;
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            return new List<Cell>();
        }
    }
}
