using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace schismTD
{
    public class Projectile : Entity
    {
        protected Game mGame;

        public float Velocity
        {
            get { return mVelocity; }
            set { mVelocity = value; }
        }
        private float mVelocity;

        public Boolean Active
        {
            get
            {
                return mActive;
            }
            set
            {
                mActive = value;

                if (!mActive)
                {
                    mGame.Context.Broadcast(Messages.GAME_PROJECTILE_REMOVE, ID);
                }
            }
        }
        private Boolean mActive = true;

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

        public Projectile(Game game, Vector2 position, Creep target, int damage = Settings.DEFAULT_DAMAGE)
        {
            mGame = game;

            Width = 5;
            Height = 5;

            // The position passed in is the center of the tower, we need to recalc the projectiles position so the center is aligned with the center of the tower
            Position = new Vector2(position.X - Width / 2, position.Y - Height / 2);
            Target = target;

            Velocity = 200;
            Damage = damage;

            mGame.Context.Broadcast(Messages.GAME_PROJECTILE_ADD, ID, Position.X, Position.Y, mVelocity, mTarget.ID);
        }

        public virtual void onHit()
        {
            // It hits
            Target.Life -= Damage;
            Active = false;
        }

        public virtual void update(long dt)
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

                // Collision check
                if (Target.HitBox.Contains(HitBox))
                {
                    onHit();
                }
            }
            else
            {
                Active = false;
            }
        }
    }
}
