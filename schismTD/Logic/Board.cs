using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class Board
    {

        private List<int> blackCellsIndex;
        private List<int> whiteCellsIndex;
        private List<int> blackBases;
        private List<int> whiteBases;

        // Height/width are in cells
        public int Width
        {
            get
            {
                return mWidth;
            }
        }
        private int mWidth = Settings.BOARD_WIDTH;

        public int Height
        {
            get
            {
                return mHeight;
            }
        }
        private int mHeight = Settings.BOARD_HEIGHT;

        // These are in pixels
        public int CellWidth
        {
            get
            {
                return mCellWidth;
            }
        }
        private int mCellWidth = Settings.BOARD_CELL_WIDTH;
        
        public int CellHeight
        {
            get
            {
                return mCellHeight;
            }
        }
        private int mCellHeight = Settings.BOARD_CELL_HEIGHT;

        public int XOffset
        {
            get
            {
                return mXOffset;
            }
        }
        private int mXOffset = Settings.BOARD_X_OFFSET;

        public int YOffset
        {
            get
            {
                return mYOffset;
            }
        }
        private int mYOffset = Settings.BOARD_Y_OFFSET;

        public List<Cell> Cells
        {
            get
            {
                return mCells;
            }
            set
            {
                mCells = value;
            }
        }
        private List<Cell> mCells = new List<Cell>();

        public Cell BlackSpawn
        {
            get
            {
                return mBlackSpawn;
            }
            set
            {
                mBlackSpawn = value;
            }
        }
        private Cell mBlackSpawn = null;

        public Cell WhiteSpawn
        {
            get
            {
                return mWhiteSpawn;
            }
            set
            {
                mWhiteSpawn = value;
            }
        }
        private Cell mWhiteSpawn = null;

        public List<Cell> BlackBase
        {
            get
            {
                return mBlackBase;
            }
            set
            {
                mBlackBase = value;
            }
        }
        private List<Cell> mBlackBase = new List<Cell>();

        public List<Cell> WhiteBase
        {
            get
            {
                return mWhiteBase;
            }
            set
            {
                mWhiteBase = value;
            }
        }
        private List<Cell> mWhiteBase = new List<Cell>();

        public List<Cell> WhiteCells
        {
            get
            {
                return mWhiteCells;
            }
        }
        private List<Cell> mWhiteCells = new List<Cell>();

        public List<Cell> BlackCells
        {
            get
            {
                return mBlackCells;
            }
        }
        private List<Cell> mBlackCells = new List<Cell>();

        public Path BlackPath
        {
            get
            {
                return mBlackPath;
            }
            set
            {
                mBlackPath = value;
            }
        }
        private Path mBlackPath = new Path();

        public Path WhitePath
        {
            get
            {
                return mWhitePath;
            }
            set
            {
                mWhitePath = value;
            }
        }
        private Path mWhitePath = new Path();

        public Board(Player black, Player white)
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Cells.Add(new Cell(getIndex(j, i), new Point(j, i), new Point(j * CellWidth + XOffset, i * CellHeight + YOffset)));
                }
            }

            // certain cells are not passable by default since they aren't in the playable area
            setupLists();
            setupCells(black, white);
            setupPathFinding();
        }

        public Path getBlackPath()
        {
            return (Path)AStar.getPath(BlackSpawn, BlackBase);
        }

        public Path getWhitePath()
        {
            return (Path)AStar.getPath(WhiteSpawn, WhiteBase);
        }

        public void calcPaths()
        {
            lock (BlackPath)
            {
                BlackPath.Clear();
                BlackPath = getBlackPath();
            }
            lock (WhitePath)
            {
                WhitePath.Clear();
                WhitePath = getWhitePath();
            }
        }

        public int getIndex(int x, int y)
        {
            return y * Width + x;
        }

        public Cell findCellByIndex(int i)
        {
            if (i < 0 || i >= Cells.Count)
                return null;

            return Cells[i];
        }

        private void setupPathFinding()
        {
            int UP = -18;
            int UPL = -19;
            int UPR = -17;

            int LEFT = -1;
            int RIGHT = 1;

            int DOWN = 18;
            int DOWNL = 17;
            int DOWNR = 19;

            // First we go through and set all the LEFT, RIGHT, UP & DOWN neighbors, even if they aren't passable or the other team etc
            foreach (Cell c in Cells)
            {
                c.Left = findCellByIndex(c.Index + LEFT);
                c.Right = findCellByIndex(c.Index + RIGHT);
                c.Up = findCellByIndex(c.Index + UP);
                c.Down = findCellByIndex(c.Index + DOWN);
            }

            // For each white/black cell create links between valid neighbors
            foreach (Cell c in Cells)
            {
                if (!c.Passable)
                    continue;

                addNeighborToCell(c, findCellByIndex(c.Index + UP));
                addNeighborToCell(c, findCellByIndex(c.Index + LEFT));
                addNeighborToCell(c, findCellByIndex(c.Index + RIGHT));
                addNeighborToCell(c, findCellByIndex(c.Index + DOWN));
                
            }

            // calculate the diagonal neighbors
            foreach (Cell c in Cells)
            {
                if (!c.Passable)
                    continue;

                // Upper left
                if (c.Up != null && c.Left != null)
                {
                    if(c.Up.Player == c.Player && c.Left.Player == c.Player && c.Up.Passable && c.Left.Passable)
                        addNeighborToCell(c, findCellByIndex(c.Index + UPL));
                }
                if (c.Up != null && c.Right != null)
                {
                    if (c.Up.Player == c.Player && c.Right.Player == c.Player && c.Up.Passable && c.Right.Passable)
                        addNeighborToCell(c, findCellByIndex(c.Index + UPR));
                }
                if (c.Down != null && c.Left != null)
                {
                    if (c.Down.Player == c.Player && c.Left.Player == c.Player && c.Down.Passable && c.Left.Passable)
                        addNeighborToCell(c, findCellByIndex(c.Index + DOWNL));
                }
                if (c.Down != null && c.Right != null)
                {
                    if (c.Down.Player == c.Player && c.Right.Player == c.Player && c.Down.Passable && c.Right.Passable)
                        addNeighborToCell(c, findCellByIndex(c.Index + DOWNR));
                }
            }


            calcPaths();
        }

        private void addNeighborToCell(Cell c, Cell neighbor)
        {
            if (neighbor != null)
            {
                if (neighbor.Player == c.Player && neighbor.Passable)
                {
                    c.Neighbors.Add(neighbor, true);
                }
            }
        }

        private void setupCells(Player black, Player white)
        {
            if (Cells == null)
                return;

            foreach (Cell c in Cells)
            {
                if (blackCellsIndex.Contains(c.Index))
                {
                    c.Passable = true;
                    c.Buildable = true;
                    c.Player = black;

                    BlackCells.Add(c);
                }

                else if (whiteCellsIndex.Contains(c.Index))
                {
                    c.Passable = true;
                    c.Buildable = true;
                    c.Player = white;

                    WhiteCells.Add(c);
                }

                if (c.Index == Settings.DEFAULT_WHITE_SPAWN)
                {
                    c.Passable = true;
                    c.Player = white;
                    WhiteSpawn = c;
                    WhiteCells.Add(c);
                }
                else if (c.Index == Settings.DEFAULT_BLACK_SPAWN)
                {
                    c.Passable = true;
                    c.Player = black;

                    BlackSpawn = c;
                    BlackCells.Add(c);
                }

                if (blackBases.Contains(c.Index))
                {
                    c.Passable = true;
                    c.Player = black;

                    BlackBase.Add(c);
                    BlackCells.Add(c);
                }
                else if (whiteBases.Contains(c.Index))
                {
                    c.Passable = true;
                    c.Player = white;

                    WhiteBase.Add(c);
                    WhiteCells.Add(c);
                }
            }
        }

        private void setupLists()
        {
            int[] bBases = new int[] { 242, 243, 260, 261 };
            blackBases = new List<int>(bBases);
            int[] wBases = new int[] { 62, 63, 80, 81 };
            whiteBases = new List<int>(wBases);

            // Add black home cells here
            int[] b = new int[] {
                                        068,069,
                                        086,087,088,
                                        104,105,106,
                                        122,123,124,125,
                                    139,140,141,142,143,
                                156,157,158,159,160,161,
                        172,173,174,175,176,177,178,179,
            187,188,189,190,191,192,193,194,195,196,197,
        204,205,206,207,208,209,210,211,212,213,214,215,
    221,222,223,224,225,226,227,228,229,230,231,232,
    239,240,241,        244,245,246,247,248,249,250,
    257,258,259,        262,263,264,265,266,267,
    275,276,277,278,279,280,281,282,283,284,
        294,295,296,297,298,299,300,301,
            313,314,315,316,317,
            };
            blackCellsIndex = new List<int>(b);

            // Add white home cells here
            int[] w = new int[] { 
                        06,07,08,09,10,
                  22,23,24,25,26,27,28,29,
               39,40,41,42,43,44,45,46,47,48,
            56,57,58,59,60,61,      64,65,66,
            73,74,75,76,77,78,79,   82,83,84,
            91,92,93,94,95,96,97,98,99,100,101,102,
        108,109,110,111,112,113,114,115,116,117,118,119,
        126,127,128,129,130,131,132,133,134,135,136,
        144,145,146,147,148,149,150,151,
        162,163,164,165,166,167,
        180,181,182,183,184,
        198,199,200,201,
            217,218,219,
            235,236,237,
                254,255
            };
            whiteCellsIndex = new List<int>(w);
        }

    }
}
