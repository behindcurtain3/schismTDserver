using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class RangeBoostEffect : Effect
    {
        public RangeBoostEffect(Entity e, int duration = int.MaxValue)
            : base(e, duration)
        {
            type = "rangeboost";
        }

        public override void apply(long dt)
        {
            if (Entity is Tower)
            {
                Tower t = (Tower)Entity;

                t.EffectedRange += (t.Range * 0.5f);
            }
        }
    }
}
