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
        private int _showbest = 1;

        private WorldInstance _world;
        private int _populationSize;
        private int _generation;
        private int _generations;
        private double _badguys;
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

            try
            {
                _world = new WorldInstance(textBox1.Text);
                _baseImage = new Bitmap(_world.ShowWorld(pictureBox1.Width, pictureBox1.Height, 2.0f));
                _populationSize = (int) numericUpDown1.Value;
                _generations = (int) numericUpDown2.Value;
                _badguys = (double) numericUpDown3.Value/100;
            }
            catch (Exception e)
            {
                label5.Text = e.Message;
                return false;
            }
            label5.Text = "";
            return true;
        }

        private void UpdateStats()
        {
            label19.Text = _generation.ToString();

            var c =_population.First();
            UpdateLabel(label13, c.Score);
            UpdateLabel(label14, c.Error);
            c = _population.Where(x => x.Error == 0).First();
            UpdateLabel(label15, c.Score);
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

            /*
            var g = 100; // generations

            var p = AlgorithmTemplate.GeneticAlgorithmStart(w, 50, AlgorithmTemplate.GenerateRandomPopulation);
            pictureBox1.Image = AlgorithmTemplate.PrintPopulation(w, p.Take(1).ToList(), img, 1.0f);
            for (var i = 0; i < g; i++)
            {
                

                AlgorithmTemplate.GeneticAlgorithmStep(w, p, 0.10, AlgorithmTemplate.Mutate, 0.05,
                                                           AlgorithmTemplate.Selection, AlgorithmTemplate.Crossover,
                                                           AlgorithmTemplate.Evaluate);

                //pictureBox1.Image = AlgorithmTemplate.PrintPopulation(w, p.Take(3).ToList(), img, h);

            }

            AlgorithmTemplate.Evaluate(p[0], w);

            pictureBox1.Image = img;
            */





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

            _started = false;
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!_started)
            {
                if (!LoadData())
                    return;
                // TODO Insert proper algorithm
                _population = AlgorithmTemplate.GeneticAlgorithmStart(_world, _populationSize, AlgorithmTemplate.GenerateRandomPopulation);
                pictureBox1.Image = AlgorithmTemplate.PrintPopulation(_world, _population.Take(_showbest).ToList(), new Bitmap(_baseImage), 1.0f);
                
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
                        
            pictureBox1.Image = AlgorithmTemplate.PrintPopulation(_world, _population.Take(_showbest).ToList(), new Bitmap(_baseImage), 1.0f);

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
