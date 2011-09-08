using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class RapidFireProjectile : Projectile
    {
        public RapidFireProjectile(Game game, Vector2 position, Creep target, int damage)
            :base(game, position, target, damage)
        {
            Type = "RapidFire";
        }
    }
}
