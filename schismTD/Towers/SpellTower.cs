using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class SpellTower : Tower
    {
        public SpellTower(Game g, Player p, Player opponent, Vector2 pos) : base(g, p, opponent, pos)
        {
            FireRate = Settings.DEFAULT_FIRE_RATE;
            Range = Settings.DEFAULT_RANGE * 3;
            Damage = Settings.DEFAULT_DAMAGE * 10;
            SellValue = (int)(Costs.SPELL * Costs.RESELL_VALUE);

            Type = Tower.SPELL;
        }
    }
}
