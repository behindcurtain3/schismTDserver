using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace schismTD
{
    public class Vector2
    {
        public float X
        {
            get
            {
                return mX;
            }
            set
            {
                mX = value;
            }
        }
        private float mX;

        public float Y
        {
            get
            {
                return mY;
            }
            set
            {
                mY = value;
            }
        }
        private float mY;

        public float Length
        {
            get
            {
                return mLength;
            }
            set
            {
                mLength = value;
                X = (float)Math.Cos(mDirection / 180 * Math.PI) * Length;
                Y = (float)Math.Sin(mDirection / 180 * Math.PI) * Length;
            }
        }
        private float mLength;

        public int Direction
        {
            get
            {
                return mDirection;
            }
            set
            {
                mDirection = value;
                X = (float)Math.Cos(mDirection / 180 * Math.PI) * Length;
                Y = (float)Math.Sin(mDirection / 180 * Math.PI) * Length;
            }
        }
        private int mDirection;

        public Vector2()
        {
            setPoints(0, 0);
        }

        public Vector2(float x, float y)
        {
            setPoints(x, y);
        }

        public Vector2(PointF point)
        {
            setPoints(point.X, point.Y);
        }

        public PointF getPointF()
        {
            return new PointF(X, Y);
        }

        public void setPoints(float x, float y)
        {
            X = x;
            Y = y;

            mLength = (float)Math.Sqrt(X * X + Y * Y);
            mDirection = (int)(Math.Atan2(X, Y) * 180 / Math.PI);
        }

        public void Normalize()
        {
            float temp = 1 / Length;
            setPoints(X * temp, Y * temp);
        }

        public float Dot(Vector2 b)
        {
            return (b.X * X) + (b.Y * Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator *(Vector2 a, float scale)
        {
            return new Vector2(a.X * scale, a.Y * scale);
        }
        public static Vector2 operator *(float scale, Vector2 a)
        {
            return new Vector2(a.X * scale, a.Y * scale);
        }

        public static Vector2 operator ++(Vector2 a)
        {
            a.X++;
            a.Y++;
            return a;
        }

        public static Vector2 operator --(Vector2 a)
        {
            a.X--;
            a.Y--;
            return a;
        }

        // Negation
        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.X, -a.Y);
        }

        public static bool operator true(Vector2 a)
        {
            if ((a.X != 0) || (a.Y != 0))
                return true;
            else
                return false;
        }

        public static bool operator false(Vector2 a)
        {
            if ((a.X == 0) && (a.Y == 0))
                return true;
            else
                return false;
        }
    }
}
