using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace InverseCinematics
{

    [Serializable]
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

        public void AddSubtree(string path, Tree<T> tree)
        {
            if (path == "")
                return;
            if (path == "L")
            {
                Subtree1 = tree;
                return;
            }
            if (path == "R")
            {
                Subtree2 = tree;
                return;
            }
            if (path[0] == 'L')
                Subtree1.AddSubtree(path.Remove(0, 1), tree);
            else
                Subtree2.AddSubtree(path.Remove(0, 1), tree);
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
            //return "(" + s1 + "\t|\t" + n + "\t|\t" + s2 + ")";
            return n + "   *(" + s1 + ")*      *("+ s2 + ")*";
        }
    }

    [Serializable]
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

        public override string ToString()
        {
            //return "<" + Angle + " ; " + Line + " [" + Score + ", " + Error + ">";
            return "<" + Score + ", " + Error + "> #" + Angle + " " + Line;
        }
    }

    /// <summary>
    /// Podstawowe informacje o chromosomie, zawierające głównie wartości kątów.
    /// </summary>
    [Serializable]
    class Chromosome
    {
        public Tree<ChromosomeNode> Tree;

        public override string ToString()
        {
            return Tree.ToString();
        }

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

        public Chromosome recalculate(Tree<NodeSpec> spec)
        {
            var tree = Tree;
            Tree<double> angles = tree.Map(n => n.Angle);
            Tree = GenerateTree(tree, angles, spec);
            return this;
        }
    }

    [Serializable]
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
    [Serializable]
    class Specification
    {
        public Tree<NodeSpec> Spec = new Tree<NodeSpec>(null);
        public int Depth;
        public Dictionary<int,List<string>> Paths;

        /// <summary>
        /// Tworzy specyfikację na podstawie konkretnych danych.
        /// </summary>
        public Specification(int depth, Tree<NodeSpec> spec)
        {
            Depth = depth;
            Spec = spec;
            generatePaths();
        }

        /// <summary>
        /// Wczytuje specyfikację z danych pobranych z pliku.
        /// </summary>
        /// <param name="spec">Kolejne linie pliku wejściowego</param>
        public Specification(int depth, List<string> spec)
        {
            Depth = depth;
            generatePaths();
            Spec = new Tree<NodeSpec>(depth);
            foreach (var s in spec.Select(sp => sp.Split()))
                Spec.Add(s[0], new NodeSpec(s.Skip(1).ToList()));
        }

        private void generatePaths()
        {
            Paths = new Dictionary<int, List<string>>();
            Paths.Add(1, new List<string>{"L", "R"});

            for (var d = 2; d <= Depth; d++)
            {
                Paths.Add(d, new List<string>());
                foreach (var path in Paths[d-1])
                {
                    Paths[d].Add(path + "L");
                    Paths[d].Add(path + "R");
                }
            }
        }
    }

    class AlgorithmTemplate
    {
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

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
        /// Mutacja chromosomu
        /// </summary>
        /// <param name="before">chromosom do mutacji</param>
        /// <param name="chance">szansa na mutacje</param>
        /// <param name="world">swiat</param>
        /// <param name="rand">Generator liczb losowych</param>
        /// <returns></returns>
        public static Chromosome Mutate2(Chromosome before, double chance, WorldInstance world, Random rand, int generation, double adjustment)
        {
            int lvl = generation/100 + rand.Next(3);
            if (lvl < 1)
                lvl = 1;
            if (lvl > world.Specification.Depth)
                lvl = world.Specification.Depth;

            foreach (var path in world.Specification.Paths[lvl])
            {
                var node = before.Tree.Get(path);
                if (rand.NextDouble() > chance)
                    continue;
                if (node.Score == 0 && node.Error == 0)
                    continue;

                var angles = world.Specification.Spec.Get(path);
                var delta = (angles.ArcMax - angles.ArcMin) * 2 * ( adjustment * rand.NextDouble() - 0.5 );
                if ((node.Angle + delta)%360 < angles.ArcMin)
                    node.Angle = angles.ArcMin;
                else if ((node.Angle + delta)%360 > angles.ArcMax)
                    node.Angle = angles.ArcMax;
                else
                    node.Angle = (node.Angle + delta)%360;
            }
            return before; // zwraca zmodyfikowany chromosom!!
        }

        public static Chromosome Mutate3(Chromosome before, double chance, WorldInstance world, Random rand)
        {
            var rl = rand.Next(world.Specification.Paths.Count);
            var randomLevel = world.Specification.Paths[rl+1];
            var randomPath = randomLevel[rand.Next(randomLevel.Count)];
            var randomNode = before.Tree.Get(randomPath);

            if (rand.NextDouble() > chance) return before;

            var t = rand.NextDouble();
            var x = 4 * (t - 0.5);
            var r = 1 - Math.Pow(Math.E, -Math.Pow(x, 2));
            var sign = rand.NextDouble() < 0.5 ? -1 : 1;

            var angles = world.Specification.Spec.Get(randomPath);
            var delta = sign * (angles.ArcMax - angles.ArcMin) * r * r;
            if ((randomNode.Angle + delta) % 360 < angles.ArcMin)
                randomNode.Angle = angles.ArcMin;
            else if ((randomNode.Angle + delta) % 360 > angles.ArcMax)
                randomNode.Angle = angles.ArcMax;
            else
                randomNode.Angle = (randomNode.Angle + delta) % 360;

            return before;
        }

        /// <summary>
        /// Crossover odbywa się dla pewnej liczby rodziców i poddrzew. 
        /// Dziecko otzrymuje pewną średnią dwóch najlepszych rodziców na danym poddrzewie
        /// lub dziedziczy poddrzewo bezpośrednio od najlepszego (na tym poddrzewie) rodzica.
        /// </summary>
        public static List<Chromosome> Crossover(List<Chromosome> population, List<string> paths, WorldInstance world, int tournament, int popSize, double chance, int evalId)
        {
            population.Select(c => Evaluate(c, world, evalId));
            var rand = new Random();
            var children = new List<Chromosome>();

            var parentsList = new List<List<Chromosome>>();

            for (var s = 0; s < popSize; s++)
            {
                var parents = new List<Chromosome>();
                for (var j = 0; j < tournament; j++) // tworzymy listę rodziców z których będzie stworzony dany potomek
                {
                    parents.Add(population[rand.Next(population.Count)]);
                }

                parentsList.Add(parents);

            }

            parentsList.AsParallel().ForAll(parents =>
            {
                var child = DeepClone(parents[rand.Next(tournament)]); // jako podstawę wybieramy losowego rodzica

                foreach (var path in paths.OrderBy(p => p.Count())) // zaczynamy podstawianie od drzew najbliżej roota
                {
                    var best = parents.OrderBy(p => p.Tree.Get(path).Score + p.Tree.Get(path).Error).ToList();

                    if (rand.NextDouble() < chance) // dziedziczenie wprost
                    {
                        child.Tree.AddSubtree(path, DeepClone(best.First().Tree).GetSubtree(path));
                    }
                    else
                    {
                        Func<ChromosomeNode, ChromosomeNode, double> f;
                        var beta = 2 * rand.NextDouble() - 1;
                        f = (n1, n2) => (n1.Angle + n2.Angle + beta * (n1.Angle - n2.Angle)) % 360;
                        
                        var tree = Tree<double>.Map2(best[0].Tree.GetSubtree(path), best[rand.Next(tournament - 1) + 1].Tree.GetSubtree(path), f);
                        var tree2 = Tree<ChromosomeNode>.Map2(child.Tree.GetSubtree(path), tree, (c, t) => new ChromosomeNode(t, c.Line));
                        child.Tree.AddSubtree(path, tree2);
                    }
                }
                lock (children)
                {
                    children.Add(child);
                }
            });
            return children;
        }

        public static List<Chromosome> Crossover2(List<Chromosome> population, List<string> paths, WorldInstance world, int tournament, int popSize, double chance, int evalId)
        {
            population.Select(c => Evaluate(c, world, evalId));
            var rand = new Random();
            var children = new List<Chromosome>();

            var parentsList = new List<List<Chromosome>>();

            for (var s = 0; s < popSize; s++)
            {
                var parents = new List<Chromosome>();
                for (var j = 0; j < tournament; j++) // tworzymy listę rodziców z których będzie stworzony dany potomek
                {
                    parents.Add(population[rand.Next(population.Count)]);
                }

                parentsList.Add(parents);

            }

            parentsList.AsParallel().ForAll(parents =>
            {
                var child1 = DeepClone(parents[rand.Next(tournament)]); // jako podstawę wybieramy losowego rodzica
                var child2 = DeepClone(child1);
                var beta = rand.NextDouble();

                foreach (var path in paths.OrderBy(p => p.Count())) // zaczynamy podstawianie od drzew najbliżej roota
                {
                    var best = parents.OrderBy(p => p.Tree.Get(path).Score + p.Tree.Get(path).Error).ToList();
                    Func<ChromosomeNode, ChromosomeNode, double> f, g;
                    
                    f = (n1, n2) => (n1.Angle + n2.Angle + beta * (n1.Angle - n2.Angle)) % 360;
                    g = (n1, n2) => (n1.Angle + n2.Angle + beta * (n2.Angle - n1.Angle)) % 360;
                    
                    var ftree = Tree<double>.Map2(best[0].Tree.GetSubtree(path), best[rand.Next(tournament - 1) + 1].Tree.GetSubtree(path), f);
                    var ftree2 = Tree<ChromosomeNode>.Map2(child1.Tree.GetSubtree(path), ftree, (c, t) => new ChromosomeNode(t, c.Line));
                    child1.Tree.AddSubtree(path, ftree2);

                    var gtree = Tree<double>.Map2(best[0].Tree.GetSubtree(path), best[rand.Next(tournament - 1) + 1].Tree.GetSubtree(path), g);
                    var gtree2 = Tree<ChromosomeNode>.Map2(child1.Tree.GetSubtree(path), gtree, (c, t) => new ChromosomeNode(t, c.Line));
                    child2.Tree.AddSubtree(path, gtree2);
                }
                child1.recalculate(world.Specification.Spec);
                child2.recalculate(world.Specification.Spec);

                var child = child1.Tree.Node.Score < child2.Tree.Node.Score ? child1 : child2;

                lock (children)
                {
                    children.Add(child);
                }
            });
            return children;
        }


        public static void Evaluate(ref Tree<ChromosomeNode> tree, ref List<Point> targets, List<Line> obstacles, int id)
        {
            var error = 0;
            foreach (var o in obstacles)
                if (tree.Node.Line.P1 != tree.Node.Line.P2 && Geometry.Intersects(tree.Node.Line, o))
                    error++;

            if (tree.Subtree1 == null && tree.Subtree2 == null)
            {
                tree.Node.Error = error;
                if (id == 0)
                    tree.Node.Score = Geometry.SLDistance(tree.Node.Line.P2, targets.First());
                if (id == 1)
                    tree.Node.Score = Math.Pow(Geometry.SLDistance(tree.Node.Line.P2, targets.First()), 2);
                targets = targets.Skip(1).ToList();
                return;
            }
            Evaluate(ref tree.Subtree1, ref targets, obstacles, id);
            Evaluate(ref tree.Subtree2, ref targets, obstacles, id);
            tree.Node.Score = tree.Subtree1.Node.Score + tree.Subtree2.Node.Score;
            tree.Node.Error = error + tree.Subtree1.Node.Error + tree.Subtree2.Node.Error;
        }

        /// <summary>
        /// Funkcja celu dla chromosomu, moze liczyc odleglosc nadgarstka od pola wskazanego przez heurystyke oraz palcow od celu.
        /// </summary>
        public static Chromosome Evaluate(Chromosome c, WorldInstance world, int id)
        {
            c.recalculate(world.Specification.Spec);
            var targets = world.Targets.Select(t => t).ToList();
            Evaluate(ref c.Tree, ref targets, world.Obstacles, id);

            return c;
        }

        public static List<Chromosome> GeneticAlgorithmStart(WorldInstance world, int populationSize, int evaluateId)
        {
            var p = GenerateRandomPopulation(world, populationSize);
            return p.Select(i => Evaluate(i, world, evaluateId)).OrderBy(c => c.Tree.Node.Score).ToList();
        }

        /// <summary>
        /// Pojedynczy krok algorytmu.
        /// </summary>
        public static List<Chromosome> GeneticAlgorithmStep(WorldInstance world, List<Chromosome> population, double alpha,
            double mutationChance, int generation, double adjustment, int tournament, double explicite,
            int mutationId, int crossoverId, int evaluateId)
        {
            var selectionPaths = new List<string> {"L", "R"};
            List<Chromosome> cross = new List<Chromosome>();
            if (crossoverId == 0)
                cross = Crossover(population, selectionPaths, world, tournament, population.Count, explicite, evaluateId);
            if (crossoverId == 1)
                cross = Crossover2(population, selectionPaths, world, tournament, population.Count, explicite, evaluateId);
            //if (cross.Any(c => c == null)) { System.Diagnostics.Debugger.Break(); }
            var children = cross.Select(c => Evaluate(c, world, evaluateId)).ToList();

            var rand = new Random();
            if (mutationId == 0)
                children = children.AsParallel().Select(c => Mutate(c, mutationChance, world, rand)).Select(c => Evaluate(c, world, evaluateId)).ToList();
            if (mutationId == 1)
                children = children.AsParallel().Select(c => Mutate2(c, mutationChance, world, rand, generation, adjustment)).Select(c => Evaluate(c, world, evaluateId)).ToList();
            if (mutationId == 2)
                children = children.AsParallel().Select(c => Mutate3(c, mutationChance, world, rand)).Select(c => Evaluate(c, world, evaluateId)).ToList();
            children.AddRange(population);

            children.AddRange(children);
            children = children.OrderBy(c => c.Tree.Node.Score).Distinct().ToList();
            
            var good = children.Where(c => c.Tree.Node.Error == 0.0).ToList();
            var bad = children.Where(c => c.Tree.Node.Error > 0.0).ToList();
            var goodnum = Math.Min(good.Count, population.Count - (int) (alpha*population.Count));
            
            var res = good.Take(goodnum).Concat(bad).Take(population.Count);

            return res.ToList();
        }
    }
}
