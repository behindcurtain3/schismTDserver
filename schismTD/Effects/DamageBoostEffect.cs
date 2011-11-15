using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class DamageBoostEffect : Effect
    {
        private Cell controlCell;

        public DamageBoostEffect(Entity e, Cell c)
            : base(e, int.MaxValue)
        {
            type = "damageboost";
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

                // Add half the current base fire rate to the effected fire rate
                t.EffectedDamage += t.Damage;
            }
        }
    }
}
