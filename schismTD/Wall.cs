using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Wall
    {
        public Point Start;
        public Point End;

        public Wall(Point s, Point e)
        {
            Start = s;
            End = e;
        }
    }
}
