using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Cell : ANode
    {
        // World Position
        public Point Position;
        public Point Center;
        
        // Index in the list
        public int Index;

        // A cell belongs to a player
        public Player Player = null;

        // A cell can have a tower
        public Tower Tower = null;

        // Can anyone build here?
        public Boolean Buildable;
       

        public Cell(int i, Point c, Point p) : base(c)
        {
            Index = i;
            Position = p;
            Center = new Point(Position.X + (int)Math.Ceiling((double)Settings.BOARD_CELL_WIDTH / 2), Position.Y + (int)Math.Ceiling((double)Settings.BOARD_CELL_HEIGHT / 2));
            Passable = false;
            Buildable = false;
        }
    }
}
