using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InverseCinematics
{
    class Point : IComparable<Point>
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            var f = obj as Point;
            if (f == null)
                return false;

            return X == f.X && Y == f.Y;
        }
        
        public override int GetHashCode()
        {
            return (int)(3*X + 7*Y);
        }

        public static bool operator ==(Point a, Point b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || (object)b == null)
                return false;
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Line a, Line b)
        {
            return !(a == b);
        }

        public int CompareTo(Point other)
        {
            if (other == null) return 1;
            if (this.X < other.X) return -1;
            if (this.X > other.X) return 1;
            return this.Y.CompareTo(other.Y);
        }

        public double distance(Point p)
        {
            return Math.Sqrt(Math.Pow(p.X - this.X, 2) + Math.Pow(p.Y - this.Y, 2)); ;
        }

        //TODO
        public double distance(Line l)
        {
            return 0;
        }
    }

    class Line
    {
        public Point P1;
        public Point P2;
        public double A;
        public double B;
        public double Len;

        public Line (double x1, double y1, double x2, double y2)
        {
            P1 = new Point(x1, y1);
            P2 = new Point(x1, y1);
            Len = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            A = (y2 - y1)/(x2-x1);
            B = y1 - A*x1;
        }

        public Line(Point p1, Point p2)
        {
            P1 = p1;
            P2 = p2;
            Len = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            A = (p2.Y - p1.Y) / (p2.X - p1.X);
            B = p1.Y - A * p1.X;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", P1.ToString(), P2.ToString(), );
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            var f = obj as Line;
            if (f == null)
                return false;

            return P1 == f.P1 && P2 == f.P2;
        }

        public bool Equals(Line f)
        {
            if (f == null)
                return false;
            return P1 == f.P1 && P2 == f.P2;
        }

        public override int GetHashCode()
        {
            return (int)( (2*P1.GetHashCode()) ^ P2.GetHashCode());
        }

        public static bool operator ==(Line a, Line b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || (object)b == null)
                return false;
            return a.P1 == b.P1 && a.P2 == b.P2;
        }

        public static bool operator !=(Line a, Line b)
        {
            return !(a == b);
        }

        //TODO
        public double distance(Point p)
        {
            return 0;
        }

        //TODO
        public double distance(Line l)
        {
            return 0;
        }

    }

    class Obstacle
    {
        public List<Line> Edges;
        protected Hull cachedHull;

        protected Obstacle() { }

        public Obstacle(List<Line> edges)
        {
            Edges = edges;
        }

        public Obstacle(List<Point> points)
        {
            Edges = new List<Line>();
            for (int i = 1; i < points.Count; i++)
            {
                Edges.Add(new Line(points[i-1], points[i]));
            }
        }

        public Hull convexHull()
        {
            if (cachedHull != null) {
                return cachedHull;
            }

            List<Point> points = new List<Point>();
            foreach(Line e in Edges)
            {
                points.Add(e.P1);
                points.Add(e.P2);
            }
            points = points.Distinct().ToList();
            points.Sort();

            Point pointOnHull = points[0];
            List<Point> hullPoints = new List<Point>();
            Point endpoint;

            do
            {
                hullPoints.Add(pointOnHull);
                endpoint = points[0];
                foreach (Point p in points)
                {
                    if ((endpoint == pointOnHull) || 
                        ((endpoint.X - pointOnHull.X)*(p.Y - pointOnHull.Y) - (p.X - pointOnHull.X)*(endpoint.Y - pointOnHull.Y)) > 0)
                    {
                        endpoint = p;
                    }
                }
                pointOnHull = endpoint;
            }
            while (endpoint != points[0]);
            cachedHull = new Hull(points);
            return cachedHull;
        }
    }
    
    class Hull : Obstacle
    {
        public Hull(List<Line> edges)
        {
            Edges = edges;
        }

        public Hull(List<Point> points)
        {
            Edges = new List<Line>();
            for (int i = 1; i < points.Count; i++)
            {
                Edges.Add(new Line(points[i-1], points[i]));
            }
        }

        public override Hull convexHull()
        {
            return this;
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
