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
        public const int BOARD_X_OFFSET = 130;
        public const int BOARD_Y_OFFSET = 30;

        // Cell sizes, in pixels
        public const int BOARD_CELL_WIDTH = 30;
        public const int BOARD_CELL_HEIGHT = 30;

        // Match
        public const int GAMES_PER_MATCH = 1;
        public const int DEFAULT_MATCH_COOLDOWN = 10;

        // Game
        public const int DEFAULT_GAME_COUNTDOWN = 5;
        public const int DEFAULT_NUM_WAVES = 10;
        public const int DEFAULT_LIFE = 20;
        public const int DEFAULT_MANA = 100;

        // Cell index to spawn creeps on
        public const int DEFAULT_BLACK_SPAWN = 050;
        public const int DEFAULT_WHITE_SPAWN = 273;

        // Towers
        public const int DEFAULT_FIRE_RATE = 300;                   // Corresponds to speed rating of 5, so a rating of 1 would equal 1500 (5 times as slow)
        public const int DEFAULT_DAMAGE = 10;                       // Corresponds to dmg rating of 1
        public const float DEFAULT_RANGE = BOARD_CELL_WIDTH * 1.5f; // Corresponds to rating of 1

        // Creeps
        public const int CREEP_LIFE = 50;
        public const int CREEP_SPEED = 50;
        public const int CREEP_WIGGLE = 2;
        public const int CREEP_DAMAGE = 1;

        // Waves
        public const int WAVE_LENGTH = 35000; // Length of time between waves
        public const int WAVE_WINDOW = 30000; // Length of time a wave has to spawn its creeps

        // Effects
        public const int DEFAULT_SLOW_DURATION = 2500;
    }
}