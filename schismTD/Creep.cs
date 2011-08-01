using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace schismTD
{
    public class Creep
    {
        // World position
        public PointF Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
            }
        }
        private PointF mPosition;

        public PointF Center
        {
            get
            {
                return mCenter;
            }
            set
            {
                mCenter = value;
            }
        }
        private PointF mCenter;

        public int Width
        {
            get
            {
                return mWidth;
            }
            set
            {
                mWidth = value;
            }
        }
        private int mWidth;


        public int Height
        {
            get
            {
                return mHeight;
            }
            set
            {
                mHeight = value;
            }
        }
        private int mHeight;


        public RectangleF HitBox
        {
            get
            {
                return mHitBox;
            }
            set
            {
                mHitBox = value;
            }
        }
        private RectangleF mHitBox;

        // Pathing
        public Path CurrentPath
        {
            get
            {
                return mCurrentPath;
            }
            set
            {
                mCurrentPath = value;
            }
        }
        private Path mCurrentPath;

        // The next cell in the path the creep is trying to get to
        public Cell MovingTo
        {
            get
            {
                return mMovingTo;
            }
            set
            {
                mMovingTo = value;
            }
        }
        private Cell mMovingTo;

        // Ultimate Goal, needs to be set independent of the path
        public Cell Target
        {
            get
            {
                return mTarget;
            }
            set
            {
                mTarget = value;
            }
        }
        private Cell mTarget;

        // Player the creep belongs to
        public Player Player
        {
            get
            {
                return mPlayer;
            }
            set
            {
                mPlayer = value;
            }
        }
        private Player mPlayer;

        // Stats
        public Boolean Alive
        {
            get
            {
                return mAlive;
            }
            set
            {
                mAlive = value;
            }
        }
        private Boolean mAlive;

        public int Life
        {
            get
            {
                return mLife;
            }
            set
            {
                mLife = value;
            }
        }
        private int mLife;

        public int Speed
        {
            get
            {
                return mSpeed;
            }
            set
            {
                mSpeed = value;
            }
        }
        private int mSpeed;

        // Network state
        public String ID
        {
            get
            {
                return mID;
            }
            set
            {
                mID = value;
            }
        }
        private String mID;

        // An invalid creep needs to be resynched with clients
        public Boolean Valid
        {
            get
            {
                return mValid;
            }
            set
            {
                mValid = value;
            }
        }
        private Boolean mValid;

        public Creep(Player player, Point pos, Path p, Cell t)
        {
            ID = Guid.NewGuid().ToString();
            Player = player;
            CurrentPath = new Path(p);
            Position = pos;
            Height = Settings.BOARD_CELL_HEIGHT;
            Width = Settings.BOARD_CELL_WIDTH;

            Center = new PointF(Position.X + (Width / 2), Position.Y + (Height / 2));

            Target = t;

            Life = 10;
            Speed = 50;

            Valid = false;
            Alive = true;
        }

        public void update(int dt)
        {
            // Move creep
            if(CurrentPath.Count > 0)
            {
                if (MovingTo == null)
                {
                    MovingTo = CurrentPath.Peek();
                    invalidate();
                }

                float d = getDistance(MovingTo);

                if (d <= 3)
                {
                    // Remove last cell
                    CurrentPath.Pop();

                    // Arrived
                    if (CurrentPath.Count == 0)
                    {
                        // Remove creep
                        Alive = false;
                    }
                    else
                    {
                        MovingTo = CurrentPath.Peek();
                        invalidate();
                    }
                }

                if (Alive)
                {
                    float dv = dt * 0.001f;
                    float dd = Speed * dv;

                    // Do movement
                    if (Math.Abs(Center.X - MovingTo.Center.X) >= Settings.CREEP_WIGGLE)
                    {
                        if (Center.X < MovingTo.Center.X)
                            mPosition.X += dd;
                        else
                            mPosition.X -= dd;
                    }
                    if (Math.Abs(Center.Y - MovingTo.Center.Y) >= Settings.CREEP_WIGGLE)
                    {
                        if (Center.Y < MovingTo.Center.Y)
                            mPosition.Y += dd;
                        else
                            mPosition.Y -= dd;
                    }

                    // Update center
                    mCenter.X = Position.X + Width / 2;
                    mCenter.Y = Position.Y + Height / 2;
                }

            }
        }

        public float getDistance(Cell c)
        {
            return Math.Abs(Center.X - c.Center.X) + Math.Abs(Center.Y - c.Center.Y);
        }

        public void invalidate()
        {
            Valid = false;
        }
    }
}
