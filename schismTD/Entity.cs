using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace schismTD
{
    public class Entity
    {
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

        // World position
        public Vector2 Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
                Center = mPosition.getPointF();
                HitBox = new RectangleF(mPosition.X, mPosition.Y, Width, Height);
            }
        }
        private Vector2 mPosition;

        public PointF Center
        {
            get
            {
                return mCenter;
            }
            set
            {
                mCenter.X = value.X + mWidth / 2;
                mCenter.Y = value.Y + mHeight / 2;
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

        public Entity()
        {
            ID = Guid.NewGuid().ToString();
        }
    }
}
