using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Tower
    {
        private int mFireRatePostion;
        private Game mGame;

        public Point Position
        {
            get
            {
                return mPostion;
            }
            set
            {
                mPostion = value;
            }
        }
        private Point mPostion;

        public Player Player
        {
            get
            {
                return mPlayer;
            }
            set
            {
                mPlayer = value;
            }
        }
        private Player mPlayer;

        public int FireRate
        {
            get
            {
                return mFireRate;
            }
            set
            {
                mFireRate = value;
            }
        }
        private int mFireRate = Settings.DEFAULT_FIRE_RATE; // in milliseconds

        public int Damage
        {
            get
            {
                return mDamage;
            }
            set
            {
                mDamage = value;
            }
        }
        private int mDamage = Settings.DEFAULT_DAMAGE;

        public float Range
        {
            get
            {
                return mRange;
            }
            set
            {
                mRange = value;
            }
        }
        private float mRange = Settings.DEFAULT_RANGE;

        public Boolean Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }
        private Boolean mEnabled = true;


        public String Type = "basic";

        public Tower(Game g, Player p, Point pos)
        {
            mGame = g;
            Player = p;
            Position = pos;

            mFireRatePostion = mFireRate;
        }

        public void update(int dt)
        {
            if (Enabled)
            {
                mFireRatePostion -= dt;

                // Fire the tower
                if (mFireRatePostion <= 0)
                {
                    mFireRatePostion = mFireRate;

                    if(Player == mGame.White)
                    {
                        // Shoot at black creeps only
                        
                    }
                    else
                    {

                    }
                    if(mGame.Creeps.Count > 0)
                        mGame.Projectiles.Add(new Projectile(mGame, Position, mGame.Creeps[0]));
                }
            }
        }
    }
}
