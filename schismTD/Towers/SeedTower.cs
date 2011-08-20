using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class SeedTower : Tower
    {
        public SeedTower(Game g, Player p, Player opponent, Vector2 pos) : base(g, p, opponent, pos)
        {
            FireRate = Settings.DEFAULT_FIRE_RATE * 30; // every 9 seconds
            Range = Settings.DEFAULT_RANGE * 3;
            Damage = Settings.DEFAULT_DAMAGE * 10;
            SellValue = (int)(Costs.SEED * Costs.RESELL_VALUE);

            Type = Tower.SEED;
        }

        public override bool fire()
        {
            Player.FreeTowers++;

            return true;
        }
    }
}
