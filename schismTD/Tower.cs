using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Tower : Entity
    {
        private int mFireRatePostion;
        private Game mGame;

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

        public Player Opponent
        {
            get
            {
                return mOpponent;
            }
        }
        private Player mOpponent;

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

        public int Cost
        {
            get
            {
                return mCost;
            }
            set
            {
                mCost = value;
            }
        }
        private int mCost;


        public String Type = "basic";

        public Tower(Game g, Player p, Player opponent, Vector2 pos)
        {
            mGame = g;
            Player = p;
            mOpponent = opponent;

            Width = Settings.BOARD_CELL_WIDTH;
            Height = Settings.BOARD_CELL_HEIGHT;
            Position = pos;
            Cost = Costs.BASIC;
            

            mFireRatePostion = mFireRate;
        }

        public void update(int dt)
        {
            if (Enabled)
            {
                if(mFireRatePostion > 0)
                    mFireRatePostion -= dt;
                else
                {
                    // Fire the tower
                    lock (mOpponent.Creeps)
                    {
                        float closestDistance = Range;
                        Creep closestCreep = null;

                        foreach (Creep creep in mOpponent.Creeps)
                        {
                            float d = creep.getDistance(this);
                            if (d < closestDistance)
                            {
                                closestDistance = d;
                                closestCreep = creep;
                            }
                        }

                        if (closestCreep != null)
                        {
                            mFireRatePostion = mFireRate;
                            lock (mGame.Projectiles)
                            {
                                mGame.Projectiles.Add(new Projectile(mGame, new Vector2(Center), closestCreep));
                            }
                        }
                    }                    
                }
            }
        }
    }
}
