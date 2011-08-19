using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    /*
     * SlowEffect is applied to a creep or tower to slow their movement or firing rate
     */
    public class SlowEffect : Effect
    {
        public SlowEffect(Entity e, int duration = Settings.DEFAULT_SLOW_DURATION)
            : base(e, duration)
        {
            type = "slow";
        }

        public override void apply(long dt)
        {
            base.apply(dt);

            if (Entity is Tower)
            {
                Tower t = (Tower)Entity;

                // Add half the current base fire rate to the effected fire rate
                t.EffectedFireRate += (int)(t.FireRate * 0.5f);
            }
            else if (Entity is Creep)
            {
                Creep c = (Creep)Entity;

                // Slow the creep
                if(!Finished)
                    c.EffectedSpeed -= (int)(c.Speed * 0.5f);
            }
        }
    }
}
