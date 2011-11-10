using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InverseCinematics
{

    class Line
    {
        public double X1;
        public double Y1;
        public double X2;
        public double Y2;
        public double A;
        public double B;
        public double Len;

        public Line (double x1, double y1, double x2, double y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Len = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            A = (y2 - y1)/(x2-x1);
            B = y1 - A*x1;
        }

        public override string ToString()
        {
            return string.Format("[({0},{1})({2},{3})]", X1, Y1, X2, Y2);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            var f = obj as Line;
            if (f == null)
                return false;

            return X1 == f.X1 && Y1 == f.Y1 && X2 == f.X2 && Y2 == f.Y2;
        }

        public bool Equals(Line f)
        {
            if (f == null)
                return false;
            return X1 == f.X1 && Y1 == f.Y1 && X2 == f.X2 && Y2 == f.Y2;
        }

        public override int GetHashCode()
        {
            return (int)(3*X1 + 7*Y1 + 11*X2 + 13*Y2);
        }

        public static bool operator ==(Line a, Line b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || (object)b == null)
                return false;
            return a.X1 == b.X1 && a.Y1 == b.Y1 && a.X2 == b.X2 && a.Y2 == b.Y2;
        }

        public static bool operator !=(Line a, Line b)
        {
            return !(a == b);
        }
    }

    class WorldInstance
    {
        public int N; // X size of plane (?)
        public int M;
        public double Sx; // Start point x coordinate
        public double Sy;
        public double Ox; // Object point x coordinate
        public double Oy;
        public List<double> Segments; // list of segments length;
        public List<double> AlphaMin; // list of min angles;
        public List<double> AlphaMax; // list of max angles;
        public List<List<Line>> Obstacles;


        public WorldInstance(string filename)
        {
            
        }
    }
}
