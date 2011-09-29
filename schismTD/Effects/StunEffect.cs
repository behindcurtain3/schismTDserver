using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    /*
     * 
     */
    public class StunEffect : Effect
    {
        public StunEffect(Entity e, int duration = Settings.DEFAULT_SLOW_DURATION)
            : base(e, duration)
        {
            type = "stun";
        }

        public override void apply(long dt)
        {
            base.apply(dt);

            if (Entity is Tower)
            {
                Tower t = (Tower)Entity;

                // Reset the fire rate position to 0 so it can no longer fire
                t.FireRatePosition = 0;
            }
        }
    }
}
