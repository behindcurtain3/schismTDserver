using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class RapidFireTower : Tower
    {
        public RapidFireTower(Game g, Player p, Player opponent, Vector2 pos) : base(g, p, opponent, pos)
        {
            FireRate = (int)(Settings.DEFAULT_FIRE_RATE * 0.33f);
            Range = Settings.DEFAULT_RANGE * 3;
            Damage = Settings.DEFAULT_DAMAGE * 2;

            Type = Tower.RAPID_FIRE;
        }

    }
}
