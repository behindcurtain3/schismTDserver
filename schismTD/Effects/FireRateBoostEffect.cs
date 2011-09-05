using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class FireRateBoostEffect : Effect
    {
        public FireRateBoostEffect(Entity e, int duration = int.MaxValue)
            : base(e, duration)
        {
            type = "rateboost";
        }

        public override void apply(long dt)
        {
            if (Entity is Tower)
            {
                Tower t = (Tower)Entity;

                t.EffectedFireRate -= (int)(t.EffectedFireRate * 0.33f);

                if (t.EffectedFireRate <= 0)
                    t.EffectedFireRate = 1;
            }
        }
    }
}
