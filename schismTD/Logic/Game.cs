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

        // Seconds to countdown at start of game (IE: dead period)
        private const long mCountdownLength = Settings.DEFAULT_GAME_COUNTDOWN * 1000; // in milliseconds
        private long mCountdownPosition;

        private long mWaveTimerLength = Settings.WAVE_LENGTH;
        private long mWaveTimerPosition;

        private long mTotalTimeElapsed = 0;
        private Boolean mIsGameSetup = false;

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

        public Game(GameCode gc, Player p1, Player p2)
        {
            mCtx = gc;
            Stats = new GameStats();
            Black = p1;
            White = p2;

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
        }

        public void setup()
        {
            mIsGameSetup = true;

            Console.WriteLine("Running setup...");
            Console.WriteLine("Black: " + Black.ConnectUserId + " --- " + Black.Id);
            Console.WriteLine("White: " + White.ConnectUserId + " --- " + White.Id);
            Black.Send(Messages.GAME_INFO, "black", Black.Id, White.Id);
            White.Send(Messages.GAME_INFO, "white", White.Id, Black.Id);

            // Reset both players
            Black.reset(this);
            White.reset(this);

            // Setup the waves
            for (int i = 0; i < Settings.DEFAULT_NUM_WAVES; i++)
            {
                Wave w = new Wave(mCtx, this, Black, White);
                w.setup(i + 1);
                w.Position = i;
                lock(Black.QueuedWaves)
                    Black.QueuedWaves.Enqueue(w);

                w = new Wave(mCtx, this, White, Black);
                w.setup(i + 1);
                w.Position = i;
                lock(White.QueuedWaves)
                    White.QueuedWaves.Enqueue(w);
            }

            Black.OnDeckWaves.Add(Black.QueuedWaves.Dequeue());
            Black.OnDeckWaves.Add(Black.QueuedWaves.Dequeue());
            Black.OnDeckWaves.Add(Black.QueuedWaves.Dequeue());

            foreach (Wave w in Black.OnDeckWaves)
            {
                w.queueClient();
            }

            White.OnDeckWaves.Add(White.QueuedWaves.Dequeue());
            White.OnDeckWaves.Add(White.QueuedWaves.Dequeue());
            White.OnDeckWaves.Add(White.QueuedWaves.Dequeue());

            foreach (Wave w in White.OnDeckWaves)
            {
                w.queueClient();
            }

            // synch the paths
            sendUpdatedPath(Black, Board.BlackPath);
            sendUpdatedPath(White, Board.WhitePath);

            mCtx.AddMessageHandler(Messages.GAME_TOWER_PLACE, placeTower);
            mCtx.AddMessageHandler(Messages.GAME_TOWER_UPGRADE, upgradeTower);
            mCtx.AddMessageHandler(Messages.GAME_TOWER_SELL, sellTower);
            mCtx.AddMessageHandler(Messages.GAME_WAVE_NEXT, setNextWave);
            mCtx.AddMessageHandler(Messages.GAME_FIRE_AT, setFireAt);
            mCtx.Broadcast(Messages.GAME_ACTIVATE);
        }

        public void start()
        {
            mIsStarted = true;
            mTotalTimeElapsed = 0;
            mWaveTimerPosition = mWaveTimerLength + 1;
            mCtx.Broadcast(Messages.GAME_START);
        }

        public void finish()
        {
            if (Finished)
                return;

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

            updatePlayerObjects();

            if (gameResult != Result.DRAW)
            {
                mCtx.Broadcast(Messages.GAME_FINISHED, mWinner.Id, Black.Life, White.Life, Black.DamageDealt, White.DamageDealt);
            }
            else
            {
                mCtx.Broadcast(Messages.GAME_FINISHED, -1, Black.Life, White.Life, Black.DamageDealt, White.DamageDealt);
            }

            updateStats();

            mIsFinished = true;
        }

        public void update(long dt)
        {
            if (!Ready)
            {
                if (Black != null && White != null)
                {
                    Ready = true;
                }
                return;
            }

            if (!Started)
            {
                mCountdownPosition -= dt;

                // Run setup() after counting down for one second
                if (!mIsGameSetup)
                {
                    if (mCountdownPosition < Settings.DEFAULT_GAME_COUNTDOWN * 1000 - 1000)
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
                            return;
                        }
                    }

                    // Check if anyone has died
                    if (Black.Life <= 0)
                    {
                        // White wins
                        finish();
                        return;
                    }
                    if (White.Life <= 0)
                    {
                        // Black wins
                        finish();
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
                            // Remove wave from OnDeckWaves w/ position = 0 & add to activewaves
                            // Decrement position on remaining OnDeckWaves
                            // Decrement position on remaining QueuedWaves
                            // Remove lowest position QueuedWave and add to OnDeckWaves
                            Wave nextWave = Black.OnDeckWaves.Find(delegate(Wave w)
                            {
                                return w.Position == 0;
                            });

                            if (nextWave != null)
                            {
                                lock (Black.ActiveWaves)
                                {
                                    Black.ActiveWaves.Add(nextWave);
                                    nextWave.activateClient();
                                }
                                lock (Black.OnDeckWaves)
                                {
                                    Black.OnDeckWaves.Remove(nextWave);

                                    foreach (Wave w in Black.OnDeckWaves)
                                        w.Position--;
                                }

                                if (Black.QueuedWaves.Count > 0)
                                {
                                    lock (Black.QueuedWaves)
                                    {
                                        foreach (Wave w in Black.QueuedWaves)
                                            w.Position--;
    
                                        lock(Black.OnDeckWaves)
                                            Black.OnDeckWaves.Add(Black.QueuedWaves.Dequeue());
                                    }
                                }

                                lock (Black.OnDeckWaves)
                                {
                                    foreach (Wave w in Black.OnDeckWaves)
                                        w.queueClient();
                                }
                            }

                            // WHITE WAVES
                            nextWave = White.OnDeckWaves.Find(delegate(Wave w)
                            {
                                return w.Position == 0;
                            });

                            if (nextWave != null)
                            {
                                lock (White.ActiveWaves)
                                {
                                    White.ActiveWaves.Add(nextWave);
                                    nextWave.activateClient();
                                }
                                lock (White.OnDeckWaves)
                                {
                                    White.OnDeckWaves.Remove(nextWave);

                                    foreach (Wave w in White.OnDeckWaves)
                                        w.Position--;
                                }

                                if (White.QueuedWaves.Count > 0)
                                {
                                    lock (White.QueuedWaves)
                                    {
                                        foreach (Wave w in White.QueuedWaves)
                                            w.Position--;

                                        lock (White.OnDeckWaves)
                                            White.OnDeckWaves.Add(White.QueuedWaves.Dequeue());
                                    }
                                }

                                lock (White.OnDeckWaves)
                                {
                                    foreach (Wave w in White.OnDeckWaves)
                                        w.queueClient();
                                }
                            }


                            /*
                            if (Black.NextWave != null)
                            {
                                Black.ActiveWaves.Add(Black.NextWave);
                                Black.NextWave.activateClient();
                            }

                            lock(Black.OnDeckWaves)
                                Black.OnDeckWaves.Remove(Black.NextWave);

                            if (Black.QueuedWaves.Count > 0)
                            {
                                Wave qWave;
                                lock(Black.QueuedWaves)
                                    qWave = Black.QueuedWaves.Dequeue();
                                lock(Black.OnDeckWaves)
                                    Black.OnDeckWaves.Add(qWave);

                                int count = 0;
                                foreach (Wave w in Black.OnDeckWaves)
                                {
                                    w.queueClient(count);
                                    count++;
                                }
                            }

                            if (Black.OnDeckWaves.Count > 0)
                                lock(Black.NextWave)
                                    Black.NextWave = Black.OnDeckWaves[0];
                            else
                                lock(Black.NextWave)
                                    Black.NextWave = null;
                            
                            if (White.NextWave != null)
                            {
                                White.ActiveWaves.Add(White.NextWave);
                                White.NextWave.activateClient();
                            }

                            lock(White.OnDeckWaves)
                                White.OnDeckWaves.Remove(White.NextWave);

                            if (White.QueuedWaves.Count > 0)
                            {
                                Wave qWave;
                                lock(White.QueuedWaves)
                                    qWave = White.QueuedWaves.Dequeue();

                                lock(White.OnDeckWaves)
                                    White.OnDeckWaves.Add(qWave);
                                qWave.queueClient();
                            }

                            if (White.OnDeckWaves.Count > 0)
                                lock(White.NextWave)
                                    White.NextWave = White.OnDeckWaves[0];
                            else
                                lock(White.NextWave)
                                    White.NextWave = null;
                            */
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
                            if (cr.Player == p)
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
                                cr.CurrentPath = tmpPaths[cr];
                                cr.MovingTo = cr.CurrentPath.Peek();
                            }
                            else
                            {
                                // else, check to see if we should use the updated main path
                                if (newPath.Contains(creepsInTheseCells[cr]))
                                {
                                    // Reapply the main path, removing any cells the creeper has already passed
                                    cr.CurrentPath = new Path(newPath);

                                    while (cr.CurrentPath.Peek() != creepsInTheseCells[cr])
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
                foreach(Creep cr in creepsToRePath)
                {
                    Cell crIn = findCellByPoint(cr.Center);

                    // else, check to see if we should use the updated main path
                    if (newPath.Contains(crIn))
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

                        cr.MovingTo = cr.CurrentPath.Peek();
                    }
                    else
                    {
                        cr.CurrentPath = AStar.getPath(crIn, targetBase);

                        if (cr.CurrentPath.Count == 0)
                            cr.CurrentPath.Push(crIn);

                        cr.MovingTo = cr.CurrentPath.Peek();
                    }
                }
            }

            sendUpdatedPath(p, newPath);
            
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
                        if (p.Mana < Costs.SLOW)
                            return;

                        // Remove the old tower
                        removeTower(p, c);

                        p.Mana -= Costs.SLOW;
                        lock (c.Tower)
                            c.Tower = new SlowTower(this, p, c.Player.Opponent, c.Position);

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
                case Tower.SLOW:
                    // Spell
                    if (choice == 1)
                    {
                        if (p.Mana < Costs.SPELL)
                            return;

                        removeTower(p, c);
                        p.Mana -= Costs.SPELL;

                        lock (c.Tower)
                            c.Tower = new SpellTower(this, p, c.Player.Opponent, c.Position);

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

            String id = m.GetString(0);
            Console.WriteLine("Setting next wave to: " + id);

            Wave result;
            Wave zero;
            lock (p.OnDeckWaves)
            {
                result = p.OnDeckWaves.Find(delegate(Wave w)
                {
                    return w.ID == id;
                });

                zero = p.OnDeckWaves.Find(delegate(Wave w)
                {
                    return w.Position == 0;
                });
            }

            if (result != null && zero != null)
            {
                int pos = result.Position;
                result.Position = zero.Position;
                zero.Position = pos;

                result.queueClient(true);
                zero.queueClient(true);
                Console.WriteLine("Next wave set!");
            }
            else
            {
                Console.WriteLine("Could not find the wave requested.");
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

            if (result != null)
            {
                lock (p.Towers)
                {
                    foreach (Tower t in p.Towers)
                    {
                        if (t.Type == Tower.SNIPER)
                            t.StaticTarget = result;
                    }
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

            mCtx.Broadcast(Messages.GAME_TOWER_PLACE, c.Index, c.Tower.Type);

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
            if(player == Black)
            {
                mWinner = White;
                mLoser = Black;
            }
            else
            {
                mWinner = Black;
                mLoser = White;
            }

            updatePlayerObjects();

            mCtx.Broadcast(Messages.GAME_FINISHED, mWinner.Id, Black.Life, White.Life, Black.DamageDealt, White.DamageDealt);
        }

        public void updatePlayerObjects()
        {
            // Update player objects
            Black.PlayerObject.Set(Properties.LastPlayed, DateTime.Now);

            if (!Black.PlayerObject.Contains(Properties.MaxDamageDealt))
                Black.PlayerObject.Set(Properties.MaxDamageDealt, Black.DamageDealt);
            else
                if (Black.DamageDealt > Black.PlayerObject.GetUInt(Properties.MaxDamageDealt))
                    Black.PlayerObject.Set(Properties.MaxDamageDealt, Black.DamageDealt);
            lock (Black.PlayerObject)
            {
                Black.PlayerObject.Save(true, delegate
                {
                    Console.WriteLine(Black.ConnectUserId + " has been saved.");
                });
            }

            White.PlayerObject.Set(Properties.LastPlayed, DateTime.Now);
            if (!White.PlayerObject.Contains(Properties.MaxDamageDealt))
                White.PlayerObject.Set(Properties.MaxDamageDealt, White.DamageDealt);
            else
                if (White.DamageDealt > White.PlayerObject.GetUInt(Properties.MaxDamageDealt))
                    White.PlayerObject.Set(Properties.MaxDamageDealt, White.DamageDealt);
            lock (White.PlayerObject)
            {
                White.PlayerObject.Save(true, delegate
                {
                    Console.WriteLine(White.ConnectUserId + " has been saved.");
                });
            }
        }

        public void updateStats()
        {
            mCtx.PlayerIO.BigDB.Load("Stats", "0.2", onStatLoad, onStatLoadError);
        }

        public void onStatLoad(DatabaseObject result)
        {
            if (result != null)
            {
                result.Set("games_played", result.GetUInt("games_played") + 1);
                result.Set("towers_basic", result.GetUInt("towers_basic") + Stats.Basic);
                result.Set("towers_rapidfire", result.GetUInt("towers_rapidfire") + Stats.RapidFire);
                result.Set("towers_sniper", result.GetUInt("towers_sniper") + Stats.Sniper);
                result.Set("towers_pulse", result.GetUInt("towers_pulse") + Stats.Pulse);
                result.Set("towers_slow", result.GetUInt("towers_slow") + Stats.Slow);
                result.Set("towers_spell", result.GetUInt("towers_spell") + Stats.Spell);
                result.Set("towers_damageboost", result.GetUInt("towers_damageboost") + Stats.DamageBoost);
                result.Set("towers_rangeboost", result.GetUInt("towers_rangeboost") + Stats.RangeBoost);
                result.Set("towers_firerateboost", result.GetUInt("towers_firerateboost") + Stats.FireRateBoost);
                result.Save(true);
            }
        }

        public void onStatLoadError(PlayerIOError error)
        {
            Console.WriteLine(error.Message);
        }
    }
}
