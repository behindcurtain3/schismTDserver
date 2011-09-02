﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class SpellTower : Tower
    {
        public SpellTower(Game g, Player p, Player opponent, Vector2 pos) : base(g, p, opponent, pos)
        {
            FireRate = 750;
            Range = Settings.DEFAULT_RANGE * 3;
            Damage = Settings.DEFAULT_DAMAGE * 15;
            SellValue = (int)(Costs.SPELL * Costs.RESELL_VALUE);

            Type = Tower.SPELL;
        }

        public override bool fire()
        {
            return base.fire();
        }
    }
}
