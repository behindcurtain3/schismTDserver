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
            Damage = 70;
            SellValue = 75;

            Type = Tower.SPELL;
        }

        public override void onPlaced(Cell c)
        {
            Player.ChiBlastCost -= Player.ChiBlastCost * 0.1f;

            base.onPlaced(c);
        }

    }
}
