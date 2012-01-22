using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace InverseCinematics
{
    /// <summary>
    /// Zawiera definicje funkcji geometrycznych i nnych przydatnych metod.
    /// </summary>
    [Serializable]
    static class Geometry
    {
        /// <summary>
        /// Zwraca wszystki możliwe permutacje listy 0..n
        /// </summary>
        /// <param name="n">Liczba elementów list</param>
        /// <returns>Możliwe permutacje list n-elementowych</returns>
        public static List<List<int>> Permutations(int n)
        {
            var l = new List<int>();
            for(var i =0; i < n; i++)
                l.Add(i);
            return Permutations(l);
        }

        public static List<List<int>> Permutations(List<int> baseList)
        {
            if (baseList.Count == 1)
                return new List<List<int>>{baseList};

            var r = new List<List<int>>();

            foreach (var f in baseList)
            {
                var b = new List<int>(baseList);
                b.Remove(f);
                var t = Permutations(b);
                foreach (var tl in t)
                {
                    r.Add(tl);
                    r.Last().Add(f);
                }
            }

            return r;
        }

        /// <summary>
        /// Tworzy wartość bezwzględną kąta z wartości kata względem poprzedniego segmentu
        /// </summary>
        /// <param name="oldAngle">bezwzględna wartość kąta poprzedniego segmentu</param>
        /// <param name="newAngle">aktualna względna wartość kąta</param>
        /// <returns>Bezwzględna wartość kąta dla nowego segmentu</returns>
        public static double RelateAngle(double oldAngle, double newAngle)
        {
            return (oldAngle + newAngle)%360;
        }

        /// <summary>
        /// Sprawdza czy dwie figury przecinają się.
        /// </summary>
        public static bool Intersects(Point p, Line l)
        {
            return SLDistance(p, l.P1) + SLDistance(p, l.P2) <= l.Len + 0.1;
        }

        public static bool Intersects(Line l, Point p)
        {
            return Intersects(p, l);
        }

        public static bool Intersects(Line l1, Line l2)
        {
            return(IntersectionPoint(l1, l2) != null && IntersectionPoint(l1, l2).Count != 0);
        }

        public static bool Intersects(Line l, Obstacle o)
        {
            return o.Edges.Any(e => Intersects(l, e));
        }

        public static bool Intersects(Obstacle o, Line l)
        {
            return Intersects(l, o);
        }

        public static bool Intersects(List<Obstacle> ol, Line l)
        {
            return ol.Any(o => Intersects(o, l));
        }

        public static List<Point> IntersectionPoint(Line l1, Line l2)
        {
            Point p1, p2, p3, p4;
            p1 = l1.P1; p2 = l1.P2;
            p3 = l2.P1; p4 = l2.P2;

            Point d1, d2, d3;
            double xD1, yD1, xD2, yD2, xD3, yD3;
            double dot, deg, len1, len2;
            double segmentLen1, segmentLen2;
            double ua, ub, div;

            d1 = p2 - p1;
            d2 = p4 - p3;
            d3 = p1 - p3;
            xD1 = d1.X;
            yD1 = d1.Y;
            xD2 = d2.X;
            yD2 = d2.Y;
            xD3 = d3.X;
            yD3 = d3.Y;

            len1 = l1.Len;
            len2 = l2.Len;
            dot = d1 * d2;
            deg = dot / (len1 * len2);
            if (Math.Abs(deg) == 1) return null;
            Point pt = new Point(0, 0);
            div = yD2 * xD1 - xD2 * yD1;
            ua = (xD2 * yD3 - yD2 * xD3) / div;
            ub = (xD1 * yD3 - yD1 * xD3) / div;
            pt.X = p1.X + ua * xD1;
            pt.Y = p1.Y + ua * yD1;
            xD1 = pt.X - p1.X;
            xD2 = pt.X - p2.X;
            yD1 = pt.Y - p1.Y;
            yD2 = pt.Y - p2.Y;
            segmentLen1 = Math.Sqrt(xD1 * xD1 + yD1 * yD1) + Math.Sqrt(xD2 * xD2 + yD2 * yD2);
            xD1 = pt.X - p3.X;
            xD2 = pt.X - p4.X;
            yD1 = pt.Y - p3.Y;
            yD2 = pt.Y - p4.Y;
            segmentLen2 = Math.Sqrt(xD1 * xD1 + yD1 * yD1) + Math.Sqrt(xD2 * xD2 + yD2 * yD2);
            if (Math.Abs(len1 - segmentLen1) > 0.01 || Math.Abs(len2 - segmentLen2) > 0.01)
                return null;
            var l = new List<Point>();
            l.Add(pt);
            return l;  
        }

        public static List<Point> IntersectionPoint(Line l, Obstacle o)
        {
            return o.Edges.FindAll(e => Intersects(l, e)).SelectMany(e => IntersectionPoint(e, l)).ToList();
        }


        /// <summary>
        /// Odleglosc pomiedzy punktami na plaszczyznie bez przeszkod.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double SLDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /* Tutaj liczymy rzut punktow na prosta zeby sprawdzic czy mozemy skorzystac ze wzoru 
         * czy musimy patrzec na odleglosci do koncow odcinka.                              */
        public static double SLDistance(Point p, Line l)
        {
            double A, B, C, x1, x2, y1, y2, d;
            x1 = l.P1.X; y1 = l.P1.Y;
            x2 = l.P2.X; y2 = l.P2.Y;
            //liczymy wspolczynniki rownania prostej
            A = x2 - x1;
            B = y1 - y2;
            C = x1 * y2 - x2 * y1;
            //odleglosc punktu od prostek
            d = Math.Abs(A * p.X + B * p.Y + C) / Math.Sqrt(A * A + B * B);
            //wektor normalny prostopadly do prostej
            var nx = A / Math.Sqrt(A * A + B * B);
            var ny = B / Math.Sqrt(A * A + B * B);
            //Nasz punkt +- wektor normalny wektor prostopadly razy dlugosc lezy na prostej
            //Patrzymy czy + czy -
            var x = p.X + nx * d;
            var y = p.Y + ny * d;
            if (A * x + B * y + C != 0)
            {
                x = p.X - nx * d;
                y = p.Y - ny * d;
            }
            //rzut powinien teraz lezec na prostej wyznaczonej przez odcinek l
            //troche mnie martwi czy zamiast (!= 0) nie lepiej dac (> eps) dla malego jakiegos malego epsilon
            Point rzut = new Point(x, y);
            double d1, d2;
            d1 = SLDistance(p, l.P1);
            d2 = SLDistance(p, l.P2);

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

        public static double SLDistance(Line l, Point p)
        {
            return SLDistance(p, l);
        }

        public static double SLDistance(Point p, Obstacle o)
        {
            return o.Edges.Min(edge => SLDistance(p, edge));
        }

        public static double SLDistance(Obstacle o, Point p)
        {
            return SLDistance(p, o);
        }

        public static double SLDistance(Line l1, Line l2)
        {
            return Math.Min(
                            Math.Min(SLDistance(l1.P1, l2), SLDistance(l1.P2, l2)),
                            Math.Min(SLDistance(l1, l2.P1), SLDistance(l1, l2.P2)));
        }

        public static double SLDistance(Line l, Obstacle o)
        {
            return o.Edges.Min(edge => SLDistance(l, edge));
        }

        public static double SLDistance(Obstacle o, Line l)
        {
            return SLDistance(l, o);
        }

        public static double SLDistance(Obstacle o1, Obstacle o2)
        {
            return o1.Edges.Min(edge => SLDistance(edge, o2));
        }

        /// <summary>
        /// Odleglosc pomiedzy punktami na powierzchni wieloboku.
        /// </summary>        
        public static double SurfDistance(Point p1, Point p2, Obstacle o)
        {
            var e1 = o.Edges.Find(e => Intersects(e, p1));
            var i1 = o.Edges.FindIndex(e => e == e1);
            var e2 = o.Edges.Find(e => Intersects(e, p2));
            var i2 = o.Edges.FindIndex(e => e == e2);
            double distance1 = SLDistance(p1, e1.P2) + SLDistance(p2, e2.P2);
            double distance2 = SLDistance(p2, e2.P1) + SLDistance(p1, e1.P2);
            int i = i1 + 1;
            while(true)
            {
                if (o.Edges.Count == i)
                {
                    i = 0;
                }
                if (o.Edges[i] == e2) break;
                distance1 += o.Edges[i].Len;
                i++;
            }

            i = i2 + 1;
            while (true)
            {
                if (o.Edges.Count == i)
                {
                    i = 0;
                }
                if (o.Edges[i] == e1) break;
                distance2 += o.Edges[i].Len;
                i++;
            }

            return Math.Min(distance1, distance2);
        }

        /// <summary>
        /// Odleglosc pomiedzy punktami na danej planszy z przeszkodami.
        /// </summary>
        public static double Distance(Point p1, Point p2, WorldInstance world)
        {
            Line l = new Line(p1, p2);
            double distance = 0;
            var iobs = world.Obstacles.FindAll(o => Intersects(o, l)).OrderBy(o => SLDistance(o, p1)).ToList();
            var ipoints = new List<List<Point>>();

            for(int i = 0; i< iobs.Count; i++)
            {
                var iobstacle = iobs[i];
                var a = iobstacle.Edges.Select(e => IntersectionPoint(e, l)).ToList().FindAll(e => e != null).SelectMany(x => x).ToList();
                var b = a.OrderBy(p => SLDistance(p1, p)).ToList();
                var ipoint = b;
                if (ipoint.Count == 1)
                {
                    iobs.RemoveAt(i);
                    i--;
                    continue;
                }
                ipoints.Add(ipoint);
            }
            if (iobs.Count == 0) return SLDistance(p1, p2);
            for (int i = 0; i < iobs.Count; i++)
            {
                if (i == 0)
                {
                    distance += SLDistance(p1, ipoints[0][0]);
                }
                else
                {
                    distance += SLDistance(ipoints[i - 1][1], ipoints[i][0]);
                }

                distance += SurfDistance(ipoints[i][0], ipoints[i][1], iobs[i]);
            }

            distance += SLDistance(ipoints.Last(p => true)[1], p2);

            return distance;
        }

    }

    [Serializable]
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

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static double operator *(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Point operator *(Point a, double l)
        {
            return new Point(a.X * l, a.Y * l);
        }

        public static Point operator *(double l, Point a)
        {
            return new Point(a.X * l, a.Y * l);
        }

        public int CompareTo(Point other)
        {
            if (other == null) return 1;
            if (this.X < other.X) return -1;
            if (this.X > other.X) return 1;
            return this.Y.CompareTo(other.Y);
        }
    }

    [Serializable]
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

        public Line(Point p1, double len, double angle) // angle is from north
        {
            var deg = (360 - (angle - 90))%360;
            var rad = Math.PI*deg/180.0;

            P1 = p1;
            P2 = new Point(P1.X + len*Math.Cos(rad), P1.Y + len*Math.Sin(rad));
            Len = len;
            A = (P2.Y - P1.Y) / (P2.X - P1.X);
            B = P1.Y - A * P1.X;
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
    }

    /// <summary>
    /// Zbiór linii tworzących (raczej spójną) przeszkodę
    /// </summary>
    [Serializable]
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
                    var det = (endpoint.X - pointOnHull.X) * (p.Y - pointOnHull.Y) - (p.X - pointOnHull.X) * (endpoint.Y - pointOnHull.Y);
                    if ( (endpoint == pointOnHull) || ( det > 0) )
                    {
                        endpoint = p;
                    }
                }
                pointOnHull = endpoint;
            }
            while (endpoint != points[0]);
            hullPoints.Add(points[0]);
            cachedHull = new Hull(hullPoints);
            return cachedHull;
        }
    }

    [Serializable]
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

        new public Hull convexHull()
        {
            return this;
        }
    }

    /// <summary>
    /// Opisuje świat w którym toczy się ewolucja
    /// </summary>
    [Serializable]
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
        public Heuristics heuristic;

        /// <summary>
        /// Wczytuje scenariusz
        /// </summary>
        /// <param name="filename">ścieżka do pliku tekstowego z opisem świata</param>
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

            Obstacles.Add(new Obstacle(new List<Point> { new Point(0, 0), new Point(SizeX, 0)}));
            Obstacles.Add(new Obstacle(new List<Point> { new Point(0, 0), new Point(0, SizeY) }));
            Obstacles.Add(new Obstacle(new List<Point> { new Point(SizeX, SizeY), new Point(SizeX, 0) }));
            Obstacles.Add(new Obstacle(new List<Point> { new Point(SizeX, SizeY), new Point(0, SizeY) }));
        }

        /// <summary>
        /// Tworzy rysunek z przedstawionym światem
        /// </summary>
        /// <param name="x">Wielkość obrazu w poziomie</param>
        /// <param name="y">Wielkość obrazu w pionie</param>
        /// <param name="penwidth">Grubość pióra</param>
        /// <param name="heuristic">Heurystyka do zwizualizowania</param>
        /// <returns>Obraz świata</returns>
        public Bitmap ShowWorld(int x, int y, float penwidth)
        {
            var s = Math.Min((float)x / SizeX, (float)y / SizeY);
            var world = new Bitmap(x, y);
            var p = new Pen(Color.Green, penwidth);
            var g = Graphics.FromImage(world);

            var b = new SolidBrush(Color.Black);
            g.FillRectangle(b, 0, 0, x, y);

            b = new SolidBrush(Color.Snow);
            for (var i = 0; i < heuristic.PartitionX; i++)
                for (var j = 0; j < heuristic.PartitionY; j++)
                    if (heuristic.Partitionning[i, j].Accessibility)
                        g.FillRectangle(b, s * i * (float)heuristic.PartitionSize, s * j * (float)heuristic.PartitionSize,
                                        s * (float)heuristic.PartitionSize, s * (float)heuristic.PartitionSize);

            b = new SolidBrush(Color.PowderBlue);
            for (var i = 0; i < heuristic.PartitionX; i++)
                for (var j = 0; j < heuristic.PartitionY; j++)
                    if (heuristic.Partitionning[i, j].PossibleWrist)
                        g.FillRectangle(b, s * i * (float)heuristic.PartitionSize, s * j * (float)heuristic.PartitionSize,
                                        s * (float)heuristic.PartitionSize, s * (float)heuristic.PartitionSize);

            g.DrawRectangle(p, s*(float)Start.X-penwidth/2, s*(float)Start.Y-penwidth/2, penwidth, penwidth);

            p.Color = Color.Orange;
            foreach (var t in Targets)
                g.DrawRectangle(p, s*(float)t.X - penwidth/2, s*(float)t.Y-penwidth/2, penwidth, penwidth);

            p.Color = Color.Red;
            foreach (var l in Obstacles.SelectMany(o => o.Edges))
                g.DrawLine(p, s*(float) l.P1.X, s*(float) l.P1.Y, s*(float) l.P2.X, s*(float) l.P2.Y);

            g.DrawImage(world, 0, 0, x, y);
            g.Dispose();
            return world;
        }

    }
}
