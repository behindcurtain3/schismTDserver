﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Settings
    {
        // Number of cells for height/width
        public const int BOARD_WIDTH = 18;
        public const int BOARD_HEIGHT = 18;

        // Pixel offsets
        public const int BOARD_X_OFFSET = 175;
        public const int BOARD_Y_OFFSET = 75;

        // Cell sizes, in pixels
        public const int BOARD_CELL_WIDTH = 25;
        public const int BOARD_CELL_HEIGHT = 25;

        // Match
        public const int GAMES_PER_MATCH = 3;

        // Game
        public const int DEFAULT_GAME_COUNTDOWN = 5;
        public const int DEFAULT_LIFE = 50;
        public const int DEFAULT_MANA = 250;

        // Towers
        public const long DEFAULT_FIRE_RATE = 500;
        public const int DEFAULT_DAMAGE = 10;
        public const float DEFAULT_RANGE = BOARD_CELL_WIDTH * 3.5f;
    }
}
