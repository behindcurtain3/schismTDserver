using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class SlowProjectile : Projectile
    {
        public SlowProjectile(Game game, Vector2 position, Creep target)
            :base(game, position, target, 0)
        {
            
        }

        public override void onHit()
        {
            if (!Target.hasEffect("slow"))
            {
                Target.addEffect(new SlowEffect(Target));
            }

            Active = false;
        }
    }
}
