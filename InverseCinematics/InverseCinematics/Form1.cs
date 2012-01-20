using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace InverseCinematics
{
    public partial class Form1 : Form
    {
        private bool _started = false;
        private const int _showbest = 1;

        private WorldInstance _world;
        private int _populationSize;
        private int _generationsArm;
        private int _generationsFingers;
        private int _generationsAll;
        private double _badguys;
        private double _mutation;
        private Bitmap _baseImage;
        private List<Chromosome> _population;
              
        private bool LoadData()
        {
            _generationsArm = 0;
            _generationsFingers = 0;
            _generationsAll = 0;
            label13.Text = "";
            label14.Text = "";
            label15.Text = "";
            label16.Text = "";
            label17.Text = "";
            label18.Text = "";
            labelGAll.Text = @"0";
            labelGFingers.Text = @"0";
            labelGArms.Text = @"0";

            try
            {
                _world = new WorldInstance(comboBox1.Text);
                var x = _world.Specification.Spec.Map(s => s.Length);// Func<NodeSpec, double>{s => s})
                //TODO _world.heuristic = new Heuristics(_world, (int)numericUpDown2.Value);
                _baseImage = new Bitmap(_world.ShowWorld(pictureBox1.Width, pictureBox1.Height, 2.0f));
                _populationSize = (int) numericUpDown1.Value;
                _badguys = (double) numericUpDown3.Value/100;
                _mutation = (double)numericUpDown4.Value / 100;
            }
            catch (Exception e)
            {
                label5.ForeColor = Color.Red;
                label5.Text = e.Message;
                return false;
            }
            label5.ForeColor = Color.Green;
            label5.Text = @"Scenariusz wczytany poprawnie.";
            return true;
        }

        private void UpdateStats()
        {
            /*
            labelGAll.Text = _generationsAll.ToString();
            labelGFingers.Text = _generationsFingers.ToString();
            labelGArms.Text = _generationsArm.ToString();

            var c =_population.First();
            UpdateLabel(label13, c.Score);
            UpdateLabel(label14, c.Error);
            var p = _population.Where(x => x.Error == 0);
            if (p.Count() > 0)
                UpdateLabel(label15, p.First().Score);
            else
                label15.Text = "";
            
            var avgScore = _population.Average(x => x.Score);
            var avgScore2 = _population.Average(x => x.Score*x.Score);
            var avgError = _population.Average(x => x.Error);
            var avgError2 = _population.Average(x => x.Error * x.Error);
            UpdateLabel(label17, avgScore);
            UpdateLabel(label18, avgError, true);
            UpdateLabel(label24, avgScore2 - avgScore * avgScore);
            UpdateLabel(label23, avgError2 - avgError * avgError);

            UpdateLabel(label29, _population.Distinct().Count() / (double)_populationSize, true);
             * */
        }

        private void UpdateLabel(Label l, double v, bool inverse=false)
        {
            if (l.Text == "" || (v < double.Parse(l.Text) && !inverse) || (inverse && v > double.Parse(l.Text)))
                l.ForeColor = Color.Green;
            else if (v == double.Parse(l.Text))
                l.ForeColor = Color.Orange;
            else
                l.ForeColor = Color.Red;
            l.Text = v.ToString();
        }

        public Form1()
        {
            InitializeComponent();
            label5.ForeColor = Color.Red;
            button2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var b = LoadData();
            button2.Enabled = b;
            button3.Enabled = b;

            if (b)
            {
                pictureBox1.Image = _baseImage;
                _population = AlgorithmTemplate.GeneticAlgorithmStart(_world, _populationSize,
                                                                      AlgorithmTemplate.GenerateRandomPopulation,
                                                                      AlgorithmTemplate.Evaluate, EvolveChoices.All);
                UpdateStats();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var ga = (int)numericGArm.Value;
            var gf = (int) numericGFingers.Value;

            for (var i = 0; i < ga; i++ )
                _population = AlgorithmTemplate.GeneticAlgorithmStep(_world, _population, _badguys,
                    AlgorithmTemplate.Mutate, _mutation,
                    AlgorithmTemplate.Selection, AlgorithmTemplate.Crossover,
                    AlgorithmTemplate.Evaluate, EvolveChoices.Arm);
            for (var i = 0; i < gf; i++)
                _population = AlgorithmTemplate.GeneticAlgorithmStep(_world, _population, _badguys,
                    AlgorithmTemplate.Mutate, _mutation,
                    AlgorithmTemplate.Selection, AlgorithmTemplate.Crossover,
                    AlgorithmTemplate.Evaluate, EvolveChoices.Fingers);

            var img2 = AlgorithmTemplate.PrintPopulation(_world, _population.Take(_showbest).ToList(), new Bitmap(_baseImage), 1.0f, Color.Blue);
            var p2 = _population;//TODO.Where(x => x.Error == 0);
            if (p2.Count() > 0)
                pictureBox1.Image = AlgorithmTemplate.PrintPopulation(_world, p2.Take(_showbest).ToList(), img2, 1.0f, Color.Green);
            else
                pictureBox1.Image = img2;

            _generationsArm += ga;
            _generationsFingers += gf;
            _generationsAll += ga + gf;
            UpdateStats();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _population = AlgorithmTemplate.GeneticAlgorithmStep(_world, _population, _badguys,
                AlgorithmTemplate.Mutate, _mutation,
                AlgorithmTemplate.Selection, AlgorithmTemplate.Crossover,
                AlgorithmTemplate.Evaluate, EvolveChoices.All);

            var img2 = AlgorithmTemplate.PrintPopulation(_world, _population.Take(_showbest).ToList(), new Bitmap(_baseImage), 1.0f, Color.Blue);
            var p2 = _population;//TODO.Where(x => x.Error == 0);
            if (p2.Count() > 0)
                pictureBox1.Image = AlgorithmTemplate.PrintPopulation(_world, p2.Take(_showbest).ToList(), img2, 1.0f, Color.Green);
            else
                pictureBox1.Image = img2;

            _generationsArm ++;
            _generationsFingers ++;
            _generationsAll++;

            UpdateStats();

        }

    }
}
