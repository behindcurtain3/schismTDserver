using System;
using System.Collections.Generic;
using System.Text;
using PlayerIO.GameLibrary;

namespace schismTD
{
    public class Wave
    {
        private GameCode mCtx;
        private Game mGame;
        private Player mPlayer;
        private Player mOpponent;

        private long mWaveTimeElapsed;
        private long mWaveTimeWindow;
        private float mTimeToNextSpawn;

        public Path Path
        {
            get;
            set;
        }

        public Vector2 StartingPosition
        {
            get;
            set;
        }

        public float HealthModifier
        {
            get;
            set;
        }

        public float ArmorModifier
        {
            get;
            set;
        }

        public float WorthModifier
        {
            get;
            set;
        }

        public Queue<Creep> SpawnQueue
        {
            get;
            set;
        }

        public Dictionary<Creep, long> SpawnTimers
        {
            get;
            set;
        }

        public List<String> CreepTypes
        {
            get;
            set;
        }

        public int Points
        {
            get;
            set;
        }

        public int Number
        {
            get;
            set;
        }

        public String ID
        {
            get;
            set;
        }

        public Wave(GameCode gc, Game game, Player player, Player opponent)
        {
            ID = Guid.NewGuid().ToString();
            mCtx = gc;
            mGame = game;
            mPlayer = player;
            mOpponent = opponent;

            mWaveTimeWindow = Settings.WAVE_WINDOW;
            mWaveTimeElapsed = 0;
            mTimeToNextSpawn = 0;

            Points = 24;

            SpawnQueue = new Queue<Creep>();
            SpawnTimers = new Dictionary<Creep, long>();
            CreepTypes = new List<String>();

            if (mPlayer == mGame.Black)
            {
                StartingPosition = mGame.Board.WhiteSpawn.Position;
            }
            else
            {
                StartingPosition = mGame.Board.BlackSpawn.Position;
            }
        }

        public void setup(int waveNum)
        {
            switch (waveNum)
            {
                case 2:
                    fillWithChigen();
                    break;
                case 3:
                case 8:
                    fillWithQuick();
                    break;
                case 4:
                case 9:
                    fillWithArmor();
                    break;
                case 1:
                case 10:
                    fillWithSwarm();
                    break;
                default:
                    fillWithRandom();
                    break;
            }

            long interval = mWaveTimeWindow / SpawnQueue.Count;
            long useableInterval;

            String previousType = "";
            int swarmCounter = 0;
            foreach (Creep creep in SpawnQueue)
            {
                useableInterval = interval;

                if (previousType == "Swarm")
                {
                    swarmCounter++;

                    if (swarmCounter % 3 != 0)
                        useableInterval /= 2;
                }

                SpawnTimers.Add(creep, useableInterval);

                previousType = creep.Type;
            }

            Number = waveNum;
            double expWave = waveNum - 1;

            HealthModifier = (float)Math.Pow((double)Settings.WAVE_HEALTH_MOD, expWave);
            ArmorModifier = (float)Math.Pow((double)Settings.WAVE_ARMOR_MOD, expWave);
            WorthModifier = (float)Math.Pow((double)Settings.WAVE_WORTH_MOD, expWave);
        }

        public void update(long dt)
        {
            mWaveTimeElapsed += dt;

            if (mWaveTimeElapsed <= mWaveTimeWindow && SpawnQueue.Count > 0)
            {
                mTimeToNextSpawn -= dt;

                if (mTimeToNextSpawn <= 0)
                {
                    Creep c;
                    lock(SpawnQueue)
                        c = SpawnQueue.Dequeue();         

                    c.CurrentPath = new Path(getCurrentPath());
                    c.Life = (int)(c.Life * HealthModifier);
                    c.StartingLife = c.Life;

                    c.Armor = (int)(c.Armor * ArmorModifier);
                    c.Worth = (int)(c.Worth * WorthModifier);

                    if (c is ChigenCreep)
                        (c as ChigenCreep).ChiAdded *= Number;

                    lock(mPlayer.Creeps)
                        mPlayer.Creeps.Add(c);

                    mCtx.Broadcast(Messages.GAME_CREEP_ADD, c.ID, c.Type, c.Player.Id, c.Center.X, c.Center.Y, c.Speed);

                    if (SpawnQueue.Count > 0)
                        mTimeToNextSpawn = SpawnTimers[SpawnQueue.Peek()];
                    else
                        mTimeToNextSpawn = 100000;
                }
            }
            else
            {
                return;
            }
        }

        public void addCreep(Creep creep)
        {
            switch (creep.Type)
            {
                case "Armor":
                    if (Points >= ArmorCreep.DEFAULT_POINTS)
                    {
                        Points -= ArmorCreep.DEFAULT_POINTS;
                        SpawnQueue.Enqueue(creep);

                        if (!CreepTypes.Contains(creep.Type))
                            CreepTypes.Add(creep.Type);
                    }
                    break;
                case "Chigen":
                    if (Points >= ChigenCreep.DEFAULT_POINTS)
                    {
                        Points -= ChigenCreep.DEFAULT_POINTS;
                        SpawnQueue.Enqueue(creep);

                        if (!CreepTypes.Contains(creep.Type))
                            CreepTypes.Add(creep.Type);
                    }
                    break;
                case "Magic":
                    if (Points >= MagicCreep.DEFAULT_POINTS)
                    {
                        Points -= MagicCreep.DEFAULT_POINTS;
                        SpawnQueue.Enqueue(creep);

                        if (!CreepTypes.Contains(creep.Type))
                            CreepTypes.Add(creep.Type);
                    }
                    break;
                case "Quick":
                    if (Points >= QuickCreep.DEFAULT_POINTS)
                    {
                        Points -= QuickCreep.DEFAULT_POINTS;
                        SpawnQueue.Enqueue(creep);

                        if (!CreepTypes.Contains(creep.Type))
                            CreepTypes.Add(creep.Type);
                    }
                    break;
                case "Regen":
                    if (Points >= RegenCreep.DEFAULT_POINTS)
                    {
                        Points -= RegenCreep.DEFAULT_POINTS;
                        SpawnQueue.Enqueue(creep);

                        if (!CreepTypes.Contains(creep.Type))
                            CreepTypes.Add(creep.Type);
                    }
                    break;
                case "Swarm":
                    if (Points >= SwarmCreep.DEFAULT_POINTS)
                    {
                        Points -= SwarmCreep.DEFAULT_POINTS;
                        // Queue three for each point
                        SpawnQueue.Enqueue(creep);
                        SpawnQueue.Enqueue(new SwarmCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                        SpawnQueue.Enqueue(new SwarmCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));

                        if (!CreepTypes.Contains(creep.Type))
                            CreepTypes.Add(creep.Type);
                    }
                    break;
            }

        }

        public void queueClient()
        {
            Message msg = Message.Create(Messages.GAME_WAVE_QUEUE);
            msg.Add(mPlayer.Id);
            msg.Add(ID);

            foreach (String str in CreepTypes)
            {
                msg.Add(str);
            }

            mCtx.Broadcast(msg);
        }

        public void activateClient()
        {
            mCtx.Broadcast(Messages.GAME_WAVE_ACTIVATE, mPlayer.Id, ID, (float)mWaveTimeWindow);
        }

        public void fillWithRandom()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                while (Points > 0)
                {
                    addCreep(getNextCreep(mPlayer));
                }
            }
        }

        public void fillWithArmor()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                while (Points > 0)
                {
                    addCreep(new ArmorCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                }
            }
        }

        public void fillWithQuick()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                while (Points > 0)
                {
                    addCreep(new QuickCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                }
            }
        }

        public void fillWithSwarm()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                while (Points > 0)
                {
                    addCreep(new SwarmCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                }
            }
        }

        public void fillWithChigen()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                while (Points > 0)
                {
                    addCreep(new ChigenCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                }
            }
        }

        public Path getCurrentPath()
        {
            Path path;
            if (mPlayer == mGame.Black)
            {
                path = mGame.Board.WhitePath;
            }
            else
            {
                path = mGame.Board.BlackPath;
            }

            return path;
        }

        public Creep getNextCreep(Player p)
        {
            Path path;
            Vector2 v;
            if (p == mGame.Black)
            {
                path = mGame.Board.WhitePath;
                v = mGame.Board.WhiteSpawn.Position;
            }
            else
            {
                path = mGame.Board.BlackPath;
                v = mGame.Board.BlackSpawn.Position;
            }

            int rand = mPlayer.Game.RandomGen.Next(2, 8);

            switch (rand)
            {
                case 2:
                    return new RegenCreep(mPlayer, mOpponent, v, path);
                case 3:
                    return new ChigenCreep(mPlayer, mOpponent, v, path);
                case 4:
                    return new QuickCreep(mPlayer, mOpponent, v, path);
                case 5:
                    return new MagicCreep(mPlayer, mOpponent, v, path);
                case 6:
                    return new ArmorCreep(mPlayer, mOpponent, v, path);
                case 7:
                    return new SwarmCreep(mPlayer, mOpponent, v, path);
                default:
                    return new SwarmCreep(mPlayer, mOpponent, v, path);
            }
        }
    }
}
