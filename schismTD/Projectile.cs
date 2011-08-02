using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace schismTD
{
    public class Projectile : Entity
    {
        private Game mGame;
        private float mVelocity;

        public Boolean Active
        {
            get
            {
                return mActive;
            }
        }
        private Boolean mActive = true;

        public int Damage
        {
            get
            {
                return mDamage;
            }
        }
        private int mDamage;

        public Creep Target
        {
            get
            {
                return mTarget;
            }
            set
            {
                mTarget = value;
            }
        }
        private Creep mTarget;

        public Projectile(Game game, Vector2 position, Creep target)
        {
            mGame = game;

            Width = 5;
            Height = 5;

            // The position passed in is the center of the tower, we need to recalc the projectiles position so the center is aligned with the center of the tower
            Position = new Vector2(position.X - Width / 2, position.Y - Height / 2);
            mTarget = target;

            mVelocity = 200;
            mDamage = 10;
        }

        public void update(int dt)
        {
            if (Target != null)
            {

                float dv = dt * 0.001f;
                float dd = mVelocity * dv;

                // Movement vector will point in the correct direction
                Vector2 movement = new Vector2(Center) - new Vector2(Target.Center);
                movement.Normalize(); // Normalize it
                movement *= dd; // Scale it based on the velocity calculated above

                Position -= movement; // Apply it to the position

                // If the projectile is within the radius of the target
                if (Target.getDistance(Center) <= Target.Width / 2)
                {
                    // It hits
                    Target.Life -= Damage;
                    mActive = false;
                }
            }
            else
            {
                mActive = false;
            }
        }

        public Boolean collidesWith(Creep creep)
        {
            //if(Center.X >= creep.Position.X && Center.X <= creep.Position.X + creep.Width)
            return false;
        }
    }
}
