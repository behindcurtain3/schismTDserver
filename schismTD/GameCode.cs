﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using PlayerIO.GameLibrary;
using System.Drawing;

namespace schismTD
{
    public class Player : BasePlayer
    {
        public string Name;

        // Game data
        public int Mana = Settings.DEFAULT_MANA;
        public int Life = Settings.DEFAULT_LIFE;

        public List<Tower> Towers = new List<Tower>();
        public List<Wall> Walls = new List<Wall>();

        public void reset()
        {
            Mana = Settings.DEFAULT_MANA;
            Life = Settings.DEFAULT_LIFE;

            Towers.Clear();
            Walls.Clear();
        }
    }

    [RoomType("schismTD")]
    public class GameCode : Game<Player>
    {
        public Match mMatch;

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
                }
            }, 40);

            // Debug Example:
            // Sometimes, it can be very usefull to have a graphical representation
            // of the state of your game.
            // An easy way to accomplish this is to setup a timer to update the
            // debug view every 250th second (4 times a second).
            AddTimer(delegate
            {
                // This will cause the GenerateDebugImage() method to be called
                // so you can draw a grapical version of the game state.
                RefreshDebugView();
            }, 250);
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
            var image = new Bitmap(400, 400);
            using (var g = Graphics.FromImage(image))
            {
                // fill the background
                g.FillRectangle(Brushes.Blue, 0, 0, image.Width, image.Height);

                // draw the current time
                g.DrawString(DateTime.Now.ToString(), new Font("Verdana", 20F), Brushes.Orange, 10, 10);

                // draw a dot based on the DebugPoint variable
                g.FillRectangle(Brushes.Red, debugPoint.X, debugPoint.Y, 5, 5);
            }
            return image;
        }

        // During development, it's very usefull to be able to cause certain events
        // to occur in your serverside code. If you create a public method with no
        // arguments and add a [DebugAction] attribute like we've down below, a button
        // will be added to the development server. 
        // Whenever you click the button, your code will run.
        [DebugAction("Play", DebugAction.Icon.Play)]
        public void PlayNow()
        {
            Console.WriteLine("The play button was clicked!");
        }

        // If you use the [DebugAction] attribute on a method with
        // two int arguments, the action will be triggered via the
        // debug view when you click the debug view on a running game.
        [DebugAction("Set Debug Point", DebugAction.Icon.Green)]
        public void SetDebugPoint(int x, int y)
        {
            debugPoint = new Point(x, y);
        }
    }
}

