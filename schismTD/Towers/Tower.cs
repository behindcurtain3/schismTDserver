using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Tower : EffectEntity
    {
        public const String BASIC = "basic";
        public const String RAPID_FIRE = "rapidfire";
        public const String SLOW = "slow";
        public const String SNIPER = "sniper";
        public const String PULSE = "pulse";
        public const String SPELL = "spell";
        public const String SEED = "seed";

        protected long mFireRatePostion;
        protected Game mGame;

        public int SellValue
        {
            get
            {
                return mSellValue;
            }
            set
            {
                mSellValue = value;
            }
        }
        private int mSellValue = (int)(Costs.BASIC * Costs.RESELL_VALUE);

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

        public int EffectedFireRate
        {
            get
            {
                return mEffectedFireRate;
            }
            set
            {
                mEffectedFireRate = value;
            }
        }
        private int mEffectedFireRate;

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


        public String Type = Tower.BASIC;

        public Tower(Game g, Player p, Player opponent, Vector2 pos)
        {
            mGame = g;
            Player = p;
            mOpponent = opponent;

            Width = Settings.BOARD_CELL_WIDTH;
            Height = Settings.BOARD_CELL_HEIGHT;
            Position = pos;
            Cost = Costs.BASIC;

            FireRate = Settings.DEFAULT_FIRE_RATE * 3;
            Range = Settings.DEFAULT_RANGE * 2;
            Damage = Settings.DEFAULT_DAMAGE;

            mFireRatePostion = mFireRate;
        }

        public virtual Boolean fire()
        {
            // Fire the tower
            lock (mOpponent.Creeps)
            {
                int leastPathLength = 999;
                Creep targetCreep = null;

                foreach (Creep creep in mOpponent.Creeps)
                {
                    float d = creep.getDistance(this);
                    if (d < Range)
                    {
                        if (creep.CurrentPath.Count < leastPathLength && !creep.isDeathWaiting())
                        {
                            leastPathLength = creep.CurrentPath.Count;
                            targetCreep = creep;
                        }
                    }
                }

                if (targetCreep != null)
                {
                    lock (mGame.Projectiles)
                    {
                        if(this is SpellTower)
                            mGame.Projectiles.Add(new SpellProjectile(mGame, new Vector2(Center), targetCreep, Damage));
                        else if(this is SniperTower)
                            mGame.Projectiles.Add(new SniperProjectile(mGame, new Vector2(Center), targetCreep, Damage));
                        else
                            mGame.Projectiles.Add(new Projectile(mGame, new Vector2(Center), targetCreep, Damage));
                    }
                    return true;
                }
            }
            return false;
        }

        public virtual void update(long dt)
        {
            if (Enabled)
            {
                // Reset our values
                EffectedFireRate = FireRate;

                // Apply effects
                applyEffects(dt);

                if(mFireRatePostion > 0)
                    mFireRatePostion -= dt;
                else
                {
                    if(fire())
                        mFireRatePostion = mFireRate;    
                }
            }
        }
    }
}
