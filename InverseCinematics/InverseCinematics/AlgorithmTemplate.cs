using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace InverseCinematics
{
    class Chromosome
    {
        public List<double> Arm;
        public List<List<double>> Fingers;
        public List<Point> TouchPoints;
        public List<Line> Bones;

        public Chromosome(List<double> arm, List<List<double>> fingers, WorldInstance world)
        {
            Arm = arm;
            Fingers = fingers;


            var angle = 0.0; // Initial angle is north
            var point = world.Start;
            Bones = new List<Line>();
            TouchPoints = new List<Point>();

            for (var i = 0; i < arm.Count; i++)
            {
                Bones.Add(new Line(point, world.Specification.ArmArcLen[i], Arm[i]));
                point = Bones[i].P2;
            }

            for (var i = 0; i < Fingers.Count; i++)
            {
                var point2 = point;
                for (var j = 0; j < fingers[i].Count; j++)
                {
                    Bones.Add(new Line(point2, world.Specification.FingersArcLen[i][j], Fingers[i][j]));
                    point2 = Bones.Last().P2;
                }
                TouchPoints.Add(point2);
            }

        }


    }

    class Specification
    {
        public List<double> ArmArcLen;
        public List<double> ArmArcMin;
        public List<double> ArmArcMax;
        public List<List<double>> FingersArcLen;
        public List<List<double>> FingersArcMin;
        public List<List<double>> FingersArcMax;

        public Specification(List<double> armArcLen, List<double> armArcMin, List<double> armArcMax, List<List<double>> fingersArcLen, List<List<double>> fingersArcMin, List<List<double>> fingersArcMax)
        {
            ArmArcLen = armArcLen;
            ArmArcMin = armArcMin;
            ArmArcMax = armArcMax;
            FingersArcLen = fingersArcLen;
            FingersArcMin = fingersArcMin;
            FingersArcMax = fingersArcMax;
        }

        public Specification(int targets, List<string> spec)
        {
            ArmArcLen = new List<double>();
            ArmArcMin = new List<double>();
            ArmArcMax = new List<double>();
            FingersArcLen = new List<List<double>>();
            FingersArcMin = new List<List<double>>();
            FingersArcMax = new List<List<double>>();

            var armSize = int.Parse(spec[0]);

            for (var i = 0; i < armSize; i++)
            {
                var part = spec[1 + i].Split();
                ArmArcLen.Add(double.Parse(part[0]));
                ArmArcMin.Add(double.Parse(part[1]));
                ArmArcMax.Add(double.Parse(part[2]));
            }

            var size = 1 + armSize;
            for (var i = 0; i < targets; i++)
            {
                FingersArcLen.Add(new List<double>());
                FingersArcMin.Add(new List<double>());
                FingersArcMax.Add(new List<double>());
                var len = int.Parse(spec[size]);
                for (var j = 0; j < len; j++)
                {
                    var part = spec[size + 1 + i].Split();
                    FingersArcLen[i].Add(double.Parse(part[0]));
                    FingersArcMin[i].Add(double.Parse(part[1]));
                    FingersArcMax[i].Add(double.Parse(part[2]));
                }
                size += 1 + len;
            }
        }
    }

    class AlgorithmTemplate
    {
        public static List<Chromosome> GenerateRandomPopulation(WorldInstance world, int size)
        {
            var rand = new Random();
            var population = new List<Chromosome>();

            var spec = world.Specification;

            for (var i =0; i < size; i++)
            {
                var arm = new List<double>();
                var fingers = new List<List<double>>();

                for (var k = 0; k < spec.ArmArcLen.Count; k++)
                    arm.Add(spec.ArmArcMin[k] + rand.NextDouble() * (spec.ArmArcMax[k] - spec.ArmArcMin[k]));

                for (var k = 0; k < spec.FingersArcLen.Count; k++)
                {
                    fingers.Add(new List<double>());
                    for (var l = 0; l < spec.FingersArcLen[k].Count; l++)
                        fingers[k].Add(spec.FingersArcMin[k][l] + rand.NextDouble()*(spec.FingersArcMax[k][l] - spec.FingersArcMin[k][l]));
                }
                population.Add(new Chromosome(arm, fingers, world));
            }

            return population;
        }

        public static Bitmap PrintPopulation(WorldInstance world, List<Chromosome> population, Bitmap img, float penwidth)
        {
            var s = Math.Min((float)img.Width / world.SizeX, (float)img.Height / world.SizeY);

            var p = new Pen(Color.Blue, penwidth);
            var g = Graphics.FromImage(img);

            foreach (var c in population)
                foreach (var b in c.Bones)
                    g.DrawLine(p, s*(float) b.P1.X, s*(float) b.P1.Y, s*(float) b.P2.X, s*(float) b.P2.Y);
                
            g.DrawImage(img, 0, 0, img.Width, img.Height);
            g.Dispose();
            return img;
        }
    
        
        public static Chromosome Mutate(Chromosome before, double chance, WorldInstance world)
        {
            var rand = new Random();
            var spec = world.Specification;
            var arm = before.Arm;
            var fingers = before.Fingers;

            for (var i = 0; i < arm.Count; i++)
                if (rand.NextDouble() < chance)
                    arm[i] = spec.ArmArcMin[i] + rand.NextDouble()*(spec.ArmArcMax[i] - spec.ArmArcMin[i]);
            
            for (var i = 0; i < fingers.Count; i++)
                for (var j = 0; j < fingers[i].Count; j++)
                    if (rand.NextDouble() < chance)
                        fingers[i][j] = spec.FingersArcMin[i][j] + rand.NextDouble()*(spec.FingersArcMax[i][j] - spec.FingersArcMin[i][j]);
                                   
            return new Chromosome(arm, fingers, world);
        }


        public static List<Chromosome> Crossover(Chromosome p1, Chromosome p2, WorldInstance world)
        {
            var beta = new Random().NextDouble();
            var c1_arm = new List<double>();
            var c1_fingers = new List<List<double>>();
            var c2_arm = new List<double>();
            var c2_fingers = new List<List<double>>();
            
            for (var i = 0; i < p1.Arm.Count; i++)
            {
                c1_arm.Add(p1.Arm[i] + p2.Arm[i] + beta * (p1.Arm[i] - p2.Arm[i]));
                c2_arm.Add(p1.Arm[i] + p2.Arm[i] + beta * (p2.Arm[i] - p1.Arm[i]));
            }

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

            return new List<Chromosome>{new Chromosome(c1_arm, c1_fingers, world), new Chromosome(c2_arm, c2_fingers, world)};
        }

        public static List<Chromosome> Selection() //TODO
        {
            return new List<Chromosome>();
        }


        public static double Evaluate( out double error) // TODO
        {

            error = 0.0;
            return 1.0;
        }

        public static void GeneticAlgorithmTemplate(WorldInstance world, int populationSize, int generations, Func<Chromosome, double, WorldInstance, Chromosome> mutateFun)
        {
            



        }

        //TODO Genetic Algorithm Step (Zwraca populację, którą można później wyświetlić)
    }
}
