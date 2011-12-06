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
        public Form1()
        {
            InitializeComponent();
            var h = 2.0f;
            var w = new WorldInstance("scenario_01.txt");
            var img = new Bitmap(w.ShowWorld(pictureBox1.Width, pictureBox1.Height, h));
            var p = AlgorithmTemplate.RunAlgorithm();
            
            img = AlgorithmTemplate.PrintPopulation(w, p, img, h);
            pictureBox1.Image = img;

        }
    }
}
