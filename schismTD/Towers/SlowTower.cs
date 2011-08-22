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
            FireRate = Settings.DEFAULT_FIRE_RATE * 3;
            Range = Settings.DEFAULT_RANGE * 3;
            Damage = 0;
            SellValue = (int)(Costs.SLOW * 0.75);

            Type = Tower.SLOW;
        }
        
        public override Boolean fire()
        {
            // Fire the tower
            lock (Opponent.Creeps)
            {
                int leastPathLength = 999;
                Creep targetCreep = null;

                foreach (Creep creep in Opponent.Creeps)
                {
                    // Magic creeps are immune to slow, don't shoot at them
                    if (creep.Type == "Magic")
                        continue;

                    float d = creep.getDistance(this);
                    if (d < Range)
                    {
                        if (creep.CurrentPath.Count < leastPathLength && !creep.isDeathWaiting() && !creep.hasEffect("slow"))
                        {
                            leastPathLength = creep.CurrentPath.Count;
                            targetCreep = creep;
                        }
                    }
                }

                if (targetCreep != null)
                {
                    lock (mGame.Projectiles)
                        mGame.Projectiles.Add(new SlowProjectile(mGame, new Vector2(Center), targetCreep));

                    return true;
                }
            }

            return false;
        }
    }
}
