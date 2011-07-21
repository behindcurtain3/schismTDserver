using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class ANode
    {
        // Used in A*
        public ANode Parent;
        public Point Coords; // Coord on map, not pixels
        public int F;
        public int G;
        public int H;

        public List<Cell> Neighbors = new List<Cell>();

        // Is this cell passable by creeps?
        public Boolean Passable;

        public ANode(Point p)
        {
            Coords = p;
        }
    }
}
