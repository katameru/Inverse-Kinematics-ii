﻿using System;
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

        public Tree(T node)
        {
            Node = node;
            Subtree1 = null;
            Subtree2 = null;
        }

        public Tree(T node, Tree<T> tree1, Tree<T> tree2)
        {
            Node = node;
            Subtree1 = tree1;
            Subtree2 = tree2;
        }

        public Tree(int depth)
        {
            Node = default(T);
            if (depth == 0)
                return;
            Subtree1 = new Tree<T>(depth - 1);
            Subtree2 = new Tree<T>(depth - 1);
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

        public T Get(string path)
        {
            if (path == "")
                return Node;
            if (path[0] == 'L' && Subtree1 != null)
                return Subtree1.Get(path.Remove(0, 1));
            if (path[0] == 'R' && Subtree2 != null)
                return Subtree2.Get(path.Remove(0, 1));
            return Node;
        }

        public Tree<T> GetSubtree(string path)
        {
            if (path == "")
                return this;
            if (path[0] == 'L' && Subtree1 != null)
                return Subtree1.GetSubtree(path.Remove(0, 1));
            if (path[0] == 'R' && Subtree2 != null)
                return Subtree2.GetSubtree(path.Remove(0, 1));
            return this;
        }

        public Tree<S> Map<S>(Func<T, S> map)
        {
            var s1 = Subtree1 == null ? null : Subtree1.Map(map);
            var s2 = Subtree2 == null ? null : Subtree2.Map(map);
            var node = Node == null ? default(S) : map(Node);
            return new Tree<S>(node,s1 , s2);
        }

        public static Tree<U> Map2<T, S, U>(Tree<T> treeA, Tree<S> treeB, Func<T, S, U> map)
        {
            var s1 = treeA.Subtree1 == null ? null : Map2(treeA.Subtree1, treeB.Subtree1, map);
            var s2 = treeA.Subtree2 == null ? null : Map2(treeA.Subtree2, treeB.Subtree2, map);
            var node = treeA.Node == null ? default(U) : map(treeA.Node, treeB.Node);
            return new Tree<U>(node,s1 , s2);
        }

        public Tree<S> MapTree<S>(Func<Tree<T>, S> map)
        {
            var s1 = Subtree1 == null ? null : Subtree1.MapTree(map);
            var s2 = Subtree2 == null ? null : Subtree2.MapTree(map);
            var node = Node == null ? default(S) : map(this);
            return new Tree<S>(node, s1, s2);
        }

        public List<S> Foldr<S>(Func<T, S> fold)
        {
            var l = Node == null ? new List<S>() : new List<S> {fold(Node)};
            if (Subtree1 != null)
                l.AddRange(Subtree1.Foldr(fold));
            if (Subtree2 != null)
                l.AddRange(Subtree2.Foldr(fold));
            return l;
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
            double angle;
            Line line;

            if (angles.Subtree1 != null)
            {
                angle = Geometry.RelateAngle(tree.Node.Angle, angles.Subtree1.Node);
                line = new Line(tree.Node.Line.P2, spec.Subtree1.Node.Length, angle);
                tree.Subtree1 = new Tree<ChromosomeNode>(new ChromosomeNode(angle, line));
                tree.Subtree1 = GenerateTree(tree.Subtree1, angles.Subtree1, spec.Subtree1);
            }
            if (angles.Subtree2 != null)
            {
                angle = Geometry.RelateAngle(tree.Node.Angle, angles.Subtree2.Node);
                line = new Line(tree.Node.Line.P2, spec.Subtree2.Node.Length, angle);
                tree.Subtree2 = new Tree<ChromosomeNode>(new ChromosomeNode(angle, line));
                tree.Subtree2 = GenerateTree(tree.Subtree2, angles.Subtree2, spec.Subtree2);
            }
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

            Func<NodeSpec, double> randomNode = s => s==null? -1 : s.ArcMin + rand.NextDouble()*(s.ArcMax - s.ArcMin);

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
            var s = Math.Min((float)img.Width / world.SizeX, (float)img.Height / world.SizeY);

            var p = new Pen(pencolor, penwidth);
            var g = Graphics.FromImage(img);

            foreach (var c in population)
                foreach (var l in c.Tree.Foldr(n => n.Line))
                    g.DrawLine(p, s*(float) l.P1.X, s*(float) l.P1.Y, s*(float) l.P2.X, s*(float) l.P2.Y);
    
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
        /// <param name="rand">Generator liczb losowych</param>
        /// <returns></returns>
        public static Chromosome Mutate(Chromosome before, double chance, WorldInstance world, Random rand)
        {
            Func<NodeSpec, ChromosomeNode, double> mutateNode = (s, c) =>
                                                                        rand.NextDouble() < chance
                                                                            ? (s == null
                                                                                   ? -1
                                                                                   : s.ArcMin +
                                                                                     rand.NextDouble()*
                                                                                     (s.ArcMax - s.ArcMin))
                                                                            : c.Angle;
            
            return new Chromosome(Tree<double>.Map2(world.Specification.Spec, before.Tree, mutateNode), world);
        }

        /// <summary>
        /// Krzyzowanie dwoch chromosomow. Jezeli jakiegos fragmentu nie chcemy zmieniac, to jest on dziedziczony od rodzicow wprost.
        /// </summary>
        public static List<Chromosome> Crossover(Chromosome p1, Chromosome p2, WorldInstance world)
        {
            var beta = new Random().NextDouble();
            Func<ChromosomeNode, ChromosomeNode, double> catf1 = (n1, n2) => n1.Angle + n2.Angle + beta * (n1.Angle - n2.Angle);
            Func<ChromosomeNode, ChromosomeNode, double> catf2 = (n1, n2) => n1.Angle + n2.Angle + beta * (n2.Angle - n1.Angle);
            var cat1 = Tree<double>.Map2(p1.Tree, p2.Tree, catf1);
            var cat2 = Tree<double>.Map2(p1.Tree, p2.Tree, catf2);
            return new List<Chromosome>{new Chromosome(cat1, world), new Chromosome(cat2, world)};
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

        public static void Evaluate(ref Tree<ChromosomeNode> tree, ref List<Point> targets, List<Line> obstacles)
        {
            var error = 0;
            foreach (var o in obstacles)
                if (tree.Node.Line.P1 != tree.Node.Line.P2 && Geometry.Intersects(tree.Node.Line, o))
                    error++;

            if (tree.Subtree1 == null && tree.Subtree2 == null)
            {
                tree.Node.Error = error;
                tree.Node.Score = Geometry.SLDistance(tree.Node.Line.P2, targets.First());
                targets = targets.Skip(1).ToList();
                return;
            }
            Evaluate(ref tree.Subtree1, ref targets, obstacles);
            Evaluate(ref tree.Subtree2, ref targets, obstacles);
            tree.Node.Score = tree.Subtree1.Node.Score + tree.Subtree2.Node.Score;
            tree.Node.Error = error + tree.Subtree1.Node.Error + tree.Subtree2.Node.Error;
        }

        /// <summary>
        /// Funkcja celu dla chromosomu, moze liczyc odleglosc nadgarstka od pola wskazanego przez heurystyke oraz palcow od celu.
        /// </summary>
        public static Chromosome Evaluate(Chromosome c, WorldInstance world)
        {
            var targets = world.Targets.Select(t => t).ToList();
            Evaluate(ref c.Tree, ref targets, world.Obstacles);

            return c;
        }

        public static List<Chromosome> GeneticAlgorithmStart(WorldInstance world, int populationSize, 
            Func<WorldInstance, int, List<Chromosome>> makepopFun,
            Func<Chromosome, WorldInstance, Chromosome> evaluateFun)
        {
            var p = makepopFun(world, populationSize);
            return p.Select(i => evaluateFun(i, world)).OrderBy(c => c.Tree.Node.Score).ToList();
        }

        /// <summary>
        /// Pojedynczy krok algorytmu.
        /// </summary>
        public static List<Chromosome> GeneticAlgorithmStep(WorldInstance world, List<Chromosome> population, double alpha,
            Func<Chromosome, double, WorldInstance, Random, Chromosome> mutateFun, double mutationChance,
            Func<List<Chromosome>, int, int, WorldInstance, List<Chromosome>> selectionFun,
            Func<Chromosome, Chromosome, WorldInstance, List<Chromosome>> crossoverFun,
            Func<Chromosome, WorldInstance, Chromosome> evaluateFun)
        {
            /*
            var parents = selectionFun(population, population.Count, 4, world);
            var children = new List<Chromosome>();
            for (var i = 0; i < parents.Count; i++)
                children.AddRange(crossoverFun(parents[i], parents[parents.Count - i - 1], world, evolveWhat));
             */
            var children = population;
            var rand = new Random();
            children = children.Select(c => mutateFun(c, mutationChance, world, rand)).Select(c => evaluateFun(c, world)).ToList();
            /*
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
            return children.OrderBy(c => c.Tree.Node.Score).ToList(); ;
        }
    }
}
