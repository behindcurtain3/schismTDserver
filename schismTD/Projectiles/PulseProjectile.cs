using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace schismTD
{
    public class PulseProjectile : Projectile
    {
        public float Radius
        {
            get
            {
                return mRadius;
            }
        }
        private float mRadius;

        private float mRange;
        private List<Creep> mCreepsHit = new List<Creep>();
        private Player mOpponent;

        public PulseProjectile(Game game, Vector2 position, Player target, int damage, float range)
            :base(game, position, null, damage)
        {
            mOpponent = target;
            mRadius = 0.0f;
            mRange = range;
            Velocity = 5 * Settings.BOARD_CELL_WIDTH;

            Type = "Pulse";
        }

        public override void updateClients()
        {
            //mGame.Context.Broadcast(Messages.GAME_PROJECTILE_ADD, ID, Position.X, Position.Y, Velocity);
        }

        public override void onHit()
        {
        }

        public override void update(long dt)
        {
            // The pulse projectile spreads outward from its source, expanding the radius each update
            mRadius += (dt * 0.001f) * Velocity;

            if (mRadius >= mRange)
            {
                Active = false;
                mRadius = mRange;
            }

            lock (mOpponent.Creeps)
            {
                foreach (Creep creep in mOpponent.Creeps)
                {
                    if (creep.getDistance(Position) <= mRadius)
                    {
                        if (!mCreepsHit.Contains(creep))
                        {
                            creep.onHit(Type, Damage);
                            mCreepsHit.Add(creep);
                        }
                        
                    }
                }
            }
        }

        public override void move(long dt)
        {   
        }

        public override void draw(Graphics g)
        {
            g.DrawEllipse(Pens.White, Position.X - mRadius, Position.Y - mRadius, mRadius * 2, mRadius * 2);
        }
    }
}
