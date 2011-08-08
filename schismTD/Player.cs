using System;
using System.Collections.Generic;
using System.Text;
using PlayerIO.GameLibrary;

namespace schismTD
{
    public class Player : BasePlayer
    {
        public String Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }
        private String mName;

        public Player Opponent
        {
            get
            {
                return mOpponent;
            }
            set
            {
                mOpponent = value;
            }
        }
        private Player mOpponent;

        // Game data
        public int Mana
        {
            get
            {
                return mMana;
            }
            set
            {
                mMana = value;
            }
        }
        private int mMana = Settings.DEFAULT_MANA;

        public int Life
        {
            get
            {
                return mLife;
            }
            set
            {
                mLife = value;
                if (mLife < 0)
                    mLife = 0;
            }
        }
        private int mLife = Settings.DEFAULT_LIFE;

        public List<Tower> Towers
        {
            get
            {
                return mTowers;
            }
            set
            {
                mTowers = value;
            }
        }
        private List<Tower> mTowers = new List<Tower>();

        public List<Creep> Creeps
        {
            get
            {
                return mCreeps;
            }
            set
            {
                mCreeps = value;
            }
        }
        private List<Creep> mCreeps = new List<Creep>();

        public List<Wave> Waves
        {
            get
            {
                return mWaves;
            }
            set
            {
                mWaves = value;
            }
        }
        private List<Wave> mWaves = new List<Wave>();

        public void reset(GameCode gc, Game game)
        {
            Mana = Settings.DEFAULT_MANA;
            Life = Settings.DEFAULT_LIFE;

            Towers.Clear();
            Creeps.Clear();
            Waves.Clear();
        }

    }
}
