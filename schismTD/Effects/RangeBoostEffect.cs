using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class RangeBoostEffect : Effect
    {
        private Cell controlCell;

        public RangeBoostEffect(Entity e, Cell c)
            : base(e, int.MaxValue)
        {
            type = "rangeboost";
            controlCell = c;
        }

        public override void apply(long dt)
        {
            if (Entity is Tower)
            {
                if (controlCell.Tower != null)
                {
                    if (controlCell.Tower.hasEffect("stun"))
                        return;
                }

                Tower t = (Tower)Entity;

                t.EffectedRange += (t.Range * 0.5f);
            }
        }
    }
}
