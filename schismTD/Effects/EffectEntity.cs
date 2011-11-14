using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class EffectEntity : Entity
    {

        public List<Effect> Effects
        {
            get
            {
                return mEffects;
            }
        }
        private List<Effect> mEffects = new List<Effect>();

        public void applyEffects(long dt)
        {
            // Apply any effects
            lock (Effects)
            {
                List<Effect> toRemove = new List<Effect>();
                foreach (Effect effect in Effects)
                {
                    effect.apply(dt);

                    if (effect.Finished)
                        toRemove.Add(effect);
                }

                foreach (Effect effect in toRemove)
                {
                    Effects.Remove(effect);
                    Console.WriteLine(effect.type);
                }
            }
        }

        public Boolean hasEffect(String t)
        {
            Effect result = Effects.Find(delegate(Effect e)
                            {
                                return e.type == t;
                            });

            return result != null;
        }

        public virtual void addEffect(Effect e)
        {
            lock (mEffects)
            {
                mEffects.Add(e);
            }
        }

        public Effect getEffect(String t)
        {
            Effect result = Effects.Find(delegate(Effect e)
            {
                return e.type == t;
            });

            return result;
        }
    }
}
