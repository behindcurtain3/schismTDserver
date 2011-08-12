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
                    float d = creep.getDistance(this);
                    if (d < Range)
                    {
                        if (creep.CurrentPath.Count < leastPathLength && !creep.isDeathWaiting())
                        {
                            leastPathLength = creep.CurrentPath.Count;
                            targetCreep = creep;
                        }
                    }
                }

                if (targetCreep != null)
                {
                    if (!targetCreep.hasEffect("slow"))
                    {
                        lock (mGame.Projectiles)
                            mGame.Projectiles.Add(new SlowProjectile(mGame, new Vector2(Center), targetCreep));

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
