using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class RegenCreep : Creep
    {
        private long mRegenTimer;
        private long mRegenPosition;

        private int mLifeRegen;
        private int mLastCheckedLife;
        private int mInitialLife;
       

        public RegenCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Regen";

            Speed = (int)(Settings.CREEP_SPEED * 1.25f);
            Damage = 2;
            Life = Settings.CREEP_LIFE * 2;

            mRegenTimer = 1000;
            mRegenPosition = 0;

            mLifeRegen = (int)(Life * 0.1f);
            mLastCheckedLife = Life;
            mInitialLife = Life;
        }

        public override void update(long dt)
        {
            if(mLastCheckedLife != Life)
            {
                mRegenPosition = 0;
            }
            else
            {
                mRegenPosition += dt;
            }
            mLastCheckedLife = Life;

            if (mRegenPosition >= mRegenTimer)
            {
                mRegenPosition = 0;

                int newLife = Life + mLifeRegen;

                if (newLife > mInitialLife)
                    newLife = mInitialLife;

                Life = newLife;
            }

            base.update(dt);
        }
    }
}
