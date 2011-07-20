using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayerIO.GameLibrary;

namespace schismTD
{
    public class Game
    {
        private GameCode mCtx;
        private Player black;
        private Player white;

        // Seconds to countdown at start of game (IE: dead period)
        private const int mCountdownLength = 15000; // in milliseconds
        private int mCountdownPosition;

        private Boolean mIsStarted = false;
        private Boolean mIsFinished = false;
        private long mTotalTimeElapsed = 0;

        public Game(GameCode gc, Player p1, Player p2)
        {
            mCtx = gc;
            black = p1;
            white = p2;

            mCountdownPosition = mCountdownLength;
        }

        public void start()
        {
            
            mIsStarted = true;
            mTotalTimeElapsed = 0;

            // Reset both players
            black.reset();
            white.reset();

            // Send player info
            black.Send(Messages.GAME_LIFE, black.Life);
            black.Send(Messages.GAME_MANA, black.Mana);

            white.Send(Messages.GAME_LIFE, white.Life);
            white.Send(Messages.GAME_MANA, white.Mana);

            // Finally send the message to start the game
            mCtx.Broadcast(Messages.GAME_START);
            mCtx.AddMessageHandler(Messages.GAME_PLACE_WALL, placeWall);
            mCtx.AddMessageHandler(Messages.GAME_PLACE_WALL, removeWall);

        }

        public void update(int dt)
        {
            if (!isStarted())
            {
                mCountdownPosition -= dt;

                if (mCountdownPosition <= 0)
                {
                    // Start the game
                    start();
                }
                else
                {
                    // Update countdown timer
                    mCtx.Broadcast(Messages.GAME_COUNTDOWN, mCountdownPosition);
                }
            }
            else
            {
                // Game has started
                if (!isFinished())
                {
                    mTotalTimeElapsed += dt;
                }
            }
        }

        private void placeWall(Player p, Message m)
        {
            Player sender;

            if (p == black)
                sender = black;
            else if (p == white)
                sender = white;
            else
            {
                // If this was sent by someone not who isn't black or white, reject the request
                //p.Send(Messages.GAME_INVALID_WALL, message.GetInt(0), message.GetInt(1), message.GetInt(2), message.GetInt(3));
                // Error message above, for now I chose not to send it since this case likely happens by nefarious means
                return;
            }

            // Place the wall
            Wall w = new Wall(new Point(m.GetInt(0), m.GetInt(1)), new Point(m.GetInt(2), m.GetInt(3)));
            if (sender.Walls.Contains(w))
            {
                mCtx.Broadcast(Messages.GAME_INVALID_WALL, m.GetInt(0), m.GetInt(1), m.GetInt(2), m.GetInt(3));
            }
            else
            {
                sender.Walls.Add(w);
                mCtx.Broadcast(Messages.GAME_PLACE_WALL, w.Start.X, w.Start.Y, w.End.X, w.End.Y);
            }
        }

        private void removeWall(Player p, Message message)
        {
            return;
        }

        public Boolean isStarted()
        {
            return mIsStarted;
        }

        public Boolean isFinished()
        {
            return mIsFinished;
        }

    }
}
