using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class FireRateBoostEffect : Effect
    {
        public Cell controlCell;

        public FireRateBoostEffect(Entity e, Cell c)
            : base(e, int.MaxValue)
        {
            type = "rateboost";
            controlCell = c;
        }

        public override void apply(long dt)
        {
            if (Entity is Tower)
            {
                if (controlCell.Tower != null)
                {
                    if(controlCell.Tower.hasEffect("stun"))
                        return;
                }

                Tower t = (Tower)Entity;

                t.EffectedFireRate -= (int)(t.EffectedFireRate * 0.5f);

                if (t.EffectedFireRate <= 0)
                    t.EffectedFireRate = 1;
            }
        }
    }
}
