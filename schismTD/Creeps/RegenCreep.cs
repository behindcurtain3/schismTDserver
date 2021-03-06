﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class RegenCreep : Creep
    {
        public const int DEFAULT_POINTS = 2;
        public const int DEFAULT_SPEED = 40;
        public const float DEFAULT_LIFE = Settings.CREEP_LIFE * 1.5f;
        public const int DEFAULT_DAMAGE = 2;
        public const long INTERVAL = 2500;

        private long mRegenTimer;
        private long mRegenPosition;

        private int mLifeRegen;
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
            Life = (int)DEFAULT_LIFE;
            StartingLife = Life;

            Points = DEFAULT_POINTS;
            Worth *= DEFAULT_POINTS;

            mRegenTimer = 1000;
            mRegenPosition = 0;

            mLifeRegen = (int)(Life * 0.1f);
            mInitialLife = Life;
            Range = Settings.BOARD_CELL_WIDTH * 4;
        }

        public override void onHit(string towerType, int damage)
        {
            mRegenPosition = 0;
            base.onHit(towerType, damage);
        }

        public override void update(long dt)
        {
            mRegenPosition += dt;

            if (mRegenPosition >= mRegenTimer)
            {
                mRegenPosition = 0;

                lock (Player.Creeps)
                {
                    foreach (Creep cr in Player.Creeps)
                    {
                        if (cr == this || cr.Type == "Regen")
                            continue;

                        if (cr.getDistance(Position) <= Range)
                        {
                            int newLife = cr.Life + (int)(StartingLife * 0.25f);

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
