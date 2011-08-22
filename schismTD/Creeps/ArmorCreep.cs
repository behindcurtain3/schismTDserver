using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class ArmorCreep : Creep
    {

        public ArmorCreep(Player player, Player opponent, Vector2 pos, Path p)
            : base(player, opponent, pos, p)
        {
            Type = "Armor";

            Speed = Settings.CREEP_SPEED;
            Damage = 3;
            Life = Settings.CREEP_LIFE * 2;
            Armor = (int)(Life * 0.1);
        }

        public override void onHit(string towerType, int damage)
        {
            if(towerType != "Spell")
                damage -= Armor;

            base.onHit(towerType, damage);
        }
    }
}
