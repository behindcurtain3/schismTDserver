using System;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class ChigenCreep : Creep
    {
        private long mChiTimer;
        private long mChiPosition;
        private int mManaAdded;

        public ChigenCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Chigen";

            Speed = Settings.CREEP_SPEED;
            Damage = 1;
            Life = Settings.CREEP_LIFE * 2;

            mChiTimer = 5000;
            mChiPosition = 0;

            mManaAdded = 1;
        }

        public override void update(long dt)
        {
            mChiPosition += dt;

            if (mChiPosition >= mChiTimer)
            {
                mChiPosition = 0;
                Player.Mana += mManaAdded;
            }

            base.update(dt);
        }

    }
}
