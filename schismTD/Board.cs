using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class Board
    {
        // Height/width are in cells
        public int Width = Settings.BOARD_WIDTH;
        public int Height = Settings.BOARD_HEIGHT;

        // These are in pixels
        public int CellWidth = Settings.BOARD_CELL_WIDTH;
        public int CellHeight = Settings.BOARD_CELL_HEIGHT;
        public int xOffset = Settings.BOARD_X_OFFSET;
        public int yOffset = Settings.BOARD_Y_OFFSET;

        public List<Cell> Cells = new List<Cell>();
        private List<int> blackCells;
        private List<int> whiteCells;

        public Board(Player black, Player white)
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Cells.Add(new Cell(getIndex(j, i), new Point(j * CellWidth + xOffset, i * CellHeight + yOffset)));
                }
            }

            // certain cells are not passable by default since they aren't in the playable area
            setupLists();
            setupCells(black, white);

        }

        public int getIndex(int x, int y)
        {
            return y * Width + x;
        }

        private void setupCells(Player black, Player white)
        {
            if (Cells == null)
                return;

            foreach (Cell c in Cells)
            {
                if (blackCells.Contains(c.Index))
                {
                    c.Passable = true;
                    c.Player = black;
                }

                if (whiteCells.Contains(c.Index))
                {
                    c.Passable = true;
                    c.Player = white;
                }
            }
        }

        private void setupLists()
        {
            // Add black home cells here
            int[] b = new int[] {
                                        050,
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
    239,240,241,242,243,244,245,246,247,248,249,250,
    257,258,259,260,261,262,263,264,265,266,267,
    275,276,277,278,279,280,281,282,283,284,
        294,295,296,297,298,299,300,301,
            313,314,315,316,317,
            };
            blackCells = new List<int>(b);

            // Add white home cells here
            int[] w = new int[] { 
                        06,07,08,09,10,
                  22,23,24,25,26,27,28,29,
               39,40,41,42,43,44,45,46,47,48,
            56,57,58,59,60,61,62,63,64,65,66,
            73,74,75,76,77,78,79,80,81,82,83,84,
            91,92,93,94,95,96,97,98,99,100,101,102,
        108,109,110,111,112,113,114,115,116,117,118,119,
        126,127,128,129,130,131,132,133,134,135,136,
        144,145,146,147,148,149,150,151,
        162,163,164,165,166,167,
        180,181,182,183,184,
        198,199,200,201,
            217,218,219,
            235,236,237,
                254,255,
                    273
            };
            whiteCells = new List<int>(w);
        }

    }
}
