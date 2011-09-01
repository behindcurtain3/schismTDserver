using System;
using System.Collections.Generic;
using System.Text;
using PlayerIO.GameLibrary;

namespace schismTD
{
    public class Player : BasePlayer
    {
        public Game Game
        {
            get
            {
                return mGame;
            }
        }
        private Game mGame = null;

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
                mGame.Context.Broadcast(Messages.PLAYER_MANA, this.Id, mMana);
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

                mGame.Context.Broadcast(Messages.PLAYER_LIFE, this.Id, mLife);
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

        public Wave ActiveWave
        {
            get;
            set;
        }

        public Wave NextWave
        {
            get;
            set;
        }

        public List<Wave> OnDeckWaves
        {
            get;
            set;
        }

        public Queue<Wave> QueuedWaves
        {
            get;
            set;
        }

        public uint DamageDealt
        {
            get;
            set;
        }

        public void reset(Game game)
        {
            mGame = game;

            Mana = Settings.DEFAULT_MANA;
            Life = Settings.DEFAULT_LIFE;

            DamageDealt = 0;

            lock(Towers)
                Towers.Clear();
            lock(Creeps)
                Creeps.Clear();

            OnDeckWaves = new List<Wave>();
            QueuedWaves = new Queue<Wave>();
            ActiveWave = null;
            NextWave = null;
        }

    }
}
