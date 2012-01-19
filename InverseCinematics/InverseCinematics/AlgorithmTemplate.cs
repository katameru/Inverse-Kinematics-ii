using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;


namespace InverseCinematics
{
    class Tree<T>
    {
        public T Node;
        public Tree<T> Subtree1;
        public Tree<T> Subtree2;
        public List<T> Leafs;

        public Tree(T node)
        {
            Node = node;
            Subtree1 = null;
            Subtree2 = null;
            Leafs = new List<T>{node};
        }

        public Tree(T node, Tree<T> tree1, Tree<T> tree2)
        {
            Node = node;
            Subtree1 = tree1;
            Subtree2 = tree2;
            Leafs = tree1.Leafs.Concat(tree2.Leafs).ToList();
        }

        public Tree(int depth)
        {
            Node = default(T);
            if (depth == 0)
                return;
            Subtree1 = new Tree<T>(depth - 1);
            Subtree2 = new Tree<T>(depth - 1);
            RestoreLeafs();
        }

        public List<T> RestoreLeafs()
        {
            var l = new List<T>();
            if (Subtree1 != null)
                l.AddRange(Subtree1.RestoreLeafs());
            if (Subtree2 != null)
                l.AddRange(Subtree2.RestoreLeafs());
            return l;
        }

        public void Add(string path, T node)
        {
            if (path == "")
            {
                Node = node;
                return;
            }

            if (path[0]=='L')
                Subtree1.Add(path.Remove(0, 1), node);
            else
                Subtree2.Add(path.Remove(0, 1), node);
        }

        public override string ToString()
        {
            var n = Node == null ? "%" : Node.ToString();
            var s1 = Subtree1 == null ? "%" : Subtree1.ToString();
            var s2 = Subtree2 == null ? "%" : Subtree2.ToString();
            return "<" + s1 + "|" + n + "|" + s2 + ">";
        }
    }

 


    /// <summary>
    /// Podstawowe informacje o chromosomie, zawierające głównie wartości kątów.
    /// </summary>
    //class Chromosome
    //{

        
    //}

    class Chromosome
    {
        public List<double> Arm;
        public List<List<double>> Fingers;
        public List<Point> TouchPoints;
        public List<Line> Bones;
        public List<Line> BonesFingers;
        public List<Line> BonesArm;
        public double Score;
        public double Error;
        //TODO
        public double ArmScore;
        public double ArmError;
        public List<double> BestFingeringScore;
        public List<double> BestFingeringError;
        public List<int> BestFingering;

        /// <summary>
        /// Tworzy nowy chromosom
        /// </summary>
        /// <param name="arm">Lista kątów ramienia</param>
        /// <param name="fingers">Lista kątów palców</param>
        /// <param name="world">świat</param>
        public Chromosome(List<double> arm, List<List<double>> fingers, WorldInstance world)
        {
            Arm = arm;
            Fingers = fingers;


            var angle = 0.0; // Initial angle is north
            var point = world.Start;
            Bones = new List<Line>();
            BonesArm = new List<Line>();
            BonesFingers = new List<Line>();

            TouchPoints = new List<Point>();

            for (var i = 0; i < arm.Count; i++)
            {
                angle = Geometry.RelateAngle(angle, Arm[i]);
                //TODO BonesArm.Add(new Line(point, world.Specification.ArmArcLen[i], angle));
                point = BonesArm.Last().P2;
            }

            for (var i = 0; i < Fingers.Count; i++)
            {
                var angle2 = angle; // remember last angle;

                var point2 = point;
                for (var j = 0; j < fingers[i].Count; j++)
                {
                    angle2 = Geometry.RelateAngle(angle2, Fingers[i][j]);
                    //TODO BonesFingers.Add(new Line(point2, world.Specification.FingersArcLen[i][j], angle2));
                    point2 = BonesFingers.Last().P2;
                }
                TouchPoints.Add(point2);
            }

            Bones = BonesArm.Concat(BonesFingers).ToList();
        }

        /// <summary>
        /// Zapisuje w chromosomie rezultat jego ewaluacji
        /// </summary>
        /// <param name="score">Wynik chromosomu</param>
        /// <param name="error">Liczba błędów</param>
        public void SaveEvaluation(double score, double error)
        {
            Score = score;
            Error = error;
        }

    }

    class NodeSpec
    {
        public double Length;
        public double ArcMin;
        public double ArcMax;

        public NodeSpec(double length, double arcmin, double arcmax)
        {
            Length = length;
            ArcMin = arcmin;
            ArcMax = arcmax;
        }

        public NodeSpec(IList<string> spec)
        {
            Length = int.Parse(spec[0]);
            ArcMin = int.Parse(spec[1]);
            ArcMax = int.Parse(spec[2]);
        }

        public override string ToString()
        {
            return Length + ":" + ArcMin + "/" + ArcMax;
        }
    }

    /// <summary>
    /// Specyfikacja chromosomów.
    /// Zawiera informacje o długościach poszczególnych kości oraz możliwych kątach rozwarć stawów
    /// </summary>
    class Specification
    {
        public Tree<NodeSpec> Spec = new Tree<NodeSpec>(null);

        /// <summary>
        /// Tworzy specyfikację na podstawie konkretnych danych.
        /// </summary>
        public Specification(Tree<NodeSpec> spec)
        {
            Spec = spec;
        }

        /// <summary>
        /// Wczytuje specyfikację z danych pobranych z pliku.
        /// </summary>
        /// <param name="spec">Kolejne linie pliku wejściowego</param>
        public Specification(int depth, List<string> spec)
        {
            Spec = new Tree<NodeSpec>(depth);
            foreach (var sp in spec)
            {
                var s = sp.Split();
                Spec.Add(s[0], new NodeSpec(s.Skip(1).ToList()));
            }
            return;
            Tree<NodeSpec> tup = null;
            var tdown = new Tree<NodeSpec>(null);
            Spec = tdown;
            var lvl = 0;

            foreach (var sp in spec)
            {
                var s = sp.Split();
                var l = s[0].Count();
                var t = new Tree<NodeSpec>(new NodeSpec(s.Skip(1).ToList()));

                if (l > lvl)
                {    
                    tdown.Subtree1 = t;
                    tup = tdown;
                    tdown = tdown.Subtree1;
                }
                else
                {
                    tup.Subtree2 = t;
                    tdown = tup.Subtree2;
                }

                lvl = l;
            }
        }
    }

    /// <summary>
    /// Typ wyliczeniowy wskazujacy ktory element ramienia bedziemy chcieli zmieniac.
    /// </summary>
    enum EvolveChoices { Arm, Fingers, All};


    class AlgorithmTemplate
    {
        /// <summary>
        /// Tworzy losową populację chromosomów
        /// </summary>
        /// <param name="world">Świat</param>
        /// <param name="size">Wielkośc populacji</param>
        /// <returns>Losowa populacja wielkości size</returns>
        public static List<Chromosome> GenerateRandomPopulation(WorldInstance world, int size)
        {
            var rand = new Random();
            var population = new List<Chromosome>();

            var spec = world.Specification;

            for (var i =0; i < size; i++)
            {
                var arm = new List<double>();
                var fingers = new List<List<double>>();

                //TODO for (var k = 0; k < spec.ArmArcLen.Count; k++)
                //TODO        arm.Add(spec.ArmArcMin[k] + rand.NextDouble() * (spec.ArmArcMax[k] - spec.ArmArcMin[k]));

                //TODO for (var k = 0; k < spec.FingersArcLen.Count; k++)
                //{
                //    fingers.Add(new List<double>());
                    //TODO for (var l = 0; l < spec.FingersArcLen[k].Count; l++)
                    //TODO    fingers[k].Add(spec.FingersArcMin[k][l] + rand.NextDouble()*(spec.FingersArcMax[k][l] - spec.FingersArcMin[k][l]));
                //}
                population.Add(new Chromosome(arm, fingers, world));
            }

            return population;
        }

        /// <summary>
        /// Drukuje populację
        /// </summary>
        /// <param name="world">świat</param>
        /// <param name="population">populacja</param>
        /// <param name="img">tło do rysowania</param>
        /// <param name="penwidth">szerokość pióra</param>
        /// <param name="pencolor">kolor pióra</param>
        /// <returns></returns>
        public static Bitmap PrintPopulation(WorldInstance world, List<Chromosome> population, Bitmap img, float penwidth, Color pencolor)
        {
            var s = Math.Min((float)img.Width / world.SizeX, (float)img.Height / world.SizeY);

            var p = new Pen(pencolor, penwidth);
            var g = Graphics.FromImage(img);

            foreach (var c in population)
            //var c= population.FindAll(o => o.Score < 6)[0];
                foreach (var b in c.Bones)
                    g.DrawLine(p, s*(float) b.P1.X, s*(float) b.P1.Y, s*(float) b.P2.X, s*(float) b.P2.Y);
                
            g.DrawImage(img, 0, 0, img.Width, img.Height);
            g.Dispose();
            return img;
        }
        
    
        /// <summary>
        /// Mutacja chromosomu
        /// </summary>
        /// <param name="before">chromosom do mutacji</param>
        /// <param name="chance">szansa na mutacje</param>
        /// <param name="world">swiat</param>
        /// <param name="evolveWhat">ktore elementy zmieniac</param>
        /// <returns></returns>
        public static Chromosome Mutate(Chromosome before, double chance, WorldInstance world, EvolveChoices evolveWhat)
        {
            var rand = new Random();
            var spec = world.Specification;
            var arm = before.Arm;
            var fingers = before.Fingers;

            if (evolveWhat == EvolveChoices.Arm || evolveWhat == EvolveChoices.All)
                for (var i = 0; i < arm.Count; i++)
                    if (rand.NextDouble() < chance)
                        {} //TODO arm[i] = spec.ArmArcMin[i] + rand.NextDouble()*(spec.ArmArcMax[i] - spec.ArmArcMin[i]);

            if (evolveWhat == EvolveChoices.Fingers || evolveWhat == EvolveChoices.All)
                for (var i = 0; i < fingers.Count; i++)
                    for (var j = 0; j < fingers[i].Count; j++)
                        if (rand.NextDouble() < chance)
                            {} //TODO fingers[i][j] = spec.FingersArcMin[i][j] + rand.NextDouble()*(spec.FingersArcMax[i][j] - spec.FingersArcMin[i][j]);

            return new Chromosome(arm, fingers, world);
        }

        /// <summary>
        /// Krzyzowanie dwoch chromosomow. Jezeli jakiegos fragmentu nie chcemy zmieniac, to jest on dziedziczony od rodzicow wprost.
        /// </summary>
        public static List<Chromosome> Crossover(Chromosome p1, Chromosome p2, WorldInstance world, EvolveChoices evolveWhat)
        {
            var beta = new Random().NextDouble();
            var c1_arm = new List<double>();
            var c1_fingers = new List<List<double>>();
            var c2_arm = new List<double>();
            var c2_fingers = new List<List<double>>();

            if (evolveWhat == EvolveChoices.Arm || evolveWhat == EvolveChoices.All)
            {
                for (var i = 0; i < p1.Arm.Count; i++)
                {
                    c1_arm.Add(p1.Arm[i] + p2.Arm[i] + beta * (p1.Arm[i] - p2.Arm[i]));
                    c2_arm.Add(p1.Arm[i] + p2.Arm[i] + beta * (p2.Arm[i] - p1.Arm[i]));
                }
            }
            else
            {
                for (var i = 0; i < p1.Arm.Count; i++)
                {
                    c1_arm.Add(p1.Arm[i]);
                    c2_arm.Add(p2.Arm[i]);
                }
            }

            if (evolveWhat == EvolveChoices.Fingers || evolveWhat == EvolveChoices.All)
            {
                for (var i = 0; i < p1.Fingers.Count; i++)
                {
                    c1_fingers.Add(new List<double>());
                    c2_fingers.Add(new List<double>());
                    for (var j = 0; j < p1.Fingers[i].Count; j++)
                    {
                        c1_fingers[i].Add(p1.Fingers[i][j] + p2.Fingers[i][j] + beta * (p1.Fingers[i][j] - p2.Fingers[i][j]));
                        c2_fingers[i].Add(p1.Fingers[i][j] + p2.Fingers[i][j] + beta * (p2.Fingers[i][j] - p1.Fingers[i][j]));
                    }
                }
            }
            else
            {
                for (var i = 0; i < p1.Fingers.Count; i++)
                {
                    c1_fingers.Add(new List<double>());
                    c2_fingers.Add(new List<double>());
                    for (var j = 0; j < p1.Fingers[i].Count; j++)
                    {
                        c1_fingers[i].Add(p1.Fingers[i][j]);
                        c2_fingers[i].Add(p2.Fingers[i][j]);
                    }
                }
            }
     

 
            return new List<Chromosome>{new Chromosome(c1_arm, c1_fingers, world), new Chromosome(c2_arm, c2_fingers, world)};
        }

        /// <summary>
        /// Operator selekcji metoda turniejowa.
        /// </summary>
        public static List<Chromosome> Selection(List<Chromosome> population, int selSize, int tournament, WorldInstance world)
        {
            if (population.Count == 0) return new List<Chromosome>();
            var selected = new List<Chromosome>();
            var rand = new Random();

            for (var i = 0; i < selSize; i++ )
            {
                var candidates = new List<Chromosome>();
                for (var j =0; j < tournament; j++)
                    candidates.Add(population[rand.Next(population.Count)]);
                selected.Add(candidates.OrderBy(p => p.Score + p.Error).First());
            }

            return selected;
        }

        /// <summary>
        /// Funkcja celu dla chromosomu, moze liczyc odleglosc nadgarstka od pola wskazanego przez heurystyke oraz palcow od celu.
        /// </summary>
        public static Chromosome Evaluate(Chromosome c, WorldInstance world, EvolveChoices whichDistance)
        {
            var dist = new List<KeyValuePair<KeyValuePair<Point, Point>, double>> ();
            foreach (var tp in c.TouchPoints)
                foreach (var t in world.Targets)
                    dist.Add(new KeyValuePair<KeyValuePair<Point, Point>, double>(new KeyValuePair<Point, Point>(tp, t), Geometry.Distance(tp, t, world)));
            var score = 0.0;

            var h = world.heuristic;

            if (whichDistance == EvolveChoices.Fingers || whichDistance == EvolveChoices.All)
            {
                for (var i = 0; i < c.TouchPoints.Count; i++)
                {
                    var ordered = dist.OrderBy(p => p.Value);
                    if (ordered.Count() > 0)
                    {
                        score += ordered.First().Value;
                        var first = ordered.First().Key;
                        dist = dist.Where(p => p.Key.Key != first.Key && p.Key.Value != first.Value).ToList();
                    }
                }
            }

            if (whichDistance == EvolveChoices.Arm)
            {
                var wristEnd = c.BonesArm.Last().P2;
                double min = Double.MaxValue;
                foreach( var p in h.Partitionning)
                {
                    if (p.PossibleWrist)
                    {
                        var d = Geometry.SLDistance(wristEnd, p.Center);
                        if (d < min) min = d;
                    }
                }
                score = min;
            }

            var error = 0.0;
            if (whichDistance == EvolveChoices.All)
            {
                foreach (var b in c.Bones)
                    foreach (var o in world.Obstacles)
                        if (Geometry.Intersects(b, o))
                            error++;
            }
            if (whichDistance == EvolveChoices.Arm)
            {
                foreach (var b in c.BonesArm)
                    foreach (var o in world.Obstacles)
                        if (Geometry.Intersects(b, o))
                            error++;
            }
            if (whichDistance == EvolveChoices.Fingers)
            {
                foreach (var b in c.BonesFingers)
                    foreach (var o in world.Obstacles)
                        if (Geometry.Intersects(b, o))
                            error++;
            }
            
            c.SaveEvaluation(score, error);
            return c;
        }

        public static List<Chromosome> GeneticAlgorithmStart(WorldInstance world, int populationSize, 
            Func<WorldInstance, int, List<Chromosome>> makepopFun,
            Func<Chromosome, WorldInstance, EvolveChoices, Chromosome> evaluateFun, EvolveChoices evolveWhat)
        {
            var p = makepopFun(world, populationSize);
            return p.Select(i => evaluateFun(i, world, evolveWhat)).ToList();
        }

        /// <summary>
        /// Pojedynczy krok algorytmu.
        /// </summary>
        public static List<Chromosome> GeneticAlgorithmStep(WorldInstance world, List<Chromosome> population, double alpha,
            Func<Chromosome, double, WorldInstance, EvolveChoices, Chromosome> mutateFun, double mutationChance,
            Func<List<Chromosome>, int, int, WorldInstance, List<Chromosome>> selectionFun,
            Func<Chromosome, Chromosome, WorldInstance, EvolveChoices, List<Chromosome>> crossoverFun,
            Func<Chromosome, WorldInstance, EvolveChoices, Chromosome> evaluateFun, EvolveChoices evolveWhat)
        {

            var parents = selectionFun(population, population.Count, 4, world);
            var children = new List<Chromosome>();
            for (var i = 0; i < parents.Count; i++)
                children.AddRange(crossoverFun(parents[i], parents[parents.Count - i - 1], world, evolveWhat));
            children = children.Select(c => mutateFun(c, mutationChance, world, evolveWhat)).Select(c => evaluateFun(c, world, evolveWhat)).ToList();
            parents = parents.Select(c => evaluateFun(c, world, evolveWhat)).ToList();
            children.AddRange(parents);
            children = children.Distinct().ToList();
            var good = children.Where(c => c.Error == 0.0).ToList();
            var bad = children.Where(c => c.Error > 0.0).ToList();
            var goodnum = Math.Min(good.Count, population.Count - (int) (alpha*population.Count));
            good = selectionFun(good, goodnum, 4, world);
            bad = selectionFun(bad, population.Count - goodnum, 4, world);
            var res = good.Concat(bad);
            return res.OrderBy(c => c.Score).ToList();
        }
    }
}
