using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    /*
    public class Match
    {
        private GameCode mCtx;
        private Player p1 = null;
        private Player p2 = null;
        private int mNumGames = Settings.GAMES_PER_MATCH;
        private int mCurrentGameNum = 0;
        private long mCooldownTime = Settings.DEFAULT_MATCH_COOLDOWN * 1000;
        private long mCooldownPosition;

        public Game Game
        {
            get
            {
                return mCurrentGame;
            }
        }
        private Game mCurrentGame;

        public Boolean Started
        {
            get
            {
                return mIsStarted;
            }
            set
            {
                mIsStarted = value;
            }
        }
        private Boolean mIsStarted = false;

        public Boolean Finished
        {
            get
            {
                return mIsFinished;
            }
            set
            {
                mIsFinished = value;
            }
        }
        private Boolean mIsFinished = false;

        public Boolean ReadyForRestart
        {
            get { return mReadyForRestart; }
        }
        private Boolean mReadyForRestart = false;

        public int Players
        {
            get
            {
                return mNumPlayers;
            }
        }
        private int mNumPlayers = 0;

        public Match(GameCode gc)
        {
            mCtx = gc;
            mCooldownPosition = mCooldownTime;
        }

        public void addPlayer(Player p)
        {
            if (p1 != null && p2 != null)
                return;

            if (p1 == null)
                p1 = p;
            else
                p2 = p;

            mNumPlayers++;
        }

        public void removePlayer(Player p)
        {
            if (p == p1 || p == p2)
            {
                if (p == p1)
                    p1 = null;
                else if (p == p2)
                    p2 = null;

                finish();
            }
        }

        public void start()
        {
            Started = true;

            p1.Opponent = p2;
            p2.Opponent = p1;

            mCtx.Broadcast(Messages.MATCH_READY);

            // Possibly add a countdown or both players click a ready button before calling this?
            mCtx.Broadcast(Messages.MATCH_STARTED, p1.Id, p2.Id);
            p1.Send(Messages.MATCH_SET_ID, p1.Id);
            p2.Send(Messages.MATCH_SET_ID, p2.Id);
        }

        public void finish()
        {
            Finished = true;

            if (mCurrentGame != null)
            {
                mCurrentGame.finish();
                mCtx.Broadcast(Messages.MATCH_FINISHED, mCurrentGame.Black.Life, mCurrentGame.White.Life, mCurrentGame.Black.DamageDealt, mCurrentGame.White.DamageDealt);
            }
        }

        public void update(long dt)
        {
            if (Finished)
            {
                mCooldownPosition -= dt;

                if (mCooldownPosition <= 0)
                {
                    mReadyForRestart = true;
                }
                return; // always return here if finished
            }

            if (Started)
            {
                if (mCurrentGame == null)
                    return;

                mCurrentGame.update(dt);

                if (mCurrentGame.Finished)
                {
                    startNextGame();
                }
            }
            else
            {
                if (p1 != null && p2 != null)
                {
                    start();
                    startNextGame();                    
                }
            }
        }

        public void startNextGame()
        {
            mCurrentGameNum++;

            if (mCurrentGameNum <= mNumGames)
            {
                mCtx.Broadcast(Messages.CHAT, "Game #" + mCurrentGameNum + " has started");
                mCurrentGame = new Game(mCtx, p1, p2);
            }
            else
            {
                finish();
            }
        }

    }
     */
}
