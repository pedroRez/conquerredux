using System.Collections.Generic; 
namespace Redux
{
    public static class Constants
    {
        public const int EXP_BALL_LIMIT = 10;
        public const uint EMERALD_ID = 1080001, DB_SCROLL_ID = 720028, DRAGONBALL_ID = 1088000, METEOR_SCROLL_ID = 720027, METEOR_ID = 1088001, METEOR_TEAR_ID = 1088002, MOONBOX_ID = 721020, CELESTIAL_STONE_ID = 721259;
        public const bool IsSameSexMarriageAllowed = true;
        public const int EXP_RATE = 8,
                              PROF_RATE = 8,
                              SKILL_RATE = 5,
                              GOLD_RATE = 5;

        /// <summary>
        /// Level-based EXP multipliers for the requested curve:
        /// 1-40: 20x, 40-100: 12x, 100-110: 8x, 110-130: 20x. Ranges are inclusive and evaluated in order
        /// (so level 40 stays at 20x and level 100 uses 8x as requested).
        /// Adjust the ranges or values here to tune progression without touching the rest of the code.
        /// </summary>
        public static readonly IReadOnlyList<LevelExpRateRange> LevelExpRateBands = new List<LevelExpRateRange>
        {
            new LevelExpRateRange(110, 130, 20),
            new LevelExpRateRange(100, 110, 8),
            new LevelExpRateRange(1, 40, 20),
            new LevelExpRateRange(40, 100, 12)
        };

        public static int GetExpRateForLevel(int level)
        {
            foreach (var band in LevelExpRateBands)
            {
                if (band.Contains(level))
                    return band.Rate;
            }

            return EXP_RATE;
        }

        // Multiplier applied to monster spawn counts when maps are initialized.
        public const double MONSTER_SPAWN_MULTIPLIER = 1.5;

        public const double SOCKET_RATE = .4,
            CHANCE_REFINED = 10.0,
            CHANCE_UNIQUE = 6.0,
            CHANCE_ELITE = 1.0,
            CHANCE_SUPER = 0.3,
            CHANCE_PLUS = 2.0,
            CHANCE_METEOR = 0.8,
            CHANCE_METEOR_HIGH_LEVEL = 0.3,
            CHANCE_DRAGONBALL = 0.08,
            CHANCE_GEAR_DROP = 25,
            CHANCE_GOLD_DROP = 18,
            CHANCE_POTION = 4,
            CHANCE_REFINED_GEM = 10,
            CHANCE_SUPER_GEM = 1;

        public const byte METEOR_REDUCED_DROP_LEVEL = 100;

        public static bool DEBUG_MODE;
        public const byte RESPONSE_INVALID = 1,
                          RESPONSE_VALID = 2,
                          RESPONSE_BANNED = 12,
                          RESPONSE_INVALID_ACCOUNT = 57;
        public const int IPSTR_SIZE = 16,
                            MACSTR_SIZE = 12,
                            MAX_NAMESIZE = 16,
                            MAX_BROADCASTSIZE = 80,
                            MAX_USERFRIENDSIZE = 50,
                            MAX_ENEMYSIZE = 10,
                            MAX_TRADEITEMS = 20,
                            MAX_TRADEMONEY = 100000000,
                            MAX_TEAMAMOUNT = 5,
                            MAX_ADDITION = 12,
                            MAX_GUILDALLYSIZE = 5,
                            MAX_GUILDENEMYSIZE = 5;
        public const ushort BOOTH_LOOK = 407;
        public const ushort
            MSG_REGISTER = 1001,
            MSG_TALK = 1004,
            MSG_WALK = 1005,
            MSG_HERO_INFORMATION = 1006,
            MSG_ITEM_INFORMATION = 1008,
            MSG_ITEM_ACTION = 1009,
            MSG_ACTION = 1010,
            MSG_STRINGS = 1015,
            MSG_UPDATE = 1017,
            MSG_ASSOCIATE = 1019,
            MSG_INTERACT = 1022,
            MSG_TEAM_INTERACT = 1023,
            MSG_ASSIGN_ATTRIBUTES = 1024,
            MSG_PROFICIENCY = 1025,
            MSG_TEAMMEMBER_INFO = 1026,
            MSG_SOCKET_GEM = 1027,
            MSG_DATE_TIME = 1033,
            MSG_CONNECT = 1052,
            MSG_TRADE = 1056,
            MSG_GROUND_ITEM = 1101,
            MSG_WAREHOUSE_ACTION = 1102,
            MSG_CONQUER_SKILL = 1103,
            MSG_SKILL_EFFECT = 1105,
            MSG_GUILD_REQUEST = 1107,
            MSG_EXAMINE_ITEM = 1108,
            MSG_NPC_SPAWN = 2030,
            MSG_NPC_INITIAL = 2031,
            MSG_NPC_DIALOG = 2032,
            MSG_ASSOCIATE_INFO = 2033,
            MSG_COMPOSE = 2036,
            MSG_OFFLINETG = 2044,
            MSG_BROADCAST = 2050,
            MSG_GUILDMEMBERINFO = 1112,
            MSG_NOBILITY = 2064,
            MSG_MENTORACTION = 2065,
            MSG_MENTORINFO = 2066,
            MSG_MENTORPRIZE = 2067;

        public const int TIME_ADJUST_HOUR = -5,
            TIME_ADJUST_MIN = 0,
            TIME_ADJUST_SEC = 0;

        public static int LOGIN_PORT = 9959;
        public static int GAME_PORT = 5816;
        public static uint MINUTES_BANNED_BRUTEFORCE = 30;
        public static uint MAX_CONNECTIONS_PER_MINUTE = 10;

        public static string GAME_IP = "0.0.0.0",
                             SERVER_NAME = "Redux_Beta";

        public const string SYSTEM_NAME = "SYSTEM",
                            ALLUSERS_NAME = "ALLUSERS",
                            REPLY_OK_STR = "ANSWER_OK",
                            REPLAY_AGAIN_STR = "ANSWER_AGAIN",
                            NEW_ROLE_STR = "NEW_ROLE",
                            DEFAULT_MATE = "None";

        public const int STAT_MAXLIFE_STR = 3, STAT_MAXLIFE_AGI = 3, STAT_MAXLIFE_VIT = 24, STAT_MAXLIFE_SPI = 3; 
        public const int STAT_MAXMANA_STR = 0, STAT_MAXMANA_AGI = 0, STAT_MAXMANA_VIT = 0, STAT_MAXMANA_SPI = 5;

        public static readonly byte[] RC5_PASSWORDKEY = new byte[]
                                                            {
                                                                0x3c, 0xdc, 0xfe, 0xe8, 0xc4, 0x54, 0xd6, 0x7e,
                                                                0x16, 0xa6, 0xf8, 0x1a, 0xe8, 0xd0, 0x38, 0xbe
                                                            };
        public const int RC5_32 = 32,
                         RC5_12 = 12,
                         RC5_SUB = RC5_12 * 2 + 2,
                         RC5_16 = 16,
                         RC5_KEY = RC5_16 / 4;

        public const uint RC5_PW32 = 0xb7e15163, RC5_QW32 = 0x9e3779b9; 
        public const char COMMAND_PREFIX = '/';

        public const string GM_ID = "GM",
                            PM_ID = "PM",
                            GM_TAG = "[" + GM_ID + "]",
                            PM_TAG = "[" + PM_ID + "]";
  
        public static readonly uint[] ProficiencyLevelExperience = new uint[] { 0, 1200, 68000, 250000, 640000, 1600000, 4000000, 10000000, 22000000, 40000000, 90000000, 95000000, 142500000, 213750000, 320625000, 480937500, 721406250, 1082109375, 1623164063, 2100000000, 0 };


        public static readonly string[] GemEffectsByID = new string[] { "phoenix", "goldendragon", "lounder1", "rainbow", "goldenkylin", "purpleray", "moon", "recovery", };
    }

    public readonly struct LevelExpRateRange
    {
        public int MinLevel { get; }
        public int MaxLevel { get; }
        public int Rate { get; }

        public LevelExpRateRange(int minLevel, int maxLevel, int rate)
        {
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            Rate = rate;
        }

        public bool Contains(int level)
        {
            return level >= MinLevel && level <= MaxLevel;
        }
    }
}
