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
            Range = 100;// Settings.DEFAULT_RANGE * 3;
            Damage = 70;
            SellValue = 75;

            EffectedFireRate = FireRate;
            EffectedDamage = Damage;
            EffectedRange = Range;

            Type = Tower.SPELL;
        }

    }
}
