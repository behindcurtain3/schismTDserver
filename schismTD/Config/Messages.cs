using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public static class Messages
    {
        public const String CHAT = "cm";
        public const String PLAYER_JOINED = "pj";
        public const String PLAYER_LEFT = "pl";
        public const String PLAYER_MANA = "pm";
        public const String PLAYER_LIFE = "pli";

        // Match
        public const String MATCH_ID = "mi";
        public const String MATCH_READY = "mr"; // Called when 2 players have joined
        public const String MATCH_STARTED = "ms";
        public const String MATCH_FINISHED = "mf";
        public const String MATCH_SET_ID = "msi";

        public const String GAME_JOINED = "gj";
        public const String GAME_INFO = "gi";
        public const String GAME_COUNTDOWN = "gc";  // Countdown status at start of each game
        public const String GAME_ACTIVATE = "ga";    // Activates tower building
        public const String GAME_START = "gs";
        public const String GAME_FINISHED = "gf";      // The game is finished
        public const String GAME_TIME = "gt";

        public const String GAME_CELL_ADD = "gca";

        public const String GAME_WAVE_QUEUE = "gwq";
        public const String GAME_WAVE_ACTIVATE = "gwa";
        public const String GAME_WAVE_NEXT = "gwn";
        public const String GAME_WAVE_REMOVE = "gwr";

        public const String GAME_TOWER_PLACE = "gtp";
        public const String GAME_TOWER_REMOVE = "gtr";
        public const String GAME_TOWER_INVALID = "gti";
        public const String GAME_TOWER_UPGRADE = "gtu";
        public const String GAME_TOWER_SELL = "gts";
        
        public const String GAME_FIRE_AT = "gfa";

        public const String GAME_CREEP_ADD = "gcra";
        public const String GAME_CREEP_REMOVE = "gcrr";
        public const String GAME_CREEP_PATH = "gcrp"; // updates path of single creep
        public const String GAME_ALL_CREEPS_PATH = "gacrp"; // updates the cached path
        public const String GAME_CREEP_UPDATE_LIFE = "gcrul";
        public const String GAME_CREEP_EFFECT = "gcre";

        public const String GAME_PROJECTILE_ADD = "gpa";
        public const String GAME_PROJECTILE_REMOVE = "gpr";
        public const String GAME_PROJECTILE_UPDATE = "gpu";

        // Lobby messages
        public const String LOBBY_LOGIN = "ll";
        public const String LOBBY_WAVE_LIST = "lwl";
    }
}
