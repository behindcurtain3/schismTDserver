using System;
using System.Collections.Generic;
using System.Text;
using PlayerIO.GameLibrary;

namespace schismTD
{
    public class Wave
    {
        public  float[] HEALTH_MODS = new float[] { 1f, 1.55f, 1.6f, 1.8f, 1.9f, 1.75f, 1.7f, 1.75f, 1.75f, 1.95f };

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
            get { return mNumber; }
            set
            {
                mNumber = value;

                HealthModifier = Settings.CREEP_LIFE;

                for (int i = 2; i <= mNumber; i++)
                {
                    HealthModifier = HealthModifier * HEALTH_MODS[i - 1];
                }

                ArmorModifier = (float)Math.Pow((double)Settings.WAVE_ARMOR_MOD, Number - 1);
                WorthModifier = (float)Math.Pow((double)Settings.WAVE_WORTH_MOD, Number - 1);
            }
        }
        private int mNumber;

        public int Position
        {
            get;
            set;
        }

        public Boolean Finished
        {
            get
            {
                return (SpawnQueue.Count == 0 && Started);
            }
        }

        public Boolean Started
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

            Started = false;
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
            if (mPlayer.PlayerObject.Contains("Waves"))
            {
                lock (SpawnQueue)
                {
                    SpawnQueue.Clear();

                    DatabaseArray wave = (DatabaseArray)mPlayer.PlayerObject.GetArray("Waves")[waveNum - 1];

                    for (int i = 0; i < wave.Count; i++)
                    {
                        switch ((String)wave[i])
                        {
                            case "Armor":
                                addCreep(new ArmorCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                                break;
                            case "Chigen":
                                addCreep(new ChigenCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                                break;
                            case "Magic":
                                addCreep(new MagicCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                                break;
                            case "Quick":
                                addCreep(new QuickCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                                break;
                            case "Regen":
                                addCreep(new RegenCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                                break;
                            case "Swarm":
                                addCreep(new SwarmCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                                break;
                        }
                    }
                }
            }
            else
            {
                switch (waveNum)
                {
                    case 1:
                        john1();
                        break;
                    case 2:
                        john2();
                        break;
                    case 3:
                        john3();
                        break;
                    case 4:
                        john4();
                        break;
                    case 5:
                        john5();
                        break;
                    case 6:
                        john6();
                        break;
                    case 7:
                        john7();
                        break;
                    case 8:
                        john8();
                        break;
                    case 9:
                        john9();
                        break;
                    case 10:
                        john10();
                        break;
                    default:
                        fillWithRandom();
                        break;
                }
            }

            long interval = mWaveTimeWindow / SpawnQueue.Count;
            long useableInterval;

            String previousType = "";
            foreach (Creep creep in SpawnQueue)
            {
                useableInterval = interval;

                if (previousType == "Quick")
                {
                    useableInterval /= 2;
                }

                SpawnTimers.Add(creep, useableInterval);

                previousType = creep.Type;
            }

            Number = waveNum;
            Position = waveNum - 1;
        }

        public void debug()
        {
            Console.WriteLine("Wave: " + Number);
            Console.WriteLine("HM: " + HealthModifier);
            Console.WriteLine("AM: " + ArmorModifier);
            Console.WriteLine("WM: " + WorthModifier);

        }

        public void debugCreep(Creep cr)
        {
            Console.WriteLine("--- CREEP --- (" + Number + ")");
            Console.WriteLine("Type: " + cr.Type);
            Console.WriteLine("Life: " + cr.Life);
            Console.WriteLine("Armor: " + cr.Armor);
            Console.WriteLine("Worth: " + cr.Worth);
        }

        public void update(long dt)
        {
            if (!Started)
                Started = true;

            mWaveTimeElapsed += dt;

            if (mWaveTimeElapsed <= mWaveTimeWindow && SpawnQueue.Count > 0)
            {
                mTimeToNextSpawn -= dt;

                if (mTimeToNextSpawn <= 0)
                {
                    Creep c;
                    lock(SpawnQueue)
                        c = SpawnQueue.Dequeue();

                    c.Wave = Number;
                    c.CurrentPath = new Path(getCurrentPath());
                    if (c is SwarmCreep)
                        c.Life = (int)Math.Round(HealthModifier * 0.33);
                    else
                        c.Life = (int)Math.Round(HealthModifier * c.Points);

                    c.StartingLife = c.Life;

                    c.Armor = (int)(c.Armor * ArmorModifier);
                    c.Worth = (3 * ((-10 / (Number + 1)) + 6)) * c.Points;//; BASE * ((-10 / (X + 1)) + 6)(int)(c.Worth * WorthModifier);

                    if (c is SwarmCreep)
                        c.Worth /= 3;

                    if (c is ChigenCreep)
                        (c as ChigenCreep).ChiAdded *= (int)Math.Ceiling(Number / 2f);

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

        public void queueClient(Boolean selfOnly = false)
        {
            Message msg = Message.Create(Messages.GAME_WAVE_QUEUE);
            msg.Add(mPlayer.Id);
            msg.Add(ID);
            msg.Add(Position);

            foreach (String str in CreepTypes)
            {
                msg.Add(str);
            }

            if (selfOnly)
                mPlayer.Send(msg);
            else
                mCtx.Broadcast(msg);
        }

        public void activateClient()
        {
            Message msg = Message.Create(Messages.GAME_WAVE_ACTIVATE);
            msg.Add(mPlayer.Id);
            msg.Add(ID);
            msg.Add(Position);
            msg.Add((float)mWaveTimeWindow);

            foreach (String str in CreepTypes)
            {
                msg.Add(str);
            }

            mCtx.Broadcast(msg);
        }

        public void removeClient()
        {
            mCtx.Broadcast(Messages.GAME_WAVE_REMOVE, mPlayer.Id, ID);
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

        public void fillWithRegen()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                while (Points > 0)
                {
                    addCreep(new RegenCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
                }
            }
        }

        public void fillWithMagic()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                while (Points > 0)
                {
                    addCreep(new MagicCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath()));
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

        public ArmorCreep getArmorCreep()
        {
            return new ArmorCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath());
        }

        public ChigenCreep getChigenCreep()
        {
            return new ChigenCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath());
        }

        public MagicCreep getMagicCreep()
        {
            return new MagicCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath());
        }

        public QuickCreep getQuickCreep()
        {
            return new QuickCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath());
        }

        public RegenCreep getRegenCreep()
        {
            return new RegenCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath());
        }

        public SwarmCreep getSwarmCreep()
        {
            return new SwarmCreep(mPlayer, mOpponent, StartingPosition, getCurrentPath());
        }

        public void wave4()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                //24 points to spend
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep());
            }
        }

        public void wave5()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();
                //24 points to spend
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());

                while (Points < 24)
                {
                    addCreep(getQuickCreep());
                }

            }
        }

        public void wave6()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep()); // 5
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep()); // 10
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep()); // 15
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep());
            }
        }

        public void john1()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getRegenCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
            }
        }

        public void john2()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getArmorCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
            }
        }

        public void john3()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getRegenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getRegenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getRegenCreep());
            }
        }

        public void john4()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());

            }
        }

        public void john5()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getArmorCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
                addCreep(getSwarmCreep());
            }
        }

        public void john6()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();


                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
            }
        }

        public void john7()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
            }
        }

        public void john8()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getRegenCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getArmorCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
            }
        }

        public void john9()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getChigenCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
                addCreep(getRegenCreep());
            }
        }

        public void john10()
        {
            lock (SpawnQueue)
            {
                SpawnQueue.Clear();

                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getMagicCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
                addCreep(getQuickCreep());
            }
        }
    }
}
