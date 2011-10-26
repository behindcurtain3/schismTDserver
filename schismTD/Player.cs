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
            set
            {
                mGame = value;
            }
        }
        private Game mGame = null;

        public String Name
        {
            get;
            set;
        }

        public Boolean isGuest
        {
            get;
            set;
        }

        public String KongId
        {
            get;
            set;
        }

        public String KongAuthToken
        {
            get;
            set;
        }

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

                if(mGame != null)
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

        public List<Wave> ActiveWaves
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

        public int WavePosition
        {
            get;
            set;
        }

        public uint DamageDealt
        {
            get;
            set;
        }

        public float ChiBlastCost
        {
            get { return mChiBlastCost; }
            set
            {
                if (value >= 50)
                    mChiBlastCost = value;
                else
                    mChiBlastCost = 50;
            }
        }
        private float mChiBlastCost;

        public float ChiBlastUses
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
            ActiveWaves = new List<Wave>();
            WavePosition = 0;

            ChiBlastCost = Settings.CHI_BLAST_INITIAL;
            ChiBlastUses = 0;
        }

    }
}
