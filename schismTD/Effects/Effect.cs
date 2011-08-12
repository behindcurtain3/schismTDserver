using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Effect
    {
        public String type = "base";

        public Entity Entity
        {
            get
            {
                return mEntity;
            }
        }
        protected Entity mEntity;

        public Boolean Finished
        {
            get
            {
                return mDurationPosition >= mDuration;
            }
        }

        protected int mDuration;
        protected int mDurationPosition;

        public Effect(Entity e, int duration)
        {
            mEntity = e;
            mDuration = duration;
            mDurationPosition = 0;
        }

        public virtual void apply(int dt) 
        {
            mDurationPosition += dt;
        }
    }
}
