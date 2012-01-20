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
        //public List<T> Leafs;

        public Tree(T node)
        {
            Node = node;
            Subtree1 = null;
            Subtree2 = null;
            //Leafs = new List<T>{node};
        }

        public Tree(T node, Tree<T> tree1, Tree<T> tree2)
        {
            Node = node;
            Subtree1 = tree1;
            Subtree2 = tree2;
            //var leafs = new List<T>();
            //if (tree1 == null && tree2 == null)
            //    leafs.Add(Node);
            //if (tree1 != null)
            //    leafs.AddRange(tree1.Leafs);
            //if (tree2 != null)
            //    leafs.AddRange(tree2.Leafs);
            //Leafs = leafs;
        }

        public Tree(int depth)
        {
            Node = default(T);
            if (depth == 0)
                return;
            Subtree1 = new Tree<T>(depth - 1);
            Subtree2 = new Tree<T>(depth - 1);
            //RestoreLeafs();
        }

        //public List<T> RestoreLeafs()
        //{
        //    var l = new List<T>();
        //    if (Subtree1 != null)
        //        l.AddRange(Subtree1.RestoreLeafs());
        //    if (Subtree2 != null)
        //        l.AddRange(Subtree2.RestoreLeafs());
        //    return l;
        //}

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

        public Tree<S> Map<S>(Func<T, S> map)
        {
            var s1 = Subtree1 == null ? null : Subtree1.Map(map);
            var s2 = Subtree2 == null ? null : Subtree2.Map(map);
            var node = Node == null ? default(S) : map(Node);
            return new Tree<S>(node,s1 , s2);
        }

        public Tree<S> MapTree<S>(Func<Tree<T>, S> map)
        {
            var s1 = Subtree1 == null ? null : Subtree1.MapTree(map);
            var s2 = Subtree2 == null ? null : Subtree2.MapTree(map);
            var node = Node == null ? default(S) : map(this);
            return new Tree<S>(node, s1, s2);
        }

        public override string ToString()
        {
            var n = Node == null ? "%" : Node.ToString();
            var s1 = Subtree1 == null ? "%" : Subtree1.ToString();
            var s2 = Subtree2 == null ? "%" : Subtree2.ToString();
            return "<" + s1 + "|" + n + "|" + s2 + ">";
        }
    }

    class ChromosomeNode
    {
        public double Angle;
        public Line Line;
        public double Error;
        public double Score;

        public ChromosomeNode(double angle, Line line)
        {
            Angle = angle;
            Line = line;
        }
    }

    /// <summary>
    /// Podstawowe informacje o chromosomie, zawierające głównie wartości kątów.
    /// </summary>
    class Chromosome
    {
        public Tree<ChromosomeNode> Tree;


        private static Tree<ChromosomeNode> GenerateTree(Tree<ChromosomeNode> tree, Tree<double> angles, Tree<NodeSpec> spec)
        {
            if (angles.Subtree1 == null && angles.Subtree2 == null)
                return null;

            var angle = Geometry.RelateAngle(tree.Node.Angle, angles.Subtree1.Node);
            var line = new Line(tree.Node.Line.P2, spec.Subtree1.Node.Length, angle);
            tree.Subtree1 = GenerateTree(tree.Subtree1, angles.Subtree1, spec.Subtree1);
            tree.Subtree2 = GenerateTree(tree.Subtree2, angles.Subtree2, spec.Subtree2);
            return tree;
        }

        /// <summary>
        /// Tworzy nowy chromosom
        /// </summary>
        /// <param name="angles">Drzewo kątów</param>
        /// <param name="world">Świat</param>
        public Chromosome(Tree<double> angles, WorldInstance world)
        {
            Tree = new Tree<ChromosomeNode>(new ChromosomeNode(0.0, new Line(world.Start, world.Start)));
            Tree = GenerateTree(Tree, angles, world.Specification.Spec);
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
            Length = double.Parse(spec[0]);
            ArcMin = double.Parse(spec[1]);
            ArcMax = double.Parse(spec[2]);
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
            foreach (var s in spec.Select(sp => sp.Split()))
                Spec.Add(s[0], new NodeSpec(s.Skip(1).ToList()));
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

            Func<NodeSpec, double> randomNode = 
                s => s==null? -1 : s.ArcMin + rand.NextDouble()*(s.ArcMax - s.ArcMin);

            for (var i =0; i < size; i++)
                population.Add(new Chromosome(spec.Spec.Map(randomNode), world));

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
            /*
            var s = Math.Min((float)img.Width / world.SizeX, (float)img.Height / world.SizeY);

            var p = new Pen(pencolor, penwidth);
            var g = Graphics.FromImage(img);

            foreach (var c in population)
            //var c= population.FindAll(o => o.Score < 6)[0];
                foreach (var b in c.Bones)
                    g.DrawLine(p, s*(float) b.P1.X, s*(float) b.P1.Y, s*(float) b.P2.X, s*(float) b.P2.Y);
                
            g.DrawImage(img, 0, 0, img.Width, img.Height);
            g.Dispose();
             */
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
            /*
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
             */
            return null;
        }

        /// <summary>
        /// Krzyzowanie dwoch chromosomow. Jezeli jakiegos fragmentu nie chcemy zmieniac, to jest on dziedziczony od rodzicow wprost.
        /// </summary>
        public static List<Chromosome> Crossover(Chromosome p1, Chromosome p2, WorldInstance world, EvolveChoices evolveWhat)
        {
            /*
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
             */
            return null;
        }

        /// <summary>
        /// Operator selekcji metoda turniejowa.
        /// </summary>
        public static List<Chromosome> Selection(List<Chromosome> population, int selSize, int tournament, WorldInstance world)
        {
            /*
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
             */
            return null;
        }

        /// <summary>
        /// Funkcja celu dla chromosomu, moze liczyc odleglosc nadgarstka od pola wskazanego przez heurystyke oraz palcow od celu.
        /// </summary>
        public static Chromosome Evaluate(Chromosome c, WorldInstance world, EvolveChoices whichDistance)
        {
            /*
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
             */
            return null;
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
            /*
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
             */
            return null;
        }
    }
}
