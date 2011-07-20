using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    
    public class Match
    {
        private int mNumGames = 3;
        private int mCurrentGameNum = 0;
        private Game mCurrentGame;

        private Boolean mIsStarted = false;
        private Boolean mIsFinished = false;

        private GameCode mCtx;
        private Player p1 = null;
        private Player p2 = null;
        private int mNumPlayers = 0;

        public Match(GameCode gc)
        {
            mCtx = gc;
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
            mIsStarted = true;
            mCtx.Broadcast(Messages.MATCH_READY);

            // Possibly add a countdown or both players click a ready button before calling this?
            mCtx.Broadcast(Messages.MATCH_STARTED);
        }

        public void finish()
        {
            mIsFinished = true;
            mCtx.Broadcast(Messages.MATCH_FINISHED);
        }

        public void update(int dt)
        {
            if (isFinished())
                return;

            if (isStarted())
            {
                if (mCurrentGame == null)
                    return;

                mCurrentGame.update(dt);
            }
            else
            {
                if (p1 != null && p2 != null)
                {
                    startNextGame();
                    start();
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

        public Boolean isStarted()
        {
            return mIsStarted;
        }

        public Boolean isFinished()
        {
            return mIsFinished;
        }

        public int numPlayers()
        {
            return mNumPlayers;
        }

    }
}
