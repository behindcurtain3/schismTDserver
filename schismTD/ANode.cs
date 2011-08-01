using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class ANode
    {
        // Used in A*
        public ANode Parent
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }
        private ANode mParent;

        // Coord on map, not pixels
        public Point Coords
        {
            get
            {
                return mCoords;
            }
        }
        private Point mCoords;

        public int F;
        public int G;
        public int H;

        public Dictionary<Cell, Boolean> Neighbors
        {
            get
            {
                return mNeighbors;
            }
            set
            {
                mNeighbors = value;
            }
        }
        private Dictionary<Cell, Boolean> mNeighbors = new Dictionary<Cell, Boolean>();

        public Cell Up
        {
            get
            {
                return mUp;
            }
            set
            {
                mUp = value;
            }
        }
        private Cell mUp;

        public Cell Down
        {
            get
            {
                return mDown;
            }
            set
            {
                mDown = value;
            }
        }
        private Cell mDown;

        public Cell Left
        {
            get
            {
                return mLeft;
            }
            set
            {
                mLeft = value;
            }
        }
        private Cell mLeft;

        public Cell Right
        {
            get
            {
                return mRight;
            }
            set
            {
                mRight = value;
            }
        }
        private Cell mRight;



        // Is this cell passable by creeps?
        public Boolean Passable
        {
            get
            {
                return mPassable;
            }
            set
            {
                mPassable = value;
            }
        }
        private Boolean mPassable = false;

        public ANode(Point p)
        {
            mCoords = p;
        }
    }
}
