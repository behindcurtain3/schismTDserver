using System;
using System.Collections.Generic;
using System.Text;

namespace schismTD
{
    public class SniperProjectile : Projectile
    {
        public SniperProjectile(Game game, Vector2 position, Creep target, int damage)
            :base(game, position, target, damage)
        {
            Type = "Sniper";
        }
    }
}
