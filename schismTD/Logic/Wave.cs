using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Wave
    {
        private GameCode mCtx;
        private Game mGame;
        private Player mPlayer;
        private Player mOpponent;

        private int mNumCreepsInWave;
        private int mNumCreepsSpawned;

        private long mWaveTimeElapsed;
        private long mWaveTimeWindow;
        private float mTimeToNextSpawn;

        public float HealthModifier
        {
            get
            {
                return mHealthModifier;
            }
            set
            {
                mHealthModifier = value;
            }
        }
        private float mHealthModifier;

        public float DamageModifier
        {
            get
            {
                return mDamageModifier;
            }
            set
            {
                mDamageModifier = value;
            }
        }
        private float mDamageModifier;

        //private float mManaModifier;
        //private float mSpeedModifier;

        public List<Creep> CreepsToSpawn
        {
            get
            {
                return mCreepsToSpawn;
            }
        }
        private List<Creep> mCreepsToSpawn = new List<Creep>();

        public Random Rnd
        {
            get;
            set;
        }

        public Wave(GameCode gc, Game game, Player player, Player opponent)
        {
            mCtx = gc;
            mGame = game;
            mPlayer = player;
            mOpponent = opponent;

            mWaveTimeWindow = Settings.WAVE_WINDOW;
            mWaveTimeElapsed = 0;
            mTimeToNextSpawn = 0;
            mNumCreepsInWave = 25;
            mNumCreepsSpawned = 0;

            Rnd = new Random(11);
        }

        public void update(long dt)
        {
            mWaveTimeElapsed += dt;

            if (mWaveTimeElapsed <= mWaveTimeWindow && mNumCreepsSpawned < mNumCreepsInWave)
            {
                mTimeToNextSpawn -= dt;

                if (mTimeToNextSpawn <= 0)
                {
                    Random r = new Random();
                    mTimeToNextSpawn = (float)r.NextDouble() * 1000 * 1.5f;

                    Creep c = getNextCreep(mPlayer);                   

                    c.Life = (int)(c.Life * mHealthModifier);
                    lock(mPlayer.Creeps)
                        mPlayer.Creeps.Add(c);

                    mCtx.Broadcast(Messages.GAME_CREEP_ADD, c.ID, c.Type, c.Player.Id, c.Center.X, c.Center.Y, c.Speed);
                    mNumCreepsSpawned++;
                }
            }
            else
            {
                return;
            }
        }

        public Creep getNextCreep(Player p)
        {
            Path path;
            Vector2 v;
            if (p == mGame.Black)
            {
                path = mGame.Board.WhitePath;
                v = mGame.Board.WhiteSpawn.Position;
            }
            else
            {
                path = mGame.Board.BlackPath;
                v = mGame.Board.BlackSpawn.Position;
            }

            int rand = Rnd.Next(1, 8);

            switch (rand)
            {
                case 1:
                    return new Creep(mPlayer, mOpponent, v, path);
                case 2:
                    return new RegenCreep(mPlayer, mOpponent, v, path);
                case 3:
                    return new ChigenCreep(mPlayer, mOpponent, v, path);
                case 4:
                    return new QuickCreep(mPlayer, mOpponent, v, path);
                case 5:
                    return new MagicCreep(mPlayer, mOpponent, v, path);
                case 6:
                    return new ArmorCreep(mPlayer, mOpponent, v, path);
                case 7:
                    return new SwarmCreep(mPlayer, mOpponent, v, path);

                default:
                    return new Creep(mPlayer, mOpponent, v, path);
            }



        }
    }
}
