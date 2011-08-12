using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace schismTD
{
    public class Creep : EffectEntity
    {
        // Pathing
        public Path CurrentPath
        {
            get
            {
                return mCurrentPath;
            }
            set
            {
                mCurrentPath = value;
            }
        }
        private Path mCurrentPath;

        // The next cell in the path the creep is trying to get to
        public Cell MovingTo
        {
            get
            {
                return mMovingTo;
            }
            set
            {
                mMovingTo = value;
            }
        }
        private Cell mMovingTo;
        
        // Player the creep belongs to
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

        // Stats
        public Boolean Alive
        {
            get
            {
                return mAlive;
            }
            set
            {
                mAlive = value;
            }
        }
        private Boolean mAlive;

        public int Life
        {
            get
            {
                return mLife;
            }
            set
            {
                if (mLife <= 0)
                    return;

                mLife = value;

                if (mLife <= 0)
                {
                    Alive = false;
                    Opponent.Mana += Worth; // Increase the opponents mana
                    Player.Game.Context.Broadcast(Messages.GAME_CREEP_REMOVE, ID);
                }
                else
                {
                    Player.Game.Context.Broadcast(Messages.GAME_CREEP_UPDATE_LIFE, ID, mLife);
                }
            }
        }
        private int mLife = Settings.CREEP_LIFE;

        public int Speed
        {
            get
            {
                return mSpeed;
            }
            set
            {
                mSpeed = value;
            }
        }
        private int mSpeed = Settings.CREEP_SPEED;

        public int EffectedSpeed
        {
            get
            {
                return mEffectedSpeed;
            }
            set
            {
                mEffectedSpeed = value;
            }
        }
        private int mEffectedSpeed = Settings.CREEP_SPEED;

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
        private int mDamage = Settings.CREEP_DAMAGE;

        // An invalid creep needs to be resynched with clients
        public Boolean Valid
        {
            get
            {
                return mValid;
            }
            set
            {
                mValid = value;
            }
        }
        private Boolean mValid;

        public int Worth
        {
            get
            {
                return mWorth;
            }
            set
            {
                mWorth = value;
            }
        }
        private int mWorth;

        public Creep(Player player, Player opponent, Vector2 pos, Path p)
        {
            Player = player;
            mOpponent = opponent;
            CurrentPath = new Path(p);
            Worth = 5;
            
            Height = Settings.BOARD_CELL_HEIGHT;
            Width = Settings.BOARD_CELL_WIDTH;

            Position = pos;

            Valid = false;
            Alive = true;
            Life = Settings.CREEP_LIFE;
        }

        public void update(int dt)
        {
            if (Life <= 0)
            {
                Alive = false;
            }

            // Reset values
            EffectedSpeed = Speed;


            // Apply effects
            applyEffects(dt);

            // Move creep
            if(CurrentPath.Count > 0)
            {
                if (MovingTo == null)
                {
                    MovingTo = CurrentPath.Peek();
                    invalidate();
                }

                float d = getDistance(MovingTo);

                if (d <= 3)
                {
                    // Remove last cell
                    CurrentPath.Pop();

                    // Arrived
                    if (CurrentPath.Count == 0)
                    {
                        // Remove creep
                        Alive = false;
                        // Deal damage
                        Opponent.Life -= Damage;

                        Player.Game.Context.Broadcast(Messages.GAME_CREEP_REMOVE, ID);
                    }
                    else
                    {
                        MovingTo = CurrentPath.Peek();
                        invalidate();
                    }
                }

                if (Alive)
                {
                    float dv = dt * 0.001f;
                    float dd = EffectedSpeed * dv;

                    // Do movement
                    Vector2 movement = new Vector2(Center) - new Vector2(MovingTo.Center);
                    movement.Normalize();
                    movement *= dd;
                    Position -= movement;
                }
            }
        }

        public float getDistance(Cell c)
        {
            return Math.Abs(Center.X - c.Center.X) + Math.Abs(Center.Y - c.Center.Y);
        }

        public float getDistance(Tower t)
        {
            return Math.Abs(Center.X - t.Center.X) + Math.Abs(Center.Y - t.Center.Y);
        }

        public float getDistance(PointF p)
        {
            return Math.Abs(Center.X - p.X) + Math.Abs(Center.Y - p.Y);
        }

        public void invalidate()
        {
            Valid = false;
        }

        /*
         * isDeathWaiting looks at all the projectiles currently fired at this creep.
         * If the total damage of those projectiles exceeds its current life this will return true
         */
        public Boolean isDeathWaiting()
        {
            int totalDamage = 0;

            lock (Player.Game.Projectiles)
            {
                foreach(Projectile p in Player.Game.Projectiles)
                {
                    if (p.Target == this)
                        totalDamage += p.Damage;
                }
            }

            return totalDamage >= Life;
        }
    }
}
