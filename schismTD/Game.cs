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
        public GameCode Context
        {
            get
            {
                return mCtx;
            }
        }
        private GameCode mCtx;

        // Seconds to countdown at start of game (IE: dead period)
        private const int mCountdownLength = Settings.DEFAULT_GAME_COUNTDOWN * 1000; // in milliseconds
        private int mCountdownPosition;

        private int mWaveTimerLength = Settings.WAVE_LENGTH;
        private int mWaveTimerPosition;

        private long mTotalTimeElapsed = 0;

        public Board Board
        {
            get
            {
                return mBoard;
            }
            set
            {
                mBoard = value;
            }
        }
        private Board mBoard;

        public Player Black
        {
            get
            {
                return mBlack;
            }
            set
            {
                mBlack = value;
            }
        }
        private Player mBlack;

        public Player White
        {
            get
            {
                return mWhite;
            }
            set
            {
                mWhite = value;
            }
        }
        private Player mWhite;

        public Boolean Started
        {
            get
            {
                return mIsStarted;
            }
        }
        private Boolean mIsStarted = false;

        public Boolean Finished
        {
            get
            {
                return mIsFinished;
            }
        }
        private Boolean mIsFinished = false;

        public List<Projectile> Projectiles
        {
            get
            {
                return mProjectiles;
            }
            set
            {
                mProjectiles = value;
            }
        }
        private List<Projectile> mProjectiles = new List<Projectile>();

        public Game(GameCode gc, Player p1, Player p2)
        {
            mCtx = gc;
            Black = p1;
            White = p2;

            Board = new Board(Black, White);

            // Send the cells to the players
            foreach (Cell c in mBoard.BlackCells)
            {
                Black.Send(Messages.GAME_ADD_CELL, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, true);
                White.Send(Messages.GAME_ADD_CELL, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, false);
            }
            foreach (Cell c in mBoard.WhiteCells)
            {
                White.Send(Messages.GAME_ADD_CELL, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, true);
                Black.Send(Messages.GAME_ADD_CELL, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, false);
            }

            mCountdownPosition = mCountdownLength;
            mWaveTimerPosition = 0;
        }

        public void start()
        {
            
            mIsStarted = true;
            mTotalTimeElapsed = 0;

            // Reset both players
            Black.reset(mCtx, this);
            White.reset(mCtx, this);

            // Send player info
            mCtx.Broadcast(Messages.GAME_LIFE, Black.Id, Black.Life);
            mCtx.Broadcast(Messages.GAME_MANA, Black.Id, Black.Mana);

            mCtx.Broadcast(Messages.GAME_LIFE, White.Id, White.Life);
            mCtx.Broadcast(Messages.GAME_MANA, White.Id, White.Mana);

            // Setup the waves
            for (int i = 0; i < Settings.DEFAULT_NUM_WAVES; i++)
            {
                Black.Waves.Add(new Wave(mCtx, this, Black, White));
                White.Waves.Add(new Wave(mCtx, this, White, Black));
            }

            float healthMod = 1;
            foreach (Wave wave in Black.Waves)
            {
                wave.HealthModifier = healthMod;
                healthMod *= 1.5f;
            }

            healthMod = 1;
            foreach (Wave wave in White.Waves)
            {
                wave.HealthModifier = healthMod;
                healthMod *= 1.5f;
            }


            // Finally send the message to start the game
            mCtx.Broadcast(Messages.GAME_START);
            mCtx.AddMessageHandler(Messages.GAME_PLACE_TOWER, placeTower);

        }

        public void finish()
        {
            mIsFinished = true;
            mCtx.Broadcast(Messages.GAME_FINISHED);
        }

        public void update(int dt)
        {
            if (!Started)
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
                if (!Finished)
                {
                    mTotalTimeElapsed += dt;

                    // Check if anyone has died
                    if (Black.Life <= 0)
                    {
                        // White wins
                        finish();
                    }
                    if (White.Life <= 0)
                    {
                        // Black wins
                        finish();
                    }

                    // Spawn new creeps
                    mWaveTimerPosition += dt;
                    if (mWaveTimerPosition <= mWaveTimerLength)
                    {
                        lock (Black.Waves)
                        {
                            Black.Waves[0].update(dt);
                        }
                        lock (White.Waves)
                        {
                            White.Waves[0].update(dt);
                        }
                    }
                    else
                    {
                        mWaveTimerPosition = 0;
                        if(Black.Waves.Count > 0)
                            Black.Waves.RemoveAt(0);
                        if(White.Waves.Count > 0)
                            White.Waves.RemoveAt(0);

                        if (Black.Waves.Count == 0)
                        {
                            finish();
                        }
                    }

                    lock (Black.Creeps)
                    {
                        List<Creep> toRemove = new List<Creep>();
                        foreach (Creep c in Black.Creeps)
                        {
                            c.update(dt);

                            if (!c.Alive)
                            {
                                toRemove.Add(c);
                                mCtx.Broadcast(Messages.GAME_CREEP_REMOVE, c.ID);
                            }
                            else
                            {
                                if (!c.Valid)
                                {
                                    mCtx.Broadcast(Messages.GAME_CREEP_UPDATE_POSITION, c.ID, c.Center.X, c.Center.Y, c.MovingTo.Center.X, c.MovingTo.Center.Y);
                                }
                            }                            
                        }

                        foreach (Creep r in toRemove)
                        {
                            if (Black.Creeps.Contains(r))
                                Black.Creeps.Remove(r);
                        }
                    }
                    lock (White.Creeps)
                    {
                        List<Creep> toRemove = new List<Creep>();
                        foreach (Creep c in White.Creeps)
                        {
                            c.update(dt);

                            if (!c.Alive)
                            {
                                toRemove.Add(c);
                                mCtx.Broadcast(Messages.GAME_CREEP_REMOVE, c.ID);
                            }
                            else
                            {
                                if (!c.Valid)
                                {
                                    mCtx.Broadcast(Messages.GAME_CREEP_UPDATE_POSITION, c.ID, c.Center.X, c.Center.Y, c.MovingTo.Center.X, c.MovingTo.Center.Y);
                                }
                            }
                        }

                        foreach (Creep r in toRemove)
                        {
                            if (White.Creeps.Contains(r))
                                White.Creeps.Remove(r);
                        }
                    }
                    lock (White.Towers)
                    {
                        foreach (Tower t in White.Towers)
                        {
                            t.update(dt);
                        }
                    }
                    lock (Black.Towers)
                    {
                        foreach (Tower t in Black.Towers)
                        {
                            t.update(dt);
                        }
                    }
                    lock (Projectiles)
                    {
                        List<Projectile> toRemove = new List<Projectile>();
                        foreach (Projectile p in Projectiles)
                        {
                            p.update(dt);

                            if (!p.Active)
                                toRemove.Add(p);
                        }
                        foreach (Projectile p in toRemove)
                        {
                            if(Projectiles.Contains(p))
                                Projectiles.Remove(p);
                        }
                    }
                }
            }
        }

        private void invalidTower(Player p, Cell c, int x, int y)
        {
            if (c != null)
            {
                c.Passable = true;

                if (c.Up != null && c.Left != null)
                {
                    lock (c.Up.Neighbors)
                        if (c.Up.Neighbors.ContainsKey(c.Left))
                            c.Up.Neighbors[c.Left] = true;
                    lock (c.Left.Neighbors)
                        if (c.Left.Neighbors.ContainsKey(c.Up))
                            c.Left.Neighbors[c.Up] = true;
                }
                if (c.Up != null && c.Right != null)
                {
                    lock (c.Up.Neighbors)
                        if (c.Up.Neighbors.ContainsKey(c.Right))
                            c.Up.Neighbors[c.Right] = true;
                    lock (c.Right.Neighbors)
                        if (c.Right.Neighbors.ContainsKey(c.Up))
                             c.Right.Neighbors[c.Up] = true;
                }
                if (c.Down != null && c.Right != null)
                {
                    lock (c.Down.Neighbors)
                        if (c.Down.Neighbors.ContainsKey(c.Right))
                            c.Down.Neighbors[c.Right] = true;
                    lock (c.Right.Neighbors)
                        if (c.Right.Neighbors.ContainsKey(c.Down))
                            c.Right.Neighbors[c.Down] = true;
                }
                if (c.Down != null && c.Left != null)
                {
                    lock (c.Down.Neighbors)
                        if (c.Down.Neighbors.ContainsKey(c.Left))
                            c.Down.Neighbors[c.Left] = true;
                    lock (c.Left.Neighbors)
                        if (c.Left.Neighbors.ContainsKey(c.Down))
                            c.Left.Neighbors[c.Down] = true;
                }
            }

            p.Send(Messages.GAME_INVALID_TOWER, x, y);
        }

        private void placeTower(Player p, Message m)
        {
            if (Finished || !Started)
            {
                return;
            }

            Cell c = findCellByPoint(new Point(m.GetInt(0), m.GetInt(1)));

            if (c == null)
            {
                invalidTower(p, null, m.GetInt(0), m.GetInt(1));
                return;
            }

            // Check for correct player on the cell && that it is in a buildable area
            if (c.Tower == null && p == c.Player && c.Buildable && c.Passable)
            {

                // Make sure the player has enough mana
                if (p.Mana < Costs.BASIC)
                {
                    invalidTower(p, null, m.GetInt(0), m.GetInt(1));
                    return;
                }

                Player self = p;
                Player opponent = (p == White) ? Black : White;

                lock (opponent.Creeps)
                {

                    // Check to see if any creeps are on the cell
                    Dictionary<Creep, Cell> creepsIn = new Dictionary<Creep, Cell>();
                    foreach (Creep cr in opponent.Creeps)
                    {
                        if (cr.Player == p)
                            continue;

                        Cell crIn = findCellByPoint(cr.Center);
                        creepsIn.Add(cr, crIn); // Save for later
                        if (crIn == c)
                        {
                            invalidTower(p, c, m.GetInt(0), m.GetInt(1));
                            return;
                        }

                    }

                    // Set to non-passable
                    c.Passable = false;

                    // Disable all links involving this square and the diagonals that go past it
                    if (c.Up != null && c.Left != null)
                    {
                        lock(c.Up.Neighbors)
                            if (c.Up.Neighbors.ContainsKey(c.Left))
                                c.Up.Neighbors[c.Left] = false;
                        lock(c.Left.Neighbors)
                            if (c.Left.Neighbors.ContainsKey(c.Up))
                                c.Left.Neighbors[c.Up] = false;
                    }
                    if (c.Up != null && c.Right != null)
                    {
                        lock (c.Up.Neighbors)
                            if (c.Up.Neighbors.ContainsKey(c.Right))
                                c.Up.Neighbors[c.Right] = false;
                        lock(c.Right.Neighbors)
                            if (c.Right.Neighbors.ContainsKey(c.Up))
                                c.Right.Neighbors[c.Up] = false;
                    }
                    if (c.Down != null && c.Right != null)
                    {
                        lock(c.Down.Neighbors)
                            if (c.Down.Neighbors.ContainsKey(c.Right))
                                c.Down.Neighbors[c.Right] = false;
                        lock (c.Right.Neighbors)
                            if (c.Right.Neighbors.ContainsKey(c.Down))
                                c.Right.Neighbors[c.Down] = false;
                    }
                    if (c.Down != null && c.Left != null)
                    {
                        lock (c.Down.Neighbors)
                            if (c.Down.Neighbors.ContainsKey(c.Left))
                                c.Down.Neighbors[c.Left] = false;
                        lock (c.Left.Neighbors)
                            if (c.Left.Neighbors.ContainsKey(c.Down))
                                c.Left.Neighbors[c.Down] = false;
                    }


                    // Now try and find a path to make sure the maze is valid
                    Path path;
                    if (p == White)
                        path = mBoard.getWhitePath();
                    else
                        path = mBoard.getBlackPath();

                    // If there is no valid path, reject the tower placement
                    if (path.Count <= 0)
                    {
                        invalidTower(p, c, m.GetInt(0), m.GetInt(1));
                        return;
                    }
                    else
                    {
                        // Valid path from the spawn

                        // Now Recheck each existing creep's path, if any are invalid return an error
                        Dictionary<Creep, Path> tmpPaths = new Dictionary<Creep, Path>();
                        foreach (Creep cr in opponent.Creeps)
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
                                if (cr.Player == White)
                                    tmpPath = AStar.getPath(crIn, mBoard.BlackBase);
                                else
                                    tmpPath = AStar.getPath(crIn, mBoard.WhiteBase);

                                if (tmpPath.Count <= 0)
                                {
                                    invalidTower(p, c, m.GetInt(0), m.GetInt(1));
                                    return;
                                }
                                else
                                    tmpPaths.Add(cr, tmpPath);
                            }
                        }

                        // If we made it this far the tower is valid!
                        // Update the creeps paths
                        foreach (Creep cr in opponent.Creeps)
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

                        if (p == White)
                            mBoard.WhitePath = path;
                        else
                            mBoard.BlackPath = path;

                        c.Tower = new Tower(this, self, opponent, c.Position);
                        c.Buildable = false;
                        c.Passable = false;

                        // Remove neighbor links
                        foreach (KeyValuePair<Cell, Boolean> neighbor in c.Neighbors)
                        {
                            if (neighbor.Key.Neighbors.ContainsKey(c))
                            {
                                neighbor.Key.Neighbors.Remove(c);
                            }
                        }
                        
                        // Check the diagonals that go past this cell
                        // Upper left
                        if (c.Up != null && c.Left != null)
                        {
                            lock (c.Up.Neighbors)
                                if (c.Up.Neighbors.ContainsKey(c.Left))
                                    c.Up.Neighbors.Remove(c.Left);
                            lock (c.Left.Neighbors)
                                if (c.Left.Neighbors.ContainsKey(c.Up))
                                    c.Left.Neighbors.Remove(c.Up);
                        }
                        // Upper right
                        if (c.Up != null && c.Right != null)
                        {
                            lock(c.Up.Neighbors)
                                if (c.Up.Neighbors.ContainsKey(c.Right))
                                    c.Up.Neighbors.Remove(c.Right);
                            lock (c.Right.Neighbors)
                                if (c.Right.Neighbors.ContainsKey(c.Up))
                                    c.Right.Neighbors.Remove(c.Up);
                        }
                        // Lower right
                        if (c.Down != null && c.Right != null)
                        {
                            lock (c.Down.Neighbors)
                                if (c.Down.Neighbors.ContainsKey(c.Right))
                                    c.Down.Neighbors.Remove(c.Right);
                            lock(c.Right.Neighbors)
                                if (c.Right.Neighbors.ContainsKey(c.Down))
                                    c.Right.Neighbors.Remove(c.Down);
                        }
                        // Lower left
                        if (c.Down != null && c.Left != null)
                        {
                            lock(c.Down.Neighbors)
                                if (c.Down.Neighbors.ContainsKey(c.Left))
                                    c.Down.Neighbors.Remove(c.Left);
                            lock (c.Left.Neighbors)
                                if (c.Left.Neighbors.ContainsKey(c.Down))
                                    c.Left.Neighbors.Remove(c.Down);
                        }
                         

                        // Clear the cells neighbors
                        c.Neighbors.Clear();

                        // Add the tower to the player
                        lock(p.Towers)
                            p.Towers.Add(c.Tower);

                        // Take the mana away from the player
                        p.Mana -= c.Tower.Cost;

                        // Send the players the new tower information
                        mCtx.Broadcast(Messages.GAME_PLACE_TOWER, c.Index, c.Tower.Type);
                    }
                }
            }
        }

        public Cell findCellByPoint(PointF p)
        {
            if (p.X < mBoard.XOffset || p.X > mBoard.XOffset + (mBoard.Width * mBoard.CellWidth) || p.Y < mBoard.YOffset || p.Y > mBoard.YOffset + (mBoard.Height * mBoard.CellHeight))
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

    }
}
