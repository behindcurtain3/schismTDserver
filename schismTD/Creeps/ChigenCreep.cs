﻿using System;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class ChigenCreep : Creep
    {
        public const int DEFAULT_POINTS = 2;
        public const int DEFAULT_SPEED = 40;
        public const int DEFAULT_LIFE = Settings.CREEP_LIFE * 2;
        public const int DEFAULT_DAMAGE = 2;

        private long mChiTimer;
        private long mChiPosition;

        public int ChiAdded
        {
            get;
            set;
        }

        public ChigenCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Chigen";

            Speed = DEFAULT_SPEED;
            Damage = DEFAULT_DAMAGE;
            Life = DEFAULT_LIFE;
            StartingLife = Life;

            Worth *= DEFAULT_POINTS;

            mChiTimer = 3000;
            mChiPosition = 0;

            ChiAdded = 1;
        }

        public override void update(long dt)
        {
            mChiPosition += dt;

            if (mChiPosition >= mChiTimer)
            {
                mChiPosition = 0;
                Player.Mana += ChiAdded;
            }

            base.update(dt);
        }

    }
}
