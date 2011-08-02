using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace schismTD
{
    public class Projectile
    {
        private Game mGame;
        private float mVelocity;
        private int mWidth = 5;
        private int mHeight = 5;

        public Boolean Active
        {
            get
            {
                return mActive;
            }
        }
        private Boolean mActive = true;

        public Vector2 Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
            }
        }
        private Vector2 mPosition;

        public PointF Center
        {
            get
            {
                return mCenter;
            }
            set
            {
                mCenter = new PointF(value.X + mWidth / 2, value.Y + mHeight / 2);
            }
        }
        private PointF mCenter;

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

        public Projectile(Game game, Point position, Creep target)
        {
            mGame = game;
            mPosition = new Vector2(position.X, position.Y);
            mCenter = position;
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
                Vector2 movement = Position - new Vector2(Target.Center);
                movement.Normalize(); // Normalize it
                movement *= dd; // Scale it based on the velocity calculated above

                Position -= movement; // Apply it to the position
                Center = Position.getPointF(); // Get new center

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
