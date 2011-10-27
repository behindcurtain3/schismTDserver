using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class QuickCreep : Creep
    {
        public const int DEFAULT_POINTS = 1;
        public const int DEFAULT_SPEED = 80;
        public const int DEFAULT_LIFE = Settings.CREEP_LIFE;
        public const int DEFAULT_DAMAGE = 1;
        public const long INTERVAL = 1250;

        public QuickCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Quick";

            Speed = DEFAULT_SPEED;
            Damage = DEFAULT_DAMAGE;
            Life = DEFAULT_LIFE;
            StartingLife = Life;
            Points = DEFAULT_POINTS;
        }
    }
}
