using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Tower
    {
        public Point Position
        {
            get
            {
                return mPostion;
            }
            set
            {
                mPostion = value;
            }
        }
        private Point mPostion;

        public Player Player
        {
            get
            {
                return mPlayer;
            }
            set
            {
                mPlayer = value;
            }
        }
        private Player mPlayer;

        public long FireRate
        {
            get
            {
                return mFireRate;
            }
            set
            {
                mFireRate = value;
            }
        }
        private long mFireRate = Settings.DEFAULT_FIRE_RATE; // in milliseconds

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
        private int mDamage = Settings.DEFAULT_DAMAGE;

        public float Range
        {
            get
            {
                return mRange;
            }
            set
            {
                mRange = value;
            }
        }
        private float mRange = Settings.DEFAULT_RANGE;

        public String Type = "basic";

        public Tower(Player p, Point pos)
        {
            Player = p;
            Position = pos;
        }
    }
}
