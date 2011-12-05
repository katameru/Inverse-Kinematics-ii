using System;
using System.Collections.Generic;
using System.Drawing;
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

        public Point(string point) // "x y"
        {
            var p = point.Split();
            X = double.Parse(p[0]);
            Y = double.Parse(p[1]);
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

        public static bool operator !=(Point a, Point b)
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


        /* Tutaj liczymy rzut punkto na prosta zeby sprawdzic czy mozemy skorzystac ze wzoru 
         * czy musimy patrzec na odleglosci do koncow odcinka.                              */
        public double distance(Line l)
        {
            double A, B, C, x1, x2, y1, y2, d;
            x1 = l.P1.X; y1 = l.P1.Y;
            x2 = l.P2.X; y2 = l.P2.Y;
            //liczymy wspolczynniki rownania prostej
            A = x2 - x1; 
            B = y1 - y2; 
            C = x1*y2 - x2*y1;
            //odleglosc punktu od prostek
            d = Math.Abs(A * this.X + B * this.Y + C)/Math.Sqrt(A*A + B*B);
            //wektor normalny prostopadly do prostej
            var nx = A / Math.Sqrt(A * A + B * B);
            var ny = B / Math.Sqrt(A * A + B * B);
            //Nasz punkt +- wektor normalny wektor prostopadly razy dlugosc lezy na prostej
            //Patrzymy czy + czy -
            var x = this.X + nx * d;
            var y = this.Y + ny * d;
            if (A * x + B * y + C != 0)
            {
                x = this.X - nx * d;
                y = this.Y - ny * d;
            }
            //rzut powinien teraz lezec na prostej wyznaczonej przez odcinek l
            //troche mnie martwi czy zamiast (!= 0) nie lepiej dac (> eps) dla malego jakiegos malego epsilon
            Point rzut = new Point(x, y);
            double d1, d2;
            d1 = this.distance(l.P1);
            d2 = this.distance(l.P2);

            //Patrzymy czy rzut lezy na odinku, i jesli nie to zwracamy odleglosc do najblizszego punktu
            if (d1 + d2 == l.Len)
            {
                return d;
            }
            else
            {
                return Math.Min(d1, d2);
            }
        }

        public double distance(Obstacle o)
        {
            return o.Edges.Min(edge => this.distance(edge));
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
            return string.Format("[{0}, {1}]", P1, P2);
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
        //public int N;
        public int SizeX;
        public int SizeY;
        public List<Point> Targets = new List<Point>();
        public Point Start;
        public List<Obstacle> Obstacles;
        public Specification Specification;
        public string DebugSTR = "";

        public WorldInstance(string filename)
        {
            var lines = System.IO.File.ReadAllLines(filename).Where(l => l.Length != 0 && l[0] != '#').ToList();

            SizeX = int.Parse(lines[0]);
            SizeY = int.Parse(lines[1]);
            Start = new Point(lines[2]);
            var T = int.Parse(lines[3]);
            for (var i = 0; i < T; i++) // Reading targets points
                Targets.Add(new Point(lines[4+i]));

            var size = 1 + int.Parse(lines[4+T]); // Reading arm specification
            for (var i = 0; i < T; i++)
                size += 1 + int.Parse(lines[4+T+size]);
            Specification = new Specification(T, lines.GetRange(4 + T, size));

            size = 4 + T + size;
            var O = int.Parse(lines[size]); // Reading obstacles
            size++;
            Obstacles = new List<Obstacle>();

            for (var i = 0; i < O; i++)
            {
                var o = int.Parse(lines[size]);
                var obs = new List<Point>();

                for (var j = 0; j < o; j++)
                    obs.Add(new Point(lines[size + j + 1]));

                Obstacles.Add(new Obstacle(obs));
                size += 1 + o;
            }
                


        }

        public Bitmap ShowWorld(int x, int y)
        {
            const int width = 3;

            var sx = (float)x / SizeX;
            var sy = (float)y / SizeY;

            var world = new Bitmap(x, y);
            var p = new Pen(Color.Green, width);
            var g = Graphics.FromImage(world);
            
            g.DrawRectangle(p, sx*(float)Start.X, sy*(float)Start.Y, width, width);

            p.Color = Color.Orange;
            foreach (var t in Targets)
                g.DrawRectangle(p, sx*(float)t.X, sy*(float)t.Y, width, width);

            p.Color = Color.Red;
            foreach (var l in Obstacles.SelectMany(o => o.Edges))
                g.DrawLine(p, sx*(float) l.P1.X, sy*(float) l.P1.Y, sx*(float) l.P2.X, sy*(float) l.P2.Y);

            g.DrawImage(world, 0, 0, x, y);
            g.Dispose();
            return world;
        }



    }
}
