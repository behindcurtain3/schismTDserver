using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class SlowTower : Tower
    {
        public SlowTower(Game g, Player p, Player opponent, Vector2 pos) : base(g, p, opponent, pos)
        {
            FireRate = Settings.DEFAULT_FIRE_RATE;
            Range = Settings.DEFAULT_RANGE * 3;
            Damage = 0;

            Type = "Slow";
        }

        public override void fire()
        {
            lock (Opponent.Creeps)
            {
                foreach (Creep creep in Opponent.Creeps)
                {
                    if (creep.getDistance(this) <= Range)
                    {
                        if (!creep.hasEffect("slow"))
                        {
                            creep.Effects.Add(new SlowEffect(creep));
                        }
                    }
                }
            }
        }
    }
}
