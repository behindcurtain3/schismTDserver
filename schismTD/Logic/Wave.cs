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

        private float mManaModifier;
        private float mSpeedModifier;

        public List<Creep> CreepsToSpawn
        {
            get
            {
                return mCreepsToSpawn;
            }
        }
        private List<Creep> mCreepsToSpawn = new List<Creep>();

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

                    Creep c;
                    if (mPlayer == mGame.Black)
                        c = new Creep(mPlayer, mOpponent, mGame.Board.WhiteSpawn.Position, mGame.Board.WhitePath);
                    else
                        c = new Creep(mPlayer, mOpponent, mGame.Board.BlackSpawn.Position, mGame.Board.BlackPath);

                    c.Life = (int)(c.Life * mHealthModifier);
                    lock(mPlayer.Creeps)
                        mPlayer.Creeps.Add(c);

                    mCtx.Broadcast(Messages.GAME_CREEP_ADD, c.ID, c.Player.Id, c.Center.X, c.Center.Y, c.Speed);
                    mNumCreepsSpawned++;
                }
            }
            else
            {
                return;
            }
        }
    }
}
