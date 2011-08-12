using System;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class PulseTower : Tower
    {
        public PulseTower(Game g, Player p, Player opponent, Vector2 pos)
            : base(g, p, opponent, pos)
        {
            FireRate = Settings.DEFAULT_FIRE_RATE * 4;
            Range = Settings.DEFAULT_RANGE;
            Damage = Settings.DEFAULT_DAMAGE * 12;

            Type = Tower.PULSE;
        }

        public override void fire()
        {
            lock (Opponent.Creeps)
            {
                foreach (Creep creep in Opponent.Creeps)
                {
                    if (creep.getDistance(this) <= Range)
                    {
                        creep.Life -= Damage;
                    }
                }
            }
        }

    }
}
