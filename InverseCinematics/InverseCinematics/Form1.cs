using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace InverseCinematics
{
    public partial class Form1 : Form
    {
        private bool _started = false;
        
        private WorldInstance _world;
        private int _populationSize;
        private int _generations;
        private double _badguys;
        private double _mutChance;
        private int _mutationId;
        private double _adjustment;
        private int _tournament;
        private double _explicite;
        private int _crossoverId;
        private int _evaluationId;
        private int _showbest;

        private Bitmap _baseImage;
        private List<Chromosome> _population;
              
        private bool LoadData()
        {
            _generations = 0;
            label13.Text = "";
            label14.Text = "";
            label15.Text = "";
            label16.Text = "";
            label17.Text = "";
            label18.Text = "";
            labelGAll.Text = @"0";

            try
            {
                _world = new WorldInstance(comboBox1.Text);
                var x = _world.Specification.Spec.Map(s => s.Length);// Func<NodeSpec, double>{s => s})
                //TODO _world.heuristic = new Heuristics(_world, (int)numericUpDown2.Value);
                _baseImage = new Bitmap(_world.ShowWorld(pictureBox1.Width, pictureBox1.Height, 2.0f));
                _populationSize = (int) numericUpDown1.Value;
                _badguys = (double) numericUpDown3.Value/100;
                _mutChance = (double)numericUpDown4.Value / 100;
                _showbest = (int) numericUpDown7.Value;
                _adjustment = (double) numericUpDown5.Value/100;
                _tournament = (int) numericUpDown2.Value;
                _explicite = (double)numericUpDown6.Value / 100;
                _mutationId = comboBox2.SelectedIndex;
                _crossoverId = comboBox3.SelectedIndex;
                _evaluationId = comboBox4.SelectedIndex;
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
            labelGAll.Text = _generations.ToString();

            var c =_population.First();
            UpdateLabel(label13, c.Tree.Node.Score);
            UpdateLabel(label14, c.Tree.Node.Error);
            var p = _population.Where(x => x.Tree.Node.Error == 0);
            if (p.Count() > 0)
                UpdateLabel(label15, p.First().Tree.Node.Score);
            else
                label15.Text = "";

            var avgScore = _population.Average(x => x.Tree.Node.Score);
            var avgScore2 = _population.Average(x => x.Tree.Node.Score * x.Tree.Node.Score);
            var avgError = _population.Average(x => x.Tree.Node.Error);
            var avgError2 = _population.Average(x => x.Tree.Node.Error * x.Tree.Node.Error);
            UpdateLabel(label17, avgScore);
            UpdateLabel(label18, avgError, true);
            UpdateLabel(label24, avgScore2 - avgScore * avgScore);
            UpdateLabel(label23, avgError2 - avgError * avgError);
        }

        private void UpdateLabel(Label l, double v, bool inverse=false)
        {
            if (l.Text == "" || (v < double.Parse(l.Text) && !inverse) || (inverse && v > double.Parse(l.Text)))
                l.ForeColor = Color.Green;
            else if (v == double.Parse(l.Text))
                l.ForeColor = Color.Orange;
            else
                l.ForeColor = Color.Red;
            l.Text = v.ToString("F");
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
                _population = AlgorithmTemplate.GeneticAlgorithmStart(_world, _populationSize, _evaluationId);
                UpdateStats();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var ga = (int)numericGArm.Value;
            for (var i = 0; i < ga; i++)
            {
                _population = AlgorithmTemplate.GeneticAlgorithmStep(_world, _population, _badguys, _mutChance, _generations, _adjustment, _tournament, _explicite, _mutationId, _crossoverId, _evaluationId);
            }
            var img2 = AlgorithmTemplate.PrintPopulation(_world, _population.Take(_showbest).ToList(), new Bitmap(_baseImage), 1.0f, Color.Blue);
            var p2 = _population.Where(x => x.Tree.Node.Error == 0);
            if (p2.Count() > 0)
                pictureBox1.Image = AlgorithmTemplate.PrintPopulation(_world, p2.Take(_showbest).ToList(), img2, 1.0f, Color.Green);
            else
                pictureBox1.Image = img2;

            _generations += ga;

            UpdateStats();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _population = AlgorithmTemplate.GeneticAlgorithmStep(_world, _population, _badguys, _mutChance, _generations, _adjustment, _tournament, _explicite, _mutationId, _crossoverId, _evaluationId);

            var img2 = AlgorithmTemplate.PrintPopulation(_world, _population.Take(_showbest).ToList(), new Bitmap(_baseImage), 1.0f, Color.Blue);
            var p2 = _population.Where(x => x.Tree.Node.Error == 0);
            if (p2.Count() > 0)
                pictureBox1.Image = AlgorithmTemplate.PrintPopulation(_world, p2.Take(_showbest).ToList(), img2, 1.0f, Color.Green);
            else
                pictureBox1.Image = img2;

            _generations++;

            UpdateStats();
        }

    }
}
