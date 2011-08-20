using System;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class SniperTower : Tower
    {
        public SniperTower(Game g, Player p, Player opponent, Vector2 pos)
            : base(g, p, opponent, pos)
        {
            FireRate = Settings.DEFAULT_FIRE_RATE * 5;
            Range = Settings.DEFAULT_RANGE * 12;
            Damage = Settings.DEFAULT_DAMAGE * 25;
            SellValue = (int)(Costs.SNIPER * Costs.RESELL_VALUE);

            Type = Tower.SNIPER;
        }

    }
}
