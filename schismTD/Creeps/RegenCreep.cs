using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class RegenCreep : Creep
    {
        public const int DEFAULT_POINTS = 1;
        public const int DEFAULT_SPEED = 40;
        public const int DEFAULT_LIFE = Settings.CREEP_LIFE;
        public const int DEFAULT_DAMAGE = 1;

        private long mRegenTimer;
        private long mRegenPosition;

        private int mLifeRegen;
        private int mLastCheckedLife;
        private int mInitialLife;

        public int Range
        {
            get;
            set;
        }

        public RegenCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Regen";

            Speed = DEFAULT_SPEED;
            Damage = DEFAULT_DAMAGE;
            Life = DEFAULT_LIFE;
            StartingLife = Life;

            Worth *= DEFAULT_POINTS;

            mRegenTimer = 1000;
            mRegenPosition = 0;

            mLifeRegen = (int)(Life * 0.1f);
            mLastCheckedLife = Life;
            mInitialLife = Life;
            Range = Settings.BOARD_CELL_WIDTH * 4;
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

                lock (Player.Creeps)
                {
                    foreach (Creep cr in Player.Creeps)
                    {
                        if (cr == this)
                            continue;

                        if (cr.getDistance(Position) <= Range)
                        {
                            int newLife = cr.Life + (int)(cr.StartingLife * 0.25f);

                            if (newLife > cr.StartingLife)
                                newLife = cr.StartingLife;

                            cr.Life = newLife;
                        }
                    }
                }
            }

            base.update(dt);
        }
    }
}
