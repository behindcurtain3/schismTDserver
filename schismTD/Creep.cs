using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace schismTD
{
    public class Creep
    {
        // World position
        public PointF Position;
        public PointF Center;

        public int Width;
        public int Height;
        public RectangleF HitBox;

        // Pathing
        public Path CurrentPath;
        public Cell MovingTo; // The next cell in the path the creep is trying to get to

        // Goal
        public Cell Target; // Ultimate Goal, needs to be set independent of the path

        // Player the creep belongs to
        public Player Player;

        // Stats
        public Boolean Alive = true;
        public int Life;
        public int Speed;

        // Network state
        public String ID;
        public Boolean Valid = false;

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
                            Position.X += dd;
                        else
                            Position.X -= dd;
                    }
                    if (Math.Abs(Center.Y - MovingTo.Center.Y) >= Settings.CREEP_WIGGLE)
                    {
                        if (Center.Y < MovingTo.Center.Y)
                            Position.Y += dd;
                        else
                            Position.Y -= dd;
                    }

                    // Update center
                    Center.X = Position.X + Width / 2;
                    Center.Y = Position.Y + Height / 2;
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
