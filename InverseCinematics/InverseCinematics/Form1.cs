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
        private int _generation;
        private int _generations;
        private double _badguys;
        private double _mutation;
        private Bitmap _baseImage;
        private List<Chromosome> _population;
              
        private bool LoadData()
        {
            _generation = 0;
            label13.Text = "";
            label14.Text = "";
            label15.Text = "";
            label16.Text = "";
            label17.Text = "";
            label18.Text = "";
            label19.Text = _generation.ToString();

            //try
            //{
                //_world = new WorldInstance(textBox1.Text);
                _world = new WorldInstance(comboBox1.Text);
                var h = new Heuristics(_world, 50);
                _baseImage = new Bitmap(_world.ShowWorld(pictureBox1.Width, pictureBox1.Height, 2.0f, h));
                _populationSize = (int) numericUpDown1.Value;
                _generations = (int) numericUpDown2.Value;
                _badguys = (double) numericUpDown3.Value/100;
                _mutation = (double)numericUpDown4.Value / 100;

                //var x = h.GetHeuristic(new Point(18, 9.99));
                //var y = h.GetHeuristic(new Point(2, 9.99));
                //var z = 0;
                //TODO
                // TESTING CODE
                //var h = new Heuristics(_world, 100);

            //}
            //catch (Exception e)
            //{
            //    label5.Text = e.Message;
            //    return false;
            //}
            label5.Text = "";
            return true;
        }

        private void UpdateStats()
        {
            label19.Text = _generation.ToString();

            var c =_population.First();
            UpdateLabel(label13, c.Score);
            UpdateLabel(label14, c.Error);
            var p = _population.Where(x => x.Error == 0);
            if (p.Count() > 0)
                UpdateLabel(label15, p.First().Score);
            else
                label15.Text = "";
            UpdateLabel(label17, _population.Average(x => x.Score));
            UpdateLabel(label18, _population.Average(x => x.Error));
        }

        private void UpdateLabel(Label l, double v)
        {
            if (l.Text == "" || v < double.Parse(l.Text))
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
            button1.Enabled = false;
            button3_Click(sender, e);

            while (_started && _generation < _generations)
            {
                //TODO timer?
                button3_Click(sender, e);
            }

            //_started = false;
            button1.Enabled = true;
            _generations += (int)numericUpDown2.Value;
            //button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!_started)
            {
                if (!LoadData())
                    return;
                // TODO Insert proper algorithm
                _population = AlgorithmTemplate.GeneticAlgorithmStart(_world, _populationSize, AlgorithmTemplate.GenerateRandomPopulation, AlgorithmTemplate.Evaluate);
                //_population = AlgorithmTemplate.RunAlgorithmStart(_populationSize, _badguys, _mutation, _world);

                var img = AlgorithmTemplate.PrintPopulation(_world, _population.Take(_showbest).ToList(), new Bitmap(_baseImage), 1.0f, Color.Blue);
                var p = _population.Where(x => x.Error == 0);
                if (p.Count() > 0)
                    pictureBox1.Image = AlgorithmTemplate.PrintPopulation(_world, p.Take(_showbest).ToList(), img, 1.0f, Color.Green);
                else
                    pictureBox1.Image = img;
                
                _started = true;
                button2.Enabled = true;
                _generation++;
                UpdateStats();
                return;
            }
            // TODO Insert proper algorithm
            _population = AlgorithmTemplate.GeneticAlgorithmStep(_world, _population, _badguys, 
                AlgorithmTemplate.Mutate, 0.05,
                AlgorithmTemplate.Selection, AlgorithmTemplate.Crossover,
                AlgorithmTemplate.Evaluate);
            //_population = AlgorithmTemplate.RunAlgorithmStep(_populationSize, _badguys, _mutation, _world, _population);

            var img2 = AlgorithmTemplate.PrintPopulation(_world, _population.Take(_showbest).ToList(), new Bitmap(_baseImage), 1.0f, Color.Blue);
            var p2 = _population.Where(x => x.Error == 0);
            if (p2.Count() > 0)
                pictureBox1.Image = AlgorithmTemplate.PrintPopulation(_world, p2.Take(_showbest).ToList(), img2, 1.0f, Color.Green);
            else
                pictureBox1.Image = img2;

            _generation++;
            UpdateStats();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _started = false;
            button2.Enabled = false;
            button1.Enabled = true;
        }
    }
}
