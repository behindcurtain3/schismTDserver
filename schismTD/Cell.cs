using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Cell : ANode
    {
        // Index in the list
        public int Index
        {
            get
            {
                return mIndex;
            }
            set
            {
                mIndex = value;
            }
        }
        private int mIndex;

        // A cell belongs to a player
        public Player Player
        {
            get
            {
                return mPlayer;
            }
            set
            {
                mPlayer = value;
            }
        }
        private Player mPlayer;

        // A cell can have a tower
        public Tower Tower
        {
            get
            {
                return mTower;
            }
            set
            {
                mTower = value;
            }
        }
        private Tower mTower;

        // Can anyone build here?
        public Boolean Buildable
        {
            get
            {
                return mBuildable;
            }
            set
            {
                mBuildable = value;
            }
        }
        private Boolean mBuildable;
       

        public Cell(int i, Point c, Point p) : base(c)
        {
            Index = i;
            Width = Settings.BOARD_CELL_WIDTH;
            Height = Settings.BOARD_CELL_HEIGHT;
            Position = new Vector2(p);
            //Center = new Point(Position.X + Settings.BOARD_CELL_WIDTH / 2, Position.Y + Settings.BOARD_CELL_HEIGHT / 2);
            Passable = false;
            Buildable = false;

            Player = null;
            Tower = null;
        }
    }
}
