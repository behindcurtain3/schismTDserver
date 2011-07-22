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
        private Board mBoard;
        private Player black;
        private Player white;

        // Seconds to countdown at start of game (IE: dead period)
        private const int mCountdownLength = Settings.DEFAULT_GAME_COUNTDOWN * 1000; // in milliseconds
        private int mCountdownPosition;

        private int mCreepTimerLength = 1000;
        private int mCreepTimerPosition;

        private Boolean mIsStarted = false;
        private Boolean mIsFinished = false;
        private long mTotalTimeElapsed = 0;

        public List<Creep> Creeps = new List<Creep>();

        public Game(GameCode gc, Player p1, Player p2)
        {
            mCtx = gc;
            black = p1;
            white = p2;

            mBoard = new Board(black, white);

            // Send the cells to the players
            foreach (Cell c in mBoard.BlackCells)
            {
                black.Send(Messages.GAME_ADD_CELL, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, true);
                white.Send(Messages.GAME_ADD_CELL, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, false);
            }
            foreach (Cell c in mBoard.WhiteCells)
            {
                white.Send(Messages.GAME_ADD_CELL, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, true);
                black.Send(Messages.GAME_ADD_CELL, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, false);
            }

            mCountdownPosition = mCountdownLength;
            mCreepTimerPosition = 0;
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
            mCtx.AddMessageHandler(Messages.GAME_PLACE_TOWER, placeTower);

        }

        public void finish()
        {
            mIsFinished = true;
            mCtx.Broadcast(Messages.GAME_FINISHED);
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

                    lock (Creeps)
                    {

                        mCreepTimerPosition -= dt;
                        if (mCreepTimerPosition <= 0)
                        {
                            mCreepTimerPosition = mCreepTimerLength;
                            Creeps.Add(new Creep(black, mBoard.WhiteSpawn.Position, mBoard.WhitePath, mBoard.WhiteBase));
                            Creeps.Add(new Creep(white, mBoard.BlackSpawn.Position, mBoard.BlackPath, mBoard.BlackBase));
                        }

                        List<Creep> toRemove = new List<Creep>();
                        foreach (Creep c in Creeps)
                        {
                            c.update(dt);

                            if (!c.Alive)
                                toRemove.Add(c);
                        }

                        foreach (Creep r in toRemove)
                        {
                            if (Creeps.Contains(r))
                                Creeps.Remove(r);
                        }
                    }
                }
            }
        }

        private void invalidTower(Player p, int x, int y)
        {
            p.Send(Messages.GAME_INVALID_TOWER, x, y);
        }

        private void placeTower(Player p, Message m)
        {
            if (isFinished() || !isStarted())
            {
                invalidTower(p, m.GetInt(0), m.GetInt(0));
                return;
            }

            Cell c = findCellByPoint(new Point(m.GetInt(0), m.GetInt(1)));

            if (c == null)
            {
                invalidTower(p, m.GetInt(0), m.GetInt(0));
                return;
            }

            // Check for correct player on the cell && that it is in a buildable area
            if (c.Tower == null && p == c.Player && c.Buildable)
            {
                lock (Creeps)
                {

                    // Check to see if any creeps are on the cell
                    Dictionary<Creep, Cell> creepsIn = new Dictionary<Creep, Cell>();
                    foreach (Creep cr in Creeps)
                    {
                        if (cr.Player == p)
                            continue;

                        Cell crIn = findCellByPoint(cr.Center);
                        creepsIn.Add(cr, crIn); // Save for later
                        if (crIn == c)
                        {
                            invalidTower(p, m.GetInt(0), m.GetInt(1));
                            return;
                        }

                    }

                    // Set to non-passable
                    c.Passable = false;

                    // Now try and find a path to make sure the maze is valid
                    Path path;
                    if (p == white)
                        path = mBoard.getWhitePath();
                    else
                        path = mBoard.getBlackPath();

                    // If there is no valid path, reject the tower placement
                    if (path.Count <= 0)
                    {
                        c.Passable = true;
                        p.Send(Messages.GAME_INVALID_TOWER, m.GetInt(0), m.GetInt(1));
                        return;
                    }
                    else
                    {
                        // Valid path from the spawn

                        // Now Recheck each existing creep's path, if any are invalid return an error
                        Dictionary<Creep, Path> tmpPaths = new Dictionary<Creep, Path>();
                        foreach (Creep cr in Creeps)
                        {
                            if (cr.Player == p)
                                continue;

                            Cell crIn = creepsIn[cr];

                            // We only need to recalc the path if the creeps current path contains where the tower is placed
                            // and if the path we generated doesn't contain the creeps current cell
                            if (cr.CurrentPath.Contains(c) && !path.Contains(crIn))
                            {
                                Path tmpPath;
                                // Recalc the path for this creep
                                if (cr.Player == white)
                                    tmpPath = AStar.getPath(crIn, mBoard.BlackBase);
                                else
                                    tmpPath = AStar.getPath(crIn, mBoard.WhiteBase);

                                if (tmpPath.Count <= 0)
                                {
                                    invalidTower(p, m.GetInt(0), m.GetInt(1));
                                    return;
                                }
                                else
                                    tmpPaths.Add(cr, tmpPath);
                            }
                        }

                        // If we made it this far the tower is valid!
                        // Update the creeps paths
                        foreach (Creep cr in Creeps)
                        {
                            if (cr.Player == p)
                                continue;

                            // If we recalced a path apply it
                            if (tmpPaths.ContainsKey(cr))
                            {
                                cr.CurrentPath = tmpPaths[cr];
                                cr.MovingTo = cr.CurrentPath.Peek();
                            }
                            else
                            {
                                // else, check to see if we should use the updated main path
                                if (path.Contains(creepsIn[cr]))
                                {
                                    // Reapply the main path, removing any cells the creeper has already passed
                                    cr.CurrentPath = new Path(path);

                                    while (cr.CurrentPath.Peek() != creepsIn[cr])
                                    {
                                        cr.CurrentPath.Pop();
                                    }
                                    // Pop off the one they are in, only if they aren't going to the last square
                                    if(cr.CurrentPath.Count > 1)
                                        cr.CurrentPath.Pop();

                                    cr.MovingTo = cr.CurrentPath.Peek();
                                }
                            }
                        }

                        if (p == white)
                            mBoard.WhitePath = path;
                        else
                            mBoard.BlackPath = path;

                        c.Tower = new Tower(p, c.Position);
                        c.Buildable = false;
                        c.Passable = false;

                        // Remove neighbor links
                        foreach (Cell neighbor in c.Neighbors)
                        {
                            if (neighbor.Neighbors.Contains(c))
                                neighbor.Neighbors.Remove(c);
                        }
                        c.Neighbors.Clear();

                        p.Towers.Add(c.Tower);

                        mCtx.Broadcast(Messages.GAME_PLACE_TOWER, c.Index, c.Tower.Type);
                    }
                }
            }
        }

        private void placeWall(Player p, Message m)
        {
            if (isFinished() || !isStarted())
                return;

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

                // check for winner
                if (sender.Walls.Count >= 5)
                {
                    // They win!
                    finish();
                }
            }
        }

        private void removeWall(Player p, Message message)
        {
            if (isFinished() || !isStarted())
                return;

            return;
        }

        public Cell findCellByPoint(PointF p)
        {
            if (p.X < mBoard.xOffset || p.X > mBoard.xOffset + (mBoard.Width * mBoard.CellWidth) || p.Y < mBoard.yOffset || p.Y > mBoard.yOffset + (mBoard.Height * mBoard.CellHeight))
                return null;

            Point indexed = new Point(((int)p.X - Settings.BOARD_X_OFFSET) / Settings.BOARD_CELL_WIDTH, ((int)p.Y - Settings.BOARD_Y_OFFSET) / Settings.BOARD_CELL_HEIGHT);
            int index = mBoard.getIndex(indexed.X, indexed.Y);

            if (index >= 0 && index < mBoard.Cells.Count)
                return mBoard.Cells[index];
            else
                return null;
        }

        public Cell findCellByPoint(Point p)
        {
            return findCellByPoint(new PointF(p.X, p.Y));
        }

        public Boolean isStarted()
        {
            return mIsStarted;
        }

        public Boolean isFinished()
        {
            return mIsFinished;
        }

        public Player Black()
        {
            return black;
        }

        public Player White()
        {
            return white;
        }

        public Board getBoard()
        {
            return mBoard;
        }

    }
}
