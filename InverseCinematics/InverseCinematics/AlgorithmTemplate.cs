using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InverseCinematics
{
    class Chromosome
    {
        public List<double> Arm;
        public List<List<double>> Fingers;

        public Chromosome(List<double> arm, List<List<double>> fingers)
        {
            Arm = arm;
            Fingers = fingers;
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

    class World
    {
        public int N;
        public int SizeX;
        public int SizeY;
        public List<KeyValuePair<double, double>> Targets;
        // Obstacles
    }

    class AlgorithmTemplate
    {
        public static List<Chromosome> GenerateRandomPopulation(Specification spec, int size)
        {
            var rand = new Random();
            var population = new List<Chromosome>();

            for (var i =0; i < size; i++)
            {
                var arm = new List<double>();
                var fingers = new List<List<double>>();

                for (var k = 0; k < spec.ArmArcLen.Count; k++)
                    arm.Add(spec.ArmArcMin[k] + rand.NextDouble() * (spec.ArmArcMax[k] - spec.ArmArcMin[k]));

                for (var k = 0; k < spec.FingersArcLen.Count; k++)
                {
                    fingers.Add(new List<double>());
                    for (var l = 0; l < spec.FingersArcLen[l].Count; l++)
                        fingers[k].Add(spec.FingersArcMin[k][l] + rand.NextDouble()*(spec.FingersArcMax[k][l] - spec.FingersArcMin[k][l]));
                }
                population.Add(new Chromosome(arm, fingers));
            }

            return population;
        }



    }
}
