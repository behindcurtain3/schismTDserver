using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Tower
    {
        public Point Position;
        public Player Player;

        public long FireRate = Settings.DEFAULT_FIRE_RATE; // in milliseconds
        public int Damage = Settings.DEFAULT_DAMAGE;
        public float Range = Settings.DEFAULT_RANGE;

        public Tower(Player p, Point pos)
        {
            Player = p;
            Position = pos;
        }
    }
}
