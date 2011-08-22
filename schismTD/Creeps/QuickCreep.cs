using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class QuickCreep : Creep
    {

        public QuickCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Quick";

            Speed = (int)(Settings.CREEP_SPEED * 1.5f);
            Damage = 2;
            Life = Settings.CREEP_LIFE;
        }
    }
}
