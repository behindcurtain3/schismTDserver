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
        public Match mMatch;

        // Debug
        private Boolean mShowNeighbors = false;
        private Boolean mShowPassable = false;
        private Boolean mShowTowers = true;
        private Boolean mShowLabels = true;
        private Boolean mShowPaths = false;
        private Boolean mShowCreeps = true;
        private Boolean mShowProjectiles = true;
        private Boolean mShowWireFrame = false;

        // This method is called when an instance of your the game is created
        public override void GameStarted()
        {
            if (mMatch == null)
            {
                mMatch = new Match(this);
            }


            // anything you write to the Console will show up in the 
            // output window of the development server
            Console.WriteLine("Game is started: " + RoomId);

            // This is how you setup a timer
            AddTimer(delegate
            {
                if (mMatch != null)
                {
                    if (mMatch.ReadyForRestart)
                    {
                        // For now just create a new match & add players
                        mMatch = new Match(this);
                        foreach (Player p in Players)
                        {
                            mMatch.addPlayer(p);
                        }
                    }
                    else
                    {
                        mMatch.update(40);
                    }
                    RefreshDebugView();
                }
            }, 40);

            // Debug Example:
            // Sometimes, it can be very usefull to have a graphical representation
            // of the state of your game.
            // An easy way to accomplish this is to setup a timer to update the
            // debug view every 250th second (4 times a second).
            //AddTimer(delegate
            //{
                // This will cause the GenerateDebugImage() method to be called
                // so you can draw a grapical version of the game state.
            //}, 250);
        }

        // This method is called when the last player leaves the room, and it's closed down.
        public override void GameClosed()
        {
            Console.WriteLine("RoomId: " + RoomId);
        }

        // This method is called whenever a player joins the game
        public override void UserJoined(Player player)
        {
            // this is how you send a player a message
            player.Send(Messages.CHAT, "Welcome to schismTD!");

            // this is how you broadcast a message to all players connected to the game
            Broadcast(Messages.PLAYER_JOINED, player.Id);

            if (mMatch != null)
            {
                mMatch.addPlayer(player);
            }
        }

        // This method is called when a player leaves the game
        public override void UserLeft(Player player)
        {
            if (mMatch != null)
            {
                mMatch.removePlayer(player);
            }

            Broadcast(Messages.PLAYER_LEFT, player.Id);
        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message message)
        {
            switch (message.Type)
            {
                // This is how you would set a players name when they send in their name in a 
                // "MyNameIs" message
                case Messages.CHAT:
                    Broadcast(Messages.CHAT, player.ConnectUserId + " says: " + message.GetString(0));
                    break;
                case "MyNameIs":
                    player.Name = message.GetString(0);
                    break;
            }
        }

        // This method get's called whenever you trigger it by calling the RefreshDebugView() method.
        public override System.Drawing.Image GenerateDebugImage()
        {
            // we'll just draw 400 by 400 pixels image with the current time, but you can
            // use this to visualize just about anything.
            var image = new Bitmap(800, 600);
            using (var g = Graphics.FromImage(image))
            {
                // fill the background
                g.FillRectangle(Brushes.Tan, 0, 0, image.Width, image.Height);

                // draw the current time
                g.DrawString(DateTime.Now.ToString(), new Font("Verdana", 10F), Brushes.Black, 10, 10);

                if (mMatch != null)
                {
                    if (mMatch.Game != null)
                    {
                        Brush off = Brushes.DarkGray;
                        Brush on = Brushes.White;
                        Brush current = on;

                        // Draw White Cells
                        foreach (Cell c in mMatch.Game.Board.WhiteCells)
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
                        foreach (Cell c in mMatch.Game.Board.BlackCells)
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
                            lock (mMatch.Game.Board.BlackPath)
                            {
                                foreach (Cell p in mMatch.Game.Board.BlackPath)
                                {
                                    g.FillRectangle(Brushes.SandyBrown, p.Position.X + 2, p.Position.Y + 2, Settings.BOARD_CELL_WIDTH - 4, Settings.BOARD_CELL_HEIGHT - 4);
                                }
                            }

                            lock (mMatch.Game.Board.WhitePath)
                            {
                                foreach (Cell p in mMatch.Game.Board.WhitePath)
                                {
                                    g.FillRectangle(Brushes.SandyBrown, p.Position.X + 2, p.Position.Y + 2, Settings.BOARD_CELL_WIDTH - 4, Settings.BOARD_CELL_HEIGHT - 4);
                                }
                            }
                        }

                        // Draw creeps!
                        if (mShowCreeps)
                        {
                            lock (mMatch.Game.Black.Creeps)
                            {
                                foreach (Creep c in mMatch.Game.Black.Creeps)
                                {
                                    g.FillEllipse(Brushes.RoyalBlue, c.Position.X, c.Position.Y, c.Width, c.Height);
                                }
                            }
                            lock (mMatch.Game.White.Creeps)
                            {
                                foreach (Creep c in mMatch.Game.White.Creeps)
                                {
                                    g.FillEllipse(Brushes.RoyalBlue, c.Position.X, c.Position.Y, c.Width, c.Height);
                                }
                            }
                        }

                        if (mShowProjectiles)
                        {
                            lock (mMatch.Game.Projectiles)
                            {
                                foreach (Projectile p in mMatch.Game.Projectiles)
                                {
                                    g.FillEllipse(Brushes.DarkTurquoise, p.Position.X, p.Position.Y, 5, 5);
                                }
                            }
                        }
                    }
                }
            }
            return image;
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
                    g.DrawString("T", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 6, c.Position.Y + 3);
            }

            // Draw Labels
            if (mShowLabels)
            {
                if (c == mMatch.Game.Board.WhiteSpawn)
                    g.DrawString("W", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 1, c.Position.Y + 3);

                if (c == mMatch.Game.Board.BlackSpawn)
                    g.DrawString("B", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 3, c.Position.Y + 3);

                if (mMatch.Game.Board.WhiteBase.Contains(c) || mMatch.Game.Board.BlackBase.Contains(c))
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

