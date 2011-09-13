using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class MagicCreep : Creep
    {
        public const int DEFAULT_POINTS = 1;
        public const int DEFAULT_SPEED = 40;
        public const int DEFAULT_LIFE = Settings.CREEP_LIFE * 1;
        public const int DEFAULT_DAMAGE = 1;

        public MagicCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Magic";

            Speed = DEFAULT_SPEED;
            Damage = DEFAULT_DAMAGE;
            Life = DEFAULT_LIFE;
            StartingLife = Life;

            Worth *= DEFAULT_POINTS;
        }

        public override void update(long dt)
        {
            // remove slow effects, this creep is immune
            List<Effect> toRemove = new List<Effect>();

            foreach (Effect e in Effects)
            {
                if (e.type == "slow")
                {
                    toRemove.Add(e);
                }
            }

            foreach (Effect e in toRemove)
            {
                if (Effects.Contains(e))
                    lock (Effects)
                        Effects.Remove(e);
            }

            base.update(dt);
        }

        public override void onHit(string towerType, int damage)
        {
            if (towerType == "Spell")
                damage = (int)(damage * 0.5);

            base.onHit(towerType, damage);
        }
    }
}
