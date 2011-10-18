using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using schismTD.Logic;
using System.Net;
using PlayerIO.GameLibrary;

namespace schismTD
{
    public class Game
    {
        public Random RandomGen = new Random(DateTime.Now.Millisecond);

        public GameCode Context
        {
            get
            {
                return mCtx;
            }
        }
        private GameCode mCtx;

        public GameStats Stats
        {
            get;
            set;
        }

        public int CurrentWaveNum
        {
            get;
            set;
        }

        // Seconds to countdown at start of game (IE: dead period)
        private const long mCountdownLength = Settings.DEFAULT_GAME_COUNTDOWN * 1000; // in milliseconds
        private long mCountdownPosition;

        private long mWaveTimerLength = Settings.WAVE_LENGTH;
        private long mWaveTimerPosition;

        private long mTotalTimeElapsed = 0;
        private Boolean mIsGameSetup = false;
        private Boolean mIsUpdating = false;

        private Dictionary<int, List<Creep>> creepsThatNeedPaths;

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
            get;
            set;
        }

        public Player White
        {
            get;
            set;
        }

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
        // Waiting to finish is true after all waves have been ran but creeps are still alive
        private Boolean mWaitingToFinish = false;

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

        private Player mWinner;
        private Player mLoser;

        public Boolean Ready
        {
            get;
            set;
        }

        public Boolean Init
        {
            get;
            set;
        }

        public Game(GameCode gc, Player p1, Player p2)
        {
            Init = false;
            mCtx = gc;
            Stats = new GameStats();
            Black = p1;
            White = p2;

            CurrentWaveNum = 0;

            Black.Opponent = White;
            White.Opponent = Black;

            Board = new Board(Black, White);

            // Send the cells to the players
            foreach (Cell c in mBoard.BlackCells)
            {
                if (c.Index == Settings.DEFAULT_BLACK_SPAWN)
                    continue;

                Boolean owner = mBoard.BlackBase.Contains(c) ? false : true;

                Black.Send(Messages.GAME_CELL_ADD, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, owner);
                White.Send(Messages.GAME_CELL_ADD, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, false);
            }
            foreach (Cell c in mBoard.WhiteCells)
            {
                if (c.Index == Settings.DEFAULT_WHITE_SPAWN)
                    continue;

                Boolean owner = mBoard.WhiteBase.Contains(c) ? false : true;

                White.Send(Messages.GAME_CELL_ADD, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, owner);
                Black.Send(Messages.GAME_CELL_ADD, c.Index, c.Position.X, c.Position.Y, Settings.BOARD_CELL_WIDTH, Settings.BOARD_CELL_HEIGHT, false);
            }

            mCountdownPosition = mCountdownLength;
            mWaveTimerPosition = 0;

            Ready = false;
            Init = true;
            mCtx.Broadcast(Messages.CHAT, "Init complete");
            Black.Send(Messages.GAME_INFO, "black", Black.Id, White.Id);
            White.Send(Messages.GAME_INFO, "white", White.Id, Black.Id);
            mCtx.Broadcast(Messages.GAME_USER_INFO, Black.Name, White.Name);
        }

        public void setup()
        {
            if (Finished)
                return;

            mIsGameSetup = true;

            mCtx.Broadcast(Messages.CHAT, "Setting up game");

            // Reset both players
            Black.reset(this);
            White.reset(this);

            // Setup the waves
            for (int i = 0; i < Settings.DEFAULT_NUM_WAVES; i++)
            {
                Wave w = new Wave(mCtx, this, Black, White);
                w.setup(i + 1);

                lock(Black.QueuedWaves)
                    Black.QueuedWaves.Enqueue(w);

                w = new Wave(mCtx, this, White, Black);
                w.setup(i + 1);

                lock(White.QueuedWaves)
                    White.QueuedWaves.Enqueue(w);
            }

            lock (Black.OnDeckWaves)
            {
                lock (Black.QueuedWaves)
                {
                    Black.OnDeckWaves.Add(Black.QueuedWaves.Dequeue());
                    Black.OnDeckWaves.Add(Black.QueuedWaves.Dequeue());
                    Black.OnDeckWaves.Add(Black.QueuedWaves.Dequeue());
                }

                foreach (Wave w in Black.OnDeckWaves)
                {
                    w.queueClient();
                }
            }

            lock (White.OnDeckWaves)
            {
                lock (White.QueuedWaves)
                {
                    White.OnDeckWaves.Add(White.QueuedWaves.Dequeue());
                    White.OnDeckWaves.Add(White.QueuedWaves.Dequeue());
                    White.OnDeckWaves.Add(White.QueuedWaves.Dequeue());
                }

                foreach (Wave w in White.OnDeckWaves)
                {
                    w.queueClient();
                }
            }

            // synch the paths
            sendUpdatedPath(Black, Board.BlackPath);
            sendUpdatedPath(White, Board.WhitePath);

            creepsThatNeedPaths = new Dictionary<int, List<Creep>>();

            mCtx.AddMessageHandler(Messages.GAME_TOWER_PLACE, placeTower);
            mCtx.AddMessageHandler(Messages.GAME_TOWER_UPGRADE, upgradeTower);
            mCtx.AddMessageHandler(Messages.GAME_TOWER_SELL, sellTower);
            mCtx.AddMessageHandler(Messages.GAME_WAVE_NEXT, setNextWave);
            mCtx.AddMessageHandler(Messages.GAME_FIRE_AT, setFireAt);
            mCtx.AddMessageHandler(Messages.GAME_SPELL_CREEP, spellCreep);
            mCtx.AddMessageHandler(Messages.GAME_SPELL_TOWER, spellTower);
            mCtx.Broadcast(Messages.GAME_ACTIVATE);

            Black.Send(Messages.GAME_SET_SPAWN, Board.BlackSpawn.Center.X, Board.BlackSpawn.Center.Y + Settings.BOARD_CELL_HEIGHT * 1.5);
            White.Send(Messages.GAME_SET_SPAWN, Board.WhiteSpawn.Center.X, Board.WhiteSpawn.Center.Y - Settings.BOARD_CELL_HEIGHT * 1.5);

            mCtx.Broadcast(Messages.CHAT, "Setup complete");
        }

        public void start()
        {
            if (Finished)
                return;

            mIsStarted = true;
            mTotalTimeElapsed = 0;
            mWaveTimerPosition = mWaveTimerLength + 1;
            mCtx.Broadcast(Messages.GAME_START);
            mCtx.Broadcast(Messages.CHAT, "Game is starting!");
        }

        public void finish()
        {
            if (Finished)
                return;

            mIsFinished = true;

            int gameResult;

            // Score the game
            if (Black.Life == White.Life)
            {
                // Draw
                gameResult = Result.DRAW;
            }
            else if (Black.Life < White.Life)
            {
                // White Wins
                gameResult = Result.WIN;
                mWinner = White;
                mLoser = Black;
            }
            else
            {
                // Black wins
                gameResult = Result.WIN;
                mWinner = Black;
                mLoser = White;
            }

            if (gameResult == Result.DRAW)
                updatePlayerObjects(true);
            else
                updatePlayerObjects();

            if (gameResult != Result.DRAW)
            {
                mCtx.Broadcast(Messages.GAME_FINISHED, mWinner.Id, Black.Life, White.Life, Black.DamageDealt, White.DamageDealt, Black.PlayerObject.GetDouble("rating"), White.PlayerObject.GetDouble("rating"), "");
            }
            else
            {
                mCtx.Broadcast(Messages.GAME_FINISHED, -1, Black.Life, White.Life, Black.DamageDealt, White.DamageDealt, Black.PlayerObject.GetDouble("rating"), White.PlayerObject.GetDouble("rating"), "");
            }
            updateStats();
        }

        public void update(long dt)
        {
            if (mIsUpdating)
                return;

            mIsUpdating = true;

            if (!Ready)
            {
                if (Black != null && White != null)
                {
                    Ready = true;
                }
                mIsUpdating = false;
                return;
            }

            if (!Started && !Finished)
            {
                mCountdownPosition -= dt;

                // Run setup() after counting down for one second
                if (!mIsGameSetup)
                {
                    if (mCountdownPosition < Settings.DEFAULT_GAME_COUNTDOWN * 1000 - 7000)
                    {
                        setup();
                    }
                }

                if (mCountdownPosition <= 0)
                {
                    // Start the game
                    start();
                }
                else
                {
                    // Update countdown timer
                    mCtx.Broadcast(Messages.GAME_COUNTDOWN, (int)mCountdownPosition);
                }
            }
            else
            {
                // Game has started
                if (!Finished)
                {
                    mTotalTimeElapsed += dt;

                    // Check if we are waiting to finish
                    if (mWaitingToFinish)
                    {
                        if (Black.Creeps.Count == 0 && White.Creeps.Count == 0)
                        {
                            finish();
                            mIsUpdating = false;
                            return;
                        }
                    }

                    // Check if anyone has died
                    if (Black.Life <= 0 || White.Life <= 0)
                    {
                        // White wins
                        finish();
                        mIsUpdating = false;
                        return;
                    }

                    // Spawn new creeps
                    mWaveTimerPosition += dt;

                    lock (Black.ActiveWaves)
                    {
                        foreach (Wave w in Black.ActiveWaves)
                            w.update(dt);

                        Black.ActiveWaves.RemoveAll(delegate(Wave w)
                        {
                            if (w.Finished)
                                w.removeClient();
                            return w.Finished;
                        });
                    }
                    lock (White.ActiveWaves)
                    {
                        foreach (Wave w in White.ActiveWaves)
                            w.update(dt);

                        White.ActiveWaves.RemoveAll(delegate(Wave w)
                        {
                            if (w.Finished)
                                w.removeClient();
                            return w.Finished;
                        });
                    }

                    // send next wave if:
                    // 1. wave timer is ready
                    // 2. player has finished a wave
                    // AND not waiting to finish
                    if ((mWaveTimerPosition >= mWaveTimerLength || (Black.ActiveWaves.Count == 0 && Black.Creeps.Count == 0) || (White.ActiveWaves.Count == 0 && White.Creeps.Count == 0)) && !mWaitingToFinish)
                    {
                        mWaveTimerPosition = 0;

                        // Update the waves
                        if (Black.OnDeckWaves.Count == 0 && White.OnDeckWaves.Count == 0)
                            mWaitingToFinish = true;
                        else
                        {
                            CurrentWaveNum++;
                            // Remove wave from OnDeckWaves w/ position = 0 & add to activewaves
                            // Decrement position on remaining OnDeckWaves
                            // Decrement position on remaining QueuedWaves
                            // Remove lowest position QueuedWave and add to OnDeckWaves
                            Wave nextWave = null;
                            int pos = Black.WavePosition;

                            while (nextWave == null && Black.WavePosition >= 0)
                            {
                                nextWave = Black.OnDeckWaves.Find(delegate(Wave w)
                                {
                                    return w.Position == Black.WavePosition;
                                });

                                if (nextWave == null && Black.WavePosition > 0)
                                    Black.WavePosition--;
                            }

                            if (nextWave != null)
                            {
                                if(pos != Black.WavePosition)
                                    Black.Send(Messages.GAME_WAVE_POSITION, Black.WavePosition);

                                lock (Black.ActiveWaves)
                                {
                                    Black.ActiveWaves.Add(nextWave);
                                    nextWave.Number = CurrentWaveNum;
                                    nextWave.activateClient();
                                }
                                lock (Black.OnDeckWaves)
                                {
                                    Black.OnDeckWaves.Remove(nextWave);

                                    foreach (Wave w in Black.OnDeckWaves)
                                    {
                                        if(w.Position > Black.WavePosition)
                                            w.Position--;
                                    }
                                }

                                if (Black.QueuedWaves.Count > 0)
                                {
                                    lock (Black.QueuedWaves)
                                    {
                                        foreach (Wave w in Black.QueuedWaves)
                                            w.Position--;

                                        lock (Black.OnDeckWaves)
                                        {
                                            Wave newOnDeckWave = Black.QueuedWaves.Dequeue();
                                            newOnDeckWave.queueClient();
                                            Black.OnDeckWaves.Add(newOnDeckWave);
                                        }
                                    }
                                }

                                lock (Black.OnDeckWaves)
                                {
                                    foreach (Wave w in Black.OnDeckWaves)
                                        w.queueClient();
                                }

                            }

                            // WHITE WAVES
                            nextWave = null;
                            pos = White.WavePosition;

                            while (nextWave == null && White.WavePosition >= 0)
                            {
                                nextWave = White.OnDeckWaves.Find(delegate(Wave w)
                                {
                                    return w.Position == White.WavePosition;
                                });

                                if (nextWave == null && White.WavePosition > 0)
                                    White.WavePosition--;
                            }

                            if (nextWave != null)
                            {
                                if(pos != White.WavePosition)
                                    White.Send(Messages.GAME_WAVE_POSITION, White.WavePosition);

                                lock (White.ActiveWaves)
                                {
                                    White.ActiveWaves.Add(nextWave);
                                    nextWave.Number = CurrentWaveNum;
                                    nextWave.activateClient();
                                }
                                lock (White.OnDeckWaves)
                                {
                                    White.OnDeckWaves.Remove(nextWave);

                                    foreach (Wave w in White.OnDeckWaves)
                                    {
                                        if(w.Position > White.WavePosition)
                                            w.Position--;
                                    }
                                }

                                if (White.QueuedWaves.Count > 0)
                                {
                                    lock (White.QueuedWaves)
                                    {
                                        foreach (Wave w in White.QueuedWaves)
                                            w.Position--;

                                        lock (White.OnDeckWaves)
                                        {
                                            Wave newOnDeckWave = White.QueuedWaves.Dequeue();
                                            newOnDeckWave.queueClient();
                                            White.OnDeckWaves.Add(newOnDeckWave);
                                        }
                                    }
                                }

                                lock (White.OnDeckWaves)
                                {
                                    foreach (Wave w in White.OnDeckWaves)
                                        w.queueClient();
                                }
                            }
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
                            {
                                toRemove.Add(p);
                            }
                        }
                        foreach (Projectile p in toRemove)
                        {
                            if(Projectiles.Contains(p))
                                Projectiles.Remove(p);
                        }
                    }
                }
            }
            mIsUpdating = false;
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

            p.Send(Messages.GAME_TOWER_INVALID, x, y);
        }

        private void placeTower(Player p, Message m)
        {
            if (Finished || !mIsGameSetup)
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

                Dictionary<Creep, Cell> creepsInTheseCells = new Dictionary<Creep, Cell>();
                lock (opponent.Creeps)
                {
                    // Check to see if any creeps are on the cell
                    foreach (Creep cr in opponent.Creeps)
                    {
                        if (cr.Player == p)
                            continue;

                        Cell crIn = findCellByPoint(cr.Center);
                        creepsInTheseCells.Add(cr, crIn); // Save for later

                        // If a creep is in the cell we are trying to place a tower at don't let them
                        if (crIn == c)
                        {
                            invalidTower(p, c, m.GetInt(0), m.GetInt(1));
                            return;
                        }

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
                Path newPath;
                if (p == White)
                    newPath = mBoard.getWhitePath();
                else
                    newPath = mBoard.getBlackPath();

                // If there is no valid path, reject the tower placement
                if (newPath.Count <= 0)
                {
                    invalidTower(p, c, m.GetInt(0), m.GetInt(1));
                    return;
                }
                else
                {
                    // Valid path from the spawn
                    Dictionary<Creep, Path> tmpPaths = new Dictionary<Creep, Path>();

                    lock(opponent.Creeps)
                    {
                        // Now Recheck each existing creep's path, if any are invalid return an error
                        foreach (Creep cr in opponent.Creeps)
                        {
                            if (cr.Player == p || !creepsInTheseCells.ContainsKey(cr))
                                continue;

                            Cell crIn = creepsInTheseCells[cr];

                            // We only need to recalc the path if the creeps current path contains where the tower is placed
                            // and if the path we generated doesn't contain the creeps current cell
                            if (!newPath.Contains(crIn))
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
                    }

                    lock(opponent.Creeps)
                    {
                        // If we made it this far the tower is valid!
                        // Update the creeps paths
                        foreach (Creep cr in opponent.Creeps)
                        {
                            if (cr.Player == p)
                                continue;

                            // If we recalced a path apply it
                            if (tmpPaths.ContainsKey(cr))
                            {
                                lock(cr.CurrentPath)
                                    cr.CurrentPath = tmpPaths[cr];
                                cr.MovingTo = cr.CurrentPath.Peek();
                            }
                            else
                            {
                                if (!creepsInTheseCells.ContainsKey(cr))
                                {
                                    creepsInTheseCells.Add(cr, findCellByPoint(cr.Center));
                                }

                                // else, check to see if we should use the updated main path
                                if (newPath.Contains(creepsInTheseCells[cr]))
                                {
                                    lock (cr.CurrentPath)
                                    {
                                        // Reapply the main path, removing any cells the creeper has already passed
                                        cr.CurrentPath = new Path(newPath);

                                        while (cr.CurrentPath.Peek() != creepsInTheseCells[cr])
                                        {
                                            cr.CurrentPath.Pop();
                                        }
                                        // Pop off the one they are in, only if they aren't going to the last square
                                        if (cr.CurrentPath.Count > 1)
                                            cr.CurrentPath.Pop();
                                    }

                                    cr.MovingTo = cr.CurrentPath.Peek();
                                }
                            }
                        }
                    }

                    if (p == White)
                        mBoard.WhitePath = newPath;
                    else
                        mBoard.BlackPath = newPath;

                    sendUpdatedPath(p, newPath);

                    c.Tower = new Tower(this, self, opponent, c.Position);
                    c.Buildable = false;
                    c.Passable = false;

                    lock(c.Neighbors)
                    {
                        // Remove neighbor links
                        foreach (KeyValuePair<Cell, Boolean> neighbor in c.Neighbors)
                        {
                            if (neighbor.Key.Neighbors.ContainsKey(c))
                            {
                                neighbor.Key.Neighbors.Remove(c);
                            }
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
                         
                    lock(c.Neighbors)
                    {
                        // Clear the cells neighbors
                        c.Neighbors.Clear();
                    }

                    // Take the mana away from the player
                    p.Mana -= c.Tower.Cost;

                    // Add the tower to the player
                    addTower(p, c);
                    
                }
            }
        }

        public void sendUpdatedPath(Player p, Path path)
        {
            // Update the cached path all new creeps take
            Message msg = Message.Create(Messages.GAME_ALL_CREEPS_PATH);
            msg.Add(p.Id);

            foreach (Cell c in path)
            {
                msg.Add(c.Index);
            }

            mCtx.Broadcast(msg);

            // Now update the path for each creep that is already out there
            if (p == Black)
            {
                lock (White.Creeps)
                {
                    foreach (Creep creep in White.Creeps)
                    {
                        msg = Message.Create(Messages.GAME_CREEP_PATH);
                        msg.Add(creep.ID);

                        lock (creep.CurrentPath)
                        {
                            foreach (Cell c in creep.CurrentPath)
                            {
                                msg.Add(c.Index);
                            }
                        }
                        mCtx.Broadcast(msg);
                    }
                }
            }
            else if (p == White)
            {
                lock (Black.Creeps)
                {
                    foreach (Creep creep in Black.Creeps)
                    {
                        msg = Message.Create(Messages.GAME_CREEP_PATH);
                        msg.Add(creep.ID);

                        lock (creep.CurrentPath)
                        {
                            foreach (Cell c in creep.CurrentPath)
                            {
                                msg.Add(c.Index);
                            }
                        }
                        mCtx.Broadcast(msg);
                    }
                }
            }
        }

        /*
         * You can ALWAYS sell a tower unless it is stunned. Therefore there is no need to check things like are the paths valid,
         * just recalculate them as it is impossible to stop creeps getting to the base by selling.
         */
        public void sellTower(Player p, Message m)
        {
            if (Finished || !mIsGameSetup)
                return;

            Cell c = findCellByPoint(new Point(m.GetInt(0), m.GetInt(1)));

            if (c == null)
                return;

            if (c.Tower == null)
                return;

            if (c.Tower.hasEffect("stun"))
                return;

            // Update the players mana
            p.Mana += c.Tower.SellValue;

            // Remove the tower
            removeTower(p, c, true);
            c.Tower = null;

            // Update the board
            c.Passable = true;
            c.Buildable = true;

            // Add neighbors
            Board.setupCellNeighbors(c);

            // Recalc the paths
            List<Creep> creepsToRePath = new List<Creep>();
            List<Cell> targetBase = new List<Cell>();
            Boolean recalcCreeps = false;
            Path newPath;
            if (p == White)
            {
                newPath = mBoard.getWhitePath();

                if (newPath != Board.WhitePath)
                {
                    recalcCreeps = true;
                    creepsToRePath = Black.Creeps;
                    targetBase = Board.WhiteBase;
                    Board.WhitePath = newPath;
                }
            }
            else
            {
                newPath = mBoard.getBlackPath();

                if (newPath != Board.BlackPath)
                {
                    recalcCreeps = true;
                    creepsToRePath = White.Creeps;
                    targetBase = Board.BlackBase;
                    Board.BlackPath = newPath;
                }
            }

            if (recalcCreeps)
            {
                lock (creepsToRePath)
                {
                    foreach (Creep cr in creepsToRePath)
                    {
                        Cell crIn = findCellByPoint(cr.Center);

                        // else, check to see if we should use the updated main path
                        if (newPath.Contains(crIn))
                        {
                            lock (cr.CurrentPath)
                            {
                                // Reapply the main path, removing any cells the creeper has already passed
                                cr.CurrentPath = new Path(newPath);

                                while (cr.CurrentPath.Peek() != crIn)
                                {
                                    cr.CurrentPath.Pop();
                                }
                                // Pop off the one they are in, only if they aren't going to the last square
                                if (cr.CurrentPath.Count > 1)
                                    cr.CurrentPath.Pop();
                            }

                            cr.MovingTo = cr.CurrentPath.Peek();
                        }
                        else
                        {
                            lock (creepsThatNeedPaths)
                            {
                                if (creepsThatNeedPaths.ContainsKey(crIn.Index))
                                    creepsThatNeedPaths[crIn.Index].Add(cr);
                                else
                                {
                                    creepsThatNeedPaths.Add(crIn.Index, new List<Creep>());
                                    creepsThatNeedPaths[crIn.Index].Add(cr);

                                    // schedule a call back for each cell
                                    mCtx.ScheduleCallback(pathCreeps, 25);
                                }
                            }
                        }
                    }
                }
            }

            sendUpdatedPath(p, newPath);
            
        }

        public void pathCreeps()
        {
            int index;
            Cell start;
            List<Creep> creeps;
            lock (creepsThatNeedPaths)
            {
                index = creepsThatNeedPaths.Keys.First<int>();
                start = Board.Cells[index];
                
                creeps = creepsThatNeedPaths[index];
                creepsThatNeedPaths.Remove(index);
            }

            Console.WriteLine("Calculating for " + start.Index + " with " + creeps.Count +  " creeps.");
            Path p;
            if(start.Player == Black)
                p = AStar.getPath(start, Board.BlackBase);
            else
                p = AStar.getPath(start, Board.WhiteBase);

            foreach (Creep cr in creeps)
            {
                lock (cr.CurrentPath)
                {
                    cr.CurrentPath = new Path(p);
                    if (cr.CurrentPath.Count == 0)
                        cr.CurrentPath.Push(start);
                }

                cr.MovingTo = cr.CurrentPath.Peek();
                cr.updateClientPath();
            }
        }

        public void upgradeTower(Player p, Message m)
        {
            if (Finished || !mIsGameSetup)
                return;

            Cell c = findCellByPoint(new Point(m.GetInt(0), m.GetInt(1)));

            if (c == null)
                return;

            if (c.Tower == null)
                return;

            if (c.Tower.hasEffect("stun"))
                return;

            int choice = m.GetInt(2);

            if (choice != 1 && choice != 2)
                return;

            switch (c.Tower.Type)
            {
                // Tier 2 towers
                case Tower.BASIC: // Upgrading from tier 1
                    if (choice == 1)
                    {
                        if (p.Mana < Costs.RAPID_FIRE)
                            return;

                        // Remove the old tower
                        removeTower(p, c);

                        p.Mana -= Costs.RAPID_FIRE;
                        lock (c.Tower)
                            c.Tower = new RapidFireTower(this, p, c.Player.Opponent, c.Position);

                        addTower(p, c);
                    }
                    else if(choice == 2)
                    {
                        if (p.Mana < Costs.SPELL)
                            return;

                        // Remove the old tower
                        removeTower(p, c);

                        p.Mana -= Costs.SPELL;
                        lock (c.Tower)
                            c.Tower = new SpellTower(this, p, c.Player.Opponent, c.Position);

                        addTower(p, c);
                    }
                    break;
                case Tower.RAPID_FIRE:
                    // Sniper
                    if (choice == 1)
                    {
                        if (p.Mana < Costs.SNIPER)
                            return;

                        removeTower(p, c);
                        p.Mana -= Costs.SNIPER;

                        lock (c.Tower)
                            c.Tower = new SniperTower(this, p, c.Player.Opponent, c.Position);

                        addTower(p, c);
                    }
                    else if (choice == 2)
                    {
                        if (p.Mana < Costs.PULSE)
                            return;

                        removeTower(p, c);
                        p.Mana -= Costs.PULSE;

                        lock (c.Tower)
                            c.Tower = new PulseTower(this, p, c.Player.Opponent, c.Position);

                        addTower(p, c);
                    }
                    break;
                case Tower.SPELL:
                    // AOE Slow
                    if (choice == 1)
                    {
                        if (p.Mana < Costs.SLOW)
                            return;

                        removeTower(p, c);
                        p.Mana -= Costs.SLOW;

                        lock (c.Tower)
                            c.Tower = new AOESlowTower(this, p, c.Player.Opponent, c.Position);

                        addTower(p, c);
                    }
                    // Damage Boost
                    else if (choice == 2)
                    {
                        if (p.Mana < Costs.DAMAGE_BOOST)
                            return;

                        removeTower(p, c);
                        p.Mana -= Costs.DAMAGE_BOOST;

                        lock (c.Tower)
                            c.Tower = new DamageBoostTower(this, p, c.Player.Opponent, c.Position);

                        addTower(p, c);
                    }
                    break;
                case Tower.DAMAGE_BOOST:
                    // Range boost
                    if (choice == 1)
                    {
                        if (p.Mana < Costs.RANGE_BOOST)
                            return;

                        removeTower(p, c);
                        p.Mana -= Costs.RANGE_BOOST;

                        lock (c.Tower)
                            c.Tower = new RangeBoostTower(this, p, c.Player.Opponent, c.Position);

                        addTower(p, c);
                    }
                    // Fire rate boost
                    else if (choice == 2)
                    {
                        if (p.Mana < Costs.RATE_BOOST)
                            return;

                        removeTower(p, c);
                        p.Mana -= Costs.RATE_BOOST;

                        lock (c.Tower)
                            c.Tower = new FireRateBoostTower(this, p, c.Player.Opponent, c.Position);

                        addTower(p, c);
                    }
                    break;
            }            
        }

        public void setNextWave(Player p, Message m)
        {
            if (!mIsGameSetup || p.OnDeckWaves.Count <= 0 || Finished)
                return;

            int id = m.GetInt(0);
            Console.WriteLine("Setting next wave to: " + id);

            if (id < 0 || id > 2)
                return;

            Wave result;
            lock (p.OnDeckWaves)
            {
                result = p.OnDeckWaves.Find(delegate(Wave w)
                {
                    return w.Position == id;
                });
            }

            if (result != null)
            {
                p.WavePosition = id;
                p.Send(Messages.GAME_WAVE_POSITION, p.WavePosition);
            }

        }

        public void setFireAt(Player p, Message m)
        {
            if (!Started || Finished)
                return;

            String id = m.GetString(0);

            Creep result;
            lock (p.Opponent.Creeps)
            {
                result = p.Opponent.Creeps.Find(delegate(Creep c)
                {
                    return c.ID == id;
                });
            }

            String oldId = "";
            if (result != null)
            {
                lock (p.Towers)
                {
                    foreach (Tower t in p.Towers)
                    {
                        if (t.Type == Tower.SNIPER)
                        {
                            if(t.StaticTarget != null)
                                oldId = t.StaticTarget.ID;
                            t.StaticTarget = result;
                        }
                    }
                }
            }
            if (oldId != "")
                mCtx.Broadcast(Messages.GAME_FIRE_REMOVE, oldId);
        }

        public void spellCreep(Player p, Message m)
        {
            if (!Started || Finished)
                return;

            if (p.Mana < p.ChiBlastCost)
                return;

            if (m.Count != 2)
                return;

            PointF position = new PointF(m.GetInt(0), m.GetInt(1));

            Cell c = findCellByPoint(position);

            if (c != null)
            {
                List<Creep> targets;
                if (c.Player == p)
                    targets = p.Opponent.Creeps;
                else
                    targets = p.Creeps;

                lock (targets)
                {
                    Boolean isMagicCreepPresent = false;
                    List<Creep> magicCreeps = targets.FindAll(delegate(Creep creep) { return creep.Type == "Magic"; });

                    foreach(Creep cr in magicCreeps)
                    {
                        if (cr.getDistance(position) <= Settings.CHI_BLAST_RANGE)
                        {
                            isMagicCreepPresent = true;
                            break;
                        }
                    }

                    //float percent =  Settings.CHI_BLAST_PERCENT + p.ChiBlastUses * 2 / 100;
                    float percent;
                    foreach (Creep cr in targets)
                    {
                        float d = cr.getDistance(position);

                        if(cr is SwarmCreep)
                            percent = (cr.StartingLife * 3) * ((Settings.CHI_BLAST_PERCENT + ((p.ChiBlastUses * 7) / 100)));// - (cr.Wave * 10 / 100));
                        else
                            percent = (cr.StartingLife / cr.Points) * ((Settings.CHI_BLAST_PERCENT + ((p.ChiBlastUses * 7) / 100)));// - (cr.Wave * 5 / 100));

                        if (percent <= 0)
                            percent = 0;

                        if (d <= Settings.CHI_BLAST_RANGE)
                        {
                            // heal
                            if (cr.Player == p)
                            {
                                int life = cr.Life;
                                
                                life += (int)percent;

                                if (life > cr.StartingLife)
                                    life = cr.StartingLife;

                                cr.Life = life;
                            }

                            // damage
                            else
                            {
                                if(isMagicCreepPresent)
                                    cr.Life -= (int)(percent - (percent * 0.5f));
                                else
                                    cr.Life -= (int)percent;
                            }
                        }
                    }
                }
                
                p.Mana -= (int)Math.Round(p.ChiBlastCost);
                p.ChiBlastCost *= Settings.CHI_BLAST_MOD;
                p.ChiBlastUses++;
                mCtx.Broadcast(Messages.GAME_SPELL_CREEP, position.X, position.Y);
            }
        }

        public void spellTower(Player p, Message m)
        {
            if (!Started || Finished)
                return;

            if (p.Mana < p.ChiBlastCost)
                return;

            int index = m.GetInt(0);
            Cell c;
            lock(Board.Cells)
                c = Board.Cells.Find(delegate(Cell cell) { return cell.Index == index; });

            if (c != null)
            {
                if (c.Player == p)
                    return;

                if (c.Tower != null)
                {
                    c.Tower.addEffect(new StunEffect(c.Tower, Settings.CHI_BLAST_DURATION + (int)(p.ChiBlastUses * 400)));
                    mCtx.Broadcast(Messages.GAME_SPELL_TOWER, c.Index);

                    p.Mana -= (int)Math.Round(p.ChiBlastCost);
                    p.ChiBlastCost *= Settings.CHI_BLAST_MOD;
                    p.ChiBlastUses++;
                }
            }
        }

        public void removeTower(Player p, Cell c, Boolean update = false)
        {
            c.Tower.onRemoved(c);

            lock (p.Towers)
                p.Towers.Remove(c.Tower);

            if(update)
                mCtx.Broadcast(Messages.GAME_TOWER_REMOVE, c.Index);
        }

        public void addTower(Player p, Cell c)
        {
            c.Tower.onPlaced(c);

            lock (p.Towers)
                p.Towers.Add(c.Tower);

            mCtx.Broadcast(Messages.GAME_TOWER_PLACE, c.Index, c.Tower.Type, c.Tower.Range);

            // update stats
            switch (c.Tower.Type)
            {
                case Tower.BASIC:
                    Stats.Basic++;
                    break;
                case Tower.RAPID_FIRE:
                    Stats.RapidFire++;
                    break;
                case Tower.SNIPER:
                    Stats.Sniper++;
                    break;
                case Tower.PULSE:
                    Stats.Pulse++;
                    break;
                case Tower.SLOW:
                    Stats.Slow++;
                    break;
                case Tower.SPELL:
                    Stats.Spell++;
                    break;
                case Tower.DAMAGE_BOOST:
                    Stats.DamageBoost++;
                    break;
                case Tower.RANGE_BOOST:
                    Stats.RangeBoost++;
                    break;
                case Tower.RATE_BOOST:
                    Stats.FireRateBoost++;
                    break;
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


        public void finishEarly(Player player)
        {
            String msg;
            if(player == Black)
            {
                mWinner = White;
                mLoser = Black;
                msg = "Black has left the game.";
            }
            else
            {
                mWinner = Black;
                mLoser = White;
                msg = "White has left the game.";
            }
            mLoser.Life = 0;

            updatePlayerObjects();

            mIsFinished = true;
            mCtx.Broadcast(Messages.GAME_FINISHED, mWinner.Id, Black.Life, White.Life, Black.DamageDealt, White.DamageDealt, Black.PlayerObject.GetDouble("rating"), White.PlayerObject.GetDouble("rating"), msg);
        }

        public void updatePlayerObjects(Boolean genericSave = false)
        {
            // Update ratings
            if (!Black.PlayerObject.Contains("rating"))
                Black.PlayerObject.Set("rating", (double)1500);
            if (!White.PlayerObject.Contains("rating"))
                White.PlayerObject.Set("rating", (double)1500);

            EloRating ratings = new EloRating(Black.PlayerObject.GetDouble("rating"), White.PlayerObject.GetDouble("rating"), Black.Life, White.Life);
            if (Black.ConnectUserId != "simpleAdmin")
                Black.PlayerObject.Set("rating", ratings.FinalResult1);
            if (White.ConnectUserId != "simpleAdmin")
                White.PlayerObject.Set("rating", ratings.FinalResult2);
            /*
            if (Black.KongId != "" && Black.KongAuthToken != "")
            {
                Dictionary<String, String> postValues = new Dictionary<string, string>();
                postValues.Add("user_id", Black.KongId);
                postValues.Add("game_auth_token", Black.KongAuthToken);
                postValues.Add("api_key", "9c4bf131-a74c-4112-8403-10a6b43b3c1f");
                postValues.Add("Rating", Black.PlayerObject["rating"].ToString());
                postValues.Add("MaxDamageDealt", Black.DamageDealt.ToString());
                if (mWinner == Black)
                    postValues.Add("MatchesWon", "1");
                mCtx.PlayerIO.Web.Post("http://www.kongregate.com/api/submit_statistics.json", postValues, onBlackKongStatsResponse);
            }

            if (White.KongId != "" && White.KongAuthToken != "")
            {
                Dictionary<String, String> postValues = new Dictionary<string, string>();
                postValues.Add("user_id", White.KongId);
                postValues.Add("game_auth_token", White.KongAuthToken);
                postValues.Add("api_key", "9c4bf131-a74c-4112-8403-10a6b43b3c1f");
                postValues.Add("Rating", White.PlayerObject["rating"].ToString());
                postValues.Add("MaxDamageDealt", White.DamageDealt.ToString());
                if (mWinner == White)
                    postValues.Add("MatchesWon", "1");
                mCtx.PlayerIO.Web.Post("http://www.kongregate.com/api/submit_statistics.json", postValues, onBlackKongStatsResponse);
            }
            */

            if (mCtx.InDevelopmentServer)
                return;

            if (genericSave)
            {
                Black.GetPlayerObject(saveBlackPlayer);
                White.GetPlayerObject(saveWhitePlayer);
            }
            else
            {
                mWinner.GetPlayerObject(saveWinner);
                mLoser.GetPlayerObject(saveLoser);
            }
        }

        public void saveBlackPlayer(DatabaseObject playerObject)
        {
            playerObject.Set(Properties.LastPlayed, DateTime.Now);

            // Damage dealt
            if (!playerObject.Contains(Properties.MaxDamageDealt))
                playerObject.Set(Properties.MaxDamageDealt, Black.DamageDealt);
            else
                if (Black.DamageDealt > playerObject.GetUInt(Properties.MaxDamageDealt))
                    playerObject.Set(Properties.MaxDamageDealt, Black.DamageDealt);

            playerObject.Save();

            Console.WriteLine(playerObject.Key + " player object has been saved.");
        }

        public void saveWhitePlayer(DatabaseObject playerObject)
        {
            playerObject.Set(Properties.LastPlayed, DateTime.Now);

            // Damage dealt
            if (!playerObject.Contains(Properties.MaxDamageDealt))
                playerObject.Set(Properties.MaxDamageDealt, Black.DamageDealt);
            else
                if (White.DamageDealt > playerObject.GetUInt(Properties.MaxDamageDealt))
                    playerObject.Set(Properties.MaxDamageDealt, White.DamageDealt);

            playerObject.Save();

            Console.WriteLine(playerObject.Key + " player object has been saved.");
        }

        public void onBlackKongStatsResponse(HttpResponse response)
        {
            Console.WriteLine(response.Text);
        }

        public void saveWinner(DatabaseObject playerObject)
        {
            playerObject.Set(Properties.LastPlayed, DateTime.Now);

            // Damage dealt
            if (!playerObject.Contains(Properties.MaxDamageDealt))
                playerObject.Set(Properties.MaxDamageDealt, Black.DamageDealt);
            else
                if (mWinner.DamageDealt > playerObject.GetUInt(Properties.MaxDamageDealt))
                    playerObject.Set(Properties.MaxDamageDealt, mWinner.DamageDealt);

            playerObject.Save();

            Console.WriteLine(playerObject.Key + " player object has been saved.");
        }

        public void saveLoser(DatabaseObject playerObject)
        {
            playerObject.Set(Properties.LastPlayed, DateTime.Now);

            // Damage dealt
            if (!playerObject.Contains(Properties.MaxDamageDealt))
                playerObject.Set(Properties.MaxDamageDealt, Black.DamageDealt);
            else
                if (mLoser.DamageDealt > playerObject.GetUInt(Properties.MaxDamageDealt))
                    playerObject.Set(Properties.MaxDamageDealt, mLoser.DamageDealt);

            playerObject.Save();

            Console.WriteLine(playerObject.Key + " player object has been saved.");
        }

        public void updateStats()
        {
            if(!mCtx.InDevelopmentServer) 
                mCtx.PlayerIO.BigDB.LoadOrCreate("Stats", "0.3", onStatLoad, onStatLoadError);
        }

        public void onStatLoad(DatabaseObject result)
        {
            if (result != null)
            {
                if(result.Contains("games_played"))
                    result.Set("games_played", result.GetInt("games_played") + 1);
                else
                    result.Set("games_played", 1);

                if (result.Contains("towers_basic"))
                    result.Set("towers_basic", result.GetInt("towers_basic") + Stats.Basic);
                else
                    result.Set("towers_basic", Stats.Basic);

                if (result.Contains("towers_rapidfire"))
                    result.Set("towers_rapidfire", result.GetInt("towers_rapidfire") + Stats.RapidFire);
                else
                    result.Set("towers_rapidfire", Stats.RapidFire);

                if(result.Contains("towers_sniper"))
                    result.Set("towers_sniper", result.GetInt("towers_sniper") + Stats.Sniper);
                else
                    result.Set("towers_sniper", Stats.Sniper);

                if(result.Contains("towers_pulse"))
                    result.Set("towers_pulse", result.GetInt("towers_pulse") + Stats.Pulse);
                else
                    result.Set("towers_pulse", Stats.Pulse);

                if(result.Contains("towers_slow"))
                    result.Set("towers_slow", result.GetInt("towers_slow") + Stats.Slow);
                else
                    result.Set("towers_slow", Stats.Slow);

                if(result.Contains("towers_spell"))
                    result.Set("towers_spell", result.GetInt("towers_spell") + Stats.Spell);
                else
                    result.Set("towers_spell", Stats.Spell);

                if(result.Contains("towers_damageboost"))
                    result.Set("towers_damageboost", result.GetInt("towers_damageboost") + Stats.DamageBoost);
                else
                    result.Set("towers_damageboost", Stats.DamageBoost);

                if(result.Contains("towers_rangeboost"))
                    result.Set("towers_rangeboost", result.GetInt("towers_rangeboost") + Stats.RangeBoost);
                else
                    result.Set("towers_rangeboost", Stats.RangeBoost);

                if(result.Contains("towers_firerateboost"))
                    result.Set("towers_firerateboost", result.GetInt("towers_firerateboost") + Stats.FireRateBoost);
                else
                    result.Set("towers_firerateboost", Stats.FireRateBoost);

                result.Save(true);
                Console.WriteLine("Server stats saved.");
            }
        }

        public void onStatLoadError(PlayerIOError error)
        {
            mCtx.PlayerIO.ErrorLog.WriteError("Unable to save server stats. " + error.Message);
        }
    }
}
