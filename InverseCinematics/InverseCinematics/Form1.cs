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
            var w = new WorldInstance("scenario_01.txt");            

            pictureBox1.Image = w.ShowWorld(400, 400);

        }

      
    }
}
