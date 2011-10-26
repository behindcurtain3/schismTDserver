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
        public const String DAMAGE_BOOST = "damageboost";
        public const String RANGE_BOOST = "rangeboost";
        public const String RATE_BOOST = "rateboost";

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
            get;
            set;
        }

        public long FireRatePosition
        {
            get;
            set;
        }

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

        public int EffectedDamage
        {
            get;
            set;
        }

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

        public float EffectedRange
        {
            get;
            set;
        }

        public Boolean Enabled
        {
            get;
            set;
        }

        public int Cost
        {
            get;
            set;
        }

        public Creep StaticTarget
        {
            get;
            set;
        }

        public Cell Cell
        {
            get;
            private set;
        }

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

            FireRate = 1800; // Settings.DEFAULT_FIRE_RATE * 3;
            Range = Settings.DEFAULT_RANGE * 2;
            Damage = 30;
            SellValue = 7;

            FireRatePosition = mFireRate;

            StaticTarget = null;
            Enabled = true;
        }

        public virtual Boolean fire()
        {
            // Fire the tower
            int leastPathLength = 999;
            Creep targetCreep = null;

            if (StaticTarget != null)
            {
                if (StaticTarget.Alive)
                    targetCreep = StaticTarget;
                else
                    StaticTarget = null;
            }

            if (targetCreep == null)
            {
                lock (mOpponent.Creeps)
                {
                    foreach (Creep creep in mOpponent.Creeps)
                    {
                        if (!creep.Active)
                            continue;

                        float d = creep.getDistance(this);
                        if (d < EffectedRange)
                        {
                            if (creep.CurrentPath.Count < leastPathLength && !creep.isDeathWaiting())
                            {
                                leastPathLength = creep.CurrentPath.Count;
                                targetCreep = creep;
                            }
                        }
                    }
                }
            }

            if (targetCreep != null)
            {
                lock (mGame.Projectiles)
                {
                    if(this is RapidFireTower)
                        mGame.Projectiles.Add(new RapidFireProjectile(mGame, new Vector2(Center), targetCreep, EffectedDamage));
                    else if (this is SniperTower)
                        mGame.Projectiles.Add(new SniperProjectile(mGame, new Vector2(Center), targetCreep, EffectedDamage));
                    else if (this is SpellTower)
                        mGame.Projectiles.Add(new SpellProjectile(mGame, new Vector2(Center), targetCreep, EffectedDamage));
                    else
                        mGame.Projectiles.Add(new Projectile(mGame, new Vector2(Center), targetCreep, EffectedDamage));
                }
                return true;
            }

            return false;
        }

        public virtual void update(long dt)
        {
            if (Enabled)
            {
                // Reset our values
                EffectedFireRate = FireRate;
                EffectedDamage = Damage;
                EffectedRange = Range;

                // Apply effects
                applyEffects(dt);

                FireRatePosition += dt;

                if (FireRatePosition > EffectedFireRate)
                {
                    if(fire())
                        FireRatePosition = 0;
                }
            }
        }

        public virtual void onPlaced(Cell c)
        {
            Cell = c;
        }

        public virtual void onRemoved(Cell c)
        {
        }

        public override void addEffect(Effect e)
        {
            if(Cell != null)
                Player.Game.Context.Broadcast(Messages.GAME_TOWER_EFFECT, Cell.Index, e.type, (int)e.Duration);

            base.addEffect(e);
        }
    }
}
