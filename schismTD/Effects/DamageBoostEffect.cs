using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class DamageBoostEffect : Effect
    {
        public DamageBoostEffect(Entity e, int duration = 0)
            : base(e, duration)
        {
            type = "damageboost";
        }

        public override void apply(long dt)
        {
            if (Entity is Tower)
            {
                Tower t = (Tower)Entity;

                // Add half the current base fire rate to the effected fire rate
                t.EffectedDamage += (int)(t.Damage * 0.25f);
            }
        }
    }
}
