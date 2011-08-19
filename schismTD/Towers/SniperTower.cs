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
            Range = Settings.DEFAULT_RANGE * 10;
            Damage = Settings.DEFAULT_DAMAGE * 15;
            SellValue = (int)(Costs.SNIPER * 0.75);

            Type = Tower.SNIPER;
        }

    }
}
