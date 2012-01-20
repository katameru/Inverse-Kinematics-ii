using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InverseCinematics
{
    /// <summary>
    /// Trzyma informacje o pewnym obszarze, m.in.:
    /// - odległości (od środka obszaru) do startu i punktów docelowych
    /// - osiągalność (z jakiegokolwiek fragmentu obszaru) startu i punktów docelowych
    /// - możliwe przypisania punktom docelowym osiagających je palców
    /// - czy ustawiony w obszarze nadgarstek ma szanse na poprawne rozwiązanie
    /// </summary>
    class PartitionHeuristic
    {
        public double SLDistanceStart;
        public List<double> SLDistanceTargets = new List<double>();
        public bool ReachableStart;
        public List<List<bool>> ReachableTargets = new List<List<bool>>();
        public List<List<int>> PossibleFingering = new List<List<int>>();
        public bool PossibleWrist;
        public Point Center;
        public double Radius;
        public double RealDistanceStart;
        public List<double> RealDistanceTargets = new List<double>();
        public bool Accessibility;

        public PartitionHeuristic(Point center, double radius, WorldInstance world, double maxArmLen, double minArmLen, List<double> maxFingersLen, List<double> minFingersLen)
        {
            SLDistanceStart = Geometry.SLDistance(center, world.Start);
            Center = center;
            Radius = radius;

            foreach (var t in world.Targets)
                SLDistanceTargets.Add(Geometry.SLDistance(center, t));

            ReachableStart = SLDistanceStart + radius >= minArmLen && SLDistanceStart - radius <= maxArmLen;

            for (var i = 0; i < world.Targets.Count; i++)
            {
                ReachableTargets.Add(new List<bool>());
                for(int j = 0; j < world.Targets.Count; j++)
                    ReachableTargets[i].Add(SLDistanceTargets[i] + radius >= minFingersLen[i] && SLDistanceTargets[i] - radius <= maxFingersLen[i]);
            }

            var perm = Geometry.Permutations(world.Targets.Count);

            foreach (var p in perm)
            {
                var b = true;
                for (var i = 0; i < world.Targets.Count; i++)
                    b &= ReachableTargets[i][p[i]];
                if (b)
                    PossibleFingering.Add(p);
            }

            
        }

        /// <summary>
        /// Oblicza możliwości dojścia/ustawienia nadgarstka.
        /// </summary>
        public void SetAccessibility()
        {
            Accessibility = true;
            PossibleWrist = PossibleFingering.Count > 0 && ReachableStart && Accessibility;
        }

        /// <summary>
        /// Dla zadanego punktu sprawdza czy da się z niego dojść do startu i celów
        /// </summary>
        /// <param name="p">punkt</param>
        /// <returns></returns>
        public bool PointAccessibility(Point p)
        {

            return true;
        }

        //public double

    }

    /// <summary>
    /// Dzieli świat na obszary dla których wykonuje obliczenia heurystyczne.
    /// </summary>
    class Heuristics
    {
        private WorldInstance _world;
        public int PartitionX;
        public int PartitionY;
        public double PartitionSize;
        public double PartitionRadius;
        public PartitionHeuristic[,] Partitionning;

        public double maxArmLen;
        public double minArmLen;
        public List<double> maxFingersLen = new List<double>();
        public List<double> minFingersLen = new List<double>();

        /// <summary>
        /// Tworzy nowy podział świata
        /// </summary>
        /// <param name="world">świat</param>
        /// <param name="maxPartition">maksymalna liczba podziałów na osi</param>
        public Heuristics(WorldInstance world, int maxPartition)
        {
            _world = world;

            if (world.SizeX >= world.SizeY)
            {
                PartitionX = maxPartition;
                PartitionSize = world.SizeX/(double)PartitionX;
                PartitionY = (int) Math.Round(world.SizeY/PartitionSize);
            }
            else
            {
                PartitionY = maxPartition;
                PartitionSize = world.SizeY / (double)PartitionY;
                PartitionX = (int)Math.Round(world.SizeX / PartitionSize);
            }

            PartitionRadius = Math.Sqrt(2*PartitionSize*PartitionSize)/2;

            CalculateLenghts();
            CalculatePartitionning();
            CalculateAccessibility();
            //CalculateRealDistances();
            world.heuristic = this;
        }

        /// <summary>
        /// Liczy dla każdej kończyny jej maksymalną i minimalną osiąganą długość.
        /// </summary>
        private void CalculateLenghts()
        {
            double min;
            double max;
            var spec = _world.Specification;
            //TODO CalculateMinMaxLenghts(spec.ArmArcLen, spec.ArmArcMin, spec.ArmArcMax, out minArmLen, out maxArmLen);

            //for (var i = 0; i < spec.FingersArcLen.Count; i++)
            //{
            //    CalculateMinMaxLenghts(spec.FingersArcLen[i], spec.FingersArcMin[i], spec.FingersArcMax[i],
            //                           out min, out max);
            //    minFingersLen.Add(min);
            //    maxFingersLen.Add(max);
            //}
        }

        /// <summary>
        /// Liczy dla kończyny o zadanych parametrach jej maksymalną i minimalną osiąganą długość.
        /// </summary>
        private static void CalculateMinMaxLenghts(List<double> len, List<double> arcMin, List<double> arcMax, out double minLen, out double maxLen)
        {
            var p1max = new Point(0, 0);
            var p1min = new Point(0, 0);

            double alpha2 = 0.0;
            double beta2 = 0.0;
            
            for (var i = 0; i < len.Count; i++)
            {
                double alpha;
                double beta;       

                var arcmin = arcMin[i];
                var arcmax = arcMax[i];
                
                if (arcmin <= 180 && arcmax >= 180)
                    alpha = 180;
                else if (Math.Abs(arcmin - 180) < Math.Abs(arcmax - 180))
                    alpha = arcmin;
                else
                    alpha = arcmax;

                alpha2 = Geometry.RelateAngle(alpha2, alpha);
                p1min = new Line(p1min, len[i], alpha2).P2;

                if (arcmin <=0 || arcmax >= 360)
                    beta = 0;
                else if (Math.Abs(arcmin - 0) < Math.Abs(arcmax - 360))
                    beta = arcmin;
                else
                    beta = arcmax;

                beta2 = Geometry.RelateAngle(beta2, beta);
                p1max = new Line(p1max, len[i], beta2).P2;
            }

            maxLen = Geometry.SLDistance(new Point(0, 0), p1max);
            minLen = Geometry.SLDistance(new Point(0, 0), p1min);
        }

        /// <summary>
        /// Buduje heurystyczne partycje oparte na zadanym podziale
        /// </summary>
        private void CalculatePartitionning()
        {
            Partitionning = new PartitionHeuristic[PartitionX, PartitionY];

            for(var x = 0; x < PartitionX; x++)
            {
                for(var y = 0; y < PartitionY; y++)
                {
                    Partitionning[x,y] = new PartitionHeuristic(new Point(x*PartitionSize + PartitionSize/2, y*PartitionSize + PartitionSize/2), PartitionRadius, _world,
                        maxArmLen, minArmLen, maxFingersLen, minFingersLen);
                }
            }
        }

        private void CalculateRealDistances()
        {
            
        }

        private void CalculateAccessibility()
        {
            var delta = new List<Point>
                            {
                                new Point(-PartitionRadius, 0),
                                new Point(PartitionRadius, 0),
                                new Point(0, -PartitionRadius),
                                new Point(0, PartitionRadius)
                            };
            var q = new Queue<Point>();
            var v = new List<Point>();
            Point p = GetHeuristic(_world.Start).Center;
            q.Enqueue(p);
            v.Add(p);

            while (q.Count > 0)
            {
                p = q.Dequeue();
                var h = GetHeuristic(p);
                h.SetAccessibility();

                foreach (var d in delta)
                {
                    var p2 = p+d;

                    //TODO if (!v.Contains(p2) && !Geometry.Intersects(_world.Obstacles, new Line(p, p2) ))
                    //{
                    //    q.Enqueue(p2);
                    //    v.Add(p2);
                    //}
                }
            }
        }

        /// <summary>
        /// Return partition heuristics with given point
        /// </summary>
        /// <param name="p">Point coordinates</param>
        /// <returns>partition heuristic</returns>
        public PartitionHeuristic GetHeuristic(Point p)
        {
            return Partitionning[(int) (p.X/PartitionSize), (int) (p.Y/PartitionSize)];
        }
    }
}
