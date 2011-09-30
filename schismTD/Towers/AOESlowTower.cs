using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class AOESlowTower : Tower
    {
         public AOESlowTower(Game g, Player p, Player opponent, Vector2 pos) : base(g, p, opponent, pos)
        {
            FireRate = 75;
            Range = Settings.DEFAULT_RANGE * 2;
            Damage = 0;
            SellValue = 210;

            Type = Tower.SLOW;
        }
        
        public override Boolean fire()
        {
            // Fire the tower
            lock (Opponent.Creeps)
            {
                foreach (Creep creep in Opponent.Creeps)
                {
                    if (!creep.Active)
                        continue;

                    // Magic creeps are immune to slow, don't shoot at them
                    if (creep.Type == "Magic")
                        continue;

                    float d = creep.getDistance(this);
                    if (d < EffectedRange)
                    {
                        if(!creep.hasEffect("slow"))
                            creep.addEffect(new SlowEffect(creep, 500));       
                    }
                }
            }

            return true;
        }

    }
}
