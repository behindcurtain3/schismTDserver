using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using PlayerIO.GameLibrary;
using System.Drawing;
using System.Diagnostics;

namespace schismTD
{
    [RoomType("schismTD")]
    public class GameCode : Game<Player>
    {
        private Game mGame;

        private Bitmap mDebugImage;

        // Debug
        private Boolean mShowNeighbors = false;
        private Boolean mShowPassable = false;
        private Boolean mShowTowers = true;
        private Boolean mShowLabels = true;
        private Boolean mShowPaths = false;
        private Boolean mShowCreeps = true;
        private Boolean mShowProjectiles = true;
        private Boolean mShowWireFrame = false;

        private Stopwatch mStopWatch = new Stopwatch();

        private Player p1 = null;
        private Player p2 = null;

        // This method is called when an instance of your the game is created
        public override void GameStarted()
        {
            PreloadPlayerObjects = true;

            mDebugImage = new Bitmap(800, 600);

            // anything you write to the Console will show up in the 
            // output window of the development server
            Console.WriteLine("Game is started: " + RoomId);

            // This is how you setup a timer
            AddTimer(delegate
            {
                if (mGame != null)
                {
                    if (mGame.Init)
                    {
                        lock (mStopWatch)
                        {
                            mStopWatch.Stop(); // Hammertime!
                        }

                        mGame.update(mStopWatch.ElapsedMilliseconds);
                        mStopWatch.Reset();
                        mStopWatch.Start();
                    }
                }
            }, 40);
            /*
            AddTimer(delegate
            {
                RefreshDebugView();
            }, 100);
             */
        }

        // This method is called when the last player leaves the room, and it's closed down.
        public override void GameClosed()
        {
            Console.WriteLine("RoomId: " + RoomId);
        }

        // This method is called whenever a player joins the game
        public override void UserJoined(Player player)
        {
            Console.WriteLine(player.ConnectUserId + " has joined the game.");
            if (player.JoinData.ContainsKey("guest"))
            {
                Boolean guest;
                Boolean.TryParse(player.JoinData["guest"], out guest);
                player.isGuest = guest;
            }

            if (player.JoinData.ContainsKey("name"))
                player.Name = player.JoinData["name"];
            else
                player.Name = "Guest";

            if (player.JoinData.ContainsKey("id"))
                player.KongId = player.JoinData["id"];
            else
                player.KongId = "";

            if (player.JoinData.ContainsKey("auth_token"))
                player.KongAuthToken = player.JoinData["auth_token"];
            else
                player.KongAuthToken = "";

            // this is how you send a player a message
            player.Send(Messages.GAME_JOINED);

            if (p1 == null)
                p1 = player;
            else if (p2 == null)
                p2 = player;

            if (p1 != null && p2 != null)
            {
                Broadcast(Messages.CHAT, "Init game");
                mGame = new Game(this, p1, p2);

                if (RoomData.ContainsKey("rated"))
                {
                    Console.WriteLine("Is game rated? " + RoomData["rated"]);
                    if (RoomData["rated"] == "false")
                        mGame.Ranked = false;
                }
            }
        }

        // This method is called when a player leaves the game
        public override void UserLeft(Player player)
        {
            Console.WriteLine(player.ConnectUserId + " has left the game.");

            if (mGame != null)
            {
                if ((mGame.Black == player || mGame.White == player) && !mGame.Finished)
                { 
                    mGame.finishEarly(player);
                }
            }
        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message message)
        {
            switch (message.Type)
            {
                case Messages.CHAT:
                    Broadcast(Messages.CHAT, player.ConnectUserId + " says: " + message.GetString(0));
                    break;
            }
        }

        // This method get's called whenever you trigger it by calling the RefreshDebugView() method.
        public override System.Drawing.Image GenerateDebugImage()
        {
            // we'll just draw 400 by 400 pixels image with the current time, but you can
            // use this to visualize just about anything.
            //var image = new Bitmap(800, 600);
            mDebugImage = new Bitmap(800, 600);

            using (var g = Graphics.FromImage(mDebugImage))
            {
                // fill the background
                g.FillRectangle(Brushes.Tan, 0, 0, mDebugImage.Width, mDebugImage.Height);

                // draw the current time
                g.DrawString(DateTime.Now.ToString(), new Font("Verdana", 10F), Brushes.Black, 10, 10);

                if (mGame != null && mGame.Started)
                {
                    Brush off = Brushes.DarkGray;
                    Brush on = Brushes.White;
                    Brush current = on;

                    // Draw White Cells
                    foreach (Cell c in mGame.Board.WhiteCells)
                    {
                        if (c.Coords.Y % 2 == 0)
                            if (c.Coords.X % 2 == 0)
                                current = off;
                            else
                                current = on;
                        else
                            if (c.Coords.X % 2 == 0)
                                current = on;
                            else
                                current = off;

                        drawCell(g, current, c);

                    }

                    on = Brushes.Black;
                    // Draw Black Cells
                    foreach (Cell c in mGame.Board.BlackCells)
                    {
                        if (c.Coords.Y % 2 == 0)
                            if (c.Coords.X % 2 == 0)
                                current = off;
                            else
                                current = on;
                        else
                            if (c.Coords.X % 2 == 0)
                                current = on;
                            else
                                current = off;

                        drawCell(g, current, c);
                    }

                    // Draw paths
                    if (mShowPaths)
                    {
                        lock (mGame.Board.BlackPath)
                        {
                            foreach (Cell p in mGame.Board.BlackPath)
                            {
                                g.FillRectangle(Brushes.SandyBrown, p.Position.X + 2, p.Position.Y + 2, Settings.BOARD_CELL_WIDTH - 4, Settings.BOARD_CELL_HEIGHT - 4);
                            }
                        }

                        lock (mGame.Board.WhitePath)
                        {
                            foreach (Cell p in mGame.Board.WhitePath)
                            {
                                g.FillRectangle(Brushes.SandyBrown, p.Position.X + 2, p.Position.Y + 2, Settings.BOARD_CELL_WIDTH - 4, Settings.BOARD_CELL_HEIGHT - 4);
                            }
                        }
                    }

                    // Draw creeps!
                    if (mShowCreeps)
                    {
                        lock (mGame.Black.Creeps)
                        {
                            foreach (Creep c in mGame.Black.Creeps)
                            {
                                g.FillEllipse(Brushes.RoyalBlue, c.Position.X, c.Position.Y, c.Width, c.Height);
                            }
                        }
                        lock (mGame.White.Creeps)
                        {
                            foreach (Creep c in mGame.White.Creeps)
                            {
                                g.FillEllipse(Brushes.RoyalBlue, c.Position.X, c.Position.Y, c.Width, c.Height);
                            }
                        }
                    }

                    if (mShowProjectiles)
                    {
                        lock (mGame.Projectiles)
                        {
                            foreach (Projectile p in mGame.Projectiles)
                            {
                                p.draw(g);
                                //g.FillEllipse(Brushes.DarkTurquoise, p.Position.X, p.Position.Y, 5, 5);
                            }
                        }
                    }
                }
            }
            return mDebugImage;
        }

        public void drawCell(Graphics g, Brush b, Cell c)
        {
            if (mShowWireFrame)
            {
                g.DrawRectangle(Pens.Black, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT);
            }
            else
            {
                g.FillRectangle(b, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT);
            }

            // Draw neighbor links
            if (mShowNeighbors)
            {
                lock (c.Neighbors)
                {
                    foreach (Cell neighbor in c.Neighbors.Keys)
                    {
                        g.DrawLine(Pens.Orange, c.Center, neighbor.Center);
                    }
                }
            }
            
            // Draw tower
            if (mShowTowers)
            {
                if (c.Tower != null)
                {
                    g.DrawString("T", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 6, c.Position.Y + 3);
                    //g.DrawString(c.Tower.EffectedDamage.ToString(), new Font("Verdana", 8F), Brushes.Blue, c.Position.X, c.Position.Y + Settings.BOARD_CELL_HEIGHT - 12);
                    //g.DrawString(c.Tower.EffectedRange.ToString(), new Font("Verdana", 8F), Brushes.Blue, c.Position.X, c.Position.Y);
                    //g.DrawString(c.Tower.EffectedFireRate.ToString(), new Font("Verdana", 8F), Brushes.Blue, c.Position.X + Settings.BOARD_CELL_WIDTH - 15, c.Position.Y + Settings.BOARD_CELL_HEIGHT - 12);
                }
            }

            // Draw Labels
            if (mShowLabels)
            {
                if (c == mGame.Board.WhiteSpawn)
                    g.DrawString("W", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 1, c.Position.Y + 3);

                if (c == mGame.Board.BlackSpawn)
                    g.DrawString("B", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 3, c.Position.Y + 3);

                if (mGame.Board.WhiteBase.Contains(c) || mGame.Board.BlackBase.Contains(c))
                    g.DrawString("X", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 4, c.Position.Y + 3);
            }

            // Draw passable marker
            if (mShowPassable)
            {
                if (c.Passable)
                    g.FillEllipse(Brushes.Tomato, c.Position.X + 10, c.Position.Y + 10, 10, 10);
            }

        }

        // Debug actions
        [DebugAction("Creeps", DebugAction.Icon.Green)]
        public void ToogleCreeps()
        {
            mShowCreeps = !mShowCreeps;
        }

        [DebugAction("Paths", DebugAction.Icon.Green)]
        public void TooglePaths()
        {
            mShowPaths = !mShowPaths;
        }

        [DebugAction("Neighbors", DebugAction.Icon.Green)]
        public void ToogleNeighbors()
        {
            mShowNeighbors = !mShowNeighbors;
        }

        [DebugAction("Passable", DebugAction.Icon.Green)]
        public void TooglePassable()
        {
            mShowPassable = !mShowPassable;
        }

        [DebugAction("Towers", DebugAction.Icon.Green)]
        public void ToogleTowers()
        {
            mShowTowers = !mShowTowers;
        }

        [DebugAction("Projectiles", DebugAction.Icon.Green)]
        public void ToogleProjectiles()
        {
            mShowProjectiles = !mShowProjectiles;
        }

        [DebugAction("Labels", DebugAction.Icon.Green)]
        public void ToogleLabels()
        {
            mShowLabels = !mShowLabels;
        }

        [DebugAction("Wire Frame", DebugAction.Icon.Green)]
        public void ToogleWireFrame()
        {
            mShowWireFrame = !mShowWireFrame;
        }


        // If you use the [DebugAction] attribute on a method with
        // two int arguments, the action will be triggered via the
        // debug view when you click the debug view on a running game.
        /*
        [DebugAction("Set Debug Point", DebugAction.Icon.Green)]
        public void SetDebugPoint(int x, int y)
        {
            debugPoint = new Point(x, y);
        }
         */
    }
}

