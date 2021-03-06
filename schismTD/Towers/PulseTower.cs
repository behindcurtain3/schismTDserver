﻿using System;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class PulseTower : Tower
    {
        public PulseTower(Game g, Player p, Player opponent, Vector2 pos)
            : base(g, p, opponent, pos)
        {
            FireRate = Settings.DEFAULT_FIRE_RATE * 3;
            Range = Settings.DEFAULT_RANGE * 1.25f;
            Damage = Settings.DEFAULT_DAMAGE * 15;
            SellValue = 210;

            Type = Tower.PULSE;
        }

        public override Boolean fire()
        {
            Boolean fired = false;
            lock (Opponent.Creeps)
            {
                foreach (Creep creep in Opponent.Creeps)
                {
                    if (!creep.Active)
                        continue;

                    if (creep.getDistance(this) <= EffectedRange && !fired)
                    {
                        lock (mGame.Projectiles)
                        {
                            mGame.Projectiles.Add(new PulseProjectile(mGame, new Vector2(Center), Opponent, EffectedDamage, EffectedRange));
                        }
                        fired = true;
                    }
                }
            }

            return fired;
        }

    }
}
