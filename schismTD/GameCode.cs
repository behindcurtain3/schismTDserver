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
        private Boolean mShowNeighbors = true;
        private Boolean mShowPassable = true;
        private Boolean mShowTowers = true;
        private Boolean mShowWalls = true;
        private Boolean mShowLabels = true;
        private Boolean mShowPaths = true;

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
                    if (mMatch.isFinished())
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

        Point debugPoint;

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

                // draw a dot based on the DebugPoint variable
                g.FillRectangle(Brushes.Red, debugPoint.X, debugPoint.Y, 5, 5);

                if (mMatch != null)
                {
                    if (mMatch.getCurrentGame() != null)
                    {
                        Brush off = Brushes.DarkGray;
                        Brush on = Brushes.White;
                        Brush current = on;

                        // Draw White Cells
                        foreach (Cell c in mMatch.getCurrentGame().getBoard().WhiteCells)
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
                        foreach (Cell c in mMatch.getCurrentGame().getBoard().BlackCells)
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

                        // Draw walls
                        if (mShowWalls)
                        {
                            foreach (Wall w in mMatch.getCurrentGame().Black().Walls)
                            {
                                g.DrawLine(Pens.Black, w.Start, w.End);
                            }

                            foreach (Wall w in mMatch.getCurrentGame().White().Walls)
                            {
                                g.DrawLine(Pens.White, w.Start, w.End);
                            }
                        }

                        // Draw paths
                        if (mShowPaths)
                        {
                            foreach (Cell p in mMatch.getCurrentGame().getBoard().BlackPath)
                            {
                                g.FillRectangle(Brushes.SandyBrown, p.Position.X + 2, p.Position.Y + 2, Settings.BOARD_CELL_WIDTH - 4, Settings.BOARD_CELL_HEIGHT - 4); 
                            }

                            foreach (Cell p in mMatch.getCurrentGame().getBoard().WhitePath)
                            {
                                g.FillRectangle(Brushes.SandyBrown, p.Position.X + 2, p.Position.Y + 2, Settings.BOARD_CELL_WIDTH - 4, Settings.BOARD_CELL_HEIGHT - 4);
                            }
                        }

                        // Draw creeps!
                        foreach (Creep c in mMatch.getCurrentGame().Creeps)
                        {
                            g.FillEllipse(Brushes.RoyalBlue, c.Position.X, c.Position.Y, c.Width, c.Height);
                        }
                    }
                }
            }
            return image;
        }

        public void drawCell(Graphics g, Brush b, Cell c)
        {
            g.FillRectangle(b, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT); 

            // Draw neighbor links
            if (mShowNeighbors)
            {
                foreach (Cell neighbor in c.Neighbors)
                {
                    g.DrawLine(Pens.Orange, c.Center, neighbor.Center);
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
                if (c == mMatch.getCurrentGame().getBoard().WhiteSpawn)
                    g.DrawString("W", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 1, c.Position.Y + 3);

                if (c == mMatch.getCurrentGame().getBoard().BlackSpawn)
                    g.DrawString("B", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 3, c.Position.Y + 3);

                if (c == mMatch.getCurrentGame().getBoard().WhiteBase || c == mMatch.getCurrentGame().getBoard().BlackBase)
                    g.DrawString("X", new Font("Verdana", 12F), Brushes.Blue, c.Position.X + 4, c.Position.Y + 3);
            }

            // Draw passable marker
            if (mShowPassable)
            {
                if (c.Passable)
                    g.FillEllipse(Brushes.Tomato, c.Position.X + 8, c.Position.Y + 8, 9, 9);
            }

        }

        // During development, it's very usefull to be able to cause certain events
        // to occur in your serverside code. If you create a public method with no
        // arguments and add a [DebugAction] attribute like we've down below, a button
        // will be added to the development server. 
        // Whenever you click the button, your code will run.
        /*
        [DebugAction("Calculate Paths", DebugAction.Icon.Play)]
        public void CalcPaths()
        {
            if (mMatch != null)
            {
                if (mMatch.isStarted())
                {
                    mMatch.getCurrentGame().getBoard().calcPaths();
                }
            }
        }
        */

        [DebugAction("Toggle Paths", DebugAction.Icon.Green)]
        public void TooglePaths()
        {
            mShowPaths = !mShowPaths;
        }

        [DebugAction("Toggle Neighbors", DebugAction.Icon.Green)]
        public void ToogleNeighbors()
        {
            mShowNeighbors = !mShowNeighbors;
        }

        [DebugAction("Toggle Passable", DebugAction.Icon.Green)]
        public void TooglePassable()
        {
            mShowPassable = !mShowPassable;
        }

        [DebugAction("Toggle Towers", DebugAction.Icon.Green)]
        public void ToogleTowers()
        {
            mShowTowers = !mShowTowers;
        }

        [DebugAction("Toggle Labels", DebugAction.Icon.Green)]
        public void ToogleLabels()
        {
            mShowLabels = !mShowLabels;
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

