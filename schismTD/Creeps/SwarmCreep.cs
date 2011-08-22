using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class SwarmCreep : Creep
    {
        public SwarmCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Swarm";

            Speed = (int)(Settings.CREEP_SPEED * 1.25f);
            Damage = 1;
            Life = Settings.CREEP_LIFE / 3;
        }
    }
}
