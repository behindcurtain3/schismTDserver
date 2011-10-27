using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class SwarmCreep : Creep
    {
        public const int DEFAULT_POINTS = 1;
        public const int DEFAULT_SPEED = 50;
        public const float DEFAULT_LIFE = Settings.CREEP_LIFE * 0.33f;
        public const int DEFAULT_DAMAGE = 1;
        public const long INTERVAL = 1250;

        public SwarmCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Swarm";

            Speed = DEFAULT_SPEED;
            Damage = DEFAULT_DAMAGE;
            Life = (int)DEFAULT_LIFE;
            StartingLife = Life;
            Points = DEFAULT_POINTS;

            Worth = (int)(Worth * 0.25f);
        }
    }
}
