using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class GameStats
    {
        public uint Basic
        {
            get;
            set;
        }

        public uint RapidFire
        {
            get;
            set;
        }

        public uint Sniper
        {
            get;
            set;
        }

        public uint Pulse
        {
            get;
            set;
        }

        public uint Slow
        {
            get;
            set;
        }

        public uint Spell
        {
            get;
            set;
        }

        public uint DamageBoost
        {
            get;
            set;
        }

        public uint RangeBoost
        {
            get;
            set;
        }

        public uint FireRateBoost
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
