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
            FireRate = 200;
            Range = 100;// Settings.DEFAULT_RANGE * 3;
            Damage = 20;
            SellValue = 75;

            Type = Tower.RAPID_FIRE;
        }

    }
}
