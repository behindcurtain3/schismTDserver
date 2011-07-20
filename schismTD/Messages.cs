﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public static class Messages
    {
        public const String CHAT = "chat_message";
        public const String PLAYER_JOINED = "player_joined";
        public const String PLAYER_LEFT = "player_left";

        // Match
        public const String MATCH_READY = "match_ready"; // Called when 2 players have joined
        public const String MATCH_STARTED = "match_start";
        public const String MATCH_FINISHED = "match_finish";

        public const String GAME_COUNTDOWN = "game_countdown"; // Countdown status at start of each game
        public const String GAME_START = "game_start";
        public const String GAME_FINISHED = "game_finish";
        public const String GAME_MANA = "game_mana";
        public const String GAME_LIFE = "game_life";

        public const String GAME_PLACE_WALL = "game_place_wall";
        public const String GAME_REMOVE_WALL = "game_remove_wall";
        public const String GAME_INVALID_WALL = "game_invalid_wall";
    }
}
