using System;
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

        public const String GAME_COUNTDOWN = "game_countdown";  // Countdown status at start of each game
        public const String GAME_ACTIVATE = "game_activate";    // Activates tower building
        public const String GAME_START = "game_start";          // The game actually starts
        public const String GAME_FINISHED = "game_finish";      // The game is finished
        public const String GAME_MANA = "game_mana";
        public const String GAME_LIFE = "game_life";
        public const String GAME_TIME = "game_time";

        public const String GAME_ADD_CELL = "game_add_cell";

        public const String GAME_PLACE_WALL = "game_place_wall";
        public const String GAME_REMOVE_WALL = "game_remove_wall";
        public const String GAME_INVALID_WALL = "game_invalid_wall";

        public const String GAME_PLACE_TOWER = "game_place_tower";
        public const String GAME_REMOVE_TOWER = "game_remove_tower";
        public const String GAME_INVALID_TOWER = "game_invalid_tower";
        public const String GAME_UPGRADE_TOWER = "game_upgrade_tower";
        public const String GAME_SELL_TOWER = "game_sell_tower";

        public const String GAME_CREEP_ADD = "game_creep_add";
        public const String GAME_CREEP_REMOVE = "game_creep_remove";
        public const String GAME_CREEP_PATH = "game_creep_path"; // updates path of single creep
        public const String GAME_ALL_CREEPS_PATH = "game_all_creeps_path"; // updates the cached path
        public const String GAME_CREEP_UPDATE_POSITION = "game_creep_update_position";
        public const String GAME_CREEP_UPDATE_LIFE = "game_creep_update_life";
        public const String GAME_CREEP_EFFECT = "game_creep_effect";

        public const String GAME_PROJECTILE_ADD = "game_projectile_add";
        public const String GAME_PROJECTILE_REMOVE = "game_projectile_remove";
        public const String GAME_PROJECTILE_UPDATE = "game_projectile_update";

        // Lobby messages
        public const String LOBBY_LOGIN = "lobby_login";
        public const String LOBBY_WAVE_LIST = "lobby_wave_list";
    }
}
