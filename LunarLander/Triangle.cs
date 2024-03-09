using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework; // This includes Vector2

namespace LunarLander
{
    public class Triangle
    {
        public Vector2 Point1, Point2, Point3;

        public Triangle(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }
    }

}
