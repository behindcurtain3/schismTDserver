using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Cell
    {
        // Position = coordinates
        public Point Position;

        // Is this cell passable by creeps?
        public Boolean Passable;
        
        // Index in the list
        public int Index;

        // A cell belongs to a player
        public Player Player = null;

        // A cell can have a tower
        public Tower Tower = null;
       

        public Cell(int i, Point p)
        {
            Index = i;
            Position = p;

            Passable = false;
        }
    }
}
