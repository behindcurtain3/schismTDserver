using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class GameStats
    {
        public int Basic
        {
            get;
            set;
        }

        public int RapidFire
        {
            get;
            set;
        }

        public int Sniper
        {
            get;
            set;
        }

        public int Pulse
        {
            get;
            set;
        }

        public int Slow
        {
            get;
            set;
        }

        public int Spell
        {
            get;
            set;
        }

        public int DamageBoost
        {
            get;
            set;
        }

        public int RangeBoost
        {
            get;
            set;
        }

        public int FireRateBoost
        {
            get;
            set;
        }

        public GameStats()
        {
            Basic = 0;
            RapidFire = 0;
            Sniper = 0;
            Pulse = 0;
            Slow = 0;
            Spell = 0;
            DamageBoost = 0;
            RangeBoost = 0;
            FireRateBoost = 0;
        }
    }
}
