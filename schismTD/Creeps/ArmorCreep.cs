using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class ArmorCreep : Creep
    {
        public const int DEFAULT_POINTS = 3;
        public const int DEFAULT_SPEED = 20;
        public const int DEFAULT_LIFE = Settings.CREEP_LIFE * 3;
        public const int DEFAULT_DAMAGE = 1;
        public const int DEFAULT_ARMOR = 10;

        public ArmorCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Armor";

            Speed = DEFAULT_SPEED;
            Damage = DEFAULT_DAMAGE;
            Life = DEFAULT_LIFE;
            StartingLife = Life;

            Worth *= DEFAULT_POINTS;

            Armor = DEFAULT_ARMOR;
        }

        public override void onHit(string towerType, int damage)
        {
            if (towerType != "Spell")
            {
                damage -= Armor;
                if (damage <= 0)
                    return;
            }

            base.onHit(towerType, damage);
        }
    }
}
